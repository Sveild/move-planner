using System.Diagnostics.CodeAnalysis;
using MovePlanner.Api.Endpoints;
using MovePlanner.Api.Errors;
using MovePlanner.Application;
using MovePlanner.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseExceptionHandler();
app.MapEndpoints();

await app.RunAsync();

public partial class Program
{
    [ExcludeFromCodeCoverage]
    protected Program()
    {
    }
}
