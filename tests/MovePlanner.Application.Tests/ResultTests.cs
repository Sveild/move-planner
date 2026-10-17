using FluentAssertions;
using MovePlanner.Application.Common;
using Xunit;

namespace MovePlanner.Application.Tests;

public sealed class ResultTests
{
    [Fact]
    public void Success_is_successful_and_exposes_the_value()
    {
        Result<int> result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Success_has_no_error()
    {
        Result<int> result = Result.Success(42);

        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_is_not_successful_and_exposes_the_error()
    {
        Error error = Error.NotFound("job_application.not_found", "No application with that id.");

        Result<int> result = Result.Failure<int>(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Accessing_the_value_of_a_failure_throws()
    {
        Result<int> result = Result.Failure<int>(Error.Unexpected("boom", "Something went wrong."));

        Action act = () => _ = result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Error_factories_set_the_matching_kind()
    {
        Error.Validation("code", "message").Kind.Should().Be(ErrorKind.Validation);
        Error.NotFound("code", "message").Kind.Should().Be(ErrorKind.NotFound);
        Error.Conflict("code", "message").Kind.Should().Be(ErrorKind.Conflict);
        Error.Unexpected("code", "message").Kind.Should().Be(ErrorKind.Unexpected);
    }

    [Fact]
    public void Errors_with_the_same_values_are_equal()
    {
        Error first = Error.Validation("code", "message");
        Error second = Error.Validation("code", "message");

        first.Should().Be(second);
    }
}
