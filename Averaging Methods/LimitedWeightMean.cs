using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AveragingMethods
{
    public class LimitedWeightMean : IAverageMethod
    {
        public string Name => "Limited Relative Weight Mean";

        public (double Mean, double Sigma) Compute(List<Value> values) => Compute(values, 0.5, 100);

        public (double Mean, double Sigma) Compute(List<Value> values, double maxRelativeWeight = 0.5, int maxIterations = 100)
        {
            int n = values.Count;

            double[] w = values.Select(v => Weight(v.DVal)).ToArray();

            for (int iter = 0; iter < maxIterations; iter++)
            {
                bool changed = false;
                double total = w.Sum();

                for (int i = 0; i < n; i++)
                {
                    double rel = w[i] / total;

                    if (rel > maxRelativeWeight)
                    {
                        double others = total - w[i];

                        double limited =
                            maxRelativeWeight / (1.0 - maxRelativeWeight) * others;

                        if (limited < w[i])
                        {
                            w[i] = limited;
                            changed = true;
                        }
                    }
                }

                if (!changed)
                    break;
            }

            double sumW = w.Sum();
            double mean = Enumerable.Range(0, n).Sum(i => w[i] * values[i].Val) / sumW;
            double sigma = Math.Sqrt(1.0 / sumW);

            return (mean, sigma);
        }

        double Weight(double sigma)
        {
            if (sigma <= 0)
                throw new ArgumentException("Sigma must be > 0");

            return 1.0 / (sigma * sigma);
        }
    }
}
