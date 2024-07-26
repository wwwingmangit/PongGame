using Pong;

namespace PongTest
{
    [TestClass]
    public class Position2DTests
    {
        [TestMethod]
        public void Position2D_DefaultConstructor_SetsXAndYToZero()
        {
            var position = new Position2D();
            Assert.AreEqual(0, position.X);
            Assert.AreEqual(0, position.Y);
        }

        [TestMethod]
        public void Position2D_ParameterizedConstructor_SetsXAndY()
        {
            var position = new Position2D(5, 10);
            Assert.AreEqual(5, position.X);
            Assert.AreEqual(10, position.Y);
        }
    }
}
