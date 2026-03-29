using CeyPASS.Business.Abstractions;
using CeyPASS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CeyPASS.Web.Controllers
{
    public class BildirimController : Controller
    {
        private readonly IBildirimService _bildirimService;
        private readonly ISessionContext _session;

        public BildirimController(IBildirimService bildirimService, ISessionContext session)
        {
            _bildirimService = bildirimService;
            _session = session;
        }

        [HttpGet]
        public IActionResult GetMyNotifications()
        {
            if (string.IsNullOrEmpty(_session.AktifSicilNo) && !_session.AktifKullaniciId.HasValue)
                return Unauthorized();

            var list = _bildirimService.GetMyNotifications(_session.AktifSicilNo, _session.AktifKullaniciId)
                .Take(10) // Sadece son 10 bildirim
                .Select(b => new {
                    b.Id,
                    b.Baslik,
                    b.Mesaj,
                    b.OkunduMu,
                    Tarih = b.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm"),
                    b.Tipi
                });

            var unreadCount = _bildirimService.GetUnreadCount(_session.AktifSicilNo, _session.AktifKullaniciId);

            return Json(new { items = list, unreadCount });
        }

        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            _bildirimService.MarkAsRead(id);
            return Ok();
        }

        [HttpPost]
        public IActionResult MarkAllAsRead()
        {
            _bildirimService.MarkAllAsRead(_session.AktifSicilNo, _session.AktifKullaniciId);
            return Ok();
        }

        [HttpGet]
        public IActionResult GetAllNotifications(int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(_session.AktifSicilNo) && !_session.AktifKullaniciId.HasValue)
                return Unauthorized();

            if (page < 1) page = 1;

            var allNotifications = _bildirimService.GetMyNotifications(_session.AktifSicilNo, _session.AktifKullaniciId);
            var totalCount = allNotifications.Count();
            var totalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);

            var list = allNotifications
                .OrderByDescending(x => x.OlusturmaTarihi)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new {
                    b.Id,
                    b.Baslik,
                    b.Mesaj,
                    b.OkunduMu,
                    Tarih = b.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm"),
                    b.Tipi
                });

            return Json(new { 
                items = list, 
                totalCount, 
                totalPages, 
                currentPage = page,
                pageSize
            });
        }
    }
}
