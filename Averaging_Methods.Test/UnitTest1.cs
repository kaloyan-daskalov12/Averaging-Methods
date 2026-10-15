using AveragingMethods;

namespace Averaging_Methods.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var input = new List<Value>
            {
                new Value(){ Val = 1, DVal = 1},
                new Value(){ Val = 1, DVal = 1}
            };

            var method = new BirgeRatioTool();

            var result = method.Calculate(input);

            Assert.Equal(0, result.Ratio, precision: 10);
        }
    }
}
