using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UI.OdinHelpers;

#nullable enable

namespace UI.ABLUnitySimulation.Editor
{
    [UsedImplicitly]
    public class PropertyProcessorSimAgentFollower : OdinPropertyProcessor<SimAgentFollower>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            propertyInfos.AddValue("Total Entities",
                (ref SimAgentFollower suf) => $"{suf.myAgent?.TotalCurrentHealth()}/{suf.myAgent?.TotalMaxHealth()}",
                new PropertyOrderAttribute(-10)
            );

            propertyInfos.AddValue("Entity Breakdown",
                GetBreakdown,
                (ref SimAgentFollower _, string __) => { },
                new PropertyOrderAttribute(-10)
            );
        }

        private static string GetBreakdown(ref SimAgentFollower follower)
        {
            return null == follower.myAgent ? "" : follower.myAgent.GetHumanReadableEntityCounts();
        }
    }
}