using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using UnityEngine;

#nullable enable

namespace Planner.ManyWorlds
{
    /// <summary>
    ///     A <see cref="SimAction" /> that take a goal method and implements a plan decomposition to achieve that goal.
    /// </summary>
    /// <remarks>
    ///     The goal is defined at creation time, while the implementation and decomposition are not defined until
    ///     after the world state has been forked. This is necessary because the values of fields for <see cref="SimAgent" />s
    ///     change every frame. If the decomposition are implementation were to be defined at creation time, they would be
    ///     defined using the wrong values because they will not be the current values when the world state forks.
    /// </remarks>
    public class ActionManyWorldsPlan : SimActionCompound
    {
        /// <summary>
        ///     The goal to achieve.
        /// </summary>
        public readonly Method goal;

        public ActionManyWorldsPlan(Method goal)
        {
            this.goal = goal;
        }

        public override SimAction DeepCopy()
        {
            var clone = (ActionManyWorldsPlan)this.MemberwiseClone();
            clone.implementation = this.implementation?.DeepCopy();
            return clone;
        }

        public override StatusReport GetStatus(SimWorldState state, bool useExpensiveExplanation)
        {
            if (null == this.implementation)
                return new StatusReport(ActionStatus.InProgress, "No implementation, unforked", this);

            return this.implementation.GetStatus(state, useExpensiveExplanation);
        }

        public override BusyStatusReport IsBusy(Handle<SimAgent> agent, SimWorldState state,
            bool calculateExpensiveExplanation)
        {
            if (null == this.implementation)
                return new BusyStatusReport(BusyStatusReport.AgentBusyStatus.NotBusy, "No implementation, unforked",
                    this);

            return this.implementation.IsBusy(agent, state, calculateExpensiveExplanation);
        }

        public override void DrawIntentDestructive(SimWorldState throwawayState, IIntentDrawer drawer)
        {
        }

        public override IEnumerable<SimAction> EnumerateCurrentPrimitives(SimWorldState state, PrimitiveMode mode)
        {
            var empty = this.implementation?.EnumerateCurrentPrimitives(state, mode);
            if (empty != null) return empty;

            return Enumerable.Empty<SimActionPrimitive>();
        }

        public override void Execute(SimWorldState state)
        {
            this.implementation?.Execute(state);
        }

        public override void UpdateForExternalSimChange(SimWorldState state)
        {
            this.implementation?.UpdateForExternalSimChange(state);
        }

        public override List<SimWorldState>? MaybeForkWorld(SimWorldState state)
        {
            if (null != this.implementation) return this.implementation.MaybeForkWorld(state);

            var context = new PlannerContext
            {
                state = state,
                methods = state.GetMethodLibrary()
            };

            var forks = new List<SimWorldState>();
            var index = 0;
            foreach (var d in this.goal.Decompose(context))
            {
                Debug.Assert(state.actions.FindAction(this.key) is ActionManyWorldsPlan);

                var fork = state.DeepCopy();
                var myClone = fork.actions.FindAction(this.key) as ActionManyWorldsPlan
                              ?? throw new Exception("Could not find clone in forked state!");

                myClone.implementation = this.CreateImplementation(d);
                myClone.decompositionChosen = d;
                myClone.decompositionIndex = index;
                forks.Add(fork);

                ++index;
            }

            if (forks.Count > 0) return forks;

            // Don't run the fork code again, please.
            // Since we should only hit the fork code RIGHT BEFORE we are asked to execute, should be safe to resolve to SimAction here.
            // Adding a ActionNoOp fallback because we can have an empty decomposition or SimAction from a method if there is really nothing to do
            //   e.g., attack a dead enemy
            this.implementation = this.goal.GetActionForSim(state) ?? new ActionNoOp();

            return null;
        }

        private SimAction CreateImplementation(Decomposition decomposition)
        {
            var wrappedMethods = decomposition.subtasks.
                Select(task => new ActionManyWorldsPlan(task));

            switch (decomposition.mode)
            {
                case Decomposition.ExecutionMode.Parallel:
                    return new ActionParallel(wrappedMethods);
                case Decomposition.ExecutionMode.Sequential:
                    return new ActionSequential(wrappedMethods);
                default:
                    throw new Exception($"Unknown decomposition mode {decomposition.mode}");
            }
        }

        public override SimAction? FindAction(long searchKey)
        {
            if (this.key == searchKey) return this;
            return this.implementation?.FindAction(searchKey);
        }

        public override Team GetPerformingTeam(SimWorldState state)
        {
            return state.teamFriendly; 
        }

        #region Filled in after Fork

        /// <summary>
        ///     The implementation of <see cref="decompositionChosen" />. Not defined until after the world state forks.
        /// </summary>
        public SimAction? implementation;

        /// <summary>
        ///     The plan decomposition chosen to achieve <see cref="goal" />. Not defined until after the world state forks.
        /// </summary>
        public Decomposition? decompositionChosen;

        public int decompositionIndex = -1;

        #endregion
    }
}