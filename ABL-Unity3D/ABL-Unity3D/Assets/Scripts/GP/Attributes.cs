using System;
using Utilities.GeneralCSharp;

namespace GP
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class RandomTreeConstructorAttribute : Attribute
    {
    }

    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter,
        Inherited = false)]
    public class FilterAttribute : Attribute
    {
        public bool IsSatisfiedBy(Type candidate)
        {
            return candidate.HasAttribute(this.GetType());
        }
    }

    public sealed class FriendlyAttribute : FilterAttribute
    {
    }

    public sealed class EnemyAttribute : FilterAttribute
    {
    }
}