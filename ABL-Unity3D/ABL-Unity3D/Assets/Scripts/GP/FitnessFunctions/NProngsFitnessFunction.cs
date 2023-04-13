using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.SimScoringFunction;
using Cysharp.Threading.Tasks;
using GP.ExecutableNodeTypes;
using GP.ExecutableNodeTypes.GpPlanner;
using JetBrains.Annotations;

#nullable enable

namespace GP.FitnessFunctions
{
    
    
    public class NProngsGpOnlyFitnessFunction : FitnessFunction, ISync,
        IUsesASuppliedSimWorldState, IUsesPlanner
    {
        public NProngsGpOnlyFitnessFunction(SimScoringFunction simScoringFunction, double simScoringFunctionWeight) : base(simScoringFunction, simScoringFunctionWeight)
        {
        }

        public Fitness GetFitnessOfIndividual(GpRunner gp, Individual i)
        {
            var gpFieldsWrapper = new GpFieldsWrapper(gp);
            if (!(i.genome is NProngsSimAction node))
            {
                throw new Exception("Individual is not of type NProngsSimAction");
            }

            var action = node.Evaluate(gpFieldsWrapper);
            
            gpFieldsWrapper.worldState.actions.Add(action);

            gpFieldsWrapper.worldState.ExecuteUntil(state => 
                state.GetTeamHandles(gp.worldState.teamFriendly).All(f => !state.IsBusy(f)), 
                gp.plannerWrapper!.manyWorldsPlannerRunner.plannerParameters.pureData.secondsPerSimStep, 200000000, // we use the planner seconds per sim step make sure the 
                gp.timeoutInfo.cancelTokenSource.Token);
            
            return Fitness.MakeFitness(0,
                this.simScoringFunctionWeight,
                this.simScoringFunction,
                gpFieldsWrapper.worldState);
        }
    }
    
    [UsedImplicitly]
    public class NProngsFitnessFunction : 
        FitnessFunction, IAsync, 
        IUsesASuppliedSimWorldState, IUsesPlanner
    {
        public NProngsFitnessFunction(SimScoringFunction simScoringFunction, double simScoringFunctionWeight)
            : base(simScoringFunction, simScoringFunctionWeight)
        {
        }
        
        public async UniTask<Fitness> GetFitnessOfIndividualAsync(GpRunner gp, Individual i)
        {
            var genome = i.genome as NProngsPlan ?? 
                         throw new Exception($"Genome is not of type {nameof(NProngsPlan)}");
            var plan = await genome.Evaluate(new GpFieldsWrapper(gp));
            var endState = plan.endState ?? throw new Exception("The end state for the generated plan is not defined");
            var simScore = this.simScoringFunction.EvaluateSimWorldState(endState);
            return new Fitness(0, this.simScoringFunctionWeight * simScore);
        }
    }
    
}