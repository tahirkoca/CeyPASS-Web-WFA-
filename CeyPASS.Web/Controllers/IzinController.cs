using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
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
        private readonly IFirmaService _firmaService;
        private readonly IPuantajService _puantajService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Izinler";

        public IzinController(
            IKisiIzinService kisiIzinService,
            IKisiQueryService kisiQueryService,
            IIzinTipService izinTipService,
            IFirmaService firmaService,
            IPuantajService puantajService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService)
        {
            _kisiIzinService = kisiIzinService;
            _kisiQueryService = kisiQueryService;
            _izinTipService = izinTipService;
            _firmaService = firmaService;
            _puantajService = puantajService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
        }

        public IActionResult Index(string personelId = null, int? izinTipId = null, DateTime? baslangic = null, DateTime? bitis = null)
        {
            // Check authorization
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "İzinler ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            // Firma ID navbar'dan seçilen aktif firmadan alınır
            int selectedFirmaId = (int)_sessionContext.AktifFirmaId;

            // Default tarih aralığı: Bu ay
            DateTime baslangicTarih = baslangic ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime bitisTarih = bitis ?? baslangicTarih.AddMonths(1).AddDays(-1);

            // İzinleri yükle
            DataTable izinlerDt = _kisiIzinService.GetTumIzinler(
                selectedFirmaId,
                personelId == "ALL" ? null : personelId,
                izinTipId == 0 ? (int?)null : izinTipId,
                baslangicTarih,
                bitisTarih
            );

            // DataTable'ı List'e çevir
            List<KisiIzinListRow> izinler = new List<KisiIzinListRow>();
            if (izinlerDt != null)
            {
                foreach (DataRow row in izinlerDt.Rows)
                {
                    izinler.Add(new KisiIzinListRow
                    {
                        KisiIzinId = row["KisiIzinId"] != DBNull.Value ? Convert.ToInt32(row["KisiIzinId"]) : 0,
                        SicilNo = row["SicilNo"]?.ToString() ?? "",
                        AdSoyad = row["AdSoyad"]?.ToString() ?? "",
                        FirmaAdi = row["FirmaAdi"]?.ToString() ?? "",
                        IzinTipi = row["IzinTipi"]?.ToString() ?? "",
                        IzinBaslangic = row["IzinBaslangic"] != DBNull.Value ? Convert.ToDateTime(row["IzinBaslangic"]) : DateTime.MinValue,
                        IzinBitis = row["IzinBitis"] != DBNull.Value ? Convert.ToDateTime(row["IzinBitis"]) : DateTime.MinValue,
                        SureGun = row["SureGun"]?.ToString() ?? "",
                        SureSaat = row["SureSaat"] != DBNull.Value ? Convert.ToDouble(row["SureSaat"]) : 0,
                        SaatlikIzin = row["SaatlikIzin"]?.ToString() ?? "",
                        Aciklama = row["Aciklama"]?.ToString() ?? "",
                        IslenmeTarihi = row["IslenmeTarihi"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["IslenmeTarihi"]) : null,
                        GuncellemeTarihi = row["GuncellemeTarihi"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["GuncellemeTarihi"]) : null
                    });
                }
            }

            // Lookup data
            var kisiler = _kisiQueryService.GetAktifKisilerByFirma(selectedFirmaId);
            var izinTipleri = _izinTipService.GetAktif();

            ViewBag.Kisiler = kisiler;
            ViewBag.IzinTipleri = izinTipleri;
            ViewBag.SelectedPersonelId = personelId;
            ViewBag.SelectedIzinTipId = izinTipId;
            ViewBag.BaslangicTarih = baslangicTarih;
            ViewBag.BitisTarih = bitisTarih;
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
            ViewBag.Kisiler = _kisiQueryService.GetAktifKisilerByFirma(model.FirmaId);
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
                ViewBag.Kisiler = _kisiQueryService.GetAktifKisilerByFirma(izin.FirmaId);
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
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "İzin eklenemedi.");
                    var kullaniciYetkileri = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_sessionContext.AktifKullaniciId);
                    var firmaYetkileri = kullaniciYetkileri.Select(y => y.FirmaId).Distinct().ToHashSet();
                    ViewBag.Firmalar = GetAuthorizedFirmalar(firmaYetkileri);
                    ViewBag.Kisiler = _kisiQueryService.GetAktifKisilerByFirma(izin.FirmaId);
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
                ViewBag.Kisiler = _kisiQueryService.GetAktifKisilerByFirma(izin.FirmaId);
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
            ViewBag.Kisiler = _kisiQueryService.GetAktifKisilerByFirma(izin.FirmaId);
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
                ViewBag.Kisiler = _kisiQueryService.GetAktifKisilerByFirma(mevcut.FirmaId);
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
            ViewBag.Kisiler = _kisiQueryService.GetAktifKisilerByFirma(izin.FirmaId);
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

        [HttpGet]
        public IActionResult GetKisiler(int firmaId)
        {
            var kisiler = _kisiQueryService.GetAktifKisilerByFirma(firmaId);
            return Json(kisiler.Select(k => new { PersonelId = k.PersonelId, AdSoyad = k.AdSoyad }));
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
