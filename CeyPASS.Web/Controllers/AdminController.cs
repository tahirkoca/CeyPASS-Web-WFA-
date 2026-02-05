using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Models.Admin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CeyPASS.Web.Controllers
{
    /// <summary>
    /// Admin paneli - sadece RolId 1 (süper admin) kullanıcılar erişebilir.
    /// Sekmeli yapıda tüm verilerin tam listesi ve CRUD işlemleri (mevcut sayfa akışları değişmez).
    /// </summary>
    public class AdminController : Controller
    {
        private readonly ISessionContext _sessionContext;
        private readonly IFirmaService _firmaService;
        private readonly IIsyeriService _isyeriService;
        private readonly ICihazService _cihazService;
        private readonly IDepartmanService _departmanService;
        private readonly IPozisyonService _pozisyonService;
        private readonly IResmiTatilService _resmiTatilService;
        private readonly ICalismaStatuService _calismaStatuService;
        private readonly ICalismaSekliService _calismaSekliService;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _env;

        public AdminController(
            ISessionContext sessionContext,
            IFirmaService firmaService,
            IIsyeriService isyeriService,
            ICihazService cihazService,
            IDepartmanService departmanService,
            IPozisyonService pozisyonService,
            IResmiTatilService resmiTatilService,
            ICalismaStatuService calismaStatuService,
            ICalismaSekliService calismaSekliService,
            INotificationService notificationService,
            IWebHostEnvironment env)
        {
            _sessionContext = sessionContext;
            _firmaService = firmaService;
            _isyeriService = isyeriService;
            _cihazService = cihazService;
            _departmanService = departmanService;
            _pozisyonService = pozisyonService;
            _resmiTatilService = resmiTatilService;
            _calismaStatuService = calismaStatuService;
            _calismaSekliService = calismaSekliService;
            _notificationService = notificationService;
            _env = env;
        }

        public IActionResult Index(string tab)
        {
            if (_sessionContext.CurrentUser == null)
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
            {
                TempData["Error"] = "Admin paneline erişim yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            var model = new AdminPanelViewModel
            {
                Firmalar = (_firmaService.GetAll() ?? new List<Firma>()).OrderBy(x => x.FirmaAdi).ToList(),
                Isyeriler = _isyeriService.GetListForAdmin() ?? new List<IsyeriItem>(),
                Cihazlar = _cihazService.GetListe(sadeceAktif: false, firmaId: null) ?? new List<CihazListDTO>(),
                Departmanlar = _departmanService.GetListForAdmin() ?? new List<DepartmanListDTO>(),
                Pozisyonlar = _pozisyonService.GetListForAdmin() ?? new List<PozisyonListDTO>(),
                ResmiTatiller = (_resmiTatilService.GetList(yil: null) ?? new List<ResmiTatilDTO>()).OrderBy(x => x.Tarih).ToList(),
                CalismaStatuleri = _calismaStatuService.GetAll() ?? new List<LookupItem>(),
                CalismaSekilleri = _calismaSekliService.GetAllForAdmin() ?? new List<CalismaSekli>(),
                AktifTab = string.IsNullOrWhiteSpace(tab) ? "firma" : tab.ToLowerInvariant()
            };

            return View(model);
        }

        /// <summary>Güncelleme bildirimi mail önizlemesi (HTML döner, yeni sekmede açılabilir).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuncellemeMailOnizleme(GuncellemeMailViewModel model)
        {
            if (_sessionContext.CurrentUser == null || !IsAdmin())
                return RedirectToAction("Login", "Account");

            var dto = ViewModelToDto(model);
            if (!GuncellemeDogrula(dto, out string hata))
            {
                TempData["Error"] = hata;
                return RedirectToAction("Index", new { tab = "guncellememail" });
            }

            string logoBase64 = GetLogoBase64FromWwwRoot();
            string html = _notificationService.OnizlemeHtmlOlustur(dto, logoBase64);
            return Content(html, "text/html; charset=utf-8");
        }

        /// <summary>Güncelleme bildirimi mailini tüm ilgili alıcılara gönderir.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuncellemeMailGonder(GuncellemeMailViewModel model)
        {
            if (_sessionContext.CurrentUser == null || !IsAdmin())
                return RedirectToAction("Login", "Account");

            var dto = ViewModelToDto(model);
            if (!GuncellemeDogrula(dto, out string hata))
            {
                TempData["Error"] = hata;
                return RedirectToAction("Index", new { tab = "guncellememail" });
            }

            try
            {
                string logoBase64 = GetLogoBase64FromWwwRoot();
                bool basarili = await _notificationService.GuncellemeNotifikasyonuGonderAsync(dto, logoBase64);
                if (basarili)
                    TempData["Success"] = "Güncelleme bildirimi başarıyla gönderildi.";
                else
                    TempData["Error"] = "Mail gönderilirken bir hata oluştu. Lütfen ayarları kontrol edin.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction("Index", new { tab = "guncellememail" });
        }

        private static GuncellemeNotifikasyonDTO ViewModelToDto(GuncellemeMailViewModel m)
        {
            var dto = new GuncellemeNotifikasyonDTO
            {
                VersiyonNumarasi = m.VersiyonNumarasi?.Trim() ?? "",
                YayinTarihi = m.YayinTarihi,
                GuncellemeTipi = m.GuncellemeTipi?.Trim() ?? "",
                EkNotlar = m.EkNotlar?.Trim() ?? ""
            };
            dto.YeniOzellikler = SatirlariListeye(m.YeniOzelliklerMetni);
            dto.Iyilestirmeler = SatirlariListeye(m.IyilestirmelerMetni);
            dto.HataDuzeltmeleri = SatirlariListeye(m.HataDuzeltmeleriMetni);
            dto.KritikDegisiklikler = SatirlariListeye(m.KritikDegisikliklerMetni);
            return dto;
        }

        private static List<string> SatirlariListeye(string metin)
        {
            if (string.IsNullOrWhiteSpace(metin)) return new List<string>();
            return metin.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();
        }

        private static bool GuncellemeDogrula(GuncellemeNotifikasyonDTO dto, out string hata)
        {
            hata = null;
            if (string.IsNullOrWhiteSpace(dto.VersiyonNumarasi)) { hata = "Versiyon numarası giriniz."; return false; }
            if (string.IsNullOrWhiteSpace(dto.GuncellemeTipi)) { hata = "Güncelleme tipini seçiniz."; return false; }
            if (dto.YeniOzellikler.Count == 0 && dto.Iyilestirmeler.Count == 0 && dto.HataDuzeltmeleri.Count == 0 && dto.KritikDegisiklikler.Count == 0)
            { hata = "En az bir kategoriye madde eklemelisiniz (her satır bir madde)."; return false; }
            return true;
        }

        private bool IsAdmin()
        {
            return _sessionContext.RolId == 1;
        }

        /// <summary>wwwroot/images/logo.png veya wwwroot/logo.png dosyasını base64 olarak okur (güncelleme maili başlığı için).</summary>
        private string GetLogoBase64FromWwwRoot()
        {
            try
            {
                string webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath ?? "", "wwwroot");
                string path = Path.Combine(webRoot, "images", "logo.png");
                if (!System.IO.File.Exists(path))
                    path = Path.Combine(webRoot, "logo.png");
                if (!System.IO.File.Exists(path))
                    return null;
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return null;
            }
        }
    }
}
