using System;
using System.Reflection.Metadata;

namespace Pong
{    
    public class Position3D : Position
    {
        public Position3D(decimal x = 0, decimal y = 0, decimal z = 0) : base(3)
        {
            Coordinates[0] = x;
            Coordinates[1] = y;
            Coordinates[2] = z;
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
        public decimal Z
        {
            get => Coordinates[2];
            set => Coordinates[2] = value;
        }
    }

    public class Speed3D : Speed
    {
        public Speed3D(decimal x = 0, decimal y = 0, decimal z = 0) : base(3)
        {
            Components[0] = x;
            Components[1] = y;
            Components[2] = z;
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
        public decimal Z
        {
            get => Components[2];
            set => Components[2] = value;
        }

    }

    public class Size3D : Size
    {
        public Size3D(decimal width = 1, decimal height = 1, decimal depth = 2) : base(3)
        {
            Components[0] = width;
            Components[1] = height;
            Components[2] = depth;
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
        public decimal Depth
        {
            get => Components[2];
            set => Components[2] = value;
        }
    }

    public class Ball3D : Ball<Position3D, Speed3D, Size3D>
    {
        public Ball3D(Position3D position, Speed3D speed, Size3D size) : base(position, speed, size)
        {
        }
    }

    public class Paddle3D : Paddle<Position3D, Speed3D, Size3D>
    {
        public Paddle3D(Position3D position, Speed3D speed, Size3D size) : base(position, speed, size)
        {
        }
    }
    public class GameBoard3D : GameBoard<Ball3D, Paddle3D, Position3D, Speed3D, Size3D>
    {
        public decimal MinX { get; protected set; }
        public decimal MaxX { get; protected set; }
        public decimal MinY { get; protected set; }
        public decimal MaxY { get; protected set; }
        public decimal MinZ { get; protected set; }
        public decimal MaxZ { get; protected set; }
        public Size3D Size { get; protected set; }
        public GameBoard3D(Size3D size, Ball3D ball, Paddle3D leftPaddle, Paddle3D rightPaddle, Serilog.ILogger logger)
            : base(ball, leftPaddle, rightPaddle, logger)
        {
            Size = size;

            MinX = -Size.Width / 2;
            MaxX = +Size.Width / 2;
            MinY = -Size.Height / 2;
            MaxY = +Size.Height / 2;
            MinZ = -Size.Depth / 2;
            MaxZ = +Size.Depth / 2;
        }
        protected override void ResetGame()
        {
            base.ResetGame();

            Ball.Position.X = Ball.Position.Y = Ball.Position.Z = 0;
            Ball.Speed.X = 0.05M * (RandomGenerator.Next(2) == 0 ? 1M : -1M);
            Ball.Speed.Y = ((decimal)(RandomGenerator.NextDouble()) * 0.05M) * (RandomGenerator.Next(2) == 0 ? 1M : -1M);
            Ball.Speed.Z = 0.05M * (RandomGenerator.Next(2) == 0 ? 1M : -1M);

            LeftPaddle.Position.X = MinX + LeftPaddle.Size.Width * 0.7M;
            LeftPaddle.Position.Y = 0;
            LeftPaddle.Position.Z = MinZ + LeftPaddle.Size.Depth * 0.7M;
            LeftPaddle.Speed.Y = 0;

            RightPaddle.Position.X = MaxX - RightPaddle.Size.Width * 0.7M;
            RightPaddle.Position.Y = 0;
            RightPaddle.Position.Z = MaxZ + RightPaddle.Size.Width * 0.7M;
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
            Paddle3D paddle2Play = playLeftPaddle ? LeftPaddle : RightPaddle;
            Paddle3D paddle2Center = playLeftPaddle ? RightPaddle : LeftPaddle;

            var paddleMinY = MinY + paddle2Play.Size.Height / 2;
            var paddleMaxY = MaxY - paddle2Play.Size.Height / 2;
            var paddleMinZ = MinZ + paddle2Play.Size.Depth / 2;
            var paddleMaxZ = MaxZ - paddle2Play.Size.Depth / 2;

            // Move playing paddle towards the ball
            MovePaddle3D(paddle2Play, Ball.Position.Y, Ball.Position.Z, paddleMinY, paddleMaxY, paddleMinZ, paddleMaxZ);

            // Re-center other paddle
            MovePaddle3D(paddle2Center, 0, 0, paddleMinY, paddleMaxY, paddleMinZ, paddleMaxZ);
        }
        private void MovePaddle3D(Paddle3D paddle, decimal targetY, decimal targetZ, decimal minY, decimal maxY, decimal minZ, decimal maxZ)
        {
            // Y-axis movement
            paddle.Speed.Y = targetY > paddle.Position.Y ? +Math.Abs(Ball.Speed.Y) : -Math.Abs(Ball.Speed.Y);
            paddle.Speed.Y *= 2 * PaddleFatigue;
            paddle.Position.Y = Math.Abs(paddle.Position.Y - targetY) > Math.Abs(paddle.Speed.Y)
                                ? paddle.Position.Y + paddle.Speed.Y
                                : targetY;
            paddle.Position.Y = Math.Clamp(paddle.Position.Y, minY, maxY);

            // Z-axis movement
            paddle.Speed.Z = targetZ > paddle.Position.Z ? +Math.Abs(Ball.Speed.Z) : -Math.Abs(Ball.Speed.Z);
            paddle.Speed.Z *= 2 * PaddleFatigue;
            paddle.Position.Z = Math.Abs(paddle.Position.Z - targetZ) > Math.Abs(paddle.Speed.Z)
                                ? paddle.Position.Z + paddle.Speed.Z
                                : targetZ;
            paddle.Position.Z = Math.Clamp(paddle.Position.Z, minZ, maxZ);
        }
        private void UpdateBall()
        {
            decimal ballMinY = MinY + Ball.Size.Height * .5M;
            decimal ballMaxY = MaxY - Ball.Size.Height * .5M;
            decimal ballMinZ = MinZ + Ball.Size.Depth * .5M;
            decimal ballMaxZ = MaxZ - Ball.Size.Depth * .5M;

            Ball.Position.X += Ball.Speed.X;
            Ball.Position.Y += Ball.Speed.Y;
            Ball.Position.Z += Ball.Speed.Z;

            Ball.Position.Y = Math.Clamp(Ball.Position.Y, ballMinY, ballMaxY);
            if (Ball.Position.Y == ballMinY || Ball.Position.Y == ballMaxY)
            {
                Ball.Speed.Y = -Ball.Speed.Y;
            }
            Ball.Position.Z = Math.Clamp(Ball.Position.Z, ballMinZ, ballMaxZ);
            if (Ball.Position.Z == ballMinY || Ball.Position.Z == ballMaxZ)
            {
                Ball.Speed.Z = -Ball.Speed.Z;
            }

            // Update Ball X position. Check bounce againt paddles.
            if (Ball.Position.X - Ball.Size.Width / 2 <= LeftPaddle.Position.X + LeftPaddle.Size.Width / 2 &&
                Ball.Position.Y >= LeftPaddle.Position.Y - LeftPaddle.Size.Height / 2 &&
                Ball.Position.Y <= LeftPaddle.Position.Y + LeftPaddle.Size.Height / 2 &&
                Ball.Position.Z >= LeftPaddle.Position.Z - LeftPaddle.Size.Depth / 2 &&
                Ball.Position.Z <= LeftPaddle.Position.Z + LeftPaddle.Size.Depth / 2)
            {
                Ball.Position.X = LeftPaddle.Position.X + LeftPaddle.Size.Width / 2 + Ball.Size.Width / 2;
                Ball.Speed.X = -Ball.Speed.X;
            }
            if (Ball.Position.X + Ball.Size.Width / 2 >= RightPaddle.Position.X - RightPaddle.Size.Width / 2 &&
                Ball.Position.Y >= RightPaddle.Position.Y - RightPaddle.Size.Height / 2 &&
                Ball.Position.Y <= RightPaddle.Position.Y + RightPaddle.Size.Height / 2 &&
                Ball.Position.Z >= RightPaddle.Position.Z - RightPaddle.Size.Depth / 2 &&
                Ball.Position.Z <= RightPaddle.Position.Z + RightPaddle.Size.Depth / 2)
            {
                Ball.Position.X = RightPaddle.Position.X - RightPaddle.Size.Width / 2 - Ball.Size.Width / 2;
                Ball.Speed.X = -Ball.Speed.X;
            }
        }
    }
}
