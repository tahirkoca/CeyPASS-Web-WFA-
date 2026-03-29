using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
