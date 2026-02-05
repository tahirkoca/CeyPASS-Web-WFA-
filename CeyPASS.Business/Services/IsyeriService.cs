using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Services
{
    public class IsyeriService:IIsyeriService
    {
        private readonly IIsyeriRepository _repo;

        public IsyeriService(IIsyeriRepository repo)
        {
            _repo = repo;
        }
        public DataTable GetAll() => _repo.GetAll();
        public List<IsyeriItem> GetListForAdmin()
        {
            var dt = _repo.GetAll();
            var list = new List<IsyeriItem>();
            if (dt == null) return list;
            foreach (DataRow r in dt.Rows)
            {
                var firmaId = r.Field<int>("FirmaId");
                var isyeriId = r.Field<int>("IsyeriId");
                var ad = r.Field<string>("IsyeriAdi") ?? "";
                list.Add(new IsyeriItem(firmaId, isyeriId, ad));
            }
            return list;
        }
        public List<IsyeriItem> GetIsyerleriByFirma(int firmaId)=>_repo.GetIsyerleriByFirma(firmaId);            
        public bool AddManual(int firmaId, int isyeriId, string ad)=> _repo.InsertManual(firmaId, isyeriId, ad);
        public bool Update(int firmaId, int isyeriId, string ad)=> _repo.Update(firmaId, isyeriId, ad);
        public bool Delete(int firmaId, int isyeriId)=> _repo.Delete(firmaId, isyeriId);
    }
}
