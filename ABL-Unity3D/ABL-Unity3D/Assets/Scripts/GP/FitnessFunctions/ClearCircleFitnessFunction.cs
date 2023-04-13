using System.Collections.Generic;
using ABLUnitySimulation;
using ABLUnitySimulation.SimScoringFunction;
using UnityEngine;

namespace GP.FitnessFunctions
{
    public class ClearCircleFitnessFunction : FitnessFunction, ICreatesSimStatesToEvaluate, ISync
    {
        public ClearCircleFitnessFunction(
            SimScoringFunction simScoringFunction,
            double simScoringFunctionWeight)
            : base(simScoringFunction, simScoringFunctionWeight)
        {
        }

        public Fitness GetFitnessOfIndividual(GpRunner gp, Individual i)
        {
            return new Fitness(0, 0);
        }

        public IEnumerable<SimWorldState> CreateEvaluationWorldStates()
        {
            var state = new SimWorldState(SimWorldState.PathProviderNoObstacles) { teamFriendly = Team.Red };
            // Friendlies
            var test = new SimAgent(state, "test");
            var test1 = new SimAgent(state, "test1");
            var test2 = new SimAgent(state, "test2");
            var test6 = new SimAgent(state, "test6");
            var test7 = new SimAgent(state, "test7");
            var test8 = new SimAgent(state, "test8");
            test.team = state.teamFriendly;
            test1.team = state.teamFriendly;
            test2.team = state.teamFriendly;
            test6.team = state.teamFriendly;
            test7.team = state.teamFriendly;
            test8.team = state.teamFriendly;
            test.positionActual = Vector2.zero;
            test1.positionActual = new Vector2(0.5f, 0.5f);
            test2.positionActual = new Vector2(1f, 1f);
            test6.positionActual = new Vector2(-1.5f, -1.5f);
            test7.positionActual = new Vector2(-0.5f, -0.5f);
            test8.positionActual = new Vector2(-1f, -1f);

            // Enemies
            var test3 = new SimAgent(state, "test3");
            var test4 = new SimAgent(state, "test4");
            var test5 = new SimAgent(state, "test5");
            test3.team = state.TeamEnemy;
            test4.team = state.TeamEnemy;
            test5.team = state.TeamEnemy;
            test3.positionActual = new Vector2(5, 5);
            test4.positionActual = new Vector2(7, 7);
            test5.positionActual = new Vector2(9, 9);

            state.Add(new[]
            {
                test,
                test1,
                test2,
                test3,
                test4,
                test5,
                test6,
                test7,
                test8
            });
            yield return state;
        }
    }
}