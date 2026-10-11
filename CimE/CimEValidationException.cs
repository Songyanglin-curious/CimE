namespace CimE;

/// <summary>
/// CimE 对象在序列化前未通过结构或值校验时抛出的异常。
/// </summary>
public sealed class CimEValidationException : Exception
{
    public CimEValidationException(string message)
        : base(message)
    {
    }

    public CimEValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
