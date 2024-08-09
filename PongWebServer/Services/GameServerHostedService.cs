using Microsoft.Extensions.Hosting;
using PongGameServer;
using System.Threading;
using System.Threading.Tasks;

namespace YourNamespace.Services
{
    public class GameServerHostedService : IHostedService
    {
        private readonly GameServer _gameServer;

        public GameServerHostedService(GameServer gameServer)
        {
            _gameServer = gameServer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _gameServer.StartServer();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _gameServer.StopServer();
            return Task.CompletedTask;
        }
    }
}