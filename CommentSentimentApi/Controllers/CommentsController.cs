using CommentSentimentApi.Application.DTOs;
using CommentSentimentApi.Application.Interfaces;
using CommentSentimentApi.Application.Services;
using CommentSentimentApi.Domain.Entities;
using CommentSentimentApi.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommentSentimentApi.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISentimentAnalyzer _sentimentAnalyzer;
        private readonly RuleBasedSentimentAnalyzer _fallbackAnalyzer;

        // Main sentiment analyzer is injected via DI (Gemini)
        // Rule-based analyzer is used as a fallback when Gemini fails
        public CommentsController(
            AppDbContext context,
            ISentimentAnalyzer sentimentAnalyzer)
        {
            _context = context;
            _sentimentAnalyzer = sentimentAnalyzer;
            _fallbackAnalyzer = new RuleBasedSentimentAnalyzer();
        }

        // Creates a new comment and analyzes its sentiment
        // POST: api/comments
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Comment_Text))
                return BadRequest("El comentario no puede estar vacío.");

            string sentiment;

            try
            {
                // Try sentiment analysis using Gemini API
                sentiment = _sentimentAnalyzer.Analyze(request.Comment_Text);
            }
            catch
            {
                // Fallback to rule-based sentiment analysis
                sentiment = _fallbackAnalyzer.Analyze(request.Comment_Text);
            }


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

        // Retrieves comments with optional filters
        // GET: api/comments
        [HttpGet]
        public async Task<IActionResult> GetComments(
            [FromQuery(Name = "product_id")] string? productId,
            [FromQuery(Name = "sentiment")] string? sentiment)
        {
            var query = _context.Comments.AsQueryable();

            // Optional filter by product_id
            if (!string.IsNullOrWhiteSpace(productId))
            {
                query = query.Where(c => c.ProductId == productId);
            }

            // Optional filter by sentiment
            if (!string.IsNullOrWhiteSpace(sentiment))
            {
                query = query.Where(c => c.Sentiment == sentiment.ToLower());
            }

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    UserId = c.UserId,
                    CommentText = c.CommentText,
                    Sentiment = c.Sentiment,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }

        // Returns a summary of comments grouped by sentiment
        // GET: api/sentiment-summary
        [HttpGet("/api/sentiment-summary")]
        public async Task<IActionResult> GetSentimentSummary()
        {
            var totalComments = await _context.Comments.CountAsync();

            var sentimentCounts = await _context.Comments
                .GroupBy(c => c.Sentiment)
                .Select(g => new
                {
                    Sentiment = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var response = new SentimentSummaryResponse
            {
                Total_Comments = totalComments
            };

            foreach (var item in sentimentCounts)
            {
                response.Sentiment_Counts[item.Sentiment] = item.Count;
            }

            // Ensure all sentiments exist even if count is 0
            response.Sentiment_Counts.TryAdd("positivo", 0);
            response.Sentiment_Counts.TryAdd("negativo", 0);
            response.Sentiment_Counts.TryAdd("neutral", 0);

            return Ok(response);
        }


    }
}
