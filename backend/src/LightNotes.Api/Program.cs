using LightNotes.Api;
using LightNotes.Application;
using LightNotes.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication().AddInfrastructure().AddApi(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.MigrateDatabaseAsync();
}

app.UseApi();
app.MapApi();

app.Run();

public partial class Program;
