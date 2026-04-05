using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CeyPASS.Entities.Concrete;
using CeyPASS.Business.Abstractions;
using IAuthorizationService = CeyPASS.Business.Abstractions.IAuthorizationService;
using CeyPASS.Models;
using CeyPASS.Infrastructure.Helpers;
using CeyPASS.Infrastructure.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Microsoft.AspNetCore.Hosting;
using CeyPASS.DataAccess.Abstractions;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using CeyPASS.Api.Services;

namespace CeyPASS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class IzinKagitController : ControllerBase
    {
        private readonly IIzinTalepService _izinTalepService;
        private readonly IIzinTalepRepository _izinTalepRepo;
        private readonly IKisiRepository _kisiRepo;
        private readonly IKisiIzinlerRepository _kisiIzinRepo;
        private readonly IIzinTipService _izinTipService;
        private readonly IPozisyonService _pozisyonService;
        private readonly ISessionContext _sessionContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWebHostEnvironment _env;
        private readonly IRazorViewToStringRenderer _viewRenderer;
        private readonly IPlaywrightPdfService _playwrightPdf;

        public IzinKagitController(
            IIzinTalepService izinTalepService,
            IIzinTalepRepository izinTalepRepo,
            IKisiRepository kisiRepo,
            IKisiIzinlerRepository kisiIzinRepo,
            IIzinTipService izinTipService,
            IPozisyonService pozisyonService,
            ISessionContext sessionContext,
            IAuthorizationService authorizationService,
            IWebHostEnvironment env,
            IRazorViewToStringRenderer viewRenderer,
            IPlaywrightPdfService playwrightPdf)
        {
            _izinTalepService = izinTalepService;
            _izinTalepRepo = izinTalepRepo;
            _kisiRepo = kisiRepo;
            _kisiIzinRepo = kisiIzinRepo;
            _izinTipService = izinTipService;
            _pozisyonService = pozisyonService;
            _sessionContext = sessionContext;
            _authorizationService = authorizationService;
            _env = env;
            _viewRenderer = viewRenderer;
            _playwrightPdf = playwrightPdf;
        }

        [HttpGet("pdf/{talepId}")]
        public async Task<IActionResult> GetPdf(int talepId)
        {
            var (talep, model) = ResolveTalepAndModel(talepId);
            if (talep == null) return NotFound(ApiResult.Failure("Talep bulunamadı veya erişim yetkiniz yok."));

            var isMazeretSaatlik = talep.SaatlikIzinMi && talep.IzinTipId == 7;
            var viewPath = isMazeretSaatlik ? "/Views/IzinKagit/MazeretSaatlik.cshtml" : "/Views/IzinKagit/KlasikGunluk.cshtml";
            var logoDataUri = ResolveLogoDataUri();

            var html = await _viewRenderer.RenderAsync(viewPath, talep, vd =>
            {
                vd["PersonelAdSoyad"] = model.AdSoyad;
                vd["PersonelGorev"] = model.Gorev;
                vd["PersonelTc"] = model.TcKimlikNo;
                vd["PersonelCep"] = model.CepTel;
                vd["IzinTipAdi"] = model.IzinTipAdi;
                vd["UstYetkiliAdSoyad"] = model.UstYetkiliAdSoyad;
                vd["LogoSrc"] = logoDataUri;
            });

            byte[] bytes;
            try
            {
                bytes = await _playwrightPdf.HtmlToPdfAsync(html);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult.Failure($"PDF üretilemedi (Playwright/Chromium). Sunucuda Chromium kurulu olmalı veya PLAYWRIGHT_BROWSERS_PATH ayarlı olmalı. Detay: {ex.Message}"));
            }

            var fileName = isMazeretSaatlik
                ? $"SaatlikMazeret_{talep.TalepId}.pdf"
                : $"PersonelIzinFormu_{talep.TalepId}.pdf";

            return File(bytes, "application/pdf", fileName);
        }

        [HttpGet("pdf-from-izin/{kisiIzinId}")]
        public async Task<IActionResult> GetPdfFromIzin(int kisiIzinId)
        {
            var (talep, model) = ResolveTalepAndModelFromIzin(kisiIzinId);
            if (talep == null) return NotFound(ApiResult.Failure("İzin kaydı bulunamadı veya erişim yetkiniz yok."));

            var isMazeretSaatlik = talep.SaatlikIzinMi && talep.IzinTipId == 7;
            var viewPath = isMazeretSaatlik ? "/Views/IzinKagit/MazeretSaatlik.cshtml" : "/Views/IzinKagit/KlasikGunluk.cshtml";
            var logoDataUri = ResolveLogoDataUri();

            var html = await _viewRenderer.RenderAsync(viewPath, talep, vd =>
            {
                vd["PersonelAdSoyad"] = model.AdSoyad;
                vd["PersonelGorev"] = model.Gorev;
                vd["PersonelTc"] = model.TcKimlikNo;
                vd["PersonelCep"] = model.CepTel;
                vd["IzinTipAdi"] = model.IzinTipAdi;
                vd["UstYetkiliAdSoyad"] = model.UstYetkiliAdSoyad;
                vd["LogoSrc"] = logoDataUri;
            });

            byte[] bytes;
            try
            {
                bytes = await _playwrightPdf.HtmlToPdfAsync(html);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult.Failure($"PDF üretilemedi (Playwright/Chromium). Sunucuda Chromium kurulu olmalı veya PLAYWRIGHT_BROWSERS_PATH ayarlı olmalı. Detay: {ex.Message}"));
            }

            var fileName = isMazeretSaatlik
                ? $"SaatlikMazeret_{kisiIzinId}.pdf"
                : $"PersonelIzinFormu_{kisiIzinId}.pdf";

            return File(bytes, "application/pdf", fileName);
        }

        private string ResolveLogoDataUri()
        {
            // API PDF'i, dış resource fetch'e ihtiyaç duymadan üretsin (mobil için daha güvenli).
            var apiPng = Path.Combine(_env.ContentRootPath, "wwwroot", "images", "ceyLogo.png");
            var webPng = Path.Combine(_env.ContentRootPath, "..", "CeyPASS.Web", "wwwroot", "images", "ceyLogo.png");
            var wfaPng = Path.Combine(_env.ContentRootPath, "..", "CeyPASS.WFA", "Resources", "ceyLogo.png");

            var pngPath =
                System.IO.File.Exists(apiPng) ? apiPng :
                System.IO.File.Exists(webPng) ? webPng :
                System.IO.File.Exists(wfaPng) ? wfaPng :
                null;

            if (pngPath != null)
            {
                var bytes = System.IO.File.ReadAllBytes(pngPath);
                return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
            }

            var apiIco = Path.Combine(_env.ContentRootPath, "wwwroot", "images", "ceyLogo.ico");
            var webIco = Path.Combine(_env.ContentRootPath, "..", "CeyPASS.Web", "wwwroot", "images", "ceyLogo.ico");
            var icoPath =
                System.IO.File.Exists(apiIco) ? apiIco :
                System.IO.File.Exists(webIco) ? webIco :
                null;
            if (icoPath != null)
            {
                var bytes = System.IO.File.ReadAllBytes(icoPath);
                return $"data:image/x-icon;base64,{Convert.ToBase64String(bytes)}";
            }

            return "/images/ceyLogo.png";
        }

        private (IzinTalep? Talep, PreviewModel Model) ResolveTalepAndModelFromIzin(int kisiIzinId)
        {
            var talep = _izinTalepRepo.GetBySonucKisiIzinId(kisiIzinId);
            if (talep != null)
            {
                return ResolveTalepAndModel(talep.TalepId);
            }

            var izin = _kisiIzinRepo.GetById(kisiIzinId);
            if (izin == null) return (null, new PreviewModel());

            var canIkView = _authorizationService.ViewAbility("IzinTalepleri");
            if (!canIkView && !string.Equals(izin.PersonelId, _sessionContext.AktifSicilNo, StringComparison.OrdinalIgnoreCase))
            {
                return (null, new PreviewModel());
            }

            talep = new IzinTalep
            {
                TalepId = 0,
                PersonelId = izin.PersonelId,
                FirmaId = izin.FirmaId,
                IzinTipId = izin.IzinId,
                Baslangic = izin.Baslangic,
                Bitis = izin.Bitis,
                SaatlikIzinMi = izin.SaatlikIzinMi,
                Aciklama = izin.Aciklama
            };

            var model = ResolvePreviewModel(talep);
            return (talep, model);
        }

        private (IzinTalep? Talep, PreviewModel Model) ResolveTalepAndModel(int talepId)
        {
            var canIkView = _authorizationService.ViewAbility("IzinTalepleri");

            IzinTalep? talep = null;
            if (canIkView)
            {
                talep = _izinTalepRepo.GetById(talepId);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_sessionContext.AktifSicilNo))
                    return (null, new PreviewModel());

                talep = _izinTalepService.PersonelTalepleri(_sessionContext.AktifSicilNo).Find(t => t.TalepId == talepId);
                if (talep == null)
                {
                    var bekleyenler = _izinTalepService.UstYetkiliBekleyenler(_sessionContext.AktifSicilNo);
                    talep = bekleyenler?.Find(t => t.TalepId == talepId);
                }
            }

            if (talep == null) return (null, new PreviewModel());

            var model = ResolvePreviewModel(talep);

            if (!string.IsNullOrWhiteSpace(talep.UstYetkiliPersonelId))
            {
                var uyDetay = _kisiRepo.GetDetay(talep.UstYetkiliPersonelId);
                if (uyDetay != null)
                    model.UstYetkiliAdSoyad = $"{(uyDetay.Ad ?? "").Trim()} {(uyDetay.Soyad ?? "").Trim()}".Trim();
            }

            return (talep, model);
        }

        private PreviewModel ResolvePreviewModel(IzinTalep talep)
        {
            var kisiDetay = _kisiRepo.GetDetay(talep.PersonelId);
            var adSoyad = kisiDetay != null ? $"{(kisiDetay.Ad ?? "").Trim()} {(kisiDetay.Soyad ?? "").Trim()}".Trim() : talep.PersonelId;

            string? pozisyonAdi = null;
            if (kisiDetay?.PozisyonId != null)
            {
                var all = _pozisyonService.GetAll();
                pozisyonAdi = all.FirstOrDefault(p => p.Id == kisiDetay.PozisyonId.Value)?.Ad;
            }

            var izinTipAdi = talep.IzinTipId.HasValue
                ? _izinTipService.GetAktif().FirstOrDefault(x => x.IzinTipId == talep.IzinTipId.Value)?.Ad
                : null;

            return new PreviewModel
            {
                AdSoyad = adSoyad,
                Gorev = pozisyonAdi ?? "",
                TcKimlikNo = kisiDetay?.TcKimlikNo ?? "",
                CepTel = kisiDetay?.CepTel ?? "",
                IzinTipAdi = izinTipAdi ?? (talep.IzinTipId?.ToString() ?? "")
            };
        }

        private sealed class PreviewModel
        {
            public string AdSoyad { get; set; } = "";
            public string Gorev { get; set; } = "";
            public string TcKimlikNo { get; set; } = "";
            public string CepTel { get; set; } = "";
            public string IzinTipAdi { get; set; } = "";
            public string UstYetkiliAdSoyad { get; set; } = "Üst Yetkili";
        }

        private static Document BuildBaseDoc(string title)
        {
            var doc = new Document();
            doc.Info.Title = title;
            var normal = doc.Styles["Normal"]!;
            normal.Font.Name = "Arial";
            normal.Font.Size = 10;
            return doc;
        }

        private static Table NewTable(Section sec)
        {
            var t = sec.AddTable();
            t.Borders.Width = 0.75;
            return t;
        }

        private static Document BuildMazeretSaatlikPdf(IzinTalep t, PreviewModel m, string? logoPath = null)
        {
            var doc = BuildBaseDoc("Saatlik Mazeret Formu");
            var sec = doc.AddSection();
            sec.PageSetup.PageFormat = PageFormat.A4;
            sec.PageSetup.Orientation = Orientation.Portrait;
            sec.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            sec.PageSetup.RightMargin = Unit.FromCentimeter(1.5);
            sec.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            sec.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);

            var headerTable = sec.AddTable();
            headerTable.Borders.Width = 0.75;
            headerTable.AddColumn(Unit.FromCentimeter(3.0));
            headerTable.AddColumn(Unit.FromCentimeter(9.0));
            headerTable.AddColumn(Unit.FromCentimeter(5.8));

            var hRow = headerTable.AddRow();
            hRow.VerticalAlignment = VerticalAlignment.Center;
            hRow.Height = Unit.FromCentimeter(2.0);

            var logoCell = hRow.Cells[0];
            logoCell.Format.Alignment = ParagraphAlignment.Center;
            if (!string.IsNullOrEmpty(logoPath) && System.IO.File.Exists(logoPath))
            {
                try
                {
                    var img = logoCell.AddParagraph().AddImage(logoPath);
                    img.Height = Unit.FromCentimeter(1.4);
                    img.LockAspectRatio = true;
                    // Bazı durumlarda logo genişliği hücreyi "itip" header çizgilerinde taşma/kayma oluşturabiliyor.
                    // Genişliği de sınırlayıp header kolonlarını sabit tut.
                    img.Width = Unit.FromCentimeter(2.6);
                }
                catch { logoCell.AddParagraph("LOGO"); }
            }
            else
            {
                logoCell.AddParagraph("CEYNAK").Format.Font.Bold = true;
            }

            var titleCell = hRow.Cells[1];
            titleCell.Format.Alignment = ParagraphAlignment.Center;
            titleCell.VerticalAlignment = VerticalAlignment.Center;
            var titlePar = titleCell.AddParagraph("SAATLİK MAZERET FORMU");
            titlePar.Format.Font.Bold = true;
            titlePar.Format.Font.Size = 14;
            titlePar.Format.Alignment = ParagraphAlignment.Center;

            var docCell = hRow.Cells[2];
            var docInner = docCell.Elements.AddTable();
            docInner.Borders.Width = 0;
            docInner.AddColumn(Unit.FromCentimeter(2.9));
            docInner.AddColumn(Unit.FromCentimeter(2.9));

            void DocRow(string label, string val)
            {
                var dr = docInner.AddRow();
                dr.Borders.Bottom.Width = 0.5;
                var lp = dr.Cells[0].AddParagraph(label);
                lp.Format.Font.Bold = true;
                lp.Format.Font.Size = 8;
                var vp = dr.Cells[1].AddParagraph(val);
                vp.Format.Font.Size = 8;
            }
            DocRow("Döküman No", "İKY.FR.050");
            DocRow("Yayın Tarihi", "01.07.2021");
            DocRow("Revizyon No", "0");
            DocRow("Rev. Tarihi", "01.07.2021");
            DocRow("Sayfa No", "1/1");

            var fTitlePar = sec.AddParagraph("SAATLİK MAZERET FORMU");
            fTitlePar.Format.Font.Bold = true;
            fTitlePar.Format.Font.Size = 12;
            fTitlePar.Format.Alignment = ParagraphAlignment.Center;
            fTitlePar.Format.Borders.Bottom.Width = 0.75;
            fTitlePar.Format.Borders.Top.Width = 0.75;
            fTitlePar.Format.SpaceBefore = Unit.FromPoint(4);
            fTitlePar.Format.SpaceAfter = Unit.FromPoint(4);

            var formTable = sec.AddTable();
            formTable.Borders.Width = 0.75;
            formTable.AddColumn(Unit.FromCentimeter(6.5));
            formTable.AddColumn(Unit.FromCentimeter(11.3));

            void FormRow(string label, string value, double? heightCm = null)
            {
                var r = formTable.AddRow();
                if (heightCm.HasValue) r.Height = Unit.FromCentimeter(heightCm.Value);
                var lp = r.Cells[0].AddParagraph(label);
                lp.Format.Font.Bold = true;
                r.Cells[1].AddParagraph(value ?? "");
            }

            FormRow("Adı Soyadı", m.AdSoyad);
            FormRow("Görevi", m.Gorev);
            FormRow("Mazereti", t.Aciklama ?? "", 1.8);
            // Match Web PDF formatting exactly
            FormRow("Tarih", t.Baslangic.ToString("dd.MM.yyyy"));
            FormRow(
                "İzinli olarak istediği süre",
                $"{t.Baslangic:HH:mm} ile {t.Bitis:HH:mm} saatleri arasında izinlidir."
            );

            var sigTable = sec.AddTable();
            sigTable.Borders.Width = 0.75;
            sigTable.Borders.Top.Width = 1.5;
            sigTable.AddColumn(Unit.FromCentimeter(5.93));
            sigTable.AddColumn(Unit.FromCentimeter(5.93));
            sigTable.AddColumn(Unit.FromCentimeter(5.94));

            var sigRow = sigTable.AddRow();
            sigRow.Height = Unit.FromCentimeter(3.5);
            sigRow.VerticalAlignment = VerticalAlignment.Top;

            var imzaSol = sigRow.Cells[0].AddParagraph("İZNİ İSTEYEN İMZASI");
            imzaSol.Format.Font.Bold = true;
            imzaSol.Format.Font.Size = 10;
            imzaSol.Format.Alignment = ParagraphAlignment.Left;
            var digSol = sigRow.Cells[0].AddParagraph(DijitalImzaText("Talep", m.AdSoyad, t.TalepImzaTarihi, t.TalepId));
            digSol.Format.Font.Size = 8;

            var imzaOrta = sigRow.Cells[1].AddParagraph("UYGUNDUR");
            imzaOrta.Format.Font.Bold = true;
            imzaOrta.Format.Font.Size = 10;
            imzaOrta.Format.Alignment = ParagraphAlignment.Center;
            var digOrta = sigRow.Cells[1].AddParagraph(
                DijitalImzaText("Onay", m.UstYetkiliAdSoyad, t.UstYetkiliOnayTarihi ?? t.IkOnayTarihi, t.TalepId, t.UstYetkiliOnayDurumu)
            );
            digOrta.Format.Font.Size = 8;
            digOrta.Format.Alignment = ParagraphAlignment.Center;

            var imzaSag = sigRow.Cells[2].AddParagraph("İZNİ KULLANANIN İMZASI");
            imzaSag.Format.Font.Bold = true;
            imzaSag.Format.Font.Size = 10;
            imzaSag.Format.Alignment = ParagraphAlignment.Right;
            var digSag = sigRow.Cells[2].AddParagraph(DijitalImzaText("Dönüş", m.AdSoyad, t.KullanimImzaTarihi, t.TalepId));
            digSag.Format.Font.Size = 8;
            digSag.Format.Alignment = ParagraphAlignment.Right;

            return doc;
        }

        private static Document BuildKlasikGunlukPdf(IzinTalep t, PreviewModel m, string? logoPath = null)
        {
            var doc = BuildBaseDoc("Personel İzin Formu");
            var sec = doc.AddSection();
            sec.PageSetup.PageFormat = PageFormat.A4;
            sec.PageSetup.Orientation = Orientation.Portrait;
            sec.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            sec.PageSetup.RightMargin = Unit.FromCentimeter(1.5);
            sec.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            sec.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);

            var headerTable = sec.AddTable();
            headerTable.Borders.Width = 0.75;
            headerTable.AddColumn(Unit.FromCentimeter(2.8));
            headerTable.AddColumn(Unit.FromCentimeter(14.9));

            var hRow = headerTable.AddRow();
            hRow.VerticalAlignment = VerticalAlignment.Center;
            hRow.Height = Unit.FromCentimeter(2.4);

            var logoCell = hRow.Cells[0];
            logoCell.Format.Alignment = ParagraphAlignment.Center;
            logoCell.VerticalAlignment = VerticalAlignment.Center;
            if (!string.IsNullOrEmpty(logoPath) && System.IO.File.Exists(logoPath))
            {
                try
                {
                    var img = logoCell.AddParagraph().AddImage(logoPath);
                    img.Height = Unit.FromCentimeter(1.6);
                    img.LockAspectRatio = true;
                }
                catch { logoCell.AddParagraph("CEYNAK").Format.Font.Bold = true; }
            }
            else { logoCell.AddParagraph("CEYNAK").Format.Font.Bold = true; }

            var rightCell = hRow.Cells[1];
            var rightInner = rightCell.Elements.AddTable();
            rightInner.Borders.Width = 0;
            rightInner.AddColumn(Unit.FromCentimeter(14.9));

            var titleRow = rightInner.AddRow();
            titleRow.Borders.Bottom.Width = 0.75;
            var tPar = titleRow.Cells[0].AddParagraph("CEYNAK KALİTE YÖNETİM SİSTEMİ");
            tPar.Format.Font.Bold = true;
            tPar.Format.Font.Size = 13;
            tPar.Format.Alignment = ParagraphAlignment.Center;
            var isoPar = titleRow.Cells[0].AddParagraph("TS-EN-ISO 9001:2015");
            isoPar.Format.Font.Bold = true;
            isoPar.Format.Font.Size = 10;
            isoPar.Format.Alignment = ParagraphAlignment.Center;

            var metaRow = rightInner.AddRow();
            var metaInner = metaRow.Cells[0].Elements.AddTable();
            metaInner.Borders.Width = 0;
            var metaW = 14.9 / 5;
            for (int i = 0; i < 5; i++) metaInner.AddColumn(Unit.FromCentimeter(metaW));
            var mr = metaInner.AddRow();
            mr.Borders.Width = 0;
            void MetaCell(int col, string label, string val)
            {
                if (col > 0) mr.Cells[col].Borders.Left.Width = 0.75;
                var lp = mr.Cells[col].AddParagraph(label);
                lp.Format.Font.Bold = true;
                lp.Format.Font.Size = 8;
                var vp = mr.Cells[col].AddParagraph(val);
                vp.Format.Font.Size = 8;
            }
            MetaCell(0, "KYS-04", "");
            MetaCell(1, "Form No", "İKY-FR-002");
            MetaCell(2, "İlk Yayın Tarihi", "01.05.2002");
            MetaCell(3, "Revizyon No/Tarihi", "03/17.12.2018");
            MetaCell(4, "Sayfa", "1 / 1");

            var formTitlePar = sec.AddParagraph("PERSONEL  İZİN  FORMU");
            formTitlePar.Format.Font.Bold = true;
            formTitlePar.Format.Font.Size = 16;
            formTitlePar.Format.Alignment = ParagraphAlignment.Center;
            formTitlePar.Format.SpaceBefore = Unit.FromPoint(6);
            formTitlePar.Format.SpaceAfter = Unit.FromPoint(4);

            var s = (m.IzinTipAdi ?? "").ToLowerInvariant();
            bool isYillik = s.Contains("yıllık") || s.Contains("yillik");
            bool isMazeret = s.Contains("mazeret");
            bool isGorev = s.Contains("görev") || s.Contains("gorev");
            bool isDenklestirme = s.Contains("denkle");
            bool isEvlilik = s.Contains("evlilik");
            bool isOlum = s.Contains("ölüm") || s.Contains("olum");
            bool isDogum = s.Contains("doğum") || s.Contains("dogum");
            bool isUcretsiz = s.Contains("ücretsiz") || s.Contains("ucretsiz");
            string Mark(bool on, string text) => on ? $"[X] {text}" : $"[ ] {text}";

            var izinTable = sec.AddTable();
            izinTable.Borders.Width = 0.75;
            izinTable.AddColumn(Unit.FromCentimeter(8.85));
            izinTable.AddColumn(Unit.FromCentimeter(8.85));

            void IzinRow(string sol, string sag)
            {
                var r = izinTable.AddRow();
                r.Cells[0].AddParagraph(sol).Format.Font.Size = 10;
                r.Cells[1].AddParagraph(sag).Format.Font.Size = 10;
            }
            IzinRow(Mark(isYillik, "1) YILLIK İZİN"), Mark(isEvlilik, "5) EVLİLİK İZNİ"));
            IzinRow(Mark(isMazeret, "2) MAZERET İZNİ"), Mark(isOlum, "6) ÖLÜM İZNİ"));
            IzinRow(Mark(isGorev, "3) GÖREV İZNİ"), Mark(isDogum, "7) DOĞUM İZNİ"));
            IzinRow(Mark(isDenklestirme, "4) DENKLEŞTİRME İZNİ"), Mark(isUcretsiz, "8) ÜCRETSİZ İZİN"));

            var days = (t.Bitis.Date - t.Baslangic.Date).TotalDays + 1;
            var sure = days >= 1 ? $"{days:0} gün" : $"{(t.Bitis - t.Baslangic).TotalHours:0.##} saat";

            var infoTable = sec.AddTable();
            infoTable.Borders.Width = 0.75;
            infoTable.AddColumn(Unit.FromCentimeter(3.3));
            infoTable.AddColumn(Unit.FromCentimeter(4.5));
            infoTable.AddColumn(Unit.FromCentimeter(4.6));
            infoTable.AddColumn(Unit.FromCentimeter(5.3));

            void InfoRow(string l1, string v1, string l2, string v2)
            {
                var r = infoTable.AddRow();
                var lp1 = r.Cells[0].AddParagraph(l1); lp1.Format.Font.Bold = true; lp1.Format.Font.Size = 9;
                r.Cells[1].AddParagraph(v1).Format.Font.Size = 10;
                var lp2 = r.Cells[2].AddParagraph(l2); lp2.Format.Font.Bold = true; lp2.Format.Font.Size = 9;
                r.Cells[3].AddParagraph(v2).Format.Font.Size = 10;
            }

            InfoRow("ADI-SOYADI", m.AdSoyad, "TALEP ETTİĞİ İZİN SÜRESİ", sure);
            InfoRow("GÖREVİ", m.Gorev, "İZİN BAŞLANGIÇ TARİHİ", t.Baslangic.ToString("dd/MM/yyyy"));
            InfoRow("T.C KİMLİK NO", m.TcKimlikNo, "İZİN BİTİŞ TARİHİ", t.Bitis.ToString("dd/MM/yyyy"));

            var adresTable = sec.AddTable();
            adresTable.Borders.Width = 0.75;
            adresTable.AddColumn(Unit.FromCentimeter(17.7));

            var adresRow = adresTable.AddRow();
            adresRow.Height = Unit.FromCentimeter(0.9);
            var adresPar = adresRow.Cells[0].AddParagraph("İZİN SIRASINDA BULUNACAĞI ADRES :  ");
            adresPar.Format.Font.Bold = true;
            adresPar.Format.Font.Size = 9;
            adresPar.AddText(t.IzinAdres ?? "");

            var telRow = adresTable.AddRow();
            telRow.Height = Unit.FromCentimeter(0.9);
            var telPar = telRow.Cells[0].AddParagraph("TELEFON NO :  ");
            telPar.Format.Font.Bold = true;
            telPar.Format.Font.Size = 9;
            telPar.AddText(t.TelefonNo ?? "");
            telPar.AddText("          CEP TELEFONU :  ");
            telPar.AddText(m.CepTel ?? "");

            var sigTable = sec.AddTable();
            sigTable.Borders.Width = 0.75;
            sigTable.AddColumn(Unit.FromCentimeter(8.85));
            sigTable.AddColumn(Unit.FromCentimeter(8.85));

            var sigRow = sigTable.AddRow();
            sigRow.Height = Unit.FromCentimeter(2.5);
            sigRow.VerticalAlignment = VerticalAlignment.Top;

            var sigSol = sigRow.Cells[0].AddParagraph("İZİN İSTEYENİN İMZASI");
            sigSol.Format.Font.Bold = true;
            sigSol.Format.Font.Size = 10;
            sigSol.Format.Alignment = ParagraphAlignment.Center;
            var digSol = sigRow.Cells[0].AddParagraph(DijitalImzaText("Talep", m.AdSoyad, t.TalepImzaTarihi, t.TalepId));
            digSol.Format.Font.Size = 8;
            digSol.Format.Alignment = ParagraphAlignment.Center;

            var sigSag = sigRow.Cells[1].AddParagraph("UYGUNDUR");
            sigSag.Format.Font.Bold = true;
            sigSag.Format.Font.Size = 10;
            sigSag.Format.Alignment = ParagraphAlignment.Center;
            var digSag = sigRow.Cells[1].AddParagraph(
                DijitalImzaText("Onay", m.UstYetkiliAdSoyad, t.UstYetkiliOnayTarihi ?? t.IkOnayTarihi, t.TalepId, t.UstYetkiliOnayDurumu)
            );
            digSag.Format.Font.Size = 8;
            digSag.Format.Alignment = ParagraphAlignment.Center;

            var kulPar = sec.AddParagraph("YUKARIDA TALEP FORMUNDA TALEP ETTİĞİM İZİN SÜRESİNİ EKSİKSİZ OLARAK KULLANDIM.");
            kulPar.Format.Font.Bold = true;
            kulPar.Format.Font.Size = 10;
            kulPar.Format.SpaceBefore = Unit.FromPoint(6);

            var kulRight = sec.AddParagraph("İZİN KULLANANIN  ADI-SOYADI / İMZASI");
            kulRight.Format.Font.Bold = true;
            kulRight.Format.Font.Size = 10;
            kulRight.Format.Alignment = ParagraphAlignment.Right;
            kulRight.Format.SpaceBefore = Unit.FromPoint(8);

            var kulDig = sec.AddParagraph(DijitalImzaText("Dönüş", m.AdSoyad, t.KullanimImzaTarihi, t.TalepId));
            kulDig.Format.Font.Size = 8;
            kulDig.Format.Alignment = ParagraphAlignment.Right;

            return doc;
        }

        private static string DijitalImzaText(string label, string? adSoyad, DateTime? tarih, int? refNo, IzinOnayDurumu? durum = null)
        {
            if (durum == IzinOnayDurumu.Reddedildi)
                return $"{label}: Reddedildi ({adSoyad} — {tarih:dd.MM.yyyy HH:mm})";

            if (!tarih.HasValue)
            {
                if (refNo == 0) return "";
                return $"{label}: Bekliyor";
            }

            var who = string.IsNullOrWhiteSpace(adSoyad) ? "-" : adSoyad;
            var rn = refNo.HasValue ? $" (No:{refNo})" : "";
            return $"{label}: {who} — {tarih:dd.MM.yyyy HH:mm}{rn}";
        }
    }
}
