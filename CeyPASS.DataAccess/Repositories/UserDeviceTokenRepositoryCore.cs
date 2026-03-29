using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class UserDeviceTokenRepositoryCore : IUserDeviceTokenRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public UserDeviceTokenRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public bool AddOrUpdate(UserDeviceToken token)
        {
            try
            {
                // Aynı token daha önce kaydedilmiş mi?
                var existing = _context.UserDeviceTokens.FirstOrDefault(t => t.FCMToken == token.FCMToken);
                
                if (existing != null)
                {
                    existing.KullaniciId = token.KullaniciId;
                    existing.PersonelId = token.PersonelId;
                    existing.DeviceType = token.DeviceType;
                    existing.LastUpdated = DateTime.Now;
                    existing.IsActive = true;
                    _context.Entry(existing).State = EntityState.Modified;
                }
                else
                {
                    token.LastUpdated = DateTime.Now;
                    token.IsActive = true;
                    _context.UserDeviceTokens.Add(token);
                }

                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Deactivate(string fcmToken)
        {
            var existing = _context.UserDeviceTokens.FirstOrDefault(t => t.FCMToken == fcmToken);
            if (existing != null)
            {
                existing.IsActive = false;
                existing.LastUpdated = DateTime.Now;
                _context.Entry(existing).State = EntityState.Modified;
                _context.SaveChanges();
            }
        }

        public List<string> GetTokensByUser(string? personelId, string? kullaniciId)
        {
            return _context.UserDeviceTokens
                .Where(t => t.IsActive && 
                            ((!string.IsNullOrEmpty(personelId) && t.PersonelId == personelId) || 
                             (!string.IsNullOrEmpty(kullaniciId) && t.KullaniciId == kullaniciId)))
                .Select(t => t.FCMToken)
                .ToList();
        }
    }
}
