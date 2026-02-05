using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
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
        private readonly IPuantajsizKartRepository _puantajsizKartRepo;
        private const string PageName = "Personeller";

        public PersonelController(
            IKisiService kisiService,
            IKisiQueryService kisiQueryService,
            IKisiEkraniLookUpService lookupService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            ICalismaSekliService calismaSekliService,
            IFirmaService firmaService,
            IPuantajsizKartRepository puantajsizKartRepo)
        {
            _kisiService = kisiService;
            _kisiQueryService = kisiQueryService;
            _lookupService = lookupService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _calismaSekliService = calismaSekliService;
            _firmaService = firmaService;
            _puantajsizKartRepo = puantajsizKartRepo;
        }

        /// <param name="kartTipi">puantaj = Puantaj Yapılan Kartlar (PuantajYapilirMi=1), puantajsiz = Puantaj Yapılmayan Kartlar (PuantajYapilirMi=0)</param>
        public IActionResult Index(string search = null, int? firmaId = null, int? isyeriId = null, string kartTipi = null)
        {
            // Check authorization
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "Personeller ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            // Determine firma
            int selectedFirmaId = firmaId ?? (int)_sessionContext.AktifFirmaId;
            bool isAdmin = _sessionContext.RolId == 1 || _sessionContext.RolId == 2;
            if (!isAdmin && selectedFirmaId != _sessionContext.AktifFirmaId)
            {
                selectedFirmaId = (int)_sessionContext.AktifFirmaId;
            }

            bool puantajYapilan = (kartTipi != "puantajsiz");
            var puantajYapilirMi = puantajYapilan;

            // Load personel list (puantaj yapılan veya puantaj yapılmayan kartlar)
            var personelList = _kisiQueryService.GetAktifKisilerByFirma(selectedFirmaId, search, puantajYapilirMi, isyeriId);

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
        public IActionResult Create(Kisi kisi, bool firmaPersoneli, bool puantajYapilabilir, bool yemekHakkiVar, int gunlukYemekLimiti, string puantajsizKartId, string puantajsizKartNo, string puantajsizKartAdi, IFormFile fotograf)
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
        public IActionResult Edit(string id, string kartTipi, int? firmaId)
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

            var kisi = _kisiQueryService.GetKisiDetay(id);
            if (kisi == null)
            {
                var puantajsiz = _puantajsizKartRepo.GetByKartId(id);
                if (puantajsiz != null)
                {
                    ViewBag.OriginalPersonelId = id;
                    return View("EditPuantajsiz", puantajsiz);
                }
                return NotFound();
            }

            LoadLookupData(kisi);
            ViewBag.OriginalPersonelId = id;
            return View(kisi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPuantajsiz(string kartId, string kartAdi, string kartNo, string calismaSekli, string kartTipi, int? firmaId)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "Personel güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(kartId))
            {
                TempData["Error"] = "Kart bulunamadı.";
                return RedirectToAction("Index");
            }
            var mevcut = _puantajsizKartRepo.GetByKartId(kartId);
            if (mevcut == null)
            {
                TempData["Error"] = "Puantajsız kart bulunamadı.";
                return RedirectToAction("Index");
            }
            _puantajsizKartRepo.UpdateByKartId(kartId, kartAdi, kartNo, calismaSekli);
            TempData["Success"] = "Puantajsız kart başarıyla güncellendi.";
            return RedirectToAction("Details", new { id = kartId, kartTipi, firmaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string originalPersonelId, KisiDetay kisiDetay, bool firmaPersoneli, bool puantajYapilabilir, bool yemekHakkiVar, int gunlukYemekAdedi, string firmaDisiKartNo, bool fotoDegisti, IFormFile fotograf, string kartTipi, int? firmaId)
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
                    Fotograf = kisiDetay.Fotograf
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
                    return RedirectToAction("Index", new { kartTipi, firmaId });
                }
                else
                {
                    ModelState.AddModelError("", "Personel güncellenemedi.");
                    LoadLookupData(kisiDetay);
                    ViewBag.OriginalPersonelId = originalPersonelId;
                    ViewBag.KartTipi = kartTipi;
                    ViewBag.SelectedFirmaId = firmaId;
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
                return View(kisiDetay);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id, DateTime? cikisTarihi, string? firmaDisiKartNo, string kartTipi, int? firmaId)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete))
            {
                TempData["Error"] = "Personel silme yetkiniz yok.";
                return RedirectToAction("Index", new { kartTipi, firmaId });
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

            return RedirectToAction("Index", new { kartTipi, firmaId });
        }

        [HttpGet]
        public IActionResult Details(string id, string kartTipi, int? firmaId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            ViewBag.KartTipi = kartTipi;
            ViewBag.SelectedFirmaId = firmaId;

            var kisi = _kisiQueryService.GetKisiDetay(id);
            if (kisi == null)
            {
                // PuantajsizKartlar'da varsa detayı oradan göster (sadece görüntüleme)
                var puantajsiz = _puantajsizKartRepo.GetByKartId(id);
                if (puantajsiz == null)
                    return NotFound();

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
                ViewBag.PuantajsizKartMi = true;
                ViewBag.PuantajsizCalismaSekli = puantajsiz.CalismaSekli ?? "-";
                ViewBag.PuantajsizZiyaretciMi = puantajsiz.ZiyaretciMi == true;
                ViewBag.PuantajsizAracKartiMi = puantajsiz.AracKartiMi == true;
                ViewBag.PuantajsizTaseronCalisanMi = puantajsiz.TaseronCalisanMi == true;
                ViewBag.PuantajsizAktifMi = puantajsiz.AktifMi;
                FillOrganizasyonAdlari(detay);
                return View(detay);
            }

            ViewBag.PuantajsizKartMi = false;
            FillOrganizasyonAdlari(kisi);
            return View(kisi);
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
