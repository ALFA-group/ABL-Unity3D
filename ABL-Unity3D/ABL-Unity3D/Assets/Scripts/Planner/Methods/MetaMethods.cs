using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Planner.Methods
{
    
    public class MethodDoSequentially : Method
    {
        public List<Method> sequentialMethods = new List<Method>();

        public MethodDoSequentially()
        {
        }

        public MethodDoSequentially(params Method[] methods)
        {
            this.sequentialMethods = methods.ToList();
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            var d = new Decomposition(Decomposition.ExecutionMode.Sequential, this.sequentialMethods);
            yield return d;
        }
    }

    public class MethodChooseOne : Method
    {
        public List<Method> optionalMethods = new List<Method>();

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            foreach (var method in this.optionalMethods)
                yield return new Decomposition(Decomposition.ExecutionMode.Sequential, method);
        }
    }

    public class MethodParallel : Method
    {
        public List<Method> methods = new List<Method>();

        public MethodParallel()
        {
        }

        public MethodParallel(IEnumerable<Method> methods)
        {
            this.methods = methods.ToList();
        }

        public override IEnumerable<Decomposition> Decompose(PlannerContext context)
        {
            if (this.methods.Count > 0)
                yield return new Decomposition(Decomposition.ExecutionMode.Parallel, this.methods);
        }
    }
}