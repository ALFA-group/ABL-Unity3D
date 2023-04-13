using System.Collections.Generic;

namespace ABLUnitySimulation.SimScoringFunction.Average.Health
{
    public abstract class AverageEntityHealth : SimScoringCriterion
    {
        public override double CalculateScore(SimWorldState state)
        {
            var healthSummary = this.GetEntityHealthSummaries(state);

            double maxHealthSum = 0;
            double currentHealthSum = 0;
            foreach ((float currentHealth, float maxHealth) in healthSummary)
            {
                maxHealthSum += maxHealth;
                currentHealthSum += currentHealth;
            }

            if (maxHealthSum > 0) return currentHealthSum / maxHealthSum;
            return 0;
        }

        protected abstract IEnumerable<(float CurrentHealth, float MaxHealth)> GetEntityHealthSummaries(
            SimWorldState state);
    }
}