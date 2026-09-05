using FluentAssertions;
using Shopizy.SharedKernel.Results;
using Xunit;

namespace Shopizy.SharedKernel.UnitTests.Results;

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_CreatesFailureResultWithError()
    {
        var error = Error.Validation("User.InvalidEmail", "Email format is invalid.");
        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void TypedResult_WhenSuccess_ReturnsValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void TypedResult_WhenFailure_AccessingValueThrowsInvalidOperationException()
    {
        var error = Error.NotFound("Item.NotFound", "The item was not found.");
        var result = Result.Failure<string>(error);

        result.IsFailure.Should().BeTrue();
        var act = () => _ = result.Value;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failure result*");
    }

    [Fact]
    public void Map_WhenSuccess_TransformsValue()
    {
        var result = Result.Success("123");
        var mapped = result.Map(int.Parse);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(123);
    }

    [Fact]
    public void Map_WhenFailure_PropagatesErrorWithoutExecutingMap()
    {
        var error = Error.Failure("Calc.Error", "Calculation failed.");
        var result = Result.Failure<string>(error);

        var executed = false;
        var mapped = result.Map(s => { executed = true; return s.Length; });

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
        executed.Should().BeFalse();
    }

    [Fact]
    public void Bind_WhenSuccess_ChainsResult()
    {
        var result = Result.Success(10);
        var bound = result.Bind(val => Result.Success(val * 2));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(20);
    }

    [Fact]
    public void Match_WhenSuccess_CallsSuccessBranch()
    {
        var result = Result.Success("Hello");
        var message = result.Match(
            val => $"Success: {val}",
            err => $"Error: {err.Code}");

        message.Should().Be("Success: Hello");
    }

    [Fact]
    public void Match_WhenFailure_CallsFailureBranch()
    {
        var result = Result.Failure<string>(Error.Conflict("ID.Exists", "Already exists"));
        var message = result.Match(
            val => $"Success: {val}",
            err => $"Error: {err.Code}");

        message.Should().Be("Error: ID.Exists");
    }
}
