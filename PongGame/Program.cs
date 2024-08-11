using Serilog;

namespace PongClient
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            ILogger logger = new LoggerConfiguration()
                                                .MinimumLevel.Debug()
                                                .WriteTo.File("log.txt")
                                                .CreateLogger();
            Application.Run(new PongGameForm(logger));
        }
    }
}