using System;
using System.Reflection.Metadata;

namespace Pong
{
    // Abstract N dimensions position class
    public abstract class Position
    {
        public decimal[] Coordinates { get; }

        protected Position(int dimensions)
        {
            Coordinates = new decimal[dimensions];
        }

        public decimal this[int index]
        {
            get => Coordinates[index];
            set => Coordinates[index] = value;
        }
    }

    // Abstract N dimensions speed class
    public abstract class Speed
    {
        public decimal[] Components { get; }

        protected Speed(int dimensions)
        {
            Components = new decimal[dimensions];
        }

        public decimal this[int index]
        {
            get => Components[index];
            set => Components[index] = value;
        }
    }

    // Abstract N dimensions size class
    public abstract class Size
    {
        public decimal[] Components { get; }

        protected Size(int dimensions)
        {
            Components = new decimal[dimensions];
        }

        public decimal this[int index]
        {
            get => Components[index];
            set => Components[index] = value;
        }
    }

    // Abstract Paddle class with generics
    public abstract class Paddle<TPosition, TSpeed, TSize>
        where TPosition : Position
        where TSpeed : Speed
        where TSize : Size
    {
        public TPosition Position { get; }
        public TSpeed Speed { get; }
        public TSize Size { get; }

        protected Paddle(TPosition position, TSpeed speed, TSize size)
        {
            Position = position;
            Speed = speed;
            Size = size;
        }
    }

    // Abstract Ball class with generics
    public abstract class Ball<TPosition, TSpeed, TSize>
        where TPosition : Position
        where TSpeed : Speed
        where TSize : Size
    {
        public TPosition Position { get; }
        public TSpeed Speed { get; }
        public TSize Size { get; }

        protected Ball(TPosition position, TSpeed speed, TSize size)
        {
            Position = position;
            Speed = speed;
            Size = size;
        }
    }

    // Score class
    public class Score
    {
        public int LeftScore { get; protected set; }
        public int RightScore { get; protected set; }

        public int IncLeftScore()
        {
            return ++LeftScore;
        }

        public int IncRightScore()
        {
            return ++RightScore;
        }

        public Score(int leftScore = 0, int rightScore = 0)
        {
            LeftScore = leftScore;
            RightScore = rightScore;
        }
    }

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

    public abstract class GameBoard<TBall, TPaddle, TPosition, TSpeed, TSize>
        where TBall : Ball<TPosition, TSpeed, TSize>
        where TPaddle : Paddle<TPosition, TSpeed, TSize>
        where TPosition : Position
        where TSpeed : Speed
        where TSize : Size
    {
        public static Random RandomGenerator = new Random(DateTime.Now.Millisecond);
        public TBall Ball { get; protected set; }
        public TPaddle LeftPaddle { get; protected set; }
        public TPaddle RightPaddle { get; protected set; }
        public Score Score { get; protected set; }
        public bool NeedToResetGame { get; protected set; }
        public decimal PaddleFatigue { get; protected set; }
        protected GameBoard(TBall ball, TPaddle leftPaddle, TPaddle rightPaddle)
        {
            Ball = ball;
            LeftPaddle = leftPaddle;
            RightPaddle = rightPaddle;

            Score = new Score();

            NeedToResetGame = true;
        }

        protected virtual void ResetParty()
        {
            PaddleFatigue = 1M;
        }

        public void Update()
        {
            // reset game if needed
            if (NeedToResetGame)
            {
                ResetParty();
                NeedToResetGame = false;
            }

            // update game
            UpdateParty();

            // check if party is over
            (bool gameIsOver, bool winnerIsLeftPaddle) = CheckPartyOver();
            if (gameIsOver)
            {
                if (winnerIsLeftPaddle)
                {
                    Score.IncLeftScore();
                }
                else
                {
                    Score.IncRightScore();
                }

                NeedToResetGame = true;
            }
        }
        protected virtual void UpdateParty()
        {
            PaddleFatigue *= 0.999M;
        }
        protected virtual (bool, bool) CheckPartyOver()
        {
            bool gameIsOver = false;
            bool winnerIsLeftPaddle = false;

            return (gameIsOver, winnerIsLeftPaddle);
        }
    }

    public class GameBoard2D : GameBoard<Ball2D, Paddle2D, Position2D, Speed2D, Size2D>
    {
        public decimal MinX { get; protected set; }
        public decimal MaxX { get; protected set; }
        public decimal MinY { get; protected set; }
        public decimal MaxY { get; protected set; }
        public Size2D Size { get; protected set; }
        public GameBoard2D(Size2D size, Ball2D ball, Paddle2D leftPaddle, Paddle2D rightPaddle)
            : base(ball, leftPaddle, rightPaddle)
        {
            Size = size;

            MinX = -Size.Width / 2;
            MaxX = +Size.Width / 2;
            MinY = -Size.Height / 2;
            MaxY = +Size.Height / 2;
        }
        protected override void ResetParty()
        {
            base.ResetParty();

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
        protected override void UpdateParty()
        {
            base.UpdateParty();

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
        protected override (bool, bool) CheckPartyOver()
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
            // choose paddle to play and to center
            Paddle2D paddle2Play = playLeftPaddle ? LeftPaddle : RightPaddle;
            Paddle2D paddle2Center = playLeftPaddle ? RightPaddle : LeftPaddle;

            var paddleMinY = MinY + paddle2Play.Size.Height / 2;
            var paddleMaxY = MaxY - paddle2Play.Size.Height / 2;

            // Update playing paddle in direction of the ball
            paddle2Play.Speed.Y = Ball.Position.Y > paddle2Play.Position.Y ? +Math.Abs(Ball.Speed.Y) : -Math.Abs(Ball.Speed.Y);
            paddle2Play.Speed.Y *= 2 * PaddleFatigue;

            paddle2Play.Position.Y = Math.Abs(paddle2Play.Position.Y - Ball.Position.Y) > Math.Abs(paddle2Play.Speed.Y)
                                             ? paddle2Play.Position.Y + paddle2Play.Speed.Y
                                             : Ball.Position.Y;
            paddle2Play.Position.Y = Math.Clamp(paddle2Play.Position.Y, paddleMinY, paddleMaxY);

            // Re-center paddle 
            paddle2Center.Speed.Y = paddle2Center.Position.Y > 0 ? -Math.Abs(Ball.Speed.Y) : +Math.Abs(Ball.Speed.Y);
            paddle2Center.Position.Y = Math.Abs(paddle2Center.Position.Y) > Math.Abs(paddle2Center.Speed.Y)
                                    ? paddle2Center.Position.Y + paddle2Center.Speed.Y
                                    : 0;
            paddle2Center.Position.Y = Math.Clamp(paddle2Center.Position.Y, paddleMinY, paddleMaxY);
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
