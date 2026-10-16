using AveragingMethods;

namespace Averaging_Methods.Test
{
    public class BirgeRatioTool
    {
        AveragingMethods.BirgeRatioTool Method = new AveragingMethods.BirgeRatioTool();

        [Fact]
        public void Test1()
        {
            var input = new List<Value>
            {
                new Value(){ Val = 1, DVal = 1},
                new Value(){ Val = 1, DVal = 1}
            };

            var result = Method.Calculate(input);

            Assert.Equal(0, result.Ratio, precision: 10);
            Assert.Equal(0, result.CriticalLimit, precision: 10);
        }

        [Fact]
        public void Test2()
        {
            var input = new List<Value>
            {
                new Value(){ Val = 1, DVal = 1},
                new Value(){ Val = 1, DVal = 1}
            };

            var result = Method.Calculate(input);

            Assert.Equal(0, result.Ratio, precision: 10);
            Assert.Equal(2.4, result.CriticalLimit, precision: 1);
        }
    }
}
