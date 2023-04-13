using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using UnityEngine;
using Utilities.GeneralCSharp;

#nullable enable

namespace ABLUnitySimulation.Actions
{
    /// <summary>
    ///     Attack any enemy agent that comes into sight of the given <see cref="SimAgent" />.
    /// </summary>
    [Serializable]
    public class ActionOpportunityFire : SimActionCompound
    {
        [SerializeField]
        public readonly Team attackingTeam;
        [SerializeField]
        public readonly Team targetTeam;
        [SerializeField]
        public Handle<SimAgent> onlyMeShoots;

        public ActionOpportunityFire(Team attackingTeam, Team targetTeam)
        {
            this.attackingTeam = attackingTeam;
            this.targetTeam = targetTeam;
        }

        public override StatusReport GetStatus(SimWorldState state, bool useExpensiveExplanation)
        {
            return new StatusReport(ActionStatus.InProgress, "Never Completes", this);
        }

        public override BusyStatusReport IsBusy(Handle<SimAgent> agent, SimWorldState state,
            bool calculateExpensiveExplanation)
        {
            return new BusyStatusReport(BusyStatusReport.AgentBusyStatus.NotBusy,
                "Opportunity fire never makes a agent busy",
                this);
        }

        public override void DrawIntentDestructive(SimWorldState throwawayState, IIntentDrawer drawer)
        {
        }

        public override IEnumerable<SimAction> EnumerateCurrentPrimitives(SimWorldState state, PrimitiveMode mode)
        {
            return this.EnumerateCurrentPrimitivesFor(state, this.attackingTeam, this.targetTeam);
        }

        public override void Execute(SimWorldState state)
        {
            foreach (var simAction in this.EnumerateCurrentPrimitives(state, PrimitiveMode.ABLUnity))
                simAction.Execute(state);
        }

        protected IEnumerable<SimActionPrimitive> EnumerateCurrentPrimitivesFor(SimWorldState state, Team teamShooting,
            Team teamTarget)
        {
            var shooters = state.GetTeam(teamShooting).Where(u => u.CanFire).ToList();
            var targets =
                state.GetTeam(teamTarget).Where(u => u.IsWorthShooting())
                    .ToList(); 

            // This is way too slow with lots of agents!
            if (shooters.Count > 200 || targets.Count > 200)
            {
                Debug.LogWarning("Too many agents for ActionOpportunityFire!");
                return Enumerable.Empty<SimActionPrimitive>();
            }

            if (this.onlyMeShoots.IsValid)
                
                shooters = shooters.Where(s => this.onlyMeShoots.simId == s.SimId).ToList();

            return this.EnumerateCurrentPrimitivesFor(shooters, targets);
        }

        protected IEnumerable<SimActionPrimitive> EnumerateCurrentPrimitivesFor(List<SimAgent> shooters,
            List<SimAgent> targets)
        {
            return shooters.Select(shooter => this.GetCurrentPrimitiveFor(shooter, targets)).WhereNotNull();
        }

        protected SimActionPrimitive? GetCurrentPrimitiveFor(SimAgent shooter, List<SimAgent> targets)
        {
            if (targets.Count < 1) return null;
            if (!shooter.CanFire) return null;

            // Who might I shoot?
            var closestVictim = targets.ArgMin(shooter.DistanceSqr2dAccordingToMe);
            float longestRange = shooter.KmMaxRange();
            if (shooter.IsNearAccordingToMe(closestVictim, longestRange))
                return new ActionAttackToDestroy
                {
                    actors = shooter,
                    name = "OpportunityFire",
                    target = closestVictim,
                    movementGuidance = ActionAttackToDestroy.MovementGuidance.NeverMove
                };

            return null;
        }


        public override void UpdateForExternalSimChange(SimWorldState state)
        {
        }

        public override SimAction DeepCopy()
        {
            return (ActionOpportunityFire)this.MemberwiseClone();
        }

        public override List<SimWorldState>? MaybeForkWorld(SimWorldState state)
        {
            return null;
        }

        public override void OnDrawGizmos(SimWorldState state, Handle<SimAgent> agentHandle,
            Func<Vector2, Vector3> simToUnityCoords)
        {
            var targets =
                state.GetTeam(this.targetTeam).Where(u => u.IsWorthShooting())
                    .ToList(); 

            var maybeShoot = this.GetCurrentPrimitiveFor(agentHandle.Get(state), targets);
            maybeShoot?.OnDrawGizmos(state, agentHandle, simToUnityCoords);
        }

        public override SimAction? FindAction(long searchKey)
        {
            if (this.key == searchKey) return this;
            return null;
        }

        public override Team GetPerformingTeam(SimWorldState state)
        {
            return this.attackingTeam;
        }
    }
}