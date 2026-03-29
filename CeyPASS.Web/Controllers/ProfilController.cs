using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Models.Profil;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Globalization;
using System.Linq;

namespace CeyPASS.Web.Controllers
{
    public class ProfilController : Controller
    {
        private const string PageName = "Profil";
        private const string IzinTalepleriPage = "IzinTalepleri";
        private const string AvansPage = "Avans";

        private readonly ISessionContext _session;
        private readonly IAuthorizationService _auth;
        private readonly IIzinTalepService _izinTalepService;
        private readonly IAvansService _avansService;
        private readonly IIzinTipService _izinTipService;
        private readonly IKisiIzinlerRepository _kisiIzinlerRepo;
        private readonly IKisiQueryService _kisiQueryService;
        private readonly IKisiEkraniLookUpService _lookupService;
        private readonly IKisiHareketService _kisiHareketService;
        private readonly IUstYetkiliRepository _ustYetkiliRepo;
        private readonly ISifreService _sifreService;

        public ProfilController(
            ISessionContext session,
            IAuthorizationService auth,
            IIzinTalepService izinTalepService,
            IAvansService avansService,
            IIzinTipService izinTipService,
            IKisiIzinlerRepository kisiIzinlerRepo,
            IKisiQueryService kisiQueryService,
            IKisiEkraniLookUpService lookupService,
            IKisiHareketService kisiHareketService,
            IUstYetkiliRepository ustYetkiliRepo,
            ISifreService sifreService)
        {
            _session = session;
            _auth = auth;
            _izinTalepService = izinTalepService;
            _avansService = avansService;
            _izinTipService = izinTipService;
            _kisiIzinlerRepo = kisiIzinlerRepo;
            _kisiQueryService = kisiQueryService;
            _lookupService = lookupService;
            _kisiHareketService = kisiHareketService;
            _ustYetkiliRepo = ustYetkiliRepo;
            _sifreService = sifreService;
        }

        public IActionResult Index(int hareketPage = 1)
        {
            if (!_auth.ViewAbility(PageName))
            {
                TempData["Error"] = "Profil ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(_session.AktifSicilNo))
            {
                TempData["Error"] = "Personel hesabı eşlemesi bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            if (hareketPage < 1) hareketPage = 1;

            var model = new ProfilIndexViewModel
            {
                SicilNo = _session.AktifSicilNo!,
                HasUstYetkiliPanel = _izinTalepService.IsSupervisor(_session.AktifSicilNo),
                HareketPage = hareketPage
            };

            try
            {
                var kisi = _kisiQueryService.GetKisiDetay(_session.AktifSicilNo);
                model.Kisi = kisi;
                model.FotografDataUrl = ToImageDataUrl(kisi?.Fotograf);

                model.YemekHakkiVar = kisi?.YemekHakkiVar;
                model.GunlukYemekAdedi = kisi?.GunlukYemekAdedi;

                if (kisi?.DepartmanId != null)
                    model.DepartmanAdi = _lookupService.GetDepartmanlar().FirstOrDefault(x => x.Id == kisi.DepartmanId)?.Ad;
                if (kisi?.PozisyonId != null)
                    model.PozisyonAdi = _lookupService.GetPozisyonlar().FirstOrDefault(x => x.Id == kisi.PozisyonId)?.Ad;

                if (_session.AktifFirmaId.HasValue && int.TryParse(_session.AktifSicilNo, out var pid))
                {
                    int total;
                    var bas = DateTime.Today.AddDays(-7);
                    var bit = DateTime.Now;
                    model.GirisCikisHareketleri = _kisiHareketService.GetByPersonsPaged(
                        new System.Collections.Generic.List<int> { pid },
                        bas,
                        bit,
                        onlyAktif: true,
                        onlyPasif: false,
                        onlyYemekhane: false,
                        firmaId: _session.AktifFirmaId.Value,
                        page: hareketPage,
                        pageSize: model.HareketPageSize,
                        out total
                    ) ?? new System.Collections.Generic.List<KisiHareketListRow>();

                    model.HareketTotalCount = total;
                    model.HareketTotalPages = total > 0 ? (int)Math.Ceiling(total / (double)model.HareketPageSize) : 1;
                }
            }
            catch
            {
                // Profil ekranını bozmayalım; kişi detayı boş kalabilir.
            }

            return View(model);
        }

        private static string? ToImageDataUrl(byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;

            // basit MIME tespiti (png/jpg); bilinmiyorsa jpeg kabul edelim
            string mime = "image/jpeg";
            if (bytes.Length >= 8 &&
                bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
            {
                mime = "image/png";
            }
            else if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            {
                mime = "image/jpeg";
            }

            return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }

        [HttpGet]
        public IActionResult Izinlerim(int taleplerPage = 1, int gecmisPage = 1)
        {
            if (!_auth.ViewAbility(PageName))
            {
                TempData["Error"] = "Bu sayfayı görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }
            if (string.IsNullOrWhiteSpace(_session.AktifSicilNo))
            {
                TempData["Error"] = "Personel hesabı eşlemesi bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.IzinTipleri = _izinTipService.GetAktif();
            try
            {
                var kisi = _kisiQueryService.GetKisiDetay(_session.AktifSicilNo);
                ViewBag.DefaultTelefon = kisi?.CepTel;
            }
            catch
            {
                ViewBag.DefaultTelefon = null;
            }

            if (taleplerPage < 1) taleplerPage = 1;
            if (gecmisPage < 1) gecmisPage = 1;

            var model = new IzinlerimViewModel
            {
                TaleplerPage = taleplerPage,
                GecmisPage = gecmisPage
            };

            // Taleplerim (paged)
            var tumTalepler = _izinTalepService.PersonelTalepleri(_session.AktifSicilNo) ?? new System.Collections.Generic.List<IzinTalep>();
            model.TaleplerTotalCount = tumTalepler.Count;
            model.TaleplerTotalPages = model.TaleplerTotalCount > 0
                ? (int)Math.Ceiling(model.TaleplerTotalCount / (double)model.TaleplerPageSize)
                : 1;
            if (model.TaleplerPage > model.TaleplerTotalPages) model.TaleplerPage = model.TaleplerTotalPages;
            model.Talepler = tumTalepler
                .Skip((model.TaleplerPage - 1) * model.TaleplerPageSize)
                .Take(model.TaleplerPageSize)
                .ToList();

            try
            {
                DataTable dt = _kisiIzinlerRepo.GetByPerson(_session.AktifSicilNo);
                foreach (DataRow r in dt.Rows)
                {
                    model.OnayliIzinler.Add(new KisiIzinGecmisRow
                    {
                        KisiIzinId = ToInt(r["KisiIzinId"]),
                        Baslangic = ToDateTime(r["İzin Başlangıcı"]),
                        Bitis = ToDateTime(r["İzin Bitişi"]),
                        SureSaat = ToDecimal(r["Süre(Saat)"]),
                        Aciklama = r["Açıklama"]?.ToString(),
                        IslenmeTarihi = ToNullableDateTime(r["İşlenme Tarihi"]),
                        SaatlikIzinMi = string.Equals(r["Saatlik İzin"]?.ToString(), "EVET", StringComparison.OrdinalIgnoreCase)
                    });
                }
            }
            catch
            {
                // Liste ekranını bozmayalım; sadece geçmiş boş gelsin.
            }

            // Onaylı izin geçmişi (paged)
            model.GecmisTotalCount = model.OnayliIzinler.Count;
            model.GecmisTotalPages = model.GecmisTotalCount > 0
                ? (int)Math.Ceiling(model.GecmisTotalCount / (double)model.GecmisPageSize)
                : 1;
            if (model.GecmisPage > model.GecmisTotalPages) model.GecmisPage = model.GecmisTotalPages;
            model.OnayliIzinler = model.OnayliIzinler
                .Skip((model.GecmisPage - 1) * model.GecmisPageSize)
                .Take(model.GecmisPageSize)
                .ToList();

            return View(model);
        }

        private static int ToInt(object? v)
        {
            if (v == null || v == DBNull.Value) return 0;
            if (v is int i) return i;
            if (int.TryParse(v.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var x)) return x;
            return 0;
        }

        private static DateTime ToDateTime(object? v)
        {
            if (v == null || v == DBNull.Value) return DateTime.MinValue;
            if (v is DateTime dt) return dt;
            if (DateTime.TryParse(v.ToString(), CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.AssumeLocal, out var x)) return x;
            if (DateTime.TryParse(v.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out x)) return x;
            return DateTime.MinValue;
        }

        private static DateTime? ToNullableDateTime(object? v)
        {
            if (v == null || v == DBNull.Value) return null;
            var dt = ToDateTime(v);
            return dt == DateTime.MinValue ? null : dt;
        }

        private static decimal ToDecimal(object? v)
        {
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            if (decimal.TryParse(v.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var x)) return x;
            if (decimal.TryParse(v.ToString(), NumberStyles.Number, CultureInfo.GetCultureInfo("tr-TR"), out x)) return x;
            return 0m;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult KullanimImzaAt(int talepId)
        {
            if (string.IsNullOrWhiteSpace(_session.AktifSicilNo))
            {
                TempData["Error"] = "Oturum bilgileri eksik.";
                return RedirectToAction(nameof(Izinlerim));
            }

            // talep kişinin kendisine ait olmalı
            var t = _izinTalepService.PersonelTalepleri(_session.AktifSicilNo).FirstOrDefault(x => x.TalepId == talepId);
            if (t == null)
            {
                TempData["Error"] = "Talep bulunamadı.";
                return RedirectToAction(nameof(Izinlerim));
            }

            var ok = _izinTalepService.KullanimImzaAt(talepId, _session.AktifKullaniciId ?? 0);
            TempData[ok ? "Success" : "Error"] = ok ? "İzni kullanan imzası kaydedildi." : "İmza kaydedilemedi (İK henüz açmamış olabilir).";
            return RedirectToAction(nameof(Izinlerim));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IzinTalep(int izinTipId, DateTime baslangic, DateTime bitis, bool saatlikIzinMi, string? aciklama, string? izinAdres, string? telefonNo)
        {
            if (!_auth.CreateAbility(IzinTalepleriPage))
            {
                TempData["Error"] = "İzin talebi oluşturma yetkiniz yok.";
                return RedirectToAction(nameof(Izinlerim));
            }
            if (string.IsNullOrWhiteSpace(_session.AktifSicilNo) || !_session.AktifFirmaId.HasValue)
            {
                TempData["Error"] = "Oturum bilgileri eksik.";
                return RedirectToAction(nameof(Izinlerim));
            }

            var talep = new IzinTalep
            {
                PersonelId = _session.AktifSicilNo,
                FirmaId = _session.AktifFirmaId.Value,
                IzinTipId = izinTipId,
                Baslangic = baslangic,
                Bitis = bitis,
                SaatlikIzinMi = saatlikIzinMi,
                Aciklama = aciklama,
                IzinAdres = izinAdres,
                TelefonNo = telefonNo
            };

            // Salt personel için KullaniciId yoktur (null gelir). 0 gönderiyoruz, servis katmanında yönetilebilir.
            _izinTalepService.TalepOlustur(talep, _session.AktifKullaniciId ?? 0);
            TempData["Success"] = "İzin talebiniz alındı.";
            return RedirectToAction(nameof(Izinlerim));
        }

        [HttpGet]
        [HttpGet]
        public IActionResult Avanslarim(int pageAktif = 1, int pageGecmis = 1)
        {
            if (!_auth.ViewAbility(PageName))
            {
                TempData["Error"] = "Bu sayfayı görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }
            if (string.IsNullOrWhiteSpace(_session.AktifSicilNo))
                return RedirectToAction("Login", "Account");

            int pageSize = 5;
            var all = _avansService.PersonelTalepleri(_session.AktifSicilNo);
            
            var aktifSource = all.Where(x => x.Durum == AvansDurumu.Bekliyor).ToList();
            var gecmisSource = all.Where(x => x.Durum != AvansDurumu.Bekliyor).ToList();

            var model = new AvanslarimViewModel
            {
                PageSize = pageSize,
                
                AktifPage = pageAktif,
                AktifTotalCount = aktifSource.Count,
                AktifTotalPages = (int)Math.Ceiling(aktifSource.Count / (double)pageSize),
                AktifTalepler = aktifSource.Skip((pageAktif - 1) * pageSize).Take(pageSize).ToList(),

                GecmisPage = pageGecmis,
                GecmisTotalCount = gecmisSource.Count,
                GecmisTotalPages = (int)Math.Ceiling(gecmisSource.Count / (double)pageSize),
                GecmisTalepler = gecmisSource.Skip((pageGecmis - 1) * pageSize).Take(pageSize).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AvansTalep(decimal miktar, string? aciklama)
        {
            if (!_auth.CreateAbility(AvansPage))
            {
                TempData["Error"] = "Avans talebi oluşturma yetkiniz yok.";
                return RedirectToAction(nameof(Avanslarim));
            }
            if (string.IsNullOrWhiteSpace(_session.AktifSicilNo))
            {
                TempData["Error"] = "Personel hesabı eşlemesi bulunamadı.";
                return RedirectToAction(nameof(Avanslarim));
            }

            _avansService.TalepOlustur(_session.AktifSicilNo, miktar, aciklama);
            TempData["Success"] = "Avans talebiniz alındı.";
            return RedirectToAction(nameof(Avanslarim));
        }

        [HttpGet]
        public IActionResult UstYetkiliPaneli()
        {
            if (string.IsNullOrWhiteSpace(_session.AktifSicilNo))
            {
                TempData["Error"] = "Personel hesabı eşlemesi bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            var items = _izinTalepService.UstYetkiliBekleyenler(_session.AktifSicilNo) ?? new System.Collections.Generic.List<IzinTalep>();

            // Bağlı personelleri de çekelim (Şifre sıfırlama vb. için)
            var bagliIds = _ustYetkiliRepo.GetSubordinates(_session.AktifSicilNo);
            var bagliPersoneller = new System.Collections.Generic.List<KisiDetay>();
            foreach (var bid in bagliIds)
            {
                var k = _kisiQueryService.GetKisiDetay(bid);
                if (k != null) bagliPersoneller.Add(k);
            }
            ViewBag.BagliPersoneller = bagliPersoneller;

            // Bekleyen taleplerdeki isimleri topla
            var pIds = items.Select(x => x.PersonelId).Distinct().ToList();
            var pDict = new System.Collections.Generic.Dictionary<string, string>();
            foreach (var pid in pIds)
            {
                var k = _kisiQueryService.GetKisiDetay(pid);
                if (k != null) pDict[pid] = $"{k.Ad} {k.Soyad}".Trim();
                else pDict[pid] = pid; // fallback
            }
            ViewBag.Personeller = pDict;

            var izinTipleri = _izinTipService.GetAktif().ToDictionary(k => k.IzinTipId, v => v.Ad);
            ViewBag.IzinTipleri = izinTipleri;

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SifreSifirlaSubordinate(string personelId, string yeniSifre)
        {
            if (string.IsNullOrWhiteSpace(_session.AktifSicilNo)) return Unauthorized();

            // Güvenlik kontrolü: Personel gerçekten bu amire mi bağlı?
            var subordinates = _ustYetkiliRepo.GetSubordinates(_session.AktifSicilNo);
            if (!subordinates.Contains(personelId))
            {
                TempData["Error"] = "Bu personel için yetkiniz yok.";
                return RedirectToAction(nameof(UstYetkiliPaneli));
            }

            var ok = _sifreService.SifreSifirlaManuel(personelId, yeniSifre);
            TempData[ok ? "Success" : "Error"] = ok ? "Şifre başarıyla güncellendi." : "Şifre güncellenemedi.";
            return RedirectToAction(nameof(UstYetkiliPaneli));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UstYetkiliOnayla(int talepId, string? aciklama)
        {
            if (string.IsNullOrWhiteSpace(_session.AktifSicilNo))
            {
                TempData["Error"] = "Oturum bilgisi eksik.";
                return RedirectToAction(nameof(UstYetkiliPaneli));
            }

            var ok = _izinTalepService.UstYetkiliOnayla(talepId, _session.AktifSicilNo, aciklama);
            TempData[ok ? "Success" : "Error"] = ok ? "Talep onaylandı." : "İşlem başarısız.";
            return RedirectToAction(nameof(UstYetkiliPaneli));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UstYetkiliReddet(int talepId, string? aciklama)
        {
            if (string.IsNullOrWhiteSpace(_session.AktifSicilNo))
            {
                TempData["Error"] = "Oturum bilgisi eksik.";
                return RedirectToAction(nameof(UstYetkiliPaneli));
            }

            var ok = _izinTalepService.UstYetkiliReddet(talepId, _session.AktifSicilNo, aciklama);
            TempData[ok ? "Success" : "Error"] = ok ? "Talep reddedildi." : "İşlem başarısız.";
            return RedirectToAction(nameof(UstYetkiliPaneli));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AvansIptal(int avansId)
        {
            if (string.IsNullOrWhiteSpace(_session.AktifSicilNo))
                return RedirectToAction("Login", "Account");

            var t = _avansService.PersonelTalepleri(_session.AktifSicilNo).FirstOrDefault(x => x.AvansId == avansId);
            if (t == null || t.Durum != AvansDurumu.Bekliyor)
            {
                TempData["Error"] = "İptal edilemez bir talep veya yetkisiz işlem.";
                return RedirectToAction(nameof(Avanslarim));
            }

            if (_avansService.IptalEt(avansId))
                TempData["Success"] = "Avans talebiniz iptal edildi.";
            else
                TempData["Error"] = "Talep iptal edilirken bir hata oluştu.";

            return RedirectToAction(nameof(Avanslarim));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AvansGuncelle(int avansId, decimal miktar, string? aciklama)
        {
            if (string.IsNullOrWhiteSpace(_session.AktifSicilNo))
                return RedirectToAction("Login", "Account");

            var t = _avansService.PersonelTalepleri(_session.AktifSicilNo).FirstOrDefault(x => x.AvansId == avansId);
            if (t == null || t.Durum != AvansDurumu.Bekliyor)
            {
                TempData["Error"] = "Güncellenemez bir talep veya yetkisiz işlem.";
                return RedirectToAction(nameof(Avanslarim));
            }

            if (_avansService.Guncelle(avansId, miktar, aciklama))
                TempData["Success"] = "Avans talebiniz güncellendi.";
            else
                TempData["Error"] = "Talep güncellenirken bir hata oluştu.";

            return RedirectToAction(nameof(Avanslarim));
        }
    }
}

