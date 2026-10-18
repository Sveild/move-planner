using FluentAssertions;
using MovePlanner.Application.Diagnostics;
using Xunit;

namespace MovePlanner.Application.Tests;

public sealed class PingQueryHandlerTests
{
    [Fact]
    public void Handle_echoes_the_message()
    {
        string result = PingQueryHandler.Handle(new PingQuery("pong"));

        result.Should().Be("pong");
    }

    [Fact]
    public void Handle_rejects_a_null_query()
    {
        Action act = () => PingQueryHandler.Handle(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("query");
    }
}
