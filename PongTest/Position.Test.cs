﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pong;

namespace PongTest
{
    [TestClass]
    public class PositionTests
    {
        public class TestPosition : Position
        {
            public TestPosition(int dimensions) : base(dimensions)
            {
            }
        }

        [TestMethod]
        public void Constructor_InitializesCoordinatesArray()
        {
            int dimensions = 3;
            var position = new TestPosition(dimensions);

            Assert.AreEqual(dimensions, position.Coordinates.Length);
        }

        [TestMethod]
        public void Indexer_GetSetValues()
        {
            int dimensions = 3;
            var position = new TestPosition(dimensions);

            position[0] = 1.1M;
            position[1] = 2.2M;
            position[2] = 3.3M;

            Assert.AreEqual(1.1M, position[0]);
            Assert.AreEqual(2.2M, position[1]);
            Assert.AreEqual(3.3M, position[2]);
        }

        [TestMethod]
        public void Indexer_OutOfRange_ThrowsException()
        {
            int dimensions = 3;
            var position = new TestPosition(dimensions);

            Assert.ThrowsException<IndexOutOfRangeException>(() => position[3] = 1.1M);
        }
    }
}