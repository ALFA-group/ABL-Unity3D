using System;
using ABLUnitySimulation;
using ABLUnitySimulation.SimScoringFunction;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Utilities.GeneralCSharp;

#nullable enable

namespace GP.FitnessFunctions
{
    [Serializable, HideReferenceObjectPicker]
    public class Fitness : IComparable<Fitness>
    {
        
        [ShowInInspector, OdinSerialize, DisplayAsString]
        public readonly double fitnessWithoutSimScore;

        [ShowInInspector, OdinSerialize, DisplayAsString]
        public readonly double weightedSimScore;

        [OdinSerialize, ShowInInspector, DisplayAsString]
        public double TotalFitness => this.weightedSimScore + this.fitnessWithoutSimScore;

        public string SummaryString =>
            $"Fitness: {this.TotalFitness:F}";//, Weighted Sim Score: {this.weightedSimScore:F}, Fitness Without Sim Score: {this.fitnessWithoutSimScore:F}";

        public Fitness(double fitnessWithoutSimScore, double weightedSimScore)
        {
            this.fitnessWithoutSimScore = fitnessWithoutSimScore;
            this.weightedSimScore = weightedSimScore;
        }

        public override int GetHashCode()
        {
            return GenericUtilities.CombineHashCodes(new[]
                { this.fitnessWithoutSimScore.GetHashCode(), this.weightedSimScore.GetHashCode() });
        }

        public int CompareTo(Fitness other)
        {
            return this.TotalFitness.CompareTo(other.TotalFitness);
        }

        public Fitness DeepCopy()
        {
            return new Fitness(this.fitnessWithoutSimScore, this.weightedSimScore);
        }

        public static Fitness MakeFitness(
            double fitnessWithoutSimScore,
            double simScoringFunctionWeight,
            SimScoringFunction simScoringFunction,
            SimWorldState worldState)
        {
            var newFitnessObject = new Fitness(fitnessWithoutSimScore, simScoringFunctionWeight * simScoringFunction.EvaluateSimWorldState(worldState));
            return newFitnessObject;
        }

        public static Fitness MakeFitnessWithOnlyFitnessScoreWithoutSimScore(double fitness)
        {
            return new Fitness(fitness, 0);
        }

        public Fitness Add(Fitness f)
        {
            return new Fitness(this.fitnessWithoutSimScore + f.fitnessWithoutSimScore,
                this.weightedSimScore + f.weightedSimScore);

        }

        public Fitness Divide(int divisor)
        {
            return divisor == 0 ? this.DeepCopy() : new Fitness(this.fitnessWithoutSimScore / divisor, this.weightedSimScore / divisor);
        }

        public bool Equals(Fitness? otherFitness)
        {
            if (null == otherFitness) return false;
            return Math.Abs(this.weightedSimScore - otherFitness.weightedSimScore) < 0.00001 &&
                   Math.Abs(this.fitnessWithoutSimScore - otherFitness.fitnessWithoutSimScore) < 0.00001;
        }
    }
}