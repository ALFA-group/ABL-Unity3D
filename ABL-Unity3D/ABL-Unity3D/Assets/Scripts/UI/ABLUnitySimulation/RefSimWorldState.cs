using ABLUnitySimulation;
using UI.InspectorDataRefs;

#nullable enable

namespace UI.ABLUnitySimulation
{
    /// <summary>
    ///     MonoBehaviour to hold a simulator world state. Allows the simulator world state to be referenced
    ///     from anywhere.
    /// </summary>
    public class RefSimWorldState : InspectorDataRef<SimWorldState>
    {
        /// <summary>
        ///     Return the reference simulator world state used in this Unity Scene file.
        /// </summary>
        /// <returns>The reference simulator world state used in this Unity Scene file.</returns>
        public static SimWorldState? GetGlobalSimWorldState()
        {
            var current = FindObjectOfType<RefSimWorldState>();
            return Fetch(current);
        }
    }
}