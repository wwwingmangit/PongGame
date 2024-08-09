using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using Serilog;

namespace Pong3DOpenTK
{
    class Program
    {
        static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Pong 3D"
            };

            ILogger logger = new LoggerConfiguration()
                                                .MinimumLevel.Debug()
                                                .WriteTo.File("log.txt")
                                                .CreateLogger();

            using (var game = new Pong3DOpenTK(GameWindowSettings.Default, nativeWindowSettings, logger))
            {
                game.Run();
            }
        }
    }

}
