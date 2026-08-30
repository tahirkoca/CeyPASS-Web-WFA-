using System;

namespace CeyPASS.Infrastructure.Helpers;

/// <summary>Canlı İzleme hesap rol metinleri (CanliIzlemeHesaplari.Rol).</summary>
public static class CanliIzlemeRoleHelper
{
    public static bool IsYemekhane(string? rolAdi) =>
        string.Equals(rolAdi ?? string.Empty, "YEMEKHANE", StringComparison.OrdinalIgnoreCase);

    public static bool IsArac(string? rolAdi)
    {
        var r = rolAdi ?? "";
        return string.Equals(r, "ARAÇ", StringComparison.OrdinalIgnoreCase)
               || string.Equals(r, "ARAC", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsDanisma(string? rolAdi)
    {
        var r = rolAdi ?? "";
        return r.IndexOf("DANIŞMA", StringComparison.OrdinalIgnoreCase) >= 0
               || r.IndexOf("DANISMA", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>YEMEKHANE veya ARAÇ (ve Danışma değilse) kart atama kapalı.</summary>
    public static bool HideKartAtama(string? rolAdi) =>
        (IsYemekhane(rolAdi) || IsArac(rolAdi)) && !IsDanisma(rolAdi);
}
