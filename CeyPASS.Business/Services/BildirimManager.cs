using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CeyPASS.Business.Services
{
    /// <summary>Bildirim servisi koordinasyonu.</summary>
    public class BildirimManager : IBildirimService
    {
        private readonly IBildirimRepository _bildirimRepository;
        private readonly IPushNotificationService _pushNotificationService;

        public BildirimManager(IBildirimRepository bildirimRepository, IPushNotificationService pushNotificationService)
        {
            _bildirimRepository = bildirimRepository;
            _pushNotificationService = pushNotificationService;
        }

        /// <inheritdoc />
        public void AddNotification(int? kullaniciId, string? personelId, string baslik, string mesaj, string tipi, int? ilgiliKayitId = null)
        {
            if (string.IsNullOrWhiteSpace(personelId) && !kullaniciId.HasValue)
                return; // Geçersiz hedef

            var b = new Bildirim
            {
                KullaniciId = kullaniciId,
                PersonelId = personelId,
                Baslik = baslik,
                Mesaj = mesaj,
                OkunduMu = false,
                OlusturmaTarihi = DateTime.Now,
                Tipi = tipi,
                IlgiliKayitId = ilgiliKayitId
            };
            
            _bildirimRepository.Ekle(b);

            // Mobil cihazlara push gönder (Arka planda çalışır, ana akışı bozmaz)
            Task.Run(() => _pushNotificationService.SendPushToUserAsync(personelId, kullaniciId?.ToString(), baslik, mesaj));
        }

        /// <inheritdoc />
        public List<Bildirim> GetMyNotifications(string? personelId, int? kullaniciId)
        {
            return _bildirimRepository.GetForUser(personelId, kullaniciId);
        }

        /// <inheritdoc />
        public void MarkAsRead(int bildirimId)
        {
            _bildirimRepository.MarkAsRead(bildirimId);
        }

        /// <inheritdoc />
        public int GetUnreadCount(string? personelId, int? kullaniciId)
        {
            return _bildirimRepository.GetUnreadCount(personelId, kullaniciId);
        }

        /// <inheritdoc />
        public void MarkAllAsRead(string? personelId, int? kullaniciId)
        {
            _bildirimRepository.MarkAllAsRead(personelId, kullaniciId);
        }
    }
}
