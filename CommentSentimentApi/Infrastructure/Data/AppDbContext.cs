using CommentSentimentApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommentSentimentApi.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Comment> Comments => Set<Comment>();
    }
}
