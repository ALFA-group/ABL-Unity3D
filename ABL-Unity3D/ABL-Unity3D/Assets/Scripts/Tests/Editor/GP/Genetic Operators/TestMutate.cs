using System;
using System.Collections.Generic;
using ABLUnitySimulation;
using GP;
using GP.ExecutableNodeTypes;
using GP.ExecutableNodeTypes.ABLUnityActionTypes;
using GP.ExecutableNodeTypes.ABLUnityTypes;
using NUnit.Framework;

namespace Tests.Editor.GP.Genetic_Operators
{
    public class TestMutate : GpRunnerUnitTest
    {
        protected static readonly ProbabilityDistribution TypesForThisUnitTest = new ProbabilityDistribution(
            new List<Type>()
            {
                typeof(Vector2Constant),
                typeof(SimAgentConstantFriendly),
                typeof(MoveToVector2),
                typeof(FloatConstant)
            }
        );
        
        private const int RandomSeed = 0;
        private const int MaxDepth = 10;

        private readonly Node _expectedResult = new MoveToVector2(
            new Vector2Constant(7, 7),
            new SimAgentConstantFriendly(new Handle<SimAgent>(new SimId(1))),
            new FloatConstant(0.177181125f)
        );

        private readonly Node _inputNode = new MoveToVector2(
            new Vector2Constant(7, 7),
            new SimAgentConstantFriendly(new Handle<SimAgent>(new SimId(1))),
            new FloatConstant(1)
        );

        public TestMutate() : base(
            new GpPopulationParameters(
                probabilityDistribution: TypesForThisUnitTest,
                maxDepth: MaxDepth
            ),
            solutionReturnType: typeof(MoveToVector2),
            randomSeed: RandomSeed
        )
        {
        }

        [Test]
        public void HardCodedInputNode()
        {
            var runner = this.GetGpRunner();
            runner.Mutate(this._inputNode);
            Assert.That(this._inputNode, Is.EqualTo(this._expectedResult).Using(new NodeComparer()));
        }
    }
}