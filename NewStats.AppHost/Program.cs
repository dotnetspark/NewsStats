var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var apiKey = builder.AddParameter("NewsApiKey", secret: true);

var api = builder.AddProject<Projects.NewsStats_Api>("api")
    .WithReference(cache)
    .WaitFor(cache)
    .WithEnvironment("NewsApiKey", apiKey);

var wasm = builder.AddProject<Projects.NewsStats_Wasm>("client");

builder.AddProject<Projects.NewsStats_Proxy>("proxy")
    .WithReference(api)
    .WithReference(wasm)
    .WaitFor(api)
    .WaitFor(wasm)
    .WithExternalHttpEndpoints();

builder.Build().Run();