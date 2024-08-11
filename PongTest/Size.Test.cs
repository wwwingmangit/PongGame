using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pong;

namespace PongTest
{
    [TestClass]
    public class SizeTests
    {
        public class TestSize : Size
        {
            public TestSize(int dimensions) : base(dimensions)
            {
            }
        }

        [TestMethod]
        public void Constructor_InitializesCoordinatesArray()
        {
            int dimensions = 3;
            var size = new TestSize(dimensions);

            Assert.AreEqual(dimensions, size.Components.Length);
        }

        [TestMethod]
        public void Indexer_GetSetValues()
        {
            int dimensions = 3;
            var size = new TestSize(dimensions);

            size[0] = 1.1M;
            size[1] = 2.2M;
            size[2] = 3.3M;

            Assert.AreEqual(1.1M, size[0]);
            Assert.AreEqual(2.2M, size[1]);
            Assert.AreEqual(3.3M, size[2]);
        }

        [TestMethod]
        public void Indexer_OutOfRange_ThrowsException()
        {
            int dimensions = 3;
            var size = new TestSize(dimensions);

            Assert.ThrowsException<IndexOutOfRangeException>(() => size[3] = 1.1M);
        }

        [TestMethod]
        public void Indexer_OutOfRange_ThrowsArgumentOutOfRangeException()
        {
            int dimensions = 3;
            var size = new TestSize(dimensions);

            Assert.ThrowsException<IndexOutOfRangeException>(() => size[-1] = 1.1M);
        }
    }
}
