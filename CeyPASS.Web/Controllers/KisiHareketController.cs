using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
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
        private readonly IMemoryCache _cache;
        private const string PageName = "KisiHareketler";
        private const int DefaultPageSize = 50;
        private static readonly int[] AllowedPageSizes = new[] { 20, 50, 100, 200 };
        private const string CacheVerPrefix = "kisihareket_ver_";

        public KisiHareketController(
            IKisiHareketService kisiHareketService,
            IKisiQueryService kisiQueryService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IFirmaService firmaService,
            IMemoryCache cache)
        {
            _kisiHareketService = kisiHareketService;
            _kisiQueryService = kisiQueryService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _firmaService = firmaService;
            _cache = cache;
        }

        public IActionResult Index(int? firmaId = null, string personelIds = null, DateTime? baslangic = null, DateTime? bitis = null, bool? sadeceAktif = null, bool? sadecePasif = null, bool? sadeceYemekhane = null, string kartTipi = null, int page = 1, int pageSize = DefaultPageSize)
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
            var personelList = GetPersonelList(selectedFirmaId, puantajYapilir);

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
                var verKey = CacheVerPrefix + selectedFirmaId;
                if (!_cache.TryGetValue(verKey, out int ver))
                {
                    ver = 0;
                    _cache.Set(verKey, ver, TimeSpan.FromHours(1));
                }

                var cacheKey = $"kisihareket_{selectedFirmaId}_v{ver}_{kartTipi}_{personelKeyPart}_{baslangicTarih:yyyyMMddHHmmss}_{bitisTarih:yyyyMMddHHmmss}_{(sadeceAktif ?? false)}_{(sadecePasif ?? false)}_{(sadeceYemekhane ?? false)}_p{page}_s{pageSize}";
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

        private List<PersonelLookupItem> GetPersonelList(int firmaId, bool puantajYapilir = true)
        {
            var list = new List<PersonelLookupItem>();
            try
            {
                var dt = _kisiHareketService.GetAktifKisilerWithSicil(firmaId, puantajYapilir);
                if (dt != null)
                {
                    bool hasId = dt.Columns.Contains("PersonelId");
                    bool hasAdSoyad = dt.Columns.Contains("AdSoyad");

                    foreach (DataRow row in dt.Rows)
                    {
                        int id = 0;
                        string ad = string.Empty;

                        if (hasId && row["PersonelId"] != DBNull.Value)
                            int.TryParse(row["PersonelId"].ToString(), out id);

                        if (hasAdSoyad && row["AdSoyad"] != DBNull.Value)
                            ad = row["AdSoyad"].ToString();

                        if (id > 0 && !string.IsNullOrWhiteSpace(ad))
                            list.Add(new PersonelLookupItem { Id = id, Ad = ad });
                    }
                }
            }
            catch (Exception)
            {
                // ignore lookup errors
            }

            return list;
        }

        private void BumpVer(int firmaId)
        {
            var key = CacheVerPrefix + firmaId;
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
