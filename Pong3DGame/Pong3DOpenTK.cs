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
        const decimal GAMEBALL_DEFAULT_WIDTH = 0.1M;
        const decimal GAMEBALL_DEFAULT_HEIGHT = 0.1M;
        const decimal GAMEBALL_DEFAULT_DEPTH = 0.1M;
        const decimal GAMEBALL_DEFAULT_SPEEDX = 0.05M;
        const decimal GAMEBALL_DEFAULT_SPEEDY = 0.05M;
        const decimal GAMEBALL_DEFAULT_SPEEDZ = 0.05M;
        const decimal GAMEPADDLE_DEFAULT_WIDTH = 0.05M;
        const decimal GAMEPADDLE_DEFAULT_HEIGHT = 0.3M;
        const decimal GAMEPADDLE_DEFAULT_DEPTH = 0.3M;

        private Pong.GameBoard3D GameBoard = new Pong.GameBoard3D(new Size3D(GAMEBOARD_DEFAULT_WIDTH, GAMEBOARD_DEFAULT_HEIGHT, GAMEBOARD_DEFAULT_DEPTH),
                                             new Ball3D(new Position3D(), new Speed3D(GAMEBALL_DEFAULT_SPEEDX, GAMEBALL_DEFAULT_SPEEDY, GAMEBALL_DEFAULT_SPEEDZ), new Size3D(GAMEBALL_DEFAULT_WIDTH, GAMEBALL_DEFAULT_HEIGHT, GAMEBALL_DEFAULT_DEPTH)),
                                             new Paddle3D(new Position3D(), new Speed3D(), new Size3D(GAMEPADDLE_DEFAULT_WIDTH, GAMEPADDLE_DEFAULT_HEIGHT, GAMEPADDLE_DEFAULT_DEPTH)),
                                             new Paddle3D(new Position3D(), new Speed3D(), new Size3D(GAMEPADDLE_DEFAULT_WIDTH, GAMEPADDLE_DEFAULT_HEIGHT, GAMEPADDLE_DEFAULT_DEPTH)));

        private int _ballVertexArrayObject;
        private int _shaderProgram;
        private Matrix4 _projection;
        private Matrix4 _view;
        private float _cameraAngle = 0.0f;

        private Vector3 _ballPosition = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 _leftPaddlePosition = new Vector3(-1.9f, 0.0f, 0.0f); // Initial position based on game board size
        private Vector3 _rightPaddlePosition = new Vector3(1.9f, 0.0f, 0.0f); // Initial position based on game board size

        private const int TargetFPS = 30;
        private const double TargetFrameTime = 1.0 / TargetFPS;
        private Stopwatch _stopwatch;

        private int _gameBoardVertexArrayObject;
        private int _leftPaddleVertexArrayObject;
        private int _rightPaddleVertexArrayObject;

        public Pong3DOpenTK(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _stopwatch = new Stopwatch();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f); // Set background to black
            GL.Enable(EnableCap.DepthTest);

            _ballVertexArrayObject = CreateBall(GameBoard.Ball.Size);
            _gameBoardVertexArrayObject = CreateScreenGameBoard(GameBoard.Size);
            _leftPaddleVertexArrayObject = CreatePaddle(GameBoard.LeftPaddle.Size);
            _rightPaddleVertexArrayObject = CreatePaddle(GameBoard.RightPaddle.Size);

            CreateShaders();

            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), ClientSize.X / (float)ClientSize.Y, 0.1f, 100.0f);
            _view = Matrix4.LookAt(new Vector3(0.0f, 0.0f, (float)GAMEBOARD_DEFAULT_DEPTH * 3f), Vector3.Zero, Vector3.UnitY);

            _stopwatch.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shaderProgram);

            // Rotate the camera around the Y-axis
            _cameraAngle += 0.01f; // Adjust the speed of rotation
            float radius = (float)GAMEBOARD_DEFAULT_DEPTH * 3f;
            float camX = (float)Math.Sin(_cameraAngle) * radius;
            float camZ = (float)Math.Cos(_cameraAngle) * radius;
            _view = Matrix4.LookAt(new Vector3(camX, 0.0f, camZ), Vector3.Zero, Vector3.UnitY);

            // Use dynamic view and projection
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view"), false, ref _view);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref _projection);

            // Set metallic white color for objects
            Vector3 metallicWhite = new Vector3(0.9f, 0.9f, 0.9f);
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "lightPos"), new Vector3(1.2f, 1.0f, 2.0f));
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "viewPos"), new Vector3(camX, 0.0f, camZ));
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "lightColor"), new Vector3(1.0f, 1.0f, 1.0f));

            // Render the ball
            var ballModel = Matrix4.CreateTranslation(_ballPosition);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref ballModel);
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "objectColor"), metallicWhite);
            GL.BindVertexArray(_ballVertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 16 * 16 * 6); // Adjust according to the sphere's vertex count

            // Render the first paddle
            var leftPaddleModel = Matrix4.CreateTranslation(_leftPaddlePosition);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref leftPaddleModel);
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "objectColor"), metallicWhite);
            GL.BindVertexArray(_leftPaddleVertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36); // Adjust according to the paddle's vertex count

            // Render the second paddle
            var rightPaddleModel = Matrix4.CreateTranslation(_rightPaddlePosition);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref rightPaddleModel);
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "objectColor"), metallicWhite);
            GL.BindVertexArray(_rightPaddleVertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36); // Adjust according to the paddle's vertex count

            // Render the game board
            var gameBoardModel = Matrix4.Identity; // Keep the game board static
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref gameBoardModel);
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "objectColor"), new Vector3(1.0f, 1.0f, 1.0f)); // Set line color to white
            GL.BindVertexArray(_gameBoardVertexArrayObject);
            GL.DrawArrays(PrimitiveType.Lines, 0, 24);

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
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), ClientSize.X / (float)ClientSize.Y, 0.1f, 100.0f);
        }

        protected override void OnUnload()
        {
            GL.DeleteVertexArray(_ballVertexArrayObject);
            GL.DeleteVertexArray(_leftPaddleVertexArrayObject);
            GL.DeleteVertexArray(_rightPaddleVertexArrayObject);
            GL.DeleteProgram(_shaderProgram);

            base.OnUnload();
        }
    }
}
