using ABLUnitySimulation;
using UnityEngine;

#nullable enable

namespace GP
{
    public static class WorldStateGenerationUtilityFunctions
    {
        public static SimWorldState GetFixedSimpleWorldState()
        {
            var simpleWorldState = new SimWorldState(SimWorldState.PathProviderNoObstacles);

            var friendly = new SimAgent(simpleWorldState, "Friendly")
            {
                team = Team.Red,
                positionActual = new Vector2(2, 2)
            };

            var enemy = new SimAgent(simpleWorldState, "Enemy")
            {
                team = Team.Blue,
                positionActual = Vector2.zero
            };

            simpleWorldState.Add(friendly);
            simpleWorldState.Add(enemy);

            return simpleWorldState;
        }
    }
}