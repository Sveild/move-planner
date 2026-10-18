using System.Diagnostics.CodeAnalysis;
using JasperFx;
using MovePlanner.Api;
using MovePlanner.Api.Endpoints;
using MovePlanner.Api.Errors;
using MovePlanner.Infrastructure;
using Wolverine.Http;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseMovePlannerWolverine(builder.Configuration, builder.Environment);
builder.Services.AddWolverineHttp();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseExceptionHandler();
app.MapEndpoints();
app.MapWolverineEndpoints();

return await app.RunJasperFxCommands(args);

public partial class Program
{
    [ExcludeFromCodeCoverage]
    protected Program()
    {
    }
}
