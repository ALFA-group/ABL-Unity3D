using System.Collections.Generic;
using ABLUnitySimulation;
using ABLUnitySimulation.SimScoringFunction;
using JetBrains.Annotations;
using Utilities.GeneralCSharp;

namespace GP.FitnessFunctions
{
    public class TestFitnessFunction : FitnessFunction, ICreatesSimStatesToEvaluate, ISync
    {
        
        [UsedImplicitly]
        public TestFitnessFunction(SimScoringFunction placeHolderFunction, double placeHolderWeight) :
            base(new SimScoringFunction(new List<SimScoringCriterionAndWeight>()), 0) 
        {
        }

        public Fitness GetFitnessOfIndividual(GpRunner gp, Individual i)
        {
            return new Fitness(0, 0);
        }

        public IEnumerable<SimWorldState> CreateEvaluationWorldStates()
        {
            return WorldStateGenerationUtilityFunctions.GetFixedSimpleWorldState().ToEnumerable();
        }
    }
}