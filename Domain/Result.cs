namespace Domain;

public class Result<T>
{
    public Error Error { get; private init; }

    public bool IsSuccess { get; private init; }

    public T Value { get; private init; }

    public Result(Error error)
    {
        IsSuccess = false;
        Error = error;
        Value = default!;
    }

    public Result(T value)
    {
        IsSuccess = true;
        Error = new Error(String.Empty);
        Value = value;
    }

    public static implicit operator Result<T>(Error error)
    {
        return new Result<T>(error);
    }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(value);
    }
}
