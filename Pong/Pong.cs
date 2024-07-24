using System;
using System.Data;

namespace Pong
{
    public class Position
    {
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public Position(decimal x = 0, decimal y = 0)
        { 
            X = x; 
            Y = y; 
        }
    }
    public class Speed
    {
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public Speed(decimal x = 0, decimal y = 0)
        { 
            X = x; 
            Y = y; 
        }
    }
    public class Paddle
    {
        public Position Position { get; }
        public Speed Speed { get; }
        public decimal Width { get; }
        public decimal Height { get; }
        public Paddle(Position position, Speed speed, decimal width, decimal height)
        { 
            Position = position;
            Speed = speed;
            Width = width;
            Height = height;
        }
    }
    public class Ball
    {
        public Position Position { get; }
        public Speed Speed { get; }
        public decimal Width { get; }
        public decimal Height { get; }
        public Ball(Position position, Speed speed, decimal width, decimal height)
        {
            Position = position;
            Speed = speed;
            Width = width;
            Height = height;
        }
    }
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
    public class GameBoard
    {
        static Random RandomGenerator = new Random(DateTime.Now.Millisecond);
        public decimal MinX { get; protected set; }
        public decimal MaxX { get; protected set; }
        public decimal MinY { get; protected set; }
        public decimal MaxY { get; protected set; }
        public decimal BoardWidth { get; protected set; }
        public decimal BoardHeight { get; protected set; }
        public Paddle LeftPaddle { get; protected set; }
        public Paddle RightPaddle { get; protected set; }
        private decimal Fatigue { get; set; }
        public Ball Ball { get; protected set; }
        public Score Score { get; protected set; }
        protected bool NeedToRestartGame { get; private set; }
        public GameBoard(decimal paddleWidth, decimal paddleHeight, decimal ballWidth, decimal ballHeight)
        {
            // Board size
            MinX = -1;
            MaxX = +1;
            BoardWidth = MaxX - MinX;

            MinY = -1;
            MaxY = +1;
            BoardHeight = MaxY - MinY;

            // Creat Ball and Paddles
            Ball = new Ball(new Position(), new Speed(), ballWidth, ballHeight);
            LeftPaddle = new Paddle(new Position(), new Speed(), paddleWidth, paddleHeight);
            RightPaddle = new Paddle(new Position(), new Speed(), paddleWidth, paddleHeight);

            // Reset Ball and Paddles positions/speeds..
            ResetBall();
            ResetPaddles();

            // Set Score to 0
            Score = new Score();
        }
        private void ResetBall()
        {
            Ball.Position.X = Ball.Position.Y = 0;

            Ball.Speed.X = 0.1M * (RandomGenerator.Next(2) == 0 ? 1M : -1M);
            Ball.Speed.Y = ((decimal)(RandomGenerator.NextDouble()) * 0.1M) * (RandomGenerator.Next(2) == 0 ? 1M : -1M);
        }
        private void ResetPaddles()
        {
            LeftPaddle.Position.X = MinX + LeftPaddle.Width/2;
            LeftPaddle.Position.Y = 0;
            LeftPaddle.Speed.X = LeftPaddle.Speed.Y = 0;

            RightPaddle.Position.X = MaxX - RightPaddle.Width / 2;
            RightPaddle.Position.Y = 0;
            RightPaddle.Speed.X = RightPaddle.Speed.Y = 0;

            Fatigue = 1M;
        }
        public void Update()
        {
            if (NeedToRestartGame)
            {
                ResetBall();
                ResetPaddles();

                NeedToRestartGame = false;
            }

            UpdateBallAndPaddles();

            // check if game is over
            (bool gameIsOver, bool winnerIsLeftPaddle) = CheckGameOver2();
            if (gameIsOver)
            {
                NeedToRestartGame = true;

                if (winnerIsLeftPaddle)
                {
                    Score.IncLeftScore();
                }
                else
                {
                    Score.IncRightScore();
                }
            }
        }
        private void UpdateBallAndPaddles()
        {
            // Update Ball position.
            // Make it bounce on Top/Bottom borders
            Ball.Position.X += Ball.Speed.X;
            Ball.Position.Y += Ball.Speed.Y;

            Ball.Position.Y = Math.Clamp(Ball.Position.Y + Ball.Speed.Y, MinY, MaxY);
            if (Ball.Position.Y == MinY || Ball.Position.Y == MaxY)
            {
                Ball.Speed.Y = -Ball.Speed.Y;
            }

            // Compute new paddles position
            if (Ball.Speed.X < 0)
            {
                // update left paddle when ball moves left
                LeftPaddle.Speed.Y = Ball.Position.Y > LeftPaddle.Position.Y ? +Math.Abs(Ball.Speed.Y) : -Math.Abs(Ball.Speed.Y);
                LeftPaddle.Speed.Y *= 2 * Fatigue;
                LeftPaddle.Position.Y += LeftPaddle.Speed.Y;
                LeftPaddle.Position.Y = Math.Clamp(LeftPaddle.Position.Y, MinY, MaxY);                
            }
            else 
            {
                // update right paddle when ball moves right
                RightPaddle.Speed.Y = Ball.Position.Y > RightPaddle.Position.Y ? +Math.Abs(Ball.Speed.Y) : -Math.Abs(Ball.Speed.Y);
                RightPaddle.Speed.Y *= 2 * Fatigue;
                RightPaddle.Position.Y += RightPaddle.Speed.Y;
                RightPaddle.Position.Y = Math.Clamp(RightPaddle.Position.Y, MinY, MaxY);
            }

            // Make ball bounce if paddles touched
            if (Ball.Position.X - Ball.Width / 2 <= LeftPaddle.Position.X + LeftPaddle.Width / 2 &&
                Ball.Position.Y >= LeftPaddle.Position.Y - LeftPaddle.Height / 2 &&
                Ball.Position.Y <= LeftPaddle.Position.Y + LeftPaddle.Height / 2)
            {
                Ball.Position.X = LeftPaddle.Position.X + LeftPaddle.Width / 2 + Ball.Width / 2;
                Ball.Speed.X = -Ball.Speed.X;
            }
            if (Ball.Position.X + Ball.Width / 2 >= RightPaddle.Position.X - RightPaddle.Width / 2 &&
                Ball.Position.Y >= RightPaddle.Position.Y - RightPaddle.Height / 2 &&
                Ball.Position.Y <= RightPaddle.Position.Y + RightPaddle.Height / 2)
            {
                Ball.Position.X = RightPaddle.Position.X - RightPaddle.Width / 2 - Ball.Width / 2;
                Ball.Speed.X = -Ball.Speed.X;
            }

            // Update paddles speed fatigue.
            Fatigue *= 0.999M;
        }

        private (bool, bool) CheckGameOver2()
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


    }

}
