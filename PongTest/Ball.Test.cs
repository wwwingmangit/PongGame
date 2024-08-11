using Pong;

namespace PongTest
{
    [TestClass]
    public class BallTests
    {
        [TestMethod]
        public void Constructor_InitializesProperties()
        {
            var position = new Position2D(1.0M, 2.0M);
            var speed = new Speed2D(0.5M, 0.5M);
            var size = new Size2D(0.1M, 0.1M);

            var ball = new Ball2D(position, speed, size);

            Assert.AreEqual(position, ball.Position);
            Assert.AreEqual(speed, ball.Speed);
            Assert.AreEqual(size, ball.Size);
        }

        [TestMethod]
        public void Position_ChangesCorrectly()
        {
            var position = new Position2D(1.0M, 2.0M);
            var speed = new Speed2D(0.5M, 0.5M);
            var size = new Size2D(0.1M, 0.1M);

            var ball = new Ball2D(position, speed, size);

            ball.Position.X = 3.0M;
            ball.Position.Y = 4.0M;

            Assert.AreEqual(3.0M, ball.Position.X);
            Assert.AreEqual(4.0M, ball.Position.Y);
        }

        [TestMethod]
        public void Speed_ChangesCorrectly()
        {
            var position = new Position2D(1.0M, 2.0M);
            var speed = new Speed2D(0.5M, 0.5M);
            var size = new Size2D(0.1M, 0.1M);

            var ball = new Ball2D(position, speed, size);

            ball.Speed.X = 1.0M;
            ball.Speed.Y = 1.5M;

            Assert.AreEqual(1.0M, ball.Speed.X);
            Assert.AreEqual(1.5M, ball.Speed.Y);
        }

        [TestMethod]
        public void Size_ChangesCorrectly()
        {
            var position = new Position2D(1.0M, 2.0M);
            var speed = new Speed2D(0.5M, 0.5M);
            var size = new Size2D(0.1M, 0.1M);

            var ball = new Ball2D(position, speed, size);

            ball.Size.Width = 0.2M;
            ball.Size.Height = 0.3M;

            Assert.AreEqual(0.2M, ball.Size.Width);
            Assert.AreEqual(0.3M, ball.Size.Height);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullPosition_ThrowsArgumentNullException()
        {
            var speed = new Speed2D(0.5M, 0.5M);
            var size = new Size2D(0.1M, 0.1M);

            _ = new Ball2D(null!, speed, size);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullSpeed_ThrowsArgumentNullException()
        {
            var position = new Position2D(1.0M, 2.0M);
            var size = new Size2D(0.1M, 0.1M);

            _ = new Ball2D(position, null!, size);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullSize_ThrowsArgumentNullException()
        {
            var position = new Position2D(1.0M, 2.0M);
            var speed = new Speed2D(0.5M, 0.5M);

            _ = new Ball2D(position, speed, null!);
        }
    }
}
