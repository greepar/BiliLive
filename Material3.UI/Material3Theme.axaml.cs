using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Windows.Input;

namespace Material3.UI;

/// <summary>
/// The Material3.UI entry-point Style. Add a single instance to
/// <c>Application.Styles</c> to apply tokens, control themes, and global styles.
/// </summary>
public partial class Material3Theme : Styles
{
    private static bool s_globalHandlersInstalled;
    private static bool s_isDarkTheme = true;

    public static ICommand SwitchThemeCommand { get; } = new RelayCommand(SwitchTheme);

    public Material3Theme()
    {
        AvaloniaXamlLoader.Load(this);
        InstallGlobalHandlers();
    }

    /// <summary>
    /// M3 spec: a click outside an editable surface should dismiss focus from
    /// any focused TextBox so the user perceives the field as deselected.
    /// We attach a class-level tunneling pointer-pressed handler on TopLevel
    /// once per app, then walk the visual tree of the click target. If the
    /// click did not land inside a TextBox, we move focus to the TopLevel.
    /// </summary>
    private static void InstallGlobalHandlers()
    {
        if (s_globalHandlersInstalled) return;
        s_globalHandlersInstalled = true;

        InputElement.PointerPressedEvent.AddClassHandler<TopLevel>(
            (top, e) => OnTopLevelPointerPressed(top, e),
            RoutingStrategies.Tunnel);
    }

    private static void OnTopLevelPointerPressed(TopLevel top, PointerPressedEventArgs e)
    {
        if (e.Source is not Visual src) return;

        // Walk up from the click target. If we find a TextBox before we leave
        // the visual tree, the click is "inside an editor"; leave focus alone.
        var node = src as Visual;
        while (node is not null)
        {
            if (node is TextBox) return;
            node = node.GetVisualParent();
        }

        // Click was outside any TextBox. If a TextBox currently holds focus,
        // move focus to the TopLevel so the field reads as deselected.
        if (top.FocusManager?.GetFocusedElement() is TextBox)
        {
            top.Focus();
        }
    }

    private static void SwitchTheme()
    {
        s_isDarkTheme = !s_isDarkTheme;
        Application.Current!.RequestedThemeVariant = s_isDarkTheme ? ThemeVariant.Light : ThemeVariant.Dark;
    }

    private sealed class RelayCommand(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
