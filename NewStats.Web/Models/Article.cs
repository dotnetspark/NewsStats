namespace NewStats.Web.Models;

public class Article
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string UrlToImage { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class NewsApiResponse
{
    public string Status { get; set; } = string.Empty;
    public int TotalResults { get; set; }
    public List<NewsApiArticle> Articles { get; set; } = [];
}

public class NewsApiArticle
{
    public NewsApiSource? Source { get; set; }
    public string? Author { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? UrlToImage { get; set; }
    public DateTime PublishedAt { get; set; }
    public string? Content { get; set; }
}

public class NewsApiSource
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}

public class SearchResult
{
    public string Query { get; set; } = string.Empty;
    public List<Article> Articles { get; set; } = [];
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
