using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.Web.Controllers
{
    public class KisiHareketController : Controller
    {
        private readonly IKisiHareketService _kisiHareketService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IFirmaService _firmaService;
        private readonly IKisiEkraniLookUpService _lookupService;
        private readonly IPuantajService _puantajService;
        private readonly IMemoryCache _cache;
        private const string PageName = "KisiHareketler";
        private const int DefaultPageSize = 50;
        private static readonly int[] AllowedPageSizes = new[] { 20, 50, 100, 200 };
        private const string CacheVerPrefix = "kisihareket_ver_";
        private const string CacheVerScopeAllFirms = "allfirms";

        public KisiHareketController(
            IKisiHareketService kisiHareketService,
            IKisiQueryService kisiQueryService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IFirmaService firmaService,
            IKisiEkraniLookUpService lookupService,
            IPuantajService puantajService,
            IMemoryCache cache)
        {
            _kisiHareketService = kisiHareketService;
            _kisiQueryService = kisiQueryService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _firmaService = firmaService;
            _lookupService = lookupService;
            _puantajService = puantajService;
            _cache = cache;
        }

        public IActionResult Index(int? firmaId = null, int? isyeriId = null, string personelIds = null, DateTime? baslangic = null, DateTime? bitis = null, bool? sadeceAktif = null, bool? sadecePasif = null, bool? sadeceYemekhane = null, string kartTipi = null, int page = 1, int pageSize = DefaultPageSize)
        {
            // Check authorization
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Kişi Hareketler ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            if (page < 1) page = 1;
            if (!AllowedPageSizes.Contains(pageSize)) pageSize = DefaultPageSize;

            // Determine firma
            int selectedFirmaId = firmaId ?? (int)_sessionContext.AktifFirmaId;
            bool isAdmin = _sessionContext.RolId == 1 || _sessionContext.RolId == 2;
            if (!isAdmin && selectedFirmaId != _sessionContext.AktifFirmaId)
            {
                selectedFirmaId = (int)_sessionContext.AktifFirmaId;
            }

            // Default tarih aralığı: Bugün
            DateTime baslangicTarih = baslangic ?? DateTime.Today;
            DateTime bitisTarih = bitis ?? DateTime.Today.AddDays(1).AddMinutes(-1);

            // Kart tipi: puantajsiz = Puantaj Yapılmayanlar, aksi halde Puantaj Yapılanlar
            bool puantajYapilir = kartTipi != "puantajsiz";
            var personelList = GetPersonelList(selectedFirmaId, puantajYapilir, isyeriId);

            // Seçili personel ID'leri
            List<int> seciliPersonelIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(personelIds))
            {
                seciliPersonelIds = personelIds.Split(',')
                    .Where(x => int.TryParse(x.Trim(), out _))
                    .Select(int.Parse)
                    .ToList();
            }

            // Hareketleri yükle (eğer personel seçilmişse)
            int totalCount = 0;
            List<KisiHareketListRow> hareketler = new List<KisiHareketListRow>();
            if (seciliPersonelIds.Any())
            {
                var personelKeyPart = string.Join(",", seciliPersonelIds.OrderBy(x => x));
                var verKey = CacheVerPrefix + CacheVerScopeAllFirms;
                if (!_cache.TryGetValue(verKey, out int ver))
                {
                    ver = 0;
                    _cache.Set(verKey, ver, TimeSpan.FromHours(1));
                }

                var isyeriSeg = IsyeriFilterCacheSegment(isyeriId, null);
                var cacheKey = $"kisihareket_{CacheVerScopeAllFirms}_v{ver}_{kartTipi}_{isyeriSeg}_{personelKeyPart}_{baslangicTarih:yyyyMMddHHmmss}_{bitisTarih:yyyyMMddHHmmss}_{(sadeceAktif ?? false)}_{(sadecePasif ?? false)}_{(sadeceYemekhane ?? false)}_p{page}_s{pageSize}";
                if (!_cache.TryGetValue(cacheKey, out KisiHareketCacheValue cached))
                {
                    var items = _kisiHareketService.GetByPersonsPaged(
                        seciliPersonelIds,
                        baslangicTarih,
                        bitisTarih,
                        sadeceAktif ?? false,
                        sadecePasif ?? false,
                        sadeceYemekhane ?? false,
                        selectedFirmaId,
                        page,
                        pageSize,
                        out totalCount
                    );
                    cached = new KisiHareketCacheValue(items, totalCount);
                    _cache.Set(cacheKey, cached, TimeSpan.FromMinutes(2));
                }

                hareketler = cached.Items;
                totalCount = cached.TotalCount;
            }

            // Firmalar (admin için)
            var firmalar = isAdmin ? _firmaService.GetAll().OrderBy(f => f.FirmaAdi).ToList() : null;

            ViewBag.SelectedFirmaId = selectedFirmaId;
            ViewBag.SelectedIsyeriId = isyeriId;
            ViewBag.Isyerleri = GetYetkiliIsyeriLookups(selectedFirmaId);
            ViewBag.Firmalar = firmalar;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.PersonelList = personelList;
            ViewBag.SeciliPersonelIds = seciliPersonelIds;
            ViewBag.BaslangicTarih = baslangicTarih;
            ViewBag.BitisTarih = bitisTarih;
            ViewBag.SadeceAktif = sadeceAktif ?? false;
            ViewBag.SadecePasif = sadecePasif ?? false;
            ViewBag.SadeceYemekhane = sadeceYemekhane ?? false;
            ViewBag.KartTipi = kartTipi ?? "puantaj";
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
            ViewBag.CanCreate = _authorizationService.Can(PageName, YetkiTipleri.Create);
            ViewBag.CanUpdate = _authorizationService.Can(PageName, YetkiTipleri.Update);
            ViewBag.CanDelete = _authorizationService.Can(PageName, YetkiTipleri.Delete);

            return View(hareketler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Ekle(int firmaId, int personelId, DateTime tarih, string tip)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Hareket ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                bool success = _kisiHareketService.InsertManual(firmaId, personelId, tarih, tip);
                if (success)
                {
                    TempData["Success"] = "Hareket başarıyla eklendi.";
                    BumpVer(firmaId);
                }
                else
                {
                    TempData["Error"] = "Hareket eklenemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guncelle(int id, DateTime tarih, string tip)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Hareket güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                bool success = _kisiHareketService.UpdateManual(id, tarih, tip);
                if (success)
                {
                    TempData["Success"] = "Hareket başarıyla güncellendi.";
                    BumpVer((int)_sessionContext.AktifFirmaId);
                }
                else
                {
                    TempData["Error"] = "Hareket güncellenemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PasifYap(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete))
            {
                TempData["Error"] = "Hareket silme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                bool success = _kisiHareketService.PasifYap(id);
                if (success)
                {
                    TempData["Success"] = "Hareket pasif yapıldı.";
                    BumpVer((int)_sessionContext.AktifFirmaId);
                }
                else
                {
                    TempData["Error"] = "Hareket pasif yapılamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AktifYap(int id)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete))
            {
                TempData["Error"] = "Hareket aktifleştirme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                bool success = _kisiHareketService.AktifYap(id);
                if (success)
                {
                    TempData["Success"] = "Hareket tekrar aktif edildi.";
                    BumpVer((int)_sessionContext.AktifFirmaId);
                }
                else
                {
                    TempData["Error"] = "Hareket aktifleştirilemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetIsyerleri(int firmaId)
        {
            return Json(GetYetkiliIsyeriLookups(firmaId));
        }

        private List<LookupItem> GetYetkiliIsyeriLookups(int firmaId)
        {
            bool isAdmin = _sessionContext.IsAdmin();
            List<FirmaIsyeriYetkiDTO> yetkiler = null;
            if (!isAdmin && _sessionContext.AktifKullaniciId.HasValue)
                yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
            return FirmaIsyeriYetkiHelper.FilterIsyeriLookup(
                _lookupService.GetIsyerleri(firmaId) ?? new List<LookupItem>(),
                firmaId,
                yetkiler,
                isAdmin);
        }

        private List<PersonelLookupItem> GetPersonelList(int firmaId, bool puantajYapilir = true, int? selectedIsyeriId = null)
        {
            var list = new List<PersonelLookupItem>();
            try
            {
                bool isAdmin = _sessionContext.IsAdmin();
                List<FirmaIsyeriYetkiDTO> yetkiler = null;
                if (!isAdmin && _sessionContext.AktifKullaniciId.HasValue)
                    yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
                var (single, idIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
                    firmaId, selectedIsyeriId, yetkiler, isAdmin);
                var kisiler = _kisiQueryService.GetAktifKisilerByFirma(firmaId, null, puantajYapilir, single, idIn)
                    ?? new List<KisiListItem>();
                foreach (var k in kisiler)
                {
                    if (string.IsNullOrWhiteSpace(k.PersonelId) || string.IsNullOrWhiteSpace(k.AdSoyad))
                        continue;
                    if (!int.TryParse(k.PersonelId, out int id) || id <= 0)
                        continue;
                    list.Add(new PersonelLookupItem { Id = id, Ad = k.AdSoyad });
                }
            }
            catch (Exception)
            {
                // ignore lookup errors
            }

            return list;
        }

        private static string IsyeriFilterCacheSegment(int? isyeriId, IReadOnlyList<int> isyeriIdIn)
        {
            if (isyeriId.HasValue) return isyeriId.Value.ToString();
            if (isyeriIdIn != null && isyeriIdIn.Count > 0)
                return "in_" + string.Join("_", isyeriIdIn.OrderBy(x => x));
            if (isyeriIdIn != null) return "in_none";
            return "all";
        }

        private void BumpVer(int firmaId)
        {
            var key = CacheVerPrefix + CacheVerScopeAllFirms;
            if (!_cache.TryGetValue(key, out int ver)) ver = 0;
            ver++;
            _cache.Set(key, ver, TimeSpan.FromHours(1));
        }

        private sealed class KisiHareketCacheValue
        {
            public KisiHareketCacheValue(List<KisiHareketListRow> items, int totalCount)
            {
                Items = items ?? new List<KisiHareketListRow>();
                TotalCount = totalCount;
            }
            public List<KisiHareketListRow> Items { get; }
            public int TotalCount { get; }
        }
    }

    // Helper class for personel lookup
    public class PersonelLookupItem
    {
        public int Id { get; set; }
        public string Ad { get; set; }
    }
}
