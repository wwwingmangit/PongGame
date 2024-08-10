using System;
using System.Reflection.Metadata;

namespace Pong
{
    public class Position2D : Position
    {
        public Position2D(decimal x = 0, decimal y = 0) : base(2)
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

    public class Speed2D : Speed
    {
        public Speed2D(decimal x = 0, decimal y = 0) : base(2)
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

    public class Size2D : Size
    {
        public Size2D(decimal width = 1, decimal height = 1) : base(2)
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

    public class Ball2D : Ball<Position2D, Speed2D, Size2D>
    {
        public Ball2D(Position2D position, Speed2D speed, Size2D size) : base(position, speed, size)
        {
        }
    }

    public class Paddle2D : Paddle<Position2D, Speed2D, Size2D>
    {
        public Paddle2D(Position2D position, Speed2D speed, Size2D size) : base(position, speed, size)
        {
        }
    }
    public class GameBoard2D : GameBoard<Ball2D, Paddle2D, Position2D, Speed2D, Size2D>
    {
        public decimal MinX { get; protected set; }
        public decimal MaxX { get; protected set; }
        public decimal MinY { get; protected set; }
        public decimal MaxY { get; protected set; }
        public Size2D Size { get; protected set; }
        public GameBoard2D(Size2D size, Ball2D ball, Paddle2D leftPaddle, Paddle2D rightPaddle, Serilog.ILogger logger)
            : base(ball, leftPaddle, rightPaddle, logger)
        {
            Size = size;

            MinX = -Size.Width / 2;
            MaxX = +Size.Width / 2;
            MinY = -Size.Height / 2;
            MaxY = +Size.Height / 2;
        }
        protected override void ResetGame()
        {
            base.ResetGame();

            Ball.Position.X = Ball.Position.Y = 0;
            Ball.Speed.X = 0.1M * (RandomGenerator.Next(2) == 0 ? 1M : -1M);
            Ball.Speed.Y = ((decimal)(RandomGenerator.NextDouble()) * 0.1M) * (RandomGenerator.Next(2) == 0 ? 1M : -1M);

            LeftPaddle.Position.X = MinX + LeftPaddle.Size.Width * 0.7M;
            LeftPaddle.Position.Y = 0;
            LeftPaddle.Speed.Y = 0;

            RightPaddle.Position.X = MaxX - RightPaddle.Size.Width * 0.7M;
            RightPaddle.Position.Y = 0;
            RightPaddle.Speed.Y = 0;
        }
        protected override void UpdateGame()
        {
            base.UpdateGame();

            // Update Paddles positions. 
            if (Ball.Speed.X < 0)
            {
                // Ball moves left. Play left paddle, center other paddle
                UpdatePaddles(true);
            }
            else
            {
                // Ball moves right. Play right paddle, center other paddle
                UpdatePaddles(false);
            }

            // Update Ball Position. Bounce againt top/bottom
            UpdateBall();
        }
        protected override (bool, bool) CheckGameOver()
        {
            bool gameIsOver = false;
            bool winnerIsLeftPaddle = false;

            if (Ball.Position.X < MinX)
            {
                gameIsOver = true;
                winnerIsLeftPaddle = false;
            }

            if (Ball.Position.X > MaxX)
            {
                gameIsOver = true;
                winnerIsLeftPaddle = true;
            }

            return (gameIsOver, winnerIsLeftPaddle);
        }
        private void UpdatePaddles(bool playLeftPaddle)
        {
            Paddle2D paddle2Play = playLeftPaddle ? LeftPaddle : RightPaddle;
            Paddle2D paddle2Center = playLeftPaddle ? RightPaddle : LeftPaddle;

            var paddleMinY = MinY + paddle2Play.Size.Height / 2;
            var paddleMaxY = MaxY - paddle2Play.Size.Height / 2;

            // Move playing paddle towards the ball
            MovePaddle(paddle2Play, Ball.Position.Y, paddleMinY, paddleMaxY);

            // Re-center other paddle
            MovePaddle(paddle2Center, 0, paddleMinY, paddleMaxY);
        }
        private void MovePaddle(Paddle2D paddle, decimal targetY, decimal minY, decimal maxY)
        {
            paddle.Speed.Y = targetY > paddle.Position.Y ? +Math.Abs(Ball.Speed.Y) : -Math.Abs(Ball.Speed.Y);
            paddle.Speed.Y *= 2 * PaddleFatigue;

            paddle.Position.Y = Math.Abs(paddle.Position.Y - targetY) > Math.Abs(paddle.Speed.Y)
                                ? paddle.Position.Y + paddle.Speed.Y
                                : targetY;
            paddle.Position.Y = Math.Clamp(paddle.Position.Y, minY, maxY);
        }
        private void UpdateBall()
        {
            decimal ballMinY = MinY + Ball.Size.Height * .5M;
            decimal ballMaxY = MaxY - Ball.Size.Height * .5M;

            Ball.Position.X += Ball.Speed.X;
            Ball.Position.Y += Ball.Speed.Y;

            Ball.Position.Y = Math.Clamp(Ball.Position.Y, ballMinY, ballMaxY);
            if (Ball.Position.Y == ballMinY || Ball.Position.Y == ballMaxY)
            {
                Ball.Speed.Y = -Ball.Speed.Y;
            }

            // Update Ball X position. Check bounce againt paddles.
            if (Ball.Position.X - Ball.Size.Width / 2 <= LeftPaddle.Position.X + LeftPaddle.Size.Width / 2 &&
                Ball.Position.Y >= LeftPaddle.Position.Y - LeftPaddle.Size.Height / 2 &&
                Ball.Position.Y <= LeftPaddle.Position.Y + LeftPaddle.Size.Height / 2)
            {
                Ball.Position.X = LeftPaddle.Position.X + LeftPaddle.Size.Width / 2 + Ball.Size.Width / 2;
                Ball.Speed.X = -Ball.Speed.X;
            }
            if (Ball.Position.X + Ball.Size.Width / 2 >= RightPaddle.Position.X - RightPaddle.Size.Width / 2 &&
                Ball.Position.Y >= RightPaddle.Position.Y - RightPaddle.Size.Height / 2 &&
                Ball.Position.Y <= RightPaddle.Position.Y + RightPaddle.Size.Height / 2)
            {
                Ball.Position.X = RightPaddle.Position.X - RightPaddle.Size.Width / 2 - Ball.Size.Width / 2;
                Ball.Speed.X = -Ball.Speed.X;
            }
        }
    }
}
