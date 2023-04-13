using System;
using System.Collections.Generic;
using System.Threading;
using ABLUnitySimulation;
using GP.Experiments;
using UI.ABLUnitySimulation;

#nullable enable

namespace GP
{
    /// <summary>
    /// Wrapper to hold relevant GP fields for genome and fitness evaluation.
    /// </summary>
    public class GpFieldsWrapper
    {
        public readonly TimeoutInfo timeoutInfo;
        public readonly PlannerWrapper? plannerWrapper;
        public readonly GpPopulationParameters populationParameters;
        public readonly PositionalArguments? positionalArguments;
        public readonly Random rand;
        public readonly SimWorldState worldState;
        public readonly SimEvaluationParameters? simEvaluationParameters;
        public readonly bool verbose;

        public GpFieldsWrapper(GpRunner gp) : this(gp, gp.worldState) // throw exception if world state is null
        {
        }

        public GpFieldsWrapper(GpRunner gp, SimWorldState worldState)
        {
            this.rand = gp.rand;
            this.timeoutInfo = gp.timeoutInfo;
            this.worldState = worldState.DeepCopy();
            this.populationParameters = gp.populationParameters;
            this.positionalArguments = gp.positionalArguments;
            this.plannerWrapper = gp.plannerWrapper;
            this.simEvaluationParameters = gp.simEvaluationParameters;
            this.verbose = gp.verbose;
        }

    }
}