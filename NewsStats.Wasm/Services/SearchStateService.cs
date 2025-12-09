using NewsStats.Wasm.Models;

namespace NewsStats.Wasm.Services;

public class SearchStateService
{
    public string Query { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<Article> Articles { get; set; } = new();
    public bool HasSearched { get; set; }

    public void SaveSearchState(string query, DateTime? fromDate, DateTime? toDate, List<Article> articles)
    {
        Query = query;
        FromDate = fromDate;
        ToDate = toDate;
        Articles = articles;
        HasSearched = true;
    }

    public void Clear()
    {
        Query = string.Empty;
        FromDate = null;
        ToDate = null;
        Articles = new();
        HasSearched = false;
    }
}
