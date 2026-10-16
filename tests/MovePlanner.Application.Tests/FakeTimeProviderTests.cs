using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace MovePlanner.Application.Tests;

public sealed class FakeTimeProviderTests
{
    [Fact]
    public void Fake_time_provider_reports_the_configured_time()
    {
        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(start);

        timeProvider.GetUtcNow().Should().Be(start);
    }
}
