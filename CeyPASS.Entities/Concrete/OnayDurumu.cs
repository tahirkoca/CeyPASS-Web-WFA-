namespace CeyPASS.Entities.Concrete
{
    /// <summary>Puantaj gün satırı onay durumu; veritabanında int olarak da tutulabilir.</summary>
    public enum OnayDurumu
    {
        Bekliyor = 0,
        Onaylandı = 1,
        Reddedildi = 2,
        Düzeltildi = 3
    }
}
