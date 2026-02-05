using CeyPASS.Entities.Concrete;
using System;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    public interface IKisiIzinService
    {
        bool Ekle(KisiIzin izin);
        bool Guncelle(KisiIzin izin);
        KisiIzin GetById(int kisiIzinId);
        bool PasifYap(int kisiIzinId);
        DataTable GetTumIzinler(int? firmaId, string personelId, int? izinTipId, DateTime bas, DateTime bit);
        (bool IsValid, string? Message) ValidateKayit(IzinKayitValidasyonDTO dto);
    }
}
