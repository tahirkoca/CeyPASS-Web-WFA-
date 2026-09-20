using CeyPASS.Entities.Concrete;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Sistem log kayıtları.</summary>
    public interface ISistemLogRepository
    {
         /// <summary>Yeni kayıt ekler.</summary>
         void Insert(SistemLog log);  
    }
}
