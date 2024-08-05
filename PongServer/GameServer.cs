using Serilog;
using System.Collections.Concurrent;

namespace PongServer
{
    class GameServer
    {
        private ConcurrentDictionary<int, GameInstance> _games;
        private ILogger _logger;

        private Thread? _serverThread;
        private readonly object _serverIsRunningLock = new object();
        private bool _serverIsRunning;
        private bool ServerIsRunning
        {
            get
            {
                lock(_serverIsRunningLock)
                {
                    return _serverIsRunning;
                }
            }

            set
            {
                lock (_serverIsRunningLock)
                {
                    _serverIsRunning = value;
                }
            }
        }
        public GameServer(ILogger logger)
        {
            _logger = logger;
            _games = new ConcurrentDictionary<int, GameInstance>();
            _logger.Debug($"GameServer>>Created");

        }

        public void StartServer()
        {
            ServerIsRunning = true;

            _serverThread = new Thread(Update);
            _serverThread.Start();
            _logger.Information($"GameServer>>Start");
        }

        public void StopServer()
        {
            _logger.Information($"GameServer>>Stop games");
            StopGames();
            _logger.Information($"GameServer>>Stop");

            ServerIsRunning = false;
            _serverThread?.Join();
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
                Console.Clear();

                foreach (var game in _games.Values.ToList())
                {
                    if (game != null)
                    {
                        (int leftScore, int rightScore) = (game.Score.LeftScore, game.Score.RightScore);
                        Console.WriteLine($"Game {game.GetHashCode()} has a score of {leftScore} / {rightScore}");

                        if (game.Status == GameInstance.StatusType.Stopped)
                        {
                            _logger.Information($"GameServer>>End GameInstance {game.GetHashCode()}");
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

        public Dictionary<int, Pong.Score> GetAllGameScores()
        {
            return _games.Values.Where(game => game.Status == GameInstance.StatusType.Playing)
                         .ToDictionary(game => game.GetHashCode(), game => game.Score);
        }

        public void AddNewGame(int gameUpdateDelayInMSec, int GameWinningScore)
        {
            // creat a new game
            var game = new GameInstance(_logger, gameUpdateDelayInMSec, GameWinningScore);
            _games.TryAdd(game.GetHashCode(), game);

            // creat a thread for running the new game
            var thread = new Thread(() => { game.Run(); });

            // start (run) the game            
            _logger.Information($"GameServer>>Created new GameInstance. Thread {thread.ManagedThreadId}");
            thread.Start();
        }

        public void StopGame(int gameId)
        {
            if (_games.TryRemove(gameId, out var game))
            {
                _logger.Information($"GameServer>>Stopping GameInstance {gameId}");
                game.Stop();
            }
        }
    }
}

 