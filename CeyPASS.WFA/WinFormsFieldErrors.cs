namespace CeyPASS.WFA;

/// <summary>
/// ErrorProvider sarmalayıcısı — zorunlu alan doğrulaması için.
/// </summary>
public sealed class WinFormsFieldErrors : IDisposable
{
    private readonly ErrorProvider _provider;
    private readonly Dictionary<Control, string> _map = new();

    public WinFormsFieldErrors(ContainerControl host)
    {
        _provider = new ErrorProvider
        {
            ContainerControl = host,
            BlinkStyle = ErrorBlinkStyle.NeverBlink
        };
    }

    public bool HasErrors => _map.Count > 0;

    public string? FirstMessage => _map.Values.FirstOrDefault();

    public void Clear()
    {
        foreach (var c in _map.Keys.ToList())
            _provider.SetError(c, string.Empty);
        _map.Clear();
    }

    public void Set(Control control, string? message)
    {
        if (control == null) return;

        if (string.IsNullOrWhiteSpace(message))
        {
            _provider.SetError(control, string.Empty);
            _map.Remove(control);
            return;
        }

        var trimmed = message.Trim();
        _provider.SetError(control, trimmed);
        _map[control] = trimmed;
    }

    public bool Require(Control control, string? value, string message)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            Set(control, null);
            return true;
        }

        Set(control, message);
        return false;
    }

    public bool Require(Control control, object? value, string message)
    {
        if (value != null && (!(value is string s) || !string.IsNullOrWhiteSpace(s)))
        {
            Set(control, null);
            return true;
        }

        Set(control, message);
        return false;
    }

    public void Dispose() => _provider.Dispose();
}
