using System.Net.Http.Json;
using NewsStats.Wasm.Models;

namespace NewsStats.Wasm.Services;

public class NewsApiClient
{
    private readonly HttpClient _httpClient;

    public NewsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<NewsApiResponse?> SearchArticlesAsync(string? query, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var url = $"api/articles/search?";
        if (!string.IsNullOrWhiteSpace(query))
            url += $"query={Uri.EscapeDataString(query)}&";
        if (fromDate.HasValue)
            url += $"from={fromDate.Value:yyyy-MM-dd}&";
        if (toDate.HasValue)
            url += $"to={toDate.Value:yyyy-MM-dd}&";

        url = url.TrimEnd('&', '?');

        try
        {
            Console.WriteLine($"Fetching from URL: {url}");
            Console.WriteLine($"Base address: {_httpClient.BaseAddress}");

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Response preview: {content.Substring(0, Math.Min(200, content.Length))}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode} - {content}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<NewsApiResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching articles: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }
}
