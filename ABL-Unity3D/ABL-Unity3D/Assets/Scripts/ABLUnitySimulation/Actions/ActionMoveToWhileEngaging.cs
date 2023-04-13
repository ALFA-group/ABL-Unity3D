using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation.Actions.Helpers;
using UnityEngine;
using Utilities.GeneralCSharp;
using Utilities.Unity;

#nullable enable

namespace ABLUnitySimulation.Actions
{
    /// <summary>
    ///     Move to a location while attacking any agents the given <see cref="SimGroup" /> sees along the way.
    /// </summary>
    [Serializable]
    public class ActionMoveToWhileEngaging : SimActionCompound
    {
        private ActionMoveToPositionWithPathfinding? _actionMoveAlongPath;
        [SerializeField]
        public SimGroup attackers;
        [SerializeField]
        public Circle destination;
        public float kmEngagementWidth;
        [SerializeField]
        public Vector2? startingPoint;
        public Team targetTeam;

        public ActionMoveToWhileEngaging(SimGroup attackers)
        {
            this.attackers = attackers;
        }

        public override StatusReport GetStatus(SimWorldState state, bool useExpensiveExplanation)
        {
            if (!this.attackers.Get(state).Any(a => a.CanMove))
                return new StatusReport(ActionStatus.Impossible, "No surviving agents", this);

            return this._actionMoveAlongPath?.GetStatus(state, useExpensiveExplanation)
                   ?? new StatusReport(ActionStatus.InProgress, "Have not created path yet", this);
        }

        public override BusyStatusReport IsBusy(Handle<SimAgent> agent, SimWorldState state,
            bool calculateExpensiveExplanation)
        {
            if (!this.attackers.Contains(agent))
                return new BusyStatusReport(BusyStatusReport.AgentBusyStatus.NotBusy, null, this);

            var simAgent = agent.GetCanFail(state);
            if (null == simAgent)
                return new BusyStatusReport(BusyStatusReport.AgentBusyStatus.NotBusy, "SimAgent is null in state", this);

            if (this.destination.IsInside(simAgent.positionActual))
                return new BusyStatusReport(BusyStatusReport.AgentBusyStatus.WaitingForOtherAgent, "At destination",
                    this);

            return new BusyStatusReport(BusyStatusReport.AgentBusyStatus.PersonallyBusy, "Not at destination", this);
        }

        public override void Execute(SimWorldState state)
        {
            foreach (var simAction in this.EnumerateCurrentPrimitives(state, PrimitiveMode.ABLUnity))
                simAction.Execute(state);
        }

        public override void DrawIntentDestructive(SimWorldState throwawayState, IIntentDrawer drawer)
        {
            var actor = throwawayState.Get(this.attackers).FirstOrDefault();
            if (null == actor) return;

            // The sub action might not exist yet, so init here.
            this._actionMoveAlongPath ??= new ActionMoveToPositionWithPathfinding(this.attackers,
                this.startingPoint ?? actor.positionActual,
                this.destination.center,
                this.destination.kmRadius);

            this._actionMoveAlongPath.DrawIntentDestructive(throwawayState, drawer);
        }

        public override IEnumerable<SimAction> EnumerateCurrentPrimitives(SimWorldState state, PrimitiveMode mode)
        {
            var primaryAttacker = this.GetPrimaryAttacker(state);
            if (null == primaryAttacker) yield break;

            this.startingPoint ??=
                primaryAttacker.positionActual; // assign starting point the first time we use this action.
            this._actionMoveAlongPath ??= new ActionMoveToPositionWithPathfinding(this.attackers,
                this.startingPoint.Value,
                this.destination.center,
                this.destination.kmRadius);

            var segments = this._actionMoveAlongPath.EnumerateSubPathInUse(state).ToList();
            var targets = state.GetTeam(this.targetTeam).Where(enemy =>
                    enemy.IsWorthShooting() && this.IsOnSegment(enemy.GetObservedPosition(primaryAttacker), segments))
                .ToList();

            var weHaveTargets = new HashSet<SimAgent>();
            var possibleAttackers = this.attackers.Get(state).ToList();

            if (targets.Count >= 1)
                // Each agent attacks a target if it near enough
                foreach (var attacker in possibleAttackers)
                {
                    if (!attacker.CanFire) continue;

                    var nearestTarget = targets.ArgMin(t => attacker.DistanceSqr2dAccordingToMe(t));
                    if (attacker.IsNearAccordingToMe(nearestTarget, this.kmEngagementWidth)
                        || this.destination.IsInside(nearestTarget.GetObservedPosition(attacker)))
                    {
                        yield return new ActionAttackToDestroy
                        {
                            actors = new SimGroup(attacker),
                            name = "MoveToWhileEngaging",
                            target = nearestTarget,
                            movementGuidance = ActionAttackToDestroy.MovementGuidance.GetIntoClosestRange
                        };

                        weHaveTargets.Add(attacker);
                    }
                }

            var justMoving = possibleAttackers.Except(weHaveTargets);
            var tempMove = (ActionMoveToPositionWithPathfinding)this._actionMoveAlongPath.DeepCopy();
            tempMove.actors = new SimGroup(justMoving);

            foreach (var primitive in tempMove.EnumerateCurrentPrimitives(state, mode)) yield return primitive;
        }

        private SimAgent? GetPrimaryAttacker(SimWorldState state)
        {
            return this.attackers.Get(state).FirstOrDefault(attacker => attacker.CanFire);
        }

        private bool IsOnSegment(Vector2 enemyPosition, Segment2 segment)
        {
            return enemyPosition.DistanceToSegmentSquared(segment.endA, segment.endB) <=
                   this.kmEngagementWidth * this.kmEngagementWidth;
        }

        private bool IsOnSegment(Vector2 enemyPosition, IEnumerable<Segment2> segments)
        {
            return segments.Any(s => this.IsOnSegment(enemyPosition, s));
        }

        public override void UpdateForExternalSimChange(SimWorldState state)
        {
        }

        public override SimAction DeepCopy()
        {
            return (SimAction)this.MemberwiseClone();
        }

        public override List<SimWorldState>? MaybeForkWorld(SimWorldState state)
        {
            return null;
        }

        public override SimAction? FindAction(long searchKey)
        {
            return this.key == searchKey ? this : null;
        }

        public override void OnDrawGizmos(SimWorldState state, Handle<SimAgent> agentHandle,
            Func<Vector2, Vector3> simToUnityCoords)
        {
            this._actionMoveAlongPath?.OnDrawGizmos(state, agentHandle, simToUnityCoords);
        }

        public override Team GetPerformingTeam(SimWorldState state)
        {
            if (this.attackers.Count < 1) return Team.Undefined;
            return state.Get(this.attackers.First()).team;
        }
    }
}