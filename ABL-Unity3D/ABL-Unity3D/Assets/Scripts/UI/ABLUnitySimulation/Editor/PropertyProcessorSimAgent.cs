using System.Collections.Generic;
using ABLUnitySimulation;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UI.OdinHelpers;

namespace UI.ABLUnitySimulation.Editor
{
    [UsedImplicitly]
    public class PropertyProcessorSimAgent : OdinPropertyProcessor<SimAgent>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            propertyInfos.AddValue("Health Summary", (ref SimAgent agent) => agent.GetTotalHealthString(),
                new PropertyOrderAttribute(-10));
            propertyInfos.AddValue("Is Worth Shooting?", (ref SimAgent agent) => agent.IsWorthShooting(),
                new PropertyOrderAttribute(-10));
        }
    }
}