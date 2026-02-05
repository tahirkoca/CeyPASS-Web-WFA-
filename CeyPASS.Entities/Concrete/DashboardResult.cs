using System.Collections.Generic;

namespace CeyPASS.Entities.Concrete
{
    public class DashboardResult
    {
        public List<GecKalanlarDashboard> LateList { get; set; } = new List<GecKalanlarDashboard>();
        public List<DogumGunleriDashboard> Birthdays { get; set; } = new List<DogumGunleriDashboard>();
        public List<IseBaslayanlarDashboard> NewHires { get; set; } = new List<IseBaslayanlarDashboard>();
        public List<IstenAyrilanlarDashboard> Resignations { get; set; } = new List<IstenAyrilanlarDashboard>();
        public AnaEkranKartlariDashboard Cards { get; set; } = new AnaEkranKartlariDashboard();
    }
}
