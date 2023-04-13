using System.Collections.Generic;

namespace ABLUnitySimulation.Actions
{
    public interface ISimActionCompound
    {
        public IEnumerable<SimAction> EnumerateCurrentActions(SimWorldState state);
        public void UpdateForExternalSimChange(SimWorldState state);
    }
}