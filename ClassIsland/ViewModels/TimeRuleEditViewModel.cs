using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using ClassIsland.Core.Services;

namespace ClassIsland.ViewModels;

public partial class TimeRuleEditViewModel : ObservableObject
{
    public static IReadOnlyList<string> WeekDayOptions { get; } =
    [
        LocalizationService.Translate("ViewModels.TimeRuleEditViewModel.Weekday.Sunday"),
        LocalizationService.Translate("ViewModels.TimeRuleEditViewModel.Weekday.Monday"),
        LocalizationService.Translate("ViewModels.TimeRuleEditViewModel.Weekday.Tuesday"),
        LocalizationService.Translate("ViewModels.TimeRuleEditViewModel.Weekday.Wednesday"),
        LocalizationService.Translate("ViewModels.TimeRuleEditViewModel.Weekday.Thursday"),
        LocalizationService.Translate("ViewModels.TimeRuleEditViewModel.Weekday.Friday"),
        LocalizationService.Translate("ViewModels.TimeRuleEditViewModel.Weekday.Saturday")
    ];

    public List<string> WeekCountDivOptions { get; set; } = [];
    
    public List<string> WeekCountDivTotalOptions { get; set; } = [];
    
    [ObservableProperty]
    private int _weekCountDivIndex;
    
    [ObservableProperty]
    private int _weekCountDivTotalIndex;

    [ObservableProperty] private DateTime _newDateTime = DateTime.Today;
}
