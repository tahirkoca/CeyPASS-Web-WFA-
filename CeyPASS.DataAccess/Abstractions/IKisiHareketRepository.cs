using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Personel giriş-çıkış hareket sorguları.</summary>
    public interface IKisiHareketRepository
    {
        /// <summary>Firmaya ait son PDKS geçiş hareketlerini getirir.</summary>
        List<KisiHareketDTO> GetLastMovesByFirma(int top, int firmaId);
        /// <summary>Yemekhane cihazlarından son geçiş hareketlerini getirir.</summary>
        List<KisiHareketDTO> GetLastMovesByFirmaYemekhane(int top, int firmaId);
        /// <summary>Araç giriş-çıkış cihazlarından son hareketleri getirir.</summary>
        List<KisiHareketDTO> GetLastMovesByFirmaArac(int top, int firmaId);
        /// <summary>Ana giriş cihazlarında personel bazında son hareket yönü (GirisMi + Tarih).</summary>
        List<PersonelSonHareketYon> GetLastGirisMiByPersonelIds(IReadOnlyList<string> personelIds);
        /// <summary>Seçili personellerin tarih aralığındaki hareketlerini getirir.</summary>
        /// <param name="firmaId">Personel listesi için; personIds doluysa hareket sorgusunda FirmaId filtresi uygulanmaz (tüm firmalardaki hareketler).</param>
        DataTable GetByPersons(List<int> personIds, DateTime bas, DateTime bit, bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId);
        /// <summary>GetByPersons sonuçlarını sayfalı döner.</summary>
        /// <param name="firmaId">Personel listesi için; personIds doluysa hareket sorgusunda FirmaId filtresi uygulanmaz (tüm firmalardaki hareketler).</param>
        List<KisiHareketListRow> GetByPersonsPaged(List<int> personIds, DateTime bas, DateTime bit, bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId, int page, int pageSize, out int totalCount);
        /// <summary>Manuel giriş-çıkış hareketi ekler.</summary>
        bool InsertManual(int firmaId, int personelId, DateTime tarih, string tip, int cihazId = 0);
        /// <summary>Manuel hareket kaydını günceller.</summary>
        bool UpdateManual(int id, DateTime tarih, string tip, int? cihazId = null);
        /// <summary>Personelin günündeki aktif ilk Giriş ve son Çıkış uçlarını döner.</summary>
        PuantajGunHareketUctanUcaDTO GetGunUctanUca(int personelId, DateTime tarih);
        /// <summary>Hareket kaydını pasifleştirir.</summary>
        bool PasifYap(int id);
        /// <summary>Pasif hareket kaydını tekrar aktifleştirir.</summary>
        bool AktifYap(int id);
        /// <summary>Firmadaki aktif personelleri sicil bilgisiyle listeler.</summary>
        DataTable GetAktifKisilerWithSicil(int firmaId, bool puantajYapilirMi = true);
    }
}
