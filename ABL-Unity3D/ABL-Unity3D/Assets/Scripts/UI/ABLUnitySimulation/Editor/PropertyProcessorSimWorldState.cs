using System.Collections.Generic;
using ABLUnitySimulation;
using Sirenix.OdinInspector.Editor;

namespace UI.ABLUnitySimulation.Editor
{
    // [UsedImplicitly]
    public class PropertyProcessorSimWorldState : OdinPropertyProcessor<SimWorldState>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            propertyInfos.Clear();
        }
    }
}