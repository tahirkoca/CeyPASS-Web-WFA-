using CeyPASS.Entities.Concrete;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface ISistemLogRepository
    {
         void Insert(SistemLog log);  
    }
}
