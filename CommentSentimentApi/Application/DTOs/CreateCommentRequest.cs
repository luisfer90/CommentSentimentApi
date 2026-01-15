namespace CommentSentimentApi.Application.DTOs
{
    public class CreateCommentRequest
    {
        public string Product_Id { get; set; } = null!;
        public string User_Id { get; set; } = null!;
        public string Comment_Text { get; set; } = null!;
    }
}
