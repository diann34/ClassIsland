using Avalonia.Controls;
using Avalonia.Interactivity;
using ClassIsland.Core;
using ClassIsland.Core.Helpers.UI;
using ClassIsland.Core.Services;
using ClassIsland.Services;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ClassIsland.Platforms.Abstraction;
using Path = System.IO.Path;

namespace ClassIsland.Views.RecoveryPages;

/// <summary>
/// HomePage.xaml 的交互逻辑
/// </summary>
public partial class HomePage : UserControl
{
    public FAFrame? MainFrame { get; init; }

    public HomePage()
    {
        InitializeComponent();
        if (AppBase.Current.PackagingType is "folder" or "folderClassic")
        {
            List<string> validInstallations;
            try
            {
                validInstallations = Directory.GetDirectories(CommonDirectories.AppPackageRoot)
                    .Where(dir =>
                        Path.GetFileName(dir).StartsWith("app", StringComparison.OrdinalIgnoreCase) &&
                        !File.Exists(Path.Combine(dir, ".destroy")) &&
                        !File.Exists(Path.Combine(dir, ".partial")) &&
                        File.Exists(Path.Combine(dir, "ClassIsland.Desktop" + AppBase.PlatformExecutableExtension)))
                    .ToList();
            }
            catch (Exception ex)
            {
                this.ShowErrorToast(LocalizationService.Translate(
                    "Views.RecoveryPages.HomePage.Message.InstallationDetectionFailed",
                    ex.Message));
                validInstallations = new List<string>();
            }

            ButtonRollBack.IsEnabled = validInstallations.Count > 1;
        }
    }

    private void ButtonContinue_OnClick(object sender, RoutedEventArgs e)
    {
        AppBase.Current.Restart(["-m"]);
    }

    private void ButtonContinueSafe_OnClick(object sender, RoutedEventArgs e)
    {
        AppBase.Current.Restart(["-m", "--safe"]);
    }

    private void ButtonContinueDiagnostic_OnClick(object sender, RoutedEventArgs e)
    {
        AppBase.Current.Restart(["-m", "--diagnostic", "--verbose"]);
    }

    private void ButtonOpenLogFolder_OnClick(object sender, RoutedEventArgs e)
    {
        PlatformServices.LauncherService.LaunchPath(CommonDirectories.AppLogFolderPath);
    }

    private async void ButtonCleanTempFiles_OnClick(object sender, RoutedEventArgs e)
    {
        var result = await ContentDialogHelper.ShowConfirmationDialog(
            LocalizationService.Translate("Views.RecoveryPages.HomePage.Title.ClearTemporaryFiles"),
            LocalizationService.Translate("Views.RecoveryPages.HomePage.Content.ConfirmClearTemporaryFiles"),
            root: TopLevel.GetTopLevel(this));
        if (!result)
        {
            return;
        }

        try
        {
            if (Directory.Exists(CommonDirectories.AppTempFolderPath))
            {
                Directory.Delete(CommonDirectories.AppTempFolderPath, true);
            }
            if (Directory.Exists(CommonDirectories.AppCacheFolderPath))
            {
                Directory.Delete(CommonDirectories.AppCacheFolderPath, true);
            }
            this.ShowSuccessToast($"操作成功完成。");
        }
        catch (Exception exception)
        {
            this.ShowErrorToast("无法清除临时文件", exception);
        }
    }

    private async void ButtonResetSettings_OnClick(object sender, RoutedEventArgs e)
    {
        var result = await ContentDialogHelper.ShowConfirmationDialog(
            LocalizationService.Translate("Views.RecoveryPages.HomePage.Title.ResetApplicationSettings"),
            LocalizationService.Translate("Views.RecoveryPages.HomePage.Content.ConfirmResetApplicationSettings"),
            LocalizationService.Translate("Views.RecoveryPages.HomePage.Verification.ResetApplicationSettings"),
            root: TopLevel.GetTopLevel(this));
        if (!result)
        {
            return;
        }

        try
        {
            if (File.Exists(Path.Combine(CommonDirectories.AppRootFolderPath, "Settings.json")))
            {
                File.Delete(Path.Combine(CommonDirectories.AppRootFolderPath, "Settings.json"));
            }
            if (File.Exists(Path.Combine(CommonDirectories.AppRootFolderPath, "Settings.json.bak")))
            {
                File.Delete(Path.Combine(CommonDirectories.AppRootFolderPath, "Settings.json.bak"));
            }
            this.ShowSuccessToast($"操作成功完成。");
        }
        catch (Exception exception)
        {
            this.ShowErrorToast("无法重置应用设置", exception);
        }
    }

    private async void ButtonResetConfigs_OnClick(object sender, RoutedEventArgs e)
    {
        var result = await ContentDialogHelper.ShowConfirmationDialog(
            LocalizationService.Translate("Views.RecoveryPages.HomePage.Title.ResetAllConfiguration"),
            LocalizationService.Translate("Views.RecoveryPages.HomePage.Content.ConfirmResetAllConfiguration"),
            LocalizationService.Translate("Views.RecoveryPages.HomePage.Verification.ResetAllConfiguration"),
            root: TopLevel.GetTopLevel(this));
        if (!result)
        {
            return;
        }

        try
        {
            if (File.Exists(Path.Combine(CommonDirectories.AppRootFolderPath, "Settings.json")))
            {
                File.Delete(Path.Combine(CommonDirectories.AppRootFolderPath, "Settings.json"));
            }
            if (File.Exists(Path.Combine(CommonDirectories.AppRootFolderPath, "Settings.json.bak")))
            {
                File.Delete(Path.Combine(CommonDirectories.AppRootFolderPath, "Settings.json.bak"));
            }
            if (Directory.Exists(CommonDirectories.AppConfigPath))
            {
                Directory.Delete(CommonDirectories.AppConfigPath, true);
            }
            
            this.ShowSuccessToast($"操作成功完成。");
        }
        catch (Exception exception)
        {
            this.ShowErrorToast("无法重置应用设置", exception);
        }
    }

    private async void ButtonResetAll_OnClick(object sender, RoutedEventArgs e)
    {
        var result = await ContentDialogHelper.ShowConfirmationDialog(
            LocalizationService.Translate("Views.RecoveryPages.HomePage.Title.ResetAllData"),
            LocalizationService.Translate("Views.RecoveryPages.HomePage.Content.ConfirmResetAllData"),
            LocalizationService.Translate("Views.RecoveryPages.HomePage.Verification.ResetAllData"),
            root: TopLevel.GetTopLevel(this));
        if (!result)
        {
            return;
        }

        try
        {
            if (File.Exists(Path.Combine(CommonDirectories.AppRootFolderPath, "Settings.json")))
            {
                File.Delete(Path.Combine(CommonDirectories.AppRootFolderPath, "Settings.json"));
            }
            if (File.Exists(Path.Combine(CommonDirectories.AppRootFolderPath, "Settings.json.bak")))
            {
                File.Delete(Path.Combine(CommonDirectories.AppRootFolderPath, "Settings.json.bak"));
            }
            if (Directory.Exists(CommonDirectories.AppConfigPath))
            {
                Directory.Delete(CommonDirectories.AppConfigPath, true);
            }
            if (Directory.Exists(Path.Combine(CommonDirectories.AppRootFolderPath, "Profiles")))
            {
                Directory.Delete(Path.Combine(CommonDirectories.AppRootFolderPath, "Profiles"), true);
            }
            if (Directory.Exists(PluginService.PluginsRootPath))
            {
                Directory.Delete(PluginService.PluginsRootPath, true);
            }

            this.ShowSuccessToast($"操作成功完成。");
        }
        catch (Exception exception)
        {
            this.ShowErrorToast("无法重置应用设置", exception);
        }
    }

    private void ButtonRecoverBackup_OnClick(object sender, RoutedEventArgs e)
    {
        if (MainFrame != null)
        {
            MainFrame.Content = new RecoverBackupPage()
            {
                MainFrame = MainFrame,
                LastPage = this
            };
        }
    }

    private void ButtonRollBack_Onclick(object sender, RoutedEventArgs e)
    {
        //TODO:实现回滚功能(版本管理器)
        this.ShowToast("此功能仍在开发中");
    }
}
