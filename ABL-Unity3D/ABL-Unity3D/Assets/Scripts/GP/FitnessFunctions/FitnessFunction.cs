using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.SimScoringFunction;
using Cysharp.Threading.Tasks;
using GP.ExecutableNodeTypes;
using JetBrains.Annotations;

#nullable enable




namespace GP.FitnessFunctions
{
    public abstract class FitnessFunction
    {
        protected readonly SimScoringFunction simScoringFunction;
        protected readonly double simScoringFunctionWeight;

        protected FitnessFunction(SimScoringFunction simScoringFunction, double simScoringFunctionWeight)
        {
            this.simScoringFunction = simScoringFunction;
            this.simScoringFunctionWeight = simScoringFunctionWeight;
        }
    }

    public interface ISync
    {
        public abstract Fitness GetFitnessOfIndividual(GpRunner gp, Individual i);
    }

    public interface IAsync
    {
        public abstract UniTask<Fitness> GetFitnessOfIndividualAsync(GpRunner gp, Individual i);
    }

    public interface ICreatesSimStatesToEvaluate
    {
        public abstract IEnumerable<SimWorldState> CreateEvaluationWorldStates();
    }

    public interface IUsesASuppliedSimWorldState { }
    
    public interface IUsesPlanner { }
}