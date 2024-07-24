using Pong;

namespace PongClient
{
    public partial class PongGameForm : Form
    {
        private Pong.GameBoard GameBoard = new GameBoard(0.05M, 0.2M, 0.05M, 0.05M);

        private Rectangle Paddle1;
        private Rectangle Paddle2;
        private Rectangle Ball;

        private BufferedGraphicsContext currentContext;
        private BufferedGraphics myBuffer;

        private System.Windows.Forms.Timer Timer = new System.Windows.Forms.Timer();

        int FormWidth, FormHeight;

        public PongGameForm()
        {
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
            this.Size = new Size(400, 400);

            FormWidth = this.ClientSize.Width;
            FormHeight = this.ClientSize.Height;

            // Initialize game elements in screen coordinates
            Pong.Paddle leftPaddle = GameBoard.LeftPaddle;
            Pong.Paddle rightPaddle = GameBoard.RightPaddle;
            Pong.Ball ball = GameBoard.Ball;

            Paddle1 = new Rectangle(ConvertToScreenX(leftPaddle.Position.X) - ConvertToScreenWidth(leftPaddle.Width) / 2,
                                    ConvertToScreenY(leftPaddle.Position.Y) - ConvertToScreenHeight(leftPaddle.Height) / 2,
                                    ConvertToScreenWidth(leftPaddle.Width),
                                    ConvertToScreenHeight(leftPaddle.Height));
            Paddle2 = new Rectangle(ConvertToScreenX(rightPaddle.Position.X) - ConvertToScreenWidth(rightPaddle.Width) / 2,
                                    ConvertToScreenY(rightPaddle.Position.Y) - ConvertToScreenHeight(rightPaddle.Height) / 2,
                                    ConvertToScreenWidth(rightPaddle.Width),
                                    ConvertToScreenHeight(rightPaddle.Height));

            Ball = new Rectangle(ConvertToScreenX(ball.Position.X) - ConvertToScreenWidth(ball.Width) / 2,
                                 ConvertToScreenY(ball.Position.Y) - ConvertToScreenHeight(ball.Height) / 2,
                                 ConvertToScreenWidth(ball.Width),
                                 ConvertToScreenHeight(ball.Height));
        }

        private int ConvertToScreenX(decimal gameX)
        {
            return (int)(gameX / GameBoard.BoardWidth * (decimal)FormWidth + (decimal)FormWidth / 2);
        }
        private int ConvertToScreenY(decimal gameY)
        {
            return (int)(gameY / GameBoard.BoardHeight * (decimal)FormHeight + (decimal)FormHeight / 2);
        }

        private int ConvertToScreenWidth(decimal gameWidth)
        {
            return (int)(gameWidth / GameBoard.BoardWidth * (decimal)FormWidth);
        }

        private int ConvertToScreenHeight(decimal gameHeight)
        {
            return (int)(gameHeight / GameBoard.BoardHeight * (decimal)FormHeight);
        }

        private void InitializeTimer()
        {
            Timer = new System.Windows.Forms.Timer();
            Timer.Interval = 1000/30; // Adjust as needed
            Timer.Tick += new EventHandler(UpdateGame);
            Timer.Start();
        }

        private void UpdateGame(object? sender, EventArgs e)
        {
            GameBoard.Update();

            Pong.Paddle leftPaddle = GameBoard.LeftPaddle;
            Pong.Paddle rightPaddle = GameBoard.RightPaddle;
            Pong.Ball ball = GameBoard.Ball;

            Paddle1.X = ConvertToScreenX(leftPaddle.Position.X) - ConvertToScreenWidth(leftPaddle.Width) / 2;
            Paddle1.Y = ConvertToScreenY(leftPaddle.Position.Y) - ConvertToScreenHeight(leftPaddle.Height) / 2;
            Paddle1.Width = ConvertToScreenWidth(leftPaddle.Width);
            Paddle1.Height = ConvertToScreenHeight(leftPaddle.Height);

            Paddle2.X = ConvertToScreenX(rightPaddle.Position.X) - ConvertToScreenWidth(rightPaddle.Width) / 2;
            Paddle2.Y = ConvertToScreenY(rightPaddle.Position.Y) - ConvertToScreenHeight(rightPaddle.Height) / 2;
            Paddle2.Width = ConvertToScreenWidth(rightPaddle.Width);
            Paddle2.Height = ConvertToScreenHeight(rightPaddle.Height);

            Ball.X = ConvertToScreenX(ball.Position.X) - ConvertToScreenWidth(ball.Width) / 2;
            Ball.Y = ConvertToScreenY(ball.Position.Y) - ConvertToScreenHeight(ball.Height) / 2;
            Ball.Width = ConvertToScreenWidth(ball.Width); 
            Ball.Height = ConvertToScreenHeight(ball.Height); 

            // repaint the form
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

                FormWidth = this.ClientSize.Width;
                FormHeight = this.ClientSize.Height;
            }
        }
    }
}
