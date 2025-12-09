using NewsStats.Wasm.Models;

namespace NewsStats.Wasm.Services;

public class ArticlesClickState : IDisposable
{
    private readonly Dictionary<string, Article> _clickedArticles = [];
    private readonly ArticleClickedEvent _articleClickedEvent;

    public event Action? OnStateChanged;

    public ArticlesClickState(ArticleClickedEvent articleClickedEvent)
    {
        _articleClickedEvent = articleClickedEvent;
        _articleClickedEvent.ArticleClicked += OnArticleClicked;
    }

    private void OnArticleClicked(Article article)
    {
        if (_clickedArticles.ContainsKey(article.Id))
        {
            _clickedArticles[article.Id].ClickCount++;
        }
        else
        {
            article.ClickCount = 1;
            _clickedArticles[article.Id] = article;
        }

        OnStateChanged?.Invoke();
    }

    public List<Article> GetClickedArticles() => _clickedArticles.Values
            .OrderByDescending(a => a.ClickCount)
            .ToList();

    public int GetTotalClicks() => _clickedArticles.Values.Sum(a => a.ClickCount);

    public void Dispose()
    {
        _articleClickedEvent.ArticleClicked -= OnArticleClicked;
    }
}