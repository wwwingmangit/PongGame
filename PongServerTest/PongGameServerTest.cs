using Pong;
using PongGameServer;
using Serilog;
using System.Threading;

namespace PongGameServerTest
{
    [TestClass]
    public class GameServerTests
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        private PongGameServer.GameServer? _gameServer;
        readonly ILogger _logger = new LoggerConfiguration()
                                    .MinimumLevel.Debug()
                                    .WriteTo.File("testlog.txt")
                                    .CreateLogger();


        [TestInitialize]
        public void Setup()
        {
            // execute before every test
            _gameServer = new GameServer(_logger, false);
        }

        [TestMethod]
        public void StartServer_SetsServerIsRunningToTrue()
        {
            _gameServer.StartServer();

            Assert.IsTrue(_gameServer.ServerIsRunning);

            _gameServer.StopServer();
        }

        [TestMethod]
        public void StopServer_SetsServerIsRunningToFalse()
        {
            _gameServer.StartServer();
            _gameServer.StopServer();

            Assert.IsFalse(_gameServer.ServerIsRunning);
        }

        [TestMethod]
        public void AddNewGame_ServerNotRunning_IncreasesGameCount()
        {
            int initialGameCount = _gameServer.GetGames().Count;

            _gameServer.AddNewGame(100, 10);

            Assert.AreEqual(initialGameCount, _gameServer.GetGames().Count);
        }

        [TestMethod]
        public void StopGame_ServerNotRunning_DecreasesGameCount()
        {
            _gameServer.StartServer();

            _gameServer.AddNewGame(100, 10);
            int gameId = _gameServer.GetGames().First().GetHashCode();
            int initialGameCount = _gameServer.GetGames().Count;
            _gameServer.StopServer();

            _gameServer.StopGame(gameId);

            Assert.AreEqual(initialGameCount, _gameServer.GetGames().Count);
        }

        [TestMethod]
        public void AddNewGame_IncreasesGameCount()
        {
            _gameServer.StartServer();

            int initialGameCount = _gameServer.GetGames().Count;

            _gameServer.AddNewGame(1000, 10);

            Assert.AreEqual(initialGameCount + 1, _gameServer.GetGames().Count);
        }

        [TestMethod]
        public void StopGame_DecreasesGameCount()
        {
            _gameServer.StartServer();

            _gameServer.AddNewGame(1000, 10);
            int gameId = _gameServer.GetGames().First().GetHashCode();
            int initialGameCount = _gameServer.GetGames().Count;

            _gameServer.StopGame(gameId);

            Assert.AreEqual(initialGameCount - 1, _gameServer.GetGames().Count);
        }


        [TestMethod]
        public void GetPlayingGames_ReturnsOnlyPlayingGames()
        {
            _gameServer.StartServer();

            _gameServer.AddNewGame(100, 10);
            _gameServer.AddNewGame(100, 10);

            Thread.Sleep(200);

            var playingGames = _gameServer.GetPlayingGames();

            CollectionAssert.AllItemsAreInstancesOfType(playingGames, typeof(GameInstance));
            Assert.IsTrue(playingGames.All(game => game.Status == GameInstance.StatusType.Playing));
        }

        [TestMethod]
        public void GetGameScore_ReturnsCorrectScore()
        {
            _gameServer.StartServer();

            _gameServer.AddNewGame(100, 10);
            int gameId = _gameServer.GetGames().First().GetHashCode();

            var score = _gameServer.GetGameScore(gameId);

            Assert.IsNotNull(score);
            Assert.AreEqual(0, score.LeftScore);
            Assert.AreEqual(0, score.RightScore);
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}