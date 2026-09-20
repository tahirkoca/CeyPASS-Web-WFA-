namespace CeyPASS.Business.Abstractions
{
    /// <summary>Sayfa bazlı yetki kontrolleri (görüntüle, oluştur, onay vb.).</summary>
    public interface IAuthorizationService
    {
        /// <summary>Belirtilen sayfa ve yetki tipi için genel kontrol.</summary>
        bool Can(string sayfaAdi, string yetkiTipi);

        /// <summary>Görüntüleme yetkisi.</summary>
        bool ViewAbility(string page);

        /// <summary>Oluşturma yetkisi.</summary>
        bool CreateAbility(string page);

        /// <summary>Güncelleme yetkisi.</summary>
        bool UpdateAbility(string page);

        /// <summary>Silme yetkisi.</summary>
        bool DeleteAbility(string page);

        /// <summary>Dışa aktarma yetkisi.</summary>
        bool ExportAbility(string page);

        /// <summary>Onay yetkisi.</summary>
        bool ApproveAbility(string page);
    }
}
