using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Entities.Helpers;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class AracKartiService : IAracKartiService
    {
        private readonly IKisiRepository _kisiRepo;
        private readonly IPuantajsizKartAtamaRepository _atamaRepo;

        public AracKartiService(IKisiRepository kisiRepo, IPuantajsizKartAtamaRepository atamaRepo)
        {
            _kisiRepo = kisiRepo;
            _atamaRepo = atamaRepo;
        }

        public List<KisiListItem> GetCardsForNew(int firmaId)
        {
            var tumKartlar = _kisiRepo.GetAktifByFirma(firmaId, null, puantajYapilirMi: false, isyeriId: null, aracKartiMi: true);
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
            return _atamaRepo.GetTodayActive(now, firmaId, aracKartiMi: true);
        }

        public int CreateAssignment(int firmaId, string personelId, string adSoyad, DateTime girisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi, string plaka)
        {
            if (string.IsNullOrWhiteSpace(adSoyad))
                throw new ArgumentException("Ad soyad boş olamaz.", nameof(adSoyad));

            var tc = TcKimlikHelper.RequireValid(tcKimlikNo);

            if (string.IsNullOrWhiteSpace(plaka))
                throw new ArgumentException("Plaka giriniz.", nameof(plaka));

            if (!_atamaRepo.CardBelongsToFirma(personelId, firmaId))
                throw new InvalidOperationException("Seçilen kart bu firmaya ait değil.");

            if (_atamaRepo.ExistsActiveForCard(personelId))
                throw new InvalidOperationException("Bu karta ait aktif bir atama zaten var. Önce çıkış veriniz.");

            return _atamaRepo.Insert(new PuantajsizKartAtama
            {
                KartId = personelId,
                MisafirAdSoyad = adSoyad.Trim(),
                TCKimlikNo = tc,
                ZiyaretEdilenKisi = string.IsNullOrWhiteSpace(ziyaretEdilenKisi) ? null : ziyaretEdilenKisi.Trim(),
                Baslangic = girisSaati,
                Bitis = null,
                Notlar = string.IsNullOrWhiteSpace(aciklama) ? "" : aciklama.Trim(),
                Plaka = plaka.Trim().ToUpperInvariant()
            });
        }

        public void UpdateAssignment(int atamaId, string adSoyad, DateTime girisSaati, DateTime? cikisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi, string plaka)
        {
            var rec = _atamaRepo.GetById(atamaId);
            if (rec == null)
                throw new InvalidOperationException("Güncellenecek kayıt bulunamadı.");

            if (string.IsNullOrWhiteSpace(adSoyad))
                throw new ArgumentException("Ad soyad boş olamaz.", nameof(adSoyad));

            if (string.IsNullOrWhiteSpace(plaka))
                throw new ArgumentException("Plaka giriniz.", nameof(plaka));

            rec.MisafirAdSoyad = adSoyad.Trim();
            rec.Baslangic = girisSaati;
            rec.Bitis = cikisSaati;
            rec.Notlar = string.IsNullOrWhiteSpace(aciklama) ? "" : aciklama.Trim();
            rec.TCKimlikNo = TcKimlikHelper.RequireValid(tcKimlikNo);
            rec.ZiyaretEdilenKisi = string.IsNullOrWhiteSpace(ziyaretEdilenKisi) ? null : ziyaretEdilenKisi.Trim();
            rec.Plaka = plaka.Trim().ToUpperInvariant();

            _atamaRepo.Update(rec);
        }

        public PuantajsizKartAtama GetBilgisiByTc(string tcKimlikNo)
        {
            if (string.IsNullOrWhiteSpace(tcKimlikNo))
                return null;

            var tc = tcKimlikNo.Trim();
            if (TcKimlikHelper.LooksMasked(tc) || !TcKimlikHelper.IsValid(tc))
                return null;

            return _atamaRepo.GetSonAtamaByTcKimlikNo(tc);
        }

        public List<GecmisZiyaretciItem> SearchGecmisZiyaretciler(int firmaId, string adFilter)
        {
            return _atamaRepo.GetGecmisZiyaretciler(firmaId, adFilter, ziyaretciMi: null, aracKartiMi: true);
        }
    }
}
