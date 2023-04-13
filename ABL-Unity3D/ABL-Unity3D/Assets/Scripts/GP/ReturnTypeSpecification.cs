using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.GeneralCSharp;

#nullable enable

namespace GP
{
    public class ReturnTypeSpecification : IEquatable<ReturnTypeSpecification>
    {
        public readonly IEnumerable<FilterAttribute> filters;
        public readonly Type returnType;

        public ReturnTypeSpecification(Type returnType, IEnumerable<FilterAttribute>? filters)
        {
            this.returnType = returnType;
            this.filters = filters ?? new FilterAttribute[] { };
        }

        public bool Equals(ReturnTypeSpecification? spec)
        {
            if (null == spec) return false;
            bool sameReturnType = this.returnType == spec.returnType;
            bool sameFilters = this.filters.SequenceEqual(spec.filters);
            return sameReturnType && sameFilters;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ReturnTypeSpecification ??
                               throw new InvalidOperationException());
        }

        public override int GetHashCode()
        {
            var allHashes = new List<int> { this.returnType.GetHashCode() };
            allHashes.AddRange(this.filters.Select(f => f.GetHashCode()));
            return GenericUtilities.CombineHashCodes(allHashes);
        }
    }
}