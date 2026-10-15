var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "LightNotes API");

app.Run();

public partial class Program;
