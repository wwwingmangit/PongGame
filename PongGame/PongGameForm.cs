using Pong;
using Serilog;
using System;
using System.Drawing;
using System.Numerics;
using System.Reflection.Metadata;
using System.Windows.Forms;

namespace PongClient
{
    public partial class PongGameForm : Form
    {
        const decimal GAMEBOARD_DEFAULT_WIDTH = 2M;
        const decimal GAMEBOARD_DEFAULT_HEIGHT = 2M;
        const decimal BALL_DEFAULT_WIDTH = 0.1M;
        const decimal BALL_DEFAULT_HEIGHT = 0.1M;
        const decimal PADDLE_DEFAULT_WIDTH = 0.05M;
        const decimal PADDLE_DEFAULT_HEIGHT = 0.3M;

        const int UPDATE_INTERVAL_IN_MSEC = (int)(1M/30M * 1000M);

        private Pong.GameBoard2D _gameBoard;
        private Pong.GameBoard2D GameBoard {  get { return _gameBoard; } }

        private Rectangle Paddle1;
        private Rectangle Paddle2;
        private Rectangle Ball;

        private BufferedGraphicsContext currentContext;
        private BufferedGraphics myBuffer;

        private System.Windows.Forms.Timer Timer = new System.Windows.Forms.Timer();

        int ScreenWidth, ScreenHeight;

        private readonly ILogger _logger;

        public PongGameForm(Serilog.ILogger logger)
        {
            _logger = logger;

            _gameBoard = new Pong.GameBoard2D(new Size2D(GAMEBOARD_DEFAULT_WIDTH, GAMEBOARD_DEFAULT_HEIGHT),
                                             new Ball2D(new Position2D(), new Speed2D(), new Size2D(BALL_DEFAULT_WIDTH, BALL_DEFAULT_HEIGHT)),
                                             new Paddle2D(new Position2D(), new Speed2D(), new Size2D(PADDLE_DEFAULT_WIDTH, PADDLE_DEFAULT_HEIGHT)),
                                             new Paddle2D(new Position2D(), new Speed2D(), new Size2D(PADDLE_DEFAULT_WIDTH, PADDLE_DEFAULT_HEIGHT)),
                                             _logger);


            InitializeComponent();
            InitializeGame();
            InitializeTimer();

            // Initialize double buffered graphics for faster graphics
            this.DoubleBuffered = true;
            currentContext = BufferedGraphicsManager.Current;
            myBuffer = currentContext.Allocate(this.CreateGraphics(), this.DisplayRectangle);
        }

        private void InitializeGame()
        {
            this.Size = new System.Drawing.Size(400, 400);

            ScreenWidth = this.ClientSize.Width;
            ScreenHeight = this.ClientSize.Height;

            // Initialize game elements in screen coordinates
            Paddle1 = CreatScreenRectangle(GameBoard.LeftPaddle.Position, GameBoard.LeftPaddle.Size);
            Paddle2 = CreatScreenRectangle(GameBoard.RightPaddle.Position, GameBoard.RightPaddle.Size);
            Ball = CreatScreenRectangle(GameBoard.Ball.Position, GameBoard.Ball.Size);
        }

        private Rectangle CreatScreenRectangle(Position2D position, Size2D size)
        {
            return new Rectangle(
                ConvertToScreenX(position.X) - ConvertToScreenWidth(size.Width) / 2,
                ConvertToScreenY(position.Y) - ConvertToScreenHeight(size.Height) / 2,
                ConvertToScreenWidth(size.Width),
                ConvertToScreenHeight(size.Height)
            );
        }
        private void UpdateScreenRectangle(Position2D position, Size2D size, ref Rectangle screenRect)
        {
            screenRect.X = ConvertToScreenX(position.X) - ConvertToScreenWidth(size.Width) / 2;
            screenRect.Y = ConvertToScreenY(position.Y) - ConvertToScreenHeight(size.Height) / 2;
            screenRect.Width = ConvertToScreenWidth(size.Width);
            screenRect.Height = ConvertToScreenHeight(size.Height);
        }

        private int ConvertToScreenX(decimal gameX)
        {
            return (int)(gameX / GameBoard.Size.Width * (decimal)ScreenWidth + (decimal)ScreenWidth / 2);
        }

        private int ConvertToScreenY(decimal gameY)
        {
            return (int)(gameY / GameBoard.Size.Height * (decimal)ScreenHeight + (decimal)ScreenHeight / 2);
        }

        private int ConvertToScreenWidth(decimal gameWidth)
        {
            return (int)(gameWidth / GameBoard.Size.Width * (decimal)ScreenWidth);
        }

        private int ConvertToScreenHeight(decimal gameHeight)
        {
            return (int)(gameHeight / GameBoard.Size.Height * (decimal)ScreenHeight);
        }

        private void InitializeTimer()
        {
            Timer = new System.Windows.Forms.Timer();
            Timer.Interval = 1000 / 30;
            Timer.Tick += new EventHandler(UpdateGame);
            Timer.Start();
        }

        private void UpdateGame(object? sender, EventArgs e)
        {
            // update game
            GameBoard.Update();

            // update screen
            UpdateScreenRectangle(GameBoard.LeftPaddle.Position, GameBoard.LeftPaddle.Size, ref Paddle1);
            UpdateScreenRectangle(GameBoard.RightPaddle.Position, GameBoard.RightPaddle.Size, ref Paddle2);
            UpdateScreenRectangle(GameBoard.Ball.Position, GameBoard.Ball.Size, ref Ball);

            // tell to repaint all the form
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // prepare game board buffer
            Graphics g = myBuffer.Graphics;
            g.Clear(Color.Black);

            // Draw the dotted line (net) in the middle
            using (Pen pen = new Pen(Color.White))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                int middleX = this.ClientSize.Width / 2;
                g.DrawLine(pen, middleX, 0, middleX, this.ClientSize.Height);
            }

            // Draw paddles and ball
            g.FillRectangle(Brushes.White, Paddle1);
            g.FillRectangle(Brushes.White, Paddle2);
            g.FillRectangle(Brushes.White, Ball);

            // Draw the scores
            using (Font font = new Font("Arial", 18))
            {
                g.DrawString(GameBoard.Score.LeftScore.ToString(), font, Brushes.White, new PointF(this.ClientSize.Width / 4, 10));
                g.DrawString(GameBoard.Score.RightScore.ToString(), font, Brushes.White, new PointF(3 * this.ClientSize.Width / 4, 10));

                //g.DrawString(GameBoard.PaddleFatigue.ToString("F2"), font, Brushes.White, new PointF(this.ClientSize.Width / 2, 20));
            }

            myBuffer.Render(e.Graphics);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (myBuffer != null)
            {
                myBuffer.Dispose();
                myBuffer = currentContext.Allocate(this.CreateGraphics(), this.DisplayRectangle);

                ScreenWidth = this.ClientSize.Width;
                ScreenHeight = this.ClientSize.Height;
            }
        }

        private void PongGameForm_Load(object sender, EventArgs e)
        {

        }
    }
}
