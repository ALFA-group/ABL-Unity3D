using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace ABLUnitySimulation
{
    /// <summary>
    ///     An abstract class for creating actions for the planner and simulator.
    /// </summary>
    // [Serializable]
    public abstract class SimAction
    {
        public enum PrimitiveMode
        {
            ABLUnity
        }

        public string name = "noname";

        /// <summary>
        ///     Gets the current status of the action, given the provided world state
        /// </summary>
        /// <param name="state">the state we examine to determine status</param>
        /// <param name="useExpensiveExplanation">Allow code to allocate strings and perform slow analysis?</param>
        /// <returns></returns>
        public abstract StatusReport GetStatus(SimWorldState state, bool useExpensiveExplanation);

        /// <summary>
        ///     Returns a report as to whether the given agent is busy in this action
        ///     Only valid while this action GetStatus is "InProgress"!!
        ///     If the status is not InProgress, all agent are not busy.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="state"></param>
        /// <param name="calculateExpensiveExplanation"></param>
        /// <returns></returns>
        public abstract BusyStatusReport IsBusy(Handle<SimAgent> agent, SimWorldState state,
            bool calculateExpensiveExplanation);

        public virtual void OnDrawGizmos(SimWorldState state, Handle<SimAgent> agentHandle,
            Func<Vector2, Vector3> simToUnityCoords)
        {
        }

        /// Draws the SimAction's intent.
        /// This can be expensive.
        public void DrawIntent(SimWorldState state, IIntentDrawer drawer)
        {
            this.DrawIntentDestructive(state.DeepCopy(), drawer);
        }

        /// <summary>
        ///     Draws the SimAction's intent.
        ///     When the SimAction has nested future actions (as in SimActionSequential), we call this function for those future
        ///     actions, too.
        ///     Destructively modifies throwawayState to teleports agents to end state of the SimAction's intent
        ///     so that calls to future SimActions' DrawIntentDestructive() will start from a plausible future world state.
        ///     WILL MODIFY throwawayState
        /// </summary>
        /// <param name="throwawayState">
        ///     Reference state, WILL BE modified so copy before calling
        ///     <see cref="SimAction.DrawIntentDestructive" />.
        /// </param>
        /// <param name="drawer"></param>
        public abstract void DrawIntentDestructive(SimWorldState throwawayState, IIntentDrawer drawer);

        public abstract IEnumerable<SimAction> EnumerateCurrentPrimitives(SimWorldState state, PrimitiveMode mode);

        public abstract void Execute(SimWorldState state);

        /// <summary>
        ///     Called when an external force has modified the sim.
        /// </summary>
        /// <param name="state"></param>
        public abstract void UpdateForExternalSimChange(SimWorldState state);

        public abstract SimAction DeepCopy();

        /// <summary>
        ///     Called before a sim step to see if any actions aren't sure what to do
        ///     and want to fork the world into multiple different states - in each fork, the action tries a single option.
        ///     Recurse to all actions.
        ///     At most one action should fork per call.
        ///     Then, using one of the new forked states, call again in case another action wants to fork on the same sim step.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public abstract List<SimWorldState>? MaybeForkWorld(SimWorldState state);

        /// <summary>
        ///     Find an action with the given key.
        /// </summary>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        public abstract SimAction? FindAction(long searchKey);

        /// <param name="state"></param>
        /// <returns>Which team is performing this action?</returns>
        public abstract Team GetPerformingTeam(SimWorldState state);

        public struct StatusReport
        {
            /// <summary>
            ///     The actual status of the action.
            /// </summary>
            public ActionStatus status;

            /// why the status is what it is.
            /// Try to avoid allocations for this string in SimAction.GetStatus()
            /// (prefer compile-time constant strings)
            /// But if needed for debugging, go for it.
            /// Mark such usage with a to do comment
            public string? explanation;

            /// which SimAction ultimately decided the status, especially if not CompletedSuccessfully
            public SimAction leafStatusReporter;

            public StatusReport(ActionStatus status, string? explanation, SimAction leafStatusReporter)
            {
                this.status = status;
                this.explanation = explanation;
                this.leafStatusReporter = leafStatusReporter;
            }

            public string ToHumanReadableString()
            {
                return $"{this.status}: {this.explanation}";
            }
        }

        [Serializable]
        public struct BusyStatusReport
        {
            public enum AgentBusyStatus
            {
                NotBusy,
                PersonallyBusy,
                WaitingForOtherAgent
            }

            public static readonly BusyStatusReport NotBusy =
                new BusyStatusReport(AgentBusyStatus.NotBusy, "Default", null);

            /// <summary>
            ///     The actual status of the action.
            /// </summary>
            public AgentBusyStatus status;

            /// why the status is what it is.
            /// Try to avoid allocations for this string in SimAction.GetBusyStatus()
            /// (prefer compile-time constant strings)
            /// But if needed for debugging, go for it.
            /// Mark such usage with a to do comment
            public string? explanation;

            /// which SimAction ultimately decided the status, especially if not CompletedSuccessfully
            public SimAction? leafStatusReporter;

            public BusyStatusReport(AgentBusyStatus status, string? explanation, SimAction? leafStatusReporter)
            {
                this.status = status;
                this.explanation = explanation;
                this.leafStatusReporter = leafStatusReporter;

                if (!this.IsBusy)
                    // Don't record the leaf action if we're reporting NotBusy
                    this.leafStatusReporter = null;
            }

            public bool IsBusy => this.status != AgentBusyStatus.NotBusy;

            public string ToHumanReadableString()
            {
                return this.IsBusy ? $"{this.status}: {this.leafStatusReporter}" : this.status.ToString();
            }
        }
    }
}