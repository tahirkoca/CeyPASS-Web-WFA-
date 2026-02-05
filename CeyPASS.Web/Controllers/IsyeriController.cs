using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Models.POY;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.Web.Controllers
{
    public class IsyeriController : Controller
    {
        private readonly IIsyeriService _isyeriService;
        private readonly IFirmaService _firmaService;
        private readonly IAuthorizationService _authorizationService;
        private const string PageName = "Isyerler";

        public IsyeriController(
            IIsyeriService isyeriService,
            IFirmaService firmaService,
            IAuthorizationService authorizationService)
        {
            _isyeriService = isyeriService;
            _firmaService = firmaService;
            _authorizationService = authorizationService;
        }

        public IActionResult Index(int? firmaId = null)
        {
            if (!_authorizationService.ViewAbility(PageName))
            {
                TempData["Error"] = "İşyerleri ekranını görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            var all = ReadAllIsyerleri();
            var selectedFirmaId = firmaId;
            if (selectedFirmaId.HasValue)
            {
                all = all.Where(x => x.FirmaId == selectedFirmaId.Value).ToList();
            }

            ViewBag.Firmalar = _firmaService.GetLookup() ?? new List<CeyPASS.Entities.Concrete.LookupItem>();
            ViewBag.SelectedFirmaId = selectedFirmaId;
            ViewBag.CanCreate = _authorizationService.Can(PageName, YetkiTipleri.Create);
            ViewBag.CanUpdate = _authorizationService.Can(PageName, YetkiTipleri.Update);
            ViewBag.CanDelete = _authorizationService.Can(PageName, YetkiTipleri.Delete);
            return View(all.OrderBy(x => x.FirmaId).ThenBy(x => x.Ad).ToList());
        }

        [HttpGet]
        public IActionResult Create(int? firmaId = null, string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "İşyeri ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            ViewBag.Firmalar = _firmaService.GetLookup() ?? new List<CeyPASS.Entities.Concrete.LookupItem>();
            return View(new IsyeriFormModel { FirmaId = firmaId ?? 0, ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IsyeriFormModel model)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Create))
            {
                TempData["Error"] = "İşyeri ekleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (model.FirmaId <= 0) ModelState.AddModelError(nameof(model.FirmaId), "Firma seçiniz.");
            if (model.IsyeriId <= 0) ModelState.AddModelError(nameof(model.IsyeriId), "İşyeri Id giriniz.");
            if (string.IsNullOrWhiteSpace(model.IsyeriAdi)) ModelState.AddModelError(nameof(model.IsyeriAdi), "İşyeri adı boş olamaz.");

            if (!ModelState.IsValid)
            {
                ViewBag.Firmalar = _firmaService.GetLookup() ?? new List<CeyPASS.Entities.Concrete.LookupItem>();
                return View(model);
            }

            if (!_isyeriService.AddManual(model.FirmaId, model.IsyeriId, model.IsyeriAdi.Trim()))
            {
                ModelState.AddModelError(string.Empty, "İşyeri ekleme başarısız.");
                ViewBag.Firmalar = _firmaService.GetLookup() ?? new List<CeyPASS.Entities.Concrete.LookupItem>();
                return View(model);
            }

            TempData["Success"] = "İşyeri kaydedildi.";
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Index", new { firmaId = model.FirmaId });
        }

        [HttpGet]
        public IActionResult Edit(int firmaId, int isyeriId, string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "İşyeri güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var row = ReadAllIsyerleri().FirstOrDefault(x => x.FirmaId == firmaId && x.IsyeriId == isyeriId);
            if (row == null) return NotFound();

            var model = new IsyeriFormModel
            {
                FirmaId = row.FirmaId,
                IsyeriId = row.IsyeriId,
                IsyeriAdi = row.Ad,
                ReturnUrl = returnUrl
            };

            ViewBag.Firmalar = _firmaService.GetLookup() ?? new List<CeyPASS.Entities.Concrete.LookupItem>();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(IsyeriFormModel model)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Update))
            {
                TempData["Error"] = "İşyeri güncelleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (model.FirmaId <= 0) ModelState.AddModelError(nameof(model.FirmaId), "Firma seçiniz.");
            if (model.IsyeriId <= 0) ModelState.AddModelError(nameof(model.IsyeriId), "İşyeri Id geçersiz.");
            if (string.IsNullOrWhiteSpace(model.IsyeriAdi)) ModelState.AddModelError(nameof(model.IsyeriAdi), "İşyeri adı boş olamaz.");

            if (!ModelState.IsValid)
            {
                ViewBag.Firmalar = _firmaService.GetLookup() ?? new List<CeyPASS.Entities.Concrete.LookupItem>();
                return View(model);
            }

            if (!_isyeriService.Update(model.FirmaId, model.IsyeriId, model.IsyeriAdi.Trim()))
            {
                ModelState.AddModelError(string.Empty, "İşyeri güncelleme başarısız.");
                ViewBag.Firmalar = _firmaService.GetLookup() ?? new List<CeyPASS.Entities.Concrete.LookupItem>();
                return View(model);
            }

            TempData["Success"] = "İşyeri güncellendi.";
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Index", new { firmaId = model.FirmaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int firmaId, int isyeriId, string returnUrl = null)
        {
            if (!_authorizationService.Can(PageName, YetkiTipleri.Delete))
            {
                TempData["Error"] = "İşyeri silme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                var ok = _isyeriService.Delete(firmaId, isyeriId);
                TempData[ok ? "Success" : "Error"] = ok ? "İşyeri silindi." : "İşyeri silinemedi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata: " + ex.Message;
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", new { firmaId });
        }

        private List<IsyeriItem> ReadAllIsyerleri()
        {
            DataTable dt = _isyeriService.GetAll() ?? new DataTable();
            var list = new List<IsyeriItem>();

            foreach (DataRow r in dt.Rows)
            {
                int fId = ToInt(r, "FirmaId");
                int iId = ToInt(r, "IsyeriId");
                string ad = ToStr(r, "IsyeriAdi");
                if (iId < 0) continue;
                list.Add(new IsyeriItem(fId, iId, ad));
            }

            return list;
        }

        private static int ToInt(DataRow r, string col, int def = 0)
        {
            if (!r.Table.Columns.Contains(col)) return def;
            var v = r[col];
            return v == null || v == DBNull.Value ? def : Convert.ToInt32(v);
        }

        private static string ToStr(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col)) return string.Empty;
            var v = r[col];
            return v == null || v == DBNull.Value ? string.Empty : v.ToString();
        }
    }
}

