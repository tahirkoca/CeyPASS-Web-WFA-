using CeyPASS.Business.Abstractions;
using CeyPASS.Business.Services;
using CeyPASS.Entities.Concrete;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.Web.Models.CanliIzleme;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;

namespace CeyPASS.Web.Controllers
{
    /// <summary>Canlı geçiş izleme; ayrı oturum (session) ve rol bazlı kart/hareket ekranı.</summary>
    public class CanliIzlemeController : Controller
    {
        private const string SessionKey = "CanliIzlemeUser";
        private readonly ICanliIzlemeService _svc;
        private readonly IKisiHareketService _khsvc;
        private readonly IKisiDetayService _kdsvc;
        private readonly IMisafirKartService _msvc;
        private readonly IAracKartiService _aracSvc;
        private readonly ICanliIzlemeKartKomutService _kartKomutSvc;

        public CanliIzlemeController(
            ICanliIzlemeService svc,
            IKisiHareketService khsvc,
            IKisiDetayService kdsvc,
            IMisafirKartService msvc,
            IAracKartiService aracSvc,
            ICanliIzlemeKartKomutService kartKomutSvc)
        {
            _svc = svc;
            _khsvc = khsvc;
            _kdsvc = kdsvc;
            _msvc = msvc;
            _aracSvc = aracSvc;
            _kartKomutSvc = kartKomutSvc;
        }

        /// <summary>Canlı izleme giriş formu (firma + kullanıcı).</summary>
        [HttpGet]
        public IActionResult Login()
        {
            var firmalar = ToFirmaOptions(_svc.GetFirmalar());
            ViewBag.Firmalar = firmalar;
            var kullanicilarByFirma = new Dictionary<int, List<string>>();
            foreach (var f in firmalar)
                kullanicilarByFirma[f.Id] = _svc.GetKullaniciAdlariByFirma(f.Id) ?? new List<string>();
            ViewBag.KullanicilarByFirmaJson = JsonSerializer.Serialize(kullanicilarByFirma);
            return View(new CanliIzlemeLoginModel());
        }

        /// <summary>Canlı izleme kimlik doğrulama; başarılı oturum session'a yazılır.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(CanliIzlemeLoginModel model)
        {
            var firmalar = ToFirmaOptions(_svc.GetFirmalar());
            ViewBag.Firmalar = firmalar;

            if (model.FirmaId <= 0) ModelState.AddModelError(nameof(model.FirmaId), "Lütfen bölge/firma seçin.");
            if (string.IsNullOrWhiteSpace(model.KullaniciAdi)) ModelState.AddModelError(nameof(model.KullaniciAdi), "Kullanıcı adı boş olamaz.");
            if (string.IsNullOrWhiteSpace(model.Sifre)) ModelState.AddModelError(nameof(model.Sifre), "Şifre boş olamaz.");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var auth = _svc.Login(model.FirmaId, model.KullaniciAdi, model.Sifre);
            if (auth == null)
            {
                ModelState.AddModelError(string.Empty, "Hatalı kullanıcı adı/şifre veya bu bölge için yetki yok.");
                return View(model);
            }

            SaveUser(auth);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Canlı izleme oturumunu sonlandırır.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            ClearUser();
            return RedirectToAction(nameof(Login));
        }

        /// <summary>Ana canlı izleme paneli; rol bayrakları ve ilk geçiş/hareket verisi.</summary>
        [HttpGet]
        public IActionResult Index()
        {
            var user = GetUser();
            if (user == null) return RedirectToAction(nameof(Login));

            ViewBag.User = user;
            ViewBag.IsYemekhane = CanliIzlemeRoleHelper.IsYemekhane(user?.Rol);
            ViewBag.IsArac = CanliIzlemeRoleHelper.IsArac(user?.Rol);
            ViewBag.IsDanisma = CanliIzlemeRoleHelper.IsDanisma(user?.Rol);
            ViewBag.CanMisafirKart = !CanliIzlemeRoleHelper.HideKartAtama(user?.Rol);
            ViewBag.ShowHareketListesi = CanliIzlemeRoleHelper.ShowHareketListesi(user?.Rol);

            // İlk render (JS zaten 1 sn'de bir yenileyecek)
            ViewBag.LastPasses = GetLastPassesInternal(user.FirmaId, 4);
            ViewBag.LastMoves = ViewBag.ShowHareketListesi
                ? GetLastMovesInternal(user.FirmaId, 15)
                : new List<KisiHareketDTO>();

            return View();
        }

        /// <summary>Son geçişler (polling); rol yemekhane/araç/danışma filtresi serviste.</summary>
        [HttpGet]
        public IActionResult LastPasses(int take = 4)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();

            var list = GetLastPassesInternal(user.FirmaId, take);
            var dto = list.Select(x => new
            {
                personelId = x.PersonelId,
                adSoyad = x.AdSoyad,
                isyeriAdi = x.IsyeriAdi,
                unvan = x.Unvan,
                zaman = x.Zaman,
                terminalAdi = x.TerminalAdi,
                girisMi = x.GirisMi,
                fotoBase64 = (x.Foto != null && x.Foto.Length > 0) ? Convert.ToBase64String(x.Foto) : null
            });

            return Json(dto);
        }

        /// <summary>Son hareket listesi; danışma rolü tam liste görür.</summary>
        [HttpGet]
        public IActionResult LastMoves(int top = 15)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();

            var list = GetLastMovesInternal(user.FirmaId, top);
            var dto = list.Select(x => new
            {
                tarih = x.Tarih,
                adSoyad = x.AdSoyad,
                isyeri = x.Isyeri,
                unvan = x.Unvan,
                cihazAdi = x.CihazAdi,
                kisiId = x.PersonelId
            });

            return Json(dto);
        }

        /// <summary>Geçiş satırından personel detay popup verisi.</summary>
        [HttpGet]
        public IActionResult KisiDetay(int kisiId)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();

            var dto = _kdsvc.GetDetay(kisiId);
            if (dto == null)
            {
                return Json(new { ok = false });
            }

            return Json(new
            {
                ok = true,
                adSoyad = dto.AdSoyad,
                unvan = dto.Unvan,
                isyeri = dto.Isyeri,
                fotoBase64 = (dto.Foto != null && dto.Foto.Length > 0) ? Convert.ToBase64String(dto.Foto) : null
            });
        }

        /// <summary>Yeni misafir kart atama partial formu.</summary>
        [HttpGet]
        public IActionResult MisafirKartYeni(string personelId = null)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();

            var cards = _msvc.GetCardsForNew(user.FirmaId);
            ViewBag.Cards = cards;
            ViewBag.PreselectPersonelId = personelId;
            return PartialView("_MisafirKartYeni", new MisafirKartYeniModel { GirisSaati = DateTime.Now });
        }

        /// <summary>Misafir kart ataması oluşturur.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MisafirKartYeni(MisafirKartYeniModel model)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();

            try
            {
                _msvc.CreateAssignment(user.FirmaId, model.KartId, model.MisafirAdSoyad, model.GirisSaati, model.Aciklama, model.TCKimlikNo, model.ZiyaretEdilenKisi, model.PasaportNo);
                return Json(new { ok = true, message = "Kayıt başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        /// <summary>TC ile önceki misafir kaydı ön doldurma.</summary>
        [HttpGet]
        public IActionResult GetMisafirByTc(string tc)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();

            if (string.IsNullOrWhiteSpace(tc))
            {
                return Json(new { ok = false, message = "T.C. kimlik numarası boş olamaz." });
            }

            var rec = _msvc.GetMisafirBilgisiByTc(tc);
            if (rec == null)
            {
                return Json(new { ok = false });
            }

            return Json(new
            {
                ok = true,
                misafirAdSoyad = rec.MisafirAdSoyad,
                ziyaretEdilenKisi = rec.ZiyaretEdilenKisi,
                pasaportNo = rec.PasaportNo,
                aciklama = rec.Notlar
            });
        }

        /// <summary>Açık misafir atamalarını güncelleme formu.</summary>
        [HttpGet]
        public IActionResult MisafirKartGuncelle(int? atamaId = null)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();

            var aktifler = _msvc.GetOpenActiveAssignments(user.FirmaId);
            ViewBag.Assignments = aktifler;
            ViewBag.PreselectAtamaId = atamaId;
            return PartialView("_MisafirKartGuncelle", new MisafirKartGuncelleModel { CikisSaati = DateTime.Now });
        }

        /// <summary>Misafir kart atamasını günceller (çıkış saati vb.).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MisafirKartGuncelle(MisafirKartGuncelleModel model)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();

            try
            {
                _msvc.UpdateAssignment(model.AtamaId, model.MisafirAdSoyad, model.GirisSaati, model.CikisSaati, model.Aciklama, model.TCKimlikNo, model.ZiyaretEdilenKisi, model.PasaportNo);
                return Json(new { ok = true, message = "Kayıt güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        /// <summary>Geçmiş ziyaretçi/araç araması (autocomplete).</summary>
        [HttpGet]
        public IActionResult SearchGecmisZiyaretciler(string ad, string tip)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();

            var isArac = string.Equals(tip, "arac", StringComparison.OrdinalIgnoreCase);
            var items = isArac
                ? _aracSvc.SearchGecmisZiyaretciler(user.FirmaId, ad)
                : _msvc.SearchGecmisZiyaretciler(user.FirmaId, ad);

            return Json(new
            {
                ok = true,
                items = (items ?? new List<GecmisZiyaretciItem>()).Select(x => new
                {
                    adSoyad = x.AdSoyad,
                    tcKimlikNo = x.TCKimlikNo,
                    pasaportNo = x.PasaportNo,
                    ziyaretEdilenKisi = x.ZiyaretEdilenKisi,
                    plaka = x.Plaka,
                    notlar = x.Notlar,
                    gosterim = x.Gosterim
                })
            });
        }

        /// <summary>Yeni araç kartı atama partial formu.</summary>
        [HttpGet]
        public IActionResult AracKartiYeni(string personelId = null)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();

            ViewBag.Cards = _aracSvc.GetCardsForNew(user.FirmaId);
            ViewBag.PreselectPersonelId = personelId;
            return PartialView("_AracKartiYeni", new AracKartiYeniModel { GirisSaati = DateTime.Now });
        }

        /// <summary>Araç kart ataması oluşturur.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AracKartiYeni(AracKartiYeniModel model)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();

            try
            {
                _aracSvc.CreateAssignment(user.FirmaId, model.KartId, model.AdSoyad, model.GirisSaati, model.Aciklama, model.TCKimlikNo, model.ZiyaretEdilenKisi, model.Plaka, model.PasaportNo);
                return Json(new { ok = true, message = "Kayıt başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        /// <summary>TC ile önceki araç ziyareti ön doldurma.</summary>
        [HttpGet]
        public IActionResult GetAracByTc(string tc)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();

            if (string.IsNullOrWhiteSpace(tc))
                return Json(new { ok = false, message = "T.C. kimlik numarası boş olamaz." });

            var rec = _aracSvc.GetBilgisiByTc(tc);
            if (rec == null)
                return Json(new { ok = false });

            return Json(new
            {
                ok = true,
                adSoyad = rec.MisafirAdSoyad,
                ziyaretEdilenKisi = rec.ZiyaretEdilenKisi,
                plaka = rec.Plaka,
                pasaportNo = rec.PasaportNo,
                aciklama = rec.Notlar
            });
        }

        /// <summary>Açık araç atamalarını güncelleme formu.</summary>
        [HttpGet]
        public IActionResult AracKartiGuncelle(int? atamaId = null)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();

            ViewBag.Assignments = _aracSvc.GetOpenActiveAssignments(user.FirmaId);
            ViewBag.PreselectAtamaId = atamaId;
            return PartialView("_AracKartiGuncelle", new AracKartiGuncelleModel { CikisSaati = DateTime.Now });
        }

        /// <summary>Araç kart atamasını günceller.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AracKartiGuncelle(AracKartiGuncelleModel model)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();

            try
            {
                _aracSvc.UpdateAssignment(model.AtamaId, model.AdSoyad, model.GirisSaati, model.CikisSaati, model.Aciklama, model.TCKimlikNo, model.ZiyaretEdilenKisi, model.Plaka, model.PasaportNo);
                return Json(new { ok = true, message = "Kayıt güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        private List<LastPassDTO> GetLastPassesInternal(int firmaId, int take)
        {
            var user = GetUser();
            var rol = user?.Rol;
            if (CanliIzlemeRoleHelper.IsArac(rol))
                return _svc.GetLastPassesArac(firmaId, take);
            if (CanliIzlemeRoleHelper.IsYemekhane(rol))
                return _svc.GetLastPassesYemekhane(firmaId, take);
            return _svc.GetLastPasses(firmaId, take);
        }

        private List<KisiHareketDTO> GetLastMovesInternal(int firmaId, int top)
        {
            var user = GetUser();
            var rol = user?.Rol;
            if (CanliIzlemeRoleHelper.IsArac(rol) && !CanliIzlemeRoleHelper.IsDanisma(rol))
                return _khsvc.GetLastMovesByFirmaArac(top, firmaId);
            if (CanliIzlemeRoleHelper.IsYemekhane(rol) && !CanliIzlemeRoleHelper.IsDanisma(rol))
                return _khsvc.GetLastMovesByFirmaYemekhane(top, firmaId);
            return _khsvc.GetLastMovesByFirma(top, firmaId);
        }

        // Ana uygulama SessionContext'ten bağımsız canlı izleme oturumu
        private AuthUserDTO GetUser()
        {
            try
            {
                var json = HttpContext?.Session?.GetString(SessionKey);
                if (string.IsNullOrWhiteSpace(json)) return null;
                return JsonSerializer.Deserialize<AuthUserDTO>(json);
            }
            catch
            {
                return null;
            }
        }

        private void SaveUser(AuthUserDTO user)
        {
            var json = JsonSerializer.Serialize(user);
            HttpContext.Session.SetString(SessionKey, json);
        }

        private void ClearUser()
        {
            HttpContext.Session.Remove(SessionKey);
        }

        /// <summary>Aktif misafir/araç kart atama listesi (JSON).</summary>
        [HttpGet]
        public IActionResult AtamaListe(string tip = "misafir")
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();
            var list = KartAtamaListePresenter.Load(_msvc, _aracSvc, _kartKomutSvc, user.FirmaId, tip);
            return Json(list);
        }

        /// <summary>Terminal kart kısıtlama/kaldırma komutunu kuyruğa alır.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult KartKomut(string personelId, bool pasif)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (CanliIzlemeRoleHelper.HideKartAtama(user?.Rol)) return Forbid();
            var pid = (personelId ?? "").Trim();
            if (string.IsNullOrEmpty(pid))
                return Json(new { ok = false, message = "PersonelId gerekli." });
            try
            {
                if (pasif)
                    _kartKomutSvc.EnqueuePasif(user.FirmaId, pid, user.KullaniciId);
                else
                    _kartKomutSvc.EnqueueAktif(user.FirmaId, pid, user.KullaniciId);
                return Json(new
                {
                    ok = true,
                    message = pasif ? "Kısıtlama komutu kuyruğa alındı." : "Kısıt kaldırma komutu kuyruğa alındı."
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        private static List<(int Id, string Ad)> ToFirmaOptions(DataTable dt)
        {
            var list = new List<(int, string)>();
            if (dt == null) return list;

            bool hasId = dt.Columns.Contains("FirmaId");
            bool hasAd = dt.Columns.Contains("FirmaAdi");
            if (!hasId || !hasAd) return list;

            foreach (DataRow r in dt.Rows)
            {
                int id = r["FirmaId"] == DBNull.Value ? 0 : Convert.ToInt32(r["FirmaId"]);
                string ad = r["FirmaAdi"] == DBNull.Value ? "" : r["FirmaAdi"].ToString();
                if (id > 0) list.Add((id, ad));
            }
            return list;
        }
    }
}

