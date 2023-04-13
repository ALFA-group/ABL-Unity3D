using System;
using System.Collections.Generic;
using System.Linq;
using GP;
using GP.ExecutableNodeTypes;
using GP.ExecutableNodeTypes.ABLUnityActionTypes;
using GP.ExecutableNodeTypes.ABLUnityTypes;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Editor.GP.Genetic_Operators
{
    public class TestGetSymTypeAndFilterLocationsInChildren
    {
        private readonly List<int> _expectedResults = new List<int> { 1 };
        private readonly List<FilterAttribute> _filtersOfReturnTypeToSearchFor = new List<FilterAttribute>();

        private readonly Node _inputNode =
            new MoveToVector2(
                new Vector2Constant(0, 0),
                new SimAgentVariableFriendly(0),
                new FloatConstant(0));

        private readonly Type _returnTypeToSearchFor = typeof(Vector2);

        [Test]
        public void TestHardCodedInputNode()
        {
            var results = this._inputNode.GetSymTypeAndFilterLocationsInDescendants(
                this._returnTypeToSearchFor,
                this._filtersOfReturnTypeToSearchFor).ToList();

            Assert.That(results, Is.EqualTo(this._expectedResults));
        }
    }
}