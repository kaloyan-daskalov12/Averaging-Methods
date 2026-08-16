using System;
using System.Collections.Generic;
using System.Linq;

namespace AveragingMethods
{
    /// <summary>
    /// Rajeval Technique (RT) - approximate implementation.
    /// Reference: M.U. Rajput, T.D. MacMahon, "Techniques for evaluating
    /// discrepant data," Nucl. Instr. Meth. A312, 289 (1992).
    ///
    /// IMPORTANT: the original 1992 paper is paywalled and its exact algorithm
    /// wasn't available to reproduce line-by-line. This class instead follows the
    /// *documented behaviour* of RT reported in secondary sources (Singh &amp; Birch,
    /// IAEA-ICTP Workshop notes, 2016; Zahn et al., J. Radioanal. Nucl. Chem., 2015):
    /// it behaves like the weighted mean, but (a) automatically flags/removes
    /// severe outliers, and (b) inflates the uncertainty of remaining measurements
    /// that are inconsistent with the mean - reportedly by larger factors than the
    /// Normalized Residual method. If you get hold of the original paper, please
    /// check the two constants below (OutlierLimit, ResidualLimit) and the
    /// iteration logic against it - treat this as a starting point, not a verified
    /// reproduction.
    ///
    /// Steps:
    ///  1. Compute the ordinary weighted mean.
    ///  2. Repeatedly drop the single worst-fitting point (largest absolute
    ///     normalized residual) as long as it exceeds OutlierLimit, recomputing
    ///     the mean each time - the "outlier removal" behaviour.
    ///  3. On the surviving points, iteratively inflate (never shrink) any
    ///     measurement's uncertainty just enough to bring its normalized residual
    ///     down to ResidualLimit, recomputing the mean, until self-consistent -
    ///     the "uncertainty inflation" behaviour.
    /// </summary>
    public class RajevalTechnique : IAverageMethod, IStringOutput
    {
        private const double OutlierLimit = 3.0;   // points beyond this are dropped entirely
        private const double ResidualLimit = 1.5;  // survivors are inflated to satisfy this
        private const int MaxIterations = 100;

        public string Name => "Rajeval Technique";

        /// <summary>Values dropped as severe outliers during the most recent Compute call.</summary>
        public List<Value> RemovedOutliers { get; private set; } = new();

        public (double Mean, double Sigma) Compute(List<Value> values)
        {
            // Filter out invalid data (missing uncertainties)
            var validData = values.Where(v => v.DVal > 0).ToList();
            RemovedOutliers = new List<Value>();

            if (validData.Count == 0)
            {
                throw new InvalidOperationException("No valid data points to average.");
            }

            if (validData.Count == 1)
            {
                return (validData[0].Val, validData[0].DVal);
            }

            double WeightedMeanOf(List<Value> data)
            {
                double sw = 0, swv = 0;
                foreach (var v in data)
                {
                    double w = 1.0 / (v.DVal * v.DVal);
                    sw += w;
                    swv += w * v.Val;
                }
                return swv / sw;
            }

            // Step 2: iteratively drop severe outliers.
            var candidate = new List<Value>(validData);
            for (int pass = 0; pass < MaxIterations && candidate.Count > 1; pass++)
            {
                double mean = WeightedMeanOf(candidate);
                var worst = candidate
                    .Select(v => (v, resid: Math.Abs((v.Val - mean) / v.DVal)))
                    .OrderByDescending(t => t.resid)
                    .First();

                if (worst.resid <= OutlierLimit) break;

                RemovedOutliers.Add(worst.v);
                candidate.Remove(worst.v);
            }

            if (candidate.Count == 0) candidate = validData; // never discard everything

            // Step 3: inflate uncertainties of remaining inconsistent points.
            double[] effectiveSigma = candidate.Select(v => v.DVal).ToArray();
            double finalMean = 0;

            for (int iter = 0; iter < MaxIterations; iter++)
            {
                double sumWeights = 0, sumWeightTimesValue = 0;
                for (int i = 0; i < candidate.Count; i++)
                {
                    double w = 1.0 / (effectiveSigma[i] * effectiveSigma[i]);
                    sumWeights += w;
                    sumWeightTimesValue += w * candidate[i].Val;
                }
                finalMean = sumWeightTimesValue / sumWeights;

                bool anyInflated = false;
                for (int i = 0; i < candidate.Count; i++)
                {
                    double residual = (candidate[i].Val - finalMean) / effectiveSigma[i];
                    if (Math.Abs(residual) > ResidualLimit)
                    {
                        double inflated = Math.Abs(candidate[i].Val - finalMean) / ResidualLimit;
                        if (inflated > effectiveSigma[i])
                        {
                            effectiveSigma[i] = inflated;
                            anyInflated = true;
                        }
                    }
                }
                if (!anyInflated) break;
            }

            double finalWeights = effectiveSigma.Sum(s => 1.0 / (s * s));
            double sigma = Math.Sqrt(1.0 / finalWeights);
            return (finalMean, sigma);
        }

        public string GetOutput(List<Value> values)
        {
            var (mean, sigma) = Compute(values); // also (re)populates RemovedOutliers

            string outliers = RemovedOutliers.Count == 0
                ? "none"
                : string.Join(", ", RemovedOutliers.Select(v => v.ToString()));

            return $"  Mean = {mean:0.####}\n  Sigma = {sigma:0.####}\n  Outliers removed = {outliers}";
        }
    }
}
