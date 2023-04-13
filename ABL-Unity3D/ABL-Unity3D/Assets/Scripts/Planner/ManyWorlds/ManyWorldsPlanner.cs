using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using ABLUnitySimulation.Actions.Helpers;
using ABLUnitySimulation.SimScoringFunction;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using Utilities.GeneralCSharp;
using Utilities.Unity;
using Debug = UnityEngine.Debug;

#nullable enable

namespace Planner.ManyWorlds
{
    /// <summary>
    ///     An implementation of AI planning (see https://en.wikipedia.org/wiki/Automated_planning_and_scheduling).
    ///     Our planner looks at all possible plans as opposed to finding a single plan that satisfies the goal. It takes a
    ///     goal <see cref="Method" />
    ///     as input. The planner then decomposes this goal into sub-methods which help achieve the given goal
    ///     method. The possible sub-methods are given by <see cref="ManyWorldsPlanner.methods" /> or
    ///     <see cref="PlannerContext.methods" />.
    ///     The methods chosen are then implemented using concrete <see cref="SimAction" />s, which are then
    ///     executed in the simulation.
    /// </summary>
    public class ManyWorldsPlanner
    {
        private readonly bool _multiThread;
        private readonly DateTime _startTime;

        /// <summary>
        ///     All methods that can be used by the planner.
        /// </summary>
        public MethodLibrary methods;

        /// <summary>
        ///     The amount of seconds to simulate per simulation step.
        /// </summary>
        public int secondsPerSimStep = 20;

        public bool shouldTimeout = false;

        /// <summary>
        ///     The maximum amount of seconds the planner is allowed to simulate a particular execution of a plan for.
        ///     The simulation will halt if this has been reached. A unique use case for this can be very useful for buggy
        ///     <see cref="SimAction" />s that never end.
        /// </summary>
        public int sMaxActionExecuteTime = 20000;
        
        private int _timeLimitInSeconds = 10000;

        
        public ManyWorldsPlanner(bool multiThread = true, MethodLibrary? methodLibrary = null)
        {
            this.methods = methodLibrary ?? MethodLibrary.FromReflection();
            this._multiThread = multiThread;
            this._startTime = DateTime.Now;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="simWorldState"></param>
        /// <param name="plannerParameters"></param>
        /// <param name="cancel"></param>
        /// <param name="plannerGoal"></param>
        /// <param name="shouldLogDebugInformation"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <remarks>
        /// Always use a deep copy of a SimWorldState when calling this function.
        /// </remarks>
        public static IUniTaskAsyncEnumerable<Plan> CreateManyWorldsPlansAsync(SimWorldState simWorldState,
            PlannerParameters plannerParameters,
            CancellationToken cancel, Method? plannerGoal = null,
            bool shouldLogDebugInformation = true)
        {
            Assert.IsFalse(cancel == CancellationToken.None, "Need valid CancellationToken");

            var generatorTokenSource = new CancellationTokenSource();
            var combinedTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancel, generatorTokenSource.Token);
        
            return UniTaskAsyncEnumerable.Create<Plan>(async (writer, token) =>
            {
                if (plannerParameters.pureData.multiThread)  await UniTaskUtility.SwitchToThread("ManyWorldsPlanner");//await UniTask.SwitchToThreadPool();
                
                #region Setup SimWorldState
                if (!simWorldState?.Agents.Any() ?? true) throw new Exception("No simAgents to generate plans for.");
                if (null == simWorldState) throw new Exception("The sim world state is null.");
                simWorldState = CreatePlannerStartState(simWorldState, plannerParameters);
                #endregion
                
                #region Setup planner and parameters
                if (plannerParameters.pureData.goalWaypointAsCircleCache == null)
                {
                    throw new Exception(
                        "PlannerParameters.goalWaypointAsCircleCache cannot be null. Call PlannerParameters.SetWaypoints() prior to calling this function");
                }
            
                plannerGoal ??= PlannerGoalBuilder.MakeGoal(
                    simWorldState, plannerParameters,
                    plannerParameters.pureData.goalWaypointAsCircleCache,
                    plannerParameters.pureData.waypointOptionsAsCirclesCache);

                if (shouldLogDebugInformation) Debug.Log("RUNNING MANY WORLDS....");
                
                var planner = new ManyWorldsPlanner(plannerParameters.pureData.multiThread)
                {
                    secondsPerSimStep = plannerParameters.pureData.secondsPerSimStep,
                    sMaxActionExecuteTime = plannerParameters.pureData.maxExecutionTimeInSeconds,
                    shouldTimeout = plannerParameters.pureData.shouldTimeout,
                    _timeLimitInSeconds = plannerParameters.pureData.timeLimitInSeconds
                };
                #endregion

                #region Generate plans
                var numPlansMade = 0;
                var minSimFinalTimeStamp = int.MaxValue;
                var maxSimFinalTimeStamp = int.MinValue;
            
                PathfindingWrapper.timeTaken = TimeSpan.Zero;  
                var planStopwatch = Stopwatch.StartNew();
                
                try
                {
                    foreach (var finalState in planner.GenerateFinalStates(simWorldState, plannerGoal,
                                 combinedTokenSource.Token))
                    {
                        numPlansMade++;
                        var p = ToPlanFromState(simWorldState, finalState);
                        p.endState = finalState;

                        await writer.YieldAsync(p);
                        
                        minSimFinalTimeStamp = Mathf.Min(minSimFinalTimeStamp, finalState.SecondsElapsed);
                        maxSimFinalTimeStamp = Mathf.Max(maxSimFinalTimeStamp, finalState.SecondsElapsed);
                        
                        if (numPlansMade >= plannerParameters.pureData.maxNumberOfPlansCreated || cancel.IsCancellationRequested) break;
                    }
                }
                catch (OperationCanceledException)
                {
                    // Swallow exception.
                    // Continue from here.
                }

                generatorTokenSource.Cancel();
                planStopwatch.Stop();
                #endregion
                
                if (plannerParameters.pureData.multiThread) await UniTask.SwitchToMainThread();
                
                #region Log remaining debugging information

                if (shouldLogDebugInformation)
                {
                    Debug.Log($" {minSimFinalTimeStamp} to {maxSimFinalTimeStamp} final sim timestamps in seconds");
                    Debug.Log(
                        $"... DONE MAKING {numPlansMade} PLANS in {planStopwatch.ElapsedMilliseconds}ms ({PathfindingWrapper.timeTaken.TotalMilliseconds:0}ms for pathfinding)");
                    
                    if (planStopwatch.ElapsedMilliseconds > 0)
                    {
                        var perPlan = $"at {1000 * numPlansMade / planStopwatch.ElapsedMilliseconds} plans per second";
                        Debug.Log(perPlan);
                    }  
                }
                #endregion
            });
        }
        
        
        // /// <summary>
        // ///     Generate plans asynchronously for the given goal method <paramref name="topTask" />.
        // ///     If cancelled, the planner will return all plans generated so far.
        // /// </summary>
        // /// <param name="channelForFinalStates">A channel used for generating the world states at the end of executing each plan.</param>
        // /// <param name="state">The world state to plan on.</param>
        // /// <param name="topTask">The goal method to achieve.</param>
        // /// <param name="cancel">A cancellation token to halt planning.</param>
        // public async UniTaskVoid GeneratePlansAsync(Channel<SimWorldState> channelForFinalStates, SimWorldState state,
        //     Method topTask, CancellationToken cancel)
        // {
        //     Assert.IsTrue(state.teamFriendly == Team.Red || state.teamFriendly == Team.Blue);
        //
        //     if (this._multiThread) await UniTaskUtility.SwitchToThread("ManyWorldsPlanner");
        //
        //     var finalStates = this.GenerateFinalStates(state, topTask, cancel);
        //     foreach (var finalState in finalStates) channelForFinalStates.Writer.TryWrite(finalState);
        //
        //     channelForFinalStates.Writer.TryComplete();
        // }

        /// <summary>
        ///     Generate the final states for each plan generated by this planner.
        /// </summary>
        /// <param name="state">The world state to run the planner on.</param>
        /// <param name="topTask">The goal method to achieve.</param>
        /// <param name="cancel">A cancellation token to halt planning and execution of the simulation.</param>
        /// <returns>An enumerable of every resulting final world state.</returns>
        /// <exception cref="Exception">
        ///     Throws if the world state does not contain the current <see cref="ActionManyWorldsPlan" />.
        ///     Should never happen.
        /// </exception>
        public virtual IEnumerable<SimWorldState> GenerateFinalStates(SimWorldState state, Method topTask,
            CancellationToken cancel)
        {
            var baseState = state.DeepCopy();
            baseState.methodLibrary = this.methods;

            int endTime = state.SecondsElapsed + this.sMaxActionExecuteTime;

            var actionManyWorldsPlan = new ActionManyWorldsPlan(topTask);
            baseState.actions.Add(actionManyWorldsPlan);

            // Using a stack means we work on clones of the most recent attempt.
            // This is like a depth-first search.
            var statesToRun = new Stack<SimWorldState>();
            statesToRun.Push(baseState);

            while (statesToRun.Count > 0)
            {
                var currentState = statesToRun.Pop();
                var doneChecker = currentState.actions.FindAction(actionManyWorldsPlan.key)
                                  ?? throw new Exception("Did not find clone of base planner action!");
                var finalState = this.RunOneSimUntilForkOrDone(currentState, endTime, statesToRun, doneChecker, cancel);
                if (null != finalState) yield return finalState;
            }
        }

        /// <summary>
        ///     Run one simulation until it forks or is done executing.
        /// </summary>
        /// <param name="currentState">The current world state of the simulation.</param>
        /// <param name="endTime">The latest time that the simulation can until.</param>
        /// <param name="statesToRunLater">States to execute in another fork.</param>
        /// <param name="doneChecker">A condition to check whether the simulation should finish running.</param>
        /// <param name="cancel">A cancellation token to halt execution.</param>
        /// <returns>The resulting world state after executing the <paramref name="currentState" />.</returns>
        protected SimWorldState? RunOneSimUntilForkOrDone(SimWorldState currentState, int endTime,
            Stack<SimWorldState> statesToRunLater, SimAction doneChecker, CancellationToken cancel)
        {
            while (currentState.SecondsElapsed < endTime &&
                   doneChecker.GetStatus(currentState, false).status == ActionStatus.InProgress)
            {
                if (cancel.IsCancellationRequested ||
                    this.shouldTimeout && GenericUtilities.SecondsElapsedSince(this._startTime) > this._timeLimitInSeconds)
                    return null;

                var newForks = currentState.actions.MaybeForkWorld(currentState);
                if (null != newForks)
                {
                    newForks.ForEach(statesToRunLater.Push);
                    return null;
                }

                // No fork needed, so run the state forward.
                currentState.Execute(this.secondsPerSimStep);
            }

            Assert.AreEqual(doneChecker.GetStatus(currentState, false).status,
                doneChecker.GetStatus(currentState, true).status);

            // Did we exit because we succeeded?
            var useExpensiveExplanation = true;
            var doneCheckStatusReport = doneChecker.GetStatus(currentState, useExpensiveExplanation);
            if (doneCheckStatusReport.status == ActionStatus.InProgress)
            {
                // No, we timed out.  Which actions are actually failing to complete???
                Debug.Log(
                    $"Many Worlds timed out: verbose {useExpensiveExplanation}: {doneCheckStatusReport.leafStatusReporter} {doneCheckStatusReport.explanation}");
                var p = ToPlanFromState(currentState, currentState);
                Debug.Log(p.PrettyPrint(1024));
                return null;
            }

            return currentState;
        }

        /// <summary>
        ///     Generate a plan from the given start and end <see cref="SimWorldState" />.
        /// </summary>
        /// <param name="startState">The starting world state to generate a plan from.</param>
        /// <param name="finalState">The ending world state to generate a plan from.</param>
        /// <returns>A plan generated from the given start and end world state.</returns>
        /// <exception cref="Exception">
        ///     Throws if there is no <see cref="ActionManyWorldsPlan" /> found within in the final world
        ///     state.
        /// </exception>
        public static Plan ToPlanFromState(SimWorldState startState, SimWorldState finalState)
        {
            var baseActions = finalState.actions.actions
                .Select(a => a.subAction as ActionManyWorldsPlan)
                .WhereNotNull()
                .ToList();

            if (baseActions.Count < 1) throw new Exception("No ActionManyWorldsPlan found!");
            if (baseActions.Count > 1) Debug.LogWarning("Multiple ActionManyWorldsPlan objects found, using last");

            var baseAction = baseActions.Last();

            var plan = new Plan(startState.DeepCopy())
            {
                topTask = baseAction.goal,
                planTimeStateSoFar = finalState.DeepCopy(),
            };

            AddToPlan(plan, baseAction);

            return plan;
        }


        /// <summary>
        ///     Add a given <see cref="SimAction" /> to the given <paramref name="plan" />.
        /// </summary>
        /// <param name="plan">The plan to add to.</param>
        /// <param name="action">The action to add to the plan.</param>
        protected static void AddToPlan(Plan plan, SimAction? action)
        {
            switch (action)
            {
                case null:
                    Debug.LogWarning("null implementation!");
                    return;
                case ActionParallel parallel:
                    parallel.actions.ForEach(ae => AddToPlan(plan, ae.subAction));
                    return;
                case ActionSequential sequential:
                    sequential.actionQueue.ForEach(a => AddToPlan(plan, a));
                    return;
                case ActionManyWorldsPlan actionManyWorldsPlan:
                    AddToPlan(plan, actionManyWorldsPlan);
                    return;
            }
        }

        /// <summary>
        ///     Add an implementation of an <see cref="ActionManyWorldsPlan" /> to the given <paramref name="plan" />.
        /// </summary>
        /// <param name="plan">The plan to add <paramref name="actionToConvert" /> to.</param>
        /// <param name="actionToConvert">The <see cref="ActionManyWorldsPlan" /> to add to the given <paramref name="plan" />.</param>
        protected static void AddToPlan(Plan plan, ActionManyWorldsPlan actionToConvert)
        {
            if (null == actionToConvert.decompositionChosen)
                // Happens whenever we encounter an operation- Method makes a SimAction, but has no decomposition.
                return;

            plan.dMethodToDecomposition[actionToConvert.goal] = actionToConvert.decompositionChosen;

            AddToPlan(plan, actionToConvert.implementation);
        }
        
                
        
        /// <summary>
        ///     Get a new <see cref="SimWorldState" /> that is ready to be used as the start state for the planner based on a given
        ///     simulator world state.
        /// </summary>
        /// <param name="simWorldState">The given simulator world state</param>
        /// <returns>The new simulator world state that is ready to be used as the starting state for the planner</returns>
        public static SimWorldState CreatePlannerStartState(SimWorldState simWorldState, PlannerParameters plannerParameters)
        {
            simWorldState = simWorldState.DeepCopy();
            simWorldState.teamFriendly = plannerParameters.pureData.activeTeam;

            // Strip plan action from world state
            simWorldState.actions.actions = simWorldState.actions.actions
                .Where(ae => !(ae.subAction is PlannerSimAction))
                .ToList();

            // Strip many-worlds planner action from world state
            simWorldState.actions.actions = simWorldState.actions.actions
                .Where(ae => !(ae.subAction is ActionManyWorldsPlan))
                .ToList();

            MaybeStripEnemyActions(simWorldState, plannerParameters);

            return simWorldState;
        }
        
        

        /// <summary>
        ///     Strip actions for the enemy team if <see cref="PlannerParameters.stripEnemyActions"/> = true
        /// </summary>
        /// <param name="simWorldState"></param>
        public static void MaybeStripEnemyActions(SimWorldState simWorldState, 
            PlannerParameters plannerParameters, bool doNotStripActionOpportunityFire = true)
        {
            if (plannerParameters.pureData.stripEnemyActions)
                // Strip enemy AI from world state
                // Note: we are not removing items from a list, we are keeping the actions that we select
                // This is why we check if the performing team is the friendly team, not the enemy team.
                // Likewise for checking if the action is `ActionOpportunityFire`.
            {
                simWorldState.actions.actions = simWorldState.actions.actions
                    .Where(ae => 
                        (doNotStripActionOpportunityFire && ae.subAction is ActionOpportunityFire)
                        || ae.subAction.GetPerformingTeam(simWorldState) == simWorldState.teamFriendly) 
                    .ToList();
            }
        }
    }
}