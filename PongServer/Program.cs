using System;
using Serilog;
using Pong;
using System.Threading;

namespace PongServer
{
    class GameInstance
    {
        const decimal BOARD_DEFAULT_WIDTH = 2M;
        const decimal BOARD_DEFAULT_HEIGHT = 2M;
        const decimal BALL_DEFAULT_WIDTH = 0.2M;
        const decimal BALL_DEFAULT_HEIGHT = 0.2M;
        const decimal BALL_DEFAULT_SPEEDX = 0.05M;
        const decimal BALL_DEFAULT_SPEEDY = 0.05M;
        const decimal PADDLE_DEFAULT_WIDTH = 0.05M;
        const decimal PADDLE_DEFAULT_HEIGHT = 0.3M;

        const int DELAY_PER_UPDATE_IN_MSEC = 0; // (1000 / 10000);

        const int WINNING_SCORE = 11;

        private Pong.GameBoard2D _gameBoard;

        private readonly object _scoreLock = new object();
        public Pong.Score Score
        {
            get
            {
                lock (_scoreLock)
                {
                    return _score;
                }
            }

            protected set
            {
                lock (_scoreLock)
                {
                    _score = value;
                }
            }
        }
        public Pong.Score _score;

        private readonly object _isRunningLock = new object();
        public bool IsRunning
        {
            get
            {
                lock (_isRunningLock)
                {
                    return _isRunning;
                }
            }

            protected set
            {
                lock (_isRunningLock)
                {
                    _isRunning = value;
                }
            }
        }
        public bool _isRunning;

        private ILogger _logger;

        public GameInstance(ILogger logger)
        {
            _logger = logger;

            _logger.Debug("GameInstance>>Creat GameBoard2D");
            _gameBoard = new Pong.GameBoard2D(new Size2D(BOARD_DEFAULT_WIDTH, BOARD_DEFAULT_HEIGHT),
                                              new Ball2D(new Position2D(), new Speed2D(), new Size2D(BALL_DEFAULT_WIDTH, BALL_DEFAULT_HEIGHT)),
                                              new Paddle2D(new Position2D(), new Speed2D(), new Size2D(PADDLE_DEFAULT_WIDTH, PADDLE_DEFAULT_HEIGHT)),
                                              new Paddle2D(new Position2D(), new Speed2D(), new Size2D(PADDLE_DEFAULT_WIDTH, PADDLE_DEFAULT_HEIGHT)));

            _score = _gameBoard.Score;
        }

        public void Run()
        {
            IsRunning = true;

            int oldLefScore = _gameBoard.Score.LeftScore;
            int oldRightScore = _gameBoard.Score.RightScore;

            while (Score.LeftScore < WINNING_SCORE && Score.RightScore < WINNING_SCORE)
            {
                // update game engine
                _gameBoard.Update();

                // check if score evolved
                if (oldLefScore != Score.LeftScore || oldRightScore != Score.RightScore)
                {
                    _logger.Debug($"GameInstace {this.GetHashCode()} score updated to ({Score.LeftScore} / {Score.RightScore})");
                }

                oldLefScore = _gameBoard.Score.LeftScore;
                oldRightScore = _gameBoard.Score.RightScore;

                Thread.Sleep(DELAY_PER_UPDATE_IN_MSEC);
            }

            _logger.Information($"GameInstace {this.GetHashCode()} ends at score ({Score.LeftScore} / {Score.RightScore})");
            IsRunning = false;
        }
        public void Stop()
        {
        }

    }
    class GameServer
    {
        private List<GameInstance> _games;
        public int NbGames { get; protected set; }
        private ILogger _logger;
        public GameServer(int numGames, ILogger logger)
        {
            _logger = logger;

            // Creat array of games
            _games = new List<GameInstance>();

            for (int i = 0; i < numGames; i++)
            {
                _logger.Debug($"GameServer>>Creat GameInstance {i}");
                var game = new GameInstance(_logger);

                // Start the game thread
                var thread = new Thread(() => { game.Run(); });
                _logger.Information($"GameServer>>Creat Thread {thread.ManagedThreadId} for GameInstance {i}");
                thread.Start();

                _games.Add(game);
            }
        }
        public void StopGames()
        {
            foreach (var game in _games)
            {
                game.Stop();
            }
        }

        public void UpdateScoresRegularly()
        {
            while (_games.Count > 0)
            {
                foreach (var game in _games.ToList())
                {
                    if (game != null)
                    {
                        int leftScore = game.Score.LeftScore;
                        int rightScore = game.Score.RightScore;
                        Console.WriteLine($"Game {game.GetHashCode()} has a score of {leftScore} / {rightScore}");

                        if (!game.IsRunning)
                        {
                            _logger.Information($"GameServer>>End GameInstance {game.GetHashCode()}");
                            _games.Remove(game);
                        }
                    }
                }
                Thread.Sleep(1000); // Update every 1 second
            }
        }
    }

    class Program
    {
        static void Main()
        {
            ILogger _logger = new LoggerConfiguration()
                                    .MinimumLevel.Debug()
                                    .WriteTo.File("log.txt")
                                    .CreateLogger();

            _logger.Information($"Main>>Start");

            GameServer gameServer = new GameServer(10, _logger);

            //while (true)
            {
                gameServer.UpdateScoresRegularly();
            }

            _logger.Information($"Main<<End");
        }
    }
}