using FluentAssertions;
using MovePlanner.Domain.Exceptions;
using Xunit;

namespace MovePlanner.Domain.Tests;

public sealed class ExceptionTests
{
    [Fact]
    public void DomainException_has_a_default_message()
    {
        new DomainException().Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void DomainException_carries_its_message()
    {
        new DomainException("A rule was violated.").Message.Should().Be("A rule was violated.");
    }

    [Fact]
    public void DomainException_carries_its_inner_exception()
    {
        var inner = new InvalidOperationException("cause");

        DomainException exception = new("A rule was violated.", inner);

        exception.Message.Should().Be("A rule was violated.");
        exception.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void InvalidStatusTransitionException_is_a_domain_exception()
    {
        new InvalidStatusTransitionException("Rejected.").Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void InvalidStatusTransitionException_has_a_default_message()
    {
        new InvalidStatusTransitionException().Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void InvalidStatusTransitionException_carries_its_inner_exception()
    {
        var inner = new InvalidOperationException("cause");

        InvalidStatusTransitionException exception = new("Rejected.", inner);

        exception.Message.Should().Be("Rejected.");
        exception.InnerException.Should().BeSameAs(inner);
    }
}
