using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.SimScoringFunction;
using ABLUnitySimulation.SimScoringFunction.Average.Health;
using NUnit.Framework;
using Utilities.GeneralCSharp;

#nullable enable

namespace Tests.Editor.Sim
{
    public class TestScoreArmor
    {
        private SimEntity MakeEntity(int count, SimAgentType simAgentType)
        {
            var armorData = new SimEntityData("??", simAgentType);
            var armor = new SimEntity(armorData, count);
            return armor;
        }

        private SimScoringFunction MakeArmorScoringFunction(bool friendly)
        {
            AverageEntityHealth criterion = new AverageArmorHealthEnemy();
            if (friendly) criterion = new AverageArmorHealthFriendly();

            var andWeight = new SimScoringCriterionAndWeight(1, criterion, false);

            var scoring = new SimScoringFunction(andWeight.ToEnumerable().ToList());
            return scoring;
        }

        [Test]
        public void TestOneEntity([Values(0, 0.5f, 1f)] float health)
        {
            var state = new SimWorldState(SimWorldState.PathProviderNoObstacles);
            var agent = new SimAgent(state, "solo", false)
            {
                team = state.teamFriendly
            };
            state.Add(agent);

            var entity = this.MakeEntity(1, SimAgentType.TypeC);
            entity.damage = 1 - health;
            agent.entities.Add(entity);

            var scoring = this.MakeArmorScoringFunction(true);
            Assert.That(health, Is.EqualTo(scoring.EvaluateSimWorldState(state)).Within(0.1).Percent);
        }

        [Test]
        public void TestMixedPlatoon([Values(0, 0.5f, 1f)] float health)
        {
            var state = new SimWorldState(SimWorldState.PathProviderNoObstacles);

            for (var i = 0; i < 4; ++i)
            {
                var agent = new SimAgent(state, "mixed", false)
                {
                    team = state.teamFriendly
                };
                state.Add(agent);

                var entityArmor = this.MakeEntity(1, SimAgentType.TypeC);
                entityArmor.damage = 1 - health;
                agent.entities.Add(entityArmor);

                var entityHuman = this.MakeEntity(46, SimAgentType.TypeA);
                entityHuman.damage = 0.1f;
                agent.entities.Add(entityHuman);

                var entityVehicle = this.MakeEntity(46, SimAgentType.TypeB);
                entityVehicle.damage = 0.1f;
                agent.entities.Add(entityVehicle);
            }

            var scoring = this.MakeArmorScoringFunction(true);
            Assert.That(health, Is.EqualTo(scoring.EvaluateSimWorldState(state)).Within(0.1).Percent);
        }

        [Test]
        public void TestAveraging()
        {
            var state = new SimWorldState(SimWorldState.PathProviderNoObstacles);

            float[] healths = new[] { 1, 0, .1f, .2f, .3f, .4f, .5f };

            foreach (float health in healths)
            {
                var agent = new SimAgent(state, "mixed", false)
                {
                    team = state.teamFriendly
                };
                state.Add(agent);

                var entityArmor = this.MakeEntity(1, SimAgentType.TypeC);
                entityArmor.damage = 1 - health;
                agent.entities.Add(entityArmor);

                var entityHuman = this.MakeEntity(46, SimAgentType.TypeA);
                entityHuman.damage = 0.1f;
                agent.entities.Add(entityHuman);

                var entityVehicle = this.MakeEntity(46, SimAgentType.TypeB);
                entityVehicle.damage = 0.1f;
                agent.entities.Add(entityVehicle);
            }

            var scoring = this.MakeArmorScoringFunction(true);
            Assert.That(healths.Average(), Is.EqualTo(scoring.EvaluateSimWorldState(state)).Within(0.1).Percent);
        }
    }
}