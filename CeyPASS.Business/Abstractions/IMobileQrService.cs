using CeyPASS.Models;
using System.Threading.Tasks;

namespace CeyPASS.Business.Abstractions
{
    public interface IMobileQrService
    {
        ApiResult<string> ProcessQrScan(QrIstekModel request, string personelId);
    }
}
