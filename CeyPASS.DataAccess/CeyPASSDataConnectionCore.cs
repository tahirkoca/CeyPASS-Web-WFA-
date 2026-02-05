using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CeyPASS.DataAccess
{
    /// <summary>
    /// EF Core DbContext - Entity Framework 6'dan geçiş için oluşturuldu
    /// </summary>
    public class CeyPASSDataConnectionCore : DbContext
    {
        public CeyPASSDataConnectionCore(DbContextOptions<CeyPASSDataConnectionCore> options) : base(options)
        {
        }

        // DbSet'ler
        public DbSet<CalismaSekilleri> CalismaSekilleri { get; set; }
        public DbSet<CanliIzlemeHesaplari> CanliIzlemeHesaplari { get; set; }
        public DbSet<Cihazlar> Cihazlar { get; set; }
        public DbSet<Departmanlar> Departmanlar { get; set; }
        public DbSet<FinalPuantajVerisi> FinalPuantajVerisi { get; set; }
        public DbSet<IzinTipleri> IzinTipleri { get; set; }
        public DbSet<KisiHareketler> KisiHareketler { get; set; }
        public DbSet<KisiIzinler> KisiIzinler { get; set; }
        public DbSet<Pozisyonlar> Pozisyonlar { get; set; }
        public DbSet<PuantajsizKartAtamalari> PuantajsizKartAtamalari { get; set; }
        public DbSet<RaporTanimlari> RaporTanimlari { get; set; }
        public DbSet<ResmiTatiller> ResmiTatiller { get; set; }
        public DbSet<SistemAyarlar> SistemAyarlar { get; set; }
        public DbSet<SistemMailAlicilari> SistemMailAlicilari { get; set; }
        public DbSet<YemekhaneEngellenenKullanicilar> YemekhaneEngellenenKullanicilar { get; set; }
        public DbSet<YemekhaneGecisHareketler> YemekhaneGecisHareketler { get; set; }
        public DbSet<CalismaStatusu> CalismaStatusu { get; set; }
        public DbSet<CokluSicilBaglantilari> CokluSicilBaglantilari { get; set; }
        public DbSet<KullaniciEkstraYetkilendirme> KullaniciEkstraYetkilendirme { get; set; }
        public DbSet<KullaniciSifreKurtarma> KullaniciSifreKurtarma { get; set; }
        public DbSet<PuantajOnay> PuantajOnay { get; set; }
        public DbSet<PuantajsizKartlar> PuantajsizKartlar { get; set; }
        public DbSet<PuantajTipleri> PuantajTipleri { get; set; }
        public DbSet<SayfaYetkileri> SayfaYetkileri { get; set; }
        public DbSet<SistemLoglari> SistemLoglari { get; set; }
        public DbSet<TaseronKartlari> TaseronKartlari { get; set; }
        public DbSet<YemekhaneGirisLimitler> YemekhaneGirisLimitler { get; set; }
        public DbSet<vw_YemekhaneEngellenenBekleyenler> vw_YemekhaneEngellenenBekleyenler { get; set; }
        public DbSet<Kisiler> Kisiler { get; set; }
        public DbSet<Kullanicilar> Kullanicilar { get; set; }
        public DbSet<Roller> Roller { get; set; }
        public DbSet<CihazTipler> CihazTipler { get; set; }
        public DbSet<Firmalar> Firmalar { get; set; }
        public DbSet<Isyerler> Isyerler { get; set; }
        public DbSet<Bolumler> Bolumler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Database First yaklaşımı - mevcut veritabanı şemasını kullan
            // Primary key'ler EDMX'ten alınan bilgilere göre tanımlanıyor

            // View'lar için key tanımlaması
            modelBuilder.Entity<vw_YemekhaneEngellenenBekleyenler>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_YemekhaneEngellenenBekleyenler");
            });

            // Primary Keys
            modelBuilder.Entity<Kisiler>().HasKey(e => e.PersonelId);
            modelBuilder.Entity<Kullanicilar>().HasKey(e => e.KullaniciId);
            modelBuilder.Entity<Departmanlar>().HasKey(e => e.DepartmanId);
            modelBuilder.Entity<Roller>().HasKey(e => e.RolId);
            modelBuilder.Entity<Firmalar>().HasKey(e => e.FirmaId);
            modelBuilder.Entity<Isyerler>().HasKey(e => e.Id);
            modelBuilder.Entity<Bolumler>().HasKey(e => e.Id);
            modelBuilder.Entity<CihazTipler>().HasKey(e => e.TipId);
            modelBuilder.Entity<CalismaSekilleri>().HasKey(e => e.CalismaSekilId);
            modelBuilder.Entity<CalismaStatusu>().HasKey(e => e.CalismaStatuId);
            modelBuilder.Entity<Pozisyonlar>().HasKey(e => e.PozisyonId);
            modelBuilder.Entity<IzinTipleri>().HasKey(e => e.IzinTipId);
            modelBuilder.Entity<Cihazlar>().HasKey(e => e.CihazId);
            modelBuilder.Entity<PuantajTipleri>().HasKey(e => e.Kod);
            modelBuilder.Entity<RaporTanimlari>().HasKey(e => e.Id);
            modelBuilder.Entity<ResmiTatiller>().HasKey(e => e.Tarih);
            modelBuilder.Entity<SistemAyarlar>().HasKey(e => e.AyarAdi);
            modelBuilder.Entity<SistemLoglari>().HasKey(e => e.Id);
            modelBuilder.Entity<KisiHareketler>().HasKey(e => e.Id);
            modelBuilder.Entity<KisiIzinler>().HasKey(e => e.KisiIzinId);
            modelBuilder.Entity<KisiIzinler>().ToTable(t => t.UseSqlOutputClause(false));
            modelBuilder.Entity<KisiIzinler>()
                .Property(e => e.SureDakika)
                .ValueGeneratedOnAddOrUpdate()
                .Metadata.SetBeforeSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
            modelBuilder.Entity<KisiIzinler>()
                .Property(e => e.SureDakika)
                .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
            modelBuilder.Entity<PuantajOnay>().HasKey(e => new { e.PersonelId, e.Tarih });
            modelBuilder.Entity<FinalPuantajVerisi>().HasKey(e => e.Id);
            modelBuilder.Entity<PuantajsizKartlar>().HasKey(e => e.KartNo);
            modelBuilder.Entity<PuantajsizKartAtamalari>().HasKey(e => e.AtamaId);
            modelBuilder.Entity<KullaniciEkstraYetkilendirme>().HasKey(e => new { e.KullaniciId, e.SayfaAdi, e.YetkiTipi, e.Allowed });
            modelBuilder.Entity<KullaniciSifreKurtarma>().HasKey(e => e.Id);
            modelBuilder.Entity<SayfaYetkileri>().HasKey(e => new { e.RolId, e.SayfaAdi, e.YetkiTipi });
            modelBuilder.Entity<CanliIzlemeHesaplari>().HasKey(e => new { e.KullaniciId, e.FirmaId });
            modelBuilder.Entity<YemekhaneEngellenenKullanicilar>().HasKey(e => e.Id);
            modelBuilder.Entity<YemekhaneGecisHareketler>().HasKey(e => e.Id);
            modelBuilder.Entity<YemekhaneGirisLimitler>().HasKey(e => e.Id);
            modelBuilder.Entity<TaseronKartlari>().HasKey(e => e.Id);
            modelBuilder.Entity<CokluSicilBaglantilari>().HasKey(e => new { e.TCKimlikNo, e.AnaPersonelId, e.HedefPersonelId });
            modelBuilder.Entity<SistemMailAlicilari>().HasKey(e => e.Id);
        }

        // Stored Procedure çağrıları için extension method'lar
        // EF Core'da stored procedure'ler FromSqlRaw ile çağrılır
    }
}
