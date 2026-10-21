using System.Diagnostics.CodeAnalysis;
using JasperFx;
using MovePlanner.Api;
using MovePlanner.Api.Errors;
using MovePlanner.Api.OpenApi;
using MovePlanner.Infrastructure;
using Scalar.AspNetCore;
using Wolverine.Http;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseMovePlannerWolverine(builder.Configuration, builder.Environment);
builder.Services.AddWolverineHttp();
builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer<CamelCaseQueryParameterTransformer>();
    options.AddOperationTransformer<ResultResponseTransformer>();
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseExceptionHandler();
app.MapWolverineEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

return await app.RunJasperFxCommands(args);

public partial class Program
{
    [ExcludeFromCodeCoverage]
    protected Program()
    {
    }
}
