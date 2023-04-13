using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ABLUnitySimulation;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UI.ABLUnitySimulation;
using UI.InspectorDataRefs;
using UI.Planner;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.GP;
using Utilities.Unity;

#nullable enable

namespace GP.Experiments
{
    public class GpExperimentRunner : MonoBehaviour
    {
        public bool clearPathfindingCacheBetweenExperiments = true;
        public RefSimWorldState? refSimWorldState;
        public bool multithread = true;

        private CancellationTokenSource? _cancelToken;
        public GpExperimentProgress? progress;

        [Button("Run All Experiments"), PropertyOrder(-1)]
        public void RunAllExperimentsWrapper()
        {
            this.CancelGp();
            this._cancelToken = new CancellationTokenSource();
            this.RunAllExperiments(this.transform, this._cancelToken).Forget();
        }

        [Button("Cancel All Experiments"), PropertyOrder(-1)]
        public void CancelGp()
        {
            this._cancelToken?.Cancel();
        }

        public async UniTaskVoid RunAllExperiments(
            Transform componentToSearch,
            CancellationTokenSource cancelTokenSource)
        {
            if (cancelTokenSource == null) throw new InvalidOperationException("Cancel token is null");
            if (null == this.refSimWorldState)
                throw new InvalidOperationException($"{nameof(this.refSimWorldState)} is null");

            var experimentInputsList = GetAllChildGpExperimentData(componentToSearch).ToList();

            if (experimentInputsList.Count == 0)
                CustomPrinter.PrintLine("There are no active experiments in the hierarchy to run.");

            // There can only be one ManyWorldsPlannerRunner in this scene, so we just grab the first one in the list of experiments.
            ManyWorldsPlannerRunner? manyWorldsPlannerRunner;
            if (null != (manyWorldsPlannerRunner = experimentInputsList.First().manyWorldsPlannerRunner))
                manyWorldsPlannerRunner.SetWaypointsAsCircles();

            this.progress = new GpExperimentProgress
            {
                totalExperimentsCount = experimentInputsList.Count,
                status = "Started"
            };
            
            var simWorldState = InspectorDataRef<SimWorldState>.Fetch(this.refSimWorldState) ?? 
                                throw new Exception($"{nameof(this.refSimWorldState)} data could not be found");

            // if (multithread) await UniTaskUtility.SwitchToThread();

            // Run experiments sequentially
            for (int i = 0; i < experimentInputsList.Count; i++)
            {
                this.progress.currentExperimentName = experimentInputsList[i].experimentName;
                if (this.clearPathfindingCacheBetweenExperiments) PathfindingWrapper.ClearCache();
                
                await this.RunOneExperiment(experimentInputsList[i], this.progress, cancelTokenSource);
                
                RefSimWorldState.Set(this.refSimWorldState, simWorldState);

                CustomPrinter.PrintLine($"Completed: {i + 1}/{experimentInputsList.Count}");
                this.progress.totalExperimentsCompleted++;
            }

            if (null != this.progress)
            {
                this.progress.status = cancelTokenSource.IsCancellationRequested ? "Canceled" : "DONE";
                this.progress = null;
            }
        }

        public async UniTask<GeneratedPopulations[]> RunOneExperiment(
            GpExperimentInputs experimentInputs,
            GpExperimentProgress currentProgress,
            CancellationTokenSource cancellationTokenSource)
        {
            if (experimentInputs.manyWorldsPlannerRunner != null)
                experimentInputs.manyWorldsPlannerRunner.SetWaypointsAsCircles();
            
            if (this.multithread) await UniTaskUtility.SwitchToThread();
            var simWorldState = InspectorDataRef<SimWorldState>.Fetch(this.refSimWorldState);
            var allGeneratedPopulations = await this.GetResultsFromGpRuns(
                experimentInputs,
                simWorldState,
                currentProgress,
                cancellationTokenSource);
            
            var startTime = allGeneratedPopulations.First().startTime;
            var endTime = allGeneratedPopulations.Last().endTime;
            Debug.Assert(startTime == allGeneratedPopulations.Select(p => p.startTime).Min());
            Debug.Assert(endTime == allGeneratedPopulations.Select(p => p.endTime).Max());

            if (this.multithread) await UniTask.SwitchToMainThread();
            
            var gpExperimentResults = GetAndPrintResults(
                allGeneratedPopulations,
                startTime, 
                endTime,
                experimentInputs);

            experimentInputs.experiment.AddResults(gpExperimentResults);
           
           return allGeneratedPopulations;
        }

        public async UniTask<GeneratedPopulations[]> GetResultsFromGpRuns(
            GpExperimentInputs experimentInputs,
            SimWorldState? simWorldState,
            GpExperimentProgress experimentProgress,
            CancellationTokenSource cancelTokenSource)
        {
            var subtasks = new List<UniTask<GeneratedPopulations>>();

            if (null == experimentInputs.experiment.parameters.gpParameters) throw new Exception("No GpParameters found");

            // await UniTaskUtility.SwitchToThread(); 
            
            //
            // if (experimentInputs.experiment.parameters.miscParameters.multiThread)
            // {
            //     var timeoutInfo = new TimeoutInfo
            //     {
            //         cancelTokenSource = cancelTokenSource,
            //         ignoreGenerationsUseTimeout = experimentInputs.experiment.parameters.miscParameters.ignoreGenerationsUseTimeout,
            //         runStartTime = DateTime.Now,
            //         timelimitInSeconds =  experimentInputs.experiment.parameters.miscParameters.timeLimitPerRunInSeconds 
            //     };
            //
            //     
            //     // Launch all subtasks
            //     for (var i = 0; i < experimentInputs.experiment.parameters.miscParameters.numberOfRuns; i++)
            //     {
            //
            //         // Reset SimWorldState
            //         var runner = experimentInputs.experiment.parameters.gpParameters.GetGp(
            //                          simWorldState?.DeepCopy(), experimentInputs.simEvaluationParameters,
            //                          timeoutInfo,
            //                          experimentInputs.manyWorldsPlannerRunner, 
            //                          experimentInputs.experiment.parameters.miscParameters.multiThread) 
            //                      ?? throw new Exception("A GpRunner could not be created.");
            //         
            //         if (timeoutInfo.ShouldTimeout) break;
            //         subtasks.Add(runner.RunAsync(experimentProgress));
            //     }
            //
            //     if (experimentInputs.experiment.parameters.gpParameters.verbose) CustomPrinter.PrintLine("Tasks built");
            //
            //     var results = await UniTask.WhenAll(subtasks);
            //     return results;
            //
            // }

            // Otherwise, run experiments sequentially
            var allResults = new GeneratedPopulations[experimentInputs.experiment.parameters.miscParameters.numberOfRuns];
            for (var i = 0; i < experimentInputs.experiment.parameters.miscParameters.numberOfRuns; i++)
            {
                var timeoutInfo = new TimeoutInfo
                {
                    cancelTokenSource = cancelTokenSource,
                    ignoreGenerationsUseTimeout = experimentInputs.experiment.parameters.miscParameters.ignoreGenerationsUseTimeout,
                    runStartTime = DateTime.Now,
                    timeLimitInSeconds =  experimentInputs.experiment.parameters.miscParameters.timeLimitPerRunInSeconds 
                };
                
                // Reset SimWorldState
                var runner = experimentInputs.experiment.parameters.gpParameters.GetGp(
                                 simWorldState?.DeepCopy(), experimentInputs.simEvaluationParameters, 
                                 timeoutInfo,
                                 experimentInputs.manyWorldsPlannerRunner) 
                             ?? throw new Exception("A GpRunner could not be created.");

                Debug.Log($"Starting GP experiment run #{i}");
                
                if (experimentInputs.experiment.parameters.miscParameters.clearPathfindingCacheBetweenRuns) PathfindingWrapper.ClearCache();
                var results = await runner.RunAsync(experimentProgress, this.multithread);
                allResults[i] = results;
                
                if (null != simWorldState) RefSimWorldState.Set(this.refSimWorldState, simWorldState);
                if (null != experimentInputs.experiment.progress) 
                    experimentInputs.experiment.progress.runsInExperimentCompleted++;
            }
            
            Debug.Log($"All experiments finished");

            return allResults;
        }

        private static GpExperimentResults GetAndPrintResults(
            GeneratedPopulations[] allPopulations,
            DateTime start,
            DateTime end,
            GpExperimentInputs experimentInputs)
        {
            var allBestEvers = allPopulations.Select(p => p.GetBestEver()).ToList();
            var results = new GpExperimentResults(
                SceneManager.GetActiveScene().name,
                experimentInputs.experiment.gameObject.GetGameObjectPath(),
                start,
                end,
                numRuns: experimentInputs.experiment.parameters.miscParameters.numberOfRuns,
                succeeded: allBestEvers.Count(
                    individual => individual?.fitness?.TotalFitness >= 
                                    experimentInputs.experiment.parameters.miscParameters.minimumFitnessNeededToSucceed),
                fitnessSummary: FitnessStats.GetDetailedSummary(allPopulations),
                parameters: experimentInputs.experiment.parameters,
                evolvedPopulationsAcrossRuns: allPopulations,
                plannerParameters: experimentInputs.manyWorldsPlannerRunner == null
                    ? null
                    : experimentInputs.manyWorldsPlannerRunner.plannerParameters,
                simEvaluationParameters: experimentInputs.simEvaluationParameters,
                simCreator: experimentInputs.simCreator
            );

            if (experimentInputs.resultInformationType == ResultInformationType.Stats ||
                experimentInputs.resultInformationType == ResultInformationType.StatsAndSucceeded)
                CustomPrinter.PrintLine(results.fitnessSummary);

            if (experimentInputs.resultInformationType == ResultInformationType.Succeeded ||
                experimentInputs.resultInformationType == ResultInformationType.StatsAndSucceeded)
            {
                double elapsed = (results.end - results.start).TotalMilliseconds;
                CustomPrinter.PrintLine($"{(int)elapsed}ms: Number of times {experimentInputs.experimentName} " +
                                        $"succeeded: {results.succeeded}/{allBestEvers.Count}"); 
            }

            return results;
        }

        private static IEnumerable<GpExperimentInputs> GetAllChildGpExperimentData(Transform componentToSearch)
        {
            foreach (Transform child in componentToSearch)
            {
                // Don't run experiments that are disabled in the hierarchy
                if (!child.gameObject.activeInHierarchy) continue;

                if (child.GetComponent<GpExperiment>() == null)
                {
                    foreach (var childData in GetAllChildGpExperimentData(child)) yield return childData;
                }
                else
                {
                    var childExperiment = child.gameObject.GetComponent<GpExperiment>() ??
                                          throw new Exception("No GP experiment found");

                    yield return new GpExperimentInputs(
                        childExperiment,
                        child.gameObject.GetGameObjectPath(),
                        childExperiment.parameters.miscParameters.resultInformationType,
                        FindObjectOfType<ManyWorldsPlannerRunner>(), 
                        FindObjectOfType<SimCreator>(),
                        FindObjectOfType<SimEvaluationParametersHolder>().simEvaluationParameters ?? 
                            throw new InvalidOperationException("Sim evaluation parameters is null")
                    );
                }
            }
        }
    }
}