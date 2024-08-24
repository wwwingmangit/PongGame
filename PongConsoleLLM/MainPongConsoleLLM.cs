using Serilog;
using PongLLM;

class MainPongConsoleLLM
{
    static async Task Main(string[] args)
    {
        ILogger _logger = new LoggerConfiguration()
                                .MinimumLevel.Debug()
                                .WriteTo.File("log.txt")
                                .CreateLogger();

        _logger.Information($"Main>>Start");

        PongLLMCommentator llm = new PongLLMCommentator(_logger);

        string llmInitResponse = await llm.Initialize(PongLLMCommentator.PersonalityType.Depressed);

        Console.Write($"Initialize: ${llmInitResponse}");

        while (true)
        {
            Console.Write("\nYou: ");
            string? userInput = Console.ReadLine();

            if (userInput == null || userInput.ToLower() == "exit")
            {
                break;
            }

            string llmResponse = await llm.GetOllamaResponse(userInput);
            Console.WriteLine($"\nAI: {llmResponse}");
        }

        _logger.Information($"Main>>Exit");
    }

}

/*class Program
{
    private static readonly HttpClient client = new HttpClient();
    private const string OLLAMA_API_URL = "http://localhost:11434/api/generate";

    static async Task Main(string[] args)
    {
        Console.WriteLine("Welcome to the OLLAMA chat program!");
        Console.WriteLine("Type 'exit' to end the conversation.");

        string conversation = "";

        while (true)
        {
            Console.Write("\nYou: ");
            string userInput = Console.ReadLine();

            if (userInput.ToLower() == "exit")
                break;

            conversation += $"Human: {userInput}\n";

            string response = await GetOllamaResponse(conversation);
            Console.WriteLine($"\nAI: {response}");

            conversation += $"Assistant: {response}\n";
        }

        Console.WriteLine("Thank you for using the OLLAMA chat program. Goodbye!");
    }

    static async Task<string> GetOllamaResponse(string conversation)
    {
        var requestBody = new
        {
            model = "llama3",
            prompt = conversation,
            stream = false
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await client.PostAsync(OLLAMA_API_URL, content);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
            return jsonResponse.GetProperty("response").GetString();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error: {e.Message}");
            return "Sorry, I encountered an error while processing your request.";
        }
    }
}*/