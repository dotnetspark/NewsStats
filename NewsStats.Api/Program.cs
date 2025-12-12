using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Distributed;
using NewsStats.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.AddRedisDistributedCache("cache");

builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // In development with Aspire, allow any localhost origin
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

builder.Services.AddHttpClient("NewsApi", (serviceProvider, client) =>
{
    client.BaseAddress = new Uri("https://newsapi.org/v2/");
    client.DefaultRequestHeaders.Add("User-Agent", "NewStatsApp");
});

builder.Services.AddScoped<NewsApiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseResponseCompression();
app.UseResponseCaching();

app.MapDefaultEndpoints();

app.MapGet("/articles/search", async (
    string? query,
    DateTime? from,
    DateTime? to,
    NewsApiService newsApiService) =>
{
    // Validate query if provided - only alphanumeric and spaces
    if (!string.IsNullOrWhiteSpace(query) && !Regex.IsMatch(query, @"^[a-zA-Z0-9\s]+$"))
    {
        return Results.BadRequest(new { error = "Query must contain only alphanumeric characters and spaces." });
    }

    var searchQuery = string.IsNullOrWhiteSpace(query) ? "*" : query;
    var result = await newsApiService.GetArticlesAsync(searchQuery, from, to);
    if (result != null)
    {
        return Results.Ok(result);
    }
    return Results.Problem("Failed to fetch articles.");
})
.WithName("SearchArticles")
.WithTags("Articles");

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/scalar/v1"))
        .ExcludeFromDescription();
}
else
{
    app.MapGet("/", () => new
    {
        message = "NewsStats API",
        endpoints = new[] {
            "/api/articles/search?query={query}&from={yyyy-MM-dd}&to={yyyy-MM-dd}"
        }
    });
}

app.Run();
