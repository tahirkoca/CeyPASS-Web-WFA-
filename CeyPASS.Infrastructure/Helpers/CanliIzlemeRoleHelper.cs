using System;

namespace CeyPASS.Infrastructure.Helpers;

/// <summary>Canlı İzleme hesap rol metinleri (CanliIzlemeHesaplari.Rol).</summary>
public static class CanliIzlemeRoleHelper
{
    /// <summary>Rol metni YEMEKHANE (büyük/küçük harf duyarsız).</summary>
    public static bool IsYemekhane(string? rolAdi) =>
        string.Equals(rolAdi ?? string.Empty, "YEMEKHANE", StringComparison.OrdinalIgnoreCase);

    /// <summary>Rol ARAÇ veya ARAC.</summary>
    public static bool IsArac(string? rolAdi)
    {
        var r = rolAdi ?? "";
        return string.Equals(r, "ARAÇ", StringComparison.OrdinalIgnoreCase)
               || string.Equals(r, "ARAC", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Rol adında DANIŞMA / DANISMA geçiyor.</summary>
    public static bool IsDanisma(string? rolAdi)
    {
        var r = rolAdi ?? "";
        return r.IndexOf("DANIŞMA", StringComparison.OrdinalIgnoreCase) >= 0
               || r.IndexOf("DANISMA", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>Tam izleme ekranı (kartlar + liste + kart atama). Rol: CANLI İZLEME / CANLI IZLEME.</summary>
    public static bool IsCanliIzleme(string? rolAdi)
    {
        var r = (rolAdi ?? "").Trim();
        if (r.Length == 0) return false;
        return string.Equals(r, "CANLI İZLEME", StringComparison.OrdinalIgnoreCase)
               || string.Equals(r, "CANLI IZLEME", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Son hareket listesi yalnızca CANLI İZLEME rolünde.</summary>
    public static bool ShowHareketListesi(string? rolAdi) => IsCanliIzleme(rolAdi);

    /// <summary>Kart atama yalnızca CANLI İZLEME rolünde; diğer roller kapalı.</summary>
    public static bool HideKartAtama(string? rolAdi) => !IsCanliIzleme(rolAdi);
}
