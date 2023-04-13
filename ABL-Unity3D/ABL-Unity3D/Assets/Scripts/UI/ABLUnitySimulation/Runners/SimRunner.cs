 using System;
using ABLUnitySimulation;
using Cysharp.Threading.Tasks;
using GP;
using GP.ExecutableNodeTypes;
using Planner;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEngine;
 using UnityEngine.Serialization;

#nullable enable

namespace UI.ABLUnitySimulation.Runners
{
    public class SimRunner : MonoBehaviour
    {
        public enum RunStatus
        {
            Paused,
            Running
        }

        
        public enum Simulation
        {
            AblUnity
        }

        [PropertyOrder(-10), BoxGroup(GroupID = "stateRef")]
        public RefSimWorldState? stateRef => FindObjectOfType<RefSimWorldState>();

        [EnumToggleButtons] public RunStatus status = RunStatus.Paused;

        /// <summary>
        ///     ABLUnity simulator and planner related settings.
        /// </summary>
        [FormerlySerializedAs("ablUnity"),InlineProperty(LabelWidth = 150), LabelText(""), LabelWidth(1), BoxGroup("ABLUnity Settings")]
        public SimRunnerHelper helper = new SimRunnerHelper();

        [ShowInInspector, ReadOnly, BoxGroup(GroupID = "stateRef")]
        public int WorldStateElapsedTime => RefSimWorldState.Fetch(this.stateRef)?.SecondsElapsed ?? -1;

        /// <summary>
        ///     Update the simulator.
        /// </summary>
        public void Update()
        {
            if (!Application.isPlaying) return;

            this.UpdateHelper();
        }

        /// <summary>
        ///     Toggle whether the simulator is running.
        /// </summary>
        /// <returns>Returns the new run status</returns>
        public RunStatus ToggleRunStatus()
        {
            this.status = this.status == RunStatus.Running ? RunStatus.Paused : RunStatus.Running;
            return this.status;
        }

        /// <summary>
        ///     Update the simulator state based on the ABLUnity settings.
        /// </summary>
        private void UpdateHelper() 
        {
            var state = RefSimWorldState.Fetch(this.stateRef) ?? throw new Exception("No SimWorldState data found");
            this.helper.Update(state, Time.deltaTime, this);
        }

        public void StartShowPlanNoReplan(SimWorldState runPlanOnMe, Plan plan)
        {
            this.status = RunStatus.Running;
            var simWorldState = runPlanOnMe.DeepCopy();

            if (simWorldState == null) throw new Exception("null SimWorldState, cannot show plan");

            
            RefSimWorldState.Set(this.stateRef, simWorldState);
            plan.ApplyToSim(simWorldState);
            this.status = RunStatus.Running;
        }

        [OnInspectorGUI] // This forces inspector to redraw all the time, which keeps the debug fields up to date
        private void RedrawConstantly()
        {
            GUIHelper.RequestRepaint();
        }

        // Throws exception when not sim action
        // Have some test to see if returns sim action
        /// <summary>
        ///     Executes a GP individual in the simulator.
        /// </summary>
        /// <param name="i">The GP individual to execute.</param>
        /// <param name="fieldsWrapper">The GpFieldWrapper object to use for GP individual execution.</param>
        /// <exception cref="Exception"></exception>
        public async UniTask StartShowIndividualExecution(Individual i, GpFieldsWrapper fieldsWrapper)
        {
            switch (i.genome)
            {
                case GpBuildingBlock<SimAction?> evalToSimAction:
                    var action = evalToSimAction.Evaluate(fieldsWrapper) ?? throw new Exception("Null simAction!");
                    fieldsWrapper.worldState.actions.Add(action);
                    break;
                case GpBuildingBlock<Plan?> evalToPlan:
                    var plan = evalToPlan.Evaluate(fieldsWrapper) ?? throw new Exception("No plan to execute.");
                    plan.ApplyToSim(fieldsWrapper.worldState);
                    Debug.Log(plan.PrettyPlanString);
                    break;
                case GpBuildingBlock<UniTask<Plan>> uniTaskPlan:
                    plan = await uniTaskPlan.Evaluate(fieldsWrapper);
                    plan.ApplyToSim(fieldsWrapper.worldState);
                    Debug.Log(plan.PrettyPlanString);
                    break;
                default:
                    throw new Exception("No valid genome type to run");
            }


            RefSimWorldState.Set(this.stateRef, fieldsWrapper.worldState);
            this.status = RunStatus.Running;
        }
        
    }
}