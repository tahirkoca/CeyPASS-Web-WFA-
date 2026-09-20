using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Entities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.Business.Services
{
    /// <summary>Misafir kart atamaları.</summary>
    public class MisafirKartService:IMisafirKartService
    {
        private readonly IKisiRepository _kisiRepo;
        private readonly IPuantajsizKartAtamaRepository _atamaRepo;
        private readonly IKisiHareketRepository _hareketRepo;

        public MisafirKartService(IKisiRepository kisiRepo, IPuantajsizKartAtamaRepository atamaRepo, IKisiHareketRepository hareketRepo)
        {
            _kisiRepo = kisiRepo;
            _atamaRepo= atamaRepo;
            _hareketRepo = hareketRepo;
        }

        /// <inheritdoc />
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
        /// <inheritdoc />
        public List<PuantajsizKartAtama> GetTodayActiveAssignments(DateTime now, int firmaId)
        {
            return _atamaRepo.GetTodayActive(now, firmaId, ziyaretciMi: true);
        }

        /// <inheritdoc />
        public List<PuantajsizKartAtama> GetOpenActiveAssignments(int firmaId)
        {
            return _atamaRepo.GetOpenActive(firmaId, ziyaretciMi: true);
        }

        /// <inheritdoc />
        public int CreateAssignment(int firmaId, string personelId, string misafirAdSoyad, DateTime girisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi, string pasaportNo)
        {
            if (string.IsNullOrWhiteSpace(misafirAdSoyad))
                throw new ArgumentException("Misafir adı soyadı boş olamaz.", nameof(misafirAdSoyad));

            var (tc, pasaport) = TcKimlikHelper.RequireTcOrPasaport(tcKimlikNo, pasaportNo);

            if (!_atamaRepo.CardBelongsToFirma(personelId, firmaId))
                throw new InvalidOperationException("Seçilen kart bu firmaya ait değil.");

            if (_atamaRepo.ExistsActiveForCard(personelId))
                throw new InvalidOperationException("Bu karta ait aktif bir atama zaten var. Önce çıkış veriniz.");

            var id = _atamaRepo.Insert(new PuantajsizKartAtama
            {
                KartId = personelId,
                MisafirAdSoyad = misafirAdSoyad.Trim(),
                TCKimlikNo = tc,
                PasaportNo = pasaport,
                ZiyaretEdilenKisi = string.IsNullOrWhiteSpace(ziyaretEdilenKisi) ? null : ziyaretEdilenKisi.Trim(),
                Baslangic = girisSaati,
                Bitis = null,
                Notlar = string.IsNullOrWhiteSpace(aciklama) ? "" : aciklama.Trim()
            });

            return id;
        }
        /// <inheritdoc />
        public void UpdateAssignment(int atamaId, string misafirAdSoyad, DateTime girisSaati, DateTime? cikisSaati, string aciklama, string tcKimlikNo, string ziyaretEdilenKisi, string pasaportNo)
        {
            var rec = _atamaRepo.GetById(atamaId);
            if (rec == null)
                throw new InvalidOperationException("Güncellenecek kayıt bulunamadı.");

            if (string.IsNullOrWhiteSpace(misafirAdSoyad))
                throw new ArgumentException("Misafir adı soyadı boş olamaz.", nameof(misafirAdSoyad));

            var (tc, pasaport) = TcKimlikHelper.RequireTcOrPasaport(tcKimlikNo, pasaportNo);

            rec.MisafirAdSoyad = misafirAdSoyad.Trim();
            rec.Baslangic = girisSaati;
            rec.Bitis = cikisSaati;
            rec.Notlar = string.IsNullOrWhiteSpace(aciklama) ? "" : aciklama.Trim();
            rec.TCKimlikNo = tc;
            rec.PasaportNo = pasaport;
            rec.ZiyaretEdilenKisi = string.IsNullOrWhiteSpace(ziyaretEdilenKisi) ? null : ziyaretEdilenKisi.Trim();

            _atamaRepo.Update(rec);
        }

        /// <inheritdoc />
        public PuantajsizKartAtama GetMisafirBilgisiByTc(string tcKimlikNo)
        {
            if (string.IsNullOrWhiteSpace(tcKimlikNo))
                return null;

            var tc = tcKimlikNo.Trim();
            if (TcKimlikHelper.LooksMasked(tc) || !TcKimlikHelper.IsValid(tc))
                return null;
            return _atamaRepo.GetSonAtamaByTcKimlikNo(tc);
        }

        /// <inheritdoc />
        public List<GecmisZiyaretciItem> SearchGecmisZiyaretciler(int firmaId, string adFilter)
        {
            return _atamaRepo.GetGecmisZiyaretciler(firmaId, adFilter, ziyaretciMi: true, aracKartiMi: null);
        }

        /// <inheritdoc />
        public List<KisiListItem> GetAktifKartlar(int firmaId)
            => _kisiRepo.GetAktifByFirma(firmaId, null, puantajYapilirMi: false, isyeriId: null, ziyaretciMi: true)
               ?? new List<KisiListItem>();

        /// <inheritdoc />
        public List<KartAtamaListeItem> GetAtamaListe(int firmaId, IReadOnlyList<KisiListItem>? cachedKartlar = null)
        {
            var kartlar = cachedKartlar != null
                ? cachedKartlar.ToList()
                : GetAktifKartlar(firmaId);
            var open = _atamaRepo.GetOpenActive(firmaId, ziyaretciMi: true) ?? new List<PuantajsizKartAtama>();
            var openIds = open
                .Where(a => !string.IsNullOrWhiteSpace(a.KartId))
                .Select(a => a.KartId!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var yonler = openIds.Count == 0
                ? new List<PersonelSonHareketYon>()
                : _hareketRepo.GetLastGirisMiByPersonelIds(openIds);
            return KartAtamaListeBuilder.Build(kartlar, open, KartAtamaListeBuilder.ToDictionary(yonler));
        }
    }
}
