using ABLUnitySimulation;

namespace Planner.ManyWorlds
{
    public static class ManyWorldsHelper
    {
        public static MethodLibrary GetMethodLibrary(this SimWorldState state)
        {
            if (!(state.methodLibrary is MethodLibrary library))
            {
                library = MethodLibrary.FromReflection();
                state.methodLibrary = library;
            }

            return library;
        }
        
    }
}