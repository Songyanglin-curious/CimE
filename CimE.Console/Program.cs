using CimE;

namespace CimE.Console;

internal static class Program
{
    private static void Main(string[] args)
    {
        var outputPath = args.Length > 0 ? args[0] : "example.cime";
        var file = BuildSettingsFile();
        EWriter.Write(file, outputPath, CimEFormat.OnLineMonitor);
        System.Console.WriteLine($"已写入 {outputPath}");
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
