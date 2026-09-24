using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AveragingMethods
{
    public class WeightedMean : IAverageMethod, IStringOutput
    {
        public string Name => "Weighted Mean";

        public (double Mean, double Sigma) Compute(List<Value> values)
        {
            double sumW = values.Sum(v => Weight(v.DVal));
            double mean = values.Sum(v => Weight(v.DVal) * v.Val) / sumW;
            double sigma = Math.Sqrt(1.0 / sumW);

            return (mean, sigma);
        }

        public string GetOutput(List<Value> values)
        {
            throw new NotImplementedException();
        }

        double Weight(double sigma)
        {
            if (sigma <= 0)
                throw new ArgumentException("Sigma must be > 0");

            return 1.0 / (sigma * sigma);
        }
    }
}
