using Serilog;
using System.Text.Json;
using System.Text;

namespace PongLLM
{
    public class PongLLMCommentator
    {
        private readonly ILogger _logger;
        private readonly HttpClient client = new HttpClient();
        private const string OLLAMA_API_URL = "http://localhost:11434/api/generate";
        private string OLLAMA_MODEL = "llama3";

        private const string INIT_PROMPT =
            "You are a data analyst specializing in sports and video games.\n" +
            "Here are my instructions that you will follow precisely.\n" +
            "I will provide you data, including server uptime and game stats. Each game entry has an ID, scores, and duration.\n" +
            "Your task is to provide a comment; this comment is a single phrase (max 20 words).\n" +
            "The style of the comment depends on your personality..\n" +
            "You comments MUST BE GIVEN IN FRENCH..\n";

        public enum PersonalityType
        {
            Serious,
            Hilarious,
            Depressed
        }

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

        private readonly List<(string userInput, string response)> _recentExchanges = new List<(string, string)>();

        public string Conversation
        {
            get
            {
                lock (_conversationLock)
                {
                    return BuildConversationContext();
                }
            }

            protected set
            {
                lock (_conversationLock)
                {
                    if (_recentExchanges.Count > 3)
                    {
                        _recentExchanges.RemoveAt(0); // Remove the oldest exchange if we already have 3
                    }
                    _recentExchanges.Add((ExtractUserInput(value), ExtractResponse(value)));
                }
            }
        }
        private readonly object _conversationLock = new object();
        private string _initialResponse;

        public PongLLMCommentator(ILogger logger)
        {
            _logger = logger.ForContext<PongLLMCommentator>();
            _logger.Information("PongLLMCommentator created with default settings.");
        }

        protected async Task<bool> LoadModel()
        {
            var requestBody = new
            {
                model = OLLAMA_MODEL,
                prompt = "" // Empty prompt to load the model into memory
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                _logger.Information("Loading model '{Model}' into memory.", OLLAMA_MODEL);
                HttpResponseMessage response = await client.PostAsync(OLLAMA_API_URL, content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                _logger.Information("Model '{Model}' loaded successfully. Response: {ResponseBody}", OLLAMA_MODEL, responseBody);

                return true;
            }
            catch (HttpRequestException e)
            {
                _logger.Error(e, "An error occurred while loading the model '{Model}'.", OLLAMA_MODEL);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An unexpected error occurred while loading the model '{Model}'.");
                return false;
            }
        }

        public async Task<string> Initialize(PersonalityType personality = PersonalityType.Serious, string prompt = INIT_PROMPT)
        {
            Personality = personality;
            _logger.Information("Initializing PongLLMCommentator with PersonalityType: {PersonalityType}", Personality.ToString());

            _logger.Debug("Initialization loading model {Model} into memory", OLLAMA_MODEL);
            bool isModelLoaded = await LoadModel();
            if (!isModelLoaded)
            {
                _logger.Error("Failed to load the model. Initialization aborted.");
                return "Initialization failed due to model loading issue.";
            }
            _logger.Debug("Initialization {Model} model loaded into memory", OLLAMA_MODEL);

            _initialResponse = await GetOllamaResponse(prompt);

            _logger.Debug("Initialization prompt sent: {Prompt}", prompt);
            _logger.Debug("Initialization response received: {Response}", _initialResponse);

            return _initialResponse;
        }

        public async Task<string> ResetConversation(PersonalityType personality, string prompt = INIT_PROMPT)
        {
            _logger.Information("Resetting conversation with a new prompt.");

            Personality = personality;

            _recentExchanges.Clear();  // Clear the recent exchanges
            _initialResponse = await GetOllamaResponse(prompt);

            _logger.Debug("Reset prompt sent: {Prompt}", prompt);
            _logger.Debug("Response received after reset: {Response}", _initialResponse);

            return _initialResponse;
        }

        public async Task<string> GetOllamaResponse(string userInput)
        {
            _logger.Information("Processing Ollama response for the request: {Request}", userInput);

            var requestBody = new
            {
                model = OLLAMA_MODEL,
                prompt = BuildConversationContext() + $"Human: {userInput}\n",
                stream = false,
                system = "You have a " + Personality.ToString() + " personality. Remember your task is to provide an answer. Your answer is a single phrase (max 20 words)."
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                _logger.Debug("Sending HTTP POST request to Ollama API with model: {Model}", OLLAMA_MODEL);
                HttpResponseMessage response = await client.PostAsync(OLLAMA_API_URL, content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var stringResponse = jsonResponse.GetProperty("response").GetString();
                // Remove all quotes from the string response
                stringResponse = stringResponse.Replace("\"", "");

                // Update the conversation history
                Conversation = $"Human: {userInput}\nAssistant: {stringResponse}";

                _logger.Information("Received response from Ollama API: {Response}", stringResponse);
                return stringResponse;
            }
            catch (HttpRequestException e)
            {
                _logger.Error(e, "An error occurred while making a request to the Ollama API. Request: {RequestBody}", requestBody);
                return "Sorry, I encountered an error while processing your request.";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An unexpected error occurred. Request: {RequestBody}", requestBody);
                return "An unexpected error occurred.";
            }
        }

        private string BuildConversationContext()
        {
            StringBuilder conversationBuilder = new StringBuilder();
            conversationBuilder.AppendLine($"Human: {INIT_PROMPT}");
            conversationBuilder.AppendLine($"Assistant: {_initialResponse}");

            foreach (var exchange in _recentExchanges)
            {
                conversationBuilder.AppendLine($"Human: {exchange.userInput}");
                conversationBuilder.AppendLine($"Assistant: {exchange.response}");
            }

            return conversationBuilder.ToString();
        }

        private string ExtractUserInput(string conversation)
        {
            // Assuming the user input is formatted as "Human: <input>"
            int start = conversation.LastIndexOf("Human: ") + 7;
            int end = conversation.IndexOf("\n", start);
            return conversation.Substring(start, end - start);
        }

        private string ExtractResponse(string conversation)
        {
            // Assuming the response is formatted as "Assistant: <response>"
            int start = conversation.LastIndexOf("Assistant: ") + 11;
            return conversation.Substring(start).Trim();
        }
    }
}
