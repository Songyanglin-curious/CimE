namespace CimE;

/// <summary>
/// 明确表达 CimE 输出类型或格式的值。
/// </summary>
public sealed class EValue
{
    private EValue(EValueKind kind, object? value, string? format)
    {
        Kind = kind;
        Value = value;
        Format = format;
    }

    internal EValueKind Kind { get; }

    internal object? Value { get; }

    internal string? Format { get; }

    public static EValue Null() => new(EValueKind.Null, null, null);

    public static EValue Token(string value) => new(EValueKind.Token, value ?? throw new ArgumentNullException(nameof(value)), null);

    public static EValue Number(decimal value, string? format = null) => new(EValueKind.Number, value, format);

    public static EValue DateTime(DateTime value, string? format = null) => new(EValueKind.DateTime, value, format);

    public static EValue String(string value) => new(EValueKind.String, value ?? throw new ArgumentNullException(nameof(value)), null);

    internal enum EValueKind
    {
        Null,
        Token,
        Number,
        DateTime,
        String
    }
}
