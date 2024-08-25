using Serilog;
using System.Text.Json;
using System.Text;
using Newtonsoft.Json;
using System.Net;

namespace PongLLM
{
    public enum PersonalityType
    {
        Serious,
        Hilarious,
        Depressed
    }

    public class ChatContext
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("message")]
        public LLMMessage Message { get; set; }

        [JsonProperty("done_reason")]
        public string DoneReason { get; set; }

        [JsonProperty("done")]
        public bool Done { get; set; }
    }

    public class LLMMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class LLMMessageHistory
    {
        private readonly List<LLMMessage> _messages = new List<LLMMessage>();
        private readonly int _maxSize;
        private readonly object _lock = new object();

        public LLMMessageHistory(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void AddMessage(LLMMessage message)
        {
            lock (_lock)
            {
                _messages.Add(message);
                TrimHistory();
            }
        }

        public LLMMessage[] GetMessages()
        {
            lock (_lock)
            {
                return _messages.ToArray();
            }
        }

        private void TrimHistory()
        {
            if (_messages.Count <= _maxSize) return;

            while (_messages.Count > _maxSize)
            {
                _messages.RemoveAt(2); // don't remove initial prompt and answer 
                _messages.RemoveAt(2);
            }
        }
    }

    public class PongLLMCommentator
    {

        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private LLMMessageHistory _llmHistory;
        private const int MAX_LLM_MESSAGES = 2 + 3 * 2;

        private const string OLLAMA_API_URL = "http://localhost:11434/api/chat";
        private string OLLAMA_MODEL = "llama3";

        private const string INIT_PROMPT =
            "Vous êtes un commentateur de jeu vidéo.\n" +
            "Soyez prêt à fournir une phrase de commentaire pour chaque série de données que je vais fournir.\n" + 
            "Les données comporteront le style à utiliser (un mot) puis des données en JSON, contenant le temps d'uptime du serveur (DD.HH:MM:SS) et les statistiques des parties de Pong auto-jouées.\n" +
            "Chaque partie a un identifiant unique, un score du joueur de gauche, un score du joueur de droite, et une durée de jeu.\n" +
            "Vos commentaires devront être en français, ne dépasser 40 mots et se concentrer exclusivement sur les parties (pas de réponse supplémentaire attendue).\n";

        public PersonalityType Personality
        {
            get
            {
                lock (_personalityLock)
                {
                    return _personality;
                }
            }

            set
            {
                lock (_personalityLock)
                {
                    _personality = value;
                }
            }
        }
        private readonly object _personalityLock = new object();
        private PersonalityType _personality;

        public PongLLMCommentator(ILogger logger)
        {
            _logger = logger.ForContext<PongLLMCommentator>();

            _httpClient = new HttpClient();
            _llmHistory = new LLMMessageHistory(MAX_LLM_MESSAGES);

            _logger.Information("PongLLMCommentator created with default settings.");
        }
        public async Task<string> Initialize(PersonalityType personality = PersonalityType.Serious)
        {
            Personality = personality;
            _logger.Information("Initializing PongLLMCommentator with PersonalityType: {PersonalityType}", Personality.ToString());

            // initial prompt and first answer
            string initialInput = INIT_PROMPT;
            string initialOutput = await GetOllamaResponse(initialInput);

            _logger.Debug("Initialization prompt sent: {Prompt}", initialInput);
            _logger.Debug("Initialization response received: {Response}", initialOutput);

            return initialOutput;
        }
        private async Task<ChatContext> SendRequest(string url, string userInput)
        {
            _llmHistory.AddMessage(new LLMMessage
            {
                Role = "user",
                Content = Personality.ToString() + "\n" + userInput
            });

            var requestData = new
            {
                model = "llama3",
                messages = _llmHistory.GetMessages(),
                stream = false, // assuming you don't want streaming
                //system = "Max comment length is 20 words. And Your personality is " + Personality.ToString() + "."
            };

            var json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseChatContext = JsonConvert.DeserializeObject<ChatContext>(responseBody);

            if (responseChatContext != null)
            {
                _llmHistory.AddMessage(responseChatContext.Message);
            }

            return responseChatContext;
        }

        public async Task<string> GetOllamaResponse(string userInput)
        {
            _logger.Information("Processing Ollama response for the request: {Request}", userInput);

            try
            {
                _logger.Debug("Sending HTTP POST request to Ollama API with model: {Model}", OLLAMA_MODEL);

                var context = await SendRequest(OLLAMA_API_URL, userInput);

                string stringResponse = context.Message.Content;

                _logger.Information("Received response from Ollama API: {Response}", stringResponse);
                return stringResponse;
            }
            catch (HttpRequestException e)
            {
                _logger.Error(e, "An error occurred while making a request to the Ollama API. Request: {userInput}", userInput);
                return "Sorry, I encountered an error while processing your request.";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An unexpected error occurred. Request: {userInput}", userInput);
                return "An unexpected error occurred.";
            }
        }
    }
}
