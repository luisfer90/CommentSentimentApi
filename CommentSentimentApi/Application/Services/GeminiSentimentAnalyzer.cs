using CommentSentimentApi.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

public class GeminiSentimentAnalyzer : ISentimentAnalyzer
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiSentimentAnalyzer(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        // API KEY must be provided via the GEMINI_API_KEY environment variable [set GEMINI_API_KEY=AIzaSyXXXX]
        _apiKey = Environment.GetEnvironmentVariable("Gemini_ApiKey")
            ?? throw new InvalidOperationException(
                "GEMINI_API_KEY no está definida como variable de entorno"
            );

        // Get Gemini model
        _model = configuration["Gemini:Model"]
            ?? throw new InvalidOperationException("Gemini:Model no configurado");
    }

    public string Analyze(string text)
    {
        var prompt = $"""
Eres un clasificador de sentimiento.
Responde únicamente con una sola palabra: positivo, negativo o neutral.
Texto: {text}
""";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-goog-api-key", _apiKey);
        request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
        );

        var response = _httpClient.SendAsync(request).Result;

        if (!response.IsSuccessStatusCode)
            return "neutral";

        var json = response.Content.ReadAsStringAsync().Result;
        using var doc = JsonDocument.Parse(json);

        var content = doc
            .RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString()!
            .Trim()
            .ToLower();

        return content switch
        {
            "positivo" => "positivo",
            "negativo" => "negativo",
            _ => "neutral"
        };
    }
}
