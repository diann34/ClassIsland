using Avalonia;
using Avalonia.Controls;
using ClassIsland.Core.Services;
using FluentAvalonia.UI.Controls;

namespace ClassIsland.Core.Controls;

/// <summary>
/// 一些常用的 FATaskDialog 
/// </summary>
public static class CommonTaskDialogs
{
    /// <summary>
    /// 显示基本提示框
    /// </summary>
    /// <param name="header">对话框头</param>
    /// <param name="content">要显示的内容</param>
    /// <param name="xamlRoot">XAML 根元素</param>
    public static async Task<object?> ShowDialog(string header, string content, Visual? xamlRoot = null)
    {
        var dialog = new FATaskDialog()
        {
            Content = LocalizationService.TranslateText(content),
            Header = LocalizationService.TranslateText(header),
            Buttons =
            {
                new FATaskDialogButton(LocalizationService.TranslateText("确定"), true)
                {
                    IsDefault = true,
                }
            },
            XamlRoot = xamlRoot ?? AppBase.Current.GetRootWindow()
        };
        
        return await dialog.ShowAsync();
    }
}
