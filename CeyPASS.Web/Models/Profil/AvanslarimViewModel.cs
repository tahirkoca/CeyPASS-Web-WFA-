using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Web.Models.Profil
{
    public class AvanslarimViewModel
    {
        public List<AvansTalep> AktifTalepler { get; set; } = new List<AvansTalep>();
        public int AktifPage { get; set; } = 1;
        public int AktifTotalCount { get; set; }
        public int AktifTotalPages { get; set; } = 1;

        public List<AvansTalep> GecmisTalepler { get; set; } = new List<AvansTalep>();
        public int GecmisPage { get; set; } = 1;
        public int GecmisTotalCount { get; set; }
        public int GecmisTotalPages { get; set; } = 1;

        public int PageSize { get; set; } = 5;
    }
}
