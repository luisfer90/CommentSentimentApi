using CommentSentimentApi.Application.Services;
using Xunit;

namespace CommentSentimentApi.Tests.Services
{
    public class RuleBasedSentimentAnalyzerTests
    {
        [Fact]
        public void Analyze_ShouldReturnPositive_WhenTextContainsPositiveWords()
        {
            var analyzer = new RuleBasedSentimentAnalyzer();
            var text = "Este producto es excelente y muy bueno";

            var result = analyzer.Analyze(text);

            Assert.Equal("positivo", result);
        }

        [Fact]
        public void Analyze_ShouldReturnNegative_WhenTextContainsNegativeWords()
        {
            var analyzer = new RuleBasedSentimentAnalyzer();
            var text = "Este producto tiene un problema terrible";

            var result = analyzer.Analyze(text);

            Assert.Equal("negativo", result);
        }

        [Fact]
        public void Analyze_ShouldReturnNeutral_WhenNoKeywordsAreFound()
        {
            var analyzer = new RuleBasedSentimentAnalyzer();
            var text = "El producto fue entregado ayer";

            var result = analyzer.Analyze(text);

            Assert.Equal("neutral", result);
        }
    }
}
