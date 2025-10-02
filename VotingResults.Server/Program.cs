using VotingResults.Server.Hubs;
using VotingResults.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddSignalR();
builder.Services.AddHostedService<ResultsBroadcastService>();

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapHealthChecks(pattern: "/healthz");
app.MapHub<ResultsHub>(pattern: "/hubs/results");
app.MapFallbackToFile(filePath: "index.html");

app.Run();