using NewsStats.Wasm.Models;

namespace NewsStats.Wasm.Services;

public class ArticleClickedEvent
{
    public event Action<Article>? ArticleClicked;

    public void NotifyArticleClicked(Article article)
    {
        ArticleClicked?.Invoke(article);
    }
}
