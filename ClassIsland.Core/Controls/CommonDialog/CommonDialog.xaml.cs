﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using ClassIsland.Core.Abstractions.Services.Management;
using ClassIsland.Core.Enums;
using ClassIsland.Core.Helpers.Native;
using ClassIsland.Shared;
using MaterialDesignThemes.Wpf;

namespace ClassIsland.Core.Controls.CommonDialog;

/// <summary>
/// CommonDialog.xaml 的交互逻辑
/// </summary>
public partial class CommonDialog : MyWindow, INotifyPropertyChanged
{
    private string _dialogContent = "";
    private Visual? _dialogIcon;
    private ObservableCollection<DialogAction> _actions = new();
    private int _executedActionIndex = -1;
    private bool _hasInput = false;
    private string _inputResult = "";

    public static ICommand ActionCommand { get; } = new RoutedCommand();

    public string DialogContent
    {
        get => _dialogContent;
        set
        {
            if (value == _dialogContent) return;
            _dialogContent = value;
            OnPropertyChanged();
        }
    }


    public Visual? DialogIcon
    {
        get => _dialogIcon;
        set
        {
            if (Equals(value, _dialogIcon)) return;
            _dialogIcon = value;
            OnPropertyChanged();
        }
    }


    public ObservableCollection<DialogAction> Actions
    {
        get => _actions;
        set
        {
            if (Equals(value, _actions)) return;
            _actions = value;
            OnPropertyChanged();
        }
    }

    public int ExecutedActionIndex
    {
        get => _executedActionIndex;
        set
        {
            if (value == _executedActionIndex) return;
            _executedActionIndex = value;
            OnPropertyChanged();
        }
    }

    public bool HasInput
    {
        get => _hasInput;
        set
        {
            if (value == _hasInput) return;
            _hasInput = value;
            OnPropertyChanged();
        }
    }

    public string InputResult
    {
        get => _inputResult;
        set
        {
            if (value == _inputResult) return;
            _inputResult = value;
            OnPropertyChanged();
        }
    }

    public CommonDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    private static bool IsEasterEggDisabled =>
        IAppHost.TryGetService<IManagementService>()?.Policy.DisableEasterEggs == true;

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        var hWnd = (HWND)new WindowInteropHelper(this).Handle;
        //NativeWindowHelper.SetWindowLong(hWnd, NativeWindowHelper.GWL_STYLE,
        //    NativeWindowHelper.GetWindowLong(hWnd, NativeWindowHelper.GWL_STYLE) & ~NativeWindowHelper.WS_SYSMENU);
        SetWindowLong(hWnd,
            WINDOW_LONG_PTR_INDEX.GWL_STYLE,
            GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE) | NativeWindowHelper.WS_EX_DLGMODALFRAME);
        SetWindowPos(hWnd, default, 0, 0, 0, 0,
            SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER |
            SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        ExecutedActionIndex = Actions.IndexOf((DialogAction)e.Parameter);
        Close();
    }

    public void ShowDialog(out string inputResult)
    {
        ShowDialog();
        inputResult = InputResult;
        return;
    }


    public static int ShowError(string message)
    {
        return new CommonDialogBuilder().SetContent(message).SetIconKind(CommonDialogIconKind.Forbidden)
            .AddConfirmAction()
            .ShowDialog();
    }

    public static int ShowInfo(string message)
    {
        return new CommonDialogBuilder().SetContent(message).SetIconKind(CommonDialogIconKind.Information)
            .AddConfirmAction()
            .ShowDialog();
    }

    public static int ShowHint(string message)
    {
        return new CommonDialogBuilder().SetContent(message).SetIconKind(CommonDialogIconKind.Hint).AddConfirmAction()
            .ShowDialog();
    }

    public static int ShowDialog(string caption, string message, BitmapImage icon, double iconWidth, double iconHeight,
        ObservableCollection<DialogAction> dialogActions)
    {
        var dialog = new CommonDialog
        {
            DialogContent = message,
            DialogIcon = new Image
            {
                Source = icon,
                Width = iconWidth,
                Height = iconHeight
            },
            Actions = dialogActions,
            Title = caption
        };
        dialog.ShowDialog();
        return dialog.ExecutedActionIndex;
    }

    public static int ShowDialog(string caption, string message, BitmapImage icon, double iconWidth,
        double iconHeight)
    {
        return ShowDialog(
            caption, message, icon, iconWidth, iconHeight, new ObservableCollection<DialogAction>
            {
                new()
                {
                    PackIconKind = PackIconKind.Check,
                    Name = "确定",
                    IsPrimary = true
                }
            }
        );
    }

    public static int ShowDialog(string message, BitmapImage icon, double iconWidth,
        double iconHeight)
    {
        return ShowDialog("ClassIsland", message, icon, iconWidth, iconHeight);
    }
}