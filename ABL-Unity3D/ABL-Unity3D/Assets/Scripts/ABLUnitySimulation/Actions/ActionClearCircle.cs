using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation.Actions.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using Utilities.GeneralCSharp;

#nullable enable

namespace ABLUnitySimulation.Actions
{
    /// <summary>
    ///     Destroy all enemies in a given circle on the map.
    /// </summary>
    [Serializable]
    public class ActionClearCircle : SimActionCompound
    {
        [SerializeField]
        public SimGroup attackers;
        [SerializeField]
        public Team attackingTeam;
        [SerializeField]
        public Team targetTeam;

        private Dictionary<Handle<SimAgent>, ActionAttackToDestroy> _cacheAttackToDestroys =
            new Dictionary<Handle<SimAgent>, ActionAttackToDestroy>();

        [SerializeField]
        
        public Circle circle;

        public string? debugName;
        public bool occupyCircleAfterwards;

        public ActionClearCircle(IEnumerable<Handle<SimAgent>> shooters, Team attackingTeam, Team targetTeam,
            Circle circle, bool occupyCircleAfterwards)
        {
            this.attackers = new SimGroup(shooters);
            this.attackingTeam = attackingTeam;
            this.targetTeam = targetTeam;
            this.circle = circle;
            this.occupyCircleAfterwards = occupyCircleAfterwards;
        }

        public override SimAction DeepCopy()
        {
            var copy = (ActionClearCircle)this.MemberwiseClone();
            copy._cacheAttackToDestroys = new Dictionary<Handle<SimAgent>, ActionAttackToDestroy>();
            return copy;
        }

        public override StatusReport GetStatus(SimWorldState state, bool useExpensiveExplanation)
        {
            var attackerAgents = this.attackers.Get(state);
            var targets = this.EnumerateCurrentTargets(state).ToList();
            
            if (!targets.Any())
            {
                // No targets!
                if (!this.occupyCircleAfterwards)
                    return new StatusReport(ActionStatus.CompletedSuccessfully, "No more targets, no occupy command.",
                        this);

                if (attackerAgents.All(attacker => !attacker.CanMove || this.circle.IsInside(attacker.positionActual)))
                    return new StatusReport(ActionStatus.CompletedSuccessfully,
                        "No targets, all attackers at destination or immovable", this);

                return new StatusReport(ActionStatus.InProgress, "No targets, moving to occupy destination", this);
            }
            
            var attackersThatCanHurtThem = attackerAgents
                .Where(attacker => attacker.CanFire && attacker.CanMove)
                .Where(attacker => targets.Any(attacker.CanDamage));
            bool canHurtThem = attackersThatCanHurtThem.Any();

            var report = canHurtThem
                ? new StatusReport(ActionStatus.InProgress, "Clearing targets", this)
                : new StatusReport(ActionStatus.Impossible, "Targets present, but cannot engage", this);
            if (!useExpensiveExplanation) return report;

            //////// Expensive explanation.  We modify report rather than creating a new one to guarantee that only the explanation text can change. 
            if (!attackerAgents.Any(a => a.CanFire))
            {
                Assert.AreEqual(report.status, ActionStatus.Impossible);
                report.explanation = "No attackers can fire";
                return report;
            }

            if (!canHurtThem)
            {
                Assert.AreEqual(report.status, ActionStatus.Impossible);
                report.explanation = "No attackers can hurt the enemy (simAgentType)";
                return report;
            }

            // In range?

            int countActiveShooters = attackersThatCanHurtThem.Count(a =>
                a.CanFire && targets.Any(t => a.IsNearAccordingToMe(t, a.KmMaxRange())));
            int countMovableShooters = attackersThatCanHurtThem.Count(a => a.CanMove && a.CanFire);
            report.explanation =
                $"{countActiveShooters} Effective attackers in range and firing.  {countMovableShooters} Effective Attackers can both move and shoot.";

            foreach (var primitiveAction in this.EnumerateCurrentPrimitives(state, PrimitiveMode.ABLUnity))
            {
                var primitiveReport = primitiveAction?.GetStatus(state, true);
                if (primitiveReport.HasValue)
                    if (primitiveReport.Value.status == report.status)
                    {
                        Assert.AreEqual(report.status, primitiveReport.Value.status);
                        return primitiveReport.Value;
                    }
            }

            return report;
        }

        public override BusyStatusReport IsBusy(Handle<SimAgent> agent, SimWorldState state,
            bool calculateExpensiveExplanation)
        {
            if (!this.attackers.Contains(agent))
                return new BusyStatusReport(BusyStatusReport.AgentBusyStatus.NotBusy, null, this);

            if (!calculateExpensiveExplanation)
                return new BusyStatusReport(
                    BusyStatusReport.AgentBusyStatus.PersonallyBusy,
                    "Part of attacking group",
                    this
                );

            var SimAgent = agent.GetCanFail(state);
            if (null == SimAgent)
                return new BusyStatusReport(BusyStatusReport.AgentBusyStatus.NotBusy, $"{agent} not found", this);

            var targets = this.EnumerateCurrentTargets(state).ToList();
            var agentPrimitive = this.GetCurrentPrimitiveFor(state, SimAgent, targets);
            if (null == agentPrimitive)
                return new BusyStatusReport(BusyStatusReport.AgentBusyStatus.WaitingForOtherAgent,
                    "No current primitive",
                    this);

            var busyStatus = agentPrimitive.IsBusy(agent, state, calculateExpensiveExplanation);
            busyStatus.explanation = $"ClearCircle: {busyStatus.explanation}";
            return busyStatus;
        }

        public override void DrawIntentDestructive(SimWorldState currentStateWillChange, IIntentDrawer drawer)
        {
            var shooter = this.attackers.Get(currentStateWillChange).FirstOrDefault();
            if (null == shooter) return;

            drawer.DrawCircle(this.attackingTeam, this.circle);
            drawer.DrawText(this.attackingTeam, this.circle.center, "Clear");

            // Creating a temp pathfinding action to draw the path
            
            // Ideally the code would be shared, but I didn't want to call the whole EnumerateCurrentPrimitives chain.
            //  In particular, I want to guarantee that I get a move command without confusing the issue with the likely attack commands.
            var occupationCircle = this.GetOccupationCircle();
            if (!occupationCircle.IsInside(shooter.positionActual))
            {
                var move = new ActionMoveToPositionWithPathfinding(
                    (Handle<SimAgent>)shooter,
                    shooter.positionActual,
                    occupationCircle.center,
                    occupationCircle.kmRadius)
                {
                    name = "Occupy waypoint"
                };

                move.DrawIntentDestructive(currentStateWillChange, drawer);
            }
        }

        private Circle GetOccupationCircle()
        {
            return new Circle(this.circle.center, this.circle.kmRadius / 4);
        }

        public override IEnumerable<SimAction> EnumerateCurrentPrimitives(SimWorldState state, PrimitiveMode mode)
        {
            var targets = this.EnumerateCurrentTargets(state).ToList();

            return this.EnumerateCurrentPrimitivesFor(state, this.attackers.Get(state), targets);
        }

        protected IEnumerable<SimAgent> EnumerateCurrentTargets(SimWorldState state)
        {
            var targets = state.GetTeam(this.targetTeam)
                // .Where(enemy => enemy.GetPositionObservedByEnemy().IsRecent(state, 300))
                .Where(enemy => this.circle.IsInside(enemy.positionActual))//GetObservedPosition(this.attackingTeam)));
                .Where(enemy => enemy.IsWorthShooting());
            return targets;
        }

        public override void Execute(SimWorldState state)
        {
            foreach (var simAction in this.EnumerateCurrentPrimitives(state, PrimitiveMode.ABLUnity))
                simAction.Execute(state);
        }

        public override void UpdateForExternalSimChange(SimWorldState state)
        {
        }

        public override List<SimWorldState>? MaybeForkWorld(SimWorldState state)
        {
            return null;
        }

        public override SimAction? FindAction(long searchKey)
        {
            return this.key == searchKey ? this : null;
        }

        public override Team GetPerformingTeam(SimWorldState state)
        {
            if (this.attackers.Count < 1) return Team.Undefined;
            return state.Get(this.attackers.First()).team;
        }

        private IEnumerable<SimActionPrimitive> EnumerateCurrentPrimitivesFor(SimWorldState state,
            IEnumerable<SimAgent> shooters,
            List<SimAgent> targets)
        {
            return shooters.Select(shooter => this.GetCurrentPrimitiveFor(state, shooter, targets)).WhereNotNull();
        }

        private SimActionPrimitive? GetCurrentPrimitiveFor(SimWorldState state, SimAgent shooter, List<SimAgent> targets)
        {
            if (shooter.CanFire)
            {
                var targetsWeCanHurt = targets.Where(shooter.CanDamage);
                var closestVictim =
                    targetsWeCanHurt.ArgMinOrDefault(shooter
                        .DistanceSqr2dAccordingToMe); 

                if (null != closestVictim)
                {
                    if (this._cacheAttackToDestroys.TryGetValue(shooter, out var oldAttackToDestroy))
                    {
                        if (oldAttackToDestroy.target == closestVictim.Handle) return oldAttackToDestroy;

                        var oldTarget = oldAttackToDestroy.target.GetCanFail(state);
                        if (null != oldTarget && oldTarget.IsActive)
                            // Don't get distracted by the new closest target
                            return oldAttackToDestroy;
                    }

                    var newAttackToDestroy = new ActionAttackToDestroy
                    {
                        actors = shooter,
                        name = "AttackInCircle",
                        target = closestVictim,
                        movementGuidance = ActionAttackToDestroy.MovementGuidance.GetIntoClosestRange
                    };
                    this._cacheAttackToDestroys[shooter] = newAttackToDestroy;
                    return newAttackToDestroy;
                }
            }

            if (!this.occupyCircleAfterwards) return null;

            var occupationCircle = this.GetOccupationCircle();
            if (!occupationCircle.IsInside(shooter.positionActual) && shooter.CanMove)
                return new ActionMoveToPositionWithPathfinding(
                    (Handle<SimAgent>)shooter,
                    shooter.positionActual,
                    occupationCircle.center,
                    occupationCircle.kmRadius)
                {
                    name = "Occupy waypoint"
                };

            return null;
        }
    }
}