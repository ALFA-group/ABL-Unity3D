using System;

namespace ABLUnitySimulation.Exceptions
{
    public class HandleException : Exception
    {
        public HandleException(string msg) : base(msg)
        {
        }

        public static HandleException CreateNotFound<T>(Handle<T> handle, SimWorldState state)
            where T : class, ISimObject
        {
            return new HandleException($"Could not find {handle} in sim {state}");
        }
    }
}