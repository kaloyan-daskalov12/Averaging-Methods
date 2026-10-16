using AveragingMethods;

namespace Averaging_Methods.Test
{
    public class EvaluatedAverage
    {
        AveragingMethods.EvaluatedAverage Method = new AveragingMethods.EvaluatedAverage();

        [Fact]
        public void Test1()
        {
            var input = new List<Value>
            {
                new Value(){ Val = 1, DVal = 1},
                new Value(){ Val = 1, DVal = 1}
            };

            var result = Method.Compute(input);

            Assert.Equal(0, result.Mean, precision: 10);
            Assert.Equal(0, result.Sigma, precision: 10);
        }
    }
}
