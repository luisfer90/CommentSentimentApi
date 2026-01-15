using CommentSentimentApi.Application.Interfaces;

namespace CommentSentimentApi.Application.Services
{
    public class RuleBasedSentimentAnalyzer : ISentimentAnalyzer
    {
        public string Analyze(string text)
        {
            var lower = text.ToLower();

            if (lower.Contains("excelente") || lower.Contains("genial") ||
                lower.Contains("fantástico") || lower.Contains("bueno") ||
                lower.Contains("increíble"))
                return "positivo";

            if (lower.Contains("malo") || lower.Contains("terrible") ||
                lower.Contains("problema") || lower.Contains("defecto") ||
                lower.Contains("horrible"))
                return "negativo";

            return "neutral";
        }
    }
}
