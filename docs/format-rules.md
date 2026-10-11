# CimE 第一阶段格式规则

本阶段只支持代码构造和写出，不包含读取、解析、反序列化、完整 CIM 模型或 Schema。

## 对象模型

- `EFile` 保存根标签、根标签值、有序属性和有序子节点。
- `EDescriptionNode` 输出属性节点自闭合标签。
- `EDataSetNode` 保存有序列定义和有序数据行。
- `EValue` 用于显式表示 `NULL`、Token、固定小数位数字、指定格式日期时间和字符串。

## 输出规则

- `Standard2013` 对应 GB/T 30149-2013：可选系统声明、`<E>...</E>` 根元素，横表式使用 `@`，纵表式使用 `@#`，数据行使用 `#`。
- `Standard2019` 对应 GB/T 30149-2019：输出 XML 声明、`<E Version="..." Type="...">` 根元素，属性使用 XML 风格引号，横表式数据块使用 `<@> ... </@>` 和 `<#> ... </#>`；纵表式标记由规范定义为 `<@#>`，第一阶段对象模型暂不单独建模纵表式。
- `OnLineMonitor` 是工程特定格式，不等同于规范版本：根头形如 `<!System=OnLineMonitor ...!>`，数据块使用 `@`、`//` 和 `#`。
- 节点、属性、列和行均按添加顺序输出。
- 2013 和 OnLineMonitor 普通字符串优先使用单引号，2019 优先使用双引号；只含一种引号时允许切换，包含单双引号时报告校验错误。
- 普通空值输出 `NULL`；`EValue.Token("-")` 输出不带引号的 `-`。
- 数字使用 `InvariantCulture`，默认日期时间格式为 `yyyy-MM-dd HH:mm:ss`。
- `CimESerializer.Serialize` 返回不带末尾换行的 CRLF 文本；`EWriter` 写入 UTF-8 无 BOM，并追加一个 CRLF。

## 校验

序列化前检查根节点和节点名称、属性重复、数据集列定义、列名重复、行列数、Token、控制字符、引号冲突以及数字和日期格式。错误统一抛出 `CimEValidationException`，消息包含节点、行号或列名等上下文。

样例文件见 [`test-data/OnLineMonitor-settings.e`](../test-data/OnLineMonitor-settings.e)。
