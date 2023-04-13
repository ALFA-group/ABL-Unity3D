using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ABLUnitySimulation;
using Cysharp.Threading.Tasks;
using GP.Experiments;
using GP.FitnessFunctions;
using Planner.ManyWorlds;
using Sirenix.Utilities;
using UI.Planner;
using UnityEngine;
using Utilities.GeneralCSharp;
using Utilities.GP;
using Utilities.Unity;
using Random = System.Random;

#nullable enable

namespace GP
{
    public struct TimeoutInfo
    {
        public int timeLimitInSeconds;
        public DateTime runStartTime; 
        public bool ignoreGenerationsUseTimeout;
        public CancellationTokenSource cancelTokenSource;

        public bool ShouldTimeout =>
            (this.ignoreGenerationsUseTimeout && GenericUtilities.SecondsElapsedSince(this.runStartTime) > this.timeLimitInSeconds) ||
            (this.cancelTokenSource.IsCancellationRequested);
    }

    public class PlannerWrapper
    {
        public readonly ManyWorldsPlanner planner;
        
        public readonly ManyWorldsPlannerRunner manyWorldsPlannerRunner;

        public PlannerWrapper(ManyWorldsPlanner planner, ManyWorldsPlannerRunner manyWorldsPlannerRunner)
        {
            this.planner = planner;
            this.manyWorldsPlannerRunner =
                manyWorldsPlannerRunner; 
        }

        
        public static PlannerWrapper? GetPlannerWrapperBasedOnFitnessFunctionAndManyWorldsPlannerRunner(
            FitnessFunction fitnessFunction, 
            ManyWorldsPlannerRunner manyWorldsPlannerRunner)
        {
                        
            if (fitnessFunction is IUsesPlanner)
            {
                if (null == manyWorldsPlannerRunner)
                {
                    throw new Exception($"nameof{manyWorldsPlannerRunner} can not be null if the " +
                                        $"given fitness function implements IUsesPlanner");
                }
                
                var planner = new ManyWorldsPlanner(manyWorldsPlannerRunner.plannerParameters.pureData.multiThread)
                {
                    secondsPerSimStep = manyWorldsPlannerRunner.plannerParameters.pureData.secondsPerSimStep,
                    sMaxActionExecuteTime = manyWorldsPlannerRunner.plannerParameters.pureData.maxExecutionTimeInSeconds
                };
                return new PlannerWrapper(planner, manyWorldsPlannerRunner);
            }

            return null;
        }
    }

    
    // The only state it maintains right now is verbose info
    // Maybe it's just better to change it to a static class?
    public partial class GpRunner
    {
        public readonly FitnessFunction fitnessFunction;
        public readonly PlannerWrapper? plannerWrapper;
        public readonly GpPopulationParameters populationParameters;
        public readonly PositionalArguments? positionalArguments;
        public readonly Random rand;
        public readonly int? randomSeed;
        public readonly Type solutionReturnType;
        public readonly VerboseInfo verbose;
        public readonly SimWorldState worldState;
        public readonly TimeoutInfo timeoutInfo;
        public readonly SimEvaluationParameters? simEvaluationParameters;
        private readonly Dictionary<Node, Fitness> _individualCache = new Dictionary<Node, Fitness>(new NodeComparer());
        // private HashSet<Individual> _nodesAlreadyGenerated = new HashSet<Individual>(new IndividualComparer());
        private readonly MethodInfo _treeGenerator;
        private readonly bool _filterForUniqueIndividuals = false; 

        public GpRunner(
            FitnessFunction fitnessFunction,
            GpPopulationParameters populationParameters,
            Type solutionReturnType,
            SimWorldState simWorldState,
            TimeoutInfo timeoutInfo,
            SimEvaluationParameters? simEvaluationParameters = null,
            PlannerWrapper? plannerWrapper = null,
            PositionalArguments? positionalArguments = null,
            int? randomSeed = null,
            bool verbose = false)
        {
            this.fitnessFunction = fitnessFunction;
            this.worldState = simWorldState;
            this.positionalArguments = positionalArguments;
            this.solutionReturnType = solutionReturnType;
            this.simEvaluationParameters = simEvaluationParameters;
            this.timeoutInfo = timeoutInfo;
            this.populationParameters = populationParameters;
            this.randomSeed = randomSeed;
            this.verbose = verbose;
            this.rand = this.randomSeed != null ? new Random(this.randomSeed.Value) : new Random();
            this.plannerWrapper = plannerWrapper;

            // NOTE: Probability distribution must be defined before the helper
            this.SetMinTreeDictionariesForSatisfiableNodeTypes();

            this._treeGenerator = this.GetType()
                .GetMethod("GenerateRandomTreeFromTypeOrReturnType")
                ?.MakeGenericMethod(this.solutionReturnType) 
                                  ?? throw new Exception("Method not found");
        }

        public async UniTask<GeneratedPopulations> RunAsync(GpExperimentProgress progress, bool multithread)
        {
            
            this.verbose.ResetCountInfo(); 
            
            progress.generationsInRunCount = this.populationParameters.numberGenerations;
            progress.generationsInRunCompleted = 0;
            progress.status = "Init population";

            var initializationMethodType = this.populationParameters.populationInitializationMethod.GetType();
            var initializationMethodInfo =
                initializationMethodType.GetMethod("GetPopulation") ??
                throw new Exception($"GetPopulation is not defined in the class {initializationMethodType.Name}");

            var population = await (Task<List<Individual>>)initializationMethodInfo
                .MakeGenericMethod(this.solutionReturnType)
                .Invoke(
                    this.populationParameters.populationInitializationMethod,
                    new object[] { this, timeoutInfo });

            // this._nodesAlreadyGenerated.AddRange(population);

            if (this.timeoutInfo.ShouldTimeout)
                return new GeneratedPopulations(
                    new[] { population }.ToNestedList(),
                    FitnessStats.GetDetailedSummary(population),
                    timeoutInfo.runStartTime,
                    DateTime.Now, 
                    population.SortedByFitness().FirstOrDefault(),
                    this.verbose
                );
            progress.status = "Evolving";
            var allPopulationsFromRun = await this.SearchLoopAsync(population, progress ,multithread);
            
            return allPopulationsFromRun;
        }

        private async UniTask<GeneratedPopulations> SearchLoopAsync(
            List<Individual> population,
            GpExperimentProgress progress, bool multithread)
        {
            // The caller is required to already have evaluated the given population
            Debug.Assert(population.All(i => null != i.fitness));

            if (population.Any(IsInDenyList)) throw new Exception("Some individuals in population are in deny list.");

            progress.generationsInRunCount = this.populationParameters.numberGenerations;
            progress.generationsInRunCompleted = 0;

            var allPopulations = new List<List<Individual>>();

            // var startTimeSearchLoop = DateTime.Now;

            if (multithread) await UniTaskUtility.SwitchToThread();

            population = population.SortedByFitness();

            allPopulations.Add(population);

            Individual bestEver = population.FirstOrDefault() ??
                           throw new Exception("Population is empty");
            
            
            var nullFitness = population.Where(i => null == i.fitness);
            foreach (var i in nullFitness)
            {
                await this.EvaluateFitnessOfIndividual(i);
            }
            
            var allFitnessValues = population.Select(i => i.fitness!).ToList();

            for (var generationIndex = 0;
                 timeoutInfo.ignoreGenerationsUseTimeout ||
                 generationIndex < this.populationParameters.numberGenerations;
                 generationIndex++)
            {
                population = await this.GenerateNewPopulation(population);
                if (this._filterForUniqueIndividuals)
                {
                    var uniqueIndividuals = population.Distinct(new IndividualComparer()).ToList();
                    
                    int maxRetries = 20;
                    int retriesSoFar = 0;
                    while (uniqueIndividuals.Count < population.Count && !this.timeoutInfo.ShouldTimeout)
                    {
                        Debug.Log("Found duplicates. Generating new individuals.");
                        
                        var randomNode = (Node)this._treeGenerator
                                             .Invoke(this, new object[] { this.populationParameters.maxDepth, false }) ??
                                         throw new Exception("No tree generated");

                        var randomNodeIndividual = new Individual(randomNode);
                        if (!uniqueIndividuals.Contains(randomNodeIndividual, new IndividualComparer()) && retriesSoFar < maxRetries && !this.timeoutInfo.ShouldTimeout)
                        {
                            await this.EvaluateFitnessOfIndividual(randomNodeIndividual);
                            uniqueIndividuals.Add(randomNodeIndividual);
                        }

                        retriesSoFar++;
                    }

                    population = uniqueIndividuals;
                }



                population = population.SortedByFitness();
                bestEver = population.First();

                allPopulations.Add(population);
                
                
                nullFitness = population.Where(i => null == i.fitness);
                foreach (var i in nullFitness)
                {
                    await this.EvaluateFitnessOfIndividual(i);
                }

                allFitnessValues.AddRange(population.Select(i => i.fitness!));

                progress.generationsInRunCompleted = generationIndex + 1;

                if (timeoutInfo.ShouldTimeout) break;
            }

            if (multithread) await UniTask.SwitchToMainThread();

            return new GeneratedPopulations(
                allPopulations,
                FitnessStats.GetDetailedSummary(allFitnessValues),
                timeoutInfo.runStartTime,
                DateTime.Now,
                bestEver,
                this.verbose);
        }

        partial void SetMinTreeDictionariesForSatisfiableNodeTypes();

        public async UniTask EvaluateFitnessOfIndividual(Individual i)
        {
            // if (this._individualCache.TryGetValue(i.genome, out var fitness))
            // {
            //     i.fitness = fitness.DeepCopy();
            // }
            // else
            // {
                i.fitness = this.fitnessFunction switch
                {
                    IAsync asyncFitnessFunction => 
                        await asyncFitnessFunction.GetFitnessOfIndividualAsync(this, i),
                    ISync syncFitnessFunction => 
                        syncFitnessFunction.GetFitnessOfIndividual(this, i),
                    _ => 
                        throw new Exception(
                            "The given fitness function does not implement either IAsync or ISync")
                };  
            //     this._individualCache.Add(i.genome, i.fitness.DeepCopy());
            // }

        }

        private (Node, Node)? OnePointCrossoverChildren(Node t1, Node t2) 
        {
            var a = t1.DeepCopy();
            var b = t2.DeepCopy();

            var xPoints = GetLegalCrossoverPointsInChildren(a, b);
            if (!xPoints.Any())
            {
                if (this.verbose)
                {
                    CustomPrinter.PrintLine("No legal crossover points. Skipped crossover");
                }
                this.verbose.numberOfTimesNoLegalCrossoverPoints++;
                return null;
            }

            int xaRand = xPoints.Keys.GetRandomEntry(this.rand);
            int xbRand = xPoints[xaRand].GetRandomEntry(this.rand);

            var xaSubTreeNodeWrapper = a.GetNodeWrapperAtIndex(xaRand);
            var xbSubTreeNodeWrapper = b.GetNodeWrapperAtIndex(xbRand);

            if (xaSubTreeNodeWrapper.child.Equals(xbSubTreeNodeWrapper.child))
            {
                if (this.verbose)
                {
                    CustomPrinter.PrintLine("Nodes swapped are equivalent");
                }
                this.verbose.numberOfTimesCrossoverSwappedEquivalentNode++;
                return null;
            }

            if (xaSubTreeNodeWrapper.child.GetHeight() + b.GetDepthOfNodeAtIndex(xbRand) >
                this.populationParameters.maxDepth ||
                xbSubTreeNodeWrapper.child.GetHeight() + a.GetDepthOfNodeAtIndex(xaRand) >
                this.populationParameters.maxDepth)
            {
                if (this.verbose)
                {
                    CustomPrinter.PrintLine("Crossover too deep");
                    CustomPrinter.PrintLine($"     Height: {a.GetDepthOfNodeAtIndex(xaRand)} --");
                    xaSubTreeNodeWrapper.child.PrintAsList("     ");
                    CustomPrinter.PrintLine($"     Height: {b.GetDepthOfNodeAtIndex(xbRand)} -- ");
                    xbSubTreeNodeWrapper.child.PrintAsList("     ");
                }
                this.verbose.numberOfTimesCrossoverWasTooDeep++;
                return null;
            }

            var tmp = xbSubTreeNodeWrapper.child;
            xbSubTreeNodeWrapper.ReplaceWith(xaSubTreeNodeWrapper.child);
            xaSubTreeNodeWrapper.ReplaceWith(tmp);
            Debug.Assert(xaSubTreeNodeWrapper.child.returnType == xbSubTreeNodeWrapper.child.returnType);

            return (a, b);
        }

        public void Mutate(Node root)
        {
            
            var nodesToChooseFrom = root.IterateNodeWrapperWithoutRoot().ToList();


            var oldNode = nodesToChooseFrom.GetRandomEntry(this.rand);
            int randomTreeMaxDepth = root.GetHeight() - root.GetDepthOfNode(oldNode.child);

            if (oldNode.parent == null) throw new NullReferenceException("Old node parent cannot be null.");
            var randomNode = oldNode.child.Mutate(this, randomTreeMaxDepth);

            if (oldNode.child.Equals(randomNode))
            {
                if (this.verbose)
                {
                    CustomPrinter.PrintLine("Mutated node is equivalent to old node.");
                }
                this.verbose.numberOfTimesMutationCreatedEquivalentNode++;
                return;
            }

            oldNode.ReplaceWith(randomNode);
            

        }

        public List<Individual> TournamentSelection(List<Individual> population)
        {
            Debug.Assert(population.All(i => null != i.fitness));

            var winners = new List<Individual>();

            for (var tournamentNumber = 0;
                 tournamentNumber < this.populationParameters.populationSize;
                 tournamentNumber++)
            {
                var tmpPopulation = population.ToList();
                var competitors = new List<Individual>();
                // Populate competitors
                for (var competitorNumber = 0;
                     competitorNumber < this.populationParameters.tournamentSize;
                     competitorNumber++)
                {
                    var competitor = tmpPopulation.GetRandomEntry(this.rand);
                    competitors.Add(competitor);
                    tmpPopulation.Remove(competitor);
                }

                competitors = competitors.SortedByFitness();
                var winner = competitors.FirstOrDefault() ??
                             throw new Exception("List of competitors cannot be empty.");
                winners.Add(winner);
            }

            return winners;
        }

        public void GenerationalReplacement(ref List<Individual> newPop, List<Individual> oldPop)
        {
            
            this.AssertPopulationsAreNotInDenyList(oldPop, newPop);

            // Sort the population
            oldPop = oldPop.SortedByFitness();
            newPop = newPop.SortedByFitness();

            // Store the original old population so we can check that individuals have been
            // propagated to the new population correctly.
            var originalOldPop = oldPop.ToList();

            // Append "elite size" best solutions from the old population to the new population.
            newPop.AddRange(oldPop.Take(this.populationParameters.eliteSize));
            // Remove those solutions from the old population so that they are not selected
            // again when checking whether the new population is the correct size.
            oldPop.RemoveRange(0, this.populationParameters.eliteSize);

            // Check if the new population has the correct size.
            // If not, add to the new population however many are missing from the old population 
            if (newPop.Count < this.populationParameters.populationSize)
            {
                int numberMissing = this.populationParameters.populationSize - newPop.Count;
                newPop.AddRange(oldPop.Take(numberMissing));
            }

            newPop = newPop.SortedByFitness();

            // We may have added more individuals than allowed, so only take "population size" individuals
            if (newPop.Count > this.populationParameters.populationSize)
            {
                newPop = newPop.Take(this.populationParameters.populationSize).ToList();
                
            }
            
            Debug.Assert(newPop.Count == this.populationParameters.populationSize);

            var bestNewPop = newPop.First().fitness ?? throw new InvalidOperationException();
            var bestOldPop = originalOldPop.SortedByFitness().First().fitness ?? throw new InvalidOperationException();
            if (bestNewPop.TotalFitness < bestOldPop.TotalFitness)
                throw new Exception("New population best is worse than old population best");
        }
        
        public void AssertPopulationsAreNotInDenyList(IEnumerable<Individual>? oldPop, IEnumerable<Individual>? newPop)
        {
            Debug.Assert(!newPop?.Any(IsInDenyList) ?? true, "New Population: Some individuals are in deny list.");
            Debug.Assert(!oldPop?.Any(IsInDenyList) ?? true, "Old Population: Some individuals are in deny list.");
        }

        public async UniTask<List<Individual>> GenerateNewPopulation(List<Individual> oldPopulation)
        {
            this.AssertPopulationsAreNotInDenyList(oldPopulation, null);

            #region Selection
                var parents = this.TournamentSelection(oldPopulation);
            #endregion 
            
            #region Variation — Generate new individuals
            // Crossover
                var newPopulation = this.CrossoverListOfParents(parents);

                // Mutation
                newPopulation.ForEach(i =>
                {
                    if (this.rand.NextDouble() > this.populationParameters.mutationProbability)
                    {
                        if (this.verbose)
                        {
                            CustomPrinter.PrintLine("Skipped mutation");
                        }
                        this.verbose.numberOfTimesMutationSkipped++;
                    }
                    else
                    {
                        this.Mutate(i.genome);
                    }
                });

                // Remove individuals in deny-list
                newPopulation = newPopulation.Where(i => !IsInDenyList(i)).ToList();

                this.AssertPopulationsAreNotInDenyList(null, newPopulation);

                if (this.verbose && newPopulation.Count < this.populationParameters.populationSize)
                    CustomPrinter.PrintLine(
                        $"{this.populationParameters.populationSize - newPopulation.Count} individuals removed in deny list");
            #endregion

            #region Evaluation
                foreach (var individual in newPopulation.TakeWhile(individual => !this.timeoutInfo.ShouldTimeout))
                {
                    await this.EvaluateFitnessOfIndividual(individual);
                }
            #endregion

            #region Generational replacement
                // Replace worst performing new individuals with the best performing old individuals
                
                this.GenerationalReplacement(ref newPopulation, oldPopulation);

                this.AssertPopulationsAreNotInDenyList(null, newPopulation);
                Debug.Assert(newPopulation.Count == this.populationParameters.populationSize);
            #endregion

            return newPopulation;
        }

        public List<Individual> CrossoverListOfParents(List<Individual> parents)
        {
            var newPopulation = new List<Individual>();
            // assert within size constraint
            while (newPopulation.Count < this.populationParameters.populationSize)
            {
                var parent1 = parents.GetRandomEntry(this.rand).DeepCopy();
                var tmpParents = parents.ToList();
                tmpParents.Remove(parent1);
                var parent2 = tmpParents.GetRandomEntry(this.rand).DeepCopy();

                if (this.rand.NextDouble() > this.populationParameters.crossoverProbability)
                {
                    if (this.verbose)
                    {
                        CustomPrinter.PrintLine("Skipped crossover");
                    }
                    this.verbose.numberOfTimesCrossoverSkipped++;
                    newPopulation.Add(parent1);
                    if (newPopulation.Count < this.populationParameters.populationSize) newPopulation.Add(parent2);

                    continue;
                }

                (Node, Node)? crossoverChildren = this.OnePointCrossoverChildren(parent1.genome, parent2.genome);

                crossoverChildren ??= (parent1.genome.DeepCopy(), parent2.genome.DeepCopy());
                var (child1, child2) = crossoverChildren.Value;

                var individual1 = new Individual(child1);
                newPopulation.Add(individual1);
                
                if (newPopulation.Count >= this.populationParameters.populationSize) continue;
                
                var individual2 = new Individual(child2);
                newPopulation.Add(individual2);
            }

            return newPopulation;
        }
    }
}