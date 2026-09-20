namespace CeyPASS.Entities.Concrete
{
    /// <summary>İzin talebi üst yetkili / İK onay adımları.</summary>
    public enum IzinOnayDurumu : byte
    {
        Bekliyor = 0,
        Onaylandi = 1,
        Reddedildi = 2
    }
}

