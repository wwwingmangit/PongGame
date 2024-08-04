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
        public Score(Score otherScore)
        {
            LeftScore = otherScore.LeftScore;
            RightScore = otherScore.RightScore;
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
}
