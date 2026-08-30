using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CeyPASS.Entities.Helpers;
using DevExpress.Xpf.Editors;

namespace CeyPASS.WPF.Views;

internal static class CanliIzlemeKartFormHelper
{
    private const double FieldGap = 8;

    private static Brush FieldBorderBrush =>
        ThemeBrushes.Get("Brush.FieldBorder", Color.FromRgb(0xD0, 0xD7, 0xE2));

    private static Brush FieldBgBrush =>
        ThemeBrushes.Get("Brush.FieldBg", Colors.White);

    private static Brush FieldFgBrush =>
        ThemeBrushes.Get("Brush.FieldFg", Color.FromRgb(0x0F, 0x17, 0x2A));

    private static Brush LabelBrush =>
        ThemeBrushes.Get("Brush.TextMuted", Color.FromRgb(0x64, 0x74, 0x8B));

    public sealed class TcField
    {
        public TextBox Box { get; }
        private string? _tamTc;
        private bool _suppress;
        private Action? _onLeave;

        public TcField(TextBox box)
        {
            Box = box;
            Box.MaxLength = 11;
            Box.TextChanged += (_, _) =>
            {
                if (_suppress) return;
                var shown = Box.Text?.Trim() ?? "";
                if (!string.IsNullOrEmpty(_tamTc) && shown == TcKimlikHelper.Mask(_tamTc))
                    return;
                _tamTc = null;
            };
            Box.LostFocus += (_, _) => _onLeave?.Invoke();
            Box.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    _onLeave?.Invoke();
                    e.Handled = true;
                }
            };
        }

        public void OnLeave(Action handler) => _onLeave = handler;

        public void ShowMasked(string? fullTc)
        {
            _tamTc = string.IsNullOrWhiteSpace(fullTc) ? null : fullTc.Trim();
            _suppress = true;
            Box.Text = string.IsNullOrEmpty(_tamTc) ? "" : TcKimlikHelper.Mask(_tamTc);
            _suppress = false;
        }

        public string ResolveForSave() => TcKimlikHelper.ResolveForSave(Box.Text, _tamTc);
    }

    public static TextBlock CreateLabel(string text) => new()
    {
        Text = text,
        FontSize = 12,
        FontWeight = FontWeights.SemiBold,
        Foreground = LabelBrush,
        Margin = new Thickness(0, 0, 0, 4)
    };

    public static TextBox CreateField(string text = "") => new()
    {
        Text = text,
        FontSize = 13.5,
        Height = 34,
        Padding = new Thickness(8, 6, 8, 6),
        BorderBrush = FieldBorderBrush,
        BorderThickness = new Thickness(1),
        Background = FieldBgBrush,
        Foreground = FieldFgBrush,
        CaretBrush = FieldFgBrush,
        VerticalContentAlignment = VerticalAlignment.Center
    };

    public static ComboBox CreateCombo() => new()
    {
        FontSize = 13.5,
        Height = 34,
        Padding = new Thickness(8, 4, 8, 4),
        BorderBrush = FieldBorderBrush,
        Background = FieldBgBrush,
        Foreground = FieldFgBrush,
        VerticalContentAlignment = VerticalAlignment.Center
    };

    public static DateEdit CreateDateTimeEdit(DateTime value, bool enabled = true) => new()
    {
        EditValue = value,
        Mask = "dd.MM.yyyy HH:mm",
        MaskUseAsDisplayFormat = true,
        FontSize = 13.5,
        Height = 34,
        IsEnabled = enabled
    };

    public static TextBox CreateAciklama(string text = "") => new()
    {
        Text = text,
        FontSize = 13.5,
        Height = 34,
        Padding = new Thickness(8, 6, 8, 6),
        BorderBrush = FieldBorderBrush,
        BorderThickness = new Thickness(1),
        Background = FieldBgBrush,
        Foreground = FieldFgBrush,
        CaretBrush = FieldFgBrush,
        VerticalContentAlignment = VerticalAlignment.Center
    };

    public static Grid CreateFormGrid()
    {
        var grid = new Grid { Margin = new Thickness(0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(FieldGap) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        return grid;
    }

    public static int AddFullRow(Grid grid, int row, string label, UIElement control)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var stack = new StackPanel { Margin = new Thickness(0, 0, 0, FieldGap) };
        stack.Children.Add(CreateLabel(label));
        stack.Children.Add(control);
        Grid.SetRow(stack, row);
        Grid.SetColumn(stack, 0);
        Grid.SetColumnSpan(stack, 3);
        grid.Children.Add(stack);
        return row + 1;
    }

    public static int AddSplitRow(Grid grid, int row, string leftLabel, UIElement leftControl, string rightLabel, UIElement rightControl)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var left = new StackPanel { Margin = new Thickness(0, 0, 0, FieldGap) };
        left.Children.Add(CreateLabel(leftLabel));
        left.Children.Add(leftControl);
        Grid.SetRow(left, row);
        Grid.SetColumn(left, 0);
        grid.Children.Add(left);

        var right = new StackPanel { Margin = new Thickness(0, 0, 0, FieldGap) };
        right.Children.Add(CreateLabel(rightLabel));
        right.Children.Add(rightControl);
        Grid.SetRow(right, row);
        Grid.SetColumn(right, 2);
        grid.Children.Add(right);

        return row + 1;
    }
}
