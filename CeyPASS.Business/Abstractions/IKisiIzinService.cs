using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Personel izin kayıtları (İK tarafı).</summary>
    public interface IKisiIzinService
    {
        /// <summary>Yeni izin kaydı.</summary>
        bool Ekle(KisiIzin izin);

        /// <summary>Mevcut izin kaydını günceller.</summary>
        bool Guncelle(KisiIzin izin);

        /// <summary>Kimliğe göre izin kaydı.</summary>
        KisiIzin GetById(int kisiIzinId);

        /// <summary>İzin kaydını pasifleştirir.</summary>
        bool PasifYap(int kisiIzinId);

        /// <summary>İzin kaydını aktifleştirir.</summary>
        bool AktifYap(int kisiIzinId);

        /// <summary>Filtreli izin listesi (tablo); isteğe bağlı işyeri kapsamı.</summary>
        DataTable GetTumIzinler(int? firmaId, string personelId, int? izinTipId, DateTime bas, DateTime bit, int? isyeriId = null, IReadOnlyList<int>? isyeriIdIn = null);

        /// <summary>Sayfalanmış izin listesi; isteğe bağlı işyeri kapsamı.</summary>
        List<KisiIzinListRow> GetTumIzinlerPaged(int? firmaId, string personelId, int? izinTipId, DateTime bas, DateTime bit, int page, int pageSize, out int totalCount, int? isyeriId = null, IReadOnlyList<int>? isyeriIdIn = null);

        /// <summary>Kayıt öncesi iş kuralı doğrulaması.</summary>
        (bool IsValid, string? Message) ValidateKayit(IzinKayitValidasyonDTO dto);
    }
}
