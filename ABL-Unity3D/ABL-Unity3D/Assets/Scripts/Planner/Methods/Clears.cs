using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using ABLUnitySimulation.Actions.Helpers;
using UnityEngine;
using Utilities.GeneralCSharp;
using Utilities.Unity;

#nullable enable

namespace Planner.Methods
{
    public struct SpecClearCircle : ITaskSpec
    {
        public Team friendlyTeam;
        public Circle circle;
        public bool occupyCircle;

        public override string ToString()
        {
            return $"{this.friendlyTeam} clears {this.circle} occupy:{this.occupyCircle}";
        }
    }

    /// <summary>
    ///     Clear each circle in order.
    ///     If "occupyCircle" is true, move to the circle when done.
    /// </summary>
    public struct SpecClearCirclesWith : ITaskSpec
    {
        public IReadOnlyList<Circle> circles;
        public SimGroup friendlies;
        public bool occupyCircle;

        public override string ToString()
        {
            return $"{this.friendlies} clears {this.circles.Count} circles";
        }
    }

    /// <summary>
    ///     Clear each circle in order.
    ///     If "occupyCircle" is true, move to the circle when done.
    /// </summary>
    public struct SpecClearOneCircleWith : ITaskSpec
    {
        public IReadOnlyList<Circle> circleOptions;
        [SerializeReference]
        public SimGroup friendlies;
        public bool occupyCircle;

        public override string ToString()
        {
            return $"{this.friendlies} clears one of {this.circleOptions.Count} circles";
        }
    }


    public struct SpecClearCircleWith : ITaskSpec
    {
        public Circle circle;
        public SimGroup friendlies;
        public bool occupyCircle;

        public override string ToString()
        {
            return $"{this.friendlies} clears {this.circle}";
        }
    }


    public class MethodClearOneCircleWith : Method<SpecClearOneCircleWith>
    {
        public MethodClearOneCircleWith(SpecClearOneCircleWith prototype) : base(prototype)
        {
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            if (this.taskSpec.circleOptions.Count <= 0) yield break; // no circles to clear

            var activeFriendlies = this.taskSpec.friendlies.Get(context.state)
                .Where(friendly => friendly.CanFire)
                .ToList();

            if (activeFriendlies.Count <= 0) yield break; // no friendlies to do the job

            var enemyTargets = context.state.GetTeam(activeFriendlies[0].team.GetEnemyTeam())
                .Where(enemy => enemy.CanFire && enemy.IsWorthShooting())
                .ToList();

            foreach (var circle in this.taskSpec.circleOptions)
            {
                var specificCircleSpec = new SpecClearCircleWith
                {
                    circle = circle,
                    friendlies = this.taskSpec.friendlies,
                    occupyCircle = this.taskSpec.occupyCircle
                };

                if (specificCircleSpec.occupyCircle)
                {
                    yield return new Decomposition(Decomposition.ExecutionMode.Sequential, specificCircleSpec.Any());
                }
                else
                {
                    // Not occupying circle, so only give this circle a go if we think there are enemies there.
                    bool hasEnemies = enemyTargets
                        .Where(enemy => circle.IsInside(enemy.GetPositionObservedByEnemy()))
                        .Any(enemy => activeFriendlies.Any(f => f.CanDamage(enemy)));

                    if (hasEnemies)
                        yield return new Decomposition(Decomposition.ExecutionMode.Sequential,
                            specificCircleSpec.Any());
                }
            }
        }
    }

    public class MethodClearCirclesOfKnownEnemiesWith : Method<SpecClearCirclesWith>
    {
        public MethodClearCirclesOfKnownEnemiesWith(SpecClearCirclesWith prototype) : base(prototype)
        {
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            Method PerCircle(Circle circle)
            {
                return new SpecClearCircleWith
                {
                    circle = circle,
                    friendlies = this.taskSpec.friendlies,
                    occupyCircle = this.taskSpec.occupyCircle
                }.Any();
            }

            var d = new Decomposition(Decomposition.ExecutionMode.Sequential, this.taskSpec.circles.Select(PerCircle));
            yield return d;
        }
    }

    public class MethodClearCircleOfKnownEnemiesWithTogether : Method<SpecClearCircleWith>
    {
        public MethodClearCircleOfKnownEnemiesWithTogether(SpecClearCircleWith prototype) : base(prototype)
        {
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            var attackers = context.state
                .GetGroupMembers(this.taskSpec.friendlies)
                .Where(attacker => attacker.CanFire)
                .ToList();


            if (attackers.Count < 1) yield break; // no agents, so no task to accomplish.

            // Find agent closest to centroid
            var centroid = attackers.Select(u => u.positionActual).Centroid();
            var primary = attackers.ArgMin(u => u.DistanceSqr2dActual(centroid));

            var moveTo = new SpecMoveToPoint
            {
                movers = this.taskSpec.friendlies,
                destination = new Circle(primary.positionActual, 1)
            };

            var gatherThenGo = new Decomposition(Decomposition.ExecutionMode.Sequential,
                new MethodMoveToPointFast(moveTo),
                new MethodClearCircleOfKnownEnemiesWith(this.taskSpec)
            );

            yield return gatherThenGo;
        }
    }

    public class MethodClearCircleOfKnownEnemiesSmallerCircleFirstInterleave : Method<SpecClearCircleWith>
    {
        private static readonly Approach[] approaches =
        {
            new Approach { direction = Vector2.up, name = "FromNorth" },
            new Approach { direction = Vector2.down, name = "FromSouth" },
            new Approach { direction = Vector2.left, name = "FromEast" },
            new Approach { direction = Vector2.right, name = "FromWest" }
        };

        public MethodClearCircleOfKnownEnemiesSmallerCircleFirstInterleave(SpecClearCircleWith prototype) :
            base(prototype)
        {
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            if (this.taskSpec.friendlies.Count < 1) yield break; // no agents, so no task to accomplish.

            var enemyAgents = context.state
                .GetTeam(context.state.TeamEnemy)
                .Where(agent => agent.CanFire && this.taskSpec.circle.IsInside(agent.positionActual));

            foreach (var approach in approaches)
            {
                var approachCenter = this.taskSpec.circle.center + approach.direction * this.taskSpec.circle.kmRadius;

                var clearSmallerCircleTaskSpec = new SpecClearCircleWith
                {
                    circle = new Circle(approachCenter, this.taskSpec.circle.kmRadius / 4),
                    friendlies = this.taskSpec.friendlies,
                    occupyCircle = this.taskSpec.occupyCircle
                };

                var clearSmallerCircleThenClearBiggerCircle = new Decomposition(Decomposition.ExecutionMode.Sequential,
                    new MethodClearCircleOfKnownEnemiesWith(clearSmallerCircleTaskSpec)
                    {
                        notes = approach.name
                    },
                    new MethodClearCircleOfKnownEnemiesWith(this.taskSpec)
                );

                yield return clearSmallerCircleThenClearBiggerCircle;
            }
        }

        private struct Approach
        {
            public Vector2 direction;
            public string name;
        }
    }

    public class MethodClearCircleOfKnownEnemiesWith : Method<SpecClearCircleWith>
    {
        public MethodClearCircleOfKnownEnemiesWith(SpecClearCircleWith prototype) : base(prototype)
        {
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            yield break;
        }

        public override SimAction GetActionForSim(SimWorldState state)
        {
            var sequence = new ActionSequential();

            var clearAction = new ActionClearCircle(
                this.taskSpec.friendlies,
                state.teamFriendly,
                state.TeamEnemy,
                this.taskSpec.circle,
                this.taskSpec.occupyCircle
            );

            var move = new ActionMoveToWhileEngaging(this.taskSpec.friendlies)
            {
                destination = this.taskSpec.circle,
                targetTeam = clearAction.targetTeam,
                attackers = this.taskSpec.friendlies,
                kmEngagementWidth = this.taskSpec.circle.kmRadius / 2,
                name = "MethodClearCircleOfKnownEnemiesWith"
            };

            sequence.actionQueue.Add(move);
            sequence.actionQueue.Add(clearAction);
            return sequence;
        }
    }


    public class MethodClearCircleOfKnownEnemies : Method<SpecClearCircle>
    {
        public MethodClearCircleOfKnownEnemies(SpecClearCircle prototype) : base(prototype)
        {
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            var state = context.state;

            var childSpec = new SpecClearCircleWith
            {
                circle = this.taskSpec.circle,
                friendlies = state.GetTeam(this.taskSpec.friendlyTeam).ToSimGroup(),
                occupyCircle = this.taskSpec.occupyCircle
            };

            yield return new Decomposition(Decomposition.ExecutionMode.Sequential, childSpec.Any());
        }
    }
}