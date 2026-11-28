using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SwapSoon.Api;
using SwapSoon.Data;
using SwapSoon.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<UrgencyCalculator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapGet("/health", () => TypedResults.Ok("ok"));
app.MapItemsEndpoints();

app.Run();

public partial class Program;
