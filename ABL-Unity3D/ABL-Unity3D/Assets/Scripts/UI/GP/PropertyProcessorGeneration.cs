using System.Collections.Generic;
using GP;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace UI.GP
{
    [UsedImplicitly]
    public class PropertyProcessorGeneration : OdinPropertyProcessor<Generation>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            propertyInfos.Clear();

            void BogusSetter(ref Generation g, Individual _)
            {
            }

            void BogusSetterGeneration(ref Generation g, List<Individual> _) { }
            
            
            propertyInfos.AddValue("Best Individual",
                (ref Generation g) => g.Best,
                BogusSetter,
                new BoxGroupAttribute("Best Individual"), new HideLabelAttribute());

            propertyInfos.AddValue("Median Individual",
                (ref Generation g) => g.Median,
                BogusSetter,
                new BoxGroupAttribute("Median Individual"), new HideLabelAttribute());

            propertyInfos.AddValue("Worst Individual",
                (ref Generation g) => g.Worst,
                BogusSetter,
                new BoxGroupAttribute("Worst Individual"), new HideLabelAttribute());
            
            
            propertyInfos.AddValue("All Individuals",
                (ref Generation g) => g.population,
                BogusSetterGeneration);
            
        }
    }
}