using System;
using System.Collections.Generic;
using System.Text;

namespace AveragingMethods
{
    public class Value
    {
        public double Val { get; set; }
        public double DVal { get; set; }

        public override string ToString()
        {
            return $"{Val} {{{DVal}}}";
        }
    }
}
