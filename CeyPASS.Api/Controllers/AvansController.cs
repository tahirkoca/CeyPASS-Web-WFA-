using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Entities.Concrete;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AvansController : ControllerBase
    {
        private readonly IAvansService _avansService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IKisiQueryService _kisiQueryService;
        private const string PageName = "Avans";

        public AvansController(
            IAvansService avansService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IKisiQueryService kisiQueryService)
        {
            _avansService = avansService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _kisiQueryService = kisiQueryService;
        }

        [HttpGet]
        public ActionResult<ApiResult<List<AvansTalepListItem>>> Get()
        {
            if (_authorizationService.ViewAbility(PageName))
            {
                var items = _avansService.TumTalepler();
                return Ok(ApiResult<List<AvansTalepListItem>>.Ok(Map(items)));
            }

            // Yetki yoksa sadece kendi taleplerini görsün
            if (!string.IsNullOrEmpty(_sessionContext.AktifSicilNo))
            {
                var items = _avansService.PersonelTalepleri(_sessionContext.AktifSicilNo);
                return Ok(ApiResult<List<AvansTalepListItem>>.Ok(Map(items)));
            }

            return Forbid();
        }

        [HttpGet("kendi")]
        public ActionResult<ApiResult<List<AvansTalepListItem>>> GetKendi()
        {
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();
            var items = _avansService.PersonelTalepleri(_sessionContext.AktifSicilNo);
            return Ok(ApiResult<List<AvansTalepListItem>>.Ok(Map(items)));
        }

        [HttpPost("talep")]
        public ActionResult<ApiResult<int>> Post([FromBody] AvansTalepRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create)) return Forbid();
            if (string.IsNullOrEmpty(_sessionContext.AktifSicilNo)) return Unauthorized();

            int id = _avansService.TalepOlustur(_sessionContext.AktifSicilNo, request.Miktar, request.Aciklama);
            return Ok(ApiResult<int>.Ok(id, "Avans talebiniz alındı."));
        }

        [HttpPost("onayla")]
        public ActionResult<ApiResult> Onayla([FromBody] AvansOnayRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            bool success = _avansService.Onayla(request.AvansId, _sessionContext.AktifKullaniciId.Value, request.Aciklama);
            return success ? Ok(ApiResult.Ok("Avans onaylandı.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpPost("reddet")]
        public ActionResult<ApiResult> Reddet([FromBody] AvansOnayRequest request)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update)) return Forbid();
            if (!_sessionContext.AktifKullaniciId.HasValue) return Unauthorized();

            bool success = _avansService.Reddet(request.AvansId, _sessionContext.AktifKullaniciId.Value, request.Aciklama);
            return success ? Ok(ApiResult.Ok("Avans reddedildi.")) : BadRequest(ApiResult.Failure("İşlem başarısız."));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResult> Delete(int id)
        {
            bool success = _avansService.IptalEt(id);
            return success ? Ok(ApiResult.Ok("Avans talebi iptal edildi.")) : BadRequest(ApiResult.Failure("Talep iptal edilemedi."));
        }

        public sealed class AvansTalepRequest
        {
            public decimal Miktar { get; set; }
            public string? Aciklama { get; set; }
        }

        public sealed class AvansOnayRequest
        {
            public int AvansId { get; set; }
            public string? Aciklama { get; set; }
        }

        public sealed class AvansTalepListItem
        {
            public int AvansId { get; set; }
            public string PersonelId { get; set; } = "";
            public string PersonelAdSoyad { get; set; } = "";
            public DateTime TalepTarihi { get; set; }
            public decimal Miktar { get; set; }
            public AvansDurumu Durum { get; set; }
            public string? Aciklama { get; set; }
        }

        private List<AvansTalepListItem> Map(List<AvansTalep> items)
        {
            var pIds = items.Select(x => x.PersonelId).Distinct().ToList();
            var names = new Dictionary<string, string>();
            foreach (var id in pIds)
            {
                var k = _kisiQueryService.GetKisiDetay(id);
                if (k != null) names[id] = $"{k.Ad} {k.Soyad}";
            }

            return items.Select(t =>
            {
                var pName = names.TryGetValue(t.PersonelId, out var n) ? n : t.PersonelId;
                return new AvansTalepListItem
                {
                    AvansId = t.AvansId,
                    PersonelId = t.PersonelId,
                    PersonelAdSoyad = pName,
                    TalepTarihi = t.TalepTarihi,
                    Miktar = t.Miktar,
                    Durum = t.Durum,
                    Aciklama = t.Aciklama
                };
            }).ToList();
        }
    }
}
