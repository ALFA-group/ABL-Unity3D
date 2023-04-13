using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ABLUnitySimulation;
using ABLUnitySimulation.SimScoringFunction;
using JetBrains.Annotations;
using Planner;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UI.ABLUnitySimulation;
using UnityEngine;
using UnityEngine.Assertions;

#nullable enable

namespace UI.Planner
{
    // [UsedImplicitly]
    // [HideReferenceObjectPicker]
    // public class SimScoringFunctionRecord : SimScoringFunction
    // {
    //     [NonSerialized, HideInEditorMode, ShowInInspector, PropertyOrder(-0.5f)]
    //     public PlanStorage.PlanRecord? best;
    //
    //     public SimScoringFunctionRecord(List<SimScoringCriterionAndWeight> criteriaAndWeights) : base(
    //         criteriaAndWeights)
    //     {
    //     }
    // }
    
    /// <summary>
    ///     Holds plans with their associated scores.
    /// </summary>
    [HideReferenceObjectPicker]
    public class PlanStorage
    {
        /// <summary>
        ///     The list of all plans generated and their scores.
        /// </summary>
        public List<PlanRecord> planRecords = new List<PlanRecord>();

        public PlanStorage DeepCopy()
        {
            return new PlanStorage()
            {
                planRecords = this.planRecords.Select(r => r.DeepCopy()).ToList()
            };
        }

        /// <summary>
        ///     Add a plan to this plan storage and evaluate it using the given scoring function.
        /// </summary>
        /// <param name="plan">The plan to add.</param>
        /// <param name="scoringFunction">The scoring function to evaluate this plan with.</param>
        public void AddPlan(Plan plan, SimScoringFunction scoringFunction, SimCreator simCreator)
        {
            var newRecord = CreatePlanRecord(plan, scoringFunction, simCreator);

            this.planRecords.Add(newRecord);
        }

        /// <summary>
        ///     Sort the plan records in order of descending score order.
        /// </summary>
        public void SortRecordsDescending()
        {
            this.planRecords = this.planRecords.OrderByDescending(planRecord => planRecord.score).ToList();
        }

        /// <summary>
        ///     Remove plans which have similar scores to plans already in this plan storage.
        /// </summary>
        /// <param name="newMaxNumPlans">The number of plans to keep.</param>
        public void DropNonNovel(int newMaxNumPlans)
        {
            if (newMaxNumPlans <= 0)
            {
                this.planRecords.Clear();
                return;
            }

            this.DropSomethingSimilar(this.planRecords.Count - newMaxNumPlans);
        }

        /// <summary>
        ///     Remove plans that are similar in score to plans that are already stored.
        /// </summary>
        /// <param name="numToDrop">The number of plans to remove.</param>
        private void DropSomethingSimilar(int numToDrop)
        {
            if (numToDrop <= 0) return;
            if (this.planRecords.Count <= numToDrop)
            {
                this.planRecords.Clear();
                return;
            }

            var isSecondTry = false;

            double epsilon = Mathf.Epsilon;
            double scoreRange = this.planRecords.Max(pr => pr.score) - this.planRecords.Min(pr => pr.score);
            double maxEpsilon = scoreRange * 0.1f;

            var scores = new List<double>();

            while (numToDrop > 0)
            {
                if (this.planRecords.Count <= 0) return;

                scores.Clear();

                for (int index = this.planRecords.Count - 1; numToDrop > 0 && index >= 0; --index)
                {
                    double score = this.planRecords[index].score;

                    if (scores.Any(d => d >= score && score + epsilon >= d))
                    {
                        // Found a similar, but larger score.
                        // Drop this lower score.
                        this.planRecords.RemoveAt(index);
                        --numToDrop;
                    }
                    else
                    {
                        scores.Add(score);
                    }
                }

                if (epsilon < maxEpsilon)
                {
                    epsilon *= 2;
                }
                else
                {
                    Assert.IsFalse(isSecondTry);
                    if (isSecondTry) return; // we have failed and I don't know why.

                    // Can't find any matches at a fairly big epsilon.  Almost certainly means that the plans are sorted in increasing order.
                    this.planRecords.Reverse();
                    epsilon = Mathf.Epsilon;
                    isSecondTry = true;
                }
            }
        }

        /// <summary>
        ///     Helper function to create a <see cref="PlanRecord" />,
        /// </summary>
        /// <param name="plan">The plan to create the <see cref="PlanRecord" /> with.</param>
        /// <param name="scoringFunction">The scoring function to create the <see cref="PlanRecord" /> with.</param>
        /// <returns>The new <see cref="PlanRecord" />.</returns>
        private PlanRecord CreatePlanRecord(Plan plan, SimScoringFunction scoringFunction, SimCreator simCreator)
        {
            var breakdown = new StringBuilder();
            double score = scoringFunction.EvaluateSimWorldState(plan.planTimeStateSoFar, breakdown);

            var record = new PlanRecord
            {
                plan = plan,
                score = score,
                scoringFunctionForScore = scoringFunction.DeepCopy(),
                scoreBreakdown = breakdown.ToString(),
                alternateScores = new List<PlanRecord.Score>(),
            };
            return record;
        }

        /// <summary>
        ///     A wrapper for a plan and its associated scores.
        /// </summary>
        [HideReferenceObjectPicker]
        public struct PlanRecord
        {
            [HideLabel]
            public Plan? plan;

            [ShowInInspector]
            [PropertyOrder(-1)]
            public long PlanRefId => this.plan == null ? 0 : ObjectIDGenerator.GetId(this.plan, out _);

            [HideInInspector]
            public double score;

            [FoldoutGroup("$ScoreString"), 
             MultiLineProperty(10), 
             HideLabel, 
             UsedImplicitly, 
             PreviouslySerializedAs("breakdown")]
            public string? scoreBreakdown;

            public string ScoreString => $"Score {this.score}";
            
            [UsedImplicitly] public string ScoringFunctionString => $"ScoringFunction: {this.scoringFunctionForScore?.name ?? "?"}";

            [LabelText("$ScoringFunctionString")]
            public SimScoringFunction? scoringFunctionForScore;


            public struct Score
            {
                [UsedImplicitly] public double score;

                [LabelText("$ScoringFunctionString")]
                public SimScoringFunction scoringFunction;

                [UsedImplicitly] public string ScoringFunctionString => $"ScoringFunction: {this.scoringFunction.name}";

                public Score(SimScoringFunction scoringFunction, double score)
                {
                    this.scoringFunction = scoringFunction;
                    this.score = score;
                }

                public Score DeepCopy()
                {
                    var copy = new Score(this.scoringFunction.DeepCopy(), this.score);
                    return copy;
                }
            }

            public List<Score> alternateScores;

            private static readonly ObjectIDGenerator ObjectIDGenerator = new ObjectIDGenerator();

            public PlanRecord DeepCopy()
            {
                return new PlanRecord()
                {
                    plan = this.plan?.DeepCopy(),
                    score = this.score,
                    scoreBreakdown = this.scoreBreakdown,
                    alternateScores = this.alternateScores.Select(s => s.DeepCopy()).ToList(),
                    scoringFunctionForScore = this.scoringFunctionForScore?.DeepCopy()
                };
            }
        }
    }
}