using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using Utilities.GeneralCSharp;

#nullable enable

namespace ABLUnitySimulation.Actions
{
    
    [Serializable]
    public class ActionArtillery : SimActionPrimitive
    {
        public Team shootingTeam = Team.Blue;

        public override StatusReport GetStatus(SimWorldState state, bool useExpensiveExplanation)
        {
            return new StatusReport(ActionStatus.InProgress, "Never completes.", this);
        }

        public override void Execute(SimWorldState state)
        {
            var shooters = state.GetTeam(this.shootingTeam);
            shooters.ForEach(actor => this.ExecuteForActor(state, actor));
        }

        public override string GetUsefulInspectorInformation(SimWorldState simWorldState)
        {
            return "";
        }

        private static List<SimAgent> GetTargets(SimWorldState state, SimAgent shooter)
        {
            return state.Agents.Where(agent => agent.IsActive && shooter.IsHostile(agent)).ToList();
        }

        private void ExecuteForActor(SimWorldState state, SimAgent shooter)
        {
            List<SimAgent>? targets = null;

            // Code assumes each entity only has one indirect option
            //Debug.Assert(!shooter.entities.Any(entity => entity.data.weapons.Count(w => w.isIndirect) > 1));

            foreach (var entityShooter in shooter.entities)
            {
                if (!entityShooter.IsActive) continue;

                var alreadyFoundIndirectWeapon = false;
                foreach (var weapon in entityShooter.data.weapons.Where(w => w.isIndirect))
                {
                    // Silly way to check that no entity has multiple indirect options.
                    //  This probably won't hold true... if it doesn't, need to recode to somehow select "best" indirect option
                    Debug.Assert(!alreadyFoundIndirectWeapon);
                    alreadyFoundIndirectWeapon = true;

                    targets ??= GetTargets(state, shooter);
                    var primaryTarget = this.GetPrimaryTarget(state, shooter, weapon, targets);

                    if (null != primaryTarget)
                    {
                        var centerOfBlast = primaryTarget.GetObservedPosition(shooter);
                        var damageProfile = weapon.damage;
                        foreach (var target in targets.Where(target =>
                                     target.IsNearActual(centerOfBlast, weapon.kmStrikeRadius)))
                            target.TakeAreaDamage(damageProfile, entityShooter.ActiveCount,
                                state.SecondsSinceLastUpdate);
                    }
                }
            }
        }

        private SimAgent? GetPrimaryTarget(SimWorldState state, SimAgent shooter, SimWeapon weapon, List<SimAgent> targets)
        {
            if (targets.Count < 1) return null;
            if (!weapon.isIndirect) return null;

            bool IsValidTarget(SimAgent target)
            {
                var observedTargetLocation = target.GetObservedPosition(shooter);
                if (!observedTargetLocation.IsRecent(state, 300)) return false; // position data too old!
                if (!shooter.IsNearAccordingToMe(target, weapon.kmMaxRange)) return false; // too far!
                if (shooter.IsNearAccordingToMe(target, weapon.kmMinRange)) return false; // too close!

                return true;
            }

            // Who might I shoot?
            var closestVictim = targets
                .Where(IsValidTarget)
                .ArgMinOrDefault(shooter.DistanceSqr2dAccordingToMe);

            return closestVictim;
        }

        public override void OnDrawGizmos(SimWorldState state, Handle<SimAgent> agentHandle,
            Func<Vector2, Vector3> simToUnityCoords)
        {
            var possibleShooter = agentHandle.Get(state);
            List<SimAgent>? targets = null;

            foreach (var entityShooter in possibleShooter.entities)
            {
                if (!entityShooter.IsActive) continue;

                foreach (var weapon in entityShooter.data.weapons.Where(w => w.isIndirect))
                {
                    targets ??= GetTargets(state, possibleShooter);
                    var primaryTarget = this.GetPrimaryTarget(state, possibleShooter, weapon, targets);

                    if (null != primaryTarget)
                    {
                        var unityCenter = simToUnityCoords(primaryTarget.GetObservedPosition(possibleShooter));
                        Gizmos.DrawWireSphere(unityCenter, weapon.kmStrikeRadius);
                        Gizmos.DrawSphere(simToUnityCoords(possibleShooter.positionActual) + Vector3.up * 0.1f, 0.05f);
                        return;
                    }
                }
            }
        }

        public override void DrawIntentDestructive(SimWorldState currentStateWillChange, IIntentDrawer drawer)
        {
        }
    }
}