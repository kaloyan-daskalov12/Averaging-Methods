using System;
using System.Collections.Generic;
using System.Linq;

namespace AveragingMethods
{
    public class AveragingResult
    {
        public double WeightedMean { get; set; }
        public double InternalUncertainty { get; set; }
        public double ExternalUncertainty { get; set; }
        public double ReducedChiSquared { get; set; }
        public double CriticalChiSquared { get; set; }
        public bool IsDiscrepant { get; set; }
        public bool LwmApplied { get; set; } // Tracks if the 50% rule was triggered
    }

    public class EvaluatedAverage : IAverageMethod
    {
        private static readonly double[] CriticalReducedChiSq95 = new double[]
        {
            0.000, 3.841, 2.996, 2.605, 2.372, 2.214, 2.099, 2.010, 1.938, 1.880, 1.831,
            1.789, 1.752, 1.720, 1.692, 1.666, 1.644, 1.623, 1.604, 1.587, 1.571,
            1.556, 1.543, 1.530, 1.518, 1.507, 1.497, 1.487, 1.478, 1.470, 1.462
        };

        public string Name => "Evaluated Average";

        public AveragingResult CalculateWeightedAverage(List<Value> data)
        {
            var validData = data.Where(v => v.DVal > 0).ToList();
            int n = validData.Count;
            if (n < 2) 
            {
                return new AveragingResult
                {
                    WeightedMean = double.NaN,
                    InternalUncertainty = double.NaN,
                    ExternalUncertainty = double.NaN,
                    ReducedChiSquared = double.NaN,
                    CriticalChiSquared = double.NaN,
                    IsDiscrepant = false,
                    LwmApplied = false 
                };
            } //throw new InvalidOperationException("Need at least 2 data points.");
            int dof = n - 1;

            // 1. Calculate Initial Weights
            List<double> weights = validData.Select(v => 1.0 / (v.DVal * v.DVal)).ToList();
            double sumWeights = weights.Sum();

            // 2. Apply Limitation of Relative Statistical Weights (LWM)
            bool lwmApplied = false;
            double maxWeight = weights.Max();
            if (maxWeight / sumWeights > 0.5)
            {
                lwmApplied = true;
                int maxIndex = weights.IndexOf(maxWeight);

                // Inflate the uncertainty so its weight is exactly 50% of the NEW total weight
                double sumOtherWeights = sumWeights - maxWeight;
                weights[maxIndex] = sumOtherWeights; // This sets the dominant weight equal to the sum of all others (50%)
                sumWeights = weights.Sum();
            }

            // 3. Calculate Weighted Mean with (potentially updated) weights
            double sumWeightTimesValue = 0;
            for (int i = 0; i < n; i++)
            {
                sumWeightTimesValue += weights[i] * validData[i].Val;
            }
            double weightedMean = sumWeightTimesValue / sumWeights;

            // 4. Calculate Internal Uncertainty & Apply Minimum Error Rule
            double internalUnc = Math.Sqrt(1.0 / sumWeights);
            double minInputUnc = validData.Min(v => v.DVal);
            if (internalUnc < minInputUnc)
            {
                internalUnc = minInputUnc; // Never allow final error to be smaller than the best measurement
            }

            // 5. Calculate Reduced Chi-Squared
            double chiSquared = 0;
            for (int i = 0; i < n; i++)
            {
                chiSquared += weights[i] * Math.Pow(validData[i].Val - weightedMean, 2);
            }
            double reducedChiSq = chiSquared / dof;

            // 6. External Uncertainty
            double externalUnc = internalUnc;
            if (reducedChiSq > 1.0)
            {
                externalUnc = internalUnc * Math.Sqrt(reducedChiSq);
            }

            double criticalValue = dof < CriticalReducedChiSq95.Length ? CriticalReducedChiSq95[dof] : 1.0;

            return new AveragingResult
            {
                WeightedMean = weightedMean,
                InternalUncertainty = internalUnc,
                ExternalUncertainty = externalUnc,
                ReducedChiSquared = reducedChiSq,
                CriticalChiSquared = criticalValue,
                IsDiscrepant = reducedChiSq > criticalValue,
                LwmApplied = lwmApplied
            };
        }

        public (double Mean, double Sigma) Compute(List<Value> values)
        {
            AveragingResult r = CalculateWeightedAverage(values);
            return (r.WeightedMean, r.InternalUncertainty);
        }
    }
}