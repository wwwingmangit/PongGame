using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using Pong;

namespace Pong3DOpenTK
{
    partial class Pong3DOpenTK : GameWindow
    {
        const decimal GAMEBOARD_DEFAULT_WIDTH = 2M;
        const decimal GAMEBOARD_DEFAULT_HEIGHT = 2M;
        const decimal GAMEBOARD_DEFAULT_DEPTH = 2M;
        const decimal GAMEBALL_DEFAULT_WIDTH = 0.2M;
        const decimal GAMEBALL_DEFAULT_HEIGHT = 0.2M;
        const decimal GAMEBALL_DEFAULT_DEPTH = 0.2M;
        const decimal GAMEBALL_DEFAULT_SPEEDX = 0.05M;
        const decimal GAMEBALL_DEFAULT_SPEEDY = 0.05M;
        const decimal GAMEBALL_DEFAULT_SPEEDZ = 0.05M;
        const decimal GAMEPADDLE_DEFAULT_WIDTH = 0.05M;
        const decimal GAMEPADDLE_DEFAULT_HEIGHT = 0.3M;
        const decimal GAMEPADDLE_DEFAULT_DEPTH = 0.3M;

        private Pong.GameBoard3D GameBoard = new Pong.GameBoard3D(new Size3D(GAMEBOARD_DEFAULT_WIDTH, GAMEBOARD_DEFAULT_HEIGHT, GAMEBOARD_DEFAULT_DEPTH),
                                             new Ball3D(new Position3D(), new Speed3D(), new Size3D(GAMEBALL_DEFAULT_WIDTH, GAMEBALL_DEFAULT_HEIGHT, GAMEBALL_DEFAULT_DEPTH)),
                                             new Paddle3D(new Position3D(), new Speed3D(), new Size3D(GAMEPADDLE_DEFAULT_WIDTH, GAMEPADDLE_DEFAULT_HEIGHT, GAMEPADDLE_DEFAULT_DEPTH)),
                                             new Paddle3D(new Position3D(), new Speed3D(), new Size3D(GAMEPADDLE_DEFAULT_WIDTH, GAMEPADDLE_DEFAULT_HEIGHT, GAMEPADDLE_DEFAULT_DEPTH)));

        private int _ballVertexArrayObject;
        private int _objectShaderProgram;
        private int _scoreShaderProgram;
        private Matrix4 _projection;
        private Matrix4 _view;
        private float _cameraAngle = 0.0f;

        private Vector3 _ballPosition = new Vector3();
        private Vector3 _leftPaddlePosition = new Vector3();
        private Vector3 _rightPaddlePosition = new Vector3();

        private const int TargetFPS = 60;
        private const double TargetFrameTime = 1.0 / TargetFPS;
        private Stopwatch _stopwatch;

        private int _gameBoardVertexArrayObject;
        private int _leftPaddleVertexArrayObject;
        private int _rightPaddleVertexArrayObject;

        private int _scoreTexture;
        private int _scoreVertexArrayObject;
        private Vector3 _leftScorePosition;
        private Vector3 _rightScorePosition;

        public Pong3DOpenTK(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _stopwatch = new Stopwatch();
        }
        protected override void OnLoad()
        {
            base.OnLoad();

            _objectShaderProgram = CreateShaderProgram(objectVertexShaderSource, objectFragmentShaderSource);
            _scoreShaderProgram = CreateShaderProgram(scoreVertexShaderSource, scoreFragmentShaderSource);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f); // Set background to black
            GL.Enable(EnableCap.DepthTest);

            _ballVertexArrayObject = CreateBall(GameBoard.Ball.Size);
            _gameBoardVertexArrayObject = CreateScreenGameBoard(GameBoard.Size);
            _leftPaddleVertexArrayObject = CreatePaddle(GameBoard.LeftPaddle.Size);
            _rightPaddleVertexArrayObject = CreatePaddle(GameBoard.RightPaddle.Size);

            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), ClientSize.X / (float)ClientSize.Y, 0.1f, 100.0f);

            float cameraDistance = (float)GAMEBOARD_DEFAULT_DEPTH * 3f; // Increase the distance from the box
            _view = Matrix4.LookAt(new Vector3(0.0f, 0.0f, cameraDistance), Vector3.Zero, Vector3.UnitY);

            _scoreTexture = LoadTexture("numbers.png");
            _scoreVertexArrayObject = CreateScoreSurface();

            _stopwatch.Start();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Rotate the camera around the Y-axis
            _cameraAngle += 0.01f; // Adjust the speed of rotation
            float radius = (float)GAMEBOARD_DEFAULT_DEPTH * 2f;
            float camX = (float)Math.Sin(_cameraAngle) * radius;
            float camZ = (float)Math.Cos(_cameraAngle) * radius;
            _view = Matrix4.LookAt(new Vector3(camX, 0.0f, camZ), Vector3.Zero, Vector3.UnitY);

            // Use dynamic view and projection
            GL.UseProgram(_objectShaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(_objectShaderProgram, "view"), false, ref _view);
            GL.UniformMatrix4(GL.GetUniformLocation(_objectShaderProgram, "projection"), false, ref _projection);

            // Set metallic white color for objects
            Vector3 metallicWhite = new Vector3(0.9f, 0.9f, 0.9f);
            GL.Uniform3(GL.GetUniformLocation(_objectShaderProgram, "lightPos"), new Vector3(1.2f, 1.0f, 2.0f));
            GL.Uniform3(GL.GetUniformLocation(_objectShaderProgram, "viewPos"), new Vector3(camX, 0.0f, camZ));
            GL.Uniform3(GL.GetUniformLocation(_objectShaderProgram, "lightColor"), new Vector3(1.5f, 1.5f, 1.5f));

            // Render the ball
            var ballModel = Matrix4.CreateTranslation(_ballPosition);
            GL.UniformMatrix4(GL.GetUniformLocation(_objectShaderProgram, "model"), false, ref ballModel);
            GL.Uniform3(GL.GetUniformLocation(_objectShaderProgram, "objectColor"), metallicWhite);
            GL.BindVertexArray(_ballVertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 16 * 16 * 6); // Adjust according to the sphere's vertex count

            // Render the first paddle
            var leftPaddleModel = Matrix4.CreateTranslation(_leftPaddlePosition);
            GL.UniformMatrix4(GL.GetUniformLocation(_objectShaderProgram, "model"), false, ref leftPaddleModel);
            GL.Uniform3(GL.GetUniformLocation(_objectShaderProgram, "objectColor"), metallicWhite);
            GL.BindVertexArray(_leftPaddleVertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36); // Adjust according to the paddle's vertex count

            // Render the second paddle
            var rightPaddleModel = Matrix4.CreateTranslation(_rightPaddlePosition);
            GL.UniformMatrix4(GL.GetUniformLocation(_objectShaderProgram, "model"), false, ref rightPaddleModel);
            GL.Uniform3(GL.GetUniformLocation(_objectShaderProgram, "objectColor"), metallicWhite);
            GL.BindVertexArray(_rightPaddleVertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36); // Adjust according to the paddle's vertex count

            // Render the game board
            var gameBoardModel = Matrix4.Identity; // Keep the game board static
            GL.UniformMatrix4(GL.GetUniformLocation(_objectShaderProgram, "model"), false, ref gameBoardModel);
            GL.Uniform3(GL.GetUniformLocation(_objectShaderProgram, "objectColor"), new Vector3(1.0f, 1.0f, 1.0f)); // Set line color to white
            GL.BindVertexArray(_gameBoardVertexArrayObject);
            GL.DrawArrays(PrimitiveType.Lines, 0, 24);

            // Render the left score
            GL.UseProgram(_scoreShaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(_scoreShaderProgram, "view"), false, ref _view);
            GL.UniformMatrix4(GL.GetUniformLocation(_scoreShaderProgram, "projection"), false, ref _projection);
            GL.BindTexture(TextureTarget.Texture2D, _scoreTexture);
            GL.BindVertexArray(_scoreVertexArrayObject);

            var scoreModel = Matrix4.CreateTranslation(_leftScorePosition) * _view.ClearTranslation().Inverted();
            GL.UniformMatrix4(GL.GetUniformLocation(_scoreShaderProgram, "model"), false, ref scoreModel);

            float[] vertices;
            UpdateScoreTextureCoordinates(GameBoard.Score.LeftScore, out vertices);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6); // Draw the left score

            // Render the right score
            var rightScoreModel = Matrix4.CreateTranslation(_rightScorePosition) * _view.ClearTranslation().Inverted();
            GL.UniformMatrix4(GL.GetUniformLocation(_scoreShaderProgram, "model"), false, ref rightScoreModel);

            UpdateScoreTextureCoordinates(GameBoard.Score.RightScore, out vertices);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6); // Draw the right score

            SwapBuffers();

            // Frame limiting
            double elapsedTime = _stopwatch.Elapsed.TotalSeconds;
            if (elapsedTime < TargetFrameTime)
            {
                int sleepTime = (int)((TargetFrameTime - elapsedTime) * 1000);
                if (sleepTime > 0)
                    System.Threading.Thread.Sleep(sleepTime);
            }
            _stopwatch.Restart();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // update game
            GameBoard.Update();

            // Update ball position based on game logic
            _ballPosition.X = (float)(GameBoard.Ball.Position.X);
            _ballPosition.Y = (float)(GameBoard.Ball.Position.Y);
            _ballPosition.Z = (float)(GameBoard.Ball.Position.Z);

            // Update paddle positions based on game logic
            _leftPaddlePosition.X = (float)(GameBoard.LeftPaddle.Position.X);
            _leftPaddlePosition.Y = (float)(GameBoard.LeftPaddle.Position.Y);
            _leftPaddlePosition.Z = (float)(GameBoard.LeftPaddle.Position.Z);

            _rightPaddlePosition.X = (float)(GameBoard.RightPaddle.Position.X);
            _rightPaddlePosition.Y = (float)(GameBoard.RightPaddle.Position.Y);
            _rightPaddlePosition.Z = (float)(GameBoard.RightPaddle.Position.Z);

            // Update score position to be in the middle of the box and facing the camera
            _leftScorePosition = new Vector3((float)-GAMEBOARD_DEFAULT_WIDTH / 2, (float)GAMEBOARD_DEFAULT_DEPTH / 2, 0.0f);
            _rightScorePosition = new Vector3((float)+GAMEBOARD_DEFAULT_WIDTH / 2, (float)GAMEBOARD_DEFAULT_DEPTH / 2, 0.0f);
        }
    }
}
