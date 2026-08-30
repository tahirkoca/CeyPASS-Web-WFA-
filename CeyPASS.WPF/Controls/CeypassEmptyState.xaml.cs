using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CeyPASS.WPF.Controls;

/// <summary>
/// Liste/grid boşken ortada başlık + yönlendirme gösterir.
/// </summary>
public partial class CeypassEmptyState : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(CeypassEmptyState),
            new PropertyMetadata("Kayıt yok"));

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(CeypassEmptyState),
            new PropertyMetadata("Filtreleri kontrol edin veya Yenile’ye basın."));

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(CeypassEmptyState),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(CeypassEmptyState),
            new PropertyMetadata(true, OnIsActiveChanged));

    private INotifyCollectionChanged? _watchingNcc;
    private IBindingList? _watchingList;

    public CeypassEmptyState()
    {
        InitializeComponent();
        Loaded += (_, _) => RefreshVisibility();
        IsHitTestVisible = false;
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>false iken (ör. henüz rapor getirilmedi) gizlenir.</summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CeypassEmptyState ctrl)
            return;
        ctrl.DetachWatch();
        ctrl.AttachWatch(e.NewValue);
        ctrl.RefreshVisibility();
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CeypassEmptyState ctrl)
            ctrl.RefreshVisibility();
    }

    private void AttachWatch(object? source)
    {
        if (source is INotifyCollectionChanged ncc)
        {
            _watchingNcc = ncc;
            ncc.CollectionChanged += OnCollectionChanged;
        }
        else if (source is IBindingList list)
        {
            _watchingList = list;
            list.ListChanged += OnListChanged;
        }
    }

    private void DetachWatch()
    {
        if (_watchingNcc != null)
        {
            _watchingNcc.CollectionChanged -= OnCollectionChanged;
            _watchingNcc = null;
        }

        if (_watchingList != null)
        {
            _watchingList.ListChanged -= OnListChanged;
            _watchingList = null;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => Dispatcher.Invoke(RefreshVisibility);

    private void OnListChanged(object? sender, ListChangedEventArgs e)
        => Dispatcher.Invoke(RefreshVisibility);

    private void RefreshVisibility()
    {
        if (!IsActive)
        {
            Visibility = Visibility.Collapsed;
            return;
        }

        var count = CountItems(ItemsSource);
        Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static int? CountItems(object? source)
    {
        if (source == null)
            return null;
        if (source is ICollection c)
            return c.Count;
        if (source is IEnumerable e)
        {
            var n = 0;
            foreach (var _ in e)
                n++;
            return n;
        }

        return null;
    }
}
