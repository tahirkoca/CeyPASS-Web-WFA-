using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class KisiService : IKisiService
    {
        private readonly IKisiRepository _kisiRepo;
        private readonly IYemekhaneRepository _yemekhaneRepo;
        private readonly ILogger<KisiService>? _logger;

        public KisiService(
            IKisiRepository kisiRepo,
            IYemekhaneRepository yemekhaneRepo,
            ILogger<KisiService>? logger = null)
        {
            _kisiRepo = kisiRepo;
            _yemekhaneRepo = yemekhaneRepo;
            _logger = logger;
        }

        public void YeniKisiEkle(Kisi kisi, bool firmaPersoneli, bool puantajYapilabilir, bool yemekHakkiVar, int gunlukYemekLimiti, string puantajsizKartId, string puantajsizKartNo, string puantajsizKartAdi)
        {
            kisi.PuantajYapilirMi = puantajYapilabilir;
            _kisiRepo.Insert(kisi, puantajsizKartNo);
            if (yemekHakkiVar)
                _yemekhaneRepo.InsertLimit(kisi.PersonelId, gunlukYemekLimiti);
        }

        public bool KisiIstenCikar(string personelId, DateTime cikisTarihi, string firmaDisiKartNo)
        {
            try
            {
                _kisiRepo.SetIstenCikisTarihi(personelId, cikisTarihi);
                _yemekhaneRepo.PasifEtByPersonel(personelId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool KisiGuncelle(Kisi kisi, string originalPersonelId, bool firmaPersoneli, bool puantajYapilabilir, bool yemekHakkiVar, int gunlukYemekAdedi, string firmaDisiKartNo, bool fotoDegisti)
        {
            try
            {
                var ok = _kisiRepo.Update(kisi, originalPersonelId, fotoDegisti, firmaDisiKartNo);
                if (!ok) return false;

                if (yemekHakkiVar && gunlukYemekAdedi > 0)
                    _yemekhaneRepo.UpsertLimit(kisi.PersonelId, gunlukYemekAdedi);
                else
                    _yemekhaneRepo.PasifEtByPersonel(kisi.PersonelId);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "KisiGuncelle başarısız. PersonelId={PersonelId}, OriginalPersonelId={OriginalPersonelId}",
                    kisi?.PersonelId, originalPersonelId);
                return false;
            }
        }

        public List<Kisi> GetKisilerForPuantaj(int firmaId, int isyeriId, int yil, int ay)
        {
            return _kisiRepo.GetKisilerForPuantaj(firmaId, isyeriId, yil, ay);
        }

        public KisiAdSoyad GetAdSoyad(string personelId)
        {
            return _kisiRepo.GetAdSoyadByPersonelId(personelId);
        }

        public (bool IsValid, string? Message) ValidateKisiKayit(KisiKayitValidasyonDTO dto)
        {
            bool firma = dto.FirmaPersoneli;
            bool puantaj = dto.PuantajYapilir;
            bool yemek = dto.YemekHakkiVar;

            if (string.IsNullOrWhiteSpace(dto.PersonelId))
                return (false, "PersonelId (Sicil No) giriniz.");

            bool puantajsizKartGerekli = (firma && !puantaj) || (!firma && !puantaj && yemek);
            if (puantajsizKartGerekli)
            {
                if (string.IsNullOrWhiteSpace(dto.FirmaDisiKartNo))
                    return (false, "Firma Dışı Kart No giriniz.");
            }

            if (yemek && dto.YemekAdedi <= 0)
                return (false, "Yemek hakkı var; günlük yemek adedini giriniz.");

            if (!firma && puantaj)
                return (false, "Bu check kombinasyonu (Firma personeli değil + Puantaj yapılabilir) için kural tanımlı değil.");

            return (true, null);
        }
    }
}
