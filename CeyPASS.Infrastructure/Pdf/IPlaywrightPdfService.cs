using System.Threading;
using System.Threading.Tasks;

namespace CeyPASS.Infrastructure.Pdf;

public interface IPlaywrightPdfService
{
    Task<byte[]> HtmlToPdfAsync(string html, CancellationToken cancellationToken = default);
}
