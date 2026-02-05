using Microsoft.AspNetCore.Mvc;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Web.Models;
using System.Linq;

namespace CeyPASS.Web.ViewComponents
{
    public class FirmaSelectViewComponent : ViewComponent
    {
        private readonly ISessionContext _session;
        private readonly IFirmaService _firmaService;
        private readonly IPuantajService _puantajService;

        public FirmaSelectViewComponent(
            ISessionContext session,
            IFirmaService firmaService,
            IPuantajService puantajService)
        {
            _session = session;
            _firmaService = firmaService;
            _puantajService = puantajService;
        }

        public IViewComponentResult Invoke()
        {
            var vm = new FirmaSelectViewModel { Firmalar = null, SelectedFirmaId = 0, ShowCombo = false };
            if (_session.CurrentUser == null)
                return View("Default", vm);

            bool isAdmin = _session.RolId == 1 || _session.RolId == 2;
            var firmalar = GetFirmalarForUser(isAdmin);
            vm.Firmalar = firmalar;
            vm.ShowCombo = firmalar != null && firmalar.Count > 1;
            vm.SelectedFirmaId = (int)_session.AktifFirmaId;

            if (firmalar != null && firmalar.Count > 0 && !firmalar.Any(f => f.FirmaId == vm.SelectedFirmaId))
                vm.SelectedFirmaId = firmalar[0].FirmaId;

            return View("Default", vm);
        }

        private List<Firma>? GetFirmalarForUser(bool isAdmin)
        {
            if (isAdmin)
                return _firmaService.GetAll().OrderBy(f => f.FirmaAdi).ToList();

            var yetkiler = _puantajService.GetKullaniciFirmaIsyeriYetkileri((int)_session.AktifKullaniciId!);
            var firmaIds = yetkiler.Select(y => y.FirmaId).Distinct().ToHashSet();
            if (firmaIds.Count == 0)
                return null;

            return _firmaService.GetAll()
                .Where(f => firmaIds.Contains(f.FirmaId))
                .OrderBy(f => f.FirmaAdi)
                .ToList();
        }
    }
}
