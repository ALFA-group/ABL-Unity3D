using System;
using UnityEngine;

namespace ABLUnitySimulation.SimScoringFunction.Average.DamageTaken
{
    public abstract class AverageDamageTaken : AverageFunction
    {
        protected override float PropertyToAverage(SimAgent agent)
        {
            if (agent.entities.Count < 1)
            {
                Debug.Assert(false, $"Found agent with no entities: {agent.Name}");
                return 0;
            }

            float maxHealth = 0;
            float damage = 0;
            foreach (var entity in agent.entities)
            {
                float entityMaxHealth = entity.MaxHealth;
                maxHealth += entityMaxHealth;
                damage += Math.Min(entity.damage, entityMaxHealth);
            }

            
            return damage / maxHealth;
        }
    }

    public class AverageDamageTakenEnemy : AverageDamageTaken
    {
        public override string Title => "EnemyAverageDamageTaken";

        public override double CalculateScore(SimWorldState state)
        {
            return this.GetResultOfFunctionMappedAcrossTeam(state, state.TeamEnemy);
        }
    }

    public class AverageDamageTakenFriendly : AverageDamageTaken
    {
        public override string Title => "FriendlyAverageDamageTaken";

        public override double CalculateScore(SimWorldState state)
        {
            return this.GetResultOfFunctionMappedAcrossTeam(state, state.teamFriendly);
        }
    }
}