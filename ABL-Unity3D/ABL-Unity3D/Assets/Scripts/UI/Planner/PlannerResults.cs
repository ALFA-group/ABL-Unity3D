using System;
using System.Linq;
using ABLUnitySimulation;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Planner;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Unity;

#nullable enable



namespace UI.Planner
{
    [Serializable]
    public class PlannerResults
    {
        [DisplayAsString, UsedImplicitly] public string sceneName;
        
        [DisplayAsString] public string objectPathInScene;
        
        [DisplayAsString, UsedImplicitly] public string? lastGitCommit;
        
        [DisplayAsString] public DateTime start;
        
        [DisplayAsString] public DateTime end;
        
        [HideInInspector] public string resultString;
        
        [UsedImplicitly] public PlannerParameters.PlannerParametersPureData plannerParameters;
        
        
        
        [UsedImplicitly] public SimEvaluationParameters simEvaluationParameters;
        
        [UsedImplicitly]
        public SimInitParameters.SimInitParametersPureData simInitParameters;

        public PlanStorage results;

        [JsonConstructor]
        public PlannerResults()
        {
            
        }

        public PlannerResults(
            string sceneName, string objectPathInScene, 
            PlannerParameters plannerParameters, SimEvaluationParameters simEvaluationParameters, 
            SimInitParameters simInitParameters, PlanStorage results,
            DateTime startTime, DateTime endTime)
        {
            this.sceneName = sceneName;
            this.objectPathInScene = objectPathInScene;
            this.plannerParameters = plannerParameters.pureData.DeepCopy();
            this.plannerParameters.showOnlyCircleCache = true;
            this.simEvaluationParameters = simEvaluationParameters.DeepCopy();
            this.simInitParameters = simInitParameters.simInitParametersPureData.DeepCopy();
            this.simInitParameters.showJsonTextCache = true;
            this.results = results.DeepCopy();
            this.start = startTime;
            this.end = endTime;
            this.lastGitCommit = Git.LastGitVersion;
            this.resultString =
                $"Best Score: {this.results.planRecords.First().score}; " +
                $"Start: {this.start}; End: {this.end}, " +
                $"Time Elapsed: {this.end - this.start}";
        }
    }
}