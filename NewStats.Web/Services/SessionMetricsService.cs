namespace NewStats.Web.Services;

public class SessionMetricsService
{
    private readonly Dictionary<string, int> _clickCounts = new();
    private readonly ILogger<SessionMetricsService> _logger;

    public SessionMetricsService(ILogger<SessionMetricsService> logger)
    {
        _logger = logger;
    }

    public void RecordClick(string articleId)
    {
        if (string.IsNullOrEmpty(articleId))
        {
            return;
        }

        if (_clickCounts.TryGetValue(articleId, out var count))
        {
            _clickCounts[articleId] = count + 1;
        }
        else
        {
            _clickCounts[articleId] = 1;
        }

        _logger.LogInformation("Recorded click for article {ArticleId}. Total clicks: {Count}", articleId, _clickCounts[articleId]);
    }

    public int GetClickCount(string articleId)
    {
        return _clickCounts.GetValueOrDefault(articleId, 0);
    }

    public Dictionary<string, int> GetAllClickCounts()
    {
        return new Dictionary<string, int>(_clickCounts);
    }

    public int GetTotalClicks()
    {
        return _clickCounts.Values.Sum();
    }

    public int GetUniqueArticlesClicked()
    {
        return _clickCounts.Count;
    }

    public IEnumerable<KeyValuePair<string, int>> GetTopClickedArticles(int count = 5)
    {
        return _clickCounts
            .OrderByDescending(x => x.Value)
            .Take(count);
    }
}
