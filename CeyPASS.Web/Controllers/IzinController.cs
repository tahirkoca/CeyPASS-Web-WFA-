using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.Web.Controllers
{
    public class IzinController : Controller
    {
        private readonly IKisiIzinService _kisiIzinService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly IIzinTipService _izinTipService;
        private readonly IIzinTalepService _izinTalepService;
        private readonly IFirmaService _firmaService;
        private readonly IPuantajService _puantajService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMemoryCache _cache;
        private const string PageName = "Izinler";
        private const string TalepPageName = "IzinTalepleri";
        private const int DefaultPageSize = 50;
        private static readonly int[] AllowedPageSizes = new[] { 20, 50, 100, 200 };
        private const string CacheVerPrefix = "izin_ver_";

        public IzinController(
            IKisiIzinService kisiIzinService,
            IKisiQueryService kisiQueryService,
            IIzinTipService izinTipService,
            IIzinTalepService izinTalepService,
            IFirmaService firmaService,
            IPuantajService puantajService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IMemoryCache cache)
        {
            _kisiIzinService = kisiIzinService;
            _kisiQueryService = kisiQueryService;
            _izinTipService = izinTipService;
            _izinTalepService = izinTalepService;
            _firmaService = firmaService;
            _puantajService = puantajService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _cache = cache;
        }

        public IActionResult Index(string personelId = null, int? izinTipId = null, DateTime? baslangic = null, DateTime? bitis = null, int page = 1, int pageSize = DefaultPageSize)
        {
            // Check authorization
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "İzinler ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            if (page < 1) page = 1;
            if (!AllowedPageSizes.Contains(pageSize)) pageSize = DefaultPageSize;

            // Firma ID navbar'dan seçilen aktif firmadan alınır
            int selectedFirmaId = (int)_sessionContext.AktifFirmaId;

            // Default tarih aralığı: Bu ay
            DateTime baslangicTarih = baslangic ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime bitisTarih = bitis ?? baslangicTarih.AddMonths(1).AddDays(-1);

            // İzinleri yükle (paged + cache)
            int totalCount = 0;
            var verKey = CacheVerPrefix + selectedFirmaId;
            if (!_cache.TryGetValue(verKey, out int ver))
            {
                ver = 0;
                _cache.Set(verKey, ver, TimeSpan.FromHours(1));
            }

            var personelKey = personelId == "ALL" ? "" : (personelId ?? "");
            var izinKey = izinTipId == 0 ? "" : (izinTipId?.ToString() ?? "");
            var cacheKey = $"izin_{selectedFirmaId}_v{ver}_{personelKey}_{izinKey}_{baslangicTarih:yyyyMMdd}_{bitisTarih:yyyyMMdd}_p{page}_s{pageSize}";
            if (!_cache.TryGetValue(cacheKey, out IzinCacheValue cached))
            {
                var items = _kisiIzinService.GetTumIzinlerPaged(
                    selectedFirmaId,
                    personelId == "ALL" ? null : personelId,
                    izinTipId == 0 ? (int?)null : izinTipId,
                    baslangicTarih,
                    bitisTarih,
                    page,
                    pageSize,
                    out totalCount
                );
                cached = new IzinCacheValue(items, totalCount);
                _cache.Set(cacheKey, cached, TimeSpan.FromMinutes(2));
            }

            var izinler = cached.Items;
            totalCount = cached.TotalCount;

            // Lookup data
            var kisiler = GetYetkiliKisilerForFirma(selectedFirmaId);
            var izinTipleri = _izinTipService.GetAktif();

            ViewBag.Kisiler = kisiler;
            ViewBag.IzinTipleri = izinTipleri;
            ViewBag.SelectedPersonelId = personelId;
            ViewBag.SelectedIzinTipId = izinTipId;
            ViewBag.BaslangicTarih = baslangicTarih;
            ViewBag.BitisTarih = bitisTarih;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
            ViewBag.CanCreate = _authorizationService.Can(PageName, YetkiTipleri.Create);
            ViewBag.CanUpdate = _authorizationService.Can(PageName, YetkiTipleri.Update);
            ViewBag.CanDelete = _authorizationService.Can(PageName, YetkiTipleri.Delete);

            return View(izinler);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "İzin ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var kullaniciYetkileri = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
            var firmaYetkileri = kullaniciYetkileri.Select(y => y.FirmaId).Distinct().ToHashSet();
            
            var model = new KisiIzin
            {
                FirmaId = (int)_sessionContext.AktifFirmaId,
                Baslangic = DateTime.Today,
                Bitis = DateTime.Today,
                SaatlikIzinMi = false
            };

            ViewBag.Firmalar = GetAuthorizedFirmalar(firmaYetkileri);
            ViewBag.Kisiler = GetYetkiliKisilerForFirma(model.FirmaId);
            ViewBag.IzinTipleri = _izinTipService.GetAktif();
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(KisiIzin izin, TimeSpan? baslangicSaati, TimeSpan? bitisSaati)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "İzin ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            // Validation
            var validationDto = new IzinKayitValidasyonDTO
            {
                SaatlikIzinMi = izin.SaatlikIzinMi,
                PersonelId = izin.PersonelId,
                IzinTipId = izin.IzinId,
                BaslangicTarihi = izin.Baslangic,
                BitisTarihi = izin.Bitis,
                BaslangicSaati = baslangicSaati,
                BitisSaati = bitisSaati
            };

            var validation = _kisiIzinService.ValidateKayit(validationDto);
            if (!validation.IsValid)
            {
                ModelState.AddModelError("", validation.Message);
                var kullaniciYetkileri = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
                var firmaYetkileri = kullaniciYetkileri.Select(y => y.FirmaId).Distinct().ToHashSet();
                ViewBag.Firmalar = GetAuthorizedFirmalar(firmaYetkileri);
                ViewBag.Kisiler = GetYetkiliKisilerForFirma(izin.FirmaId);
                ViewBag.IzinTipleri = _izinTipService.GetAktif();
                return View(izin);
            }

            // Saatlik izin için saatleri ekle
            if (izin.SaatlikIzinMi && baslangicSaati.HasValue && bitisSaati.HasValue)
            {
                izin.Baslangic = izin.Baslangic.Date.Add(baslangicSaati.Value);
                izin.Bitis = izin.Bitis.Date.Add(bitisSaati.Value);
            }

            izin.OlusturanKullaniciId = (int)_sessionContext.AktifKullaniciId;

            try
            {
                bool success = _kisiIzinService.Ekle(izin);
                if (success)
                {
                    TempData["Success"] = "İzin başarıyla eklendi.";
                    BumpVer(izin.FirmaId);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "İzin eklenemedi.");
                    var kullaniciYetkileri = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
                    var firmaYetkileri = kullaniciYetkileri.Select(y => y.FirmaId).Distinct().ToHashSet();
                    ViewBag.Firmalar = GetAuthorizedFirmalar(firmaYetkileri);
                    ViewBag.Kisiler = GetYetkiliKisilerForFirma(izin.FirmaId);
                    ViewBag.IzinTipleri = _izinTipService.GetAktif();
                    return View(izin);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "İzin eklenirken bir hata oluştu: " + ex.Message);
                var kullaniciYetkileri = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
                var firmaYetkileri = kullaniciYetkileri.Select(y => y.FirmaId).Distinct().ToHashSet();
                ViewBag.Firmalar = GetAuthorizedFirmalar(firmaYetkileri);
                ViewBag.Kisiler = GetYetkiliKisilerForFirma(izin.FirmaId);
                ViewBag.IzinTipleri = _izinTipService.GetAktif();
                return View(izin);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "İzin güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var izin = _kisiIzinService.GetById(id);
            if (izin == null)
            {
                TempData["Error"] = "İzin bulunamadı.";
                return RedirectToAction("Index");
            }

            var kullaniciYetkileri = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
            var firmaYetkileri = kullaniciYetkileri.Select(y => y.FirmaId).Distinct().ToHashSet();
            ViewBag.Firmalar = GetAuthorizedFirmalar(firmaYetkileri);
            ViewBag.Kisiler = GetYetkiliKisilerForFirma(izin.FirmaId);
            ViewBag.IzinTipleri = _izinTipService.GetAktif();
            return View(izin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(KisiIzin izin, TimeSpan? baslangicSaati, TimeSpan? bitisSaati)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "İzin güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var mevcut = _kisiIzinService.GetById(izin.KisiIzinId ?? 0);
            if (mevcut == null)
            {
                TempData["Error"] = "İzin bulunamadı.";
                return RedirectToAction("Index");
            }

            var validationDto = new IzinKayitValidasyonDTO
            {
                SaatlikIzinMi = izin.SaatlikIzinMi,
                PersonelId = mevcut.PersonelId,
                IzinTipId = izin.IzinId,
                BaslangicTarihi = izin.Baslangic,
                BitisTarihi = izin.Bitis,
                BaslangicSaati = baslangicSaati,
                BitisSaati = bitisSaati
            };
            var validation = _kisiIzinService.ValidateKayit(validationDto);
            if (!validation.IsValid)
            {
                ModelState.AddModelError("", validation.Message);
                izin.FirmaId = mevcut.FirmaId;
                izin.PersonelId = mevcut.PersonelId;
                izin.KisiIzinId = mevcut.KisiIzinId;
                var kullaniciYetkileri = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
                var firmaYetkileri = kullaniciYetkileri.Select(y => y.FirmaId).Distinct().ToHashSet();
                ViewBag.Firmalar = GetAuthorizedFirmalar(firmaYetkileri);
                ViewBag.Kisiler = GetYetkiliKisilerForFirma(mevcut.FirmaId);
                ViewBag.IzinTipleri = _izinTipService.GetAktif();
                return View(izin);
            }

            if (izin.SaatlikIzinMi && baslangicSaati.HasValue && bitisSaati.HasValue)
            {
                izin.Baslangic = izin.Baslangic.Date.Add(baslangicSaati.Value);
                izin.Bitis = izin.Bitis.Date.Add(bitisSaati.Value);
            }

            izin.FirmaId = mevcut.FirmaId;
            izin.PersonelId = mevcut.PersonelId;
            izin.OlusturanKullaniciId = mevcut.OlusturanKullaniciId;
            izin.SureDakika = mevcut.SureDakika;

            try
            {
                bool success = _kisiIzinService.Guncelle(izin);
                if (success)
                {
                    TempData["Success"] = "İzin başarıyla güncellendi.";
                    BumpVer(izin.FirmaId);
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "İzin güncellenemedi.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "İzin güncellenirken hata: " + ex.Message);
            }

            izin.FirmaId = mevcut.FirmaId;
            izin.PersonelId = mevcut.PersonelId;
            izin.KisiIzinId = mevcut.KisiIzinId;
            var k2 = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
            var f2 = k2.Select(y => y.FirmaId).Distinct().ToHashSet();
            ViewBag.Firmalar = GetAuthorizedFirmalar(f2);
            ViewBag.Kisiler = GetYetkiliKisilerForFirma(izin.FirmaId);
            ViewBag.IzinTipleri = _izinTipService.GetAktif();
            return View(izin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete))
            {
                TempData["Error"] = "İzin silme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                bool success = _kisiIzinService.PasifYap(id);
                if (success)
                {
                    TempData["Success"] = "İzin başarıyla silindi.";
                    BumpVer((int)_sessionContext.AktifFirmaId);
                }
                else
                {
                    TempData["Error"] = "İzin silinemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private void BumpVer(int firmaId)
        {
            var key = CacheVerPrefix + firmaId;
            if (!_cache.TryGetValue(key, out int ver)) ver = 0;
            ver++;
            _cache.Set(key, ver, TimeSpan.FromHours(1));
        }

        private List<KisiListItem> GetYetkiliKisilerForFirma(int firmaId, int? selectedIsyeriId = null)
        {
            bool isAdmin = _sessionContext.IsAdmin();
            List<FirmaIsyeriYetkiDTO> yetkiler = null;
            if (!isAdmin && _sessionContext.AktifKullaniciId.HasValue)
                yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
            var (single, idIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
                firmaId, selectedIsyeriId, yetkiler, isAdmin);
            return _kisiQueryService.GetAktifKisilerByFirma(firmaId, isyeriId: single, isyeriIdIn: idIn);
        }

        private sealed class IzinCacheValue
        {
            public IzinCacheValue(List<KisiIzinListRow> items, int totalCount)
            {
                Items = items ?? new List<KisiIzinListRow>();
                TotalCount = totalCount;
            }
            public List<KisiIzinListRow> Items { get; }
            public int TotalCount { get; }
        }

        [HttpGet]
        public IActionResult GetKisiler(int firmaId)
        {
            var kisiler = GetYetkiliKisilerForFirma(firmaId);
            return Json(kisiler.Select(k => new { PersonelId = k.PersonelId, AdSoyad = k.AdSoyad }));
        }

        // ─── İzin Talepleri (Onay Mekanizması) ───────────────────────────────

        [HttpGet]
        public IActionResult TalepListesi()
        {
            if (!_authorizationService.ViewAbility(TalepPageName))
            {
                TempData["Error"] = "İzin talepleri ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            var items = _izinTalepService.IkBekleyenler();

            // Fetch names
            var pIds = items.Select(x => x.PersonelId).Distinct().ToList();
            var pNames = new Dictionary<string, string>();
            foreach (var id in pIds)
            {
                var k = _kisiQueryService.GetKisiDetay(id);
                if (k != null) pNames[id] = $"{k.Ad} {k.Soyad}";
            }

            var iTypes = _izinTipService.GetAktif().ToDictionary(x => x.IzinTipId, x => x.Ad);

            ViewBag.Personeller = pNames;
            ViewBag.IzinTipleri = iTypes;
            ViewBag.CanUpdate = _authorizationService.Can(TalepPageName, YetkiTipleri.Update);
            
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TalepOnayla(int talepId, string? aciklama)
        {
            if (!_authorizationService.Can(TalepPageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "İzin talebi onaylama yetkiniz yok.";
                return RedirectToAction(nameof(TalepListesi));
            }
            if (!_sessionContext.AktifKullaniciId.HasValue)
            {
                TempData["Error"] = "Oturum bilgisi bulunamadı.";
                return RedirectToAction(nameof(TalepListesi));
            }

            var ok = _izinTalepService.IkOnayla(talepId, _sessionContext.AktifKullaniciId.Value, aciklama);
            TempData[ok ? "Success" : "Error"] = ok ? "Talep onaylandı." : "Talep onaylanamadı.";
            return RedirectToAction(nameof(TalepListesi));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TalepReddet(int talepId, string? aciklama)
        {
            if (!_authorizationService.Can(TalepPageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "İzin talebi reddetme yetkiniz yok.";
                return RedirectToAction(nameof(TalepListesi));
            }
            if (!_sessionContext.AktifKullaniciId.HasValue)
            {
                TempData["Error"] = "Oturum bilgisi bulunamadı.";
                return RedirectToAction(nameof(TalepListesi));
            }

            var ok = _izinTalepService.IkReddet(talepId, _sessionContext.AktifKullaniciId.Value, aciklama);
            TempData[ok ? "Success" : "Error"] = ok ? "Talep reddedildi." : "Talep reddedilemedi.";
            return RedirectToAction(nameof(TalepListesi));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DonusImzasinaAc(int talepId)
        {
            if (!_authorizationService.Can(TalepPageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Dönüş imzasına açma yetkiniz yok.";
                return RedirectToAction(nameof(TalepListesi));
            }
            if (!_sessionContext.AktifKullaniciId.HasValue)
            {
                TempData["Error"] = "Oturum bilgisi bulunamadı.";
                return RedirectToAction(nameof(TalepListesi));
            }

            var ok = _izinTalepService.DonusImzasinaAc(talepId, _sessionContext.AktifKullaniciId.Value);
            TempData[ok ? "Success" : "Error"] = ok ? "Dönüş imzasına açıldı." : "İşlem başarısız.";
            return RedirectToAction(nameof(TalepListesi));
        }

        private List<Firma> GetAuthorizedFirmalar(HashSet<int> firmaYetkileri)
        {
            var firmalar = _firmaService.GetPuantajFirmalar();
            if (firmaYetkileri.Count > 0)
            {
                firmalar = firmalar.Where(f => firmaYetkileri.Contains(f.FirmaId)).ToList();
            }
            return firmalar.OrderBy(f => f.FirmaAdi).ToList();
        }
    }
}
