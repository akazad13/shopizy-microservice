using System.Diagnostics.CodeAnalysis;

namespace Shopizy.SharedKernel.Results;

/// <summary>
/// Represents the non-generic result of an operation.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot contain an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must contain an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Represents the typed result of an operation returning <typeparamref name="TValue"/>.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failure result.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);

    public Result<TTarget> Map<TTarget>(Func<TValue, TTarget> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return IsSuccess ? Result.Success(map(Value)) : Result.Failure<TTarget>(Error);
    }

    public Result<TTarget> Bind<TTarget>(Func<TValue, Result<TTarget>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        return IsSuccess ? bind(Value) : Result.Failure<TTarget>(Error);
    }

    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }
}
