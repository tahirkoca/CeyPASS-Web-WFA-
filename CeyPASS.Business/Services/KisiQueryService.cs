using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.Business.Services
{
    public class KisiQueryService : IKisiQueryService
    {
        private readonly IKisiRepository _repo;
        private readonly IPuantajsizKartRepository _puantajsizKartRepo;

        public KisiQueryService(IKisiRepository repo, IPuantajsizKartRepository puantajsizKartRepo)
        {
            _repo = repo;
            _puantajsizKartRepo = puantajsizKartRepo;
        }

        public List<KisiListItem> GetAktifKisilerByFirma(int firmId, string? search = null, bool? puantajYapilirMi = true, int? isyeriId = null)
        {
            if (puantajYapilirMi != false)
                return _repo.GetAktifByFirma(firmId, search, puantajYapilirMi, isyeriId);

            var kisiList = _repo.GetAktifByFirma(firmId, search, false, isyeriId);
            var puantajsizKartlar = _puantajsizKartRepo.GetByFirmaOrderByName(firmaId: firmId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLowerInvariant();
                puantajsizKartlar = puantajsizKartlar
                    .Where(p => (p.KartAdi ?? "").ToLowerInvariant().Contains(search) ||
                                (p.KartNo ?? "").ToLowerInvariant().Contains(search) ||
                                (p.KartId ?? "").ToLowerInvariant().Contains(search))
                    .ToList();
            }

            var kisiPersonelIds = new HashSet<string>(kisiList.Select(k => k.PersonelId), StringComparer.OrdinalIgnoreCase);
            var ekstra = puantajsizKartlar
                .Where(p => !kisiPersonelIds.Contains(p.KartId ?? ""))
                .Select(p => new KisiListItem { PersonelId = p.KartId ?? "", AdSoyad = p.KartAdi ?? "" })
                .ToList();

            return kisiList
                .Concat(ekstra)
                .OrderBy(x => x.AdSoyad, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        public KisiDetay GetKisiDetay(string personelId)
        {
            return _repo.GetDetay(personelId);
        }

        public (KisiDetay? detay, bool isPuantajsizKart) GetDetayOrPuantajsizKart(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return (null, false);

            var kisi = _repo.GetDetay(id);
            if (kisi != null)
                return (kisi, false);

            var puantajsiz = _puantajsizKartRepo.GetByKartId(id);
            if (puantajsiz == null)
                return (null, false);

            var detay = new KisiDetay
            {
                PersonelId = puantajsiz.KartId,
                Ad = puantajsiz.KartAdi ?? "",
                Soyad = "",
                KartNo = puantajsiz.KartNo,
                FirmaId = puantajsiz.FirmaId ?? 0,
                FirmaPersoneli = false,
                PuantajYapilabilir = false,
                YemekHakkiVar = false,
                CalismaSekliCsv = puantajsiz.CalismaSekli
            };
            return (detay, true);
        }
    }
}
