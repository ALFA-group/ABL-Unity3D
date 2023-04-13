using System.Collections.Generic;
using ABLUnitySimulation;

namespace Planner
{
    public static class PlannerExtensions
    {
        public static MethodAny<TSpec> Any<TSpec>(this TSpec t) where TSpec : struct, ITaskSpec
        {
            return new MethodAny<TSpec>(t);
        }

        public static SimGroup ToSimGroup(this IEnumerable<SimAgent> groupMembers)
        {
            return new SimGroup(groupMembers);
        }
        
        public static SimGroup ToSimGroup(this IEnumerable<Handle<SimAgent>> groupMembers)
        {
            return new SimGroup(groupMembers);
        }
    }
}