using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    public interface ICanliIzlemeService
    {
        DataTable GetFirmalar();
        AuthUserDTO Login(int firmaId, string user, string pass);
        List<LastPassDTO> GetLastPasses(int firmaId, int take);
        List<LastPassDTO> GetLastPassesYemekhane(int firmaId, int take);
    }
}
