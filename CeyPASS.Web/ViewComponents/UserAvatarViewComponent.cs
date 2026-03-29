using CeyPASS.Business.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CeyPASS.Web.ViewComponents
{
    public class UserAvatarViewComponent : ViewComponent
    {
        private readonly ISessionContext _session;
        private readonly IKisiQueryService _kisiQueryService;

        public UserAvatarViewComponent(ISessionContext session, IKisiQueryService kisiQueryService)
        {
            _session = session;
            _kisiQueryService = kisiQueryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Try session cache first
            if (!string.IsNullOrEmpty(_session.UserInitials))
            {
                ViewBag.PhotoUrl = _session.UserPhotoUrl;
                ViewBag.Initials = _session.UserInitials;
                return View();
            }

            string? photoUrl = null;
            string initials = "U"; // Default

            if (!string.IsNullOrWhiteSpace(_session.AktifSicilNo))
            {
                try
                {
                    var kisi = _kisiQueryService.GetKisiDetay(_session.AktifSicilNo);
                    if (kisi != null)
                    {
                        if (kisi.Fotograf != null && kisi.Fotograf.Length > 0)
                        {
                            photoUrl = ToImageDataUrl(kisi.Fotograf);
                        }
                        
                        if (!string.IsNullOrWhiteSpace(kisi.Ad))
                        {
                            initials = kisi.Ad.Substring(0, 1).ToUpper();
                        }

                        // Store in session
                        _session.UserPhotoUrl = photoUrl;
                        _session.UserInitials = initials;
                    }
                }
                catch
                {
                    // Ignore errors, use default
                }
            }
            else if (!string.IsNullOrWhiteSpace(_session.AdSoyad))
            {
                initials = _session.AdSoyad.Substring(0, 1).ToUpper();
                _session.UserInitials = initials;
            }

            ViewBag.PhotoUrl = photoUrl;
            ViewBag.Initials = initials;

            return View();
        }

        private static string? ToImageDataUrl(byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            string mime = "image/jpeg";
            if (bytes.Length >= 8 &&
                bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
            {
                mime = "image/png";
            }
            return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }
    }
}
