using System.Collections.Generic;
using System.Linq;

namespace ABLUnitySimulation.SimScoringFunction.Average
{
    public abstract class AverageFunction : TeamFunctionMap<float>
    {
        protected abstract float PropertyToAverage(SimAgent agent);

        protected override float Function(IEnumerable<SimAgent> SimAgents)
        {
            if (!SimAgents.Any()) return 0;
            return SimAgents.Average(this.PropertyToAverage);
        }
    }
}