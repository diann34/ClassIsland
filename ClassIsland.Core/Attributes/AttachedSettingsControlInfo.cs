using System.Diagnostics.CodeAnalysis;
using ClassIsland.Core.Enums;
using MaterialDesignThemes.Wpf;

namespace ClassIsland.Core.Attributes;

/// <summary>
/// 附加设置信息
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AttachedSettingsControlInfo(
    string guid,
    string name,
    PackIconKind iconKind = PackIconKind.CogOutline,
    bool hasEnabledState = true) : Attribute
{
    /// <summary>
    /// 附加设置 GUID
    /// </summary>
    public Guid Guid { get; } = new(guid);

    /// <summary>
    /// 附加设置控件名称
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 附加设置图标类型
    /// </summary>
    public PackIconKind IconKind { get; } = iconKind;

    /// <summary>
    /// 是否具有开关状态
    /// </summary>
    public bool HasEnabledState { get; } = hasEnabledState;

    /// <summary>
    /// 附加设置控件类型
    /// </summary>
    public Type AttachedSettingsControlType { get; internal set; } = null!;

    /// <summary>
    /// 附加设置可以附加的目标。
    /// </summary>
    public AttachedSettingsTargets Targets { get; internal set; } = AttachedSettingsTargets.None;
}