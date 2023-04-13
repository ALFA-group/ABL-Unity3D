using System.Collections.Generic;
using ABLUnitySimulation;
using Sirenix.OdinInspector;

#nullable enable

namespace Planner
{
    /// <summary>
    ///     A method is an abstract way of describing how to complete a task.
    ///     This specific class is used as an abstraction for when code nee
    ///     to take any type of method regardless of the task specification.
    /// </summary>
    /// <remarks>
    ///     A method decomposes into either sub-methods or
    ///     <see cref="SimAction" />s which achieve the given task specification. These method decompositions
    ///     are what are used in a <see cref="Plan" /> during execution. This specific class does not take a task
    ///     decomposition and is used purely for abstraction purposes, as stated in the above summary.
    /// </remarks>
    public abstract class Method
    {
        /// <summary>
        ///     Notes regarding this method.
        /// </summary>
        public string notes = "";

        /// <summary>
        ///     Get decompositions for this method. Essentially, these are instructions for how to decompose this method.
        /// </summary>
        /// <param name="context">The context which is used in method decomposition.</param>
        /// <returns>Decompositions for this method.</returns>
        public abstract IEnumerable<Decomposition> Decompose(PlannerContext context);

        /// <summary>
        ///     Get a <see cref="SimAction" /> when this method cannot be further decomposed and the
        ///     method has a meaningful implementation that is not "do nothing".
        /// </summary>
        /// <param name="state">The current simulation world state.</param>
        /// <returns>
        ///     A <see cref="SimAction" /> that has a meaningful implementation for the given world <paramref name="state" />.
        ///     <see cref="SimAction" />.
        /// </returns>
        public virtual SimAction? GetActionForSim(SimWorldState state)
        {
            return null;
        }
    }

    /// <summary>
    ///     A more concrete version of <see cref="Method" /> which takes a task specification
    ///     to achieve by using this method. See <see cref="Method" /> for an explanation of
    ///     how methods work in combination with the planner.
    /// </summary>
    /// <typeparam name="TTaskSpec">The task specification to achieve by using this method.</typeparam>
    public abstract class Method<TTaskSpec> : Method where TTaskSpec : struct, ITaskSpec
    {
        [HideDuplicateReferenceBox]
        public TTaskSpec taskSpec;

        protected Method(TTaskSpec prototype)
        {
            this.taskSpec = prototype;
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}{{ {this.taskSpec}  }}";
        }

        /// <summary>
        ///     Copy a method and set its task specification to the given task <paramref name="specification" />.
        /// </summary>
        /// <param name="specification">The task specification to associate with the returned method.</param>
        /// <returns>A copy of this method with the task specification set to the given task <paramref name="specification" />.</returns>
        public virtual Method<TTaskSpec> CloneWith(TTaskSpec specification)
        {
            var clone = (Method<TTaskSpec>)this.MemberwiseClone();
            clone.taskSpec = specification;
            return clone;
        }
    }

    /// <summary>
    ///     This method decomposes into any method that uses the same TTaskSpec,
    ///     but not to its own type again!
    /// </summary>
    /// <typeparam name="TTaskSpec"></typeparam>
    public class MethodAny<TTaskSpec> : Method<TTaskSpec> where TTaskSpec : struct, ITaskSpec
    {
        public MethodAny(TTaskSpec prototype) : base(prototype)
        {
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            foreach (Method methodOption in context.methods.GetMethodOptions(this.taskSpec))
                if (!(methodOption is MethodAny<TTaskSpec>))
                    yield return new Decomposition(Decomposition.ExecutionMode.Sequential, methodOption);
        }

        public static implicit operator MethodAny<TTaskSpec>(TTaskSpec spec)
        {
            return new MethodAny<TTaskSpec>(spec);
        }

        public override string ToString()
        {
            return $"MethodAny<{typeof(TTaskSpec).Name}>{{{this.taskSpec}}}";
        }
    }
}