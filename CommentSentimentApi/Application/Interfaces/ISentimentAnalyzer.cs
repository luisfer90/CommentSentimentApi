namespace CommentSentimentApi.Application.Interfaces
{
    public interface ISentimentAnalyzer
    {
        string Analyze(string text);
    }
}
