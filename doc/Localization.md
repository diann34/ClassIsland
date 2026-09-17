# 本地化

ClassIsland 的 UI 本地化使用资源 ID，不在运行时对任意用户文本做简繁转换。

## 资源目录

每个包含 UI 的程序集在 `Assets/Localization` 下维护标准 `.resx` 资源。资源文件按照使用它们的代码位置拆分，例如：

- `Views/ProfileSettingsWindow.resx`：档案编辑窗口的简体中文回退资源；
- `Views/ProfileSettingsWindow.zh-Hant.resx`：档案编辑窗口的繁体中文资源；
- `Views/SettingPages/GeneralSettingsPage.resx`：常规设置页的简体中文回退资源；
- `Controls/TimeRuleEditControl.zh-Hant.resx`：时间规则控件的繁体中文资源。

中性文件（不带文化名称的 `.resx`）是简体中文回退资源。以后新增语言时，为每个中性文件添加同路径、同名的 `.<culture>.resx` 文件，翻译资源值并保留全部资源 ID，然后在 `LocalizationService` 的 `RegisteredLanguages` 注册表中显式注册该语言。

资源文件由程序集清单统一加载，因此拆分或新增资源文件时不需要修改调用代码。语言仍然只通过 `RegisteredLanguages` 显式注册，不会根据现有资源文件动态出现在语言列表中。

资源项结构固定为：

```xml
<data name="SettingPages.GeneralSettingsPage.Header.StartOnBoot" xml:space="preserve">
  <value>開機自動啟動</value>
</data>
```

## AXAML 用法

静态 UI 文案通过 `ci:Tr` 引用资源 ID：

```xml
<TextBlock Text="{ci:Tr SettingPages.GeneralSettingsPage.Header.StartOnBoot}" />
```

资源 ID 使用点号分隔的 PascalCase 路径，通常为 `区域.页面或控件.属性.语义名称`。例如：
`SettingPages.GeneralSettingsPage.Header.StartOnBoot`。

不要给绑定值、课表/档案数据、日志内容或用户输入添加本地化标记。C# 创建的静态对话框文本可使用 `LocalizationService.TranslateText`；包含动态参数的文本应拆分为资源模板并显式格式化。

语言设置保存在全局存储中，修改后重启应用生效。`auto` 会根据系统文化选择最接近的已注册语言，找不到时回退到 `zh-Hans`。仅添加卫星 `.resx` 不会让语言出现在选择列表中。
