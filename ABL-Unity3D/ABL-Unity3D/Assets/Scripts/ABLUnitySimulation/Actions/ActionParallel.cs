using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Assertions;

#nullable enable

namespace ABLUnitySimulation.Actions
{
    /// <summary>
    ///     An action which executes multiple actions at the same time (in parallel).
    /// </summary>
    // [Serializable]
    public class ActionParallel : SimActionCompound
    {
        /// <summary>
        ///     The list of subactions to execute in parallel.
        /// </summary>
        public List<ActionEntry> actions = new List<ActionEntry>();

        public ActionParallel()
        {
        }

        public ActionParallel(IEnumerable<SimAction> initialActions)
        {
            initialActions.ForEach(this.Add);
        }

        public override SimAction DeepCopy()
        {
            var clone = new ActionParallel(Enumerable.Empty<SimAction>());
            clone.actions.AddRange(this.actions.Select(ae => ae.DeepCopy()));
            return clone;
        }

        public override List<SimWorldState>? MaybeForkWorld(SimWorldState state)
        {
            // Fork the world based on the first subaction that has not been forked
            return this.actions
                .Where(action => ActionStatus.InProgress == action.lastStatusReport.status)
                .Select(action => action.subAction.MaybeForkWorld(state))
                .FirstOrDefault(list => null != list);
        }

        public override SimAction? FindAction(long searchKey)
        {
            if (this.key == searchKey) return this;

            return this.actions
                .Select(action => action.subAction.FindAction(searchKey))
                .FirstOrDefault(action => null != action);
        }

        /// <summary>
        ///     Return the friendly ("performing") team.
        /// </summary>
        /// <param name="state">The current simulator world state which this action is contained in.</param>
        /// <returns></returns>
        public override Team GetPerformingTeam(SimWorldState state)
        {
            if (this.actions.Count < 1) return Team.Undefined;

            Assert.IsTrue(this.actions.Select(ae => ae.subAction.GetPerformingTeam(state)).Distinct().Count() == 1);

            return this.actions[0].subAction.GetPerformingTeam(state);
        }

        /// <summary>
        ///     Return whether all subactions have been completed.
        /// </summary>
        /// <param name="state">The simulator world state the action is contained in.</param>
        /// <param name="useExpensiveExplanation">Whether to return a more verbose explanation which requires more computation.</param>
        /// <returns>Whether all subactions have been completed.ds</returns>
        public override StatusReport GetStatus(SimWorldState state, bool useExpensiveExplanation)
        {
            if (this.actions.Count < 1)
                return new StatusReport(ActionStatus.CompletedSuccessfully, "No Subactions", this);

            var worst = new StatusReport(ActionStatus.CompletedSuccessfully, "All subactions CompletedSuccessfully",
                this);

            // if (this.actions.All(a =>
            //         new List<ActionStatus>() { ActionStatus.Impossible, ActionStatus.Undefined, ActionStatus.Undefined }
            //             .Contains(a.lastStatusReport.status)))
            //     return worst;

            foreach (var entry in this.actions)
            {
                var entryReport = entry.lastStatusReport;
                switch (entryReport.status)
                {
                    case ActionStatus.Impossible:
                    case ActionStatus.Undefined:
                        return !useExpensiveExplanation
                            ? entryReport
                            : entry.subAction.GetStatus(state, useExpensiveExplanation);
                    case ActionStatus.InProgress:
                        worst = !useExpensiveExplanation
                            ? entryReport
                            : entry.subAction.GetStatus(state, useExpensiveExplanation);
                        break;
                    case ActionStatus.CompletedSuccessfully:
                        break;
                    default:
                        Assert.IsTrue(false,
                            $"Unhandled ActionStatus {entryReport.status} in ActionParallel.GetStatus()");
                        break;
                }
            }

            return worst;
        }

        public override BusyStatusReport IsBusy(Handle<SimAgent> agent, SimWorldState state,
            bool calculateExpensiveExplanation)
        {
            foreach (var ae in this.actions)
                if (ae.lastStatusReport.status == ActionStatus.InProgress)
                {
                    var subActionBusy = ae.subAction.IsBusy(agent, state, calculateExpensiveExplanation);
                    if (subActionBusy.IsBusy) return subActionBusy;
                }

            return new BusyStatusReport(BusyStatusReport.AgentBusyStatus.NotBusy, null, this);
        }

        public override void DrawIntentDestructive(SimWorldState throwawayState, IIntentDrawer drawer)
        {
            foreach (var actionEntry in this.actions)
            {
                var status = actionEntry.lastStatusReport.status;
                
                //if (status == ActionStatus.Undefined || status == ActionStatus.InProgress)
                {
                    actionEntry.subAction.DrawIntentDestructive(throwawayState, drawer);
                }
            }
        }

        public override IEnumerable<SimAction> EnumerateCurrentPrimitives(SimWorldState state, PrimitiveMode mode)
        {
            return this.actions
                .Where(ae => ae.lastStatusReport.status == ActionStatus.InProgress)
                .SelectMany(ae => ae.subAction.EnumerateCurrentPrimitives(state, mode));
        }

        public override void Execute(SimWorldState state)
        {
            this.actions
                .Where(ae => ae.lastStatusReport.status == ActionStatus.InProgress)
                .ForEach(ae => ae.subAction.Execute(state));
        }

        public override void UpdateForExternalSimChange(SimWorldState state)
        {
            for (var index = 0; index < this.actions.Count; index++)
            {
                var actionEntry = this.actions[index];
                if (actionEntry.lastStatusReport.status == ActionStatus.CompletedSuccessfully) continue;

                actionEntry.subAction.UpdateForExternalSimChange(state);
                actionEntry.lastStatusReport = actionEntry.subAction.GetStatus(state, true);
                this.actions[index] = actionEntry;
            }
        }

        public static ActionStatus CombinedParallelStatus(IEnumerable<ActionStatus> statuses)
        {
            var worst = ActionStatus.CompletedSuccessfully;

            foreach (var status in statuses)
                switch (status)
                {
                    case ActionStatus.Impossible:
                    case ActionStatus.Undefined:
                        worst = status;
                        break;
                    case ActionStatus.InProgress:
                        return status;
                    case ActionStatus.CompletedSuccessfully:
                        break;
                }

            return worst;
        }

        public void CullTopLevelCompletedActions()
        {
            for (int index = this.actions.Count - 1; index >= 0; index--)
                if (this.actions[index].lastStatusReport.status == ActionStatus.CompletedSuccessfully)
                    this.actions.RemoveAt(index);
        }

        public void Add(SimAction action)
        {
            var ae = new ActionEntry
            {
                subAction = action,
                lastStatusReport = new StatusReport(ActionStatus.InProgress, null, this)
            };
            this.actions.Add(ae);
        }

        public void Add(IEnumerable<SimAction> newActions)
        {
            newActions.ForEach(this.Add);
        }

        public override void OnDrawGizmos(SimWorldState state, Handle<SimAgent> agentHandle,
            Func<Vector2, Vector3> simToUnityCoords)
        {
            this.actions.ForEach(a => a.subAction.OnDrawGizmos(state, agentHandle, simToUnityCoords));
        }

        /// <summary>
        ///     A wrapper for subactions used by the <see cref="ActionParallel" /> class. Keeps track of whether the subaction
        ///     has been marked as complete in the past through the field <see cref="lastStatusReport" />.
        ///     This is necessary because sometimes after an action reports that it's completed, eventually it may say
        ///     it is not completed, which is by design due to this being simpler in cases of sequential actions.
        ///     The <see cref="ActionParallel" /> class dynamically checks all subactions to see if they are complete. A.k.a,
        ///     it checks the current status of the subactions, as opposed to whether they have been marked as complete in
        ///     the past.
        /// </summary>
        // [Serializable]
        public struct ActionEntry
        {
            public SimAction subAction;
            [JsonIgnore] public StatusReport lastStatusReport;

            public ActionEntry DeepCopy()
            {
                return new ActionEntry
                {
                    subAction = this.subAction.DeepCopy(),
                    lastStatusReport = this.lastStatusReport
                };
            }

            public string ToHumanReadableString()
            {
                return this.lastStatusReport.ToHumanReadableString();
            }
        }
    }
}