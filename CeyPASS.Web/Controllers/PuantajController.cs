using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;

namespace CeyPASS.Web.Controllers
{
    /// <summary>Aylık puantaj listeleme, onay/red/düzenleme ve Excel dışa aktarma.</summary>
    public class PuantajController : Controller
    {
        private readonly IPuantajService _puantajService;
        private readonly IFirmaService _firmaService;
        private readonly IIsyeriService _isyeriService;
        private readonly IKisiService _kisiService;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICokluSicilService _cokluSicilService;
        private const string PageName = "AylikPuantaj";

        public PuantajController(
            IPuantajService puantajService,
            IFirmaService firmaService,
            IIsyeriService isyeriService,
            IKisiService kisiService,
            IKisiQueryService kisiQueryService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            ICokluSicilService cokluSicilService)
        {
            _puantajService = puantajService;
            _firmaService = firmaService;
            _isyeriService = isyeriService;
            _kisiService = kisiService;
            _kisiQueryService = kisiQueryService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _cokluSicilService = cokluSicilService;
        }

        /// <summary>Puantaj ekranı; firma/işyeri yetkisine göre filtreler ve günlük satırları yükler.</summary>
        public IActionResult Index(int? yil = null, int? ay = null, int? firmaId = null, int? isyeriId = null, string personelId = null)
        {
            // Sayfa yetkisi: AylikPuantaj ViewAbility
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Aylık Puantaj ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            // Default values
            int selectedYil = yil ?? DateTime.Today.Year;
            int selectedAy = ay ?? DateTime.Today.Month;
            int selectedFirmaId = firmaId ?? (int)_sessionContext.AktifFirmaId;

            // Kullanıcı yetkilerine göre firma filtreleme
            var kullaniciYetkileri = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
            var firmaYetkileri = kullaniciYetkileri.Select(y => y.FirmaId).Distinct().ToHashSet();
            
            if (firmaYetkileri.Count > 0 && !firmaYetkileri.Contains(selectedFirmaId))
            {
                selectedFirmaId = firmaYetkileri.First();
            }

            // Puantaj verisi (eğer personel seçilmişse)
            var puantajGunleri = new List<PuantajGunSatirDTO>();
            if (!string.IsNullOrWhiteSpace(personelId) && int.TryParse(personelId, out int personelIdInt))
            {
                try
                {
                    puantajGunleri = _puantajService.GetAy(personelIdInt, selectedYil, selectedAy);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Puantaj verisi yüklenirken hata: " + ex.Message;
                }
            }

            // Lookup data
            bool isAdmin = _sessionContext.IsAdmin();
            var firmalar = GetAuthorizedFirmalar(firmaYetkileri);
            var isyerleri = GetYetkiliIsyeriler(selectedFirmaId, kullaniciYetkileri, isAdmin);
            var kisiler = selectedFirmaId > 0
                ? GetYetkiliKisilerForPuantaj(selectedFirmaId, isyeriId, selectedYil, selectedAy, kullaniciYetkileri, isAdmin)
                : new List<KisiListItem>();
            var puantajTipleri = _puantajService.GetPuantajTipleri();
            var ekKayitGun = _puantajService.GetEkKayitGun();

            ViewBag.SelectedYil = selectedYil;
            ViewBag.SelectedAy = selectedAy;
            ViewBag.SelectedFirmaId = selectedFirmaId;
            ViewBag.SelectedIsyeriId = isyeriId;
            ViewBag.SelectedPersonelId = personelId;
            ViewBag.Firmalar = firmalar;
            ViewBag.Isyerleri = isyerleri;
            ViewBag.Kisiler = kisiler;
            ViewBag.PuantajTipleri = puantajTipleri;
            ViewBag.EkKayitGun = ekKayitGun;
            ViewBag.CanUpdate = _authorizationService.Can(PageName, YetkiTipleri.Update);
            ViewBag.CanApprove = _authorizationService.Can(PageName, YetkiTipleri.Approve);
            ViewBag.CanDelete = _authorizationService.Can(PageName, YetkiTipleri.Delete);
            ViewBag.CanExport = _authorizationService.Can(PageName, YetkiTipleri.Export);

            return View(puantajGunleri);
        }

        /// <summary>Aylık puantajı Excel olarak indirir; export yetkisi ve kullanıcı firma/işyeri kapsamı uygulanır.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportExcel(int yil, int ay)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Export))
            {
                TempData["Error"] = "Puantaj Excel export yetkiniz yok.";
                return RedirectToAction("Index", new { yil, ay });
            }

            try
            {
                var yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
                var request = new PuantajExportRequest
                {
                    Yil = yil,
                    Ay = ay,
                    Yetkiler = yetkiler
                };

                var exportData = _puantajService.PrepareMonthlyExport(request);
                if (exportData == null || exportData.Count == 0)
                {
                    TempData["Error"] = "Export edilecek veri bulunamadı.";
                    return RedirectToAction("Index", new { yil, ay });
                }

                string fileName = $"{yil}_{ay:D2}_Puantaj.xlsx";
                string tempPath = Path.Combine(Path.GetTempPath(), fileName);

                ExcelHelper.ExceleDonustur(exportData, tempPath);

                byte[] fileBytes = System.IO.File.ReadAllBytes(tempPath);
                System.IO.File.Delete(tempPath);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Excel export hatası: " + ex.Message;
                return RedirectToAction("Index", new { yil, ay });
            }
        }

        /// <summary>Tek gün puantaj onayı; AJAX isteklerinde JSON döner.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Onayla(int personelId, DateTime tarih, int duzenlenmisFm, string aciklama, string calismaTipi, string saat)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Approve))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Puantaj onaylama yetkiniz yok." });
                TempData["Error"] = "Puantaj onaylama yetkiniz yok.";
                return RedirectToAction("Index");
            }

            decimal saatDec = 0;
            if (!string.IsNullOrWhiteSpace(saat) && !TryParseSaat(saat, out saatDec))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Çalışma saati geçersiz (örn: 7,50 veya 7.50)." });
                TempData["Error"] = "Çalışma saati geçersiz.";
                return RedirectToAction("Index");
            }

            try
            {
                _puantajService.Onayla(personelId, tarih, duzenlenmisFm, aciklama, calismaTipi, saatDec, (int)_sessionContext.AktifKullaniciId);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Puantaj başarıyla onaylandı.", onayDurumu = "Onaylandı" });
                TempData["Success"] = "Puantaj başarıyla onaylandı.";
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Hata: " + ex.Message });
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        /// <summary>Seçili personelin ilgili ayının tüm günlerini toplu onaylar.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TopluOnayla(int personelId, int yil, int ay)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Approve))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Puantaj onaylama yetkiniz yok." });
                TempData["Error"] = "Puantaj onaylama yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                _puantajService.TopluOnayla(personelId, yil, ay, (int)_sessionContext.AktifKullaniciId);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Seçili personelin aylık puantajı başarıyla toplu onaylandı." });
                TempData["Success"] = "Toplu onay işlemi başarıyla tamamlandı.";
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Hata: " + ex.Message });
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        /// <summary>Puantaj kaydını reddeder (Delete yetkisi).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reddet(int personelId, DateTime tarih, string aciklama)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Puantaj reddetme yetkiniz yok." });
                TempData["Error"] = "Puantaj reddetme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                _puantajService.Reddet(personelId, tarih, aciklama, (int)_sessionContext.AktifKullaniciId);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Puantaj başarıyla reddedildi.", onayDurumu = "Reddedildi", aciklama = aciklama ?? "" });
                TempData["Success"] = "Puantaj başarıyla reddedildi.";
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Hata: " + ex.Message });
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        /// <summary>Puantaj düzeltmesi; çalışma tipi+saat varsa FM dakikası yeniden hesaplanır.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Duzenle(int personelId, DateTime tarih, int duzenlenmisFm, string aciklama, string calismaTipi = null, string saat = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Puantaj düzenleme yetkiniz yok." });
                TempData["Error"] = "Puantaj düzenleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            decimal? saatDec = null;
            if (!string.IsNullOrWhiteSpace(saat))
            {
                if (!TryParseSaat(saat, out decimal parsed))
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, message = "Çalışma saati geçersiz (örn: 7,50 veya 7.50)." });
                    TempData["Error"] = "Çalışma saati geçersiz.";
                    return RedirectToAction("Index");
                }
                saatDec = parsed;
            }

            try
            {
                int duzenlenenFmDakika = duzenlenmisFm;
                if (!string.IsNullOrWhiteSpace(calismaTipi) && saatDec.HasValue)
                {
                    duzenlenenFmDakika = _puantajService.HesaplaFazlaMesaiDakika(calismaTipi, saatDec.Value);
                    _puantajService.DuzenleOnayla(personelId, tarih, duzenlenmisFm, aciklama, calismaTipi, saatDec.Value, (int)_sessionContext.AktifKullaniciId);
                }
                else
                    _puantajService.Duzenle(personelId, tarih, duzenlenmisFm, aciklama, (int)_sessionContext.AktifKullaniciId);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new
                    {
                        success = true,
                        message = "Puantaj başarıyla düzenlendi.",
                        onayDurumu = "Düzeltildi",
                        calismaTipi = calismaTipi ?? "",
                        saat = saatDec.HasValue ? (double)saatDec.Value : (double?)null,
                        aciklama = aciklama ?? "",
                        duzenlenenFmDakika = duzenlenenFmDakika
                    });
                TempData["Success"] = "Puantaj başarıyla düzenlendi.";
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Hata: " + ex.Message });
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        /// <summary>Geçmişe puantaj girişi için izin verilen ek gün sayısını ayarlar.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetEkKayitGun(int gun)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Ek kayıt günü ayarlama yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                _puantajService.SetEkKayitGun(gun, (int)_sessionContext.AktifKullaniciId);
                TempData["Success"] = $"Ek kayıt günü {gun} olarak ayarlandı.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        /// <summary>Ana sicilden bağlı sicillere aylık puantaj aktarımı.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CokluSicileAktar(int personelId, int yil, int ay)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Çoklu sicil aktarım yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                _puantajService.CokluSicileAktar(personelId, yil, ay, (int)_sessionContext.AktifKullaniciId);
                TempData["Success"] = "Çoklu sicil aktarımı tamamlandı.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Aktarım sırasında hata: " + ex.Message;
            }

            return RedirectToAction("Index", new { yil, ay, personelId });
        }

        /// <summary>Çoklu sicil özet bilgisini JSON döner.</summary>
        [HttpGet]
        public IActionResult GetCokluSicilOzet(int personelId)
        {
            if (!_authorizationService.ViewAbility(PageName)) return Forbid();
            if (personelId <= 0) return Json(new { isAnaSicil = false, aktifHedefSayisi = 0 });
            var ozet = _cokluSicilService.GetOzet(personelId);
            return Json(new { isAnaSicil = ozet.IsAnaSicil, aktifHedefSayisi = ozet.AktifHedefSayisi });
        }

        /// <summary>Yetkili işyeri listesi (cascade dropdown).</summary>
        [HttpGet]
        public IActionResult GetIsyerleri(int firmaId)
        {
            var yetkiler = LoadKullaniciYetkileri();
            bool isAdmin = _sessionContext.IsAdmin();
            var isyerleri = GetYetkiliIsyeriler(firmaId, yetkiler, isAdmin);
            return Json(isyerleri.Select(i => new { Id = i.IsyeriId, Ad = i.Ad }));
        }

        /// <summary>Puantaj dönemine göre yetkili personel listesi.</summary>
        [HttpGet]
        public IActionResult GetKisiler(int firmaId, int? isyeriId, int? yil, int? ay)
        {
            int yilVal = yil ?? DateTime.Today.Year;
            int ayVal = ay ?? DateTime.Today.Month;
            var yetkiler = LoadKullaniciYetkileri();
            bool isAdmin = _sessionContext.IsAdmin();
            var kisiler = GetYetkiliKisilerForPuantaj(firmaId, isyeriId, yilVal, ayVal, yetkiler, isAdmin);
            return Json(kisiler.Select(k => new { PersonelId = k.PersonelId, AdSoyad = k.AdSoyad }));
        }

        private List<FirmaIsyeriYetkiDTO> LoadKullaniciYetkileri()
        {
            if (!_sessionContext.AktifKullaniciId.HasValue)
                return new List<FirmaIsyeriYetkiDTO>();
            return _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId)
                ?? new List<FirmaIsyeriYetkiDTO>();
        }

        private List<IsyeriItem> GetYetkiliIsyeriler(int firmaId, List<FirmaIsyeriYetkiDTO> yetkiler, bool isAdmin)
        {
            if (firmaId <= 0)
                return new List<IsyeriItem>();
            return FirmaIsyeriYetkiHelper.FilterIsyeriler(
                _isyeriService.GetIsyerleriByFirma(firmaId) ?? new List<IsyeriItem>(),
                firmaId,
                yetkiler,
                isAdmin);
        }

        private List<KisiListItem> GetYetkiliKisilerForPuantaj(
            int firmaId,
            int? selectedIsyeriId,
            int yil,
            int ay,
            List<FirmaIsyeriYetkiDTO> yetkiler,
            bool isAdmin)
        {
            if (selectedIsyeriId.HasValue && !FirmaIsyeriYetkiHelper.IsIsyeriFilterTumu(selectedIsyeriId))
            {
                var kp = _kisiService.GetKisilerForPuantaj(firmaId, selectedIsyeriId.Value, yil, ay);
                return kp.Select(k => new KisiListItem
                {
                    PersonelId = k.PersonelId,
                    AdSoyad = ((k.Ad ?? "") + " " + (k.Soyad ?? "")).Trim()
                }).ToList();
            }

            var (single, idIn) = FirmaIsyeriYetkiHelper.ResolveKisiQueryIsyeriFilter(
                firmaId, selectedIsyeriId, yetkiler, isAdmin);
            return _kisiQueryService.GetAktifKisilerByFirma(firmaId, isyeriId: single, isyeriIdIn: idIn)
                ?? new List<KisiListItem>();
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

        /// <summary>
        /// WinForms PuantajService.ParseSaatValue ile aynı mantık: virgül→nokta, 750→7,5 düzeltmesi.
        /// </summary>
        private static bool TryParseSaat(string value, out decimal result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.Trim();
            // WinForms: Replace(",", ".") ile virgülü noktaya çevir, sonra InvariantCulture ile parse et
            var normalized = value.Replace(",", ".");
            var tr = CultureInfo.GetCultureInfo("tr-TR");
            var inv = CultureInfo.InvariantCulture;
            if (decimal.TryParse(normalized, NumberStyles.Number, inv, out result)) goto Normalize;
            if (decimal.TryParse(value, NumberStyles.Number, tr, out result)) goto Normalize;
            if (decimal.TryParse(value, NumberStyles.Number, inv, out result)) goto Normalize;
            return false;
        Normalize:
            // WinForms: if (saat > 1000) saat /= 100; — 750 yazılmışsa 7,5 kabul et
            if (result > 24m)
            {
                if (result <= 99m) result /= 10m;   // 75 -> 7,5
                else result /= 100m;                // 750 -> 7,5
            }
            return true;
        }
    }
}
