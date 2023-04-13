using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions.Helpers;
using ABLUnitySimulation.SimScoringFunction;
using Cysharp.Threading.Tasks;
using GP;
using Planner;
using Planner.ManyWorlds;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UI.ABLUnitySimulation;
using UI.GP.Editor;
using UI.InspectorDataRefs;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Utilities.GeneralCSharp;
using Utilities.GP;
using Utilities.Unity;
using Object = UnityEngine.Object;

#nullable enable

namespace UI.Planner
{
    public class ManyWorldsPlannerRunner : SerializedMonoBehaviour
    {
        [NonSerialized, OdinSerialize, ListDrawerSettings(DraggableItems = false)]
        public List<PlannerResultsScriptableObject> resultsFromPreviousRuns =
            new List<PlannerResultsScriptableObject>();

        public PlannerParameters plannerParameters = new PlannerParameters();

        /// <summary>
        ///     The simulator state to use for planning.
        /// </summary>
        public RefSimWorldState? stateRef;

        private CancellationTokenSource? _cancellationTokenSource;

        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly, EnableGUI, NonSerialized, MultiLineProperty(10)]
        public string? goalDescription;

        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly, EnableGUI, NonSerialized, MultiLineProperty(10)]
        public string lastPlan = "";
        
        private void OnDisable()
        {
            this._cancellationTokenSource?.Cancel();
            this._cancellationTokenSource = null;
        }

        private void OnValidate()
        {
            var simWorldState = RefSimWorldState.Fetch(this.stateRef);
            if (simWorldState?.Agents.Any() ?? false)
            {
                var m = PlannerGoalBuilder.MakeGoal(simWorldState, this.plannerParameters, this.GetGoalWaypointAsCircle(), this.GetWaypointOptionsAsCircles());
                this.goalDescription = m.ToJson();
            }
            else
            {
                this.goalDescription = "No simAgents in SimWorldState";
            }
        }

        public void SetWaypointsAsCircles()
        {
            this.plannerParameters.pureData.waypointOptionsAsCirclesCache = this.GetWaypointOptionsAsCircles().ToList();
            this.plannerParameters.pureData.goalWaypointAsCircleCache = this.GetGoalWaypointAsCircle();
        }

        [Button]
        [PropertyOrder(-2)]
        [DisableInEditorMode]
        protected void CancelPlanning()
        {
            this._cancellationTokenSource?.Cancel();
        }

        /// <summary>
        ///     Run the many worlds planner and store the results in the plan storage. Generate the goal for the planner
        ///     from the simulator world state.
        /// </summary>
        [DisableInEditorMode]
        [Button]
        [PropertyOrder(-1)]
        public async void CreateManyWorldsPlans()
        {
            var start = DateTime.Now;
            this._cancellationTokenSource?.Cancel();
            this._cancellationTokenSource = new CancellationTokenSource();

            var plans = await CreateManyWorldsPlansAsync();
            var end = DateTime.Now;

            var simEvaluationParametersHolder =
                FindObjectOfType<SimEvaluationParametersHolder>()?.simEvaluationParameters
                ?? throw new InvalidOperationException(
                    $"{nameof(SimEvaluationParametersHolder.simEvaluationParameters)} is null after running the Planner!");
            
            var results = new PlannerResults(
                SceneManager.GetActiveScene().name,
                gameObject.GetGameObjectPath(),
                this.plannerParameters,
                simEvaluationParametersHolder,
                FindObjectOfType<SimCreator>().simInitParameters,
                plans, 
                start,
                end
            );
            
            this.AddResults(results);
        }

        public void AddResults(PlannerResults results)
        {
            var so = PlannerResultsScriptableObject.Create(results);
            this.resultsFromPreviousRuns.Add(so);
        }
        
        /// <summary>
        ///     Run the many worlds planner and store the results in the plan storage. Generate the goal for the planner
        ///     from the simulator world state.
        /// </summary>
        [DisableInEditorMode]
        [Button]
        [PropertyOrder(-1)]
        
        public async UniTask GetHighestScoringPlansForMultipleManyWorldsPlanningRuns(int numberOfRuns = 2, bool clearPathfindingCacheBetweenRuns = true)
        {
            if (numberOfRuns < 1) throw new Exception($"{nameof(numberOfRuns)} must be greater than zero.");
            
            this._cancellationTokenSource?.Cancel();
            this._cancellationTokenSource = new CancellationTokenSource();

            this.SetWaypointsAsCircles();

            var simCreator = FindObjectOfType<SimCreator>();
            var highestScoringPlans = new PlanStorage();


            var scoringFunction = FindObjectOfType<SimEvaluationParametersHolder>().simEvaluationParameters?.primaryScoringFunction ?? 
                                  throw new Exception("Primary scoring function is null");
            
            var start = DateTime.Now;

            
            for (int i = 0; i < numberOfRuns; i++)
            {
                if (this._cancellationTokenSource.IsCancellationRequested) break;
                
                if (clearPathfindingCacheBetweenRuns) PathfindingWrapper.ClearCache();

                var bestPlanForThisRun =
                    await GetBestPlan(scoringFunction, this._cancellationTokenSource.Token, shouldLogDebugInformation: true);

                highestScoringPlans.AddPlan(bestPlanForThisRun, scoringFunction, simCreator);
            }
            
            var end = DateTime.Now;
            
            highestScoringPlans.SortRecordsDescending();

            var simEvaluationParametersHolder = FindObjectOfType<SimEvaluationParametersHolder>();
            
            simEvaluationParametersHolder.SetBestAlternatePlans(this._cancellationTokenSource.Token, highestScoringPlans);

            var simEvaluationParameters =
                simEvaluationParametersHolder.simEvaluationParameters
                ?? throw new InvalidOperationException(
                    $"{nameof(SimEvaluationParametersHolder.simEvaluationParameters)} is null after running the Planner!");
            
            var results = new PlannerResults(
                SceneManager.GetActiveScene().name,
                gameObject.GetGameObjectPath() + $" — {numberOfRuns} Runs",
                this.plannerParameters,
                simEvaluationParameters,
                simCreator.simInitParameters,
                highestScoringPlans,
                start,
                end
            );
            
            this.AddResults(results);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scoringFunction"></param>
        /// <param name="cancel"></param>
        /// <param name="goal"></param>
        /// <param name="shouldLogDebugInformation"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <remarks>
        ///     The RefSimWorldState must be reset at the end of this function! Also, always use a deep copy of the SimWorldState when calling ManyWorldsPlanner.CreateManyWorldsPlansAsync.
        /// </remarks>
        public async UniTask<Plan> GetBestPlan(
            SimScoringFunction scoringFunction,
            CancellationToken cancel, 
            Method? goal = null,
            bool shouldLogDebugInformation = true,
            TimeoutInfo? timeoutInfo = null) 
        {
            // Always use the startState from the planner, don't refetch the RefSimWorldState because that changes after each plan is generated
            var startState = InspectorDataRef<SimWorldState>.Fetch(this.stateRef)?.DeepCopy() ?? 
                                throw new Exception("The sim world state is null.");
            startState.goalWaypoint = this.plannerParameters.pureData.goalWaypointAsCircleCache;

            var bestScore = double.NegativeInfinity;
            Plan? bestPlan = null;
            await foreach (var plan in ManyWorldsPlanner.CreateManyWorldsPlansAsync(
                               startState, this.plannerParameters,
                               cancel, goal, shouldLogDebugInformation))
            {
                if (null == plan.endState)
                {
                    throw new Exception("End state should not be null");
                }
                
                // Change the RefSimWorldState 
                RefSimWorldState.Set(this.stateRef, plan.endState);

                var score = scoringFunction.EvaluateSimWorldState(plan.endState);

                // Is the current best plan better than the newly generated plan?
                if (null != bestPlan && score <= bestScore) continue;

                bestPlan = plan;
                bestScore = score;
                
                
                if (timeoutInfo?.ShouldTimeout ?? false) break;
            }

            if (null == bestPlan)
                throw new Exception(
                    $"No plans were generated by {nameof(this.CreateManyWorldsPlansAsync)}");
            
            // Reset RefSimWorldState to startState.
            RefSimWorldState.Set(this.stateRef, startState);
            
            return bestPlan;

        }

        
        /// <summary>
        ///     Run the many worlds planner and store the results in the plan storage.
        ///     Use the given parameter <see cref="goalMethod" /> as the goal for the planner to achieve.
        /// </summary>
        /// <param name="goalMethod"></param>
        /// <remarks>
        /// The RefSimWorldState must be reset at the end of this function! Also, always use a deep copy of the SimWorldState when calling ManyWorldsPlanner.CreateManyWorldsPlansAsync.
        /// </remarks>
        public async UniTask<PlanStorage> CreateManyWorldsPlansAsync(Method? goalMethod = null)
        {
            this.SetWaypointsAsCircles();
            this._cancellationTokenSource?.Cancel();
            this._cancellationTokenSource = new CancellationTokenSource();
            
            var startState = InspectorDataRef<SimWorldState>.Fetch(this.stateRef)?.DeepCopy() ?? 
                                throw new Exception("The sim world state is null.");
            startState.goalWaypoint = this.plannerParameters.pureData.goalWaypointAsCircleCache;
            var scoringFunction = FindObjectOfType<SimEvaluationParametersHolder>().simEvaluationParameters?.primaryScoringFunction ?? 
                                  throw new Exception("Primary scoring function is null");

            var simCreator = Object.FindObjectOfType<SimCreator>();
            var storage = new PlanStorage();
            
            await foreach (var plan in ManyWorldsPlanner.CreateManyWorldsPlansAsync(
                               startState,
                               this.plannerParameters,
                               this._cancellationTokenSource.Token, goalMethod))
            {
                if (null == plan.endState)
                {
                    throw new Exception($"{nameof(plan.endState)} should not be null");
                }
                RefSimWorldState.Set(this.stateRef, plan.endState);
                storage.AddPlan(plan, scoringFunction, simCreator);
            }
            
            TrimPlanStorage(storage, this.plannerParameters.pureData.maxNumberOfPlansKept);
            storage.SortRecordsDescending();
            
            FindObjectOfType<SimEvaluationParametersHolder>().SetBestAlternatePlans(
                this._cancellationTokenSource.Token, storage);
            
            // Reset RefSimWorldState
            RefSimWorldState.Set(this.stateRef, startState);

            return storage;
        }

        private static void TrimPlanStorage(PlanStorage? planStorage, int numPlansToKeep)
        {
            if (null == planStorage || planStorage.planRecords.Count <= numPlansToKeep) return;

            planStorage.DropNonNovel(numPlansToKeep);
        }
        

        private IEnumerable<Circle> GetWaypointOptionsAsCircles()
        {
            if (null != this.plannerParameters.pureData.waypointOptionsAsCirclesCache)
                return this.plannerParameters.pureData.waypointOptionsAsCirclesCache;
            if (!this.plannerParameters.waypointsAsCapsuleColliders.waypointOptionsAsCapsuleColliders.Any())
                throw new Exception($"{nameof(this.plannerParameters.waypointsAsCapsuleColliders.waypointOptionsAsCapsuleColliders)} is empty!");
            return this.plannerParameters.waypointsAsCapsuleColliders.waypointOptionsAsCapsuleColliders.Select(c => c.ToCircle());
        }

        private Circle GetGoalWaypointAsCircle()
        {
            if (null != this.plannerParameters.pureData.goalWaypointAsCircleCache) return this.plannerParameters.pureData.goalWaypointAsCircleCache;
            
            if (null == this.plannerParameters.waypointsAsCapsuleColliders.goalWaypointAsCapsuleCollider) throw new Exception("importantWaypoint not set!");
            return this.plannerParameters.waypointsAsCapsuleColliders.goalWaypointAsCapsuleCollider.ToCircle();
        }
    }
}