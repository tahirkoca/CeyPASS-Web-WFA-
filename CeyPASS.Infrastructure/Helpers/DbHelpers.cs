using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Versioning;

namespace CeyPASS.Infrastructure.Helpers
{
    [SupportedOSPlatform("windows")]
    public class DbHelpers
    {
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

        [SupportedOSPlatform("windows")]
        public static Image? BytesToImage(byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            using (var ms = new MemoryStream(bytes))
                return Image.FromStream(ms);
        }
    }
}
