using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Linq;
using System.Drawing;

namespace CeyPASS.Web.Controllers
{
    public class PersonelController : Controller
    {
        private readonly IKisiService _kisiService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly IKisiEkraniLookUpService _lookupService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICalismaSekliService _calismaSekliService;
        private readonly IFirmaService _firmaService;
        private readonly IMemoryCache _cache;
        private const string PageName = "Personeller";
        private const int DefaultPageSize = 20;
        private static readonly int[] AllowedPageSizes = new[] { 10, 20, 50, 100 };
        private const string CacheVerPrefix = "personel_list_ver_";

        public PersonelController(
            IKisiService kisiService,
            IKisiQueryService kisiQueryService,
            IKisiEkraniLookUpService lookupService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            ICalismaSekliService calismaSekliService,
            IFirmaService firmaService,
            IMemoryCache cache)
        {
            _kisiService = kisiService;
            _kisiQueryService = kisiQueryService;
            _lookupService = lookupService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _calismaSekliService = calismaSekliService;
            _firmaService = firmaService;
            _cache = cache;
        }

        /// <param name="kartTipi">puantaj = Puantaj Yapılan Kartlar (PuantajYapilirMi=1), puantajsiz = Puantaj Yapılmayan Kartlar (PuantajYapilirMi=0)</param>
        public IActionResult Index(string search = null, int? firmaId = null, int? isyeriId = null, string kartTipi = null, int page = 1, int pageSize = DefaultPageSize)
        {
            // Check authorization
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Personeller ekranını görüntüleme yetkiniz yok.";
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

            bool puantajYapilan = (kartTipi != "puantajsiz");
            var puantajYapilirMi = puantajYapilan;

            // Load personel list (paged + cache)
            var searchNorm = NormalizeSearch(search);
            var verKey = CacheVerPrefix + selectedFirmaId;
            if (!_cache.TryGetValue(verKey, out int ver))
            {
                ver = 0;
                _cache.Set(verKey, ver, TimeSpan.FromHours(1));
            }

            var cacheKey = $"personel_list_{selectedFirmaId}_v{ver}_{(isyeriId.HasValue ? isyeriId.Value.ToString() : "all")}_{(puantajYapilan ? "puantaj" : "puantajsiz")}_{searchNorm}_p{page}_s{pageSize}";
            if (!_cache.TryGetValue(cacheKey, out PersonelListCacheValue cached))
            {
                int totalCount;
                var items = _kisiQueryService.GetAktifKisilerByFirmaPaged(selectedFirmaId, search, puantajYapilirMi, isyeriId, page, pageSize, out totalCount);
                cached = new PersonelListCacheValue(items, totalCount);
                _cache.Set(cacheKey, cached, TimeSpan.FromMinutes(2));
            }

            var personelList = cached.Items;
            var total = cached.TotalCount;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            // Load lookup data for filters
            var firmalar = isAdmin ? _firmaService.GetAll().OrderBy(f => f.FirmaAdi).ToList() : null;
            var isyerleri = _lookupService.GetIsyerleri(selectedFirmaId);

            ViewBag.Search = search;
            ViewBag.SelectedFirmaId = selectedFirmaId;
            ViewBag.SelectedIsyeriId = isyeriId;
            ViewBag.KartTipi = puantajYapilan ? "puantaj" : "puantajsiz";
            ViewBag.Firmalar = firmalar;
            ViewBag.Isyerleri = isyerleri;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.CanCreate = _authorizationService.Can(PageName, YetkiTipleri.Create);
            ViewBag.CanUpdate = _authorizationService.Can(PageName, YetkiTipleri.Update);
            ViewBag.CanDelete = _authorizationService.Can(PageName, YetkiTipleri.Delete);
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = total;
            ViewBag.TotalPages = totalPages;

            return View(personelList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Personel ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var model = new Kisi
            {
                FirmaId = (int)_sessionContext.AktifFirmaId,
                IseGirisTarihi = DateTime.Today,
                PuantajYapilirMi = true
            };

            LoadLookupData(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Kisi kisi, bool firmaPersoneli, bool puantajYapilabilir, bool yemekHakkiVar, int gunlukYemekLimiti, string puantajsizKartId, string puantajsizKartNo, string puantajsizKartAdi, IFormFile fotograf, bool ziyaretciMi, bool aracKartiMi, bool taseronCalisanMi)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "Personel ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            // Validation
            var validationDto = new KisiKayitValidasyonDTO
            {
                PersonelId = kisi.PersonelId,
                FirmaPersoneli = firmaPersoneli,
                PuantajYapilir = puantajYapilabilir,
                YemekHakkiVar = yemekHakkiVar,
                YemekAdedi = gunlukYemekLimiti,
                FirmaDisiKartNo = puantajsizKartNo
            };

            var validation = _kisiService.ValidateKisiKayit(validationDto);
            if (!validation.IsValid)
            {
                ModelState.AddModelError("", validation.Message);
                LoadLookupData(kisi);
                return View(kisi);
            }

            try
            {
                kisi.ZiyaretciMi = ziyaretciMi;
                kisi.AracKartiMi = aracKartiMi;
                kisi.TaseronCalisanMi = taseronCalisanMi;

                // Fotoğraf yükleme
                if (fotograf != null && fotograf.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        fotograf.CopyTo(ms);
                        var imageBytes = ms.ToArray();
                        using (var img = Image.FromStream(new MemoryStream(imageBytes)))
                        {
                            kisi.Fotograf = DbHelpers.ImageToBytes(img);
                        }
                    }
                }

                _kisiService.YeniKisiEkle(kisi, firmaPersoneli, puantajYapilabilir, yemekHakkiVar, gunlukYemekLimiti, puantajsizKartId, puantajsizKartNo, puantajsizKartAdi);
                TempData["Success"] = "Personel başarıyla eklendi.";
                BumpPersonelCacheVersion(kisi.FirmaId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Personel eklenirken bir hata oluştu: " + ex.Message);
                LoadLookupData(kisi);
                return View(kisi);
            }
        }

        [HttpGet]
        public IActionResult Edit(string id, string kartTipi, int? firmaId, int page = 1, int pageSize = DefaultPageSize, string search = null, int? isyeriId = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Personel güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            ViewBag.KartTipi = kartTipi;
            ViewBag.SelectedFirmaId = firmaId;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Search = search;
            ViewBag.SelectedIsyeriId = isyeriId;

            var kisi = _kisiQueryService.GetKisiDetay(id);
            if (kisi == null)
                return NotFound();

            LoadLookupData(kisi);
            ViewBag.OriginalPersonelId = id;
            return View(kisi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPuantajsiz(string kartId, string kartAdi, string kartNo, string calismaSekli, string kartTipi, int? firmaId)
        {
            TempData["Info"] = "Personel bilgileri artık Kisiler üzerinden düzenlenir.";
            return RedirectToAction("Index", new { kartTipi, firmaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string originalPersonelId, KisiDetay kisiDetay, bool firmaPersoneli, bool puantajYapilabilir, bool yemekHakkiVar, int gunlukYemekAdedi, string firmaDisiKartNo, bool fotoDegisti, IFormFile fotograf, string kartTipi, int? firmaId, int page = 1, int pageSize = DefaultPageSize, string search = null, int? isyeriId = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Personel güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                // KisiDetay'dan Kisi'ye dönüştür
                var kisi = new Kisi
                {
                    PersonelId = kisiDetay.PersonelId,
                    Ad = kisiDetay.Ad,
                    Soyad = kisiDetay.Soyad,
                    KartNo = kisiDetay.KartNo,
                    TcKimlikNo = kisiDetay.TcKimlikNo,
                    PozisyonId = kisiDetay.PozisyonId,
                    DepartmanId = kisiDetay.DepartmanId,
                    FirmaId = kisiDetay.FirmaId,
                    IsyeriId = kisiDetay.IsyeriId,
                    BolumId = kisiDetay.BolumId,
                    DogumTarihi = kisiDetay.DogumTarihi,
                    IseGirisTarihi = kisiDetay.IseGirisTarihi ?? DateTime.Today,
                    IstenCikisTarihi = kisiDetay.IstenCikisTarihi,
                    CepTel = kisiDetay.CepTel,
                    Email = kisiDetay.Email,
                    PuantajYapilirMi = kisiDetay.PuantajYapilabilir,
                    Fotograf = kisiDetay.Fotograf,
                    ZiyaretciMi = kisiDetay.ZiyaretciMi,
                    AracKartiMi = kisiDetay.AracKartiMi,
                    TaseronCalisanMi = kisiDetay.TaseronCalisanMi
                };

                // Fotoğraf yükleme (yeni fotoğraf varsa)
                if (fotoDegisti && fotograf != null && fotograf.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        fotograf.CopyTo(ms);
                        var imageBytes = ms.ToArray();
                        using (var img = Image.FromStream(new MemoryStream(imageBytes)))
                        {
                            kisi.Fotograf = DbHelpers.ImageToBytes(img);
                        }
                    }
                }

                bool success = _kisiService.KisiGuncelle(kisi, originalPersonelId, firmaPersoneli, puantajYapilabilir, yemekHakkiVar, gunlukYemekAdedi, firmaDisiKartNo, fotoDegisti);
                if (success)
                {
                    TempData["Success"] = "Personel başarıyla güncellendi.";
                    BumpPersonelCacheVersion(kisi.FirmaId);
                    return RedirectToAction("Index", new { kartTipi, firmaId, page, pageSize, search, isyeriId });
                }
                else
                {
                    ModelState.AddModelError("", "Personel güncellenemedi.");
                    LoadLookupData(kisiDetay);
                    ViewBag.OriginalPersonelId = originalPersonelId;
                    ViewBag.KartTipi = kartTipi;
                    ViewBag.SelectedFirmaId = firmaId;
                    ViewBag.Page = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.Search = search;
                    ViewBag.SelectedIsyeriId = isyeriId;
                    return View(kisiDetay);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Personel güncellenirken bir hata oluştu: " + ex.Message);
                LoadLookupData(kisiDetay);
                ViewBag.OriginalPersonelId = originalPersonelId;
                ViewBag.KartTipi = kartTipi;
                ViewBag.SelectedFirmaId = firmaId;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = search;
                ViewBag.SelectedIsyeriId = isyeriId;
                return View(kisiDetay);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id, DateTime? cikisTarihi, string? firmaDisiKartNo, string kartTipi, int? firmaId, int page = 1, int pageSize = DefaultPageSize, string search = null, int? isyeriId = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete))
            {
                TempData["Error"] = "Personel silme yetkiniz yok.";
                return RedirectToAction("Index", new { kartTipi, firmaId, page, pageSize, search, isyeriId });
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var cikis = cikisTarihi ?? DateTime.Today;
            var kartNo = string.IsNullOrWhiteSpace(firmaDisiKartNo) ? null : firmaDisiKartNo.Trim();

            try
            {
                bool success = _kisiService.KisiIstenCikar(id, cikis, kartNo);
                if (success)
                {
                    TempData["Success"] = "Personel işten çıkarıldı.";
                    if (firmaId.HasValue) BumpPersonelCacheVersion(firmaId.Value);
                }
                else
                {
                    TempData["Error"] = "Personel işten çıkarılamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index", new { kartTipi, firmaId, page, pageSize, search, isyeriId });
        }

        [HttpGet]
        public IActionResult Details(string id, string kartTipi, int? firmaId, int page = 1, int pageSize = DefaultPageSize, string search = null, int? isyeriId = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            ViewBag.KartTipi = kartTipi;
            ViewBag.SelectedFirmaId = firmaId;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Search = search;
            ViewBag.SelectedIsyeriId = isyeriId;

            var kisi = _kisiQueryService.GetKisiDetay(id);
            if (kisi == null)
                return NotFound();

            FillOrganizasyonAdlari(kisi);
            return View(kisi);
        }

        private void BumpPersonelCacheVersion(int firmaId)
        {
            var verKey = CacheVerPrefix + firmaId;
            if (!_cache.TryGetValue(verKey, out int ver))
                ver = 0;
            ver++;
            _cache.Set(verKey, ver, TimeSpan.FromHours(1));
        }

        private static string NormalizeSearch(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.Trim();
            while (s.Contains("  "))
                s = s.Replace("  ", " ");
            return s.ToLowerInvariant();
        }

        private sealed class PersonelListCacheValue
        {
            public PersonelListCacheValue(System.Collections.Generic.List<KisiListItem> items, int totalCount)
            {
                Items = items ?? new System.Collections.Generic.List<KisiListItem>();
                TotalCount = totalCount;
            }
            public System.Collections.Generic.List<KisiListItem> Items { get; }
            public int TotalCount { get; }
        }

        private void FillOrganizasyonAdlari(KisiDetay kisi)
        {
            if (kisi == null) return;

            var firmalar = _firmaService.GetAll();
            var firma = firmalar?.FirstOrDefault(f => f.FirmaId == kisi.FirmaId);
            ViewBag.FirmaAdi = firma?.FirmaAdi ?? "-";

            var isyerleri = _lookupService.GetIsyerleri(kisi.FirmaId);
            var isyeri = isyerleri?.FirstOrDefault(i => i.Id == (kisi.IsyeriId ?? 0));
            ViewBag.IsyeriAdi = isyeri?.Ad ?? "-";

            var departmanlar = _lookupService.GetDepartmanlar();
            var departman = departmanlar?.FirstOrDefault(d => d.Id == (kisi.DepartmanId ?? 0));
            ViewBag.DepartmanAdi = departman?.Ad ?? "-";

            var pozisyonlar = _lookupService.GetPozisyonlar();
            var pozisyon = pozisyonlar?.FirstOrDefault(p => p.Id == (kisi.PozisyonId ?? 0));
            ViewBag.PozisyonAdi = pozisyon?.Ad ?? "-";

            var bolumler = _lookupService.GetBolumler(kisi.FirmaId);
            var bolum = bolumler?.FirstOrDefault(b => b.Id == (kisi.BolumId ?? 0));
            ViewBag.BolumAdi = bolum?.Ad ?? "-";

            var statuler = _lookupService.GetCalismaStatuleri();
            var st = statuler?.FirstOrDefault(s => s.Id == (kisi.CalismaStatusuId ?? 0));
            kisi.CalismaStatusuText = st?.Ad ?? (kisi.CalismaStatusuText ?? "-");
        }

        // AJAX endpoints for lookups
        [HttpGet]
        public IActionResult GetDepartmanlar()
        {
            var departmanlar = _lookupService.GetDepartmanlar();
            return Json(departmanlar);
        }

        [HttpGet]
        public IActionResult GetPozisyonlar()
        {
            var pozisyonlar = _lookupService.GetPozisyonlar();
            return Json(pozisyonlar);
        }

        [HttpGet]
        public IActionResult GetIsyerleri(int firmaId)
        {
            var isyerleri = _lookupService.GetIsyerleri(firmaId);
            return Json(isyerleri);
        }

        [HttpGet]
        public IActionResult GetBolumler(int firmaId)
        {
            var bolumler = _lookupService.GetBolumler(firmaId);
            return Json(bolumler);
        }

        private void LoadLookupData(object kisiOrDetay)
        {
            int firmaId = 0;
            if (kisiOrDetay is Kisi kisi)
            {
                firmaId = kisi.FirmaId > 0 ? kisi.FirmaId : (int)_sessionContext.AktifFirmaId;
            }
            else if (kisiOrDetay is KisiDetay kisiDetay)
            {
                firmaId = kisiDetay.FirmaId > 0 ? kisiDetay.FirmaId : (int)_sessionContext.AktifFirmaId;
            }
            else
            {
                firmaId = (int)_sessionContext.AktifFirmaId;
            }

            ViewBag.Departmanlar = _lookupService.GetDepartmanlar();
            ViewBag.Pozisyonlar = _lookupService.GetPozisyonlar();
            ViewBag.Isyerleri = _lookupService.GetIsyerleri(firmaId);
            ViewBag.Bolumler = _lookupService.GetBolumler(firmaId);
            ViewBag.CalismaSekilleri = _calismaSekliService.GetAll(firmaId);
            ViewBag.CalismaStatuleri = _lookupService.GetCalismaStatuleri();
            ViewBag.Firmalar = _firmaService.GetAll().OrderBy(f => f.FirmaAdi).ToList();
        }
    }
}
