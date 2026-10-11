namespace CimE;

/// <summary>
/// 只有名称和属性的自闭合节点。
/// </summary>
public sealed class EDescriptionNode
{
    private readonly List<KeyValuePair<string, object?>> _attributes = new();

    public EDescriptionNode(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Name { get; }

    public IReadOnlyList<KeyValuePair<string, object?>> Attributes => _attributes;

    public EDescriptionNode SetAttribute(string name, object? value)
    {
        _attributes.Add(new KeyValuePair<string, object?>(name, value));
        return this;
    }
}
