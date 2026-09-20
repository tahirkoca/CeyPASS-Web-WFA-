namespace CeyPASS.WFA;

/// <summary>
/// ErrorProvider sarmalayıcısı — zorunlu alan doğrulaması için.
/// </summary>
public sealed class WinFormsFieldErrors : IDisposable
{
    private readonly ErrorProvider _provider;
    private readonly Dictionary<Control, string> _map = new();

    /// <summary>Form veya UserControl için ErrorProvider oluşturur.</summary>
    public WinFormsFieldErrors(ContainerControl host)
    {
        _provider = new ErrorProvider
        {
            ContainerControl = host,
            BlinkStyle = ErrorBlinkStyle.NeverBlink
        };
    }

    /// <summary>En az bir alan hatası var mı.</summary>
    public bool HasErrors => _map.Count > 0;

    /// <summary>İlk hata mesajı.</summary>
    public string? FirstMessage => _map.Values.FirstOrDefault();

    /// <summary>Tüm alan hatalarını temizler.</summary>
    public void Clear()
    {
        foreach (var c in _map.Keys.ToList())
            _provider.SetError(c, string.Empty);
        _map.Clear();
    }

    /// <summary>Belirtilen kontrol için hata mesajı ayarlar veya temizler.</summary>
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

    /// <summary>Boş olmayan string zorunluluğu; geçerliyse true.</summary>
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

    /// <summary>Null/boş nesne zorunluluğu; geçerliyse true.</summary>
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

    /// <summary>ErrorProvider kaynaklarını serbest bırakır.</summary>
    public void Dispose() => _provider.Dispose();
}
