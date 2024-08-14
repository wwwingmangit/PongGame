using PongLLM;
using System.Text.Json;

namespace PongGameServer.Services
{
    public class LLMCommentService
    {
        private readonly Serilog.ILogger _logger;
        private readonly PongLLMCommentator _commentator;

        public LLMCommentService(Serilog.ILogger logger)
        {
            _logger = logger;
            _commentator = new PongLLMCommentator(_logger);
            _commentator.Initialize(PongLLMCommentator.PersonalityType.Depressed);
        }

        public async Task<string> GenerateCommentAsync(object gameStats)
        {
            var statsJson = JsonSerializer.Serialize(gameStats);
            return await _commentator.GetOllamaResponse(statsJson);
        }
    }
}
