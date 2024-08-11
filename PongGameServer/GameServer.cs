using Serilog;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using System.Threading;

namespace PongGameServer
{
    public class GameServer
    {
        private ConcurrentDictionary<int, GameInstance> _games;
        private ILogger _logger;

        private Thread? _serverThread;
        private readonly object _serverIsRunningLock = new object();
        private bool _serverIsRunning;
        public bool ServerIsRunning
        {
            get
            {
                lock (_serverIsRunningLock)
                {
                    return _serverIsRunning;
                }
            }

            protected set
            {
                lock (_serverIsRunningLock)
                {
                    _serverIsRunning = value;
                }
            }
        }
        private bool _writeToConsole;
        public GameServer(ILogger logger, bool writeToConsole = true)
        {
            _logger = logger.ForContext<GameServer>();

            _games = new ConcurrentDictionary<int, GameInstance>();

            _writeToConsole = writeToConsole;
            
            _logger.Information("GameServer created");
        }

        public void StartServer()
        {
            ServerIsRunning = true;

            _serverThread = new Thread(Update);
            _serverThread.Start();

            _logger.Information("GameServer started");
        }

        public void StopServer()
        {
            _logger.Information("Stopping GameServer");
            StopGames();

            ServerIsRunning = false;
            _serverThread?.Join();

            _logger.Information("GameServer stopped");
        }
        public void StopGames()
        {
            foreach (var game in _games.Values.ToList())
            {
                game.Stop();
            }
        }
        public void Update()
        {
            while (ServerIsRunning)
            {
                if (_writeToConsole)
                    Console.Clear();

                foreach (var game in GetGames())
                {
                    if (game != null)
                    {
                        (int leftScore, int rightScore) = (game.Score.LeftScore, game.Score.RightScore);

                        if (_writeToConsole)
                            Console.WriteLine($"Game {game.GetHashCode()} has a score of {leftScore} / {rightScore}");

                        if (game.Status == GameInstance.StatusType.Stopped)
                        {
                            _logger.Information("GameInstance {GameId} ended", game.GetHashCode());
                            _games.TryRemove(game.GetHashCode(), out _);
                        }
                    }
                }
                Thread.Sleep(1000); // Update every 1 second
            }
        }
        public List<GameInstance> GetGames()
        {
            return _games.Values.ToList();
        }
        public List<GameInstance> GetPlayingGames()
        {
            return _games.Values.Where(game => (game.Status == GameInstance.StatusType.Playing)).ToList();
        }

        public Pong.Score? GetGameScore(int gameId)
        {
            _games.TryGetValue(gameId, out var game);
            return game?.Score;
        }

        public void AddNewGame(int gameUpdateDelayInMSec, int GameWinningScore)
        {
            if (ServerIsRunning)
            {
                var game = new GameInstance(_logger, gameUpdateDelayInMSec, GameWinningScore);
                _games.TryAdd(game.GetHashCode(), game);

                var thread = new Thread(() => { game.Run(); });

                //_logger.Information($"GameServer>>Created new GameInstance. Thread {thread.ManagedThreadId}");
                _logger.Information("Created new GameInstance {@GameDetails}",
                                    new { GameId = game.GetHashCode(), ThreadId = thread.ManagedThreadId });
                thread.Start();
            }
            else
            {
                _logger.Warning("Attempted to add game when server is not running");
                // should throw exception when server not running
            }
        }
        public void StopGame(int gameId)
        {
            if (ServerIsRunning)
            {
                if (_games.TryRemove(gameId, out var game))
                {
                    _logger.Information("Stopping GameInstance {GameId}", gameId);
                    game.Stop();
                }
                else
                {
                    _logger.Warning("Attempted to stop non-existent game {GameId}", gameId);
                }
            }
            else
            {
                _logger.Warning("Attempted to stop game when server is not running");
                // Throw exception here
            }
        }
    }
}

