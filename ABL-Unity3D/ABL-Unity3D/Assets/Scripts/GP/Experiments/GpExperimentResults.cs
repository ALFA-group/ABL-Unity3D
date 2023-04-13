using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ABLUnitySimulation;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Planner;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UI.ABLUnitySimulation;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.GeneralCSharp;
using Utilities.GP;
using Utilities.Unity;

#nullable enable

namespace GP.Experiments
{
    public class GpExperimentResults
    {
        [DisplayAsString, UsedImplicitly] public string sceneName;

        [FormerlySerializedAs("objectNameInScene")] [DisplayAsString] public string objectPathInScene;
        
        [DisplayAsString, UsedImplicitly] public string? lastGitCommit;

        [DisplayAsString] public DateTime start;

        [DisplayAsString] public DateTime end;

        [DisplayAsString, FoldoutGroup("$resultString")]
        public int succeeded;

        [DisplayAsString, FoldoutGroup("$resultString")]
        public int numRuns;

        [FoldoutGroup("$resultString")]
        public GpExperimentParameters gpParameters;

        [HideInInspector] public string resultString;

        [OdinSerialize]
        public PlannerParameters.PlannerParametersPureData? plannerParameters;

        // [ReadOnly] 
        
        [UsedImplicitly] public SimEvaluationParameters? simEvaluationParameters;

        public SimInitParameters.SimInitParametersPureData? simInitParameters;

        [FoldoutGroup("$resultString"), NonSerialized, OdinSerialize, 
         ListDrawerSettings(DraggableItems = false, HideRemoveButton = true, 
             ListElementLabelName = "GpExperimentRunNumberString")] 
        // ReadOnly]
        
        public List<GeneratedPopulations> evolvedPopulationsAcrossRuns;

        [FoldoutGroup("$resultString")]
        public FitnessStats.DetailedSummary fitnessSummary;
        
        public GpExperimentResults(
            string sceneName,
            string objectPathInScene,
            DateTime start,
            DateTime end,
            int succeeded,
            int numRuns,
            FitnessStats.DetailedSummary fitnessSummary,
            GpExperimentParameters parameters,
            IEnumerable<GeneratedPopulations> evolvedPopulationsAcrossRuns,
            PlannerParameters? plannerParameters,
            SimEvaluationParameters? simEvaluationParameters,
            SimCreator? simCreator)
        {
            this.sceneName = sceneName;
            this.objectPathInScene = objectPathInScene;
            this.lastGitCommit = Git.LastGitVersion;
            this.start = start;
            this.end = end;
            this.succeeded = succeeded;
            this.numRuns = numRuns;
            this.fitnessSummary = fitnessSummary;
            this.gpParameters = (GpExperimentParameters)SerializationUtility.CreateCopy(parameters);  
            this.evolvedPopulationsAcrossRuns = 
                evolvedPopulationsAcrossRuns.Select(p => new GeneratedPopulations(
                p.DeepCopy().generations.First().population.Take(100).ToList().ToEnumerable().ToList(), p.fitnessSummary, p.startTime, p.endTime, p.bestEver, p.verboseInfo)
                ).ToList();
            this.resultString =
                $"Best Fitness: {this.fitnessSummary.totalFitnessSummary.max}; " +
                $"Start: {this.start}; End: {this.end}, " +
                $"Time Elapsed: {this.end - this.start}";
            this.plannerParameters = plannerParameters?.pureData.DeepCopy();
            if (null != this.plannerParameters) this.plannerParameters.showOnlyCircleCache = true;
            this.simEvaluationParameters = simEvaluationParameters?.DeepCopy();

            if (null != simCreator)
            {
                this.simInitParameters = simCreator.simInitParameters.simInitParametersPureData.DeepCopy();
                this.simInitParameters.showJsonTextCache = true;
                
                
                // Suppose you initialize the scenario through SimCreator.CreateNow,
                // then change the initialization type without calling SimCreator.CreateNow.
                // That would result in the wrong SimInitializationType being stored in the
                // experimental results. This is to account for that.
                if (null != simCreator.mostRecentInitializationType)
                {
                    this.simInitParameters.initializationMethod =
                        (SimInitParameters.SimInitializationType)simCreator.mostRecentInitializationType;
                }
            }
            else
            {
                this.simInitParameters = null;
            }
        }

        
        public GpExperimentResults()
        {
        }

        [Button]
        public void WriteFitnessResultsToJson(string fileName)
        {
            var fitnessResults = this.evolvedPopulationsAcrossRuns.Select(g => g.GetFitnessValuesForAllGenerations()).ToList();
            var json = JsonConvert.SerializeObject(fitnessResults, Formatting.Indented);
            File.WriteAllText(fileName, json);
        }

        [Button]
        public void WriteFlatListOfIndividualsToJson(string fileName)
        {
            var individuals = this.evolvedPopulationsAcrossRuns
                .SelectMany(generations => 
                    generations.generations.SelectMany(g => g.population))
                .WhereNotNull()
                .ToList();
            var json = JsonConvert.SerializeObject(individuals, Formatting.Indented);
            File.WriteAllText(fileName, json);
            
        }
    }
}