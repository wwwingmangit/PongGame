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
        private const int ROLLING_LLM_WINDOW_SIZE = 3; // initial prompt + 3 last answers

        private const string INIT_PROMPT =
            "You are a radio commentator and data analyst expert in video games.\n" +
            "Your task is to make a one phrase comment based on your personality and on the data that I will provide you.\n" +
            "The data is in json format, it contains the a server up time (in DD.HH:MM:SS format), and a list of stats of auto playing pong games.\n" +
            "Each game stat contains the game ID, the game left and right scores, and the game duration.\n" +
            "The comment you will provide will not exceed 20 words. And will be given in French\n";

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
                    if (_recentExchanges.Count > ROLLING_LLM_WINDOW_SIZE)
                    {
                        _recentExchanges.RemoveAt(0); // Remove the oldest exchange 
                    }
                    _recentExchanges.Add((ExtractUserInput(value), ExtractResponse(value)));
                }
            }
        }
        private readonly object _conversationLock = new object();
        private string? _initialResponse;

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

        public async Task<string> ResetConversation(PersonalityType personality)
        {
            _logger.Information("Resetting conversation with a new prompt.");

            Personality = personality;

            string prompt = INIT_PROMPT;

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
                system = "You will follow the orders given (1 phrase comment, max 20 words, in French) and have a " + Personality.ToString() + " personality."
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                _logger.Debug("Sending HTTP POST request to Ollama API with model: {Model}", OLLAMA_MODEL);
                HttpResponseMessage response = await client.PostAsync(OLLAMA_API_URL, content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var stringResponse = jsonResponse.GetProperty("response").GetString().Replace("\"", "");
                // Remove all quotes from the string response
                //stringResponse = stringResponse.Replace("\"", "");

                // Update the conversation history
                Conversation = $"Human: {userInput}\nLLM: {stringResponse}";

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
            conversationBuilder.AppendLine($"LLM: {_initialResponse}");

            foreach (var exchange in _recentExchanges)
            {
                conversationBuilder.AppendLine($"Human: {exchange.userInput}");
                conversationBuilder.AppendLine($"LLM: {exchange.response}");
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
            int start = conversation.LastIndexOf("LLM: ") + 11;
            return conversation.Substring(start).Trim();
        }
    }
}
