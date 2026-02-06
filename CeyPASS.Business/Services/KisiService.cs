using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class KisiService : IKisiService
    {
        private readonly IKisiRepository _kisiRepo;
        private readonly IPuantajsizKartRepository _puantajsizKartRepo;
        private readonly IYemekhaneRepository _yemekhaneRepo;

        public KisiService(IKisiRepository kisiRepo, IPuantajsizKartRepository puantajsizKartRepo, IYemekhaneRepository yemekhaneRepo)
        {
            _kisiRepo = kisiRepo;
            _puantajsizKartRepo = puantajsizKartRepo;
            _yemekhaneRepo = yemekhaneRepo;
        }

        public void YeniKisiEkle(Kisi kisi, bool firmaPersoneli, bool puantajYapilabilir, bool yemekHakkiVar, int gunlukYemekLimiti, string puantajsizKartId, string puantajsizKartNo, string puantajsizKartAdi)
        {
            if (firmaPersoneli && puantajYapilabilir)
            {
                kisi.PuantajYapilirMi = true;
                _kisiRepo.Insert(kisi, puantajsizKartNo);
                if (yemekHakkiVar)
                    _yemekhaneRepo.InsertLimit(kisi.PersonelId, gunlukYemekLimiti);
                return;
            }

            if (firmaPersoneli && !puantajYapilabilir)
            {
                _puantajsizKartRepo.Insert(puantajsizKartId, puantajsizKartNo,
                              string.IsNullOrWhiteSpace(puantajsizKartAdi) ? (kisi.Ad + " " + kisi.Soyad).Trim() : puantajsizKartAdi,
                              kisi.FirmaId, kisi.CalismaSekli);
                if (yemekHakkiVar)
                    _yemekhaneRepo.InsertLimit(kisi.PersonelId, gunlukYemekLimiti);
                return;
            }

            if (!firmaPersoneli && !puantajYapilabilir && yemekHakkiVar)
            {
                kisi.PuantajYapilirMi = false;
                _kisiRepo.Insert(kisi, puantajsizKartNo);
                _yemekhaneRepo.InsertLimit(kisi.PersonelId, gunlukYemekLimiti);
                _puantajsizKartRepo.Insert(puantajsizKartId, puantajsizKartNo,
                              string.IsNullOrWhiteSpace(puantajsizKartAdi) ? (kisi.Ad + " " + kisi.Soyad).Trim() : puantajsizKartAdi,
                              kisi.FirmaId, kisi.CalismaSekli);
                return;
            }

            kisi.PuantajYapilirMi = false;
            _kisiRepo.Insert(kisi, puantajsizKartNo);
        }
        public bool KisiIstenCikar(string personelId, DateTime cikisTarihi, string firmaDisiKartNo)
        {
            try
            {
                _kisiRepo.SetIstenCikisTarihi(personelId, cikisTarihi);

                _yemekhaneRepo.PasifEtByPersonel(personelId);

                if (!string.IsNullOrWhiteSpace(firmaDisiKartNo))
                    _puantajsizKartRepo.PasifEtByKartId(firmaDisiKartNo);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool KisiGuncelle(Kisi kisi,string originalPersonelId,bool firmaPersoneli,bool puantajYapilabilir,bool yemekHakkiVar,int gunlukYemekAdedi,string firmaDisiKartNo,bool fotoDegisti)
        {
            try
            {
                var ok = _kisiRepo.Update(kisi, originalPersonelId, fotoDegisti, firmaDisiKartNo);
                if (!ok) return false;

                if (yemekHakkiVar && gunlukYemekAdedi > 0)
                    _yemekhaneRepo.UpsertLimit(kisi.PersonelId, gunlukYemekAdedi);
                else
                    _yemekhaneRepo.PasifEtByPersonel(kisi.PersonelId);

                if (firmaPersoneli && !puantajYapilabilir && !string.IsNullOrWhiteSpace(firmaDisiKartNo))
                {
                    var kartAdi = $"{kisi.Ad ?? ""} {kisi.Soyad ?? ""}".Trim();
                    _puantajsizKartRepo.UpsertByKartNo(
                        firmaDisiKartNo.Trim(),
                        kisi.FirmaId,
                        kartAdi,
                        kisi.CalismaSekli ?? ""
                    );
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(firmaDisiKartNo))
                        _puantajsizKartRepo.PasifEtByKartNo(firmaDisiKartNo.Trim());
                }

                return true;
            }
            catch
            {
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
