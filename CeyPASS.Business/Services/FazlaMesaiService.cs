using CeyPASS.Business.Abstractions;
using System;

namespace CeyPASS.Business.Services
{
    public class FazlaMesaiService:IFazlaMesaiService
    {
        public int Yuvarla30(int dakika)
        {
            if (dakika <= 0) return 0;
            return (int)Math.Round(dakika / 30.0, MidpointRounding.AwayFromZero) * 30;
        }
        public int HesaplaSistemFm(int erkenDakika, int gecDakika)
        {
            var e = Yuvarla30(Math.Max(0, erkenDakika));
            var g = Yuvarla30(Math.Max(0, gecDakika));
            return e + g;
        }
    }
}
