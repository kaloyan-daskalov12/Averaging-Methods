using System;
using System.Collections.Generic;
using System.Text;

namespace AveragingMethods
{
    public class AveragingResult
    {
        public double WeightedMean { get; set; }
        public double InternalUncertainty { get; set; }
        public double ExternalUncertainty { get; set; }
        public double ReducedChiSquared { get; set; }
        public double CriticalChiSquared { get; set; }
        public bool IsDiscrepant { get; set; }
        public bool LwmApplied { get; set; } // Tracks if the 50% rule was triggered
    }
}
