using System;
using System.Collections.Generic;
using System.Linq;

namespace AveragingMethods
{
    /// <summary>
    /// Unweighted (arithmetic) average.
    /// Ignores the individual uncertainties entirely and treats every measurement
    /// as equally trustworthy - the maximum-likelihood estimator for the mean of a
    /// normal distribution given a plain sample. Recommended when a set of
    /// discrepant measurements can't be reconciled with any confidence by the
    /// evaluator (see Singh &amp; Birch, "Averaging Methods for Experimental
    /// Measurements", IAEA-ICTP Workshop, 2016).
    /// </summary>
    public class UnweightedAverage : IAverageMethod, IStringOutput
    {
        public string Name => "Unweighted Average";

        public (double Mean, double Sigma) Compute(List<Value> values)
        {
            // Filter out invalid data (missing uncertainties)
            var validData = values.Where(v => v.DVal > 0).ToList();

            if (validData.Count == 0)
            {
                throw new InvalidOperationException("No valid data points to average.");
            }

            int n = validData.Count;
            double mean = validData.Average(v => v.Val);

            if (n == 1)
            {
                // Can't estimate a spread from a single point - fall back to its own uncertainty.
                return (mean, validData[0].DVal);
            }

            // Standard error of the mean: sqrt( 1/(N(N-1)) * sum((x_i - mean)^2) )
            double sumSquares = validData.Sum(v => (v.Val - mean) * (v.Val - mean));
            double sigma = Math.Sqrt(sumSquares / (n * (n - 1)));

            return (mean, sigma);
        }

        public string GetOutput(List<Value> values)
        {
            var validData = values.Where(v => v.DVal > 0).ToList();
            var (mean, sigma) = Compute(values);

            return $"  N = {validData.Count}\n  Mean = {mean:0.####}\n  Sigma (standard error of the mean) = {sigma:0.####}";
        }
    }
}
