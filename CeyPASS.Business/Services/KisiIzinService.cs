using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Data;

namespace CeyPASS.Business.Services
{
    public class KisiIzinService : IKisiIzinService
    {
        private readonly IKisiIzinlerRepository _repo;

        public KisiIzinService(IKisiIzinlerRepository repo)
        {
            _repo=repo;
        }
        public bool Ekle(KisiIzin izin) => _repo.Insert(izin);
        public bool Guncelle(KisiIzin izin) => _repo.Update(izin);
        public KisiIzin GetById(int kisiIzinId) => _repo.GetById(kisiIzinId);
        public bool PasifYap(int kisiIzinId) => _repo.PasifYap(kisiIzinId);
        public DataTable GetTumIzinler(int? firmaId, string personelId, int? izinTipId, DateTime bas, DateTime bit) => _repo.GetIzinRaporu(firmaId, personelId, izinTipId, bas, bit);
        public (bool IsValid, string? Message) ValidateKayit(IzinKayitValidasyonDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PersonelId))
                return (false, "Kayıt için lütfen belirli bir kişi seçiniz.");

            if (!dto.SaatlikIzinMi && (!dto.IzinTipId.HasValue || dto.IzinTipId.Value <= 0))
                return (false, "Kayıt için lütfen belirli bir izin tipi seçiniz.");

            var basT = dto.BaslangicTarihi.Date;
            var bitT = dto.BitisTarihi.Date;

            if (dto.SaatlikIzinMi)
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
