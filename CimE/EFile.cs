namespace CimE;

/// <summary>
/// 代表一个完整的 CimE 文件。
/// </summary>
public sealed class EFile
{
    private readonly List<KeyValuePair<string, object?>> _attributes = new();
    private readonly List<object> _children = new();

    public EFile(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Name { get; }

    public object? Value { get; private set; }

    public IReadOnlyList<KeyValuePair<string, object?>> Attributes => _attributes;

    public IReadOnlyList<object> Children => _children;

    public EFile SetValue(object? value)
    {
        Value = value;
        return this;
    }

    public EFile SetAttribute(string name, object? value)
    {
        _attributes.Add(new KeyValuePair<string, object?>(name, value));
        return this;
    }

    public EDescriptionNode AddDescription(string name)
    {
        var node = new EDescriptionNode(name);
        _children.Add(node);
        return node;
    }

    public EDataSetNode AddDataSet(string name)
    {
        var node = new EDataSetNode(name);
        _children.Add(node);
        return node;
    }
}
