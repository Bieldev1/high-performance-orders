namespace Domain.Common.Results;

/// <summary>
/// Representa o resultado de uma operação. Por padrão é assumido como válido ao ser instanciado.
/// </summary>
public class Result
{
    public Result() => SetToOk();

    public string Message { get; private set; } = "Success";

    public ResultCode ResultCode { get; private set; } = ResultCode.Ok;

    public bool Valid => (int)ResultCode < 400;

    public virtual Result Set(ResultCode code, string? message)
    {
        if (code == ResultCode.Ok)
        {
            SetToOk(message);
            return this;
        }

        Message = message ?? string.Empty;
        ResultCode = code;
        return this;
    }

    public static Result Ok(string? message = null) => new Result().Set(ResultCode.Ok, message ?? "Success");

    public static Result Fail(ResultCode code, string message) => new Result().Set(code, message);

    private void SetToOk(string? message = null)
    {
        Message = string.IsNullOrWhiteSpace(message) ? "Success" : message;
        ResultCode = ResultCode.Ok;
    }
}
