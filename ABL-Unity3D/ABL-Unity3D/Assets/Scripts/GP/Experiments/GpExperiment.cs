using System;
using System.Collections.Generic;
using System.Threading;
using ABLUnitySimulation;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UI.ABLUnitySimulation;
using UI.GP;
using UI.GP.Editor;
using UI.Planner;
using UnityEngine;
using Utilities.Unity;

#nullable enable

namespace GP.Experiments
{
    public enum ResultInformationType
    {
        Succeeded,
        Stats,
        StatsAndSucceeded
    }

    
    public readonly struct GpExperimentInputs 
    {
        public readonly GpExperiment experiment;
        public readonly string experimentName;
        public readonly ResultInformationType resultInformationType;
        public readonly ManyWorldsPlannerRunner? manyWorldsPlannerRunner;
        public readonly SimCreator? simCreator;
        public readonly SimEvaluationParameters simEvaluationParameters;

        public GpExperimentInputs(
            GpExperiment experiment,
            string experimentName,
            ResultInformationType resultInformationType,
            ManyWorldsPlannerRunner? manyWorldsPlannerRunner,
            SimCreator? simCreator,
            SimEvaluationParameters simEvaluationParameters)
        {
            this.experiment = experiment;
            this.experimentName = experimentName;
            this.resultInformationType = resultInformationType;
            this.manyWorldsPlannerRunner = manyWorldsPlannerRunner;
            this.simCreator = simCreator;
            this.simEvaluationParameters = simEvaluationParameters;
        }
    }

    public class GpExperiment : SerializedMonoBehaviour
    {
        [NonSerialized, OdinSerialize]
        public List<GpExperimentResultsScriptableObject> resultsFromPreviousRuns =
            new List<GpExperimentResultsScriptableObject>();
        
        [TextArea, UsedImplicitly] public string experimentNotes = "";

        [NonSerialized] public GpExperimentProgress? progress;

        public readonly GpExperimentParameters parameters = new GpExperimentParameters();

        private CancellationTokenSource? _cancelToken;
        

        [Button("Run This Experiment")]
        [PropertyOrder(-1)]
        public async void RunThisExperiment()
        {
            this._cancelToken = new CancellationTokenSource();
            var gpRunData = new GpExperimentInputs(
                experimentName: this.gameObject.GetGameObjectPath(),
                experiment: this,
                resultInformationType: this.parameters.miscParameters.resultInformationType,
                manyWorldsPlannerRunner: FindObjectOfType<ManyWorldsPlannerRunner>(),
                simCreator: FindObjectOfType<SimCreator>(),
                simEvaluationParameters: FindObjectOfType<SimEvaluationParametersHolder>().simEvaluationParameters 
                                         ?? throw new InvalidOperationException("Sim evaluation parameters ia null.")
            );

            if (null != gpRunData.manyWorldsPlannerRunner) gpRunData.manyWorldsPlannerRunner.SetWaypointsAsCircles();
            var runner = this.GetComponentInParent<GpExperimentRunner>() ??
                         throw new Exception("No Gp Experiment Runner found in parents");
            
            this.progress = new GpExperimentProgress
            {
                totalExperimentsCount = 1,
                currentExperimentName = this.name,
                runsInExperimentCount = this.parameters.miscParameters.numberOfRuns
            };

            await runner.RunOneExperiment(gpRunData, this.progress, this._cancelToken);
        }

        [Button("Cancel This Experiment")]
        [PropertyOrder(-2)]
        public void CancelThisExperiment()
        {
            this._cancelToken?.Cancel();
            this.EndProgress("Cancelled");
        }

        private void EndProgress(string status)
        {
            if (null != this.progress) this.progress.status = status;

            this.progress = null;
        }


        public void AddResults(GpExperimentResults results)
        {
            var so = GpExperimentResultsScriptableObject.Create(results);
            this.resultsFromPreviousRuns.Add(so);
        }
    }
}