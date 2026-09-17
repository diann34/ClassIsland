using System;
using System.IO;
using System.Net.Mime;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Platform;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;
using ClassIsland.Core.Services;
using ClassIsland.Enums.AppUpdating;
using ClassIsland.Services.AppUpdating;
using ClassIsland.Shared;
using ClassIsland.Shared.Enums;
using ClassIsland.ViewModels.SettingsPages;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace ClassIsland.Views.SettingPages;

[SettingsPageInfo("update", "更新", "\ue161", "\ue160", SettingsPageCategory.Internal)]
public partial class UpdateSettingsPage : SettingsPageBase
{
    private IDisposable? _updateSettingsObserver;
    private IDisposable? _newVersionChangeLogObserver;
    
    public UpdateSettingsPageViewModel ViewModel { get; } = IAppHost.GetService<UpdateSettingsPageViewModel>();

    public static readonly FuncValueConverter<UpdateStatus, string> UpdateStatusToIconGlyphConverter =
        new(x => x switch
        {
            UpdateStatus.UpToDate => "\ue1a1",
            UpdateStatus.UpdateAvailable => "\ue161",
            UpdateStatus.UpdateDownloaded => "\ue0d3",
            UpdateStatus.UpdateDeployed => "\ue163",
            _ => ""
        });
    
    public static readonly FuncValueConverter<UpdateStatus, string> UpdateStatusToMessageConverter =
        new(x => x switch
        {
            UpdateStatus.UpToDate => LocalizationService.Translate("SettingPages.UpdateSettingsPage.Status.UpToDate"),
            UpdateStatus.UpdateAvailable => LocalizationService.Translate("SettingPages.UpdateSettingsPage.Status.UpdateAvailable"),
            UpdateStatus.UpdateDownloaded => LocalizationService.Translate("SettingPages.UpdateSettingsPage.Status.UpdateDownloaded"),
            UpdateStatus.UpdateDeployed => LocalizationService.Translate("SettingPages.UpdateSettingsPage.Status.UpdateDeployed"),
            _ => ""
        });
    
    public static readonly FuncValueConverter<UpdateWorkingStatus, string> UpdateWorkingStatusToMessageConverter =
        new(x => x switch
        {
            UpdateWorkingStatus.Idle => LocalizationService.Translate("SettingPages.UpdateSettingsPage.WorkingStatus.Idle"),
            UpdateWorkingStatus.CheckingUpdates => LocalizationService.Translate("SettingPages.UpdateSettingsPage.WorkingStatus.CheckingUpdates"),
            UpdateWorkingStatus.DownloadingUpdates => LocalizationService.Translate("SettingPages.UpdateSettingsPage.WorkingStatus.DownloadingUpdates"),
            UpdateWorkingStatus.ExtractingUpdates => LocalizationService.Translate("SettingPages.UpdateSettingsPage.WorkingStatus.ExtractingUpdates"),
            _ => "???"
        });

    public static readonly FuncValueConverter<DownloadState, string> DownloadStateToMessageConverter =
        new(x => x switch
        {
            DownloadState.Pending => LocalizationService.Translate("SettingPages.UpdateSettingsPage.DownloadStatus.Pending"),
            DownloadState.Downloading => LocalizationService.Translate("SettingPages.UpdateSettingsPage.DownloadStatus.Downloading"),
            DownloadState.Completed => LocalizationService.Translate("SettingPages.UpdateSettingsPage.DownloadStatus.Completed"),
            DownloadState.Error => LocalizationService.Translate("SettingPages.UpdateSettingsPage.DownloadStatus.Error"),
            _ => "???"
        });
    
    public UpdateSettingsPage()
    {
        DataContext = this;
        InitializeComponent();
    }
    
    private async void ButtonCheckUpdate_OnClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.UpdateService.CheckUpdateAsync();
    }

    private async void ButtonDownloadUpdate_OnClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.UpdateService.DownloadUpdateAsync();
        if (ViewModel.SettingsService.Settings.LastUpdateStatus == UpdateStatus.UpdateDownloaded)
        {
            await ViewModel.UpdateService.ExtractUpdateAsync();
        }
    }

    private void UpdateChannelInfo()
    {
        ViewModel.SelectedChannel =
            ViewModel.UpdateService.DistributionMetadata.Channels.TryGetValue(
                ViewModel.SettingsService.Settings.SelectedUpdateChannelV3, out var v1)
                ? v1
                : ViewModel.SelectedChannel;
    }

    private void UpdateNewVersionChangeLog()
    {
        ViewModel.NewVersionChangeLogDocument =
            ViewModel.SettingsService.Settings.LastUpdateStatus != UpdateStatus.UpToDate
                ? ViewModel.UpdateService.DistributionInfo.ChangeLog
                : "";
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        UpdateChannelInfo();
        UpdateNewVersionChangeLog();
        _updateSettingsObserver ??= ViewModel.SettingsService.Settings
            .ObservableForProperty(x => x.SelectedUpdateChannelV3)
            .Subscribe(_ => UpdateChannelInfo());
        _newVersionChangeLogObserver ??= ViewModel.UpdateService
            .WhenAnyPropertyChanged()
            .Subscribe(_ => UpdateNewVersionChangeLog());
        
    }


    private void Control_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        _updateSettingsObserver?.Dispose();
        _updateSettingsObserver = null;
        _newVersionChangeLogObserver?.Dispose();
        _newVersionChangeLogObserver = null;
    }

    private void ButtonOpenDownloadTasks_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenDrawer("DownloadInfoDrawer");
    }

    private async void ButtonCancelDownload_OnClick(object? sender, RoutedEventArgs e)
    {
        await ViewModel.UpdateService.StopDownloading();
    }

    private async void ButtonDeployUpdate_OnClick(object? sender, RoutedEventArgs e)
    {
        await ViewModel.UpdateService.ExtractUpdateAsync();
    }

    private void InfoBarError_OnCloseButtonClick(FAInfoBar sender, EventArgs args)
    {
        ViewModel.UpdateService.NetworkErrorException = null;
    }
    
    private void InfoBarDeployError_OnCloseButtonClick(FAInfoBar sender, EventArgs args)
    {
        ViewModel.UpdateService.DeployErrorException = null;
    }

    private void ButtonRestart_OnClick(object? sender, RoutedEventArgs e)
    {
        AppBase.Current.Restart(["-m"], true);
    }

    private async void SettingsExpanderItemCheckUpdateForce_OnClick(object? sender, RoutedEventArgs e)
    {
        await ViewModel.UpdateService.CheckUpdateAsync(true);
    }

    private void ButtonShowChangeLogs_OnClick(object? sender, RoutedEventArgs e)
    {
        using var sr =
            new StreamReader(AssetLoader.Open(new Uri("avares://ClassIsland/Assets/Documents/ChangeLog.md")));
        ViewModel.ChangeLogDocument = sr.ReadToEnd();
        OpenDrawer("ChangeLogDrawer");
    }

    private async void MenuItemDownloadOnly_OnClick(object? sender, RoutedEventArgs e)
    {
        await ViewModel.UpdateService.DownloadUpdateAsync();
    }
}
