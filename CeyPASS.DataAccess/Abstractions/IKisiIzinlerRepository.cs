using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Personel izin kayıtları (KisiIzinler).</summary>
    public interface IKisiIzinlerRepository
    {
        /// <summary>Izinleri sorgularını getirir.</summary>
        DataTable GetIzinleri(string personelId, DateTime baslangic, DateTime bitis);
        /// <summary>Kimliğe göre kaydı getirir.</summary>
        KisiIzin GetById(int kisiIzinId);
        /// <summary>Yeni kayıt ekler.</summary>
        bool Insert(KisiIzin x);
        /// <summary>Kaydı günceller.</summary>
        bool Update(KisiIzin x);
        /// <summary>Izin Raporu sorgularını getirir; isteğe bağlı işyeri filtresi.</summary>
        DataTable GetIzinRaporu(int? firmaId, string personelId, int? izinTipId, DateTime bas, DateTime bit, int? isyeriId = null, IReadOnlyList<int> isyeriIdIn = null);
        /// <summary>Izin Raporu Paged sorgularını getirir; isteğe bağlı işyeri filtresi.</summary>
        List<KisiIzinListRow> GetIzinRaporuPaged(int? firmaId, string personelId, int? izinTipId, DateTime bas, DateTime bit, int page, int pageSize, out int totalCount, int? isyeriId = null, IReadOnlyList<int> isyeriIdIn = null);
        /// <summary>Pasif Yap işlemini gerçekleştirir.</summary>
        bool PasifYap(int kisiIzinId);
        /// <summary>Aktif Yap işlemini gerçekleştirir.</summary>
        bool AktifYap(int kisiIzinId);
        /// <summary>By Person sorgularını getirir.</summary>
        DataTable GetByPerson(string personelId, DateTime? bas = null, DateTime? bit = null);
    }
}
