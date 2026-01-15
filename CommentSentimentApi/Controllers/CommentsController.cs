using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommentSentimentApi.Infrastructure.Data;
using CommentSentimentApi.Application.Interfaces;
using CommentSentimentApi.Domain.Entities;
using CommentSentimentApi.Application.DTOs;

namespace CommentSentimentApi.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISentimentAnalyzer _sentimentAnalyzer;

        public CommentsController(
            AppDbContext context,
            ISentimentAnalyzer sentimentAnalyzer)
        {
            _context = context;
            _sentimentAnalyzer = sentimentAnalyzer;
        }

        // POST: api/comments
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Comment_Text))
                return BadRequest("El comentario no puede estar vacío.");

            var sentiment = _sentimentAnalyzer.Analyze(request.Comment_Text);

            var comment = new Comment
            {
                ProductId = request.Product_Id,
                UserId = request.User_Id,
                CommentText = request.Comment_Text,
                Sentiment = sentiment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(comment);
        }
    }
}
