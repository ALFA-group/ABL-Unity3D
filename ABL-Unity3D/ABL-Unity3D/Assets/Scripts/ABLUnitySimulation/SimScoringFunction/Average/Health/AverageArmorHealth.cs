using System.Collections.Generic;
using System.Linq;

namespace ABLUnitySimulation.SimScoringFunction.Average.Health
{
    public class AverageArmorHealthFriendly : AverageEntityHealth
    {
        public override string Title => nameof(AverageArmorHealthFriendly);

        protected override IEnumerable<(float CurrentHealth, float MaxHealth)> GetEntityHealthSummaries(
            SimWorldState state)
        {
            var healthSummary = state.GetTeam(state.teamFriendly)
                .SelectMany(agent => agent.entities)
                .Where(entity => entity.data.simAgentType == SimAgentType.TypeC)
                .Select(entity => (entity.CurrentHealth, entity.MaxHealth));
            return healthSummary;
        }
    }

    public class AverageArmorHealthEnemy : AverageEntityHealth
    {
        public override string Title => nameof(AverageArmorHealthEnemy);

        protected override IEnumerable<(float CurrentHealth, float MaxHealth)> GetEntityHealthSummaries(
            SimWorldState state)
        {
            var healthSummary = state.GetTeam(state.TeamEnemy)
                .SelectMany(agent => agent.entities)
                .Where(entity => entity.data.simAgentType == SimAgentType.TypeC)
                .Select(entity => (entity.CurrentHealth, entity.MaxHealth));
            return healthSummary;
        }
    }
}