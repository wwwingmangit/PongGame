using PongLLM;
using System.Text.Json;
using System.Threading.Tasks;

namespace PongGameServer.Services
{
    public class LLMCommentService
    {
        private readonly Serilog.ILogger _logger;
        private readonly PongLLMCommentator _commentator;

        // private constructor.
        private LLMCommentService(Serilog.ILogger logger, PongLLMCommentator commentator)
        {
            _logger = logger;
            _commentator = commentator;
        }

        // Factory method to create an async constructor that allows us to call the Initialize fonction
        public static async Task<LLMCommentService> AsyncLLMCommentServiceConstructor(Serilog.ILogger logger)
        {
            PongLLMCommentator commentator = new PongLLMCommentator(logger);
            await commentator.Initialize();
            return new LLMCommentService(logger, commentator);
        }

        public async Task<string> GenerateCommentAsync(object gameStats)
        {
            var statsJson = JsonSerializer.Serialize(gameStats);
            return await _commentator.GetOllamaResponse(statsJson);
        }

        public void SetPersonality(PongLLM.PersonalityType newPersonality)
        {
            if (_commentator.Personality != newPersonality)
            {
                _logger.Information("Changing personality to {PersonalityType}", newPersonality.ToString());
                _commentator.Personality = newPersonality;
            }
        }
    }
}
