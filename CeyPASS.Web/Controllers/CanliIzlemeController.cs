using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
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
    public class CanliIzlemeController : Controller
    {
        private const string SessionKey = "CanliIzlemeUser";
        private readonly ICanliIzlemeService _svc;
        private readonly IKisiHareketService _khsvc;
        private readonly IKisiDetayService _kdsvc;
        private readonly IMisafirKartService _msvc;

        public CanliIzlemeController(
            ICanliIzlemeService svc,
            IKisiHareketService khsvc,
            IKisiDetayService kdsvc,
            IMisafirKartService msvc)
        {
            _svc = svc;
            _khsvc = khsvc;
            _kdsvc = kdsvc;
            _msvc = msvc;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var firmalar = ToFirmaOptions(_svc.GetFirmalar());
            ViewBag.Firmalar = firmalar;
            return View(new CanliIzlemeLoginModel());
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            ClearUser();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Index()
        {
            var user = GetUser();
            if (user == null) return RedirectToAction(nameof(Login));

            ViewBag.User = user;
            ViewBag.IsYemekhane = IsYemekhaneRole(user?.Rol);
            ViewBag.IsDanisma = IsDanismaRole(user?.Rol);
            ViewBag.CanMisafirKart = !(IsYemekhaneRole(user?.Rol) && !IsDanismaRole(user?.Rol));

            // İlk render (JS zaten 1 sn'de bir yenileyecek)
            ViewBag.LastPasses = GetLastPassesInternal(user.FirmaId, 4);
            ViewBag.LastMoves = GetLastMovesInternal(user.FirmaId, 15);

            return View();
        }

        // JSON endpoints (WinForms Timer = 1000ms polling)
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
                departmanAdi = x.DepartmanAdi,
                unvan = x.Unvan,
                zaman = x.Zaman,
                terminalAdi = x.TerminalAdi,
                girisMi = x.GirisMi,
                fotoBase64 = (x.Foto != null && x.Foto.Length > 0) ? Convert.ToBase64String(x.Foto) : null
            });

            return Json(dto);
        }

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
                departman = x.Departman,
                unvan = x.Unvan,
                cihazAdi = x.CihazAdi,
                kisiId = x.PersonelId
            });

            return Json(dto);
        }

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
                departman = dto.Departman,
                fotoBase64 = (dto.Foto != null && dto.Foto.Length > 0) ? Convert.ToBase64String(dto.Foto) : null
            });
        }

        // Misafir Kart (WinForms dialog)
        [HttpGet]
        public IActionResult MisafirKartYeni()
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (IsYemekhaneRole(user?.Rol) && !IsDanismaRole(user?.Rol)) return Forbid();

            var cards = _msvc.GetCardsForNew(user.FirmaId);
            ViewBag.Cards = cards;
            return PartialView("_MisafirKartYeni", new MisafirKartYeniModel { GirisSaati = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MisafirKartYeni(MisafirKartYeniModel model)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (IsYemekhaneRole(user?.Rol) && !IsDanismaRole(user?.Rol)) return Forbid();

            try
            {
                _msvc.CreateAssignment(user.FirmaId, model.KartId, model.MisafirAdSoyad, model.GirisSaati, model.Aciklama);
                return Json(new { ok = true, message = "Kayıt başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult MisafirKartGuncelle()
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (IsYemekhaneRole(user?.Rol) && !IsDanismaRole(user?.Rol)) return Forbid();

            var aktifler = _msvc.GetTodayActiveAssignments(DateTime.Now, user.FirmaId);
            ViewBag.Assignments = aktifler;
            return PartialView("_MisafirKartGuncelle", new MisafirKartGuncelleModel { CikisSaati = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MisafirKartGuncelle(MisafirKartGuncelleModel model)
        {
            var user = GetUser();
            if (user == null) return Unauthorized();
            if (IsYemekhaneRole(user?.Rol) && !IsDanismaRole(user?.Rol)) return Forbid();

            try
            {
                _msvc.UpdateAssignment(model.AtamaId, model.MisafirAdSoyad, model.GirisSaati, model.CikisSaati, model.Aciklama);
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
            bool yemekhane = IsYemekhaneRole(user?.Rol);
            return yemekhane ? _svc.GetLastPassesYemekhane(firmaId, take) : _svc.GetLastPasses(firmaId, take);
        }

        private List<KisiHareketDTO> GetLastMovesInternal(int firmaId, int top)
        {
            var user = GetUser();
            bool yemekhane = IsYemekhaneRole(user?.Rol) && !IsDanismaRole(user?.Rol);
            return yemekhane ? _khsvc.GetLastMovesByFirmaYemekhane(top, firmaId) : _khsvc.GetLastMovesByFirma(top, firmaId);
        }

        private static bool IsYemekhaneRole(string rolAdi) =>
            string.Equals(rolAdi ?? string.Empty, "YEMEKHANE", StringComparison.OrdinalIgnoreCase);

        private static bool IsDanismaRole(string rolAdi)
        {
            var r = rolAdi ?? "";
            return r.IndexOf("DANIŞMA", StringComparison.OrdinalIgnoreCase) >= 0
                   || r.IndexOf("DANISMA", StringComparison.OrdinalIgnoreCase) >= 0;
        }

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

