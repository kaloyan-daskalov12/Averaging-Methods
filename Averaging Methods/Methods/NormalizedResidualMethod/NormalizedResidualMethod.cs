using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AveragingMethods
{
    public class NormalizedResidualMethod : IAverageMethod
    {
        public string Name => "Normalized Residual Method";

        public (double Mean, double Sigma) Compute(List<Value> values) => Compute(values, 2.0, 50);

        public (double Mean, double Sigma) Compute(List<Value> values, double residualLimit = 2.0, int maxIterations = 50)
        {
            int n = values.Count;

            double[] w = values.Select(v => Weight(v.DVal)).ToArray();

            for (int iter = 0; iter < maxIterations; iter++)
            {
                double sumW = w.Sum();
                double mean = Enumerable.Range(0, n).Sum(i => w[i] * values[i].Val) / sumW;

                bool changed = false;

                for (int i = 0; i < n; i++)
                {
                    double baseW = Weight(values[i].DVal);
                    double residual = Math.Abs((values[i].Val - mean) / values[i].DVal);

                    double newW = baseW;

                    if (residual > residualLimit)
                        newW = baseW / (residual * residual);

                    if (Math.Abs(newW - w[i]) > 1e-12)
                        changed = true;

                    w[i] = newW;
                }

                if (!changed)
                    break;
            }

            double finalW = w.Sum();
            double finalMean = Enumerable.Range(0, n).Sum(i => w[i] * values[i].Val) / finalW;
            double sigma = Math.Sqrt(1.0 / finalW);

            return (finalMean, sigma);
        }

        double Weight(double sigma)
        {
            if (sigma <= 0)
                throw new ArgumentException("Sigma must be > 0");

            return 1.0 / (sigma * sigma);
        }
    }
}
