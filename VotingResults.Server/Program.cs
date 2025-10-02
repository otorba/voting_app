using VotingResults.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapHealthChecks(pattern: "/healthz");
app.MapHub<ResultsHub>(pattern: "/hubs/results");
app.MapFallbackToFile(filePath: "index.html");

app.Run();