using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NewsStats.Wasm;
using NewsStats.Wasm.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Use relative URLs - YARP proxy handles routing
var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);

// Configure HttpClient for API calls (will use /api/* routes through YARP)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseAddress });

// Register services
builder.Services.AddScoped<NewsApiClient>();
builder.Services.AddScoped<SearchStateService>();
builder.Services.AddSingleton<ArticleClickedEvent>();
builder.Services.AddScoped<ArticlesClickState>();

await builder.Build().RunAsync();
