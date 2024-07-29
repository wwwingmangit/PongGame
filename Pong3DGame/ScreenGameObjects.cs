using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System;
using Pong;

namespace Pong3DOpenTK
{
    partial class Pong3DOpenTK : GameWindow
    {
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

        private void CreateShaders()
        {
            string vertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                layout (location = 1) in vec3 aNormal;
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

            string fragmentShaderSource = @"
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
                    // Ambient
                    float ambientStrength = 0.1;
                    vec3 ambient = ambientStrength * lightColor;
                
                    // Diffuse 
                    vec3 norm = normalize(Normal);
                    vec3 lightDir = normalize(lightPos - FragPos);
                    float diff = max(dot(norm, lightDir), 0.0);
                    vec3 diffuse = diff * lightColor;
                
                    // Specular
                    float specularStrength = 0.5;
                    vec3 viewDir = normalize(viewPos - FragPos);
                    vec3 reflectDir = reflect(-lightDir, norm);  
                    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
                    vec3 specular = specularStrength * spec * lightColor;  
                
                    vec3 result = (ambient + diffuse + specular) * objectColor;
                    FragColor = vec4(result, 1.0);
                }";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
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

                for (int j = 0; j <= sectorCount; ++j)
                {
                    sectorAngle = j * sectorStep;           // starting from 0 to 2pi

                    // vertex position (x, y, z)
                    x = xy * MathF.Cos(sectorAngle);        // r * cos(u) * cos(v)
                    y = xy * MathF.Sin(sectorAngle);        // r * cos(u) * sin(v)
                    vertices[count++] = x;
                    vertices[count++] = y;
                    vertices[count++] = z;

                    // normalized vertex normal (nx, ny, nz)
                    nx = x * lengthInv;
                    ny = y * lengthInv;
                    nz = z * lengthInv;
                    vertices[count++] = nx;
                    vertices[count++] = ny;
                    vertices[count++] = nz;
                }
            }

            return vertices;
        }

        private float[] GenerateBoxVertices(float width, float height, float depth)
        {
            float halfWidth = width / 2.0f;
            float halfHeight = height / 2.0f;
            float halfDepth = depth / 2.0f;

            return new float[]
            {
                // Front face
                -halfWidth, -halfHeight,  halfDepth,  0.0f,  0.0f,  1.0f,
                 halfWidth, -halfHeight,  halfDepth,  0.0f,  0.0f,  1.0f,
                 halfWidth,  halfHeight,  halfDepth,  0.0f,  0.0f,  1.0f,
                 halfWidth,  halfHeight,  halfDepth,  0.0f,  0.0f,  1.0f,
                -halfWidth,  halfHeight,  halfDepth,  0.0f,  0.0f,  1.0f,
                -halfWidth, -halfHeight,  halfDepth,  0.0f,  0.0f,  1.0f,

                // Back face
                -halfWidth, -halfHeight, -halfDepth,  0.0f,  0.0f, -1.0f,
                -halfWidth,  halfHeight, -halfDepth,  0.0f,  0.0f, -1.0f,
                 halfWidth,  halfHeight, -halfDepth,  0.0f,  0.0f, -1.0f,
                 halfWidth,  halfHeight, -halfDepth,  0.0f,  0.0f, -1.0f,
                 halfWidth, -halfHeight, -halfDepth,  0.0f,  0.0f, -1.0f,
                -halfWidth, -halfHeight, -halfDepth,  0.0f,  0.0f, -1.0f,

                // Left face
                -halfWidth,  halfHeight,  halfDepth, -1.0f,  0.0f,  0.0f,
                -halfWidth,  halfHeight, -halfDepth, -1.0f,  0.0f,  0.0f,
                -halfWidth, -halfHeight, -halfDepth, -1.0f,  0.0f,  0.0f,
                -halfWidth, -halfHeight, -halfDepth, -1.0f,  0.0f,  0.0f,
                -halfWidth, -halfHeight,  halfDepth, -1.0f,  0.0f,  0.0f,
                -halfWidth,  halfHeight,  halfDepth, -1.0f,  0.0f,  0.0f,

                // Right face
                 halfWidth,  halfHeight,  halfDepth,  1.0f,  0.0f,  0.0f,
                 halfWidth, -halfHeight,  halfDepth,  1.0f,  0.0f,  0.0f,
                 halfWidth, -halfHeight, -halfDepth,  1.0f,  0.0f,  0.0f,
                 halfWidth, -halfHeight, -halfDepth,  1.0f,  0.0f,  0.0f,
                 halfWidth,  halfHeight, -halfDepth,  1.0f,  0.0f,  0.0f,
                 halfWidth,  halfHeight,  halfDepth,  1.0f,  0.0f,  0.0f,

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
                -halfWidth, -halfHeight, -halfDepth,  0.0f, -1.0f,  0.0f
            };
        }

    }
}
