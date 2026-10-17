using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MovePlanner.Api.Endpoints;
using MovePlanner.Application.Common;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

public sealed class ResultExtensionsTests
{
    [Fact]
    public void Successful_result_becomes_ok_with_the_value()
    {
        Result<string> result = Result.Success("value");

        IResult httpResult = result.ToHttpResult();

        httpResult.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status200OK);
        httpResult.Should().BeAssignableTo<IValueHttpResult>()
            .Which.Value.Should().Be("value");
    }

    [Theory]
    [InlineData(ErrorKind.Validation, StatusCodes.Status400BadRequest, "Validation failed")]
    [InlineData(ErrorKind.NotFound, StatusCodes.Status404NotFound, "Not found")]
    [InlineData(ErrorKind.Conflict, StatusCodes.Status409Conflict, "Conflict")]
    [InlineData(ErrorKind.Unexpected, StatusCodes.Status500InternalServerError, "Unexpected error")]
    public void Failed_result_becomes_a_problem_of_its_error_kind(ErrorKind kind, int statusCode, string title)
    {
        Error error = new(kind, "job_application.invalid", "The request is not valid.");

        IResult httpResult = Result.Failure<string>(error).ToHttpResult();

        ProblemHttpResult problem = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problem.StatusCode.Should().Be(statusCode);
        problem.ProblemDetails.Status.Should().Be(statusCode);
        problem.ProblemDetails.Title.Should().Be(title);
        problem.ProblemDetails.Detail.Should().Be(error.Message);
        problem.ProblemDetails.Extensions.Should().ContainKey("code").WhoseValue.Should().Be(error.Code);
    }

    [Fact]
    public void Error_kind_outside_the_enum_becomes_an_unexpected_problem()
    {
        Error error = new((ErrorKind)int.MaxValue, "job_application.unknown", "Something went wrong.");

        IResult httpResult = Result.Failure<string>(error).ToHttpResult();

        ProblemHttpResult problem = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problem.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void ToHttpResult_rejects_a_null_result()
    {
        Result<string>? result = null;

        Action map = () => result!.ToHttpResult();

        map.Should().Throw<ArgumentNullException>();
    }
}
