using JetBrains.Annotations;
using UnityEngine;

namespace GP.Experiments
{
    public class GpExperimentNotes : MonoBehaviour
    {
        [TextArea, UsedImplicitly] public string experimentNotes;
    }
}