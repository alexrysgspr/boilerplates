using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Host.ConfigureDefaultAppConfiguration<Program>();
builder.Host.UseSerilog();

builder
    .Services
    .AddDependencies(configuration);


var app = builder.Build();
Log.Logger = app.CreateDefaultLogger();

app
    .UseRouting()
    .UseCustomHealthChecks(app.Configuration);

try
{
    await app.SetupTableStorageServices();
    await app.RunAsync();
    return 0;
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly, check the application's host configuration.");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}


public partial class Program
{
}
