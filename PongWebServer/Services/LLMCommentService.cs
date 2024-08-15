using PongLLM;
using System.Text.Json;
using System.Threading.Tasks;

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
            _commentator.Initialize(PongLLMCommentator.PersonalityType.Depressed); // Default initialization
        }

        public async Task<string> GenerateCommentAsync(object gameStats)
        {
            var statsJson = JsonSerializer.Serialize(gameStats);
            return await _commentator.GetOllamaResponse(statsJson);
        }

        public void SetPersonality(PongLLMCommentator.PersonalityType newPersonality)
        {
            if (_commentator.Personality != newPersonality)
            {
                _logger.Information("Changing personality to {PersonalityType}", newPersonality.ToString());
                _commentator.Personality = newPersonality;
            }
        }
    }
}
