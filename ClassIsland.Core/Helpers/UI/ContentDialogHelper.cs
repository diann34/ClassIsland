using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ClassIsland.Core.Extensions.UI;
using ClassIsland.Core.Services;
using FluentAvalonia.UI.Controls;
using ShimSkiaSharp;

namespace ClassIsland.Core.Helpers.UI;

/// <summary>
/// <see cref="FAContentDialog"/> 辅助类
/// </summary>
public static class ContentDialogHelper
{
    /// <summary>
    /// 显示确认提示框
    /// </summary>
    /// <param name="title">提示框标题</param>
    /// <param name="body">内容</param>
    /// <param name="confirmation">确认文字，留空禁用</param>
    /// <param name="root">视觉根</param>
    /// <param name="positiveText">确认按钮文字</param>
    /// <param name="negativeText">取消按钮文字</param>
    /// <returns>是否通过验证</returns>
    public static async Task<bool> ShowConfirmationDialog(string title, string body, string? confirmation = null, TopLevel? root = null, string positiveText = "确认", string negativeText = "取消")
    {
        var enablesConfirmation = !string.IsNullOrWhiteSpace(confirmation);
        var textBox = new TextBox();
        var stackPanel = new StackPanel()
        {
            Spacing = 6,
            Children =
            {
                new TextBlock()
                {
                    Text = LocalizationService.TranslateText(body),
                    TextWrapping = TextWrapping.Wrap
                }
            }
        };
        if (enablesConfirmation)
        {
            stackPanel.Children.Add(textBox);
        }

        var dialog = new FAContentDialog()
        {
            Title = LocalizationService.TranslateText(title),
            Content = stackPanel,
            DefaultButton = FAContentDialogButton.Primary,
            PrimaryButtonText = LocalizationService.TranslateText(positiveText),
            CloseButtonText = LocalizationService.TranslateText(negativeText)
        };
        var r = await dialog.ShowAsyncAuto(root);
        if (r != FAContentDialogResult.Primary)
        {
            return false;
        }

        if (!enablesConfirmation) 
            return true;
        if (textBox.Text == confirmation) 
            return true;
        root?.ShowWarningToast(LocalizationService.Translate(
            "Core.Helpers.UI.ContentDialogHelper.Message.IncorrectVerificationResult"));
        return false;

    }
}
