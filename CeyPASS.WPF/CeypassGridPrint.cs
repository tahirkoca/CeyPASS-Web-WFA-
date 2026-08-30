using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;

namespace CeyPASS.WPF;

/// <summary>
/// TableView: Ctrl+P, sağ tık Yazdır / Excel / PDF ve kullanıcı ipucu.
/// </summary>
public static class CeypassGridPrint
{
    public const string HintText = "Ctrl+F ara · Ctrl+P veya sağ tık: yazdır / Excel / PDF";

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(CeypassGridPrint),
            new PropertyMetadata(false, OnEnableChanged));

    public static void SetEnable(DependencyObject element, bool value)
        => element.SetValue(EnableProperty, value);

    public static bool GetEnable(DependencyObject element)
        => (bool)element.GetValue(EnableProperty);

    private static readonly DependencyProperty HintAttachedProperty =
        DependencyProperty.RegisterAttached(
            "HintAttached",
            typeof(bool),
            typeof(CeypassGridPrint));

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TableView view)
            return;

        view.Loaded -= OnViewLoaded;
        view.ShowGridMenu -= OnShowGridMenu;
        if (Equals(e.NewValue, true))
        {
            if (view.IsLoaded)
                Attach(view);
            else
                view.Loaded += OnViewLoaded;
        }
    }

    private static void OnViewLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not TableView view)
            return;
        view.Loaded -= OnViewLoaded;
        Attach(view);
    }

    private static void Attach(TableView view)
    {
        view.ShowGridMenu -= OnShowGridMenu;
        view.ShowGridMenu += OnShowGridMenu;

        var hasPrintShortcut = false;
        foreach (InputBinding b in view.InputBindings)
        {
            if (b is KeyBinding kb && kb.Key == Key.P && kb.Modifiers == ModifierKeys.Control)
            {
                hasPrintShortcut = true;
                break;
            }
        }

        if (!hasPrintShortcut)
        {
            view.InputBindings.Add(new KeyBinding(view.Commands.ShowPrintPreview, Key.P, ModifierKeys.Control)
            {
                CommandTarget = view
            });
        }

        EnsureHint(view);
    }

    private static void EnsureHint(TableView view)
    {
        if (view.DataControl is not GridControl grid)
            return;
        if ((bool)grid.GetValue(HintAttachedProperty))
            return;

        // Mantıksal parent; görsel parent DX içinde farklı olabilir.
        var parent = LogicalTreeHelper.GetParent(grid) as FrameworkElement;
        if (parent is null)
            return;

        var hint = new TextBlock
        {
            Text = HintText,
            FontSize = 12,
            Margin = new Thickness(2, 8, 2, 0),
            TextWrapping = TextWrapping.Wrap
        };
        hint.SetResourceReference(TextBlock.ForegroundProperty, "Brush.TextMuted");

        var host = new DockPanel();
        DockPanel.SetDock(hint, Dock.Bottom);
        CopyLayoutSlot(grid, host);

        // Önce grid'i parent'tan ayır, sonra host'a ekle.
        switch (parent)
        {
            case Decorator decorator when ReferenceEquals(decorator.Child, grid):
                decorator.Child = null;
                host.Children.Add(hint);
                host.Children.Add(grid);
                decorator.Child = host;
                break;
            case Panel panel:
                var index = panel.Children.IndexOf(grid);
                if (index < 0)
                    return;
                panel.Children.RemoveAt(index);
                host.Children.Add(hint);
                host.Children.Add(grid);
                panel.Children.Insert(index, host);
                break;
            case ContentControl content when ReferenceEquals(content.Content, grid):
                content.Content = null;
                host.Children.Add(hint);
                host.Children.Add(grid);
                content.Content = host;
                break;
            default:
                return;
        }

        grid.SetValue(HintAttachedProperty, true);
    }

    private static void CopyLayoutSlot(FrameworkElement from, FrameworkElement to)
    {
        to.Margin = from.Margin;
        from.Margin = new Thickness(0);
        Grid.SetRow(to, Grid.GetRow(from));
        Grid.SetRowSpan(to, Grid.GetRowSpan(from));
        Grid.SetColumn(to, Grid.GetColumn(from));
        Grid.SetColumnSpan(to, Grid.GetColumnSpan(from));
        DockPanel.SetDock(to, DockPanel.GetDock(from));
    }

    private static void OnShowGridMenu(object sender, GridMenuEventArgs e)
    {
        if (sender is not TableView view)
            return;
        if (e.MenuType is not GridMenuType.RowCell and not GridMenuType.Column)
            return;

        e.Customizations.Add(new BarButtonItem
        {
            Content = "Yazdır / Excel / PDF",
            Command = view.Commands.ShowPrintPreview,
            CommandTarget = view
        });
    }
}
