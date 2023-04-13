using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABLUnitySimulation.SimScoringFunction.Average.Health;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

#nullable enable




namespace ABLUnitySimulation.SimScoringFunction
{
    /// <summary>
    ///     An abstract class used to help define criterion to be used with a <see cref="SimScoringFunction" />.
    /// </summary>
    public abstract class SimScoringCriterion
    {
        public abstract string Title { get; }
        
        // ReSharper disable once MemberCanBeProtected.Global
        public virtual string Info => "";

        /// <summary>
        ///     Get the score for this particular criterion in regards to the given world state.
        /// </summary>
        /// <param name="state">The world state to evaluate this criterion on.</param>
        /// <returns>The score for this criterion on the given world state.</returns>
        public abstract double CalculateScore(SimWorldState state);

        public override string ToString()
        {
            if (this.Title != string.Empty) return this.Title;
            if (this.Info != string.Empty) return this.Info;
            return this.GetType().Name;
        }

        public virtual SimScoringCriterion DeepCopy()
        {
            return (SimScoringCriterion)this.MemberwiseClone();
        }
    }

    /// <summary>
    ///     A wrapper class for holding a <see cref="SimScoringCriterion" /> and a weight associated with it.
    /// </summary>
    public struct SimScoringCriterionAndWeight
    {
        /// <summary>
        ///     The weight associated with the given criterion.
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public double weight;

        /// <summary>
        ///     The criterion to be used for a scoring function.
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public SimScoringCriterion criterion;
        
        public SimScoringCriterionAndWeight(double weight, SimScoringCriterion criterion, bool isOdinReadOnly = false)
        {
            this.weight = weight;
            this.criterion = criterion;
        }

        public SimScoringCriterionAndWeight DeepCopy()
        {
            return new SimScoringCriterionAndWeight(this.weight, this.criterion.DeepCopy());
        }
    }

    /// <summary>
    ///     A function for assigning scores to simulation world states
    ///     uses a set of given scoring criterion and weights associated with them.
    /// </summary>
    [HideReferenceObjectPicker]
    public class SimScoringFunction
    {
        /// <summary>
        ///     The list of scoring criterion and weights associated with them.
        /// </summary>
        [OnInspectorInit("InitCriteriaAndWeights")]
        public List<SimScoringCriterionAndWeight>? criteriaAndWeights;

        [HideInInspector] public string name = "?";

        public SimScoringFunction(string name, List<SimScoringCriterionAndWeight> criteriaAndWeights)
        {
            this.criteriaAndWeights = criteriaAndWeights;
            this.name = name;
        }

        public SimScoringFunction(List<SimScoringCriterionAndWeight> criteriaAndWeights)
        {
            this.criteriaAndWeights = criteriaAndWeights;
        }

        // ReSharper disable once UnusedMember.Global
        public void InitCriteriaAndWeights()
        {
            this.criteriaAndWeights ??= new List<SimScoringCriterionAndWeight>();
        }

        /// <summary>
        ///     Returns the total score for a given world state using the defined scoring criterion and weights.
        /// </summary>
        /// <param name="state">The world state to evaluate.</param>
        /// <param name="breakdownDescription">If not null, adds a human-readable scoring breakdown to the string builder.</param>
        /// <returns>A score for the given world state.</returns>
        public double EvaluateSimWorldState(SimWorldState state, StringBuilder? breakdownDescription = null)
        {
            if (null == this.criteriaAndWeights) return 0;

            double sum = 0;
            foreach (var criterionAndWeight in this.criteriaAndWeights)
            {
                double weight = criterionAndWeight.weight;
                double score = criterionAndWeight.criterion.CalculateScore(state);

                breakdownDescription?.AppendLine(
                    $"{weight * score}={weight:0.00}w * {score}s for {criterionAndWeight.criterion.Title}");

                Debug.Assert(!double.IsNaN(score));
                Debug.Assert(!double.IsNaN(weight));

                sum += weight * score;
            }

            return sum;
        }

        public SimScoringFunction DeepCopy()
        {
            var cloneCriteria = this.criteriaAndWeights?
                .Select(caw => caw.DeepCopy())
                .ToList();

            var clone = new SimScoringFunction(this.name, cloneCriteria ?? new List<SimScoringCriterionAndWeight>());
            return clone;
        }

        public static SimScoringFunction CreateDefaultScoringFunction()
        {
            var list = new List<SimScoringCriterionAndWeight>
                { new SimScoringCriterionAndWeight(1, new AverageTeamHealthFriendly(), false) };
            return new SimScoringFunction("AvoidDamage", list);
        }
    }
}