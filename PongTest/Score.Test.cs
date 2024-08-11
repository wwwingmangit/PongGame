using Pong;

namespace PongTest
{
    [TestClass]
    public class ScoreTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Score_NegativeLeftScore_ThrowsArgumentOutOfRangeException()
        {
            _ = new Score(-1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Score_NegativeRightScore_ThrowsArgumentOutOfRangeException()
        {
            _ = new Score(0, -1);
        }
    }
}
