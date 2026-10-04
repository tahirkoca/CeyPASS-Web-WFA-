using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Personel geçiş hareketleri sorgu ve manuel düzeltme.</summary>
    public interface IKisiHareketService
    {
        /// <summary>Firmadaki son geçişler.</summary>
        List<KisiHareketDTO> GetLastMovesByFirma(int top, int firmaId);

        /// <summary>Son yemekhane geçişleri.</summary>
        List<KisiHareketDTO> GetLastMovesByFirmaYemekhane(int top, int firmaId);

        /// <summary>Son araç geçişleri.</summary>
        List<KisiHareketDTO> GetLastMovesByFirmaArac(int top, int firmaId);

        /// <summary>Seçili personellerin hareketleri (filtreli).</summary>
        /// <param name="firmaId">Personel listesi için; personIds doluysa hareketler tüm firmalardan listelenir.</param>
        DataTable GetByPersons(List<int> personIds, DateTime bas, DateTime bit, bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId);

        /// <summary>Sayfalanmış hareket listesi.</summary>
        /// <param name="firmaId">Personel listesi için; personIds doluysa hareketler tüm firmalardan listelenir.</param>
        List<KisiHareketListRow> GetByPersonsPaged(List<int> personIds, DateTime bas, DateTime bit, bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId, int page, int pageSize, out int totalCount);

        /// <summary>Manuel geçiş kaydı ekler.</summary>
        bool InsertManual(int firmaId, int personelId, DateTime tarih, string tip, int cihazId = 0);

        /// <summary>Manuel geçiş kaydını günceller.</summary>
        bool UpdateManual(int id, DateTime tarih, string tip, int? cihazId = null);

        /// <summary>Personelin günündeki aktif ilk Giriş ve son Çıkış uçlarını döner.</summary>
        PuantajGunHareketUctanUcaDTO GetGunUctanUca(int personelId, DateTime tarih);

        /// <summary>Hareketi pasifleştirir.</summary>
        bool PasifYap(int id);

        /// <summary>Hareketi tekrar aktifleştirir.</summary>
        bool AktifYap(int id);

        /// <summary>Puantaj yapılan aktif personel sicil tablosu.</summary>
        DataTable GetAktifKisilerWithSicil(int firmaId, bool puantajYapilirMi = true);
    }
}
