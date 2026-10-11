using System.Globalization;
using System.Text;

namespace CimE;

/// <summary>
/// 将 CimE 对象序列化为确定性的 CRLF 文本。
/// </summary>
public static class CimESerializer
{
    private const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    public static string Serialize(EFile file, CimEFormat format)
    {
        ArgumentNullException.ThrowIfNull(file);
        Validate(file, format);

        return format switch
        {
            CimEFormat.Standard2013 => Serialize2013(file),
            CimEFormat.Standard2019 => Serialize2019(file),
            CimEFormat.OnLineMonitor => SerializeOnLineMonitor(file),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "不支持的 CimE 格式。")
        };
    }

    private static string Serialize2013(EFile file)
    {
        var lines = new List<string>();
        if (file.Value is not null || file.Attributes.Count > 0)
        {
            var header = new StringBuilder("<!");
            if (file.Value is not null)
            {
                header.Append("System=").Append(FormatHeaderValue(file.Value, $"根节点 '{file.Name}' 的值", '\''));
            }

            foreach (var attribute in file.Attributes)
            {
                if (header.Length > 2)
                {
                    header.Append(' ');
                }

                header.Append(attribute.Key).Append('=')
                    .Append(FormatHeaderValue(attribute.Value, $"根节点 '{file.Name}' 属性 '{attribute.Key}'", '\''));
            }

            lines.Add(header.Append(" !>").ToString());
        }

        lines.Add($"<{file.Name}>");
        foreach (var child in file.Children)
        {
            switch (child)
            {
                case EDescriptionNode description:
                    lines.Add(RenderDescription(description, '\''));
                    break;
                case EDataSetNode dataSet:
                    lines.AddRange(RenderDataSet(dataSet, CimEFormat.Standard2013));
                    break;
            }
        }

        lines.Add($"</{file.Name}>");
        return string.Join("\r\n", lines);
    }

    private static string Serialize2019(EFile file)
    {
        var lines = new List<string>
        {
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
            RenderStartTag(file.Name, file.Attributes, '\"', file.Value)
        };

        foreach (var child in file.Children)
        {
            switch (child)
            {
                case EDescriptionNode description:
                    lines.Add(RenderDescription(description, '\"'));
                    break;
                case EDataSetNode dataSet:
                    lines.AddRange(RenderDataSet(dataSet, CimEFormat.Standard2019));
                    break;
            }
        }

        lines.Add($"</{file.Name}>");
        return string.Join("\r\n", lines);
    }

    private static string SerializeOnLineMonitor(EFile file)
    {
        var lines = new List<string>
        {
            RenderOnLineHeader(file)
        };

        var previousWasDescription = false;
        foreach (var child in file.Children)
        {
            switch (child)
            {
                case EDescriptionNode description:
                    lines.Add(RenderDescription(description, '\''));
                    previousWasDescription = true;
                    break;
                case EDataSetNode dataSet:
                    if (previousWasDescription)
                    {
                        lines.Add(string.Empty);
                    }

                    lines.AddRange(RenderDataSet(dataSet, CimEFormat.OnLineMonitor));
                    previousWasDescription = false;
                    break;
            }
        }

        return string.Join("\r\n", lines);
    }

    private static string RenderOnLineHeader(EFile file)
    {
        var builder = new StringBuilder("<!").Append(file.Name);
        if (file.Value is not null)
        {
            builder.Append('=').Append(FormatHeaderValue(file.Value, $"根节点 '{file.Name}' 的值", '\''));
        }

        foreach (var attribute in file.Attributes)
        {
            builder.Append(' ').Append(attribute.Key).Append('=')
                .Append(FormatHeaderValue(attribute.Value, $"根节点 '{file.Name}' 属性 '{attribute.Key}'", '\''));
        }

        return builder.Append("!>").ToString();
    }

    private static string RenderStartTag(string name, IReadOnlyList<KeyValuePair<string, object?>> attributes, char quote, object? value = null)
    {
        var builder = new StringBuilder("<").Append(name);
        if (value is not null)
        {
            builder.Append(" Value=").Append(FormatValue(value, $"根节点 '{name}' 的值", quote));
        }

        AppendAttributes(builder, attributes, name, quote);
        return builder.Append('>').ToString();
    }

    private static string RenderDescription(EDescriptionNode node, char quote)
    {
        var builder = new StringBuilder("<").Append(node.Name);
        AppendAttributes(builder, node.Attributes, node.Name, quote);
        return builder.Append("/>").ToString();
    }

    private static IEnumerable<string> RenderDataSet(EDataSetNode node, CimEFormat format)
    {
        var xmlStyle = format == CimEFormat.Standard2019;
        var quote = xmlStyle ? '\"' : '\'';
        var lines = new List<string>
        {
            RenderStartTag(node.Name, node.Attributes, quote),
            xmlStyle
                ? $"<@> {string.Join(' ', node.Columns.Select(column => column.Name))} </@>"
                : $"@ {string.Join(' ', node.Columns.Select(column => column.Name))}"
        };

        if (node.Columns.Any(column => !string.IsNullOrEmpty(column.Description)))
        {
            var descriptions = string.Join(' ', node.Columns.Select(column => column.Description ?? string.Empty));
            lines.Add(xmlStyle ? $"<!-- {descriptions} -->" : $"// {descriptions}".TrimEnd());
        }

        lines.AddRange(node.Rows.Select((row, index) =>
        {
            var values = string.Join(' ', row.Values.Select((value, columnIndex) =>
                FormatValue(value, RowContext(node, index, columnIndex), quote)));
            return xmlStyle ? $"<#> {values} </#>" : $"# {values}";
        }));
        lines.Add($"</{node.Name}>");
        return lines;
    }

    private static void AppendAttributes(StringBuilder builder, IReadOnlyList<KeyValuePair<string, object?>> attributes, string nodeName, char quote)
    {
        foreach (var attribute in attributes)
        {
            builder.Append(' ').Append(attribute.Key).Append('=')
                .Append(FormatValue(attribute.Value, $"节点 '{nodeName}' 属性 '{attribute.Key}'", quote));
        }
    }

    private static string FormatHeaderValue(object? value, string context, char quote)
    {
        if (value is EValue eValue)
        {
            return eValue.Kind switch
            {
                EValue.EValueKind.String => Quote((string)eValue.Value!, context, quote),
                EValue.EValueKind.DateTime => Quote(FormatDateTime((DateTime)eValue.Value!, eValue.Format, context), context, quote),
                _ => FormatValue(value, context, quote)
            };
        }

        if (value is string text)
        {
            return IsSafeHeaderToken(text) ? text : Quote(text, context, quote);
        }

        return FormatValue(value, context, quote);
    }

    private static string FormatValue(object? value, string context, char quote)
    {
        if (value is EValue eValue)
        {
            return eValue.Kind switch
            {
                EValue.EValueKind.Null => "NULL",
                EValue.EValueKind.Token => FormatToken((string)eValue.Value!, context),
                EValue.EValueKind.Number => FormatNumber((decimal)eValue.Value!, eValue.Format, context),
                EValue.EValueKind.DateTime => Quote(FormatDateTime((DateTime)eValue.Value!, eValue.Format, context), context, quote),
                EValue.EValueKind.String => Quote((string)eValue.Value!, context, quote),
                _ => throw new CimEValidationException($"{context} 的值类型不受支持。")
            };
        }

        if (value is null)
        {
            return "NULL";
        }

        if (value is string text)
        {
            return Quote(text, context, quote);
        }

        if (value is DateTime dateTime)
        {
            return Quote(FormatDateTime(dateTime, null, context), context, quote);
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return Quote(dateTimeOffset.ToString(DefaultDateTimeFormat, CultureInfo.InvariantCulture), context, quote);
        }

        if (value is bool boolean)
        {
            return boolean ? "true" : "false";
        }

        if (value is char character)
        {
            return Quote(character.ToString(), context, quote);
        }

        if (value is IFormattable formattable)
        {
            if (value is double doubleValue && (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue)) ||
                value is float floatValue && (float.IsNaN(floatValue) || float.IsInfinity(floatValue)))
            {
                throw new CimEValidationException($"{context} 的数字必须是有限值。");
            }

            return formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        throw new CimEValidationException($"{context} 的值类型 '{value.GetType().Name}' 不受支持。");
    }

    private static string FormatToken(string value, string context)
    {
        if (string.IsNullOrEmpty(value) || value.Any(char.IsWhiteSpace) || value.Any(char.IsControl) ||
            value.IndexOfAny(new[] { '#', '<', '>', '\'', '"' }) >= 0)
        {
            throw new CimEValidationException($"{context} 的 Token 不能包含空格、换行、控制字符、引号或分隔符。");
        }

        return value;
    }

    private static string FormatNumber(decimal value, string? format, string context)
    {
        try
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }
        catch (FormatException exception)
        {
            throw new CimEValidationException($"{context} 的数字格式 '{format}' 无效。", exception);
        }
    }

    private static string FormatDateTime(DateTime value, string? format, string context)
    {
        try
        {
            return value.ToString(format ?? DefaultDateTimeFormat, CultureInfo.InvariantCulture);
        }
        catch (FormatException exception)
        {
            throw new CimEValidationException($"{context} 的日期时间格式 '{format}' 无效。", exception);
        }
    }

    private static string Quote(string value, string context, char preferredQuote)
    {
        ValidateText(value, context);
        var alternateQuote = preferredQuote == '\'' ? '"' : '\'';
        if (!value.Contains(preferredQuote))
        {
            return $"{preferredQuote}{value}{preferredQuote}";
        }

        if (!value.Contains(alternateQuote))
        {
            return $"{alternateQuote}{value}{alternateQuote}";
        }

        throw new CimEValidationException($"{context} 同时包含单引号和双引号，无法在第一阶段格式中安全输出。");
    }

    private static void Validate(EFile file, CimEFormat format)
    {
        ValidateName(file.Name, "根节点");
        ValidateAttributes(file.Name, file.Attributes);
        _ = FormatValue(file.Value, $"根节点 '{file.Name}' 的值", '\'' );

        foreach (var child in file.Children)
        {
            switch (child)
            {
                case EDescriptionNode description:
                    ValidateName(description.Name, "节点");
                    ValidateAttributes(description.Name, description.Attributes);
                    break;
                case EDataSetNode dataSet:
                    ValidateDataSet(dataSet);
                    break;
                default:
                    throw new CimEValidationException($"根节点 '{file.Name}' 包含不受支持的子节点类型。");
            }
        }

        if (format == CimEFormat.OnLineMonitor && file.Value is null)
        {
            throw new CimEValidationException($"根节点 '{file.Name}' 在 OnLineMonitor 模式下必须设置根标签值。");
        }
    }

    private static void ValidateDataSet(EDataSetNode dataSet)
    {
        ValidateName(dataSet.Name, "数据节点");
        ValidateAttributes(dataSet.Name, dataSet.Attributes);
        if (dataSet.Columns.Count == 0)
        {
            throw new CimEValidationException($"数据节点 '{dataSet.Name}' 至少需要一列。");
        }

        var duplicateColumn = dataSet.Columns
            .GroupBy(column => column.Name, StringComparer.Ordinal)
            .FirstOrDefault(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1);
        if (duplicateColumn is not null)
        {
            throw new CimEValidationException($"数据节点 '{dataSet.Name}' 的列名 '{duplicateColumn.Key}' 为空或重复。");
        }

        foreach (var column in dataSet.Columns)
        {
            ValidateText(column.Name, $"数据节点 '{dataSet.Name}' 的列名");
            if (column.Description is not null)
            {
                ValidateText(column.Description, $"数据节点 '{dataSet.Name}' 的列 '{column.Name}' 说明");
            }
        }

        for (var rowIndex = 0; rowIndex < dataSet.Rows.Count; rowIndex++)
        {
            var row = dataSet.Rows[rowIndex];
            if (row.Values.Count != dataSet.Columns.Count)
            {
                throw new CimEValidationException($"数据节点 '{dataSet.Name}' 第 {rowIndex + 1} 行有 {row.Values.Count} 个值，但列数为 {dataSet.Columns.Count}。");
            }

            for (var columnIndex = 0; columnIndex < row.Values.Count; columnIndex++)
            {
                _ = FormatValue(row.Values[columnIndex], RowContext(dataSet, rowIndex, columnIndex), '\'' );
            }
        }
    }

    private static void ValidateAttributes(string nodeName, IReadOnlyList<KeyValuePair<string, object?>> attributes)
    {
        var duplicate = attributes
            .GroupBy(attribute => attribute.Key, StringComparer.Ordinal)
            .FirstOrDefault(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1);
        if (duplicate is not null)
        {
            throw new CimEValidationException($"节点 '{nodeName}' 的属性名 '{duplicate.Key}' 为空或重复。");
        }

        foreach (var attribute in attributes)
        {
            _ = FormatValue(attribute.Value, $"节点 '{nodeName}' 属性 '{attribute.Key}'", '\'' );
        }
    }

    private static void ValidateName(string name, string kind)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new CimEValidationException($"{kind}名称不能为空。");
        }

        ValidateText(name, $"{kind}名称 '{name}'");
    }

    private static void ValidateText(string text, string context)
    {
        if (text.Any(char.IsControl) || text.Contains('\r') || text.Contains('\n'))
        {
            throw new CimEValidationException($"{context} 不能包含换行或控制字符。");
        }
    }

    private static bool IsSafeHeaderToken(string value)
    {
        return !string.IsNullOrEmpty(value) && !value.Any(char.IsWhiteSpace) && !value.Any(char.IsControl) &&
            value.IndexOfAny(new[] { '=', '!', '<', '>', '\'', '"' }) < 0;
    }

    private static string RowContext(EDataSetNode node, int rowIndex, int columnIndex)
    {
        return $"节点 '{node.Name}' 第 {rowIndex + 1} 行列 '{node.Columns[columnIndex].Name}'";
    }
}
