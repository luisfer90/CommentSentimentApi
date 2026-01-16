namespace CommentSentimentApi.Application.DTOs
{
    public class SentimentSummaryResponse
    {
        public int Total_Comments { get; set; }

        public Dictionary<string, int> Sentiment_Counts { get; set; }
            = new Dictionary<string, int>();
    }
}
