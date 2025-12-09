using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Distributed;
using NewsStats.Api.Hubs;
using NewsStats.Api.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add OpenAPI
builder.Services.AddOpenApi();

// Add SignalR
builder.Services.AddSignalR();

// Add distributed caching with Redis
builder.AddRedisDistributedCache("cache");

// Add response caching and compression
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Add CORS
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

// Add services
builder.Services.AddHttpClient("NewsApi", (serviceProvider, client) =>
{
    client.BaseAddress = new Uri("https://newsapi.org/v2/");
    client.DefaultRequestHeaders.Add("User-Agent", "NewStatsApp");
});

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

app.MapHub<MetricsHub>("/metricshub");

app.MapGet("/articles/search", async (
    string? query,
    DateTime? from,
    DateTime? to,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IDistributedCache cache) =>
{
    var client = httpClientFactory.CreateClient("NewsApi");
    // Validate query if provided - only alphanumeric and spaces
    if (!string.IsNullOrWhiteSpace(query) && !Regex.IsMatch(query, @"^[a-zA-Z0-9\s]+$"))
    {
        return Results.BadRequest(new { error = "Query must contain only alphanumeric characters and spaces." });
    }

    // Generate cache key by hashing all parameters
    var cacheKeyInput = $"{query ?? ""}|{from:yyyy-MM-dd}|{to:yyyy-MM-dd}";
    var cacheKey = $"articles:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(cacheKeyInput)))}";

    // Try to get from cache
    var cachedData = await cache.GetStringAsync(cacheKey);
    if (!string.IsNullOrEmpty(cachedData))
    {
        var cachedResponse = JsonSerializer.Deserialize<NewsApiResponse>(cachedData);
        return Results.Ok(cachedResponse);
    }

    // Build NewsAPI URL
    var apiKey = configuration["NewsApiKey"]
        ?? throw new InvalidOperationException("NewsApiKey not configured.");

    var searchQuery = string.IsNullOrWhiteSpace(query) ? "*" : query;
    var url = $"everything?q={Uri.EscapeDataString(searchQuery)}&apiKey={apiKey}";

    if (from.HasValue) url += $"&from={from.Value:yyyy-MM-dd}";
    if (to.HasValue) url += $"&to={to.Value:yyyy-MM-dd}";

    try
    {
        var newsApiResponse = await client.GetFromJsonAsync<NewsApiResponse>(url);

        if (newsApiResponse != null)
        {
            // Cache the response for 5 minutes
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(newsApiResponse), cacheOptions);

            return Results.Ok(newsApiResponse);
        }

        return Results.Problem("Failed to fetch articles.");
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem($"Error fetching articles: {ex.Message}");
    }
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
