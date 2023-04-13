using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using ABLUnitySimulation.Actions.Helpers;

namespace Planner.Methods
{
    public struct SpecAttack : ITaskSpec
    {
        public Handle<SimAgent> target;
        public SimGroup attackers;

        public override string ToString()
        {
            return $"{this.attackers} targeting {this.target}";
        }
    }

    public class MethodAttackToDestroy : Method<SpecAttack>
    {
        public MethodAttackToDestroy(SpecAttack prototype) : base(prototype)
        {
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            yield break;
        }

        public override SimAction GetActionForSim(SimWorldState state)
        {
            // Is the target already dead?
            var target = this.taskSpec.target.GetCanFail(state);
            if (null == target) return null;
            if (!target.IsWorthShooting()) return null;

            return new ActionAttackToDestroy
            {
                actors = this.taskSpec.attackers,
                target = this.taskSpec.target
            };
        }

        public class MethodMoveToAndDestroy : Method<SpecAttack>
        {
            public MethodMoveToAndDestroy(SpecAttack prototype) : base(prototype)
            {
            }

            public override IEnumerable<Decomposition> Decompose(PlannerContext context)
            {
                var attackers = context.state
                    .GetGroupMembers(this.taskSpec.attackers)
                    .Where(attacker => attacker.CanFire)
                    .ToList();

                if (attackers.Count <= 0) yield break;

                var target = context.state.Get(this.taskSpec.target);
                if (!target.IsWorthShooting()) yield break;

                float range = attackers.Min(attacker => attacker.KmMaxRange());

                var specGetClose = new SpecMoveToPoint
                {
                    destination = new Circle(target.GetObservedPosition(attackers[0]), range * 0.9f),
                    movers = this.taskSpec.attackers
                };

                var d = new Decomposition(
                    Decomposition.ExecutionMode.Sequential,
                    specGetClose.Any(), // we have freedom in how we get close
                    new MethodAttackToDestroy(this.taskSpec) // but always perform a simple attack when we get there
                );

                yield return d;
            }
        }
    }
}