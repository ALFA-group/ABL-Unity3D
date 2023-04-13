using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.SimScoringFunction;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UI.ABLUnitySimulation;
using UnityEngine;
// ReSharper disable ConvertToConstant.Global

#nullable enable

namespace GP.Experiments
{
    [HideReferenceObjectPicker]
    public class MiscParameters
    {
        [MinValue(1)]
        public readonly int numberOfRuns = 1;

        public readonly bool clearPathfindingCacheBetweenRuns = false;
        
        public readonly bool ignoreGenerationsUseTimeout = false;
        
        [ShowIf("$ignoreGenerationsUseTimeout"), Indent, MinValue(1)]
        public readonly int timeLimitPerRunInSeconds = 10;

        [LabelText("Display Number Succeeded And/Or Other Relevant Stats")]
        public readonly ResultInformationType resultInformationType = ResultInformationType.Stats;
        
        [ShowIf(
            "@resultInformationType == ResultInformationType.Succeeded || resultInformationType == ResultInformationType.StatsAndSucceeded")]
        [Indent]
        public readonly double minimumFitnessNeededToSucceed = -1;
    }

    [HideReferenceObjectPicker]
    [Serializable]
    public class GpExperimentParameters
    {
        // null! is fine because Odin will always initialize it using the OnInspectorInit attribute
        // ReSharper disable once NullableWarningSuppressionIsUsed
        [OnInspectorInit("InitGpParameters")] public GpParameters gpParameters = null!;

        [OnInspectorInit("InitMiscParameters")]
        // null! is fine because Odin will always initialize it using the OnInspectorInit attribute
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public MiscParameters miscParameters = null!;

        public void InitGpParameters()
        {
            // ReSharper disable once ConstantNullCoalescingCondition
            this.gpParameters ??= new GpParameters();
        }

        public void InitMiscParameters()
        {
            // ReSharper disable once ConstantNullCoalescingCondition
            this.miscParameters ??= new MiscParameters();
        }
    }
}