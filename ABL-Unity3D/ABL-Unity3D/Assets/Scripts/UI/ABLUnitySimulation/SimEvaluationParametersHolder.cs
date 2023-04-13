using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ABLUnitySimulation;
using ABLUnitySimulation.SimScoringFunction;
using ABLUnitySimulation.SimScoringFunction.Average.Health;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UI.Planner;
using UnityEngine;

#nullable enable

namespace UI.ABLUnitySimulation
{
    public class SimEvaluationParametersHolder : SerializedMonoBehaviour
    {
        public SimEvaluationParameters? simEvaluationParameters;

        public void OnValidate()
        {
            this.simEvaluationParameters ??= new SimEvaluationParameters();

            if (null == this.simEvaluationParameters?.primaryScoringFunction?.criteriaAndWeights ||
                this.simEvaluationParameters.primaryScoringFunction.criteriaAndWeights.Count < 1)
            {
                this.simEvaluationParameters!.primaryScoringFunction = SimScoringFunction.CreateDefaultScoringFunction();
            }
               
        }

        public void SetBestAlternatePlans(CancellationToken cancel, PlanStorage planStorage)
        {
            var start = DateTime.UtcNow;
            var planRecords = planStorage.planRecords;

            if (null == this.simEvaluationParameters) throw new Exception("Sim Parameters is null");
            
            foreach (var alternateScoringFunction in this.simEvaluationParameters.alternateScoringFunctions)
            {
                SetAlternatePlanScores(cancel,
                    alternateScoringFunction,
                    planRecords);
            }

            var end = DateTime.UtcNow;
            double ms = (end - start).TotalMilliseconds;
            Debug.Log($"... DONE scoring alternate plans in {(int)ms}ms");
        }
        
        
        private static void SetAlternatePlanScores(CancellationToken cancel,
            SimScoringFunction alternateScoringFunction,
            List<PlanStorage.PlanRecord> planRecords)
        {

            foreach (var planRecord in planRecords)
            {
                if (null != planRecord.plan)
                {
                    double planScore =
                        alternateScoringFunction.EvaluateSimWorldState(planRecord.plan.planTimeStateSoFar);

                    planRecord.alternateScores.Add(
                        new PlanStorage.PlanRecord.Score(alternateScoringFunction.DeepCopy(), planScore));
                }
            }

        }
    }
}