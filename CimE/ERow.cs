namespace CimE;

/// <summary>
/// 数据集中的一行有序数据。
/// </summary>
public sealed class ERow
{
    public ERow(params object?[] values)
    {
        Values = values ?? throw new ArgumentNullException(nameof(values));
    }

    public IReadOnlyList<object?> Values { get; }
}
