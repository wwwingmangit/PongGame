using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pong;

namespace PongTest
{
    [TestClass]
    public class SpeedTests
    {
        public class TestSpeed : Speed
        {
            public TestSpeed(int dimensions) : base(dimensions)
            {
            }
        }

        [TestMethod]
        public void Constructor_InitializesCoordinatesArray()
        {
            int dimensions = 3;
            var speed = new TestSpeed(dimensions);

            Assert.AreEqual(dimensions, speed.Components.Length);
        }

        [TestMethod]
        public void Indexer_GetSetValues()
        {
            int dimensions = 3;
            var speed = new TestSpeed(dimensions);

            speed[0] = 1.1M;
            speed[1] = 2.2M;
            speed[2] = 3.3M;

            Assert.AreEqual(1.1M, speed[0]);
            Assert.AreEqual(2.2M, speed[1]);
            Assert.AreEqual(3.3M, speed[2]);
        }

        [TestMethod]
        public void Indexer_OutOfRange_ThrowsException()
        {
            int dimensions = 3;
            var speed = new TestSpeed(dimensions);

            Assert.ThrowsException<IndexOutOfRangeException>(() => speed[3] = 1.1M);
        }

        [TestMethod]
        public void Indexer_OutOfRange_ThrowsArgumentOutOfRangeException()
        {
            int dimensions = 3;
            var speed = new TestSpeed(dimensions);

            Assert.ThrowsException<IndexOutOfRangeException>(() => speed[-1] = 1.1M);
        }
    }
}
