namespace ABLUnitySimulation.SimScoringFunction.Average.Health
{
    public abstract class AverageTeamHealth : AverageFunction
    {
        protected override float PropertyToAverage(SimAgent agent)
        {
            float maxHealth = 0;
            float health = 0;
            foreach (var entity in agent.entities)
            {
                maxHealth += entity.MaxHealth;
                health += entity.CurrentHealth;
            }

            
            return health / maxHealth;
        }
    }

    public class AverageTeamHealthEnemy : AverageTeamHealth
    {
        public override string Title => "EnemyAverageTeamHealth";

        public override double CalculateScore(SimWorldState state)
        {
            return this.GetResultOfFunctionMappedAcrossTeam(state, state.TeamEnemy);
        }
    }

    public class AverageTeamHealthFriendly : AverageTeamHealth
    {
        public override string Title => "FriendlyAverageTeamHealth";

        public override double CalculateScore(SimWorldState state)
        {
            return this.GetResultOfFunctionMappedAcrossTeam(state, state.teamFriendly);
        }
    }
}