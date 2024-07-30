using Pong;

namespace PongTest
{
    [TestClass]
    public class Speed3DTests
    {
        [TestMethod]
        public void Speed3D_DefaultConstructor_SetsXAndYToZero()
        {
            var position = new Speed3D();
            Assert.AreEqual(0, position.X);
            Assert.AreEqual(0, position.Y);
            Assert.AreEqual(0, position.Z);
        }

        [TestMethod]
        public void Speed3D_ParameterizedConstructor_SetsXAndY()
        {
            var position = new Speed3D(5, 10, 15);
            Assert.AreEqual(5, position.X);
            Assert.AreEqual(10, position.Y);
            Assert.AreEqual(15, position.Z);
        }
    }
}
