using AveragingMethods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Averaging_Methods.Methods.WeightedAverage
{
    // A simple container just for the pure math results

    public class WeightedAverage: IAverageMethod
    {
        public string Name => "Weighted Average v2";

        public (double Mean, double Sigma) Compute(List<Value> values)
        {
            // Filter out invalid data (missing uncertainties)
            var validData = values.Where(v => v.DVal > 0).ToList();

            if (validData.Count == 0)
            {
                throw new InvalidOperationException("No valid data points to average.");
            }

            double sumWeights = 0;
            double sumWeightTimesValue = 0;

            // Calculate standard weights (w = 1 / sigma^2)
            foreach (var item in validData)
            {
                double weight = 1.0 / (item.DVal * item.DVal);
                sumWeights += weight;
                sumWeightTimesValue += weight * item.Val;
            }

            // Return the pure mathematical mean and standard error
            return (sumWeightTimesValue / sumWeights,Math.Sqrt(1.0 / sumWeights));
        }
    }
}