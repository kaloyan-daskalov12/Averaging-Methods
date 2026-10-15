using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AveragingMethods
{
    public interface IAverageMethod
    {
        public string Name { get; }

        public (double Mean, double Sigma) Compute(List<Value> values);
    }
}
