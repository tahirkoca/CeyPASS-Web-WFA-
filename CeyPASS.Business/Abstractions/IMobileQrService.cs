using CeyPASS.Models;
using System.Threading.Tasks;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Mobil QR okuma ve geçiş isteği işleme.</summary>
    public interface IMobileQrService
    {
        /// <summary>QR taramasını doğrular ve geçiş/kayıt sonucunu döner.</summary>
        ApiResult<string> ProcessQrScan(QrIstekModel request, string personelId);
    }
}
