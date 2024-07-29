using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using Pong;
using OpenTK.Windowing.Desktop;

namespace Pong3DOpenTK
{
    partial class Pong3DOpenTK : GameWindow
    {
        private readonly string objectVertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec3 aPosition;
            layout (location = 2) in vec3 aNormal;

            out vec3 FragPos;
            out vec3 Normal;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            void main()
            {
                FragPos = vec3(model * vec4(aPosition, 1.0));
                Normal = mat3(transpose(inverse(model))) * aNormal;
                gl_Position = projection * view * model * vec4(aPosition, 1.0);
            }";

        private readonly string objectFragmentShaderSource = @"
            #version 330 core
            out vec4 FragColor;

            in vec3 FragPos;
            in vec3 Normal;

            uniform vec3 lightPos;
            uniform vec3 viewPos;
            uniform vec3 lightColor;
            uniform vec3 objectColor;

            void main()
            {
                // Increase ambient strength for better base color
                float ambientStrength = 0.5; 
                vec3 ambient = ambientStrength * lightColor;

                // Diffuse lighting
                vec3 norm = normalize(Normal);
                vec3 lightDir = normalize(lightPos - FragPos);
                float diff = 0.5 * max(dot(norm, lightDir), 0.0); // Change this line to reduce the diffuse component

                vec3 diffuse = diff * lightColor;

                // Increase specular strength for more metallic effect
                float specularStrength = 0.5; 
                vec3 viewDir = normalize(viewPos - FragPos);
                vec3 reflectDir = reflect(-lightDir, norm);
                float spec = pow(max(dot(viewDir, reflectDir), 0.0), 128);
                vec3 specular = specularStrength * spec * lightColor;

                //vec3 result = (ambient + diffuse + specular) * objectColor;
                vec3 result = (ambient + diffuse) * objectColor + specular;  // Adjusted line

                FragColor = vec4(result, 1.0);
            }";

        private readonly string scoreVertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec3 aPosition;
            layout (location = 1) in vec2 aTexCoord;

            out vec2 TexCoord;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            void main()
            {
                TexCoord = aTexCoord;
                gl_Position = projection * view * model * vec4(aPosition, 1.0);
            }";

        private readonly string scoreFragmentShaderSource = @"
            #version 330 core
            out vec4 FragColor;

            in vec2 TexCoord;

            uniform sampler2D texture1;

            void main()
            {
                FragColor = texture(texture1, TexCoord);
            }";

        private int CreateObject(float[] vertices, int vertexSize, int normalOffset)
        {
            int vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            int vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            if (normalOffset >= 0)
            {
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, vertexSize * sizeof(float), normalOffset * sizeof(float));
                GL.EnableVertexAttribArray(1);
            }

            return vertexArrayObject;
        }

        private int CreateBall(Size3D size)
        {
            float[] ballVertices = GenerateSphereVertices((float)size.Width / 2, 16, 16);
            return CreateObject(ballVertices, 6, 3);
        }

        private int CreatePaddle(Size3D size)
        {
            float[] paddleVertices = GenerateBoxVertices((float)size.Width, (float)size.Height, (float)size.Depth);
            return CreateObject(paddleVertices, 6, 3);
        }

        private int CreateScreenGameBoard(Size3D size)
        {
            float halfWidth = (float)size.Width / 2.0f;
            float halfHeight = (float)size.Height / 2.0f;
            float halfDepth = (float)size.Depth / 2.0f;

            float[] gameBoardVertices = {
                -halfWidth, -halfHeight, -halfDepth,  halfWidth, -halfHeight, -halfDepth,
                 halfWidth, -halfHeight, -halfDepth,  halfWidth,  halfHeight, -halfDepth,
                 halfWidth,  halfHeight, -halfDepth, -halfWidth,  halfHeight, -halfDepth,
                -halfWidth,  halfHeight, -halfDepth, -halfWidth, -halfHeight, -halfDepth,
                -halfWidth, -halfHeight,  halfDepth,  halfWidth, -halfHeight,  halfDepth,
                 halfWidth, -halfHeight,  halfDepth,  halfWidth,  halfHeight,  halfDepth,
                 halfWidth,  halfHeight,  halfDepth, -halfWidth,  halfHeight,  halfDepth,
                -halfWidth,  halfHeight,  halfDepth, -halfWidth, -halfHeight,  halfDepth,
                -halfWidth, -halfHeight, -halfDepth, -halfWidth, -halfHeight,  halfDepth,
                 halfWidth, -halfHeight, -halfDepth,  halfWidth, -halfHeight,  halfDepth,
                 halfWidth,  halfHeight, -halfDepth,  halfWidth,  halfHeight,  halfDepth,
                -halfWidth,  halfHeight, -halfDepth, -halfWidth,  halfHeight,  halfDepth
            };

            return CreateObject(gameBoardVertices, 3, -1);
        }

        private int CreateBorder(float width, float height, float depth)
        {
            float[] borderVertices = GenerateBoxVertices(width, height, depth);
            return CreateObject(borderVertices, 6, 3);
        }

        private float[] GenerateSphereVertices(float radius, int sectorCount, int stackCount)
        {
            float x, y, z, xy;                              // vertex position
            float nx, ny, nz, lengthInv = 1.0f / radius;    // normal

            float sectorStep = 2 * MathF.PI / sectorCount;
            float stackStep = MathF.PI / stackCount;
            float sectorAngle, stackAngle;

            int count = 0;
            float[] vertices = new float[(sectorCount + 1) * (stackCount + 1) * 6];

            for (int i = 0; i <= stackCount; ++i)
            {
                stackAngle = MathF.PI / 2 - i * stackStep;        // starting from pi/2 to -pi/2
                xy = radius * MathF.Cos(stackAngle);             // r * cos(u)
                z = radius * MathF.Sin(stackAngle);              // r * sin(u)

                // add (sectorCount+1) vertices per stack
                // the first and last vertices have same position and normal, but different tex coords
                for (int j = 0; j <= sectorCount; ++j, ++count)
                {
                    sectorAngle = j * sectorStep;           // starting from 0 to 2pi

                    // vertex position (x, y, z)
                    x = xy * MathF.Cos(sectorAngle);             // r * cos(u) * cos(v)
                    y = xy * MathF.Sin(sectorAngle);             // r * cos(u) * sin(v)
                    vertices[count * 6] = x;
                    vertices[count * 6 + 1] = y;
                    vertices[count * 6 + 2] = z;

                    // normalized vertex normal (nx, ny, nz)
                    nx = x * lengthInv;
                    ny = y * lengthInv;
                    nz = z * lengthInv;
                    vertices[count * 6 + 3] = nx;
                    vertices[count * 6 + 4] = ny;
                    vertices[count * 6 + 5] = nz;
                }
            }

            return vertices;
        }

        private float[] GenerateBoxVertices(float width, float height, float depth)
        {
            float halfWidth = width / 2.0f;
            float halfHeight = height / 2.0f;
            float halfDepth = depth / 2.0f;

            float[] vertices = {
                // Front face
                -halfWidth, -halfHeight, halfDepth,  0.0f,  0.0f,  1.0f,
                 halfWidth, -halfHeight, halfDepth,  0.0f,  0.0f,  1.0f,
                 halfWidth,  halfHeight, halfDepth,  0.0f,  0.0f,  1.0f,
                 halfWidth,  halfHeight, halfDepth,  0.0f,  0.0f,  1.0f,
                -halfWidth,  halfHeight, halfDepth,  0.0f,  0.0f,  1.0f,
                -halfWidth, -halfHeight, halfDepth,  0.0f,  0.0f,  1.0f,

                // Back face
                -halfWidth, -halfHeight, -halfDepth,  0.0f,  0.0f, -1.0f,
                -halfWidth,  halfHeight, -halfDepth,  0.0f,  0.0f, -1.0f,
                 halfWidth,  halfHeight, -halfDepth,  0.0f,  0.0f, -1.0f,
                 halfWidth,  halfHeight, -halfDepth,  0.0f,  0.0f, -1.0f,
                 halfWidth, -halfHeight, -halfDepth,  0.0f,  0.0f, -1.0f,
                -halfWidth, -halfHeight, -halfDepth,  0.0f,  0.0f, -1.0f,

                // Top face
                -halfWidth,  halfHeight, -halfDepth,  0.0f,  1.0f,  0.0f,
                -halfWidth,  halfHeight,  halfDepth,  0.0f,  1.0f,  0.0f,
                 halfWidth,  halfHeight,  halfDepth,  0.0f,  1.0f,  0.0f,
                 halfWidth,  halfHeight,  halfDepth,  0.0f,  1.0f,  0.0f,
                 halfWidth,  halfHeight, -halfDepth,  0.0f,  1.0f,  0.0f,
                -halfWidth,  halfHeight, -halfDepth,  0.0f,  1.0f,  0.0f,

                // Bottom face
                -halfWidth, -halfHeight, -halfDepth,  0.0f, -1.0f,  0.0f,
                 halfWidth, -halfHeight, -halfDepth,  0.0f, -1.0f,  0.0f,
                 halfWidth, -halfHeight,  halfDepth,  0.0f, -1.0f,  0.0f,
                 halfWidth, -halfHeight,  halfDepth,  0.0f, -1.0f,  0.0f,
                -halfWidth, -halfHeight,  halfDepth,  0.0f, -1.0f,  0.0f,
                -halfWidth, -halfHeight, -halfDepth,  0.0f, -1.0f,  0.0f,

                // Right face
                 halfWidth, -halfHeight, -halfDepth,  1.0f,  0.0f,  0.0f,
                 halfWidth,  halfHeight, -halfDepth,  1.0f,  0.0f,  0.0f,
                 halfWidth,  halfHeight,  halfDepth,  1.0f,  0.0f,  0.0f,
                 halfWidth,  halfHeight,  halfDepth,  1.0f,  0.0f,  0.0f,
                 halfWidth, -halfHeight,  halfDepth,  1.0f,  0.0f,  0.0f,
                 halfWidth, -halfHeight, -halfDepth,  1.0f,  0.0f,  0.0f,

                // Left face
                -halfWidth, -halfHeight, -halfDepth, -1.0f,  0.0f,  0.0f,
                -halfWidth, -halfHeight,  halfDepth, -1.0f,  0.0f,  0.0f,
                -halfWidth,  halfHeight,  halfDepth, -1.0f,  0.0f,  0.0f,
                -halfWidth,  halfHeight,  halfDepth, -1.0f,  0.0f,  0.0f,
                -halfWidth,  halfHeight, -halfDepth, -1.0f,  0.0f,  0.0f,
                -halfWidth, -halfHeight, -halfDepth, -1.0f,  0.0f,  0.0f
            };

            return vertices;
        }

        private int LoadTexture(string path)
        {
            int textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            using (var image = new Bitmap(path))
            {
                var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                image.UnlockBits(data);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return textureId;
        }

        private int CreateScoreSurface()
        {
            float[] vertices = {
                // Positions        // Texture Coords
                -0.1f, -0.1f, 0.0f, 0.0f, 1.0f,
                 0.1f, -0.1f, 0.0f, 1.0f, 1.0f,
                 0.1f,  0.1f, 0.0f, 1.0f, 0.0f,
                 0.1f,  0.1f, 0.0f, 1.0f, 0.0f,
                -0.1f,  0.1f, 0.0f, 0.0f, 0.0f,
                -0.1f, -0.1f, 0.0f, 0.0f, 1.0f
            };

            int vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            int vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            return vertexArrayObject;
        }

        private void UpdateScoreTextureCoordinates(int score, out float[] vertices)
        {
            // Assuming the texture is a single row of digits from 0 to 9
            int digit = score % 10;
            float digitWidth = 1.0f / 10.0f;

            float leftX = digit * digitWidth;
            float rightX = (digit + 1) * digitWidth;

            vertices = new float[]
            {
                // Positions        // Texture Coords
                -0.1f, -0.1f, 0.0f, leftX, 1.0f,
                 0.1f, -0.1f, 0.0f, rightX, 1.0f,
                 0.1f,  0.1f, 0.0f, rightX, 0.0f,
                 0.1f,  0.1f, 0.0f, rightX, 0.0f,
                -0.1f,  0.1f, 0.0f, leftX, 0.0f,
                -0.1f, -0.1f, 0.0f, leftX, 1.0f
            };
        }
        private int CreateShaderProgram(string vertexShaderSource, string fragmentShaderSource)
        {
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            CheckShaderCompileErrors(vertexShader, "VERTEX");

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompileErrors(fragmentShader, "FRAGMENT");

            int shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            CheckProgramLinkErrors(shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }

        private void CheckShaderCompileErrors(int shader, string type)
        {
            int success;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"ERROR::SHADER_COMPILATION_ERROR of type: {type}\n{infoLog}");
            }
        }

        private void CheckProgramLinkErrors(int program)
        {
            int success;
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine($"ERROR::PROGRAM_LINKING_ERROR\n{infoLog}");
            }
        }

    }
}
