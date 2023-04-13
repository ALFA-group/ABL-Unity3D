using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.GeneralCSharp;
using Utilities.GP;

#nullable enable


namespace GP
{
    [HideReferenceObjectPicker]
    public class Generation 
    {
        [HideInInspector]
        public readonly string generationNumberString;

        
        public readonly List<Individual> population;

        public Generation(List<Individual> sortedPopulation, int generationNumber)
        {
            this.population = sortedPopulation;
            this.generationNumberString = $"Generation {generationNumber}";
        }

        public Individual? Best => this.population.Any() ? this.population[0] : null;
        public Individual? Worst => this.population.Any() ? this.population[this.population.Count - 1] : null;
        public Individual? Median => this.population.Any() ? this.population[this.population.Count / 2] : null;
    }

    [HideReferenceObjectPicker]
    public class GeneratedPopulations
    {
        [ListDrawerSettings(ListElementLabelName = "generationNumberString", DraggableItems = false,
            HideRemoveButton = true)]
        // ReSharper disable once MemberCanBePrivate.Global
        public readonly List<Generation> generations;

        // This warning about the HideInInspector attribute being redundant is wrong. It is necessary.
        // ReSharper disable once Unity.RedundantHideInInspectorAttribute
        [HideInInspector]
        public readonly List<List<Individual>> generationsAsNestedList;
        
        // ReSharper disable once MemberCanBePrivate.Global
        public readonly Individual? bestEver;
        // ReSharper disable once MemberCanBePrivate.Global
        public FitnessStats.DetailedSummary fitnessSummary;
        // ReSharper disable once MemberCanBePrivate.Global
        [UsedImplicitly, DisplayAsString] public readonly float secondsElapsed;
        // ReSharper disable once MemberCanBePrivate.Global
        [DisplayAsString]
        public readonly DateTime startTime;
        // ReSharper disable once MemberCanBePrivate.Global
        [DisplayAsString]
        public readonly DateTime endTime;

        public readonly VerboseInfo verboseInfo;

        [HideInInspector, UsedImplicitly]
        public string GpExperimentRunNumberString
        {
            get
            {
                var s = "GP Experiment Run";
                if (null != this.bestEver?.fitness) s += $"; Best Fitness: {this.bestEver.fitness.TotalFitness}";
                return s;
            }
        }

        public GeneratedPopulations(
            List<List<Individual>> populations,
            FitnessStats.DetailedSummary fitnessSummary,
            DateTime startTime,
            DateTime endTime,
            Individual? bestEver,
            VerboseInfo verboseInfo)
        {
            this.generationsAsNestedList = populations;
            this.generations = new List<Generation>();
            var populationsAsList = this.generationsAsNestedList.ToList();
            for (var i = 0; i < populationsAsList.Count; i++)
                this.generations.Add(new Generation(populationsAsList[i].SortedByFitness(), i));

            this.fitnessSummary = fitnessSummary;
            this.startTime = startTime;
            this.endTime = endTime;
            this.secondsElapsed = GenericUtilities.SecondsElapsed(startTime, endTime);
            this.bestEver = bestEver;
            this.verboseInfo = verboseInfo;
        }
        
        public Individual? GetBestEver()
        {
            return GetBestEver(this.generationsAsNestedList);
        }

        private static Individual? GetBestEver(IEnumerable<IEnumerable<Individual>> populations)
        {
            return populations.Last().SortedByFitness().FirstOrDefault();
        }

        public GeneratedPopulations DeepCopy()
        {
            var newPopulations = this.generations.Select(population =>
                    population.population.Select(individual =>
                        individual.DeepCopy()
                    ).ToList())
                .ToList();

            return new GeneratedPopulations(newPopulations, this.fitnessSummary, this.startTime, this.endTime, this.bestEver, this.verboseInfo);
        }

        public List<List<double>> GetFitnessValuesForAllGenerations()
        {
            return this.generationsAsNestedList
                .Select(individuals => individuals
                    .Select(i => i.fitness?.TotalFitness)
                    .WhereNotNull()
                    .ToList()
                ).ToList();
        }
    }
}