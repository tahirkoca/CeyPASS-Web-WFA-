using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class MisafirKartService:IMisafirKartService
    {
        private readonly IPuantajsizKartRepository _kartRepo;
        private readonly IPuantajsizKartAtamaRepository _atamaRepo;

        public MisafirKartService(IPuantajsizKartRepository kartRepo, IPuantajsizKartAtamaRepository atamaRepo)
        {
            _kartRepo = kartRepo;
            _atamaRepo= atamaRepo;
        }

        public List<PuantajsizKart> GetCardsForNew(int firmaId)
        {
            var tumKartlar = _kartRepo.GetByFirmaOrderByName(firmaId);
            var sonuc = new List<PuantajsizKart>();
            foreach (var k in tumKartlar)
            {
                if (string.IsNullOrWhiteSpace(k.KartId)) continue;
                if (!int.TryParse(k.KartId, out int kartIdInt))
                    continue;
                if (_atamaRepo.ExistsActiveForCard(kartIdInt))
                    continue;
                sonuc.Add(k);
            }
            return sonuc;
        }
        public List<PuantajsizKartAtama> GetTodayActiveAssignments(DateTime now, int firmaId)
        {
            return _atamaRepo.GetTodayActive(now, firmaId);
        }
        public int CreateAssignment(int firmaId, int kartId, string misafirAdSoyad, DateTime girisSaati, string aciklama)
        {
            if (string.IsNullOrWhiteSpace(misafirAdSoyad))
                throw new ArgumentException("Misafir adı soyadı boş olamaz.", nameof(misafirAdSoyad));

            if (!_atamaRepo.CardBelongsToFirma(kartId, firmaId))
                throw new InvalidOperationException("Seçilen kart bu firmaya ait değil.");

            if (_atamaRepo.ExistsActiveForCard(kartId))
                throw new InvalidOperationException("Bu karta ait aktif bir atama zaten var. Önce çıkış veriniz.");

            var id = _atamaRepo.Insert(new PuantajsizKartAtama
            {
                KartId = Convert.ToString(kartId),
                MisafirAdSoyad = misafirAdSoyad.Trim(),
                Baslangic = girisSaati,
                Bitis = null,
                Notlar = string.IsNullOrWhiteSpace(aciklama) ? "" : aciklama.Trim()
            });

            return id;
        }
        public void UpdateAssignment(int atamaId, string misafirAdSoyad, DateTime girisSaati, DateTime? cikisSaati, string aciklama)
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

            _atamaRepo.Update(rec);
        }
    }
}
