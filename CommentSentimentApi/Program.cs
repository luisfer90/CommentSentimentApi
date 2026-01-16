using CommentSentimentApi.Application.Interfaces;
using CommentSentimentApi.Application.Services;
using CommentSentimentApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Add services to the container
// --------------------------------------------------

// Register controllers for the Web API
builder.Services.AddControllers();

// Register Entity Framework Core with SQL Server
// The connection string is read from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Register Swagger/OpenAPI services for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register HttpClient
// Required for calling external AI APIs (e.g., Gemini)
builder.Services.AddHttpClient();

// --------------------------------------------------
// Sentiment Analyzer provider selection
// --------------------------------------------------
// The sentiment analysis provider is selected via configuration
// to avoid coupling controllers or business logic to a specific
// AI implementation.
//
// Possible values:
// - "Gemini"     -> Uses Google Gemini API
// - Any other    -> Falls back to rule-based analysis
//
var sentimentProvider = builder.Configuration["Sentiment:Provider"];

if (sentimentProvider == "Gemini")
{
    // Register Gemini-based sentiment analyzer
    builder.Services.AddScoped<ISentimentAnalyzer, GeminiSentimentAnalyzer>();
}
else
{
    // Register rule-based sentiment analyzer as a fallback
    builder.Services.AddScoped<ISentimentAnalyzer, RuleBasedSentimentAnalyzer>();
}

// --------------------------------------------------
// Build application
// --------------------------------------------------
var app = builder.Build();

// --------------------------------------------------
// Configure the HTTP request pipeline
// --------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CommentSentiment API v1");
});


// Enforce HTTPS redirection
app.UseHttpsRedirection();

// Enable authorization middleware
app.UseAuthorization();

// Map controller routes
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}


// Start the application
app.Run();
