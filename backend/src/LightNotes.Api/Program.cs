using LightNotes.Api;
using LightNotes.Application;
using LightNotes.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication().AddInfrastructure().AddApi();

var app = builder.Build();

app.UseApi();
app.MapApi();

app.Run();

public partial class Program;
