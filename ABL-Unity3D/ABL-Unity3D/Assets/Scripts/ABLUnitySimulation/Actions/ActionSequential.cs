using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation.Actions.Helpers;
using Sirenix.Serialization;
using UnityEngine;
using Utilities.Unity;

#nullable enable

namespace ABLUnitySimulation.Actions
{
    // [Serializable]
    public class ActionSequential : SimActionCompound
    {
        [OdinSerialize]
        public List<SimAction> actionQueue = new List<SimAction>();
        public int actionsCompletedSuccessfully;

        public ActionSequential()
        {
        }

        public ActionSequential(IEnumerable<SimAction> actions)
        {
            this.actionQueue = actions.ToList();
        }

        public ActionSequential(params SimAction[] actions)
        {
            this.actionQueue = actions.ToList();
        }

        public override SimAction DeepCopy()
        {
            var clone = (ActionSequential)this.MemberwiseClone();
            clone.actionQueue = this.actionQueue.Select(a => a.DeepCopy()).ToList();
            return clone;
        }

        public override List<SimWorldState>? MaybeForkWorld(SimWorldState state)
        {
            var current = this.GetCurrentAction();
            return current?.MaybeForkWorld(state);
        }

        public override SimAction? FindAction(long searchKey)
        {
            if (this.key == searchKey) return this;

            return this.actionQueue
                .Select(action => action.FindAction(searchKey))
                .FirstOrDefault(action => null != action);
        }

        public override Team GetPerformingTeam(SimWorldState state)
        {
            if (this.actionQueue.Count < 1) return Team.Undefined;
            return this.actionQueue[0].GetPerformingTeam(state);
        }

        public override StatusReport GetStatus(SimWorldState state, bool useExpensiveExplanation)
        {
            var current = this.GetCurrentAction();
            if (null == current)
                return new StatusReport(ActionStatus.CompletedSuccessfully, "All subactions completed", this);

            var subActionStatus = current.GetStatus(state, useExpensiveExplanation);
            return subActionStatus.status == ActionStatus.CompletedSuccessfully
                ? new StatusReport(ActionStatus.InProgress,
                    "Current action completed successfully, but the next one is unknown status",
                    subActionStatus.leafStatusReporter)
                : subActionStatus;
        }

        public override BusyStatusReport IsBusy(Handle<SimAgent> agent, SimWorldState state,
            bool calculateExpensiveExplanation)
        {
            var current = this.GetCurrentAction();
            if (null == current)
                return new BusyStatusReport(BusyStatusReport.AgentBusyStatus.NotBusy, "No current action", this);

            return current.IsBusy(agent, state, calculateExpensiveExplanation);
        }

        public override void DrawIntentDestructive(SimWorldState throwawayState, IIntentDrawer drawer)
        {
            foreach (var simAction in this.actionQueue.Skip(this.actionsCompletedSuccessfully))
                simAction.DrawIntentDestructive(throwawayState, drawer);
        }

        public override IEnumerable<SimAction> EnumerateCurrentPrimitives(SimWorldState state, PrimitiveMode mode)
        {
            var current = this.GetCurrentAction();
            if (null != current) return current.EnumerateCurrentPrimitives(state, mode);

            return Enumerable.Empty<SimActionPrimitive>();
        }

        public override void Execute(SimWorldState state)
        {
            var current = this.GetCurrentAction();
            current?.Execute(state);
        }

        public override void UpdateForExternalSimChange(SimWorldState state)
        {
            var current = this.GetCurrentAction();
            current?.UpdateForExternalSimChange(state);
            if (current?.GetStatus(state, false).status == ActionStatus.CompletedSuccessfully)
                ++this.actionsCompletedSuccessfully;
        }

        private SimAction? GetCurrentAction()
        {
            if (this.actionsCompletedSuccessfully < this.actionQueue.Count)
                return this.actionQueue[this.actionsCompletedSuccessfully];

            return null;
        }

        public override void OnDrawGizmos(SimWorldState state, Handle<SimAgent> agentHandle,
            Func<Vector2, Vector3> simToUnityCoords)
        {
            foreach (var simAction in this.actionQueue.Skip(this.actionsCompletedSuccessfully))
                simAction.OnDrawGizmos(state, agentHandle, simToUnityCoords);
        }
    }
}