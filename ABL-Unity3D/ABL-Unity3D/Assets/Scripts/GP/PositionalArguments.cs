using System.Linq;
using Utilities.GeneralCSharp;

#nullable enable

namespace GP
{
    public class PositionalArguments
    {
        public MultiTypeCollection positionalArguments;
        public int positionalArgumentsUsed;

        public PositionalArguments(MultiTypeCollection args)
        {
            this.positionalArguments = args;
        }

        public PositionalArguments()
        {
            this.positionalArguments = new MultiTypeCollection();
        }

        public int PopNextIndex()
        {
            return this.positionalArgumentsUsed++;
        }

        public bool MapToTypedArgument<T>(int untypedIndex, out T? arg) where T : struct
        {
            int numOfType = this.positionalArguments.Get<T>().Count();
            if (numOfType < 1)
            {
                arg = default(T);
                return false;
            }

            int typedIndex = untypedIndex % numOfType;
            arg = this.positionalArguments.Get<T>().Skip(typedIndex).FirstOrDefault();
            return true;
        }


        public bool MapToTypedArgument<T>(int untypedIndex, out T? arg) where T : class
        {
            int numOfType = this.positionalArguments.Get<T>().Count();
            if (numOfType < 1)
            {
                arg = default;
                return false;
            }

            int typedIndex = untypedIndex % numOfType;
            arg = this.positionalArguments.Get<T>().Skip(typedIndex).FirstOrDefault();
            return true;
        }
    }
}