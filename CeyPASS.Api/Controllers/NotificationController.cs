using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IBildirimService _bildirimService;
        private readonly ISessionContext _sessionContext;

        public NotificationController(
            IBildirimService bildirimService,
            ISessionContext sessionContext)
        {
            _bildirimService = bildirimService;
            _sessionContext = sessionContext;
        }

        [HttpGet]
        public ActionResult<ApiResult<List<Bildirim>>> Get()
        {
            var pId = _sessionContext.AktifSicilNo;
            var uId = _sessionContext.AktifKullaniciId;
            var items = _bildirimService.GetMyNotifications(pId, uId);
            return Ok(ApiResult<List<Bildirim>>.Ok(items));
        }

        public sealed class NotificationHistoryItem
        {
            public int Id { get; set; }
            public string? Baslik { get; set; }
            public string? Mesaj { get; set; }
            public bool OkunduMu { get; set; }
            public string? Tarih { get; set; }
            public string? Tipi { get; set; }
        }

        public sealed class NotificationHistoryResponse
        {
            public List<NotificationHistoryItem> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int TotalPages { get; set; }
            public int CurrentPage { get; set; }
            public int PageSize { get; set; }
        }

        [HttpGet("history")]
        public ActionResult<ApiResult<NotificationHistoryResponse>> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pId = _sessionContext.AktifSicilNo;
            var uId = _sessionContext.AktifKullaniciId;
            if (string.IsNullOrEmpty(pId) && !uId.HasValue) return Unauthorized();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var all = _bildirimService.GetMyNotifications(pId, uId)
                .OrderByDescending(x => x.OlusturmaTarihi);

            var totalCount = all.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var items = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new NotificationHistoryItem
                {
                    Id = b.Id,
                    Baslik = b.Baslik,
                    Mesaj = b.Mesaj,
                    OkunduMu = b.OkunduMu,
                    Tarih = b.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm", CultureInfo.GetCultureInfo("tr-TR")),
                    Tipi = b.Tipi
                })
                .ToList();

            var res = new NotificationHistoryResponse
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };

            return Ok(ApiResult<NotificationHistoryResponse>.Ok(res));
        }

        [HttpGet("unread-count")]
        public ActionResult<ApiResult<int>> GetUnreadCount()
        {
            var pId = _sessionContext.AktifSicilNo;
            var uId = _sessionContext.AktifKullaniciId;
            var count = _bildirimService.GetUnreadCount(pId, uId);
            return Ok(ApiResult<int>.Ok(count));
        }

        [HttpPost("read/{id}")]
        public ActionResult<ApiResult> MarkAsRead(int id)
        {
            _bildirimService.MarkAsRead(id);
            return Ok(ApiResult.Ok("Bildirim okundu olarak işaretlendi."));
        }

        [HttpPost("read-all")]
        public ActionResult<ApiResult> MarkAllAsRead()
        {
            var pId = _sessionContext.AktifSicilNo;
            var uId = _sessionContext.AktifKullaniciId;
            _bildirimService.MarkAllAsRead(pId, uId);
            return Ok(ApiResult.Ok("Tüm bildirimleriniz okundu olarak işaretlendi."));
        }

        [HttpPost("register-device")]
        public ActionResult<ApiResult> RegisterDevice([FromBody] string deviceToken)
        {
            // Note: Normally we'd store this in a database table like 'UserDevices'
            // For now, providing the endpoint for future expansion.
            return Ok(ApiResult.Ok("Cihaz başarıyla kaydedildi."));
        }
    }
}
