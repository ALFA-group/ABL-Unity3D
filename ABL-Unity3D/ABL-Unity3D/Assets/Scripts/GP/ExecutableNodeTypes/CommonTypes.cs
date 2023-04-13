using System;
using Sirenix.Serialization;
using UnityEngine;
using Utilities.GeneralCSharp;
using Utilities.Unity;

namespace GP.ExecutableNodeTypes
{
    
    public abstract class BooleanOperator : BinaryOperator<bool, bool>
    {
        protected BooleanOperator(GpBuildingBlock<bool> left, GpBuildingBlock<bool> right) :
            base(left, right)
        {
        }

        protected BooleanOperator() : base(new BooleanConstant(true), new BooleanConstant(true))
        {
        }
    }

    public class And : BooleanOperator
    {
        public And(GpBuildingBlock<bool> left, GpBuildingBlock<bool> right) :
            base(left, right)
        {
        }

        public And()
        {
        }

        public override bool Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this.Left.Evaluate(gpFieldsWrapper) && this.Right.Evaluate(gpFieldsWrapper);
        }
    }

    public class Or : BooleanOperator
    {
        public Or(GpBuildingBlock<bool> left, GpBuildingBlock<bool> right) :
            base(left, right)
        {
        }

        public Or()
        {
        }

        public override bool Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this.Left.Evaluate(gpFieldsWrapper) || this.Right.Evaluate(gpFieldsWrapper);
        }
    }

    public class Not : GpBuildingBlock<bool>
    {
        public Not(GpBuildingBlock<bool> operand) : base(operand)
        {
        }

        public Not()
        {
        }

        public GpBuildingBlock<bool> Operand => (GpBuildingBlock<bool>)this.children[0];

        public override bool Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return !this.Operand.Evaluate(gpFieldsWrapper);
        }
    }

    public class GreaterThan : BinaryOperator<bool, int>
    {
        public GreaterThan(GpBuildingBlock<int> left, GpBuildingBlock<int> right) :
            base(left, right)
        {
        }

        
        public GreaterThan() : base(new IntegerConstant(0), new IntegerConstant(0))
        {
        }

        public override bool Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this.Left.Evaluate(gpFieldsWrapper) > this.Right.Evaluate(gpFieldsWrapper);
        }
    }

    public class BoolEquals : BooleanOperator
    {
        public BoolEquals(GpBuildingBlock<bool> left, GpBuildingBlock<bool> right) :
            base(left, right)
        {
        }

        public override bool Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this.Left.Evaluate(gpFieldsWrapper) == this.Right.Evaluate(gpFieldsWrapper);
        }
    }

    public class IntEquals : BinaryOperator<bool, int>
    {
        public IntEquals(GpBuildingBlock<int> left, GpBuildingBlock<int> right) :
            base(left, right)
        {
        }

        public IntEquals() : base(new IntegerConstant(0), new IntegerConstant(0))
        {
        }

        public override bool Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this.Left.Evaluate(gpFieldsWrapper) == this.Right.Evaluate(gpFieldsWrapper);
        }
    }


    public class Vector2Constant : GpBuildingBlock<Vector2>
    {
        private readonly Vector2 _vector2Representation;

        public Vector2Constant(float x, float y)
        {
            this._vector2Representation = new Vector2(x, y);
            this.symbol = this._vector2Representation.ToString();
        }

        [RandomTreeConstructor]
        public Vector2Constant(GpFieldsWrapper gpFieldsWrapper) : base(gpFieldsWrapper)
        {
            this._vector2Representation =
                gpFieldsWrapper.rand.NextVector2();
            this.symbol = this._vector2Representation.ToString();
        }
        
        public override Vector2 Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this._vector2Representation;
        }
    }

    public class Vector2Variable : GpBuildingBlock<Vector2>
    {
        [OdinSerialize] private readonly int _argIndex;

        public Vector2Variable(int argIndex)
        {
            this._argIndex = argIndex;
        }

        [RandomTreeConstructor]
        public Vector2Variable(GpFieldsWrapper gpFieldsWrapper) : base(gpFieldsWrapper)
        {
            this._argIndex = gpFieldsWrapper.positionalArguments?.PopNextIndex() ?? throw new Exception("Positional arguments is null.");
        }

        public override Vector2 Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            if (null == gpFieldsWrapper.positionalArguments) throw new Exception("Positional arguments are null.");
            if (gpFieldsWrapper.positionalArguments.MapToTypedArgument(this._argIndex,
                    out Vector2? arg)) 
                if (arg.HasValue)
                    return arg.Value;

            return Vector2.zero; 
            // Sounds like a bad idea to use a vector2variable when there is no positional argument of type vector
        }
    }

    public class BooleanConstant : GpBuildingBlock<bool>
    {
        [OdinSerialize] private readonly bool _value;

        public BooleanConstant(bool v)
        {
            this._value = v;
            this.symbol = v.ToString();
        }

        [RandomTreeConstructor]
        public BooleanConstant(GpFieldsWrapper gpFieldsWrapper) : base(gpFieldsWrapper)
        {
            this._value = gpFieldsWrapper.rand.NextBool();
            this.symbol = this._value.ToString();
        }

        public override bool Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this._value;
        }
    }

    public class IntegerConstant : GpBuildingBlock<int>
    {
        [OdinSerialize] private readonly int _value;

        public IntegerConstant(int v)
        {
            this._value = v;
            this.symbol = v.ToString();
        }

        [RandomTreeConstructor]
        public IntegerConstant(GpFieldsWrapper gpFieldsWrapper) : base(gpFieldsWrapper)
        {
            
            this._value = gpFieldsWrapper.rand.Next(0, 2); 
            this.symbol = this._value.ToString();
        }

        public override int Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this._value;
        }
    }


    public class FloatConstant : GpBuildingBlock<float>
    {
        [OdinSerialize] protected float value; 

        public FloatConstant(float v)
        {
            this.value = v;
            this.symbol = v.ToString();
        }

        [RandomTreeConstructor]
        public FloatConstant(GpFieldsWrapper gpFieldsWrapper) : base(gpFieldsWrapper)
        {
            this.value = gpFieldsWrapper.rand.NextFloat();
            this.symbol = this.value.ToString();
        }

        public override float Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this.value;
        }
    }

    
    public class DiscreteFloatConstant : FloatConstant
    {
        public DiscreteFloatConstant(float value) : base(value)
        {
        }

        [RandomTreeConstructor]
        public DiscreteFloatConstant(GpFieldsWrapper gpFieldsWrapper) : base(gpFieldsWrapper)
        {
            this.value = this.value.Quantize(
                4, 0, 1);
            this.symbol = this.value.ToString();
        }
    }
}