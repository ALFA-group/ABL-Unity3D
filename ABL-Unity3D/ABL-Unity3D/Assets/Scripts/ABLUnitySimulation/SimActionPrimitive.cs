using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace ABLUnitySimulation
{
    /// <summary>
    ///     A stateless controller for the sim.
    ///     Ideally, all changes to the world state happen through these primitives,
    ///     or through an external simulator.
    ///     Does not add actions to the sim world state.
    /// </summary>
    public abstract class SimActionPrimitive : SimAction
    {
        public SimGroup actors = new SimGroup("empty");

        public override BusyStatusReport IsBusy(Handle<SimAgent> agent, SimWorldState state, bool useExpensiveExplanation)
        {
            var status = this.actors.Contains(agent)
                ? BusyStatusReport.AgentBusyStatus.PersonallyBusy
                : BusyStatusReport.AgentBusyStatus.NotBusy;

            return new BusyStatusReport(status, "In Primitive Actor list", this);
        }

        public override IEnumerable<SimAction> EnumerateCurrentPrimitives(SimWorldState state, PrimitiveMode _)
        {
            yield return this;
        }

        public override void UpdateForExternalSimChange(SimWorldState state)
        {
            // Primitives should not maintain internal state 
        }

        public override SimAction DeepCopy()
        {
            return (SimActionPrimitive)this.MemberwiseClone();
        }

        public override List<SimWorldState>? MaybeForkWorld(SimWorldState state)
        {
            return null;
        }

        public override SimAction? FindAction(long searchKey)
        {
            return null;
        }

        public abstract string GetUsefulInspectorInformation(SimWorldState simWorldState);

        public override Team GetPerformingTeam(SimWorldState state)
        {
            if (this.actors.Count < 1) return Team.Undefined;
            return state.Get(this.actors.First()).team;
        }
    }
}