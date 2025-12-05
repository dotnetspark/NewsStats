var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

var webProject = builder.AddProject("newstats-web", @"..\NewStats.Web\NewStats.Web.csproj")
    .WithReference(redis)
    .WaitFor(redis)
    .WithExternalHttpEndpoints();

builder.Build().Run();
