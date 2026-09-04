namespace Domain.Common.Results;

/// <summary>
/// Segue a numeração do HTTP para facilitar conversões nos controllers.
/// </summary>
public enum ResultCode
{
    Ok = 200,
    BadRequest = 400,
    BusinessError = 422,
    GenericError = 500
}
