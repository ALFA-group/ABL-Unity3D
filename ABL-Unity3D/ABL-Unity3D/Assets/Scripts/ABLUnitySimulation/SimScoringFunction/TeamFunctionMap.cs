using System.Collections.Generic;

#nullable enable

namespace ABLUnitySimulation.SimScoringFunction
{
    public abstract class TeamFunctionMap<TOutputType> : SimScoringCriterion
    {
        protected abstract TOutputType Function(IEnumerable<SimAgent> SimAgents);

        protected TOutputType GetResultOfFunctionMappedAcrossTeam(SimWorldState state, Team team)
        {
            return this.Function(state.GetTeam(team));
        }
    }
}