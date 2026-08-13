using System;
using System.Collections.Generic;
using System.Linq;

namespace AveragingMethods
{
    public class BirgeRatioTool : IStringOutput
    {
        public string Name => "Birge Ratio";

        internal static BirgeRatioResult Calculate(List<Value> data)
        {
            var validData = data.Where(v => v.DVal > 0).ToList();
            int n = validData.Count;

            if (n < 2)
            {
                throw new InvalidOperationException("Need at least 2 data points for a Birge Ratio test.");
            }

            int dof = n - 1;

            // 1. Calculate Weights and Mean
            double sumWeights = 0;
            double sumWeightTimesValue = 0;

            foreach (var item in validData)
            {
                double weight = 1.0 / (item.DVal * item.DVal);
                sumWeights += weight;
                sumWeightTimesValue += weight * item.Val;
            }

            double weightedMean = sumWeightTimesValue / sumWeights;

            // 2. Calculate Chi-Squared
            double chiSquared = 0;
            foreach (var item in validData)
            {
                double weight = 1.0 / (item.DVal * item.DVal);
                chiSquared += weight * Math.Pow(item.Val - weightedMean, 2);
            }

            double reducedChiSq = chiSquared / dof;

            // 3. Calculate Birge Ratio and Critical Limit
            double birgeRatio = Math.Sqrt(reducedChiSq);

            // The standard threshold for data consistency using the Birge test
            double criticalLimit = 1.0 + Math.Sqrt(2.0 / dof);

            return new BirgeRatioResult
            {
                Ratio = birgeRatio,
                CriticalLimit = criticalLimit,
                IsConsistent = birgeRatio <= criticalLimit
            };
        }

        public string GetOutput(List<Value> values)
        {
            throw new NotImplementedException();
        }
    }
}