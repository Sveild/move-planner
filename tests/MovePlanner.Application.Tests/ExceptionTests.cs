using FluentAssertions;
using MovePlanner.Application.Common.Exceptions;
using Xunit;

namespace MovePlanner.Application.Tests;

public sealed class ExceptionTests
{
    [Fact]
    public void NotFoundException_has_a_default_message()
    {
        new NotFoundException().Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void NotFoundException_carries_its_message()
    {
        new NotFoundException("The application does not exist.").Message
            .Should().Be("The application does not exist.");
    }

    [Fact]
    public void NotFoundException_carries_its_inner_exception()
    {
        var inner = new InvalidOperationException("cause");

        NotFoundException exception = new("The application does not exist.", inner);

        exception.Message.Should().Be("The application does not exist.");
        exception.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void ConflictException_has_a_default_message()
    {
        new ConflictException().Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ConflictException_carries_its_message()
    {
        new ConflictException("The application already exists.").Message
            .Should().Be("The application already exists.");
    }

    [Fact]
    public void ConflictException_carries_its_inner_exception()
    {
        var inner = new InvalidOperationException("cause");

        ConflictException exception = new("The application already exists.", inner);

        exception.Message.Should().Be("The application already exists.");
        exception.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void ForbiddenAccessException_has_a_default_message()
    {
        new ForbiddenAccessException().Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ForbiddenAccessException_carries_its_message()
    {
        new ForbiddenAccessException("Another user owns the application.").Message
            .Should().Be("Another user owns the application.");
    }

    [Fact]
    public void ForbiddenAccessException_carries_its_inner_exception()
    {
        var inner = new InvalidOperationException("cause");

        ForbiddenAccessException exception = new("Another user owns the application.", inner);

        exception.Message.Should().Be("Another user owns the application.");
        exception.InnerException.Should().BeSameAs(inner);
    }
}
