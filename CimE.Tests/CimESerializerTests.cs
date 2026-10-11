using System.Text;
using CimE;
using Xunit;

namespace CimE.Tests;

public sealed class CimESerializerTests
{
    private static readonly string SamplePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "test-data", "OnLineMonitor-settings.CIME");

    [Fact]
    public void OnLineMonitorSettingsMatchesTheReferenceByteForByte()
    {
        var actual = CimESerializer.Serialize(BuildSettingsFile(), CimEFormat.OnLineMonitor);
        var actualBytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(actual);
        var expectedBytes = File.ReadAllBytes(SamplePath);
        var expected = Encoding.UTF8.GetString(expectedBytes);

        Assert.Equal(expected, actual);
        Assert.Equal(expectedBytes, actualBytes);
        Assert.Equal(31, actual.Split("# ", StringSplitOptions.None).Length - 1);
        Assert.Contains("0.2200", actual);
        Assert.Contains("57.7400", actual);
        Assert.Contains(" 200 -", actual);
        Assert.Contains("'1#CT 额定一次值'", actual);
        Assert.Contains("Time='2026-06-17 15:39:40'!>", actual);
    }

    [Fact]
    public void Standard2013WritesSystemDeclarationPlainDataBlockAndNull()
    {
        var file = new EFile("E")
            .SetAttribute("Version", "1.0")
            .SetAttribute("Type", "FullModel");
        file.AddDataSet("类名")
            .SetAttribute("entity", "demo")
            .AddColumn("ID")
            .AddColumn("Name")
            .AddRow(1, null);

        var actual = CimESerializer.Serialize(file, CimEFormat.Standard2013);

        Assert.Equal(
            "<!Version=1.0 Type=FullModel !>\r\n" +
            "<E>\r\n" +
            "<类名 entity='demo'>\r\n" +
            "@ ID Name\r\n" +
            "# 1 NULL\r\n" +
            "</类名>\r\n" +
            "</E>", actual);
    }

    [Fact]
    public void Standard2019WritesXmlDeclarationAndAngleBracketDataMarkers()
    {
        var file = new EFile("E")
            .SetAttribute("Version", "1.0")
            .SetAttribute("Type", "FullModel")
            .SetAttribute("T_Head", "@")
            .SetAttribute("V_Head", "@#")
            .SetAttribute("Object", "#")
            .SetAttribute("Time", new DateTime(2026, 8, 31, 10, 20, 30));
        file.AddDescription("FullModel")
            .SetAttribute("ID", "1")
            .SetAttribute("Profile", "demo");
        file.AddDataSet("类名")
            .SetAttribute("entity", "demo")
            .AddColumn("ID", "序号")
            .AddColumn("Name", "名称")
            .AddRow(1, null);

        var actual = CimESerializer.Serialize(file, CimEFormat.Standard2019);

        Assert.Equal(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
            "<E Version=\"1.0\" Type=\"FullModel\" T_Head=\"@\" V_Head=\"@#\" Object=\"#\" Time=\"2026-08-31 10:20:30\">\r\n" +
            "<FullModel ID=\"1\" Profile=\"demo\"/>\r\n" +
            "<类名 entity=\"demo\">\r\n" +
            "<@> ID Name </@>\r\n" +
            "<!-- 序号 名称 -->\r\n" +
            "<#> 1 NULL </#>\r\n" +
            "</类名>\r\n" +
            "</E>", actual);
    }

    [Fact]
    public void InvalidRowsAndTokensAreReportedWithContext()
    {
        var file = new EFile("E");
        file.AddDataSet("参数")
            .AddColumn("Value")
            .AddRow(EValue.Token("bad token"));

        var exception = Assert.Throws<CimEValidationException>(() => CimESerializer.Serialize(file, CimEFormat.Standard2013));

        Assert.Contains("节点 '参数' 第 1 行列 'Value'", exception.Message);
        Assert.Contains("Token", exception.Message);
    }

    [Fact]
    public void WriterDoesNotReplaceAnExistingFileWhenValidationFails()
    {
        var path = Path.Combine(Path.GetTempPath(), $"cime-invalid-{Guid.NewGuid():N}.e");
        File.WriteAllText(path, "original", Encoding.UTF8);

        try
        {
            var file = new EFile("E");
            file.AddDataSet("参数")
                .AddColumn("A")
                .AddColumn("A")
                .AddRow(1, 2);

            Assert.Throws<CimEValidationException>(() => EWriter.Write(file, path, CimEFormat.Standard2013));
            Assert.Equal("original", File.ReadAllText(path, Encoding.UTF8));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static EFile BuildSettingsFile()
    {
        var file = new EFile("System")
            .SetValue("OnLineMonitor")
            .SetAttribute("Version", "V1.0")
            .SetAttribute("Code", "UTF-8")
            .SetAttribute("Data", "1.0")
            .SetAttribute("Module", "Infos")
            .SetAttribute("Type", "Settings")
            .SetAttribute("Time", new DateTime(2026, 6, 17, 15, 39, 40));

        file.AddDescription("变电站")
            .SetAttribute("Name", "STATION")
            .SetAttribute("Desc", "Substation")
            .SetAttribute("VoltageLevel", "Unknown");
        file.AddDescription("装置")
            .SetAttribute("Name", "CT2201")
            .SetAttribute("Desc", "1 号主变中压侧测控");

        var dataSet = file.AddDataSet("装置参数")
            .SetAttribute("UpdateTime", new DateTime(2026, 6, 16, 15, 40, 32))
            .AddColumn("ID", "序号")
            .AddColumn("DataRef", "数据引用")
            .AddColumn("ParamName", "参数名称")
            .AddColumn("GrpNum", "参数组序号")
            .AddColumn("Value", "值")
            .AddColumn("Unit", "单位");

        var rows = new (string DataRef, string ParamName, decimal Value, string ValueFormat, object Unit)[]
        {
            ("CT2201MEAS/LLN0.StrVal1[SP]", "电流电压变化死区", 0.22m, "0.0000", EValue.Token("-")),
            ("CT2201CTRL/LLN0.Phase[SP]", "同期抽取电压", 1m, "0", EValue.Token("-")),
            ("CT2201MEAS/LLN0.StrVal2[SP]", "电流电压归零死区", 0.20m, "0.0000", EValue.Token("-")),
            ("CT2201CTRL/LLN0.LinVRtgSnd[SP]", "测量侧额定电压", 57.74m, "0.0000", EValue.Token("V")),
            ("CT2201CTRL/LLN0.SynVRtgSnd[SP]", "抽取侧额定电压", 57.74m, "0.0000", EValue.Token("V")),
            ("CT2201MEAS/LLN0.StrVal3[SP]", "功率变化死区", 0.53m, "0.0000", EValue.Token("-")),
            ("CT2201MEAS/LLN0.StrVal4[SP]", "功率归零死区", 0.51m, "0.0000", EValue.Token("-")),
            ("CT2201CTRL/LLN0.LivVal[SP]", "同期有压定值", 34.64m, "0.0000", EValue.Token("V")),
            ("CT2201CTRL/LLN0.DeaVal[SP]", "同期无压定值", 17.32m, "0.0000", EValue.Token("V")),
            ("CT2201MEAS/LLN0.StrVal5[SP]", "功率因数变化死区", 0.005m, "0.0000", EValue.Token("-")),
            ("CT2201MEAS/LLN0.StrVal6[SP]", "频率变化死区", 0.005m, "0.0000", EValue.Token("Hz")),
            ("CT2201CTRL/LLN0.RteHz[SP]", "滑差定值", 1.0000m, "0.0000", EValue.Token("Hz/s")),
            ("CT2201CTRL/LLN0.DifHz[SP]", "频差定值", 0.5000m, "0.0000", EValue.Token("Hz")),
            ("CT2201MEAS/TVTR1.VRtg[SP]", "PT 额定一次值", 110.0000m, "0.0000", EValue.Token("kV")),
            ("CT2201CTRL/LLN0.DifV[SP]", "压差定值", 10.0000m, "0.0000", EValue.Token("V")),
            ("CT2201MEAS/TVTR1.VRtgSnd[SP]", "PT 额定二次值", 100.0000m, "0.0000", EValue.Token("V")),
            ("CT2201MEAS/TVTR2.VRtg[SP]", "同期侧PT 额定一次值", 110.0000m, "0.0000", EValue.Token("kV")),
            ("CT2201CTRL/LLN0.DifAng[SP]", "角差定值", 15.0000m, "0.0000", EValue.Token("deg")),
            ("CT2201CTRL/LLN0.BkrTmms[SP]", "导前时间", 200m, "0", EValue.Token("-")),
            ("CT2201MEAS/TVTR2.VRtgSnd[SP]", "同期侧PT 额定二次值", 100.0000m, "0.0000", EValue.Token("V")),
            ("CT2201MEAS/TVTR3.VRtg[SP]", "零序PT 额定一次值", 110.0000m, "0.0000", EValue.Token("kV")),
            ("CT2201CTRL/LLN0.AngOfs[SP]", "固有相角差", 0.0000m, "0.0000", EValue.Token("deg")),
            ("CT2201MEAS/TVTR3.VRtgSnd[SP]", "零序PT 额定二次值", 100.0000m, "0.0000", EValue.Token("V")),
            ("CT2201CTRL/LLN0.PTFBlkEna[SP]", "PT 断线闭锁使能", 1m, "0", EValue.Token("-")),
            ("CT2201CTRL/LLN0.SynRsTmms[SP]", "同期复归时间", 40m, "0", EValue.Token("-")),
            ("CT2201MEAS/TCTR1.ARtg[SP]", "1#CT 额定一次值", 1000.0000m, "0.0000", EValue.Token("A")),
            ("CT2201MEAS/TCTR1.ARtgSnd[SP]", "1#CT 额定二次值", 1.0000m, "0.0000", EValue.Token("A")),
            ("CT2201MEAS/TCTR2.ARtg[SP]", "零序CT 额定一次值", 1000.0000m, "0.0000", EValue.Token("A")),
            ("CT2201MEAS/TCTR2.ARtgSnd[SP]", "零序CT 额定二次值", 1.0000m, "0.0000", EValue.Token("A")),
            ("CT2201MEAS/TCTR3.ARtg[SP]", "2#CT 额定一次值", 1000.0000m, "0.0000", EValue.Token("A")),
            ("CT2201MEAS/TCTR3.ARtgSnd[SP]", "2#CT 额定二次值", 1.0000m, "0.0000", EValue.Token("A"))
        };

        for (var index = 0; index < rows.Length; index++)
        {
            var row = rows[index];
            dataSet.AddRow(index + 1, row.DataRef, row.ParamName, 1,
                EValue.Number(row.Value, row.ValueFormat), row.Unit);
        }

        return file;
    }
}
