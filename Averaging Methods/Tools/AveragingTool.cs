using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AveragingMethods
{
    public class AveragingTool
    {
        public AveragingTool()
        {
            Methods = new List<IAverageMethod>();

            Methods.Add(WeightedMean);
            Methods.Add(LimitedWeightMean);
            Methods.Add(NormalizedResidualMethod);
            Methods.Add(GaussianConsensusAveraging);
            Methods.Add(WeightedAverage);
            Methods.Add(EvaluatedAverage);
        }

        List<IAverageMethod> Methods { get; }

        public WeightedMean WeightedMean { get; } = new WeightedMean();
        public LimitedWeightMean LimitedWeightMean { get; } = new LimitedWeightMean();
        public NormalizedResidualMethod NormalizedResidualMethod { get; } = new NormalizedResidualMethod();
        public GaussianConsensusAveraging GaussianConsensusAveraging { get; } = new GaussianConsensusAveraging();
        public WeightedAverage WeightedAverage { get; } = new WeightedAverage();
        public EvaluatedAverage EvaluatedAverage { get; } = new EvaluatedAverage();

        public string[] GetResult(List<Value> values, List<int> indices)
        {
            Console.WriteLine(string.Join("; ", values));
            List<string> results = new List<string>();
            for (int i = 0; i < Methods.Count; i++)
            {
                if (!indices.Contains(i)) continue;
                try
                {
                    var result = Methods[i].Compute(values);
                    results.Add($"{Methods[i].Name}: {result.Mean:0.00} ± {result.Sigma:0.00};");
                }
                catch (Exception e)
                {
                    results.Add($"{Methods[i].Name}: Exited with error: {{{e.Message}}}");
                }
            }
            return results.ToArray();
        }

        public string GetMethods()
        {
            string result = "";
            for (int i = 0; i < Methods.Count; i++)
            {
                result += $"[{i}] {Methods[i].Name}\n";
            }
            return result;
        }
    }
}
