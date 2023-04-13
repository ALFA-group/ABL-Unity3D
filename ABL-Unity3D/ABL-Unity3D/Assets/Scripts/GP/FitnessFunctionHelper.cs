using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.SimScoringFunction;
using GP.FitnessFunctions;
using JetBrains.Annotations;

#nullable enable

namespace GP
{
    public static class FitnessFunctionHelper
    {
        public static FitnessFunction ConstructAndVerify(Type fitnessFunctionType, SimScoringFunction simScoringFunction,
            int simScoringFunctionWeight)
        {
            var fitnessFunctionConstructorArgs = new List<object>
            {
                simScoringFunction, 
                simScoringFunctionWeight
            };
            
            var fitnessFunction = (FitnessFunction)Activator.CreateInstance(
                fitnessFunctionType,
                fitnessFunctionConstructorArgs.ToArray());

            // Check if the fitness function defines whether it is synchronous xor asynchronous 
            if (!(fitnessFunction is IAsync ^ 
                  fitnessFunction is ISync))
            {
                throw new Exception(
                    "The given fitness function does not implement xor " +
                    "ISync or IAsync");
            }
            
            // Check if the fitness function defines whether it uses a supplied world state xor creates its own sim state
            if (!(fitnessFunction is IUsesASuppliedSimWorldState ^ 
                  fitnessFunction is ICreatesSimStatesToEvaluate))
            {
                throw new Exception(
                    "The given fitness function type does not implement " +
                    "ICreatesSimStatesToEvaluate xor " +
                    "IUsesASuppliedSimWorldState");
            }

            return fitnessFunction;
        }
        
        public static SimWorldState GetCorrectSimWorldStateGivenAFitnessFunction(
            FitnessFunction fitnessFunctionToMaybeGetSimWorldStateFrom,
            SimWorldState? simWorldStateToMaybeUseForFitnessEvaluation)
        {
            return fitnessFunctionToMaybeGetSimWorldStateFrom switch
            {
                IUsesASuppliedSimWorldState _ =>
                    simWorldStateToMaybeUseForFitnessEvaluation ?? throw new Exception(
                        $"{nameof(simWorldStateToMaybeUseForFitnessEvaluation)} cannot be null"),
                ICreatesSimStatesToEvaluate fitnessFunctionWhichCreatesSimStatesToEvaluate =>
                    // Doesn't matter which world state is chosen,
                    // as long as it is similar to the state required for fitness evaluations
                    fitnessFunctionWhichCreatesSimStatesToEvaluate.CreateEvaluationWorldStates().First().DeepCopy(),
                _ => 
                    throw new ArgumentOutOfRangeException(
                        nameof(fitnessFunctionToMaybeGetSimWorldStateFrom), 
                        fitnessFunctionToMaybeGetSimWorldStateFrom, 
                        "The given fitness function type does not implement " +
                        "ICreatesSimStatesToEvaluate xor " +
                        "IUsesASuppliedSimWorldState")
            };
        }
        
    }
}