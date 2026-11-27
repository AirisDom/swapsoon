var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => TypedResults.Ok("ok"));

app.Run();

public partial class Program;
