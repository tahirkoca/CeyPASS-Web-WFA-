using System.Collections;
using System.ComponentModel;

namespace CeyPASS.WPF;

/// <summary>
/// Form alanı hataları — XAML: FieldValidation.Error="{Binding Errors[Ad]}"
/// </summary>
public sealed class BindableFieldErrors : INotifyPropertyChanged, IEnumerable<KeyValuePair<string, string>>
{
    private readonly Dictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase);

    public event PropertyChangedEventHandler? PropertyChanged;

    public string? this[string key]
    {
        get => _map.TryGetValue(key, out var v) ? v : null;
        set => Set(key, value);
    }

    public bool HasErrors => _map.Count > 0;

    public string? FirstMessage => _map.Values.FirstOrDefault();

    public void Clear()
    {
        if (_map.Count == 0) return;
        var keys = _map.Keys.ToList();
        _map.Clear();
        foreach (var k in keys)
            RaiseItem(k);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasErrors)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    public void Set(string key, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            if (!_map.Remove(key)) return;
        }
        else
        {
            _map[key] = message.Trim();
        }

        RaiseItem(key);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasErrors)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    public bool Require(string key, string? value, string message)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            Set(key, null);
            return true;
        }

        Set(key, message);
        return false;
    }

    public bool Require(string key, object? value, string message)
    {
        if (value != null && (!(value is string s) || !string.IsNullOrWhiteSpace(s)))
        {
            Set(key, null);
            return true;
        }

        Set(key, message);
        return false;
    }

    private void RaiseItem(string key)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _map.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
