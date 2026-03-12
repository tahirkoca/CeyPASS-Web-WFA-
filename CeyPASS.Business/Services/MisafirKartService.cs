using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class MisafirKartService:IMisafirKartService
    {
        private readonly IKisiRepository _kisiRepo;
        private readonly IPuantajsizKartAtamaRepository _atamaRepo;

        public MisafirKartService(IKisiRepository kisiRepo, IPuantajsizKartAtamaRepository atamaRepo)
        {
            _kisiRepo = kisiRepo;
            _atamaRepo= atamaRepo;
        }

        public List<KisiListItem> GetCardsForNew(int firmaId)
        {
            // Misafire atanacak kartlar: sadece ZiyaretciMi=1 (ziyaretçi kartı) olanlar
            var tumKartlar = _kisiRepo.GetAktifByFirma(firmaId, null, puantajYapilirMi: false, isyeriId: null, ziyaretciMi: true);
            var sonuc = new List<KisiListItem>();
            foreach (var k in tumKartlar)
            {
                if (string.IsNullOrWhiteSpace(k.PersonelId)) continue;
                if (_atamaRepo.ExistsActiveForCard(k.PersonelId))
                    continue;
                sonuc.Add(k);
            }
            return sonuc;
        }
        public List<PuantajsizKartAtama> GetTodayActiveAssignments(DateTime now, int firmaId)
        {
            return _atamaRepo.GetTodayActive(now, firmaId);
        }
        public int CreateAssignment(int firmaId, string personelId, string misafirAdSoyad, DateTime girisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi)
        {
            if (string.IsNullOrWhiteSpace(misafirAdSoyad))
                throw new ArgumentException("Misafir adı soyadı boş olamaz.", nameof(misafirAdSoyad));

            if (!_atamaRepo.CardBelongsToFirma(personelId, firmaId))
                throw new InvalidOperationException("Seçilen kart bu firmaya ait değil.");

            if (_atamaRepo.ExistsActiveForCard(personelId))
                throw new InvalidOperationException("Bu karta ait aktif bir atama zaten var. Önce çıkış veriniz.");

            var tc = string.IsNullOrWhiteSpace(tcKimlikNo) ? null : tcKimlikNo.Trim();
            // TC'yi sadece boş değilse kaydediyoruz, 11 karakter kontrolü gerekirse eklenebilir.

            var id = _atamaRepo.Insert(new PuantajsizKartAtama
            {
                KartId = personelId,
                MisafirAdSoyad = misafirAdSoyad.Trim(),
                TCKimlikNo = tc,
                ZiyaretEdilenKisi = string.IsNullOrWhiteSpace(ziyaretEdilenKisi) ? null : ziyaretEdilenKisi.Trim(),
                Baslangic = girisSaati,
                Bitis = null,
                Notlar = string.IsNullOrWhiteSpace(aciklama) ? "" : aciklama.Trim()
            });

            return id;
        }
        public void UpdateAssignment(int atamaId, string misafirAdSoyad, DateTime girisSaati, DateTime? cikisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi)
        {
            var rec = _atamaRepo.GetById(atamaId);
            if (rec == null)
                throw new InvalidOperationException("Güncellenecek kayıt bulunamadı.");

            if (string.IsNullOrWhiteSpace(misafirAdSoyad))
                throw new ArgumentException("Misafir adı soyadı boş olamaz.", nameof(misafirAdSoyad));

            rec.MisafirAdSoyad = misafirAdSoyad.Trim();
            rec.Baslangic = girisSaati;
            rec.Bitis = cikisSaati;
            rec.Notlar = string.IsNullOrWhiteSpace(aciklama) ? "" : aciklama.Trim();
            rec.TCKimlikNo = string.IsNullOrWhiteSpace(tcKimlikNo) ? null : tcKimlikNo.Trim();
            rec.ZiyaretEdilenKisi = string.IsNullOrWhiteSpace(ziyaretEdilenKisi) ? null : ziyaretEdilenKisi.Trim();

            _atamaRepo.Update(rec);
        }

        public PuantajsizKartAtama GetMisafirBilgisiByTc(string tcKimlikNo)
        {
            if (string.IsNullOrWhiteSpace(tcKimlikNo))
                return null;

            var tc = tcKimlikNo.Trim();
            return _atamaRepo.GetSonAtamaByTcKimlikNo(tc);
        }
    }
}
