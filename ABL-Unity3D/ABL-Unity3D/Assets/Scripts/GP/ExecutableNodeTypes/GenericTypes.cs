using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Assertions;
using Utilities.GeneralCSharp;
using Utilities.GP;
using Utilities.Unity;

#nullable enable

namespace GP.ExecutableNodeTypes
{
    public abstract class GpBuildingBlockClosed : Node
    {
        protected GpBuildingBlockClosed(Type returnType, List<Node> inputs) : base(returnType, inputs)
        {
        }

        protected GpBuildingBlockClosed(Type returnType) : base(returnType)
        {
        }

        public abstract object? EvaluateToObject(GpFieldsWrapper gpFieldsWrapper);
    }

    public abstract class GpBuildingBlock<TReturnType> : GpBuildingBlockClosed
    {
        protected GpBuildingBlock(params Node[] inputs) : base(typeof(TReturnType), inputs.ToList())
        {
        }

        protected GpBuildingBlock() : base(typeof(TReturnType))
        {
        }

        [RandomTreeConstructor]
        // ReSharper disable once UnusedParameter.Local
        protected GpBuildingBlock(GpFieldsWrapper gpFieldsWrapper) : base(typeof(TReturnType))
        {
        }

        public abstract TReturnType Evaluate(GpFieldsWrapper gpFieldsWrapper);

        public override object? EvaluateToObject(GpFieldsWrapper gpFieldsWrapper)
        {
            return this.Evaluate(gpFieldsWrapper);
        }
    }

    public class TypedRootNode<TReturnType> : GpBuildingBlock<TReturnType>
    {
        public TypedRootNode(GpBuildingBlock<TReturnType> child) : base(child)
        {
            this.symbol = "RootNode" + GpUtility.GetBetterClassName(typeof(TReturnType));
        }

        public GpBuildingBlock<TReturnType> Child =>
            (GpBuildingBlock<TReturnType>)this.children[0]; // There is only ever one child in this tree

        public override TReturnType Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this.Child.Evaluate(gpFieldsWrapper);
        }
    }

    public abstract class BinaryOperator<TReturnType, TOperandType> : GpBuildingBlock<TReturnType>
    {
        protected BinaryOperator(GpBuildingBlock<TOperandType> left, GpBuildingBlock<TOperandType> right) :
            base(left, right)
        {
        }

        public GpBuildingBlock<TOperandType> Left => (GpBuildingBlock<TOperandType>)this.children[0];
        public GpBuildingBlock<TOperandType> Right => (GpBuildingBlock<TOperandType>)this.children[1];
    }
}