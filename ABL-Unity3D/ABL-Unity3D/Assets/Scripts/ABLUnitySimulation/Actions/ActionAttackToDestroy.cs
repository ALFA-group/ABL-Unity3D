using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ABLUnitySimulation.Actions
{
    /// <summary>
    ///     Primitive SimAction which moves toward an enemy and attacks to destroy it.
    /// </summary>
    [Serializable]
    public class ActionAttackToDestroy : SimActionPrimitive
    {
        public enum MovementGuidance
        {
            NeverMove,
            GetBarelyIntoRange,
            GetIntoClosestRange
        }

        private Dictionary<Handle<SimAgent>, ActionMoveToPositionWithPathfinding> _cachedMoveActions =
            new Dictionary<Handle<SimAgent>, ActionMoveToPositionWithPathfinding>();

        [SerializeField]
        public MovementGuidance movementGuidance = MovementGuidance.NeverMove;
        [SerializeField]
        public Handle<SimAgent> target;

        public override void Execute(SimWorldState state)
        {
            var targetAgent = state.Get(this.target);
            foreach (var attackerAgent in this.actors.Get(state))
                this.ExecuteForOneAttacker(state, attackerAgent, targetAgent);
        }

        private void ExecuteForOneAttacker(SimWorldState state, SimAgent attacker, SimAgent targetAgent)
        {
            Assert.AreNotEqual(attacker.team, targetAgent.team);

            var everyoneFired = true;
            foreach (var entity in attacker.entities)
                if (entity.CanFire)
                {
                    bool someoneFired = entity.AttackDirect(state, attacker, targetAgent);
                    if (!someoneFired) everyoneFired = false;
                }

            if (this.movementGuidance == MovementGuidance.NeverMove) return;

            if (!everyoneFired)
            {
                float desiredDistance = this.movementGuidance == MovementGuidance.GetBarelyIntoRange
                    ? attacker.KmMaxRange() * 0.9f
                    : attacker.KmSmallestMaxRange() * 0.9f;

                var preferredDestination = targetAgent.GetObservedPosition(attacker);

                // We're close.  Stop moving.
                if (attacker.IsNearActual(preferredDestination, desiredDistance)) return;

                if (this._cachedMoveActions.TryGetValue(attacker, out var oldMoveAction))
                {
                    float distanceBetweenDestinationsSqr =
                        (oldMoveAction.preferredDestination - preferredDestination).sqrMagnitude;
                    if (distanceBetweenDestinationsSqr < desiredDistance * desiredDistance / 2)
                    {
                        oldMoveAction.Execute(state);
                        return;
                    }
                }

                var attackerHandle = attacker.Handle;
                // stupider and slower
                var moveAction = new ActionMoveToPositionWithPathfinding(attackerHandle,
                    attacker.positionActual,
                    preferredDestination,
                    desiredDistance)
                {
                    name = $"move from {nameof(ActionAttackToDestroy)}"
                };

                Assert.IsTrue(attackerHandle.IsValid);
                Assert.IsTrue(attacker.Handle.IsValid);
                this._cachedMoveActions[attackerHandle] = moveAction;
                moveAction.Execute(state);
            }
        }

        public override StatusReport GetStatus(SimWorldState state, bool useExpensiveExplanation)
        {
            var targetAgent = state.Get(this.target);
            if (!targetAgent.IsWorthShooting())
                return new StatusReport(ActionStatus.CompletedSuccessfully, "Target not worth shooting", this);

            foreach (var attackerAgent in this.actors.Get(state))
            {
                bool attackerCanDamageTarget = attackerAgent.CanDamage(targetAgent);

                if (attackerCanDamageTarget && this.CanAttack(attackerAgent, targetAgent))
                    return !useExpensiveExplanation
                        ? new StatusReport(ActionStatus.InProgress, "At least one attacker actively shooting", this)
                        : new StatusReport(ActionStatus.InProgress,
                            $"{attackerAgent.Name} actively shooting {targetAgent.Name}", this);

                if (attackerCanDamageTarget && attackerAgent.CanFire && attackerAgent.CanMove)
                    return !useExpensiveExplanation
                        ? new StatusReport(ActionStatus.InProgress,
                            "At least one attack capable of moving and attacking", this)
                        : new StatusReport(ActionStatus.InProgress,
                            $"{attackerAgent.Name} can move and attack but is at " +
                            $"{attackerAgent.positionActual}, " +
                            $"{attackerAgent.Distance2dAccordingToMe(targetAgent)}/{attackerAgent.KmMaxRange()}km away from " +
                            $"{targetAgent.Name} at {targetAgent.GetObservedPosition(attackerAgent).Position}",
                            this);
            }

            return new StatusReport(ActionStatus.Impossible, "Cannot reach target with an effective active attacker",
                this);
        }

        private bool CanAttack(SimAgent attackerAgent, SimAgent targetAgent)
        {
            foreach (var attackerEntity in attackerAgent.entities)
                if (attackerEntity.CanAttack(attackerAgent, targetAgent))
                    return true;

            return false;
        }

        public override void OnDrawGizmos(SimWorldState state, Handle<SimAgent> agentHandle,
            Func<Vector2, Vector3> simToUnityCoords)
        {
            if (this.actors.Contains(agentHandle))
            {
                var possibleShooterAgent = agentHandle.Get(state);
                var targetAgent = state.Get(this.target);
                if (this.CanAttack(possibleShooterAgent, targetAgent))
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(simToUnityCoords(possibleShooterAgent.positionActual),
                        simToUnityCoords(targetAgent.GetObservedPosition(possibleShooterAgent)));
                }
            }
        }

        public override void DrawIntentDestructive(SimWorldState currentStateWillChange, IIntentDrawer drawer)
        {
            var performingTeam = this.GetPerformingTeam(currentStateWillChange);
            var targetPosition = this.target.Get(currentStateWillChange).positionActual;
            drawer.DrawText(performingTeam, targetPosition, "Attack");
        }

        public override string GetUsefulInspectorInformation(SimWorldState simWorldState)
        {
            if (!this.target.IsValid) return "Invalid Target";

            var targetAgent = simWorldState.Get(this.target);
            return $"Agent: {targetAgent}, Position: {targetAgent.positionActual}";
        }

        public override SimAction DeepCopy()
        {
            var copy = (ActionAttackToDestroy)this.MemberwiseClone();
            copy._cachedMoveActions = new Dictionary<Handle<SimAgent>, ActionMoveToPositionWithPathfinding>();
            return copy;
        }
    }
}