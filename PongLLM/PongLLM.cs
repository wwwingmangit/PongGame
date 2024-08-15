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
            "Your task is to provide an answer. Your answer is a single phrase (max 20 words).\n";

            //"I will provide you data you in JSON format, including server uptime and game stats. Each game entry has an ID, scores, and duration.\n" +

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

        public string Conversation
        {
            get
            {
                lock (_conversationLock)
                {
                    return _conversation;
                }
            }

            protected set
            {
                lock (_conversationLock)
                {
                    _conversation = value;
                }
            }
        }
        private readonly object _conversationLock = new object();
        private string _conversation;

        public PongLLMCommentator(ILogger logger)
        {
            _logger = logger.ForContext<PongLLMCommentator>();

            Conversation = "";

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

        public async Task<string> Initialize(PersonalityType personality, string prompt = INIT_PROMPT)
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

            string response = await GetOllamaResponse(prompt);

            _logger.Debug("Initialization prompt sent: {Prompt}", prompt);
            _logger.Debug("Initialization response received: {Response}", response);

            Conversation = prompt + "\n" + response + "\n";

            _logger.Information("PongLLMCommentator initialized successfully with PersonalityType: {PersonalityType}", Personality.ToString());
            return response;
        }

        public async Task<string> ResetConversation(PersonalityType personality, string prompt = INIT_PROMPT)
        {
            _logger.Information("Resetting conversation with a new prompt.");

            Personality = personality;

            Conversation = "";  // Clear the conversation history
            string response = await GetOllamaResponse(prompt);

            _logger.Debug("Reset prompt sent: {Prompt}", prompt);
            _logger.Debug("Response received after reset: {Response}", response);

            Conversation = prompt + "\n" + response + "\n";

            _logger.Information("Conversation reset successfully. Personality is now {Personality}", Personality.ToString());
            return response;
        }

        public async Task<string> GetOllamaResponse(string request)
        {
            _logger.Information("Processing Ollama response for the request: {Request}", request);
            Conversation += request + "\n";

            var requestBody = new
            {
                model = OLLAMA_MODEL,
                prompt = Conversation,
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

                Conversation += stringResponse + "\n";

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
    }
}
