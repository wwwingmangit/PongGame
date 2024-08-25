using Serilog;
using System.Text.Json;
using System.Text;

namespace PongLLM3
{
    public enum PersonalityType
    {
        Serious,
        Hilarious,
        Depressed
    }

    public class PongLLMCommentator
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        private const string OLLAMA_API_URL = "http://localhost:11434/api/chat";
        private string OLLAMA_MODEL = "llama3";
        private const int ROLLING_LLM_WINDOW_SIZE = 3; // initial prompt + 3 last answers

        private const string INIT_PROMPT =
            "Vous êtes un commentateur de jeu vidéo.\n" +
            "Soyez prêt à fournir une phrase de commentaire pour chaque données en JSON, contenant le temps d'uptime du serveur (DD.HH:MM:SS) et les statistiques des parties de Pong auto-jouées.\n" +
            "Chaque partie a un ID, des scores gauche et droite, et une durée de jeu.\n" +
            "Vos commentaires devront être en français, ne dépasser 20 mots et se concentrer exclusivement sur les parties (pas de réponse supplémentaire attendue).\n";

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

            _logger.Information("PongLLMCommentator created with default settings.");
        }
        public async Task<string> Initialize(PersonalityType personality = PersonalityType.Serious)
        {
            Personality = personality;
            _logger.Information("Initializing PongLLMCommentator with PersonalityType: {PersonalityType}", Personality.ToString());

            // Load model
            bool isModelLoaded = await LoadModelToOllama();
            if (!isModelLoaded)
            {
                _logger.Error("Failed to load the model. Initialization aborted.");
                return "Initialization failed due to model loading issue.";
            }
            _logger.Debug("Initialization {Model} model loaded into memory", OLLAMA_MODEL);

            // define initial prompt and first answer
            string initialInput = INIT_PROMPT;
            string initialOutput = await GetOllamaResponse(initialInput);

            _logger.Debug("Initialization prompt sent: {Prompt}", initialInput);
            _logger.Debug("Initialization response received: {Response}", initialOutput);

            return initialOutput;
        }
        private async Task<bool> LoadModelToOllama()
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
                HttpResponseMessage response = await _httpClient.PostAsync(OLLAMA_API_URL, content);
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
        public async Task<string> GetOllamaResponse(string userInput)
        {
            _logger.Information("Processing Ollama response for the request: {Request}", userInput);

            //string requestBodyPrompt = BuildRequestBodyPrompt(userInput);

            var requestBody = new
            {
                model = OLLAMA_MODEL,
                prompt = userInput,
                stream = false,
                system = "Your taks is to provide a single comment, max 20 words, in French. Your personality is " + Personality.ToString()
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                _logger.Debug("Sending HTTP POST request to Ollama API with model: {Model}", OLLAMA_MODEL);
                HttpResponseMessage response = await _httpClient.PostAsync(OLLAMA_API_URL, content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var stringResponse = jsonResponse.GetProperty("response").GetString();
                // Remove all quotes from the string response
                stringResponse = stringResponse.Replace("\"", "");

                // Update the conversation history
                //_llmConversation.AddInteractionOutput(stringResponse);

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

        /*private string BuildConversationContext()
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
        }*/
    }
}
