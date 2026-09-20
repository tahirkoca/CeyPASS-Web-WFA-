using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Services
{
    /// <summary>Personel izin kayıtları.</summary>
    public class KisiIzinService : IKisiIzinService
    {
        private readonly IKisiIzinlerRepository _repo;

        public KisiIzinService(IKisiIzinlerRepository repo)
        {
            _repo=repo;
        }
        /// <inheritdoc />
        public bool Ekle(KisiIzin izin) => _repo.Insert(izin);
        /// <inheritdoc />
        public bool Guncelle(KisiIzin izin) => _repo.Update(izin);
        /// <inheritdoc />
        public KisiIzin GetById(int kisiIzinId) => _repo.GetById(kisiIzinId);
        /// <inheritdoc />
        public bool PasifYap(int kisiIzinId) => _repo.PasifYap(kisiIzinId);
        /// <inheritdoc />
        public bool AktifYap(int kisiIzinId) => _repo.AktifYap(kisiIzinId);
        /// <inheritdoc />
        public DataTable GetTumIzinler(int? firmaId, string personelId, int? izinTipId, DateTime bas, DateTime bit, int? isyeriId = null, IReadOnlyList<int>? isyeriIdIn = null)
            => _repo.GetIzinRaporu(firmaId, personelId, izinTipId, bas, bit, isyeriId, isyeriIdIn);
        /// <inheritdoc />
        public List<KisiIzinListRow> GetTumIzinlerPaged(int? firmaId, string personelId, int? izinTipId, DateTime bas, DateTime bit, int page, int pageSize, out int totalCount, int? isyeriId = null, IReadOnlyList<int>? isyeriIdIn = null)
            => _repo.GetIzinRaporuPaged(firmaId, personelId, izinTipId, bas, bit, page, pageSize, out totalCount, isyeriId, isyeriIdIn);
        public (bool IsValid, string? Message) ValidateKayit(IzinKayitValidasyonDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PersonelId))
                return (false, "Kayıt için lütfen belirli bir kişi seçiniz.");

            if (dto.YarimGunYillikIzinMi)
            {
                if (dto.IzinTipId != 2)
                    return (false, "Yarım gün izin yalnızca yıllık izin (Yİ) tipi ile kaydedilebilir.");
                if (!dto.SaatlikIzinMi)
                    return (false, "Yarım gün izin saatlik kayıt olarak işlenmelidir.");
            }

            if (!dto.SaatlikIzinMi && !dto.YarimGunYillikIzinMi && (!dto.IzinTipId.HasValue || dto.IzinTipId.Value <= 0))
                return (false, "Kayıt için lütfen belirli bir izin tipi seçiniz.");

            var basT = dto.BaslangicTarihi.Date;
            var bitT = dto.BitisTarihi.Date;

            if (dto.SaatlikIzinMi || dto.YarimGunYillikIzinMi)
            {
                if (basT != bitT)
                    return (false, "Saatlik izinde başlangıç ve bitiş tarihi aynı gün olmalıdır.");

                var bas = basT + (dto.BaslangicSaati ?? TimeSpan.Zero);
                var bit = bitT + (dto.BitisSaati ?? TimeSpan.Zero);

                if (bit <= bas)
                    return (false, "Bitiş, başlangıçtan sonra olmalı.");
            }
            else
            {
                if (bitT < basT)
                    return (false, "Bitiş tarihi, başlangıç tarihinden önce olamaz.");
            }
            return (true, null);
        }
    }
}
