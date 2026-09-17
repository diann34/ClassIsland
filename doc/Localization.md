# 本地化

ClassIsland 的 UI 本地化使用资源 ID，不在运行时对任意用户文本做简繁转换。

## 资源目录

每个包含 UI 的程序集在 `Assets/Localization` 下维护标准 `.resx` 资源：

- `Localization.resx` 是简体中文回退资源；
- `Localization.zh-Hant.resx` 是繁体中文资源；
- 以后新增语言时，复制回退资源为 `Localization.<culture>.resx`，翻译资源值并保留全部资源 ID，然后在 `LocalizationService` 的 `RegisteredLanguages` 注册表中显式注册该语言。

目录结构固定为：

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
