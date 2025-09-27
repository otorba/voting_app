using Microsoft.Extensions.Options;
using StackExchange.Redis;
using VotingApp.Server.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(key: "Redis"));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
  var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;

  return string.IsNullOrWhiteSpace(options.ConnectionString)
    ? throw new InvalidOperationException(message: "Redis connection string is not configured.")
    : ConnectionMultiplexer.Connect(options.ConnectionString);
});

builder.Services.AddHealthChecks();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
  app.UseDeveloperExceptionPage();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapHealthChecks(pattern: "/healthz");
app.MapFallbackToFile(filePath: "index.html");

app.MapControllers();

app.Run();