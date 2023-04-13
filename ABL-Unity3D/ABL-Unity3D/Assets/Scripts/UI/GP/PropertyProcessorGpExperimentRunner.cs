using System.Collections.Generic;
using GP.Experiments;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

#nullable enable

namespace UI.GP.Editor
{
    // [UsedImplicitly]
    public class PropertyProcessorGpExperimentRunner : OdinPropertyProcessor<GpExperimentRunner>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            if (this.Property.ValueEntry.WeakSmartValue is GpExperimentRunner runner)
                propertyInfos.AddDelegate("Display Progress",
                    () => { GpProgressDialog.CreateAndGo(runner.progress!); },
                    new ButtonGroupAttribute(),
                    new PropertyOrderAttribute(-1),
                    new ShowIfAttribute("@null != progress"));
        }
    }
}