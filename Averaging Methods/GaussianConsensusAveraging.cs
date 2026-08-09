using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AveragingMethods
{
    public class GaussianConsensusAveraging : IAverageMethod
    {
        public string Name => "Gaussian Consensus Averaging";

        double Support(double x, double mean, double sigma)
        {
            if (sigma <= 0)
                return 0;

            double z = (x - mean) / sigma;
            return Math.Exp(-0.5 * z * z);
        }

        public (double Mean, double Sigma) Compute(List<Value> values) => Compute(values, 1, true);

        /// <summary>
        /// Computes the mean with Gaussian Consensus Averaging (GCA) method
        /// </summary>
        /// <param name="values">The set of values</param>
        /// <param name="alpha">Self-trust coefficient</param>
        /// <param name="fullSigma">Wether to add the reported uncertainties to the summarized standart deviation</param>
        /// <returns>The mean and it's standart deviation</returns>
        public (double Mean, double Sigma) Compute(List<Value> values, double alpha = 1, bool fullSigma = true)
        {
            int n = values.Count;

            double[,] P = new double[n, n];

            for (int dist = 0; dist < n; dist++)
            {
                for (int candidate = 0; candidate < n; candidate++)
                {
                    P[dist, candidate] = Support(
                        values[candidate].Val,
                        values[dist].Val,
                        values[dist].DVal
                    );
                }
            }

            double[] S = new double[n];

            for (int candidate = 0; candidate < n; candidate++)
            {
                double sum = 0;

                for (int dist = 0; dist < n; dist++)
                {
                    double factor = (dist == candidate) ? alpha : 1.0;
                    sum += factor * P[dist, candidate];
                }

                S[candidate] = sum / n;
            }

            double sSum = S.Sum();

            if (sSum == 0)
                throw new Exception("All support values are zero.");

            double[] W = new double[n];

            for (int i = 0; i < n; i++)
                W[i] = S[i] / sSum;

            double mean = Enumerable.Range(0, n)
                .Sum(i => W[i] * values[i].Val);

            double sigmaSpread = Math.Sqrt(
                Enumerable.Range(0, n)
                    .Sum(i => W[i] * Math.Pow(values[i].Val - mean, 2))
            );

            double sigmaReported = fullSigma
                ? Math.Sqrt(
                    Enumerable.Range(0, n)
                        .Sum(i => W[i] * values[i].DVal * values[i].DVal)
                  )
                : 0;

            double sigma = Math.Sqrt(
                sigmaSpread * sigmaSpread +
                sigmaReported * sigmaReported
            );

            return (mean, sigma);
        }
    }
}
