using System.Collections.Generic;
using System.Linq;
using GP;
using GP.FitnessFunctions;
using Utilities.GeneralCSharp;
// ReSharper disable MemberCanBePrivate.Global

#nullable enable

namespace Utilities.GP
{
    public class FitnessStats
    {
        
        public readonly SimpleStats fitnessWithoutSimScoreStats;
        public readonly SimpleStats simScoreStats;
        public readonly SimpleStats totalFitnessStats;

        private FitnessStats(IEnumerable<Fitness>? fitnessValues)
        {
            if (null == fitnessValues) fitnessValues = new List<Fitness>();
            
            var fitnessValuesAsList = fitnessValues.ToList();
            this.totalFitnessStats = new SimpleStats(fitnessValuesAsList.Select(f => f.TotalFitness));
            this.simScoreStats = new SimpleStats(fitnessValuesAsList.Select(f => f.weightedSimScore));
            this.fitnessWithoutSimScoreStats =
                new SimpleStats(fitnessValuesAsList.Select(f => f.fitnessWithoutSimScore));
        }

        public static DetailedSummary GetDetailedSummary(IEnumerable<Fitness> fitnessValues)
        {
            return new FitnessStats(fitnessValues).GetDetailedSummary();
        }

        public static DetailedSummary GetDetailedSummary(IEnumerable<Individual> generatedPopulation)
        {
            return GetDetailedSummary(new[] { generatedPopulation });
        }

        private static DetailedSummary GetDetailedSummary(
            IEnumerable<IEnumerable<Individual>> generatedPopulationsDoubleNestedEnumerable)
        {
            var allFitnessValues = GetAllFitnessValues(generatedPopulationsDoubleNestedEnumerable);
            return GetDetailedSummary(allFitnessValues);
        }


        public static DetailedSummary GetDetailedSummary(GeneratedPopulations[] generatedPopulationsArray)
        {
            return GetDetailedSummary(
                GetAllFitnessValues(generatedPopulationsArray));
        }

        private static IEnumerable<Fitness> GetAllFitnessValues(IEnumerable<GeneratedPopulations> generatedPopulationsArray)
        {
            var asNestedList =
                generatedPopulationsArray.Select(generatedPopulation => generatedPopulation.generationsAsNestedList);
            return GetAllFitnessValues(asNestedList);
        }

        private static IEnumerable<Fitness> GetAllFitnessValues(
            IEnumerable<IEnumerable<IEnumerable<Individual>>> generatedPopulationsTripleNestedEnumerable)
        {
            return generatedPopulationsTripleNestedEnumerable.SelectMany(GetAllFitnessValues);
        }

        private static IEnumerable<Fitness> GetAllFitnessValues(
            IEnumerable<IEnumerable<Individual>> generatedPopulationsDoubleNestedEnumerable)
        {
            return generatedPopulationsDoubleNestedEnumerable.SelectMany(generatedPopulation =>
                generatedPopulation.Select(ind => ind.fitness));
        }

        private DetailedSummary GetDetailedSummary()
        {
            return new DetailedSummary
            {
                totalFitnessSummary = this.totalFitnessStats.GetSummary(),
                fitnessWithoutSimScoreSummary = this.fitnessWithoutSimScoreStats.GetSummary(),
                simScoreSummary = this.simScoreStats.GetSummary()
            };
        }


        public struct DetailedSummary
        {
            public SimpleStats.Summary totalFitnessSummary;
            public SimpleStats.Summary fitnessWithoutSimScoreSummary;
            public SimpleStats.Summary simScoreSummary;

            public override string ToString()
            {
                return "Total Fitness Summary:\n" +
                       $"{this.totalFitnessSummary.GetString(4)}\n" +
                       "Fitness Without Sim Score Summary:\n" +
                       $"{this.fitnessWithoutSimScoreSummary.GetString(4)}\n" +
                       "Sim Score Summary:\n" +
                       $"{this.simScoreSummary.GetString(4)}\n";
            }
        }
    }
}