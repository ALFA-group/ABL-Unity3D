using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.SimScoringFunction;

namespace GP.FitnessFunctions
{
    public abstract class AverageFitnessOverMultipleExecutionsSync : FitnessFunction, ICreatesSimStatesToEvaluate, ISync
    {
        protected AverageFitnessOverMultipleExecutionsSync(SimScoringFunction simScoringFunction,
            double simScoringFunctionWeight)
            : base(simScoringFunction, simScoringFunctionWeight)
        {
        }

        public abstract Fitness GetOneFitnessEvaluationOfIndividual(GpFieldsWrapper gpFieldsWrapper, Individual i);

        public abstract IEnumerable<SimWorldState> CreateEvaluationWorldStates();

        public Fitness GetFitnessOfIndividual(GpRunner gp, Individual i)
        {
            var fitness = new Fitness(0, 0);

            var fieldWrappers = this.CreateEvaluationWorldStates().Select(state => new GpFieldsWrapper(gp, state)).ToList();
            foreach (var fieldsWrapper in fieldWrappers)
            {
                var newFitness = this.GetOneFitnessEvaluationOfIndividual(fieldsWrapper, i);
                fitness = fitness.Add(newFitness);
            }

            // Average the fitness results
            fitness = fitness.Divide(fieldWrappers.Count);

            return fitness;
        }
    }
}