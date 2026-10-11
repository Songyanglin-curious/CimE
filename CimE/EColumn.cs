namespace CimE;

/// <summary>
/// 数据集中的一列定义。
/// </summary>
public sealed class EColumn
{
    public EColumn(string name, string? description = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
    }

    public string Name { get; }

    public string? Description { get; }
}
