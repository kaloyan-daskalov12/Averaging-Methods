using System;
using System.Collections.Generic;
using System.Linq;

namespace AveragingMethods
{
    /// <summary>
    /// Expected Value Method (EVM).
    /// Reference: M. Birch, B. Singh, "Method of Best Representation for Averages
    /// in Data Evaluation," Applied Radiation and Isotopes / Nucl. Data Sheets 120,
    /// 106 (2014).
    ///
    /// Treats each measurement as its own Gaussian probability density and builds
    /// a single "mean probability density" by averaging all of them with equal
    /// (1/N) weight:
    ///
    ///     f(x) = (1/N) * sum_i Normal(x; x_i, sigma_i)
    ///
    /// The reported value is the expectation of that mixture density, and the
    /// reported uncertainty is its standard deviation. For a mixture of Gaussians
    /// both have closed forms:
    ///
    ///     Mean = (1/N) * sum_i x_i
    ///     Var  = (1/N) * sum_i sigma_i^2   +   (1/N) * sum_i (x_i - Mean)^2
    ///            \_______ within ________/     \_______ between _________/
    ///
    /// i.e. the average of the individual (within-measurement) variances plus the
    /// spread (between-measurement variance) of the central values. This is why
    /// EVM never shrinks the uncertainty below what the scatter of the data
    /// justifies, is robust to a single outlier (it only ever gets a 1/N share of
    /// the mean, never a weight large enough to swamp the rest), and leaves the
    /// input data untouched. Note: this implementation assumes symmetric
    /// uncertainties; the original paper also covers the asymmetric case.
    /// </summary>
    public class ExpectedValueMethod : IAverageMethod, IStringOutput
    {
        public string Name => "Expected Value Method";

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
                return (mean, validData[0].DVal);
            }

            double withinVariance = validData.Average(v => v.DVal * v.DVal);
            double betweenVariance = validData.Average(v => (v.Val - mean) * (v.Val - mean));

            double sigma = Math.Sqrt(withinVariance + betweenVariance);
            return (mean, sigma);
        }

        public string GetOutput(List<Value> values)
        {
            var validData = values.Where(v => v.DVal > 0).ToList();
            var (mean, sigma) = Compute(values);

            if (validData.Count < 2)
            {
                return $"  N = {validData.Count}\n  Mean = {mean:0.####}\n  Sigma = {sigma:0.####}";
            }

            double within = validData.Average(v => v.DVal * v.DVal);
            double between = validData.Average(v => (v.Val - mean) * (v.Val - mean));

            return $"  N = {validData.Count}\n  Mean = {mean:0.####}\n  Within-measurement variance = {within:0.####}\n  Between-measurement variance = {between:0.####}\n  Sigma = {sigma:0.####}";
        }
    }
}
