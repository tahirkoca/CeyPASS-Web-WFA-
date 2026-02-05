using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.DataAccess.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace CeyPASS.Business.Services
{
    public class FirmaService:IFirmaService
    {
        private readonly IFirmaRepository _repo;

        public FirmaService(IFirmaRepository repo)
        {
            _repo = repo;
        }
        public List<Firma> GetAll() => _repo.GetAll();
        public List<LookupItem> GetLookup()=> GetAll().Select(x => new LookupItem { Id = x.FirmaId, Ad = x.FirmaAdi }).ToList();
        public int SuggestNextId()
        {
            var max = _repo.GetMaxId();
            return (max ?? 100) + 1;
        }
        public bool Add(int id, string ad, string itMail, out string msg)
        {
            msg = string.Empty;
            if (id <= 0) { msg = "Geçerli bir Firma Id giriniz."; return false; }
            if (string.IsNullOrWhiteSpace(ad)) { msg = "Firma adı boş olamaz."; return false; }

            if (!string.IsNullOrWhiteSpace(itMail))
            {
                if (!IsValidEmail(itMail)) { msg = "IT birim maili geçersiz."; return false; }
            }

            if (GetAll().Any(x => x.FirmaId == id))
            {
                msg = "Bu ID zaten kullanılıyor.";
                return false;
            }

            return _repo.Insert(new Firma { FirmaId = id, FirmaAdi = ad.Trim(), ITBirimMail = itMail?.Trim() ?? string.Empty });
        }
        public bool Update(int id, string ad, string itMail, out string msg)
        {
            msg = string.Empty;
            if (id <= 0) { msg = "Geçersiz ID."; return false; }
            if (string.IsNullOrWhiteSpace(ad)) { msg = "Firma adı boş olamaz."; return false; }
            if (!string.IsNullOrWhiteSpace(itMail) && !IsValidEmail(itMail)) { msg = "IT birim maili geçersiz."; return false; }

            return _repo.Update(new Firma { FirmaId = id, FirmaAdi = ad.Trim(), ITBirimMail = itMail?.Trim() ?? string.Empty });
        }
        public bool Delete(int id) => _repo.Delete(id);
        private bool IsValidEmail(string mail)
        {
            try { var _ = new MailAddress(mail); return true; }
            catch { return false; }
        }
        public List<Firma> GetPuantajFirmalar()
        {
            return _repo.GetPuantajFirmalari();
        }
    }
}
