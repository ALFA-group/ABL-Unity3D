using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using GP;
using GP.ExecutableNodeTypes;
using GP.ExecutableNodeTypes.ABLUnityActionTypes;
using GP.ExecutableNodeTypes.ABLUnityTypes;
using NUnit.Framework;

namespace Tests.Editor.GP.Genetic_Operators
{
    public class TestCrossoverParents : GpRunnerUnitTest
    {
        protected static readonly ProbabilityDistribution TypesForThisUnitTest = new ProbabilityDistribution(
            new List<Type>()
            {
                typeof(Vector2Constant),
                typeof(MoveToVector2),
                typeof(SimAgentConstantFriendly),
                typeof(FloatConstant)
            }
        );
        
        private const int PopulationSize = 3;
        private const int MaxDepth = 10;
        private const double CrossoverProbability = 1.0;
        private const int RandomSeed = 7;

        private readonly List<Node> _expectedResults = new List<Node>
        {
            new MoveToVector2(
                new Vector2Constant(4, 4),
                new SimAgentConstantFriendly(new Handle<SimAgent>(new SimId(3))),
                new FloatConstant(4)),

            new MoveToVector2(
                new Vector2Constant(3, 3),
                new SimAgentConstantFriendly(new Handle<SimAgent>(new SimId(4))),
                new DiscreteFloatConstant(3)),

            new MoveToVector2(
                new Vector2Constant(4, 4),
                new SimAgentConstantFriendly(new Handle<SimAgent>(new SimId(2))),
                new FloatConstant(2))
        };

        private readonly List<Individual> _inputPopulation = new List<Individual>
        {
            new Individual(
                new MoveToVector2(
                    new Vector2Constant(2, 2),
                    new SimAgentConstantFriendly(new Handle<SimAgent>(new SimId(2))),
                    new FloatConstant(2)
                )
            ),
            new Individual(
                new MoveToVector2(
                    new Vector2Constant(3, 3),
                    new SimAgentConstantFriendly(new Handle<SimAgent>(new SimId(3))),
                    new DiscreteFloatConstant(3)
                )
            ),
            new Individual(
                new MoveToVector2(
                    new Vector2Constant(4, 4),
                    new SimAgentConstantFriendly(new Handle<SimAgent>(new SimId(4))),
                    new FloatConstant(4)
                )
            )
        };

        public TestCrossoverParents() : base(
            new GpPopulationParameters(
                probabilityDistribution: TypesForThisUnitTest,
                populationSize: PopulationSize,
                maxDepth: MaxDepth,
                crossoverProbability: CrossoverProbability
            ),
            randomSeed: RandomSeed,
            solutionReturnType: typeof(MoveToVector2))
        {
        }

        [Test]
        public void HardCodedInputPopulations()
        {
            var results = this.GetGpRunner(true)
                .CrossoverListOfParents(this._inputPopulation)
                .Select(i => i.genome);
            Assert.That(results, Is.EquivalentTo(this._expectedResults).Using(new NodeComparer()));
        }
    }
}