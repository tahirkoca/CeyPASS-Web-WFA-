using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Windows.Forms;

namespace CeyPASS.WFA;

/// <summary>
/// WinForms sayfalarında Tag ile işaretlenmiş kontrollerin yetkilerini uygular.
/// AuthorizationHelper ile birlikte kullanılır; Infrastructure WinForms referansı almadığı için bu sınıf WFA'da.
/// </summary>
public static class WinFormsAuthHelper
{
    /// <summary>
    /// Sayfa adı ve container üzerinde Tag ile işaretlenmiş kontrollerin yetkilerini uygular.
    /// Tag değeri YetkiTipleri (View, Create, Update, Delete, Export, Approve) olan kontroller
    /// ilgili yetkiye göre Enabled ayarlanır.
    /// </summary>
    public static void ApplyPageAuthorization(IAuthorizationService auth, ISessionContext session, string pageName, Control container)
    {
        if (container == null) return;
        ApplyRecursive(auth, pageName, container);
    }

    private static void ApplyRecursive(IAuthorizationService auth, string pageName, Control control)
    {
        if (control.Tag is string tag && IsYetkiTipi(tag))
        {
            control.Enabled = auth.Can(pageName, tag);
        }
        foreach (Control child in control.Controls)
        {
            ApplyRecursive(auth, pageName, child);
        }
    }

    private static bool IsYetkiTipi(string tag)
    {
        return tag == YetkiTipleri.View || tag == YetkiTipleri.Create || tag == YetkiTipleri.Update
            || tag == YetkiTipleri.Delete || tag == YetkiTipleri.Export || tag == YetkiTipleri.Approve;
    }
}
