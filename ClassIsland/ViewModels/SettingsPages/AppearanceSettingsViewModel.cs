using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Media;
using ClassIsland.Core;
using ClassIsland.Core.Services;
using ClassIsland.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClassIsland.ViewModels.SettingsPages;

public partial class AppearanceSettingsViewModel(SettingsService settingsService) : ObservableRecipient
{
    [ObservableProperty] private string _fontSizeTestText = LocalizationService.Translate(
        "SettingPages.AppearanceSettingsPage.Text.FontPreview");
    
    public ObservableCollection<FontFamily> FontFamilies { get; } =
        new([..FontManager.Current.SystemFonts, MainWindow.DefaultFontFamily]);
    
    public SettingsService SettingsService { get; } = settingsService;
}
