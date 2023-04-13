using System.Collections.Generic;
using GP;
using GP.ExecutableNodeTypes;
using GP.ExecutableNodeTypes.ABLUnityActionTypes;
using GP.ExecutableNodeTypes.ABLUnityTypes;
using NUnit.Framework;

namespace Tests.Editor.GP.Genetic_Operators
{
    public class TestGetLegalCrossoverPointsInChildren
    {
        private readonly Dictionary<int, List<int>> _expectedResults = new Dictionary<int, List<int>>
        {
            { 1, new List<int> { 1 } },
            { 2, new List<int> { 2 } },
            { 3, new List<int> { 3 } }
        };

        private readonly Node _testNodeA = new MoveToVector2(
            new Vector2Constant(0, 0),
            new SimAgentVariableFriendly(0),
            new DiscreteFloatConstant(0)
        );

        private readonly Node _testNodeB = new MoveToVector2(
            new Vector2Constant(4, 4),
            new SimAgentVariableFriendly(0),
            new FloatConstant(0));

        [Test]
        public void TestHardCodedNodeInputs()
        {
            var results = GpRunner.GetLegalCrossoverPointsInChildren(this._testNodeA, this._testNodeB);
            Assert.That(results, Is.EquivalentTo(this._expectedResults));
        }
    }
}