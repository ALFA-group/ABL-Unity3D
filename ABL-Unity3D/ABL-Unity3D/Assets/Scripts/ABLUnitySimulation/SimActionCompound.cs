using System;
using Utilities.GeneralCSharp;

#nullable enable

namespace ABLUnitySimulation
{
    /// <summary>
    ///     A <see cref="SimAction" /> that should use other <see cref="SimAction" />s.
    ///     Essentially a composition of one or more <see cref="SimAction" />s.
    /// </summary>
    // [Serializable]
    public abstract class SimActionCompound : SimAction
    {
        public readonly long key = ThreadSafeRandom.Next();
    }
}