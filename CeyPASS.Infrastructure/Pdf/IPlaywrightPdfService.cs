using System.Threading;
using System.Threading.Tasks;

namespace CeyPASS.Infrastructure.Pdf;

/// <summary>HTML içeriğini PDF bayt dizisine dönüştürme.</summary>
public interface IPlaywrightPdfService
{
    /// <summary>Verilen HTML’i A4 PDF olarak üretir.</summary>
    Task<byte[]> HtmlToPdfAsync(string html, CancellationToken cancellationToken = default);
}
