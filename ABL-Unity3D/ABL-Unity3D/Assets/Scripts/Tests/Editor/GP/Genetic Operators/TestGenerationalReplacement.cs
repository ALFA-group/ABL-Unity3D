using System;
using System.Collections.Generic;
using GP;
using GP.ExecutableNodeTypes;
using GP.FitnessFunctions;
using NUnit.Framework;

namespace Tests.Editor.GP.Genetic_Operators
{
    public class TestGenerationalReplacement : GpRunnerUnitTest
    {
        protected static readonly ProbabilityDistribution TypesForThisUnitTest = new ProbabilityDistribution(
            new List<Type>()
            {
                typeof(BooleanConstant)
            }
        );
        
        private const int PopulationSize = 5;
        private const int EliteSize = 3;

        private readonly List<Individual> _expectedNewPopulation = new List<Individual>
        {
            new Individual(new BooleanConstant(false),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(100)),
            new Individual(new BooleanConstant(true),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(0)),
            new Individual(new BooleanConstant(false),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(-1)),
            new Individual(new BooleanConstant(false),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(-2)),
            new Individual(new BooleanConstant(true),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(-3))
        };

        private readonly List<Individual> _oldPopulation = new List<Individual>
        {
            new Individual(new BooleanConstant(true),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(-3)),
            new Individual(new BooleanConstant(false),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(-4)),
            new Individual(new BooleanConstant(false),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(-5)),
            new Individual(new BooleanConstant(false),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(-6)),
            new Individual(new BooleanConstant(false),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(100))
        };

        private List<Individual> _newPopulation = new List<Individual>
        {
            new Individual(new BooleanConstant(true),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(0)),
            new Individual(new BooleanConstant(false),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(-1)),
            new Individual(new BooleanConstant(false),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(-2)),
            new Individual(new BooleanConstant(false),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(-10)),
            new Individual(new BooleanConstant(false),
                Fitness.MakeFitnessWithOnlyFitnessScoreWithoutSimScore(-11))
        };

        public TestGenerationalReplacement() : base(new GpPopulationParameters(
            probabilityDistribution: TypesForThisUnitTest,
            populationSize: PopulationSize,
            eliteSize: EliteSize),
            solutionReturnType: typeof(bool))
        {
        }

        [Test]
        public void HardCodedInputPopulations()
        {
            this.GetGpRunner(true).GenerationalReplacement(ref this._newPopulation, this._oldPopulation);

            Assert.That(this._newPopulation,
                Is.EquivalentTo(this._expectedNewPopulation).Using(new IndividualComparer()));
        }
    }
}