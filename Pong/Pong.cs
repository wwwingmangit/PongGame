﻿using Serilog;

namespace Pong
{
    // Abstract N dimensions position class
    public abstract class Position
    {
        public decimal[] Coordinates { get; }

        protected Position(int dimensions)
        {
            if (dimensions <= 0)
                throw new ArgumentException($"{nameof(Position)}: Dimensions must be a positive integer.", nameof(dimensions));

            Coordinates = new decimal[dimensions];
        }

        public decimal this[int index]
        {
            get
            {
                if (index < 0 || index >= Coordinates.Length)
                    throw new IndexOutOfRangeException($"{nameof(Position)}[{nameof(index)}]: Index is out of range.");
                return Coordinates[index];
            }
            set
            {
                if (index < 0 || index >= Coordinates.Length)
                    throw new IndexOutOfRangeException($"{nameof(Position)}[{nameof(index)}]: Index is out of range.");
                Coordinates[index] = value;
            }
        }
    }

    // Abstract N dimensions speed class
    public abstract class Speed
    {
        public decimal[] Components { get; }

        protected Speed(int dimensions)
        {
            if (dimensions <= 0)
                throw new ArgumentException($"{nameof(Speed)}: Dimensions must be a positive integer.", nameof(dimensions));

            Components = new decimal[dimensions];
        }

        public decimal this[int index]
        {
            get
            {
                if (index < 0 || index >= Components.Length)
                    throw new IndexOutOfRangeException($"{nameof(Speed)}[{nameof(index)}]: Index is out of range.");
                return Components[index];
            }
            set
            {
                if (index < 0 || index >= Components.Length)
                    throw new IndexOutOfRangeException($"{nameof(Speed)}[{nameof(index)}]: Index is out of range.");
                Components[index] = value;
            }
        }
    }

    // Abstract N dimensions size class
    public abstract class Size
    {
        public decimal[] Components { get; }

        protected Size(int dimensions)
        {
            if (dimensions <= 0)
                throw new ArgumentException($"{nameof(Size)}: Dimensions must be a positive integer.", nameof(dimensions));

            Components = new decimal[dimensions];
        }

        public decimal this[int index]
        {
            get
            {
                if (index < 0 || index >= Components.Length)
                    throw new IndexOutOfRangeException($"{nameof(Size)}[{nameof(index)}]: Index is out of range.");
                return Components[index];
            }
            set
            {
                if (index < 0 || index >= Components.Length)
                    throw new IndexOutOfRangeException($"{nameof(Size)}[{nameof(index)}]: Index is out of range.");
                Components[index] = value;
            }
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
            Position = position ?? throw new ArgumentNullException($"{nameof(Paddle<TPosition, TSpeed, TSize>)}.ctor: {nameof(position)} cannot be null.");
            Speed = speed ?? throw new ArgumentNullException($"{nameof(Paddle<TPosition, TSpeed, TSize>)}.ctor: {nameof(speed)} cannot be null.");
            Size = size ?? throw new ArgumentNullException($"{nameof(Paddle<TPosition, TSpeed, TSize>)}.ctor: {nameof(size)} cannot be null.");
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
            Position = position ?? throw new ArgumentNullException($"{nameof(Ball<TPosition, TSpeed, TSize>)}.ctor: {nameof(position)} cannot be null.");
            Speed = speed ?? throw new ArgumentNullException($"{nameof(Ball<TPosition, TSpeed, TSize>)}.ctor: {nameof(speed)} cannot be null.");
            Size = size ?? throw new ArgumentNullException($"{nameof(Ball<TPosition, TSpeed, TSize>)}.ctor: {nameof(size)} cannot be null.");
        }
    }

    // Score class
    public class Score
    {
        public int LeftScore { get; private set; }
        public int RightScore { get; private set; }
        public int IncLeftScore() => ++LeftScore;
        public int IncRightScore() => ++RightScore;
        public Score(int leftScore = 0, int rightScore = 0)
        {
            Update(leftScore, rightScore);
        }
        public Score(Score otherScore) : 
            this(otherScore.LeftScore, otherScore.RightScore) 
        { }
        public void Update(Score otherScore) => Update(otherScore.LeftScore, otherScore.RightScore);
        public void Update(int leftScore, int rightScore)
        {
            if (leftScore < 0)
                throw new ArgumentOutOfRangeException(nameof(leftScore), "Score cannot be negative.");
            if (rightScore < 0)
                throw new ArgumentOutOfRangeException(nameof(rightScore), "Score cannot be negative.");

            LeftScore = leftScore;
            RightScore = rightScore;
        }
    }

    public abstract class GameBoard<TBall, TPaddle, TPosition, TSpeed, TSize>
        where TBall : Ball<TPosition, TSpeed, TSize>
        where TPaddle : Paddle<TPosition, TSpeed, TSize>
        where TPosition : Position
        where TSpeed : Speed
        where TSize : Size
    {
        private const decimal PADDLE_FATIGUE_DECAY = 0.999M;
        private const decimal INITIAL_PADDLE_FATIGUE_DECAY = 1M;

        public static Random RandomGenerator = new Random(DateTime.Now.Millisecond);
        private readonly ILogger _logger;
        public TBall Ball { get; protected set; }
        public TPaddle LeftPaddle { get; protected set; }
        public TPaddle RightPaddle { get; protected set; }
        public Score Score { get; protected set; }
        public bool NeedToResetGame { get; protected set; }
        public decimal PaddleFatigue { get; protected set; }
        protected GameBoard(TBall ball, TPaddle leftPaddle, TPaddle rightPaddle, ILogger logger)
        {
            Ball = ball ?? throw new ArgumentNullException($"{GetType().Name} constructor: {nameof(ball)} cannot be null.");
            LeftPaddle = leftPaddle ?? throw new ArgumentNullException($"{GetType().Name} constructor: {nameof(leftPaddle)} cannot be null.");
            RightPaddle = rightPaddle ?? throw new ArgumentNullException($"{GetType().Name} constructor: {nameof(rightPaddle)} cannot be null.");
            _logger = logger ?? throw new ArgumentNullException($"{GetType().Name} constructor: {nameof(logger)} cannot be null.");

            Score = new Score();
            NeedToResetGame = true;
        }
        protected virtual void ResetGame()
        {
            PaddleFatigue = INITIAL_PADDLE_FATIGUE_DECAY;
        }
        public void Update()
        {
            // reset game if needed
            if (NeedToResetGame)
            {
                ResetGame();
                NeedToResetGame = false;
            }

            // update game
            UpdateGame();

            // check if party is over
            (bool gameIsOver, bool winnerIsLeftPaddle) = CheckGameOver();
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
        protected virtual void UpdateGame()
        {
            PaddleFatigue *= PADDLE_FATIGUE_DECAY;
        }
        protected virtual (bool, bool) CheckGameOver()
        {
            bool gameIsOver = false;
            bool winnerIsLeftPaddle = false;

            return (gameIsOver, winnerIsLeftPaddle);
        }
    }
}
