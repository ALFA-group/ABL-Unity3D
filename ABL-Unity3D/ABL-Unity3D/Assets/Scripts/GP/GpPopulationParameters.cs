using System.Collections.Generic;
using UnityEngine;

// ReSharper disable InconsistentNaming

#nullable enable

namespace GP
{
    public class GpPopulationParameters
    {
        public const int DEFAULT_MAX_DEPTH = 5;
        public const int DEFAULT_POPULATION_SIZE = 100;
        public const int DEFAULT_NUMBER_GENERATIONS = 20;
        public const double DEFAULT_CROSSOVER_PROBABILITY = 0.8;
        public const double DEFAULT_MUTATION_PROBABILITY = 0.2;
        public const int DEFAULT_TOURNAMENT_SIZE = 3;
        public const int DEFAULT_ELITE_SIZE = 2;
        public const bool DEFAULT_RAMP = true;
        
        public readonly double mutationProbability;
        public readonly double crossoverProbability;
        public readonly int eliteSize;
        public readonly int maxDepth;
        public readonly int numberGenerations;
        public readonly PopulationInitializationMethod populationInitializationMethod;
        public readonly int populationSize;
        public readonly ProbabilityDistribution probabilityDistribution;
        public readonly bool ramp;
        public readonly int tournamentSize;

        public GpPopulationParameters(
            int eliteSize = DEFAULT_ELITE_SIZE,
            int tournamentSize = DEFAULT_TOURNAMENT_SIZE,
            int populationSize = DEFAULT_POPULATION_SIZE,
            int maxDepth = DEFAULT_MAX_DEPTH,
            int numberGenerations = DEFAULT_NUMBER_GENERATIONS,
            double crossoverProbability = DEFAULT_CROSSOVER_PROBABILITY,
            double mutationProbability = DEFAULT_MUTATION_PROBABILITY,
            bool ramp = DEFAULT_RAMP,
            ProbabilityDistribution? probabilityDistribution = null,
            PopulationInitializationMethod? populationInitializationMethod = null)
        {
            this.eliteSize = eliteSize;
            this.tournamentSize = tournamentSize;
            this.populationSize = populationSize;
            this.maxDepth = maxDepth;
            this.numberGenerations = numberGenerations;
            this.crossoverProbability = crossoverProbability;
            this.mutationProbability = mutationProbability;
            this.ramp = ramp;
            
            this.probabilityDistribution = probabilityDistribution ??
                                           new ProbabilityDistribution(new List<TypeProbability>
                                               { new TypeProbability() });
            this.populationInitializationMethod =
                populationInitializationMethod ?? new RampedPopulationInitialization();
        }
    }
}