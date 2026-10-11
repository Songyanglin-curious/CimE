namespace CimE;

/// <summary>
/// CimE 第一阶段支持的两种输出格式。
/// </summary>
public enum CimEFormat
{
    /// <summary>
    /// GB/T 30149-2013：系统声明 + E 根元素，数据块使用 @/@# 和 #。
    /// </summary>
    Standard2013,

    /// <summary>
    /// GB/T 30149-2019：XML 声明 + XML 属性，数据块使用 &lt;@&gt;/&lt;@#&gt; 和 &lt;#&gt;。
    /// </summary>
    Standard2019,

    /// <summary>
    /// OnLineMonitor 工程样例格式。
    /// </summary>
    OnLineMonitor

}
