using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

await app.RunAsync();

public partial class Program
{
    [ExcludeFromCodeCoverage]
    protected Program()
    {
    }
}

