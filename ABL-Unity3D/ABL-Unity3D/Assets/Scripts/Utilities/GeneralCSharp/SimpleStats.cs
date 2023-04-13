using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Utilities.GeneralCSharp
{
    public class SimpleStats
    {
        private readonly List<double> _samples; 

        public SimpleStats(IEnumerable<double> samples)
        {
            this._samples = samples.ToList();
        }

        public SimpleStats()
        {
            this._samples = new List<double>();
        }

        public bool HasSamples => this._samples.Count > 0;
        public int NumSamples => this._samples.Count;

        public double Min => this.HasSamples ? this._samples.Min() : 0;
        public double Max => this.HasSamples ? this._samples.Max() : 0;
        public double StandardDeviation => Math.Sqrt(this.Variance);

        public double Mean
        {
            get
            {
                if (!this.HasSamples) return 0;

                double sum = this._samples.Sum();
                return sum / this._samples.Count;
            }
        }

        public double Variance
        {
            get
            {
                if (this._samples.Count < 2) return 0;

                double mean = this.Mean;
                double sum = this._samples.Sum(sample => (sample - mean) * (sample - mean));
                return sum / (this._samples.Count - 1);
            }
        }

        public void Add(double newSample)
        {
            this._samples.Add(newSample);
        }

        public void Add(IEnumerable<double> newSamples)
        {
            this._samples.AddRange(newSamples);
        }

        public Summary GetSummary()
        {
            return new Summary
            {
                numSamples = this._samples.Count,
                min = this.Min,
                max = this.Max,
                mean = this.Mean,
                variance = this.Variance,
                standardDeviation = this.StandardDeviation
            };
        }

        [Serializable]
        public struct Summary
        {
            [DisplayAsString]
            public int numSamples;
            [DisplayAsString]
            public double min;
            [DisplayAsString]
            public double mean;
            [DisplayAsString]
            public double max;
            [DisplayAsString]
            public double variance;
            [DisplayAsString]
            public double standardDeviation;

            public string GetString(int indent = 0) 
            {
                return GenericUtilities.Indent(indent) +
                       $"N: {this.numSamples}\n" +
                       GenericUtilities.Indent(indent) + $"Min: {this.min}\n" +
                       GenericUtilities.Indent(indent) + $"Mean: {this.mean}\n" +
                       GenericUtilities.Indent(indent) + $"Max: {this.max}\n" +
                       GenericUtilities.Indent(indent) + $"Variance: {this.variance}\n" +
                       GenericUtilities.Indent(indent) + $"STD: {this.standardDeviation}";
            }
        }
    }
}