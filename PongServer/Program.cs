using Pong;
using Serilog;
using Serilog.Core;
using System;

namespace PongServer
{
    class Program
    {
        static void Main()
        {
            ILogger _logger = new LoggerConfiguration()
                                    .MinimumLevel.Information()
                                    .WriteTo.File("log.txt")
                                    .CreateLogger();
            _logger.Information($"Main>>Start");

            GameServer gameServer = new GameServer(_logger);

            gameServer.StartServer();
            MainServerLoop(gameServer);
            gameServer.StopServer();

            _logger.Information($"Main<<End");
        }

        static void MainServerLoop(GameServer gameServer)
        {
            while (true)
            {
                int gameUpdateDelayInMSec = 1;
                int winningScore = 3;

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;

                    switch (key)
                    {
                        case ConsoleKey.A:
                            // add a new game
                            gameServer.AddNewGame(gameUpdateDelayInMSec, winningScore);
                            break;

                        case ConsoleKey.R:
                            // remove first game
                            var games = gameServer.GetGames();
                            if (games.Count > 0)
                            {
                                gameServer.StopGame(games[0].GetHashCode());
                            }
                            break;

                        case ConsoleKey.S:
                            // stop all games, server continues to run
                            gameServer.StopGames();
                            break;

                        case ConsoleKey.Q:
                            // stop loop
                            return;

                        default:
                            break;
                    }
                }
            }
        }
    }
}