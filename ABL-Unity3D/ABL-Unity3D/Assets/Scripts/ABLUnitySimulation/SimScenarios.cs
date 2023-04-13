using System.Collections.Generic;
using ABLUnitySimulation.Actions;
using ABLUnitySimulation.Actions.Helpers;
using UnityEngine;
using Utilities.GeneralCSharp;
using Random = System.Random;

#nullable enable


namespace ABLUnitySimulation
{
    /// <summary>
    ///     Generate and modify simulator world states using predefined methods.
    /// </summary>
    public static class SimScenarios
    {

        public static SimWorldState CreateTestSim(int numRed, Rect redRect, int numBlue, Rect blueRect,
            int? randomSeed = null)
        {
            var state = new SimWorldState(SimWorldState.PathProviderNavMesh)
            {
                Random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random()
            };
            
            AddSimAgents(state, Team.Red, redRect, numRed, 1);
            AddSimAgents(state, Team.Blue, blueRect, numBlue, 2.5f);
            
            return state;
        }

        public static void AddSimAgents(SimWorldState state, Team team, Rect area, int agentCount, float rangeMultiplier,
            bool useNonDefaultAgent = false)
        {
            for (var i = 0; i < agentCount; ++i)
            {
                var agent = SimAgent.Create($"{team}#{i + 1}", team, GetRandomSimPosition(state, area), rangeMultiplier,
                    !useNonDefaultAgent);
                if (useNonDefaultAgent)
                {
                    var increasedRangeEntityData = new SimEntityData(
                        "test",
                        SimAgentType.TypeC,
                        true,
                        100,
                        10,
                        4,
                        new SimWeaponSet(new List<SimWeapon> { SimWeapon.defaultWeapon3 })
                    );
                    agent.entities = new List<SimEntity> { new SimEntity(increasedRangeEntityData) };
                }

                state.Add(agent);
            }
        }

        public static Vector2 GetRandomSimPosition(SimWorldState state, Rect rect)
        {
            return new Vector2(
                state.Random.NextFloat(rect.xMin, rect.xMax),
                state.Random.NextFloat(rect.yMin, rect.yMax)
            );
        }
    }
}