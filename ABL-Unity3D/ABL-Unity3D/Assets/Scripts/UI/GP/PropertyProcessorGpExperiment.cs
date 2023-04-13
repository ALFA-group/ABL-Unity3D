using System.Collections.Generic;
using GP.Experiments;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace UI.GP.Editor
{
    // [UsedImplicitly]
    public class PropertyProcessorGpExperiment : OdinPropertyProcessor<GpExperiment>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            if (this.Property.ValueEntry.WeakSmartValue is GpExperiment experiment)
                propertyInfos.AddDelegate("Display Progress",
                    () => { GpProgressDialog.CreateAndGo(experiment.progress!); },
                    new ButtonGroupAttribute(),
                    new PropertyOrderAttribute(-1),
                    new ShowIfAttribute("@null != progress"));
        }
    }
}