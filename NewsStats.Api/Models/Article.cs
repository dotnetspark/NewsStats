namespace NewsStats.Api.Models;

public class Article
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string UrlToImage { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string Author { get; set; } = string.Empty;
    public Source Source { get; set; } = new();
    public string Content { get; set; } = string.Empty;
}

public class NewsApiResponse
{
    public string Status { get; set; } = string.Empty;
    public int TotalResults { get; set; }
    public List<Article> Articles { get; set; } = [];
}

public class Source
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}
