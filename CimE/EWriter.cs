using System.Text;

namespace CimE;

/// <summary>
/// 将序列化后的 CimE 内容写入文件或流。
/// </summary>
public static class EWriter
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static void Write(EFile file, string path, CimEFormat format)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("文件路径不能为空。", nameof(path));
        }

        var content = CimESerializer.Serialize(file, format);
        File.WriteAllBytes(path, Utf8NoBom.GetBytes(content));
    }

    public static void Write(EFile file, Stream stream, CimEFormat format)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite)
        {
            throw new ArgumentException("目标流不可写。", nameof(stream));
        }

        var content = CimESerializer.Serialize(file, format);
        var bytes = Utf8NoBom.GetBytes(content);
        stream.Write(bytes, 0, bytes.Length);
    }
}
