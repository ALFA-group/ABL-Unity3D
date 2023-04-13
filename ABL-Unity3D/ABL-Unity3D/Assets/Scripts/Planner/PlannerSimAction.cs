using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using UnityEngine;

#nullable enable

namespace Planner
{
    public class PlannerSimAction : SimActionCompound
    {
        private SimAction? _currentAction;

        private StatusReport? _currentStatusReport;
        // holds all sequential methods in the plan, already flattened. 
        

        public int numMethodsConvertedToActions;
        public Plan plan;


        public List<Method> toDo; // shallow copied!  Do not alter.

        public PlannerSimAction(Plan myPlan)
        {
            this.plan = myPlan;
            this.toDo = myPlan.EnumerateSequentialActions().ToList();
        }

        public PlannerSimAction(Plan myPlan, Method startHere)
        {
            this.plan = myPlan;
            this.toDo = myPlan.EnumerateSequentialActions(startHere).ToList();
        }

        public SimAction? UpdateCurrentAction(SimWorldState state)
        {
            if (null == this._currentAction)
            {
                if (this.toDo.Count <= this.numMethodsConvertedToActions) return null;

                this._currentAction = this.ConvertToAction(state, this.toDo[this.numMethodsConvertedToActions]);
                ++this.numMethodsConvertedToActions;
                return this.UpdateCurrentAction(state); // skip all the silly null actions.
            }

            return this._currentAction;
        }

        public SimAction? GetCurrentAction()
        {
            return this._currentAction;
        }

        private SimAction? ConvertToAction(SimWorldState state, Method method)
        {
            if (this.plan.dMethodToDecomposition.TryGetValue(method, out var d))
            {
                // Method should not have both a decomposition AND a SimAction
                Debug.Assert(method.GetActionForSim(state) == null);

                if (d.mode == Decomposition.ExecutionMode.Parallel)
                    return this.DecompositionToSimActionParallel(d);
                return null;
            }

            return method.GetActionForSim(state);
        }

        private SimAction DecompositionToSimActionParallel(Decomposition decomposition)
        {
            Debug.Assert(decomposition.mode == Decomposition.ExecutionMode.Parallel);

            var parallelAction = new ActionParallel
            {
                name = "ParallelPlanDecomposition"
            };

            foreach (var subtask in decomposition.subtasks)
            {
                var subPlanAction = new PlannerSimAction(this.plan, subtask);
                parallelAction.Add(subPlanAction);
            }

            return parallelAction;
        }

        public override StatusReport GetStatus(SimWorldState state, bool useExpensiveExplanation)
        {
            if (this.numMethodsConvertedToActions >= this.toDo.Count && this._currentAction == null)
                return new StatusReport(ActionStatus.CompletedSuccessfully, "Done with all subactions", this);

            if (null == this._currentAction)
                return new StatusReport(ActionStatus.InProgress, "Some planner action is not complete", this);

            if (this._currentStatusReport.HasValue) return this._currentStatusReport.Value;

            return new StatusReport(ActionStatus.InProgress, "Some planner action is not complete",
                this._currentAction);
        }

        public override BusyStatusReport IsBusy(Handle<SimAgent> agent, SimWorldState state,
            bool calculateExpensiveExplanation)
        {
            if (null == this._currentAction)
                return new BusyStatusReport(BusyStatusReport.AgentBusyStatus.NotBusy, "No current action in plan", this);

            return this._currentAction.IsBusy(agent, state, calculateExpensiveExplanation);
        }

        public override void DrawIntentDestructive(SimWorldState throwawayState, IIntentDrawer drawer)
        {
            // Draw current action
            this._currentAction?.DrawIntentDestructive(throwawayState, drawer);

            // Draw future actions
            foreach (var method in this.toDo.Skip(this.numMethodsConvertedToActions))
            {
                var action = this.ConvertToAction(throwawayState, method);
                action?.DrawIntentDestructive(throwawayState, drawer);
            }
        }

        public override IEnumerable<SimAction> EnumerateCurrentPrimitives(SimWorldState state, PrimitiveMode mode)
        {
            var currentAction = this.UpdateCurrentAction(state);
            return currentAction?.EnumerateCurrentPrimitives(state, mode) ?? Enumerable.Empty<SimAction>();
        }

        public override void Execute(SimWorldState state)
        {
            var currentAction = this.UpdateCurrentAction(state);
            currentAction?.Execute(state);
        }

        public override void UpdateForExternalSimChange(SimWorldState state)
        {
            var currentAction = this.UpdateCurrentAction(state);

            if (null != currentAction)
            {
                currentAction.UpdateForExternalSimChange(state);
                if (currentAction.GetStatus(state, false).status != ActionStatus.InProgress)
                {
                    this._currentAction = null;
                    this.UpdateForExternalSimChange(state);
                }
            }
        }

        public override void OnDrawGizmos(SimWorldState state, Handle<SimAgent> agentHandle,
            Func<Vector2, Vector3> simToUnityCoords)
        {
            var currentAction = this.UpdateCurrentAction(state);
            currentAction?.OnDrawGizmos(state, agentHandle, simToUnityCoords);
        }

        public override SimAction DeepCopy()
        {
            // oh oh
            var clone = (PlannerSimAction)this.MemberwiseClone();
            clone._currentAction = this._currentAction?.DeepCopy();
            return clone;
        }

        public override List<SimWorldState> MaybeForkWorld(SimWorldState state)
        {
            throw new NotImplementedException();
        }

        public override SimAction? FindAction(long searchKey)
        {
            if (this.key == searchKey) return this;
            return null;
        }

        public override Team GetPerformingTeam(SimWorldState state)
        {
            
            return state.teamFriendly;

            // return this.plan.startState.teamFriendly;
        }
    }
}