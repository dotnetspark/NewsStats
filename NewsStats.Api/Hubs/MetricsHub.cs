using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace NewsStats.Api.Hubs;

public class MetricsHub : Hub
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _sessionMetrics = new();

    public override async Task OnConnectedAsync()
    {
        // Get sessionId from query string when client connects
        var sessionId = Context.GetHttpContext()?.Request.Query["sessionId"].ToString();
        if (!string.IsNullOrEmpty(sessionId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        }
        await base.OnConnectedAsync();
    }

    public async Task IncrementClickCount(string sessionId, string articleId)
    {
        var sessionData = _sessionMetrics.GetOrAdd(sessionId, _ => new ConcurrentDictionary<string, int>());
        var newCount = sessionData.AddOrUpdate(articleId, 1, (_, count) => count + 1);

        // Push update to all clients in this session group
        await Clients.Group(sessionId).SendAsync("MetricsUpdated", articleId, newCount);
    }

    public async Task<Dictionary<string, int>> GetSessionMetrics(string sessionId)
    {
        var metrics = _sessionMetrics.TryGetValue(sessionId, out var sessionData)
            ? new Dictionary<string, int>(sessionData)
            : new Dictionary<string, int>();

        return await Task.FromResult(metrics);
    }
}
