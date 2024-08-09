using Pong;
using Serilog;
using Serilog.Core;
using System;
using PongGameServer;


namespace PongConsoleServer
{
    class Program
    {
        static void Main()
        {
            ILogger _logger = new LoggerConfiguration()
                                    .MinimumLevel.Debug()
                                    .WriteTo.File("log.txt")
                                    .CreateLogger();
            _logger.Information($"Main>>Start");

            GameServer gameServer = new GameServer(_logger);

            gameServer.StartServer();
            MainServerKeyInputLoop(gameServer, _logger);
            gameServer.StopServer();

            _logger.Information($"Main<<End");
        }

        static void MainServerKeyInputLoop(GameServer gameServer, ILogger _logger)
        {
            while (true)
            {
                int gameUpdateDelayInMSec = 1;
                int winningScore = 11;

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;

                    switch (key)
                    {
                        case ConsoleKey.A:
                            // add a new game
                            _logger.Information($"Main>>User Requests a new game");
                            gameServer.AddNewGame(gameUpdateDelayInMSec, winningScore);
                            break;

                        case ConsoleKey.R:
                            // remove first game
                            _logger.Information($"Main>>User Requests to remove first game");
                            var games = gameServer.GetGames();
                            if (games.Count > 0)
                            {
                                gameServer.StopGame(games[0].GetHashCode());
                            }
                            break;

                        case ConsoleKey.S:
                            // stop all games, server continues to run
                            _logger.Information($"Main>>User Requests to stop all games");
                            gameServer.StopGames();
                            break;

                        case ConsoleKey.Q:
                            // get out
                            _logger.Information($"Main>>User Requests to stop server");
                            return;

                        default:
                            break;
                    }
                }
            }
        }
    }
}