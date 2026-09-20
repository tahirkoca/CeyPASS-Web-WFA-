using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Versioning;

namespace CeyPASS.Infrastructure.Helpers
{
    /// <summary>Windows GDI+ görüntü ↔ bayt dönüşümleri (veritabanı alanları).</summary>
    [SupportedOSPlatform("windows")]
    public class DbHelpers
    {
        /// <summary><see cref="Image"/> örneğini ham bayt dizisine yazar; format hatasında PNG.</summary>
        [SupportedOSPlatform("windows")]
        public static byte[]? ImageToBytes(Image? img)
        {
            if (img == null) return null;
            using (var ms = new MemoryStream())
            {
                var format = img.RawFormat;
                try { img.Save(ms, format); }
                catch { img.Save(ms, ImageFormat.Png); }
                return ms.ToArray();
            }
        }

        /// <summary>Bayt dizisinden bellekte <see cref="Image"/> oluşturur.</summary>
        [SupportedOSPlatform("windows")]
        public static Image? BytesToImage(byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            using (var ms = new MemoryStream(bytes))
                return Image.FromStream(ms);
        }
    }
}
