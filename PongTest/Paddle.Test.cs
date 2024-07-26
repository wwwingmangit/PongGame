using Pong;

namespace PongTest
{
    [TestClass]
    public class PaddleTests
    {
        public class PositionTest2D : Position
        {
            public PositionTest2D(decimal x = 0, decimal y = 0) : base(2)
            {
                Coordinates[0] = x;
                Coordinates[1] = y;
            }

            public decimal X
            {
                get => Coordinates[0];
                set => Coordinates[0] = value;
            }

            public decimal Y
            {
                get => Coordinates[1];
                set => Coordinates[1] = value;
            }
        }

        public class SpeedTest2D : Speed
        {
            public SpeedTest2D(decimal x = 0, decimal y = 0) : base(2)
            {
                Components[0] = x;
                Components[1] = y;
            }

            public decimal X
            {
                get => Components[0];
                set => Components[0] = value;
            }

            public decimal Y
            {
                get => Components[1];
                set => Components[1] = value;
            }
        }
        public class SizeTest2D : Size
        {
            public SizeTest2D(decimal width = 1, decimal height = 1) : base(2)
            {
                Components[0] = width;
                Components[1] = height;
            }

            public decimal Width
            {
                get => Components[0];
                set => Components[0] = value;
            }

            public decimal Height
            {
                get => Components[1];
                set => Components[1] = value;
            }
        }
        public class PaddleTest2D : Paddle<PositionTest2D, SpeedTest2D, SizeTest2D>
        {
            public PaddleTest2D(PositionTest2D position, SpeedTest2D speed, SizeTest2D size)
                : base(position, speed, size)
            {
            }
        }

        [TestMethod]
        public void Constructor_InitializesProperties()
        {
            var position = new PositionTest2D(1.0M, 2.0M);
            var speed = new SpeedTest2D(0.5M, 0.5M);
            var size = new SizeTest2D(0.1M, 0.3M);

            var paddle = new PaddleTest2D(position, speed, size);

            Assert.AreEqual(position, paddle.Position);
            Assert.AreEqual(speed, paddle.Speed);
            Assert.AreEqual(size, paddle.Size);
        }

        [TestMethod]
        public void Position_ChangesCorrectly()
        {
            var position = new PositionTest2D(1.0M, 2.0M);
            var speed = new SpeedTest2D(0.5M, 0.5M);
            var size = new SizeTest2D(0.1M, 0.3M);

            var paddle = new PaddleTest2D(position, speed, size);

            paddle.Position.X = 3.0M;
            paddle.Position.Y = 4.0M;

            Assert.AreEqual(3.0M, paddle.Position.X);
            Assert.AreEqual(4.0M, paddle.Position.Y);
        }

        [TestMethod]
        public void Speed_ChangesCorrectly()
        {
            var position = new PositionTest2D(1.0M, 2.0M);
            var speed = new SpeedTest2D(0.5M, 0.5M);
            var size = new SizeTest2D(0.1M, 0.3M);

            var paddle = new PaddleTest2D(position, speed, size);

            paddle.Speed.X = 1.0M;
            paddle.Speed.Y = 1.5M;

            Assert.AreEqual(1.0M, paddle.Speed.X);
            Assert.AreEqual(1.5M, paddle.Speed.Y);
        }

        [TestMethod]
        public void Size_ChangesCorrectly()
        {
            var position = new PositionTest2D(1.0M, 2.0M);
            var speed = new SpeedTest2D(0.5M, 0.5M);
            var size = new SizeTest2D(0.1M, 0.3M);

            var paddle = new PaddleTest2D(position, speed, size);

            paddle.Size.Width = 0.2M;
            paddle.Size.Height = 0.4M;

            Assert.AreEqual(0.2M, paddle.Size.Width);
            Assert.AreEqual(0.4M, paddle.Size.Height);
        }
    }
}
