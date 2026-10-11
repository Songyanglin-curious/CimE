namespace CimE;

/// <summary>
/// 包含列定义和多行数据的数据节点。
/// </summary>
public sealed class EDataSetNode
{
    private readonly List<KeyValuePair<string, object?>> _attributes = new();
    private readonly List<EColumn> _columns = new();
    private readonly List<ERow> _rows = new();

    public EDataSetNode(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Name { get; }

    public IReadOnlyList<KeyValuePair<string, object?>> Attributes => _attributes;

    public IReadOnlyList<EColumn> Columns => _columns;

    public IReadOnlyList<ERow> Rows => _rows;

    public EDataSetNode SetAttribute(string name, object? value)
    {
        _attributes.Add(new KeyValuePair<string, object?>(name, value));
        return this;
    }

    public EDataSetNode AddColumn(string name, string? description = null)
    {
        _columns.Add(new EColumn(name, description));
        return this;
    }

    public EDataSetNode AddRow(params object?[] values)
    {
        _rows.Add(new ERow(values));
        return this;
    }
}
