using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class AvansService : IAvansService
    {
        private readonly IAvansRepository _repo;
        private readonly IBildirimService _bildirimService;

        public AvansService(IAvansRepository repo, IBildirimService bildirimService)
        {
            _repo = repo;
            _bildirimService = bildirimService;
        }

        public int TalepOlustur(string personelId, decimal miktar, string? aciklama)
        {
            if (string.IsNullOrWhiteSpace(personelId))
                throw new ArgumentException("PersonelId zorunludur.", nameof(personelId));
            if (miktar <= 0)
                throw new ArgumentException("Avans miktarı 0'dan büyük olmalıdır.", nameof(miktar));

            var talep = new AvansTalep
            {
                PersonelId = personelId.Trim(),
                Miktar = miktar,
                Aciklama = (aciklama ?? "").Trim(),
                TalepTarihi = DateTime.Now,
                Durum = AvansDurumu.Bekliyor
            };

            var id = _repo.Ekle(talep);
            if (id > 0)
            {
                // Avans talebi genelde muhasebe veya İK'ya (adminlere) gider. 
                // Şimdilik Admin rolündeki kullanıcılara gitmesi için KullaniciId=-1 gibi bir sanal hedef 
                // veya tüm adminlere düşecek bir mantık kurulabilir. 
                // Şuanlık basitleştirmek adına sadece sisteme ekliyoruz.
                _bildirimService.AddNotification(null, null, "Yeni Avans Talebi", 
                    $"{personelId} numaralı personel {miktar} TL avans talebinde bulundu.", "AvansTalep", id);
            }
            return id;
        }

        public List<AvansTalep> PersonelTalepleri(string personelId) => _repo.GetByPersonel(personelId);

        public List<AvansTalep> TumTalepler() => _repo.GetAll();

        public bool Onayla(int avansId, int onaylayanKullaniciId, string? aciklama)
        {
            var t = _repo.GetById(avansId);
            var ok = _repo.GuncelleOnay(avansId, AvansDurumu.Onaylandi, onaylayanKullaniciId, aciklama);
            if (ok && t != null)
            {
                _bildirimService.AddNotification(null, t.PersonelId, "Avans Talebi Onaylandı", 
                    $"{t.Miktar} TL tutarındaki avans talebiniz onaylandı.", "AvansOnay", avansId);
            }
            return ok;
        }

        public bool Reddet(int avansId, int onaylayanKullaniciId, string? aciklama)
        {
            var t = _repo.GetById(avansId);
            var ok = _repo.GuncelleOnay(avansId, AvansDurumu.Reddedildi, onaylayanKullaniciId, aciklama);
            if (ok && t != null)
            {
                _bildirimService.AddNotification(null, t.PersonelId, "Avans Talebi Reddedildi", 
                    $"{t.Miktar} TL tutarındaki avans talebiniz reddedildi.", "AvansRed", avansId);
            }
            return ok;
        }

        public bool IptalEt(int avansId) => _repo.Sil(avansId);

        public bool Guncelle(int avansId, decimal miktar, string? aciklama) => _repo.Guncelle(avansId, miktar, aciklama);
    }
}

