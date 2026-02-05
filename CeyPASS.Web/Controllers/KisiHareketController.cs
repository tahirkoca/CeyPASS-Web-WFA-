using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
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
        private const string PageName = "KisiHareketler";

        public KisiHareketController(
            IKisiHareketService kisiHareketService,
            IKisiQueryService kisiQueryService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IFirmaService firmaService)
        {
            _kisiHareketService = kisiHareketService;
            _kisiQueryService = kisiQueryService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _firmaService = firmaService;
        }

        public IActionResult Index(int? firmaId = null, string personelIds = null, DateTime? baslangic = null, DateTime? bitis = null, bool? sadeceAktif = null, bool? sadecePasif = null, bool? sadeceYemekhane = null)
        {
            // Check authorization
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Kişi Hareketler ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

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

            // Personel listesi
            var personelList = GetPersonelList(selectedFirmaId);

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
            DataTable hareketlerDt = null;
            if (seciliPersonelIds.Any())
            {
                hareketlerDt = _kisiHareketService.GetByPersons(
                    seciliPersonelIds,
                    baslangicTarih,
                    bitisTarih,
                    sadeceAktif ?? false,
                    sadecePasif ?? false,
                    sadeceYemekhane ?? false,
                    selectedFirmaId
                );
            }

            // DataTable'ı List'e çevir
            List<KisiHareketListRow> hareketler = new List<KisiHareketListRow>();
            if (hareketlerDt != null)
            {
                foreach (DataRow row in hareketlerDt.Rows)
                {
                    hareketler.Add(new KisiHareketListRow
                    {
                        Id = row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0,
                        Firma = row["Firma"]?.ToString() ?? "",
                        SicilNo = row["SicilNo"]?.ToString() ?? "",
                        AdSoyad = row["AdSoyad"]?.ToString() ?? "",
                        CihazAdi = row["CihazAdi"]?.ToString() ?? "",
                        Tarih = row["Tarih"] != DBNull.Value ? Convert.ToDateTime(row["Tarih"]) : DateTime.MinValue,
                        Tip = row["Tip"]?.ToString() ?? "",
                        KayitZamani = row["KayitZamani"] != DBNull.Value ? Convert.ToDateTime(row["KayitZamani"]) : DateTime.MinValue,
                        AktifMi = row["AktifMi"] != DBNull.Value && Convert.ToBoolean(row["AktifMi"])
                    });
                }
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

        private List<PersonelLookupItem> GetPersonelList(int firmaId)
        {
            var list = new List<PersonelLookupItem>();
            try
            {
                var dt = _kisiHareketService.GetAktifKisilerWithSicil(firmaId);
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
                // Log error if needed
            }

            return list;
        }
    }

    // Helper class for personel lookup
    public class PersonelLookupItem
    {
        public int Id { get; set; }
        public string Ad { get; set; }
    }
}
