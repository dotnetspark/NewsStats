namespace NewsStats.Wasm.Models;

public class Article
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? UrlToImage { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Author { get; set; }
    public Source Source { get; set; } = new();
    public string? Content { get; set; }
    public int ClickCount { get; set; }
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
