using System.Collections.Generic;
using System.Linq;

namespace Planner
{
    public class Decomposition
    {
        public enum ExecutionMode
        {
            Sequential,
            Parallel
        }

        public ExecutionMode mode;
        public List<Method> subtasks;

        public Decomposition(ExecutionMode mode)
        {
            this.mode = mode;
            this.subtasks = new List<Method>();
        }

        public Decomposition(ExecutionMode mode, Method onlyMethod)
        {
            this.mode = mode;
            this.subtasks = new List<Method> { onlyMethod };
        }

        public Decomposition(ExecutionMode mode, params Method[] methods)
        {
            this.mode = mode;
            this.subtasks = methods.ToList();
        }

        public Decomposition(ExecutionMode mode, IEnumerable<Method> methods)
        {
            this.mode = mode;
            this.subtasks = methods.ToList();
        }
    }
}