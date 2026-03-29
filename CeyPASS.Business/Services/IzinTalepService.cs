using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class IzinTalepService : IIzinTalepService
    {
        private readonly IIzinTalepRepository _repo;
        private readonly IUstYetkiliRepository _ustRepo;
        private readonly IKisiIzinlerRepository _kisiIzinRepo;
        private readonly IBildirimService _bildirimService;

        public IzinTalepService(
            IIzinTalepRepository repo,
            IUstYetkiliRepository ustRepo,
            IKisiIzinlerRepository kisiIzinRepo,
            IBildirimService bildirimService)
        {
            _repo = repo;
            _ustRepo = ustRepo;
            _kisiIzinRepo = kisiIzinRepo;
            _bildirimService = bildirimService;
        }

        public int TalepOlustur(IzinTalep talep, int talepEdenKullaniciId)
        {
            if (talep == null) throw new ArgumentNullException(nameof(talep));
            if (string.IsNullOrWhiteSpace(talep.PersonelId))
                throw new ArgumentException("PersonelId zorunludur.", nameof(talep));
            if (!talep.IzinTipId.HasValue || talep.IzinTipId.Value <= 0)
                throw new ArgumentException("IzinTipId zorunludur.", nameof(talep));

            talep.PersonelId = talep.PersonelId.Trim();
            talep.TalepTarihi = DateTime.Now;
            talep.TalepImzaKullaniciId = talepEdenKullaniciId <= 0 ? null : talepEdenKullaniciId;
            talep.TalepImzaTarihi = DateTime.Now;

            var ust = _ustRepo.GetUstYetkili(talep.PersonelId);
            if (!string.IsNullOrWhiteSpace(ust))
            {
                talep.UstYetkiliPersonelId = ust;
                talep.UstYetkiliOnayDurumu = IzinOnayDurumu.Bekliyor;
                talep.IkOnayDurumu = null;
            }
            else
            {
                talep.UstYetkiliPersonelId = null;
                talep.UstYetkiliOnayDurumu = null;
                talep.IkOnayDurumu = IzinOnayDurumu.Bekliyor;
            }

            var id = _repo.Ekle(talep);
            if (id > 0)
            {
                if (!string.IsNullOrEmpty(talep.UstYetkiliPersonelId))
                {
                    _bildirimService.AddNotification(null, talep.UstYetkiliPersonelId, "Yeni İzin Talebi", 
                        $"{talep.PersonelId} numaralı personel izin talebinde bulundu.", "IzinTalep", id);
                }
                else
                {
                    // IK için genel bildirim (şimdilik boş bıraktım veya belirli bir rol/grup hedeflenebilir)
                    // _bildirimService.AddNotification(null, null, "Yeni İzin Talebi (İK)", ...);
                }
            }
            return id;
        }

        public List<IzinTalep> PersonelTalepleri(string personelId) => _repo.GetByPersonel(personelId);

        public List<IzinTalep> UstYetkiliBekleyenler(string ustYetkiliPersonelId) => _repo.GetUstYetkiliBekleyenler(ustYetkiliPersonelId);

        public bool UstYetkiliOnayla(int talepId, string ustYetkiliPersonelId, string? aciklama)
        {
            var t = _repo.GetById(talepId);
            if (t == null) return false;
            if (!string.Equals(t.UstYetkiliPersonelId, ustYetkiliPersonelId, StringComparison.OrdinalIgnoreCase))
                return false;

            var ok = _repo.UstYetkiliGuncelle(talepId, IzinOnayDurumu.Onaylandi, aciklama);
            if (ok)
            {
                _bildirimService.AddNotification(null, t.PersonelId, "İzin Talebi Onaylandı", 
                    "İzin talebiniz üst yetkili tarafından onaylandı.", "IzinOnay", talepId);
            }
            return ok;
        }

        public bool UstYetkiliReddet(int talepId, string ustYetkiliPersonelId, string? aciklama)
        {
            var t = _repo.GetById(talepId);
            if (t == null) return false;
            if (!string.Equals(t.UstYetkiliPersonelId, ustYetkiliPersonelId, StringComparison.OrdinalIgnoreCase))
                return false;

            var ok = _repo.UstYetkiliGuncelle(talepId, IzinOnayDurumu.Reddedildi, aciklama);
            if (ok)
            {
                _bildirimService.AddNotification(null, t.PersonelId, "İzin Talebi Reddedildi", 
                    "İzin talebiniz üst yetkili tarafından reddedildi.", "IzinRed", talepId);
            }
            return ok;
        }

        public List<IzinTalep> IkBekleyenler() => _repo.GetIkBekleyenler();

        public bool IkOnayla(int talepId, int ikKullaniciId, string? aciklama)
        {
            var t = _repo.GetById(talepId);
            if (t == null) return false;

            if (!_repo.IkGuncelle(talepId, IzinOnayDurumu.Onaylandi, ikKullaniciId, aciklama))
                return false;

            var izin = new KisiIzin
            {
                FirmaId = t.FirmaId,
                PersonelId = t.PersonelId,
                IzinId = t.IzinTipId ?? 0,
                Baslangic = t.Baslangic,
                Bitis = t.Bitis,
                Aciklama = t.Aciklama ?? "",
                SaatlikIzinMi = t.SaatlikIzinMi,
                OlusturanKullaniciId = ikKullaniciId
            };

            if (!_kisiIzinRepo.Insert(izin) || !izin.KisiIzinId.HasValue)
                return false;

            _repo.SetSonucKisiIzinId(talepId, izin.KisiIzinId.Value);
            
            _bildirimService.AddNotification(null, t.PersonelId, "İzin İşlemi Tamamlandı", 
                "İzin talebiniz İK tarafından onaylandı ve sisteme işlendi.", "IzinOnay", talepId);

            return true;
        }

        public bool IkReddet(int talepId, int ikKullaniciId, string? aciklama)
        {
            var t = _repo.GetById(talepId);
            var ok = _repo.IkGuncelle(talepId, IzinOnayDurumu.Reddedildi, ikKullaniciId, aciklama);
            if (ok && t != null)
            {
                _bildirimService.AddNotification(null, t.PersonelId, "İzin Talebi Reddedildi", 
                    "İzin talebiniz İK tarafından reddedildi.", "IzinRed", talepId);
            }
            return ok;
        }

        public bool DonusImzasinaAc(int talepId, int ikKullaniciId) => _repo.DonusImzasinaAc(talepId, ikKullaniciId);

        public bool KullanimImzaAt(int talepId, int personelKullaniciId) => _repo.KullanimImzaAt(talepId, personelKullaniciId);

        public bool IsSupervisor(string personelId)
        {
            if (string.IsNullOrWhiteSpace(personelId)) return false;
            return _ustRepo.AnySubordinates(personelId);
        }
    }
}

