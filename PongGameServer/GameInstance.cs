using Serilog;
using Pong;
using System.Net.NetworkInformation;

namespace PongGameServer
{
    public class GameInstance
    {
        public const decimal BOARD_DEFAULT_WIDTH = 2M;
        public const decimal BOARD_DEFAULT_HEIGHT = 2M;
        public const decimal BALL_DEFAULT_WIDTH = 0.2M;
        public const decimal BALL_DEFAULT_HEIGHT = 0.2M;
        public const decimal BALL_DEFAULT_SPEEDX = 0.05M;
        public const decimal BALL_DEFAULT_SPEEDY = 0.05M;
        public const decimal PADDLE_DEFAULT_WIDTH = 0.05M;
        public const decimal PADDLE_DEFAULT_HEIGHT = 0.3M;

        public const int UPDATE_DEFAULT_DELAY_IN_MSEC = (1000 / 30);

        public const int DEFAULT_WINNING_SCORE = 11;

        private Pong.GameBoard2D _gameBoard;
        public enum StatusType
        {
            Initiated,
            Playing,
            Stopped
        }
        public StatusType Status
        {
            get
            {
                lock (_stateLock)
                {
                    return _status;
                }
            }

            protected set
            {
                lock (_stateLock)
                {
                    _status = value;
                }
            }
        }

        private readonly object _scoreLock = new object();
        public Pong.Score Score
        {
            get
            {
                lock (_scoreLock)
                {
                    return _gameBoard.Score;
                }
            }
        }

        private readonly object _stateLock = new object();
        private StatusType _status;

        private ILogger _logger;
        private int _updateDelayInMSec;
        private int _winningScore;

        public GameInstance(Serilog.ILogger logger,
                            int updateDelayInMSec = UPDATE_DEFAULT_DELAY_IN_MSEC,
                            int winningScore = DEFAULT_WINNING_SCORE)
        {
            _logger = logger;

            _updateDelayInMSec = updateDelayInMSec;
            _winningScore = winningScore;

            _gameBoard = new Pong.GameBoard2D(new Size2D(BOARD_DEFAULT_WIDTH, BOARD_DEFAULT_HEIGHT),
                                              new Ball2D(new Position2D(), new Speed2D(), new Size2D(BALL_DEFAULT_WIDTH, BALL_DEFAULT_HEIGHT)),
                                              new Paddle2D(new Position2D(), new Speed2D(), new Size2D(PADDLE_DEFAULT_WIDTH, PADDLE_DEFAULT_HEIGHT)),
                                              new Paddle2D(new Position2D(), new Speed2D(), new Size2D(PADDLE_DEFAULT_WIDTH, PADDLE_DEFAULT_HEIGHT)),
                                              _logger);

            Status = StatusType.Initiated;

            _logger.Information($"GameInstance>>New GameInstance created {this.GetHashCode()}");
        }

        public void Run()
        {
            Status = StatusType.Playing;

            (int oldLefScore, int oldRightScore) = (_gameBoard.Score.LeftScore, _gameBoard.Score.RightScore);

            while (Score.LeftScore < _winningScore && Score.RightScore < _winningScore
                   && Status != StatusType.Stopped)
            {
                // update game engine
                lock (_scoreLock)
                {
                    _gameBoard.Update();
                }

                // check if score evolved
                if (oldLefScore != Score.LeftScore || oldRightScore != Score.RightScore)
                {
                    _logger.Debug($"GameInstace {this.GetHashCode()} score updated to ({Score.LeftScore} / {Score.RightScore})");
                }
                (oldLefScore, oldRightScore) = (_gameBoard.Score.LeftScore, _gameBoard.Score.RightScore);

                // delay execution of the thread handling the game
                Thread.Sleep(_updateDelayInMSec);
            }

            _logger.Information($"GameInstace {this.GetHashCode()} ends at score ({Score.LeftScore} / {Score.RightScore})");
            Stop();
        }
        public void Stop()
        {
            Status = StatusType.Stopped;
        }
    }
}
