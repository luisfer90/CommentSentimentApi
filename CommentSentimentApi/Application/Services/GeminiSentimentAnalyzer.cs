using CommentSentimentApi.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

public class GeminiSentimentAnalyzer : ISentimentAnalyzer
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GeminiSentimentAnalyzer(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public string Analyze(string text)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var model = _configuration["Gemini:Model"];

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
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-goog-api-key", apiKey);
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
