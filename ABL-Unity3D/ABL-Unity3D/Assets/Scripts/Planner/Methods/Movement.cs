using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using ABLUnitySimulation.Actions.Helpers;
using Sirenix.Serialization;
using UnityEngine;

#nullable enable

namespace Planner.Methods
{
    public struct SpecMoveToPoint : ITaskSpec
    {
        public Circle destination;
        [OdinSerialize]
        public SimGroup movers;

        public override string ToString()
        {
            return $"{this.movers} to {this.destination}";
        }
    }

    public struct SpecMoveToPointUntilNearEnemy : ITaskSpec
    {
        public Circle destination;
        public SimGroup? movers;
        public float kmInterruptionDistance;
        public List<Circle>? ignoreEnemiesInTheseCircles;

        public override string ToString()
        {
            return $"{this.movers} to {this.destination}";
        }

        public static bool IsEnemyNear(IEnumerable<Vector2> friendlyPositions, IEnumerable<Vector2> enemyPositions,
            float kmNearDistance, List<Circle>? ignoreEnemiesInTheseCircles)
        {
            List<Vector2>? filteredEnemyPositions = null;

            foreach (var friendlyPosition in friendlyPositions)
            {
                filteredEnemyPositions ??= enemyPositions
                    .Where(enemyPosition => ignoreEnemiesInTheseCircles != null &&
                                            ignoreEnemiesInTheseCircles.Any(circle => circle.IsInside(enemyPosition)))
                    .ToList();

                if (filteredEnemyPositions.Any(enemyPosition =>
                        (friendlyPosition - enemyPosition).sqrMagnitude < kmNearDistance * kmNearDistance)) return true;
            }

            return false;
        }
    }


    public struct SpecInRangeVector2 : ITaskSpec
    {
        public Vector2 target;
        public SimGroup movers;

        public override string ToString()
        {
            return $"{this.movers} targeting {this.target}";
        }
    }

    public struct SpecInRangeSimAgent : ITaskSpec
    {
        public Handle<SimAgent> mover;
        public Handle<SimAgent> target;
    }

    public class MethodMoveToPointFast : Method<SpecMoveToPoint>
    {
        public MethodMoveToPointFast(SpecMoveToPoint prototype) : base(prototype)
        {
        }

        public MethodMoveToPointFast(Vector2 destination, float kmCloseEnough, SimGroup movers) :
            this(new SpecMoveToPoint { destination = new Circle(destination, kmCloseEnough), movers = movers })
        {
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            yield break; // operator!
        }

        public override SimAction? GetActionForSim(SimWorldState state)
        {
            if (this.taskSpec.movers.Count < 1) return null;

            var firstGuy = state.GetGroupMembers(this.taskSpec.movers).FirstOrDefault(su => su.CanMove);

            if (null == firstGuy) return null;

            return new ActionMoveToPositionWithPathfinding(this.taskSpec.movers, this.taskSpec.destination);
        }
    }

    /*
    public class MoveToPointTogether : Method<SpecMoveToPoint>
    {
        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            var moverHandles = this.taskSpec.movers;
            if (moverHandles.Count <= 1) yield break; // Don't move together if solo!

            var state = context.state;
            var movers = moverHandles.Get(state).Where(m => m.IsActive).ToList();
            if (!movers.Any()) yield break;
            
            var centroid = movers.Select(m => m.position).Centroid();

            var rendezvous = new SpecMoveToPoint()
            {
                destination = centroid,
                movers = new SimGroup(movers)
            };

            var destination = this.taskSpec;
            destination.movers = rendezvous.movers;
            
            Decomposition d = new Decomposition(
                new MoveToPointFast(rendezvous), 
                new MoveToPointFast(destination));

            yield return d;
        }

        public MoveToPointTogether(SpecMoveToPoint prototype) : base(prototype)
        {
        }
    }
    */

    public class MethodGetInRangeDirect : Method<SpecInRangeVector2>
    {
        public MethodGetInRangeDirect(SpecInRangeVector2 prototype) : base(prototype)
        {
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            var d = new Decomposition(Decomposition.ExecutionMode.Sequential);

            var state = context.state;
            var movers = this.taskSpec.movers.Get(state); //state.GetGroupMembers(this.taskSpec.movers);

            foreach (var mover in movers)
            {
                var toMoverFromTarget = mover.positionActual - this.taskSpec.target;
                float currentDistance = toMoverFromTarget.magnitude;

                float kmMaxRange = mover.KmMaxRange();
                if (currentDistance > kmMaxRange)
                {
                    float scale = kmMaxRange / currentDistance;
                    var justInRange = toMoverFromTarget * scale;
                    d.subtasks.Add(new MethodMoveToPointFast(this.taskSpec.target + justInRange,
                        mover.KmSmallestMaxRange(), mover));
                }
            }

            if (d.subtasks.Count > 0) yield return d;
        }
    }

    public class MethodMoveAlongPathUntilSeeingEnemy : Method<SpecMoveToPointUntilNearEnemy>
    {
        public MethodMoveAlongPathUntilSeeingEnemy(SpecMoveToPointUntilNearEnemy prototype) : base(prototype)
        {
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            yield break; // operator!
        }

        public override SimAction? GetActionForSim(SimWorldState state)
        {
            if (null == this.taskSpec.movers) return null;
            if (this.taskSpec.movers.Count < 1) return null;

            var firstGuy = state.GetGroupMembers(this.taskSpec.movers).FirstOrDefault(su => su.IsActive);

            if (null == firstGuy) return null;

            // Avoid closure
            float kmMaxEnemyDistance = this.taskSpec.kmInterruptionDistance;
            var excludedCircles = this.taskSpec.ignoreEnemiesInTheseCircles;

            var move = new ActionMoveToPositionWithPathfinding(this.taskSpec.movers, firstGuy.positionActual,
                this.taskSpec.destination.center,
                this.taskSpec.destination.kmRadius)
            {
                name = $"MoveAlongPath until enemy within {kmMaxEnemyDistance}km",
                shouldCompleteBeforeFinishing = (worldState, moveAction) =>
                    IsEnemyNear(worldState, moveAction.actors, kmMaxEnemyDistance, excludedCircles)
            };

            return move;
        }

        public static bool IsEnemyNear(SimWorldState state, SimGroup friendlies, float kmNearDistance,
            List<Circle>? ignoreEnemiesInTheseCircles)
        {
            List<SimAgent>? enemies = null;

            foreach (var friendlySimAgent in friendlies.Get(state))
            {
                var myTeam = friendlySimAgent.team;

                if (null == enemies)
                    enemies = state.GetTeamEnemies(friendlySimAgent)
                        .Where(enemy => ignoreEnemiesInTheseCircles == null
                                        || !ignoreEnemiesInTheseCircles.Any(circle =>
                                            circle.IsInside(enemy.GetObservedPosition(myTeam))))
                        .ToList();

                if (enemies.Any(enemy => friendlySimAgent.IsNearAccordingToMe(enemy, kmNearDistance))) return true;
            }

            return false;
        }
    }


    // public class MethodGetInRangeGpEvolved : Method<InRangeVector3>
    // {
    //     public MethodGetInRangeGpEvolved(InRangeVector3 prototype) : base(prototype)
    //     {
    //     }
    //
    //     public override IEnumerable<Decomposition> Decompose(PlannerContext context)
    //     {
    //         if (context.gpEvolvedMethodFiles.TryGetValue(typeof(InRangeVector3), out string methodFile))
    //         {
    //             throw new Exception("No method for GetInRange found");
    //         }
    //         Decomposition d = new Decomposition();
    //
    //         GP.Node methodImplementation = GpRunnerHelper.LoadTreeFromFile(methodFile);
    //
    //         var nodes = methodImplementation.IterateNodes().ToList();
    //         
    //         
    //         var args = new PositionalArguments();
    //         args.positionalArguments.Add(
    //             (Handle<SimAgent>)context.state.GetAll<SimAgent>()
    //                 .First(u => u.team == Team.Red));
    //         var gpWrapper =
    //             new GpFieldsWrapperForExecutingTrees(context.state, Team.Red, Team.Blue, args);
    //
    //         // foreach (var node in nodes)
    //         for (int i = 0; i < nodes.Count(); i++)
    //         {
    //             var node = nodes[i];
    //             if (node.GetType() == typeof(ABLUnityActionTypes.MoveTo))
    //             {
    //                 var target = (GP.ExecutableNodeTypes.ExecutableNode<Vector3>) node.children[0];
    //                 var friendly = (GP.ExecutableNodeTypes.ExecutableNode<SimAgent>) node.children[1];
    //                 d.subtasks.Add(new MoveToPointFast(target.Execute(gpWrapper), friendly.Execute(gpWrapper)));
    //                 i += 2;
    //             }
    //         }
    //         if (d.subtasks.Count > 0) yield return d;
    //         
    //         //
    //         // foreach (var mover in movers)
    //         // {
    //         //     var toMoverFromTarget = mover.position - this.taskSpec.target;
    //         //     var currentDistance = toMoverFromTarget.magnitude;
    //         //     if (currentDistance > mover.weapon.kmMaxRange)
    //         //     {
    //         //         var scale = mover.weapon.kmMaxRange / currentDistance;
    //         //         var justInRange = toMoverFromTarget * scale;
    //         //         d.subtasks.Add( new MoveToPointFast(this.taskSpec.target + justInRange, mover));
    //         //     }
    //         // }
    //         //
    //         // if (d.subtasks.Count > 0) yield return d;
    //     }
    // }
}