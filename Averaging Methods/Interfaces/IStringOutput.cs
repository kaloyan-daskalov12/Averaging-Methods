using System;
using System.Collections.Generic;
using System.Text;

namespace AveragingMethods
{
    public interface IStringOutput
    {
        public string Name { get; }

        public string GetOutput(List<Value> values);
    }
}
