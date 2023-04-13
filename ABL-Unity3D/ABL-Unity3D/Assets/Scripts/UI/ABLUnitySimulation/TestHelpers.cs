using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using ABLUnitySimulation.Actions.Helpers;
using UnityEngine;
using Utilities.Unity;
using Random = System.Random;

#nullable enable

namespace UI.ABLUnitySimulation
{
    public static class TestHelpers
    {
        private static void AddAgents(int numAgents, Team team, SimWorldState state)
        {
            var teamName = team.ToString();

            for (var i = 0; i < numAgents; ++i)
            {
                var randomOffset = state.Random.NextVector2() - new Vector2(0.5f, 0.5f);

                var agent = new SimAgent(state, $"{teamName}#{i}")
                {
                    team = team,
                    positionActual = 10 * randomOffset
                };

                state.Add(agent);
            }
        }

        internal static ActionMoveToPositionWithPathfinding? CreateRandomMoveAction(Random random, SimAgent mover,
            List<SimAgent> targets, out SimAgent target)
        {
            target = targets.GetRandom(random);
            if (null == target) return null;

            var action =
                new ActionMoveToPositionWithPathfinding(mover,
                    new Circle(target.GetObservedPosition(mover.team), 0.1f));
            return action;
        }

        public static ActionMoveToPositionWithPathfinding? CreateRandomMoveAction(Random random, SimAgent mover,
            List<SimAgent> targets)
        {
            return CreateRandomMoveAction(random, mover, targets, out _);
        }
    }
}