using Pong;

namespace PongTest
{
    [TestClass]
    public class ScoreTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeLeftScore_ThrowsArgumentOutOfRangeException()
        {
            _ = new Score(-1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeRightScore_ThrowsArgumentOutOfRangeException()
        {
            _ = new Score(0, -1);
        }

        [TestMethod]
        public void IncLeftScore_IncrementsLeftScoreByOne()
        {
            var score = new Score(0, 0);
            Assert.AreEqual(1, score.IncLeftScore());
            Assert.AreEqual(1, score.LeftScore);
        }

        [TestMethod]
        public void IncRightScore_IncrementsRightScoreByOne()
        {
            var score = new Score(0, 0);
            Assert.AreEqual(1, score.IncRightScore());
            Assert.AreEqual(1, score.RightScore);
        }

        [TestMethod]
        public void CopyConstructor_CreatesExactCopy()
        {
            var original = new Score(3, 4);
            var copy = new Score(original);
            Assert.AreEqual(original.LeftScore, copy.LeftScore);
            Assert.AreEqual(original.RightScore, copy.RightScore);
        }

        [TestMethod]
        public void Update_WithScore_UpdatesBothScores()
        {
            var score = new Score(1, 1);
            score.Update(new Score(2, 3));
            Assert.AreEqual(2, score.LeftScore);
            Assert.AreEqual(3, score.RightScore);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Update_WithNegativeLeftScore_ThrowsArgumentOutOfRangeException()
        {
            var score = new Score(0, 0);
            score.Update(-1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Update_WithNegativeRightScore_ThrowsArgumentOutOfRangeException()
        {
            var score = new Score(0, 0);
            score.Update(0, -1);
        }
    }
}

