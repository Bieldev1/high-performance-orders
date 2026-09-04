namespace Domain.Common.Results;

/// <summary>
/// Representa o resultado de uma operação com valor de retorno. Só deve possuir Value quando Valid for true.
/// </summary>
public class Result<T> : Result
{
    public Result(T? value = default) => Value = value;

    public T? Value { get; private set; }

    public static Result<T> Ok(T value) => new(value);

    public static new Result<T> Fail(ResultCode code, string message) => new Result<T>().Set(code, message) as Result<T>;

    public new Result<T> Set(ResultCode code, string message)
    {
        base.Set(code, message);
        return this;
    }
}
