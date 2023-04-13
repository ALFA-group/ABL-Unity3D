using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

#nullable enable

namespace GP.Experiments
{
    [ReadOnly]
    public class GpExperimentProgress
    {
        [UsedImplicitly] public string? currentExperimentName;

        [ProgressBar(0, "generationsInRunCount"), UsedImplicitly]
        public int generationsInRunCompleted;

        [UsedImplicitly] public int generationsInRunCount;


        [ProgressBar(0, "runsInExperimentCount"), UsedImplicitly]
        public int runsInExperimentCompleted;

        [UsedImplicitly] public int runsInExperimentCount;

        [UsedImplicitly] public string status = "Running";

        [ProgressBar(0, "totalExperimentsCount"), UsedImplicitly]
        public int totalExperimentsCompleted; // how many experiments are done?

        [UsedImplicitly] public int totalExperimentsCount; // how many experiments are we running?
    }
}