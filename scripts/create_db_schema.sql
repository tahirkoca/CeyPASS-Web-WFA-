USE [master]
GO

CREATE DATABASE [CeyPASS]
GO

USE [CeyPASS]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_IzinSureDakika]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   FUNCTION [dbo].[fn_IzinSureDakika]
(
    @Baslangic     datetime,
    @Bitis         datetime,
    @SaatlikIzinMi bit
)
RETURNS int
AS
BEGIN
    IF (@Baslangic IS NULL OR @Bitis IS NULL)
        RETURN NULL;

    -- SAATLİK İZİN
    IF (@SaatlikIzinMi = 1)
    BEGIN
        DECLARE @min int = DATEDIFF(MINUTE, @Baslangic, @Bitis);
        IF (@min < 0) SET @min = 0;
        RETURN @min;
    END

    -- GÜNLÜK İZİN
    DECLARE @S date = CONVERT(date, @Baslangic);
    DECLARE @E date = CONVERT(date, @Bitis);
    IF (@E < @S) RETURN 0;

    DECLARE @ToplamGun int = DATEDIFF(DAY, @S, @E) + 1;

    -- Pazar: DATEFIRST bağımsız
    DECLARE @IlkPazar date =
        DATEADD(DAY, (7 - (DATEDIFF(DAY, '19000107', @S) % 7)) % 7, @S);

    DECLARE @Pazar int =
        CASE WHEN @IlkPazar > @E THEN 0
             ELSE 1 + DATEDIFF(DAY, @IlkPazar, @E) / 7
        END;

    -- RT azalış: Pazar olan RT'ler hariç; yarım gün RT -> oranlı düş
    DECLARE @RTAzalis decimal(10,4) =
    (
        SELECT ISNULL(SUM(
            CASE
              -- Bu RT Pazar mı? (mod 7 = 0 -> Pazar) -> RT etkisi yok,
              -- çünkü Pazar zaten 1 gün olarak düşüldü
              WHEN (DATEDIFF(DAY,'19000107', rt.Tarih) % 7) = 0 THEN 0.0
              -- Tam gün ya da 7.50 üstü
              WHEN rt.CalismaSaati >= 7.5 THEN 1.0
              -- Yarım gün
              WHEN rt.CalismaSaati > 0 THEN rt.CalismaSaati / 7.5
              -- Saat girilmemişse tam gün varsay
              ELSE 1.0
            END
        ),0.0)
        FROM dbo.ResmiTatiller rt
        WHERE rt.Tarih BETWEEN @S AND @E
    );

    DECLARE @NetGun decimal(10,4) = @ToplamGun - @Pazar - @RTAzalis;
    IF (@NetGun < 0) SET @NetGun = 0;

    RETURN CONVERT(int, ROUND(@NetGun * 450 /*dk*/, 0));
END
GO
/****** Object:  Table [dbo].[Kisiler]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Kisiler](
	[PersonelId] [nvarchar](30) NOT NULL,
	[Ad] [nvarchar](100) NULL,
	[Soyad] [nvarchar](100) NULL,
	[KartNo] [nvarchar](30) NULL,
	[TcKimlikNo] [nvarchar](11) NULL,
	[PozisyonId] [int] NULL,
	[DogumTarihi] [date] NULL,
	[DepartmanId] [int] NULL,
	[IseGirisTarihi] [date] NULL,
	[IstenCikisTarihi] [date] NULL,
	[CalismaStatusu] [nvarchar](30) NULL,
	[FirmaId] [int] NULL,
	[IsyeriId] [int] NULL,
	[CalismaSekli] [nvarchar](30) NULL,
	[CepTel] [nvarchar](14) NULL,
	[Fotograf] [image] NULL,
	[KayitTarihi] [date] NULL,
	[Email] [nvarchar](50) NULL,
	[PuantajYapilirMi] [bit] NULL,
	[BolumId] [int] NULL,
	[ZiyaretciMi] [bit] NULL,
	[AracKartiMi] [bit] NULL,
	[TaseronCalisanMi] [bit] NULL,
 CONSTRAINT [PK_Kisiler_1] PRIMARY KEY CLUSTERED 
(
	[PersonelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[YemekhaneGirisLimitler]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[YemekhaneGirisLimitler](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonelId] [nvarchar](50) NOT NULL,
	[GunlukLimit] [int] NULL,
	[KayitTarihi] [datetime] NULL,
	[AktifMi] [bit] NULL,
UNIQUE NONCLUSTERED 
(
	[PersonelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_Personeller]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   VIEW [dbo].[vw_Personeller] AS
SELECT k.PersonelId, k.KartNo, k.Ad + ' ' + k.Soyad AS AdSoyad
FROM     dbo.Kisiler k
WHERE  k.IstenCikisTarihi IS NULL
UNION ALL
SELECT p.KartId AS PersonelId, p.KartNo, p.KartAdi AS AdSoyad
FROM     dbo.PuantajsizKartlar p 
LEFT JOIN dbo.YemekhaneGirisLimitler y ON y.PersonelId = p.KartId AND y.AktifMi = 1
WHERE  p.AktifMi = 1;
GO
/****** Object:  Table [dbo].[YemekhaneEngellenenKullanicilar]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[YemekhaneEngellenenKullanicilar](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonelId] [nvarchar](50) NOT NULL,
	[CihazId] [int] NOT NULL,
	[KartNo] [nvarchar](50) NOT NULL,
	[AdSoyad] [nvarchar](100) NULL,
	[EngellemeTarihi] [datetime] NULL,
	[TekrarEklendiMi] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_YemekhaneEngellenenBekleyenler]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_YemekhaneEngellenenBekleyenler]
AS
WITH Base AS (
    SELECT 
        e.PersonelId,
        e.CihazId,
        e.KartNo,
        e.AdSoyad,
        e.EngellemeTarihi,
        e.TekrarEklendiMi
    FROM dbo.YemekhaneEngellenenKullanicilar e
)
-- 1) Gerçek personeller (Kisiler)
SELECT 
    b.PersonelId,
    b.CihazId,
    b.KartNo,
    b.AdSoyad,
    ISNULL(k.CalismaSekli, N'') AS CalismaSekli
FROM Base b
JOIN dbo.Kisiler k
  ON k.PersonelId = b.PersonelId
WHERE b.TekrarEklendiMi = 0
  AND CAST(b.EngellemeTarihi AS date) >= DATEADD(DAY, -1, CAST(GETDATE() AS date))

UNION ALL

-- 2) Puantajsız/havuz/misafir kartları
SELECT 
    b.PersonelId,
    b.CihazId,
    b.KartNo,
    b.AdSoyad,
    ISNULL(p.CalismaSekli, N'') AS CalismaSekli
FROM Base b
JOIN dbo.PuantajsizKartlar p
  ON p.KartId = b.PersonelId
WHERE b.TekrarEklendiMi = 0
  AND CAST(b.EngellemeTarihi AS date) >= DATEADD(DAY, -1, CAST(GETDATE() AS date))
  AND p.AktifMi = 1;
GO
/****** Object:  Table [dbo].[AvansTalepleri]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvansTalepleri](
	[AvansId] [int] IDENTITY(1,1) NOT NULL,
	[PersonelId] [varchar](50) NOT NULL,
	[Miktar] [decimal](18, 2) NOT NULL,
	[Aciklama] [nvarchar](500) NULL,
	[TalepTarihi] [datetime] NOT NULL,
	[Durum] [tinyint] NOT NULL,
	[OnaylayanId] [int] NULL,
	[OnayTarihi] [datetime] NULL,
	[OnayAciklama] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[AvansId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bildirimler]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bildirimler](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[KullaniciId] [int] NULL,
	[PersonelId] [nvarchar](50) NULL,
	[Baslik] [nvarchar](200) NOT NULL,
	[Mesaj] [nvarchar](1000) NOT NULL,
	[OkunduMu] [bit] NOT NULL,
	[OlusturmaTarihi] [datetime] NOT NULL,
	[Tipi] [nvarchar](50) NULL,
	[IlgiliKayitId] [int] NULL,
 CONSTRAINT [PK_Bildirimler] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bolumler]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bolumler](
	[BolumId] [int] NULL,
	[FirmaId] [int] NULL,
	[BolumAdi] [nvarchar](150) NULL,
	[Id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CalismaSekilleri]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CalismaSekilleri](
	[CalismaSekilId] [int] IDENTITY(1,1) NOT NULL,
	[CalismaSekliAdi] [nvarchar](50) NOT NULL,
	[BaslangicZaman] [datetime] NOT NULL,
	[BitisZaman] [datetime] NOT NULL,
	[BaslangicToleransZaman] [datetime] NOT NULL,
	[BitisToleransZaman] [datetime] NOT NULL,
	[YemekAktiflestirmeZaman] [datetime] NULL,
	[FirmaId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[CalismaSekilId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CalismaStatusu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CalismaStatusu](
	[CalismaStatuId] [int] NOT NULL,
	[CalismaStatuAdi] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CanliIzlemeHesaplari]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CanliIzlemeHesaplari](
	[KullaniciId] [int] IDENTITY(1,1) NOT NULL,
	[FirmaId] [int] NOT NULL,
	[KullaniciAdi] [nvarchar](50) NOT NULL,
	[SifreHash] [varbinary](64) NOT NULL,
	[Rol] [nvarchar](20) NOT NULL,
	[AktifMi] [bit] NOT NULL,
	[OlusturmaT] [datetime2](0) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[KullaniciId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CihazGrupDetay]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CihazGrupDetay](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GrupId] [int] NOT NULL,
	[CihazId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CihazGruplari]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CihazGruplari](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GrupAdi] [nvarchar](50) NOT NULL,
	[FirmaId] [int] NULL,
	[KayitTarihi] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Cihazlar]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cihazlar](
	[CihazId] [int] IDENTITY(1,1) NOT NULL,
	[FirmaId] [int] NOT NULL,
	[CihazAdi] [nvarchar](100) NOT NULL,
	[IPAdres] [nvarchar](50) NOT NULL,
	[Port] [int] NOT NULL,
	[AktifMi] [bit] NOT NULL,
	[BaglandiMi] [bit] NOT NULL,
	[Notlar] [nvarchar](200) NULL,
	[CihazTipi] [tinyint] NOT NULL,
	[CihazModeli] [nvarchar](20) NULL,
	[AnaGirisCikisMi] [bit] NULL,
	[BaglantiParametreleri] [nvarchar](500) NULL,
	[Marka] [nvarchar](50) NULL,
	[SdkTipi] [nvarchar](20) NULL,
	[Latitude] [decimal](18, 7) NULL,
	[Longitude] [decimal](18, 7) NULL,
	[MesafeToleransMetre] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[CihazId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CihazTetikKuyrugu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CihazTetikKuyrugu](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CihazId] [int] NOT NULL,
	[Komut] [nvarchar](50) NOT NULL,
	[Tarih] [datetime] NOT NULL,
	[OkunduMu] [bit] NOT NULL,
	[PersonelId] [nvarchar](50) NULL,
 CONSTRAINT [PK_CihazTetikKuyrugu] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CihazTipler]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CihazTipler](
	[TipId] [int] NOT NULL,
	[TipAdi] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[TipId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CokluSicilBaglantilari]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CokluSicilBaglantilari](
	[TCKimlikNo] [nvarchar](11) NOT NULL,
	[AnaPersonelId] [int] NOT NULL,
	[HedefPersonelId] [int] NOT NULL,
	[FirmaId] [int] NULL,
	[SirketId] [int] NULL,
	[BolumId] [int] NULL,
	[IseGirisTarihi] [date] NULL,
	[IstenCikisTarihi] [date] NULL,
	[AktarimGunSayisi] [int] NOT NULL,
	[AktifMi] [bit] NOT NULL,
	[Aciklama] [nvarchar](200) NULL,
	[OlusturmaZamani] [datetime2](3) NOT NULL,
	[OlusturanKullaniciId] [int] NOT NULL,
	[GuncellemeZamani] [datetime2](3) NULL,
	[GuncelleyenKullaniciId] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Departmanlar]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Departmanlar](
	[DepartmanId] [int] IDENTITY(1,1) NOT NULL,
	[DepartmanAdi] [nvarchar](100) NULL,
	[Aciklama] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[DepartmanId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FinalPuantajVerisi]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FinalPuantajVerisi](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SicilNo] [int] NOT NULL,
	[Ad] [nvarchar](50) NOT NULL,
	[Soyad] [nvarchar](50) NOT NULL,
	[Tarih] [date] NOT NULL,
	[CalismaTipi] [nvarchar](10) NOT NULL,
	[Saat] [decimal](5, 2) NOT NULL,
	[KayitZamani] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[SicilNo] ASC,
	[Tarih] ASC,
	[CalismaTipi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Firmalar]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Firmalar](
	[FirmaId] [int] NOT NULL,
	[FirmaAdi] [nvarchar](200) NULL,
	[ITBirimMail] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[FirmaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Isyerler]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Isyerler](
	[IsyeriId] [int] NULL,
	[FirmaId] [int] NULL,
	[IsyeriAdi] [nvarchar](100) NULL,
	[TaseronMu] [bit] NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_Isyerler] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IzinTalepleri]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IzinTalepleri](
	[TalepId] [int] IDENTITY(1,1) NOT NULL,
	[PersonelId] [varchar](50) NOT NULL,
	[FirmaId] [int] NOT NULL,
	[IzinTipId] [int] NULL,
	[Baslangic] [datetime] NOT NULL,
	[Bitis] [datetime] NOT NULL,
	[SaatlikIzinMi] [bit] NOT NULL,
	[Aciklama] [nvarchar](500) NULL,
	[IzinAdres] [nvarchar](500) NULL,
	[TelefonNo] [nvarchar](50) NULL,
	[TalepTarihi] [datetime] NOT NULL,
	[TalepImzaKullaniciId] [int] NULL,
	[TalepImzaTarihi] [datetime] NULL,
	[UstYetkiliPersonelId] [varchar](50) NULL,
	[UstYetkiliOnayDurumu] [tinyint] NULL,
	[UstYetkiliOnayTarihi] [datetime] NULL,
	[UstYetkiliAciklama] [nvarchar](500) NULL,
	[IkOnayDurumu] [tinyint] NULL,
	[IkOnaylayanKullaniciId] [int] NULL,
	[IkOnayTarihi] [datetime] NULL,
	[IkAciklama] [nvarchar](500) NULL,
	[SonucKisiIzinId] [int] NULL,
	[KullanimImzaIstenen] [bit] NOT NULL,
	[KullanimImzaIstenmeTarihi] [datetime] NULL,
	[KullanimImzaIstenmeKullaniciId] [int] NULL,
	[KullanimImzaKullaniciId] [int] NULL,
	[KullanimImzaTarihi] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[TalepId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IzinTipleri]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IzinTipleri](
	[IzinTipId] [int] IDENTITY(1,1) NOT NULL,
	[Kod] [nvarchar](30) NOT NULL,
	[Adi] [nvarchar](100) NOT NULL,
	[UcretliMi] [bit] NOT NULL,
	[AktifMi] [bit] NOT NULL,
	[OlusturmaZamani] [datetime2](0) NOT NULL,
	[GuncellemeZamani] [datetime2](0) NULL,
	[SaatlikKullanilabilirMi] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[IzinTipId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Kod] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KisiCihazYetkileri]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KisiCihazYetkileri](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirmaId] [int] NULL,
	[PersonelId] [nvarchar](50) NOT NULL,
	[CihazId] [int] NOT NULL,
	[AktifMi] [bit] NOT NULL,
	[KayitZamani] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KisiHareketler]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KisiHareketler](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirmaId] [int] NOT NULL,
	[CihazId] [int] NOT NULL,
	[PersonelId] [int] NOT NULL,
	[Tarih] [datetime] NOT NULL,
	[Tip] [nvarchar](20) NOT NULL,
	[KayitZamani] [datetime] NOT NULL,
	[AktifMi] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KisiIzinler]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KisiIzinler](
	[KisiIzinId] [int] IDENTITY(1,1) NOT NULL,
	[FirmaId] [int] NOT NULL,
	[PersonelId] [nvarchar](50) NULL,
	[IzinId] [int] NOT NULL,
	[Baslangic] [datetime2](0) NOT NULL,
	[Bitis] [datetime2](0) NOT NULL,
	[Aciklama] [nvarchar](500) NULL,
	[OlusturanKullaniciId] [int] NULL,
	[OlusturmaTarihi] [datetime2](0) NOT NULL,
	[GuncellemeTarihi] [datetime2](0) NULL,
	[AktifMi] [bit] NOT NULL,
	[SaatlikIzinMi] [bit] NULL,
	[SureDakika]  AS ([dbo].[fn_IzinSureDakika]([Baslangic],[Bitis],[SaatlikIzinMi])),
PRIMARY KEY CLUSTERED 
(
	[KisiIzinId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KullaniciEkstraYetkilendirme]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KullaniciEkstraYetkilendirme](
	[KullaniciId] [int] NOT NULL,
	[SayfaAdi] [nvarchar](100) NOT NULL,
	[YetkiTipi] [nvarchar](50) NOT NULL,
	[Allowed] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KullaniciFirmaIsyeriYetkileri]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KullaniciFirmaIsyeriYetkileri](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[KullaniciId] [int] NOT NULL,
	[FirmaId] [int] NOT NULL,
	[IsyeriId] [int] NULL,
	[AktifMi] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Kullanicilar]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Kullanicilar](
	[KullaniciId] [nvarchar](30) NOT NULL,
	[KullaniciAdi] [nvarchar](100) NULL,
	[Sifre] [nvarchar](100) NULL,
	[RolId] [int] NULL,
	[PersonelId] [int] NULL,
	[SorumluFirmalari] [nvarchar](100) NULL,
	[SorumluIsyerleri] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[KullaniciId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KullaniciSifreKurtarma]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KullaniciSifreKurtarma](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[KullaniciId] [int] NOT NULL,
	[KurtarmaKodu] [nvarchar](20) NOT NULL,
	[SonKullanmaZamani] [datetime] NOT NULL,
	[Kullanildi] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MobilUygulamaTokenleri]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MobilUygulamaTokenleri](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[KullaniciId] [nvarchar](50) NULL,
	[PersonelId] [varchar](50) NULL,
	[FCMToken] [nvarchar](max) NOT NULL,
	[DeviceType] [nvarchar](50) NULL,
	[LastUpdated] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PersonelWebSifreler]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonelWebSifreler](
	[PersonelId] [nvarchar](50) NOT NULL,
	[Sifre] [nvarchar](100) NOT NULL,
	[KurtarmaKodu] [nvarchar](10) NULL,
	[SonKullanmaZamani] [datetime] NULL,
	[Kullanildi] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PersonelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Pozisyonlar]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pozisyonlar](
	[PozisyonId] [int] IDENTITY(1,1) NOT NULL,
	[PozisyonAdi] [nvarchar](100) NULL,
	[Aciklama] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[PozisyonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PuantajOnay]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PuantajOnay](
	[PersonelId] [int] NOT NULL,
	[Tarih] [date] NOT NULL,
	[OnayDurumu] [int] NOT NULL,
	[DuzenlenmisFMDakika] [int] NULL,
	[Aciklama] [nvarchar](500) NULL,
	[OlusturmaZamani] [datetime] NOT NULL,
	[OlusturanKullaniciId] [int] NULL,
	[GuncellemeZamani] [datetime] NULL,
	[GuncelleyenKullaniciId] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PuantajsizKartAtamalari]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PuantajsizKartAtamalari](
	[AtamaId] [int] IDENTITY(1,1) NOT NULL,
	[KartId] [nvarchar](30) NOT NULL,
	[AtananPersonelId] [int] NULL,
	[MisafirAdSoyad] [nvarchar](150) NULL,
	[Baslangic] [datetime] NOT NULL,
	[Bitis] [datetime] NULL,
	[Notlar] [nvarchar](200) NULL,
	[TCKimlikNo] [nvarchar](11) NULL,
	[ZiyaretEdilenKisi] [nvarchar](150) NULL,
PRIMARY KEY CLUSTERED 
(
	[AtamaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PuantajTipleri]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PuantajTipleri](
	[Kod] [nvarchar](5) NOT NULL,
	[Ad] [nvarchar](120) NOT NULL,
	[VarsayilanSaat] [decimal](5, 2) NULL,
	[AktifMi] [bit] NOT NULL,
	[OlusturmaZamani] [datetime2](3) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RaporTanimlari]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RaporTanimlari](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RaporAdi] [nvarchar](100) NULL,
	[ProcedureAdi] [nvarchar](100) NULL,
	[Aciklama] [nvarchar](255) NULL,
	[AktifMi] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResmiTatiller]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResmiTatiller](
	[Tarih] [date] NOT NULL,
	[Ad] [nvarchar](100) NOT NULL,
	[CalismaSaati] [decimal](5, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[Tarih] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Roller]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roller](
	[RolId] [int] NOT NULL,
	[RolTanimi] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[RolId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SayfaYetkileri]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SayfaYetkileri](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RolId] [int] NOT NULL,
	[SayfaAdi] [nvarchar](100) NOT NULL,
	[YetkiTipi] [nvarchar](50) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SenkronizasyonModulKullanicilar]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SenkronizasyonModulKullanicilar](
	[FirmaId] [int] NULL,
	[RolId] [int] NULL,
	[KullaniciAdi] [nvarchar](200) NULL,
	[Sifre] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SistemAyarlar]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SistemAyarlar](
	[AyarAdi] [nvarchar](50) NOT NULL,
	[AyarDegeri] [nvarchar](50) NOT NULL,
	[GuncellemeZamani] [datetime2](3) NOT NULL,
	[GuncelleyenId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[AyarAdi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SistemLoglari]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SistemLoglari](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LogZamani] [datetime2](3) NOT NULL,
	[KullaniciId] [int] NULL,
	[KullaniciAdi] [nvarchar](150) NULL,
	[IslemTuru] [tinyint] NOT NULL,
	[Kaynak] [nvarchar](100) NULL,
	[Islem] [nvarchar](100) NULL,
	[Mesaj] [nvarchar](2000) NULL,
	[IpAdres] [nvarchar](100) NULL,
	[BilgisayarAdi] [nvarchar](200) NULL,
	[KorelasyonId] [nvarchar](50) NULL,
	[DetayJson] [nvarchar](max) NULL,
	[HataMesaji] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SistemMailAlicilari]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SistemMailAlicilari](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GrupAdi] [nvarchar](50) NOT NULL,
	[EmailAdresi] [nvarchar](150) NOT NULL,
	[AdSoyad] [nvarchar](100) NULL,
	[Aktif] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TaseronKartlari]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TaseronKartlari](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonelId] [int] NOT NULL,
	[TaseronId] [nvarchar](50) NOT NULL,
	[AktifMi] [bit] NOT NULL,
	[OlusturmaTarihi] [datetime2](3) NOT NULL,
	[GuncellemeTarihi] [datetime2](3) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UstYetkililer]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UstYetkililer](
	[PersonelId] [varchar](50) NOT NULL,
	[UstYetkiliPersonelId] [varchar](50) NOT NULL,
	[OlusturmaTarihi] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PersonelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[YemekhaneGecisHareketler]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[YemekhaneGecisHareketler](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonelId] [int] NOT NULL,
	[CihazId] [int] NOT NULL,
	[Tarih] [date] NOT NULL,
	[Saat] [datetime] NOT NULL,
	[KayitZamani] [datetime] NULL,
	[AktifMi] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_CihazTetikKuyrugu_OkunduMu]    Script Date: 13.04.2026 10:28:37 ******/
CREATE NONCLUSTERED INDEX [IX_CihazTetikKuyrugu_OkunduMu] ON [dbo].[CihazTetikKuyrugu]
(
	[OkunduMu] ASC
)
INCLUDE([CihazId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AvansTalepleri] ADD  CONSTRAINT [DF_AvansTalepleri_TalepTarihi]  DEFAULT (getdate()) FOR [TalepTarihi]
GO
ALTER TABLE [dbo].[AvansTalepleri] ADD  CONSTRAINT [DF_AvansTalepleri_Durum]  DEFAULT ((0)) FOR [Durum]
GO
ALTER TABLE [dbo].[Bildirimler] ADD  DEFAULT ((0)) FOR [OkunduMu]
GO
ALTER TABLE [dbo].[Bildirimler] ADD  DEFAULT (getdate()) FOR [OlusturmaTarihi]
GO
ALTER TABLE [dbo].[CanliIzlemeHesaplari] ADD  DEFAULT ((1)) FOR [AktifMi]
GO
ALTER TABLE [dbo].[CanliIzlemeHesaplari] ADD  DEFAULT (sysutcdatetime()) FOR [OlusturmaT]
GO
ALTER TABLE [dbo].[CihazGruplari] ADD  DEFAULT (getdate()) FOR [KayitTarihi]
GO
ALTER TABLE [dbo].[Cihazlar] ADD  DEFAULT ((1)) FOR [AktifMi]
GO
ALTER TABLE [dbo].[Cihazlar] ADD  DEFAULT ((0)) FOR [BaglandiMi]
GO
ALTER TABLE [dbo].[CihazTetikKuyrugu] ADD  CONSTRAINT [DF_CihazTetikKuyrugu_Komut]  DEFAULT ('ACUnlock') FOR [Komut]
GO
ALTER TABLE [dbo].[CihazTetikKuyrugu] ADD  CONSTRAINT [DF_CihazTetikKuyrugu_Tarih]  DEFAULT (getdate()) FOR [Tarih]
GO
ALTER TABLE [dbo].[CihazTetikKuyrugu] ADD  CONSTRAINT [DF_CihazTetikKuyrugu_OkunduMu]  DEFAULT ((0)) FOR [OkunduMu]
GO
ALTER TABLE [dbo].[CokluSicilBaglantilari] ADD  CONSTRAINT [DF_CSB_AktifMi]  DEFAULT ((1)) FOR [AktifMi]
GO
ALTER TABLE [dbo].[CokluSicilBaglantilari] ADD  CONSTRAINT [DF_CSB_OlusturmaZamani]  DEFAULT (sysutcdatetime()) FOR [OlusturmaZamani]
GO
ALTER TABLE [dbo].[FinalPuantajVerisi] ADD  CONSTRAINT [DF_PuantajKesin_Kayit]  DEFAULT (sysdatetime()) FOR [KayitZamani]
GO
ALTER TABLE [dbo].[IzinTalepleri] ADD  CONSTRAINT [DF_IzinTalepleri_SaatlikIzinMi]  DEFAULT ((0)) FOR [SaatlikIzinMi]
GO
ALTER TABLE [dbo].[IzinTalepleri] ADD  CONSTRAINT [DF_IzinTalepleri_TalepTarihi]  DEFAULT (getdate()) FOR [TalepTarihi]
GO
ALTER TABLE [dbo].[IzinTalepleri] ADD  CONSTRAINT [DF_IzinTalepleri_KullanimImzaIstenen]  DEFAULT ((0)) FOR [KullanimImzaIstenen]
GO
ALTER TABLE [dbo].[IzinTipleri] ADD  DEFAULT ((1)) FOR [UcretliMi]
GO
ALTER TABLE [dbo].[IzinTipleri] ADD  DEFAULT ((1)) FOR [AktifMi]
GO
ALTER TABLE [dbo].[IzinTipleri] ADD  DEFAULT (sysutcdatetime()) FOR [OlusturmaZamani]
GO
ALTER TABLE [dbo].[KisiCihazYetkileri] ADD  CONSTRAINT [DF_KCY_AktifMi]  DEFAULT ((1)) FOR [AktifMi]
GO
ALTER TABLE [dbo].[KisiCihazYetkileri] ADD  CONSTRAINT [DF_KCY_Kayit]  DEFAULT (sysdatetime()) FOR [KayitZamani]
GO
ALTER TABLE [dbo].[KisiHareketler] ADD  DEFAULT (getdate()) FOR [KayitZamani]
GO
ALTER TABLE [dbo].[KisiIzinler] ADD  DEFAULT (getdate()) FOR [OlusturmaTarihi]
GO
ALTER TABLE [dbo].[KisiIzinler] ADD  DEFAULT ((1)) FOR [AktifMi]
GO
ALTER TABLE [dbo].[KullaniciFirmaIsyeriYetkileri] ADD  DEFAULT ((1)) FOR [AktifMi]
GO
ALTER TABLE [dbo].[KullaniciSifreKurtarma] ADD  DEFAULT ((0)) FOR [Kullanildi]
GO
ALTER TABLE [dbo].[PersonelWebSifreler] ADD  DEFAULT ((0)) FOR [Kullanildi]
GO
ALTER TABLE [dbo].[SistemAyarlar] ADD  CONSTRAINT [DF_SistemAyarlar_GZ]  DEFAULT (sysdatetime()) FOR [GuncellemeZamani]
GO
ALTER TABLE [dbo].[SistemLoglari] ADD  DEFAULT (getdate()) FOR [LogZamani]
GO
ALTER TABLE [dbo].[SistemMailAlicilari] ADD  DEFAULT ((1)) FOR [Aktif]
GO
ALTER TABLE [dbo].[TaseronKartlari] ADD  DEFAULT (getdate()) FOR [OlusturmaTarihi]
GO
ALTER TABLE [dbo].[UstYetkililer] ADD  CONSTRAINT [DF_UstYetkili_OlusturmaTarihi]  DEFAULT (getdate()) FOR [OlusturmaTarihi]
GO
ALTER TABLE [dbo].[YemekhaneEngellenenKullanicilar] ADD  DEFAULT (getdate()) FOR [EngellemeTarihi]
GO
ALTER TABLE [dbo].[YemekhaneEngellenenKullanicilar] ADD  DEFAULT ((0)) FOR [TekrarEklendiMi]
GO
ALTER TABLE [dbo].[YemekhaneGecisHareketler] ADD  DEFAULT (getdate()) FOR [KayitZamani]
GO
ALTER TABLE [dbo].[YemekhaneGecisHareketler] ADD  DEFAULT ((1)) FOR [AktifMi]
GO
ALTER TABLE [dbo].[YemekhaneGirisLimitler] ADD  DEFAULT ((1)) FOR [GunlukLimit]
GO
ALTER TABLE [dbo].[YemekhaneGirisLimitler] ADD  DEFAULT ((1)) FOR [AktifMi]
GO
ALTER TABLE [dbo].[CihazGrupDetay]  WITH CHECK ADD  CONSTRAINT [FK_CihazGrupDetay_GrupId] FOREIGN KEY([GrupId])
REFERENCES [dbo].[CihazGruplari] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CihazGrupDetay] CHECK CONSTRAINT [FK_CihazGrupDetay_GrupId]
GO
/****** Object:  StoredProcedure [dbo].[sp_AktifPersonellerRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_AktifPersonellerRaporu]
    @FirmaIdList      NVARCHAR(MAX),       -- virgüllü firmaId listesi
    @IsyeriIdList     NVARCHAR(MAX) = NULL,
    @TarihBaslangic   DATE         = NULL, -- bu raporda kullanılmayacak
    @TarihBitis       DATE         = NULL  -- bu raporda kullanılmayacak
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================
    -- 1) Firma & İşyeri filtreleri
    -- =========================
    IF OBJECT_ID('tempdb..#Firmalar') IS NOT NULL DROP TABLE #Firmalar;
    IF OBJECT_ID('tempdb..#Isyerleri') IS NOT NULL DROP TABLE #Isyerleri;

    SELECT TRY_CAST(value AS INT) AS FirmaId
    INTO #Firmalar
    FROM STRING_SPLIT(@FirmaIdList, ',')
    WHERE TRY_CAST(value AS INT) IS NOT NULL;

    CREATE TABLE #Isyerleri (IsyeriId INT PRIMARY KEY);

    IF (@IsyeriIdList IS NOT NULL AND LEN(@IsyeriIdList) > 0)
    BEGIN
        INSERT INTO #Isyerleri(IsyeriId)
        SELECT DISTINCT TRY_CAST(value AS INT)
        FROM STRING_SPLIT(@IsyeriIdList, ',')
        WHERE TRY_CAST(value AS INT) IS NOT NULL;
    END
    ELSE
    BEGIN
        INSERT INTO #Isyerleri(IsyeriId)
        SELECT DISTINCT i.IsyeriId
        FROM dbo.Isyerler i WITH (NOLOCK)
        WHERE EXISTS (SELECT 1 FROM #Firmalar f WHERE f.FirmaId = i.FirmaId);
    END

    -- =========================
    -- 2) Aktif personel listesi
    -- =========================
    SELECT
        f.FirmaAdi              AS [Firma],
        k.PersonelId            AS [Sicil No],
        k.Ad,
        k.Soyad,
        k.IseGirisTarihi        AS [İşe Giriş Tarihi],
        i.IsyeriAdi             AS [İşyeri]
    FROM dbo.Kisiler   k WITH (NOLOCK)
    INNER JOIN dbo.Firmalar f WITH (NOLOCK) ON f.FirmaId = k.FirmaId
    LEFT  JOIN dbo.Isyerler i WITH (NOLOCK) ON i.IsyeriId = k.IsyeriId
    WHERE k.PuantajYapilirMi = 1
      AND k.IstenCikisTarihi IS NULL
      AND EXISTS (SELECT 1 FROM #Firmalar  ff WHERE ff.FirmaId  = k.FirmaId)
      AND EXISTS (SELECT 1 FROM #Isyerleri ii WHERE ii.IsyeriId = k.IsyeriId)
    ORDER BY
        f.FirmaAdi,
        k.Soyad,
        k.Ad;

    DROP TABLE IF EXISTS #Firmalar;
    DROP TABLE IF EXISTS #Isyerleri;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_AnlikDisaridakilerRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_AnlikDisaridakilerRaporu]
(
    @FirmaIdList     NVARCHAR(MAX),
    @IsyeriIdList    NVARCHAR(MAX),
    @TarihBaslangic  DATE,  -- UC uyumu için var, anlık raporda kullanılmaz
    @TarihBitis      DATE   -- UC uyumu için var, anlık raporda kullanılmaz
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Bugun DATE = CAST(GETDATE() AS DATE);
    DECLARE @BugunBas DATETIME = CAST(@Bugun AS DATETIME);
    DECLARE @BugunBit DATETIME = DATEADD(SECOND, -1, DATEADD(DAY, 1, @BugunBas));

    DECLARE @Firma TABLE (FirmaId INT PRIMARY KEY);
    IF (ISNULL(@FirmaIdList,'') <> '')
        INSERT INTO @Firma(FirmaId)
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@FirmaIdList, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

    DECLARE @Isyeri TABLE (IsyeriId INT PRIMARY KEY);
    IF (ISNULL(@IsyeriIdList,'') <> '')
        INSERT INTO @Isyeri(IsyeriId)
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@IsyeriIdList, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

    ;WITH
    AktifPersoneller AS
    (
        SELECT
            K.PersonelId,
            K.FirmaId,
            K.IsyeriId,
            K.Ad,
            K.Soyad
        FROM dbo.Kisiler K
        WHERE
            K.PuantajYapilirMi = 1           
            AND K.IseGirisTarihi <= @Bugun
            AND (K.IstenCikisTarihi IS NULL OR K.IstenCikisTarihi >= @Bugun)
            AND (ISNULL(@FirmaIdList,'')  = '' OR K.FirmaId  IN (SELECT FirmaId  FROM @Firma))
            AND (ISNULL(@IsyeriIdList,'') = '' OR K.IsyeriId IN (SELECT IsyeriId FROM @Isyeri))
    ),
    GunlukIzinliler AS
    (
        SELECT DISTINCT KI.PersonelId
        FROM dbo.KisiIzinler KI
        WHERE
            KI.AktifMi = 1
            AND KI.SaatlikIzinMi = 0
            AND KI.Baslangic <= @BugunBas
            AND KI.Bitis     >= DATEADD(DAY, 1, @BugunBas)
    ),
    BugunHareketiOlanlar AS
    (
        SELECT DISTINCT KH.PersonelId
        FROM dbo.KisiHareketler KH
        INNER JOIN dbo.Cihazlar C ON C.CihazId = KH.CihazId
        WHERE
            KH.AktifMi = 1
            AND KH.Tip IN (N'Giriş', N'Çıkış')
            AND ISNULL(C.Notlar,N'') NOT LIKE N'%(Ara Kat)%'
            AND KH.Tarih BETWEEN @BugunBas AND @BugunBit
    ),
    SonHareket AS
    (
        SELECT *
        FROM
        (
            SELECT
                KH.PersonelId,
                KH.Tip,
                KH.Tarih,
                C.CihazAdi,
                ROW_NUMBER() OVER
                (
                    PARTITION BY KH.PersonelId
                    ORDER BY KH.Tarih DESC, KH.Id DESC
                ) AS rn
            FROM dbo.KisiHareketler KH
            INNER JOIN dbo.Cihazlar C ON C.CihazId = KH.CihazId
            WHERE
                KH.AktifMi = 1
                AND KH.Tip IN (N'Giriş', N'Çıkış')
                AND ISNULL(C.Notlar,N'') NOT LIKE N'%(Ara Kat)%'
        ) X
        WHERE rn = 1
    )
    SELECT
        AP.PersonelId                                       AS [Sicil No],
        AP.Ad + N' ' + AP.Soyad                             AS [Adı Soyadı],
        IY.IsyeriAdi                                        AS [İşyeri],
        FORMAT(SH.Tarih, 'dd.MM.yyyy dddd', 'tr-TR')        AS [Son Hareket Tarihi],
        FORMAT(SH.Tarih, 'HH:mm')                           AS [Son Hareket Saati],
        SH.CihazAdi                                         AS [Son Turnike],
        CASE
            WHEN SH.Tip = N'Çıkış' THEN DATEDIFF(MINUTE, SH.Tarih, GETDATE())
            ELSE NULL
        END                                                 AS [Dışarıda Kalma Süresi (Dakika)]
    FROM AktifPersoneller AP
    INNER JOIN BugunHareketiOlanlar BHO ON BHO.PersonelId = AP.PersonelId
    LEFT JOIN SonHareket SH ON SH.PersonelId = AP.PersonelId
    LEFT JOIN dbo.Isyerler IY ON IY.IsyeriId = AP.IsyeriId AND IY.FirmaId = AP.FirmaId
    WHERE
        AP.PersonelId NOT IN (SELECT PersonelId FROM GunlukIzinliler)
        AND (SH.Tip IS NULL OR SH.Tip <> N'Giriş')
    ORDER BY SH.Tarih DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_AnlikIceridekilerRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_AnlikIceridekilerRaporu]
(
    @FirmaIdList     NVARCHAR(MAX),
    @IsyeriIdList    NVARCHAR(MAX),
    @TarihBaslangic  DATE,  -- UC uyumu için var, anlık raporda kullanılmaz
    @TarihBitis      DATE   -- UC uyumu için var, anlık raporda kullanılmaz
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Bugun DATE = CAST(GETDATE() AS DATE);
    DECLARE @BugunBas DATETIME = CAST(@Bugun AS DATETIME);
    DECLARE @BugunBit DATETIME = DATEADD(SECOND, -1, DATEADD(DAY, 1, @BugunBas));

    /* Firma / İşyeri CSV → tablo (boşsa filtre yok) */
    DECLARE @Firma TABLE (FirmaId INT PRIMARY KEY);
    IF (ISNULL(@FirmaIdList,'') <> '')
        INSERT INTO @Firma(FirmaId)
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@FirmaIdList, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

    DECLARE @Isyeri TABLE (IsyeriId INT PRIMARY KEY);
    IF (ISNULL(@IsyeriIdList,'') <> '')
        INSERT INTO @Isyeri(IsyeriId)
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@IsyeriIdList, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

    ;WITH

    /* 1) Bugün itibarıyla aktif + puantajlı personeller */
    AktifPersoneller AS
    (
        SELECT
            K.PersonelId,
            K.FirmaId,
            K.IsyeriId,
            K.Ad,
            K.Soyad
        FROM dbo.Kisiler K
        WHERE
            K.PuantajYapilirMi = 1
            AND K.IseGirisTarihi <= @Bugun
            AND (K.IstenCikisTarihi IS NULL OR K.IstenCikisTarihi >= @Bugun)
            AND (ISNULL(@FirmaIdList,'')  = '' OR K.FirmaId  IN (SELECT FirmaId  FROM @Firma))
            AND (ISNULL(@IsyeriIdList,'') = '' OR K.IsyeriId IN (SELECT IsyeriId FROM @Isyeri))
    ),

    /* 2) Bugün tam gün izinliler (hariç) */
    GunlukIzinliler AS
    (
        SELECT DISTINCT KI.PersonelId
        FROM dbo.KisiIzinler KI
        WHERE
            KI.AktifMi = 1
            AND KI.SaatlikIzinMi = 0
            AND KI.Baslangic <= @BugunBas
            AND KI.Bitis     >= DATEADD(DAY, 1, @BugunBas)
    ),

    /* 3) Bugün en az 1 hareketi olanlar (Ara Kat hariç, Giriş/Çıkış) */
    BugunHareketiOlanlar AS
    (
        SELECT DISTINCT KH.PersonelId
        FROM dbo.KisiHareketler KH
        INNER JOIN dbo.Cihazlar C ON C.CihazId = KH.CihazId
        WHERE
            KH.AktifMi = 1
            AND KH.Tip IN (N'Giriş', N'Çıkış')
            AND ISNULL(C.Notlar,N'') NOT LIKE N'%(Ara Kat)%'
            AND KH.Tarih BETWEEN @BugunBas AND @BugunBit
    ),

    /* 4) Kişi bazında son hareket (tüm zamanlar) */
    SonHareket AS
    (
        SELECT *
        FROM
        (
            SELECT
                KH.PersonelId,
                KH.Tip,
                KH.Tarih,
                C.CihazAdi,
                ROW_NUMBER() OVER
                (
                    PARTITION BY KH.PersonelId
                    ORDER BY KH.Tarih DESC, KH.Id DESC
                ) AS rn
            FROM dbo.KisiHareketler KH
            INNER JOIN dbo.Cihazlar C ON C.CihazId = KH.CihazId
            WHERE
                KH.AktifMi = 1
                AND KH.Tip IN (N'Giriş', N'Çıkış')
                AND ISNULL(C.Notlar,N'') NOT LIKE N'%(Ara Kat)%'
        ) X
        WHERE rn = 1
    )

    /* ANLIK İÇERİDEKİLER = bugün hareketi var + son hareket giriş + izinli değil */
    SELECT
        AP.PersonelId                                       AS [Sicil No],
        AP.Ad + N' ' + AP.Soyad                             AS [Adı Soyadı],
        IY.IsyeriAdi                                        AS [İşyeri],
        FORMAT(SH.Tarih, 'dd.MM.yyyy dddd', 'tr-TR')        AS [Son Hareket Tarihi],
        FORMAT(SH.Tarih, 'HH:mm')                           AS [Son Giriş Saati],
        SH.CihazAdi                                         AS [Giriş Turnikesi],
        DATEDIFF(MINUTE, SH.Tarih, GETDATE())               AS [İçeride Kalma Süresi (Dakika)]
    FROM AktifPersoneller AP
    INNER JOIN BugunHareketiOlanlar BHO ON BHO.PersonelId = AP.PersonelId
    INNER JOIN SonHareket SH ON SH.PersonelId = AP.PersonelId
    LEFT JOIN dbo.Isyerler IY ON IY.IsyeriId = AP.IsyeriId AND IY.FirmaId = AP.FirmaId
    WHERE
        AP.PersonelId NOT IN (SELECT PersonelId FROM GunlukIzinliler)
        AND SH.Tip = N'Giriş'
    ORDER BY SH.Tarih DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_Ayar_Get]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- 2) Get/Set SP'leri (istersen tek satır SQL de kullanabilirsin)
CREATE   PROC [dbo].[sp_Ayar_Get]
    @AyarAdi nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP(1) AyarDegeri FROM dbo.SistemAyarlar WITH (UPDLOCK, HOLDLOCK) WHERE AyarAdi=@AyarAdi;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_Ayar_Set]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROC [dbo].[sp_Ayar_Set]
    @AyarAdi nvarchar(50),
    @AyarDegeri nvarchar(50),
    @KullaniciId int = NULL
AS
BEGIN
    SET NOCOUNT ON;
    MERGE dbo.SistemAyarlar AS T
    USING (SELECT @AyarAdi AS AyarAdi) AS S
    ON T.AyarAdi = S.AyarAdi
    WHEN MATCHED THEN
        UPDATE SET T.AyarDegeri=@AyarDegeri, T.GuncellemeZamani=sysdatetime(), T.GuncelleyenId=@KullaniciId
    WHEN NOT MATCHED THEN
        INSERT (AyarAdi, AyarDegeri, GuncellemeZamani, GuncelleyenId)
        VALUES (@AyarAdi, @AyarDegeri, sysdatetime(), @KullaniciId);
END
GO
/****** Object:  StoredProcedure [dbo].[sp_AylikPuantajVeri]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--AN İTİBARİYLE EN KARARLI VERSİYONUM BU!!!!!!!-----
 CREATE   PROCEDURE [dbo].[sp_AylikPuantajVeri]
  @PersonelId int,
  @Yil int,
  @Ay  int
AS
BEGIN
  SET NOCOUNT ON;

  DECLARE @AyBas date = DATEFROMPARTS(@Yil,@Ay,1);
  DECLARE @AySon date = EOMONTH(@AyBas);

  DECLARE @TolBasSaat int = 4;   -- başlangıç toleransı (saat)
  DECLARE @TolBitSaat int = 4;   -- bitiş toleransı (saat)

  ----------------------------------------------------------------
  -- 0) Kişinin vardiya seçenekleri
  ----------------------------------------------------------------
  DECLARE @Opts TABLE(
    CalismaSekilId int PRIMARY KEY,
    VardiyaTuru nvarchar(200),
    VarBas time(0),
    VarBit time(0),
    IsOvernight bit
  );

  INSERT @Opts(CalismaSekilId,VardiyaTuru,VarBas,VarBit,IsOvernight)
  SELECT cs.CalismaSekilId,
         cs.CalismaSekliAdi,
         CAST(cs.BaslangicZaman AS time(0)),
         CAST(cs.BitisZaman     AS time(0)),
         CASE WHEN CAST(cs.BitisZaman AS time(0)) <= CAST(cs.BaslangicZaman AS time(0)) THEN 1 ELSE 0 END
  FROM dbo.Kisiler k
  OUTER APPLY (
      SELECT TRY_CONVERT(int, LTRIM(RTRIM(value))) AS SekilId
      FROM STRING_SPLIT(ISNULL(k.CalismaSekli,''), ',')
      WHERE TRY_CONVERT(int, LTRIM(RTRIM(value))) IS NOT NULL
  ) s
  JOIN dbo.CalismaSekilleri cs ON cs.CalismaSekilId = s.SekilId
  WHERE k.PersonelId = @PersonelId;

  ----------------------------------------------------------------
  -- 1) Ham hareketler (Yemekhane hariç)
  ----------------------------------------------------------------
  IF OBJECT_ID('tempdb..#H') IS NOT NULL DROP TABLE #H;
  CREATE TABLE #H (Id int PRIMARY KEY, Tarih datetime);

  INSERT #H(Id,Tarih)
  SELECT Id, Tarih
  FROM dbo.KisiHareketler WITH (NOLOCK)
  WHERE PersonelId=@PersonelId
    AND AktifMi=1
    AND (Tip IS NULL OR Tip <> 'Yemekhane')
    AND Tarih >= @AyBas
    AND Tarih <  DATEADD(DAY,2,@AySon);   -- ay sonu gece sarksın

  -- Kullanılan hareketler
  IF OBJECT_ID('tempdb..#Used') IS NOT NULL DROP TABLE #Used;
  CREATE TABLE #Used (Id int PRIMARY KEY);

  -- Gün sonu çıktı havuzu
  IF OBJECT_ID('tempdb..#Out') IS NOT NULL DROP TABLE #Out;
  CREATE TABLE #Out (
    Tarih date,
    VardiyaTuru nvarchar(200) NULL,
    IlkGiris time(0) NULL,
    SonCikis time(0) NULL,
    VardiyaBaslangic time(0) NULL,
    VardiyaBitis time(0) NULL,
    SistemFMDakika int NOT NULL,
    TekKayit bit NOT NULL,
    HamSay int NOT NULL
  );

  ----------------------------------------------------------------
  -- 2) Gün gün ilerle
  ----------------------------------------------------------------
  DECLARE @Gun date = @AyBas;

  WHILE @Gun <= @AySon
  BEGIN
    DECLARE @HamSay int =
      (SELECT COUNT(*) FROM #H h
        WHERE h.Tarih >= CAST(@Gun AS datetime)
          AND h.Tarih <  DATEADD(HOUR,12, DATEADD(DAY,1, CAST(@Gun AS datetime))));

    IF OBJECT_ID('tempdb..#Cand') IS NOT NULL DROP TABLE #Cand;
    CREATE TABLE #Cand (
      CalismaSekilId int,
      VardiyaTuru nvarchar(200),
      IsOvernight bit,
      VarBasDT datetime,
      VarBitDT datetime,
      IlkId int NULL,
      IlkZaman datetime NULL,
      SonId int NULL,
      SonZaman datetime NULL,
      IlkSkor int NULL,
      SonSkor int NULL,
      Skor int NULL
    );

    DECLARE
      @OptSekilId int, @OptTur nvarchar(200),
      @OptBas time(0), @OptBit time(0), @OptOV bit;

    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
      SELECT CalismaSekilId, VardiyaTuru, VarBas, VarBit, IsOvernight FROM @Opts;

    OPEN cur;
    FETCH NEXT FROM cur INTO @OptSekilId, @OptTur, @OptBas, @OptBit, @OptOV;

    WHILE @@FETCH_STATUS = 0
    BEGIN
      DECLARE
        @VarBasDT datetime =
          DATETIMEFROMPARTS(YEAR(@Gun),MONTH(@Gun),DAY(@Gun),
                            DATEPART(HOUR,@OptBas),DATEPART(MINUTE,@OptBas),0,0),
        @VarBitDT datetime =
          DATETIMEFROMPARTS(
              YEAR(CASE WHEN @OptOV=1 THEN DATEADD(DAY,1,@Gun) ELSE @Gun END),
              MONTH(CASE WHEN @OptOV=1 THEN DATEADD(DAY,1,@Gun) ELSE @Gun END),
              DAY  (CASE WHEN @OptOV=1 THEN DATEADD(DAY,1,@Gun) ELSE @Gun END),
              DATEPART(HOUR,@OptBit),DATEPART(MINUTE,@OptBit),0,0);

      -- başlangıç adayı
      DECLARE @IlkId int = NULL, @IlkDT datetime = NULL, @IlkSkor int = NULL;

      SELECT TOP(1)
        @IlkId = h.Id,
        @IlkDT = h.Tarih,
        @IlkSkor = ABS(DATEDIFF(MINUTE, @VarBasDT, h.Tarih))
      FROM #H h
      WHERE NOT EXISTS (SELECT 1 FROM #Used u WHERE u.Id = h.Id)
        AND h.Tarih BETWEEN DATEADD(HOUR,-@TolBasSaat,@VarBasDT)
                        AND DATEADD(HOUR, @TolBasSaat,@VarBasDT)
      ORDER BY ABS(DATEDIFF(MINUTE, @VarBasDT, h.Tarih)) ASC, h.Tarih ASC;

      -- bitiş adayı
      DECLARE @SonId int = NULL, @SonDT datetime = NULL, @SonSkor int = NULL;

      SELECT TOP(1)
        @SonId = h.Id,
        @SonDT = h.Tarih,
        @SonSkor = ABS(DATEDIFF(MINUTE, @VarBitDT, h.Tarih))
      FROM #H h
      WHERE NOT EXISTS (SELECT 1 FROM #Used u WHERE u.Id = h.Id)
        AND h.Tarih BETWEEN DATEADD(HOUR,-@TolBitSaat,@VarBitDT)
                        AND DATEADD(HOUR, @TolBitSaat,@VarBitDT)
      ORDER BY ABS(DATEDIFF(MINUTE, @VarBitDT, h.Tarih)) ASC, h.Tarih DESC;

      INSERT #Cand
      SELECT @OptSekilId, @OptTur, @OptOV, @VarBasDT, @VarBitDT,
             @IlkId, @IlkDT, @SonId, @SonDT,
             @IlkSkor, @SonSkor,
             (ISNULL(@IlkSkor,1440) + ISNULL(@SonSkor,1440));

      FETCH NEXT FROM cur INTO @OptSekilId, @OptTur, @OptBas, @OptBit, @OptOV;
    END
    CLOSE cur; DEALLOCATE cur;

    -- En iyi adayı seç
    DECLARE @PickSekilId int =
      (SELECT TOP(1) CalismaSekilId
       FROM #Cand
       ORDER BY
         CASE WHEN IlkId IS NOT NULL AND SonId IS NOT NULL THEN 1 ELSE 0 END DESC,
         CASE WHEN IsOvernight=0 THEN 1 ELSE 0 END DESC,
         Skor ASC,
         CalismaSekilId);

    DECLARE
      @pTur nvarchar(200) = NULL, @pOV bit = 0,
      @pVarBasDT datetime = NULL, @pVarBitDT datetime = NULL,
      @pIlkId int = NULL, @pIlkDT datetime = NULL,
      @pSonId int = NULL, @pSonDT datetime = NULL;

    SELECT TOP(1)
      @pTur = VardiyaTuru, @pOV = IsOvernight,
      @pVarBasDT = VarBasDT, @pVarBitDT = VarBitDT,
      @pIlkId = IlkId, @pIlkDT = IlkZaman,
      @pSonId = SonId, @pSonDT = SonZaman
    FROM #Cand WHERE CalismaSekilId = @PickSekilId;

    -- Sistem FM
    DECLARE @Erken int = 0, @Gec int = 0, @SFM int = 0;
    IF @pIlkDT IS NOT NULL AND @pIlkDT < @pVarBasDT
      SET @Erken = ((DATEDIFF(MINUTE,@pIlkDT,@pVarBasDT)+15)/30)*30;
    IF @pSonDT IS NOT NULL AND @pSonDT > @pVarBitDT
      SET @Gec   = ((DATEDIFF(MINUTE,@pVarBitDT,@pSonDT)+15)/30)*30;
    SET @SFM = @Erken + @Gec;

    DECLARE @Tek bit = CASE WHEN (@pIlkId IS NULL AND @pSonId IS NOT NULL)
                               OR (@pIlkId IS NOT NULL AND @pSonId IS NULL)
                             THEN 1 ELSE 0 END;

    IF @pIlkId IS NOT NULL INSERT #Used(Id) SELECT @pIlkId WHERE NOT EXISTS(SELECT 1 FROM #Used u WHERE u.Id=@pIlkId);
    IF @pSonId IS NOT NULL INSERT #Used(Id) SELECT @pSonId WHERE NOT EXISTS(SELECT 1 FROM #Used u WHERE u.Id=@pSonId);

    INSERT #Out
    SELECT
      @Gun,
      @pTur,
      CONVERT(time(0), @pIlkDT),
      CONVERT(time(0), @pSonDT),
      CONVERT(time(0), @pVarBasDT),
      CONVERT(time(0), @pVarBitDT),
      ISNULL(@SFM,0),
      ISNULL(@Tek,0),
      @HamSay;

    SET @Gun = DATEADD(DAY,1,@Gun);
  END

  ----------------------------------------------------------------
  -- 3) Ek durumları ve saat hesabını uygula
  ----------------------------------------------------------------
  ;WITH Final AS (SELECT * FROM #Out),
  KisiStat AS (SELECT TOP(1) CalismaStatusu FROM dbo.Kisiler WHERE PersonelId=@PersonelId),
 GunEk AS (
    SELECT 
        o.Tarih,
        -- O tarihte resmi tatil var mı?
        TatilFlag = CASE 
                      WHEN EXISTS(SELECT 1 FROM dbo.ResmiTatiller rt WHERE rt.Tarih = o.Tarih) 
                      THEN 1 ELSE 0 
                    END,
        -- Varsa ilgili çalışılacak saat (tam gün=7.50, yarım gün=3.75 vb.)
        TatilSaat = (
            SELECT TOP (1) rt.CalismaSaati 
            FROM dbo.ResmiTatiller rt 
            WHERE rt.Tarih = o.Tarih
        ),
        -- O tarihte kişiye ait bir izin var mı? (kod/isim)
        IzinAdi = (
            SELECT TOP(1) COALESCE(it.Kod, it.Adi, N'İzin')
            FROM dbo.KisiIzinler ki
            LEFT JOIN dbo.IzinTipleri it ON it.IzinTipId = ki.IzinId
            WHERE ki.PersonelId = @PersonelId 
              AND ki.AktifMi = 1
              AND CAST(o.Tarih AS datetime) BETWEEN ki.Baslangic AND ki.Bitis
            ORDER BY ki.Baslangic DESC
        )
    FROM Final o
)
,
  AyAralik AS (SELECT MIN(CAST(Tarih AS date)) AS IlkGun, MAX(CAST(Tarih AS date)) AS SonGun FROM #H)
 -- >>> (mevcut CTE zincirinde) AyAralik AS (...)'den SONRA ek CTE'ler ve final SELECT
 , IzinOzet AS
(
    -- Bu ay için SAATLİK izinlerin günlük toplamını (dakika) hazırla
    SELECT
        ki.PersonelId,
        Tarih = CAST(ki.Baslangic AS date),
        SaatlikIzinDakika = SUM(ki.SureDakika)
    FROM dbo.KisiIzinler ki
    WHERE ki.AktifMi = 1
      AND ki.SaatlikIzinMi = 1
      AND ki.PersonelId = @PersonelId
      AND ki.Baslangic >= DATEFROMPARTS(@Yil, @Ay, 1)
      AND ki.Baslangic <  DATEADD(MONTH, 1, DATEFROMPARTS(@Yil, @Ay, 1))
    GROUP BY ki.PersonelId, CAST(ki.Baslangic AS date)
)

, K AS (
    -- FinalPuantajVerisi join'i için SicilNo
    SELECT TOP(1) PersonelId
    FROM dbo.Kisiler
    WHERE PersonelId = @PersonelId
)
, Calc AS (
    SELECT
      f.Tarih,
      CASE WHEN f.HamSay>0 AND f.VardiyaTuru IS NULL THEN NULL ELSE f.VardiyaTuru END AS VardiyaTuru,
      f.IlkGiris,
      f.SonCikis,
      f.VardiyaBaslangic,
      f.VardiyaBitis,

      OnayDurumu_Hesap    = 0,
      DuzenlenmisFM_Hesap = NULL,
      Aciklama_Hesap =
        CASE
          WHEN f.HamSay>0 AND f.VardiyaTuru IS NULL THEN N'TANIMLANMAMIŞ VARDİYA'
          WHEN f.TekKayit = 1 THEN N'EKSİK VERİ'
          ELSE NULL
        END,

      -- Çalışma Tipi öncelik: İZİN > RESMİ TATİL > (uyarılar/statü)
      CalismaTipi_Calc =
        CASE
          WHEN ge.IzinAdi IS NOT NULL THEN ge.IzinAdi
          WHEN ge.TatilFlag = 1        THEN N'RT'
          WHEN f.HamSay>0 AND f.VardiyaTuru IS NULL THEN N'TANIMLANMAMIŞ VARDİYA'
          WHEN f.TekKayit = 1 THEN N'EKSİK VERİ'
          ELSE
            CASE (SELECT CalismaStatusu FROM KisiStat)
              WHEN 1 THEN CASE WHEN (f.IlkGiris IS NOT NULL OR f.SonCikis IS NOT NULL) THEN 'NG' ELSE 'HT' END
              WHEN 2 THEN
                CASE
                  WHEN (f.IlkGiris IS NOT NULL AND f.SonCikis IS NOT NULL AND f.SistemFMDakika=0) THEN 'NG'
                  WHEN (f.IlkGiris IS NOT NULL OR f.SonCikis IS NOT NULL) THEN 'FM1'
                  ELSE 'D'
                END
              WHEN 3 THEN
                CASE WHEN f.Tarih BETWEEN (SELECT IlkGun FROM AyAralik) AND (SELECT SonGun FROM AyAralik)
                     THEN 'NG' ELSE 'HT' END
              ELSE 'HT'
            END
        END,

      -- Saat hesabı: İZİN => 7.50, RT => ResmiTatiller.CalismaSaati, diğerleri mevcut mantık
      Saat_Calc = CAST(
                   CASE
                     WHEN ge.IzinAdi IS NOT NULL THEN 7.50
                     WHEN ge.TatilFlag = 1        THEN ISNULL(ge.TatilSaat, 7.50)
                     WHEN f.HamSay>0 AND f.VardiyaTuru IS NULL THEN 0
                     WHEN f.TekKayit = 1 THEN 0
                     ELSE CASE (SELECT CalismaStatusu FROM KisiStat)
                            WHEN 2 THEN CASE 
                                           WHEN (f.IlkGiris IS NOT NULL OR f.SonCikis IS NOT NULL)
                                             THEN 7.5 + (CAST(f.SistemFMDakika AS decimal(10,2))/60.0)
                                           ELSE 7.5
                                         END
                            ELSE 7.5
                          END
                   END AS decimal(6,2))
    FROM Final f
    LEFT JOIN GunEk ge ON ge.Tarih = f.Tarih
)

SELECT
  c.Tarih,
  c.VardiyaTuru,
  c.IlkGiris,
  c.SonCikis,
  c.VardiyaBaslangic,
  c.VardiyaBitis,
  	  ISNULL(iz.SaatlikIzinDakika, 0) AS SaatlikIzinDakika,
  -- Onay verileri: varsa PuantajOnay'dan, yoksa hesap (0/NULL)
  OnayDurumu          = ISNULL(po.OnayDurumu, c.OnayDurumu_Hesap),
  DuzenlenmisFMDakika = ISNULL(po.DuzenlenmisFMDakika, c.DuzenlenmisFM_Hesap),
  Aciklama            = ISNULL(po.Aciklama, c.Aciklama_Hesap),

  -- Çalışma Tipi & Saat: varsa Final'den, yoksa hesaplanan
  CalismaTipi = ISNULL(fpv.CalismaTipi, c.CalismaTipi_Calc),
  Saat        = CAST(ISNULL(fpv.Saat,     c.Saat_Calc)       AS decimal(6,2))

FROM Calc c
CROSS JOIN K
LEFT JOIN dbo.PuantajOnay po
  ON po.PersonelId = K.PersonelId
 AND po.Tarih      = c.Tarih
LEFT JOIN dbo.FinalPuantajVerisi fpv
  ON fpv.SicilNo = K.PersonelId
 AND fpv.Tarih   = c.Tarih
 LEFT JOIN IzinOzet iz
  ON iz.PersonelId = K.PersonelId
 AND iz.Tarih      = c.Tarih
ORDER BY c.Tarih;

END
GO
/****** Object:  StoredProcedure [dbo].[sp_CokluSicileAktar]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_CokluSicileAktar]
    @AnaPersonelId int,      -- ana sicil (FinalPuantajVerisi.SicilNo / PuantajOnay.PersonelId)
    @Yil           int,
    @Ay            int,
    @KullaniciId   int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @SonGun date = EOMONTH(DATEFROMPARTS(@Yil,@Ay,1));

    /* Hedefler + kişi bilgisi + kaç gün aktarılacağı */
    DECLARE @Hedefler TABLE
    (
        HedefSicilNo     int            NOT NULL PRIMARY KEY,
        AktarimGunSayisi int            NOT NULL,
        Ad               nvarchar(100)  NULL,
        Soyad            nvarchar(100)  NULL
    );

    INSERT @Hedefler(HedefSicilNo, AktarimGunSayisi, Ad, Soyad)
    SELECT
        c.HedefPersonelId,
        NULLIF(c.AktarimGunSayisi,0),               -- 0 gelirse işlemesin
        k.Ad, k.Soyad
    FROM dbo.CokluSicilBaglantilari c
    LEFT JOIN dbo.Kisiler k ON k.PersonelId = c.HedefPersonelId
    WHERE c.AnaPersonelId = @AnaPersonelId
      AND c.AktifMi = 1;

    DECLARE @N       int = COALESCE((SELECT SUM(AktarimGunSayisi) FROM @Hedefler),0);
    DECLARE @MaxGun  int = COALESCE((SELECT MAX(AktarimGunSayisi) FROM @Hedefler),0);
	DECLARE @AnaAd    nvarchar(100) = (SELECT TOP(1) Ad    FROM dbo.Kisiler WHERE PersonelId = @AnaPersonelId);
	DECLARE @AnaSoyad nvarchar(100) = (SELECT TOP(1) Soyad FROM dbo.Kisiler WHERE PersonelId = @AnaPersonelId);

    IF @N = 0 OR @MaxGun = 0 RETURN;

    /* --- 1) HEDEF sicillerine gün gün NG 7,5 yaz --- */
    ;WITH Tally AS
    (
        SELECT TOP (@MaxGun)
               ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
        FROM sys.all_objects a CROSS JOIN sys.all_objects b
    ),
    S_Hedef AS
    (
        SELECT
        h.HedefSicilNo               AS SicilNo,
        COALESCE(NULLIF(h.Ad,    N''), @AnaAd,    N'') AS Ad,
        COALESCE(NULLIF(h.Soyad, N''), @AnaSoyad, N'') AS Soyad,
        DATEADD(DAY, -t.n, @SonGun)  AS Tarih,
        CAST(7.50 AS decimal(6,2))   AS Saat
    FROM @Hedefler h
    JOIN Tally t
         ON t.n < h.AktarimGunSayisi
    )
    MERGE dbo.FinalPuantajVerisi AS T
    USING (SELECT SicilNo, Ad, Soyad, Tarih,
                  CalismaTipi = N'NG', Saat, KayitZamani = SYSDATETIME()
           FROM S_Hedef) AS S
      ON  T.SicilNo = S.SicilNo
      AND T.Tarih   = S.Tarih
    WHEN MATCHED THEN
        UPDATE SET T.CalismaTipi = S.CalismaTipi,
                   T.Saat        = S.Saat,
                   T.KayitZamani = S.KayitZamani
    WHEN NOT MATCHED THEN
        INSERT (SicilNo, Ad, Soyad, Tarih, CalismaTipi, Saat, KayitZamani)
        VALUES (S.SicilNo, S.Ad, S.Soyad, S.Tarih, S.CalismaTipi, S.Saat, S.KayitZamani);

    /* --- 2) ANA sicilden toplam @N gün sil --- */
    DELETE F
    FROM dbo.FinalPuantajVerisi F
    WHERE F.SicilNo = @AnaPersonelId
      AND F.Tarih BETWEEN DATEADD(DAY, -(@N-1), @SonGun) AND @SonGun;

    /* --- 3) PuantajOnay: HEDEF(son gün) Onaylandı --- */
    MERGE dbo.PuantajOnay AS T
    USING (
        SELECT DISTINCT PersonelId = h.HedefSicilNo, Tarih = @SonGun
        FROM @Hedefler h
    ) AS S
      ON T.PersonelId = S.PersonelId AND T.Tarih = S.Tarih
    WHEN MATCHED THEN
        UPDATE SET T.OnayDurumu = 1,  -- Onaylandı
                   T.DuzenlenmisFMDakika = 0,
                   T.Aciklama = N'Çoklu Sicil Aktarım: NG 7,5',
                   T.GuncellemeZamani = GETDATE(),
                   T.GuncelleyenKullaniciId = @KullaniciId
    WHEN NOT MATCHED THEN
        INSERT (PersonelId, Tarih, OnayDurumu, DuzenlenmisFMDakika, Aciklama, OlusturmaZamani, OlusturanKullaniciId)
        VALUES (S.PersonelId, S.Tarih, 1, 0, N'Çoklu Sicil Aktarım: NG 7,5', GETDATE(), @KullaniciId);

    /* --- 4) PuantajOnay: ANA(son N günün TAMAMI) Düzeltildi --- */
    ;WITH Tally2 AS
    (
        SELECT TOP (@N)
               ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS d
        FROM sys.all_objects a CROSS JOIN sys.all_objects b
    ),
    S_Ana AS
    (
        SELECT @AnaPersonelId AS PersonelId,
               DATEADD(DAY, -d, @SonGun) AS Tarih
        FROM Tally2
    )
    MERGE dbo.PuantajOnay AS T
    USING S_Ana AS S
      ON T.PersonelId = S.PersonelId AND T.Tarih = S.Tarih
    WHEN MATCHED THEN
        UPDATE SET T.OnayDurumu = 3,   -- Düzeltildi
                   T.DuzenlenmisFMDakika = 0,
                   T.Aciklama = N'Çoklu Sicil Aktarım: Son ' + CAST(@N AS nvarchar(10)) + N' gün silindi',
                   T.GuncellemeZamani = GETDATE(),
                   T.GuncelleyenKullaniciId = @KullaniciId
    WHEN NOT MATCHED THEN
        INSERT (PersonelId, Tarih, OnayDurumu, DuzenlenmisFMDakika, Aciklama, OlusturmaZamani, OlusturanKullaniciId)
        VALUES (@AnaPersonelId, S.Tarih, 3, 0,
                N'Çoklu Sicil Aktarım: Son ' + CAST(@N AS nvarchar(10)) + N' gün silindi',
                GETDATE(), @KullaniciId);
END
GO
/****** Object:  StoredProcedure [dbo].[sp_DashboardAnaEkran]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_DashboardAnaEkran]
    @FirmaIdList   NVARCHAR(MAX),
    @IsyeriIdList  NVARCHAR(MAX) = NULL,
    @Gun           DATE,
    @AyBas         DATE,
    @AySon         DATE,
    @TolBasSaat    FLOAT = 0.25,   -- uyumluluk için duruyor
    @TolBitSaat    FLOAT = 0.25,   -- uyumluluk için duruyor
    @AnlikLimit    INT   = 30      -- uyumluluk için duruyor
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    /* -----------------------------
       Temp tabloları temizle
    ------------------------------*/
    DROP TABLE IF EXISTS #Firmalar;
    DROP TABLE IF EXISTS #Isyerleri;
    DROP TABLE IF EXISTS #AktifPersonel;
    DROP TABLE IF EXISTS #BugunHareketler;
    DROP TABLE IF EXISTS #SonHareketBugun;
    DROP TABLE IF EXISTS #TamGunIzinliler;
    DROP TABLE IF EXISTS #CalismaBeklenenler;
    DROP TABLE IF EXISTS #FirstIn;
    DROP TABLE IF EXISTS #Late;
    DROP TABLE IF EXISTS #GunIzinKapsam;

    /* -----------------------------
       Parametre kontrolleri
    ------------------------------*/
    IF (@Gun IS NULL OR @AyBas IS NULL OR @AySon IS NULL)
    BEGIN
        RAISERROR('Gun/AyBas/AySon boş olamaz.', 16, 1);
        RETURN;
    END

    DECLARE @GunBas DATETIME = CAST(@Gun AS DATETIME);
    DECLARE @GunBit DATETIME = DATEADD(SECOND, -1, DATEADD(DAY, 1, @GunBas));

    /* ============================================================
       1) FİRMA / İŞYERİ FİLTRELERİ
       ============================================================ */
    SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT) AS FirmaId
    INTO #Firmalar
    FROM STRING_SPLIT(ISNULL(@FirmaIdList,''), ',')
    WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

    CREATE TABLE #Isyerleri (IsyeriId INT PRIMARY KEY);

    IF (ISNULL(@IsyeriIdList,'') <> '')
    BEGIN
        INSERT INTO #Isyerleri(IsyeriId)
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@IsyeriIdList, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;
    END
    ELSE
    BEGIN
        INSERT INTO #Isyerleri(IsyeriId)
        SELECT DISTINCT i.IsyeriId
        FROM dbo.Isyerler i WITH (NOLOCK)
        WHERE EXISTS (SELECT 1 FROM #Firmalar f WHERE f.FirmaId = i.FirmaId);
    END

    /* ============================================================
       2) AKTİF PERSONEL (BUGÜN İTİBARIYLA)
       ============================================================ */
    SELECT
        k.PersonelId,
        k.Ad,
        k.Soyad,
        k.FirmaId,
        k.IsyeriId,
        k.CalismaSekli,
        k.IseGirisTarihi,
        k.IstenCikisTarihi,
        k.DogumTarihi
    INTO #AktifPersonel
    FROM dbo.Kisiler k WITH (NOLOCK)
    WHERE k.PuantajYapilirMi = 1
      AND EXISTS (SELECT 1 FROM #Firmalar f WHERE f.FirmaId = k.FirmaId)
      AND EXISTS (SELECT 1 FROM #Isyerleri i WHERE i.IsyeriId = k.IsyeriId)
      AND k.IseGirisTarihi <= @Gun
      AND (k.IstenCikisTarihi IS NULL OR k.IstenCikisTarihi >= @Gun);

    /* ============================================================
       3) BUGÜN HAREKETLERİ (TURNİKE, ARA KAT HARİÇ)
       ============================================================ */
    SELECT
        h.Id,
        h.PersonelId,
        h.Tarih,
        h.Tip
    INTO #BugunHareketler
    FROM dbo.KisiHareketler h WITH (NOLOCK)
    INNER JOIN dbo.Cihazlar c WITH (NOLOCK) ON c.CihazId = h.CihazId
    WHERE h.AktifMi = 1
      AND h.Tip IN (N'Giriş', N'Giris', N'Çıkış', N'Cikis')
      AND ISNULL(c.Notlar,N'') NOT LIKE N'%(Ara Kat)%'
      AND h.Tarih BETWEEN @GunBas AND @GunBit
      AND EXISTS (SELECT 1 FROM #AktifPersonel ap WHERE ap.PersonelId = h.PersonelId);

    /* ============================================================
       4) BUGÜN PERSONEL BAZINDA SON HAREKET (EŞİTLİK İÇİN BUGÜN İÇİNDEN)
       ============================================================ */
    ;WITH X AS
    (
        SELECT
            bh.PersonelId,
            bh.Tip,
            bh.Tarih,
            ROW_NUMBER() OVER (PARTITION BY bh.PersonelId ORDER BY bh.Tarih DESC, bh.Id DESC) AS rn
        FROM #BugunHareketler bh
    )
    SELECT PersonelId, Tip, Tarih
    INTO #SonHareketBugun
    FROM X
    WHERE rn = 1;

    /* ============================================================
       5) TAM GÜN İZİNLİLER (SAATLİK HARİÇ) - RAPORLA AYNI
       ============================================================ */
    SELECT DISTINCT ki.PersonelId
    INTO #TamGunIzinliler
    FROM dbo.KisiIzinler ki WITH (NOLOCK)
    WHERE ki.AktifMi = 1
      AND ki.SaatlikIzinMi = 0
      AND ki.Baslangic <= @GunBas
      AND ki.Bitis     >= DATEADD(DAY, 1, @GunBas)
      AND EXISTS (SELECT 1 FROM #AktifPersonel ap WHERE ap.PersonelId = ki.PersonelId);

    /* ============================================================
       6) RESMİ TATİL (TAM GÜN) KONTROLÜ
       ============================================================ */
    DECLARE @RT_CalismaSaati DECIMAL(5,2) =
    (
        SELECT TOP(1) rt.CalismaSaati
        FROM dbo.ResmiTatiller rt WITH (NOLOCK)
        WHERE rt.Tarih = @Gun
    );

    DECLARE @GunTamResmiTatil BIT =
        CASE WHEN @RT_CalismaSaati IS NULL THEN 0
             WHEN @RT_CalismaSaati >= 7.5 THEN 1
             ELSE 0 END;

    /* ============================================================
       7) ÇALIŞMASI BEKLENENLER (RESMİ TATİL TAM GÜNSE BOŞ)
       ============================================================ */
    SELECT ap.*
    INTO #CalismaBeklenenler
    FROM #AktifPersonel ap
    WHERE @GunTamResmiTatil = 0
      AND NOT EXISTS (SELECT 1 FROM #TamGunIzinliler ti WHERE ti.PersonelId = ap.PersonelId);

    /* ============================================================
       7.1) DEVAMSIZ RAPORUYLA UYUMLU "GÜN İÇİN İZİN KAPSAMI"
          (Saatlik değilse, gün aralığına denk geliyorsa devamsızdan düş)
       ============================================================ */
    SELECT DISTINCT
        ki.PersonelId
    INTO #GunIzinKapsam
    FROM dbo.KisiIzinler ki WITH (NOLOCK)
    WHERE ki.AktifMi = 1
      AND (ki.SaatlikIzinMi IS NULL OR ki.SaatlikIzinMi = 0)
      AND @Gun BETWEEN CONVERT(DATE, ki.Baslangic) AND CONVERT(DATE, ki.Bitis)
      AND EXISTS (SELECT 1 FROM #AktifPersonel ap WHERE ap.PersonelId = ki.PersonelId);

    /* ============================================================
       8) GEÇ KALANLAR (RAPORLA AYNI MANTIK) - LİSTE + COUNT
       ============================================================ */

    -- Bugünün ilk girişleri
    SELECT
        bh.PersonelId,
        CAST(bh.Tarih AS DATE) AS Gun,
        MIN(bh.Tarih) AS IlkGiris
    INTO #FirstIn
    FROM #BugunHareketler bh
    WHERE bh.Tip IN (N'Giriş', N'Giris')
    GROUP BY bh.PersonelId, CAST(bh.Tarih AS DATE);

    ;WITH KS AS
    (
        SELECT k.PersonelId,
               TRY_CAST(LTRIM(RTRIM(s.value)) AS INT) AS CalismaSekliId
        FROM #CalismaBeklenenler k
        CROSS APPLY STRING_SPLIT(ISNULL(k.CalismaSekli,''), ',') s
        WHERE ISNULL(k.CalismaSekli,'') <> ''
    ),
    S AS
    (
        SELECT
            cs.CalismaSekilId,
            CAST(cs.BaslangicZaman         AS TIME) AS BasTime,
            CAST(cs.BaslangicToleransZaman AS TIME) AS BasTolTime
        FROM dbo.CalismaSekilleri cs WITH (NOLOCK)
    )
    SELECT
        cb.PersonelId,
        cb.Ad,
        cb.Soyad,
        cb.FirmaId,
        cb.IsyeriId,
        DATEDIFF(MINUTE, Best.ToleransliDT, fi.IlkGiris) AS FazlaDakika
    INTO #Late
    FROM #CalismaBeklenenler cb
    INNER JOIN #FirstIn fi ON fi.PersonelId = cb.PersonelId AND fi.Gun = @Gun
    OUTER APPLY
    (
        SELECT TOP (1)
            BeklenenDT =
                DATEADD(SECOND, DATEDIFF(SECOND,'19000101',C.BasTime), CAST(@Gun AS DATETIME)),
            ToleransliDT =
                DATEADD(SECOND, DATEDIFF(SECOND,'19000101',COALESCE(C.BasTolTime, C.BasTime)),
                        CAST(@Gun AS DATETIME)),
            Score =
                ABS(DATEDIFF(MINUTE,
                    DATEADD(SECOND, DATEDIFF(SECOND,'19000101',C.BasTime), CAST(@Gun AS DATETIME)),
                    fi.IlkGiris))
        FROM
        (
            SELECT s.BasTime, s.BasTolTime
            FROM KS
            INNER JOIN S ON S.CalismaSekilId = KS.CalismaSekliId
            WHERE KS.PersonelId = cb.PersonelId

            UNION ALL
            SELECT CAST('08:00:00' AS TIME), CAST(NULL AS TIME)
            WHERE NOT EXISTS (SELECT 1 FROM KS WHERE PersonelId = cb.PersonelId)
        ) C
        ORDER BY Score ASC
    ) Best
    WHERE fi.IlkGiris > Best.ToleransliDT;

    /* ============================================================
       RESULT SET 1: EN GEÇ GELEN PERSONELLER (SINIR YOK)
       ============================================================ */
    SELECT
        PersonelId,
        Ad,
        Soyad,
        FirmaId,
        IsyeriId,
        FazlaDakika
    FROM #Late
    ORDER BY FazlaDakika DESC, Soyad, Ad;

    /* ============================================================
       RESULT SET 2: BU AY DOĞUM GÜNÜ OLANLAR (AKTİF PERSONEL)
       ============================================================ */
    DECLARE @Yil INT = YEAR(@Gun);

    SELECT
        ap.PersonelId,
        ap.Ad,
        ap.Soyad,
        ap.FirmaId,
        ap.IsyeriId,
        CASE
            WHEN MONTH(ap.DogumTarihi) = 2 AND DAY(ap.DogumTarihi) = 29
                 AND NOT (((@Yil % 4 = 0) AND (@Yil % 100 <> 0)) OR (@Yil % 400 = 0))
            THEN DATEFROMPARTS(@Yil, 2, 28)
            ELSE DATEFROMPARTS(@Yil, MONTH(ap.DogumTarihi), DAY(ap.DogumTarihi))
        END AS BuYilDogumGunu,
        DAY(ap.DogumTarihi)   AS Gun,
        MONTH(ap.DogumTarihi) AS Ay,
        DATEDIFF(YEAR, ap.DogumTarihi, @Gun) AS Yas
    FROM #AktifPersonel ap
    WHERE ap.DogumTarihi IS NOT NULL
      AND MONTH(ap.DogumTarihi) = MONTH(@Gun)
    ORDER BY DAY(ap.DogumTarihi), ap.Soyad, ap.Ad;

    /* ============================================================
       RESULT SET 3: BU AY İŞE BAŞLAYANLAR (AYNI AY İÇİNDE ÇIKANLAR HARİÇ)
       ============================================================ */
    SELECT
        k.PersonelId,
        k.Ad,
        k.Soyad,
        k.FirmaId,
        k.IsyeriId,
        k.IseGirisTarihi AS BaslamaTarihi
    FROM dbo.Kisiler k WITH (NOLOCK)
    WHERE k.PuantajYapilirMi = 1
      AND EXISTS (SELECT 1 FROM #Firmalar f WHERE f.FirmaId = k.FirmaId)
      AND EXISTS (SELECT 1 FROM #Isyerleri i WHERE i.IsyeriId = k.IsyeriId)
      AND CONVERT(DATE, k.IseGirisTarihi) BETWEEN @AyBas AND @AySon
      AND (k.IstenCikisTarihi IS NULL OR k.IstenCikisTarihi > @AySon)
    ORDER BY k.IseGirisTarihi DESC, k.Soyad, k.Ad;

    /* ============================================================
       RESULT SET 4: BU AY İŞTEN AYRILANLAR
       ============================================================ */
    SELECT
        k.PersonelId,
        k.Ad,
        k.Soyad,
        k.FirmaId,
        k.IsyeriId,
        k.IstenCikisTarihi AS AyrilmaTarihi
    FROM dbo.Kisiler k WITH (NOLOCK)
    WHERE k.PuantajYapilirMi = 1
      AND EXISTS (SELECT 1 FROM #Firmalar f WHERE f.FirmaId = k.FirmaId)
      AND EXISTS (SELECT 1 FROM #Isyerleri i WHERE i.IsyeriId = k.IsyeriId)
      AND k.IstenCikisTarihi IS NOT NULL
      AND CONVERT(DATE, k.IstenCikisTarihi) BETWEEN @AyBas AND @AySon
    ORDER BY k.IstenCikisTarihi DESC, k.Soyad, k.Ad;

    /* ============================================================
       RESULT SET 5: DASHBOARD KARTLARI (TUTARLI)
       ============================================================ */
    DECLARE @HareketiBulunanlar INT =
        (SELECT COUNT(DISTINCT PersonelId) FROM #BugunHareketler);

    DECLARE @Iceridekiler INT =
        (SELECT COUNT(*) FROM #SonHareketBugun WHERE Tip IN (N'Giriş', N'Giris'));

    DECLARE @Disaridakiler INT =
        (SELECT COUNT(*) FROM #SonHareketBugun WHERE Tip IN (N'Çıkış', N'Cikis'));

    -- Devamsız: çalışması beklenen + bugün hiç hareketi yok + (gün izin kapsamı yok)
    DECLARE @Devamsizlar INT =
        (SELECT COUNT(*)
         FROM #CalismaBeklenenler cb
         WHERE NOT EXISTS (SELECT 1 FROM #BugunHareketler bh WHERE bh.PersonelId = cb.PersonelId)
           AND NOT EXISTS (SELECT 1 FROM #GunIzinKapsam iz WHERE iz.PersonelId = cb.PersonelId));

    DECLARE @Izinli INT = (SELECT COUNT(DISTINCT PersonelId) FROM #TamGunIzinliler);

    DECLARE @GecKalanlar INT = (SELECT COUNT(DISTINCT PersonelId) FROM #Late);

    DECLARE @IseBaslayan INT =
    (
        SELECT COUNT(*)
        FROM dbo.Kisiler k WITH (NOLOCK)
        WHERE k.PuantajYapilirMi = 1
          AND EXISTS (SELECT 1 FROM #Firmalar f WHERE f.FirmaId = k.FirmaId)
          AND EXISTS (SELECT 1 FROM #Isyerleri i WHERE i.IsyeriId = k.IsyeriId)
          AND CONVERT(DATE, k.IseGirisTarihi) BETWEEN @AyBas AND @AySon
          AND (k.IstenCikisTarihi IS NULL OR k.IstenCikisTarihi > @AySon)
    );

    DECLARE @IstenAyrilan INT =
    (
        SELECT COUNT(*)
        FROM dbo.Kisiler k WITH (NOLOCK)
        WHERE k.PuantajYapilirMi = 1
          AND EXISTS (SELECT 1 FROM #Firmalar f WHERE f.FirmaId = k.FirmaId)
          AND EXISTS (SELECT 1 FROM #Isyerleri i WHERE i.IsyeriId = k.IsyeriId)
          AND k.IstenCikisTarihi IS NOT NULL
          AND CONVERT(DATE, k.IstenCikisTarihi) BETWEEN @AyBas AND @AySon
    );

    SELECT
        GirisYapan   = @HareketiBulunanlar,   -- UI’de “Hareketi Bulunanlar" olarak bunu değiştirdim
        Iceridekiler = @Iceridekiler,
        GecKalanlar  = @GecKalanlar,
        Disaridakiler= @Disaridakiler,
        Devamsizlar  = @Devamsizlar,
        Izinli       = @Izinli,
        IseBaslayan  = @IseBaslayan,
        IstenAyrilan = @IstenAyrilan;

END
GO
/****** Object:  StoredProcedure [dbo].[sp_DevamsizlarRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_DevamsizlarRaporu]
    @FirmaIdList nvarchar(max),
    @IsyeriIdList nvarchar(max),
    @TarihBaslangic datetime,
    @TarihBitis datetime,
    @ResmiTatilDahil bit = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET DATEFIRST 1;

    DECLARE @Bas date = CONVERT(date, @TarihBaslangic);
    DECLARE @Bit date = CONVERT(date, @TarihBitis);

    IF @Bas IS NULL OR @Bit IS NULL OR @Bas > @Bit
        THROW 51000, N'Geçersiz tarih aralığı.', 1;

    DECLARE @F TABLE (Id int PRIMARY KEY);
    IF (LEN(ISNULL(@FirmaIdList,N''))>0)
        INSERT @F(Id)
        SELECT DISTINCT TRY_CONVERT(int, LTRIM(RTRIM(value)))
        FROM STRING_SPLIT(@FirmaIdList, ',')
        WHERE TRY_CONVERT(int, LTRIM(RTRIM(value))) IS NOT NULL;

    DECLARE @I TABLE (Id int PRIMARY KEY);
    IF (LEN(ISNULL(@IsyeriIdList,N''))>0)
        INSERT @I(Id)
        SELECT DISTINCT TRY_CONVERT(int, LTRIM(RTRIM(value)))
        FROM STRING_SPLIT(@IsyeriIdList, ',')
        WHERE TRY_CONVERT(int, LTRIM(RTRIM(value))) IS NOT NULL;

    ;WITH Takvim AS (
        SELECT @Bas AS Tarih
        UNION ALL
        SELECT DATEADD(DAY,1,Tarih)
        FROM Takvim
        WHERE DATEADD(DAY,1,Tarih) <= @Bit
    )
    , Kapsam AS (
        SELECT 
            k.PersonelId,
            k.TcKimlikNo,
            k.Ad,
            k.Soyad,
            k.FirmaId,
            k.IsyeriId,
            k.BolumId,
            k.IseGirisTarihi,
            k.IstenCikisTarihi
        FROM dbo.Kisiler k
        WHERE k.PuantajYapilirMi=1
          AND k.IseGirisTarihi <= @Bit
          AND (k.IstenCikisTarihi IS NULL OR k.IstenCikisTarihi >= @Bas)
          AND (NOT EXISTS(SELECT 1 FROM @F) OR k.FirmaId IN (SELECT Id FROM @F))
          AND (NOT EXISTS(SELECT 1 FROM @I) OR k.IsyeriId IN (SELECT Id FROM @I))
    )
    , KG AS (
        SELECT 
            k.PersonelId,
            k.TcKimlikNo,
            k.Ad,
            k.Soyad,
            k.FirmaId,
            k.IsyeriId,
            k.BolumId,
            t.Tarih,
            DATEPART(WEEKDAY,t.Tarih) AS Wd
        FROM Kapsam k
        CROSS JOIN Takvim t
    )
    , H AS (
        SELECT 
            kh.PersonelId,
            CONVERT(date,kh.Tarih) AS Gun,
            COUNT(*) AS Cnt
        FROM dbo.KisiHareketler kh WITH (NOLOCK)
        INNER JOIN dbo.Cihazlar c ON c.CihazId = kh.CihazId
        WHERE kh.AktifMi=1
          AND kh.Tip IN (N'Giriş', N'Çıkış')                           -- turnike hareketi
          AND ISNULL(c.Notlar,N'') NOT LIKE N'%(Ara Kat)%'             -- Ara Kat hariç
          -- AND ISNULL(c.Notlar,N'') NOT LIKE N'%Yemekhane%'          -- (opsiyon) yemekhane hariç
          AND kh.Tarih >= @Bas
          AND kh.Tarih < DATEADD(DAY,1,@Bit)
        GROUP BY kh.PersonelId, CONVERT(date,kh.Tarih)
    )
    , RT AS (
        SELECT rt.Tarih
        FROM dbo.ResmiTatiller rt
        WHERE rt.Tarih BETWEEN @Bas AND @Bit
    )
    , IZ AS (
        SELECT DISTINCT
            ki.PersonelId,
            t.Tarih
        FROM dbo.KisiIzinler ki
        INNER JOIN Takvim t 
            ON t.Tarih >= CONVERT(date, ki.Baslangic)
            AND t.Tarih <= CONVERT(date, ki.Bitis)
        WHERE ki.AktifMi = 1
          AND (ki.SaatlikIzinMi IS NULL OR ki.SaatlikIzinMi = 0)
    )
    SELECT 
        kg.PersonelId AS [Sicil No],
        kg.Ad,
        kg.Soyad,
        f.FirmaAdi AS [Firma],
        i.IsyeriAdi AS [İşyeri],
        FORMAT(kg.Tarih, N'd MMMM yyyy dddd', 'tr-TR') AS [Gelmediği Tarih]
    FROM KG kg
    LEFT JOIN H  ON H.PersonelId = kg.PersonelId AND H.Gun = kg.Tarih
    LEFT JOIN RT ON RT.Tarih = kg.Tarih
    LEFT JOIN IZ ON IZ.PersonelId = kg.PersonelId AND IZ.Tarih = kg.Tarih
    LEFT JOIN dbo.Firmalar f ON f.FirmaId = kg.FirmaId
    LEFT JOIN dbo.Isyerler i ON i.IsyeriId = kg.IsyeriId AND i.FirmaId = kg.FirmaId
    WHERE H.PersonelId IS NULL
      AND (@ResmiTatilDahil = 1 OR RT.Tarih IS NULL)
      AND IZ.PersonelId IS NULL
      AND kg.Wd <> 7   -- şimdilik pazar hariç
    ORDER BY kg.PersonelId, kg.Tarih
    OPTION (MAXRECURSION 0);
END
GO
/****** Object:  StoredProcedure [dbo].[sp_FazlaMesai]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_FazlaMesai]
  @FirmaIdList       nvarchar(max) = NULL,   -- "101" veya "101,102"
  @IsyeriIdList      nvarchar(max) = NULL,   -- "13,14"
  @TarihBaslangic    date,
  @TarihBitis        date,
  @MolaDakika        int  = 60,              -- sabit mola (dk)
  @CikisAramaBuffer  int  = 120              -- vardiya bitişinden sonra çıkış arama penceresi (dk)
AS
BEGIN
  SET NOCOUNT ON;

  ;WITH F AS (
    SELECT TRY_CAST(LTRIM(RTRIM(value)) AS int) AS FirmaId
    FROM STRING_SPLIT(ISNULL(@FirmaIdList,''), ',')
    WHERE ISNULL(@FirmaIdList,'') <> ''
  ),
  IY AS (
    SELECT TRY_CAST(LTRIM(RTRIM(value)) AS int) AS IsyeriId
    FROM STRING_SPLIT(ISNULL(@IsyeriIdList,''), ',')
    WHERE ISNULL(@IsyeriIdList,'') <> ''
  ),
  K AS (
    SELECT k.PersonelId, k.Ad, k.Soyad, k.FirmaId, k.IsyeriId, k.CalismaSekli
    FROM dbo.Kisiler k
    WHERE (NOT EXISTS (SELECT 1 FROM F)  OR k.FirmaId  IN (SELECT FirmaId  FROM F))
      AND (NOT EXISTS (SELECT 1 FROM IY) OR k.IsyeriId IN (SELECT IsyeriId FROM IY))
      AND (k.IstenCikisTarihi IS NULL OR k.IstenCikisTarihi >= @TarihBaslangic)
      AND (k.IseGirisTarihi  IS NULL OR k.IseGirisTarihi  <= @TarihBitis)
  ),
  H AS (
    SELECT
      h.PersonelId,
      CAST(h.Tarih AS date) AS Gun,
      h.Tip,
      h.KayitZamani
    FROM dbo.KisiHareketler h
    INNER JOIN K ON K.PersonelId = h.PersonelId
    WHERE h.AktifMi = 1
      AND h.Tip <> N'Yemekhane'
      AND CAST(h.Tarih AS date) BETWEEN @TarihBaslangic AND @TarihBitis
  ),
  G AS (
    SELECT
      PersonelId,
      Gun,
      COALESCE(MIN(CASE WHEN Tip = N'Giriş'  THEN KayitZamani END), MIN(KayitZamani)) AS IlkGiris,
      COALESCE(MAX(CASE WHEN Tip = N'Çıkış' THEN KayitZamani END), MAX(KayitZamani)) AS SonCikis
    FROM H
    GROUP BY PersonelId, Gun
  ),
  KS AS (
    SELECT
      k.PersonelId,
      TRY_CAST(LTRIM(RTRIM(s.value)) AS int) AS CalismaSekliId
    FROM K
    CROSS APPLY STRING_SPLIT(ISNULL(k.CalismaSekli,''), ',') s
    WHERE ISNULL(k.CalismaSekli,'') <> ''
  ),
  S AS (
    SELECT
      cs.CalismaSekilId,
      cs.CalismaSekliAdi,
      cs.BaslangicZaman,
      cs.BitisZaman,
      cs.BaslangicToleransZaman,
      cs.BitisToleransZaman,
      cs.YemekAktiflestirmeZaman
    FROM dbo.CalismaSekilleri cs
  )

  SELECT
    k.FirmaId AS [Firma Id],
    k.IsyeriId AS [İşyeri Id],
    g.PersonelId AS [Sicil No],
    k.Ad + N' ' + k.Soyad AS [Adı Soyadı],
    g.Gun AS Tarih,
    g.IlkGiris AS [İlk Giriş],
    E.EfektifSonCikis AS [Son Çıkış],
    Best.CalismaSekliAdi AS [Vardiya],
    Best.VBaslangic    AS [Vardiya Başlangıcı],
    Best.VGercekBitis  AS [Vardiya Bitişi],
    @MolaDakika        AS [Mola(Dakika)],
    CA.CalismaDakika AS [Toplam Çalışma(Dakika)],
    CA.VardiyaDakika AS [Beklenen Çalışma(Dakika)],
    CA.FazlaMesaiDakika AS [Fazla Mesai(Dakika)],
    CA.EksikDakika AS [Eksik Mesai(Dakika)]
  FROM G g
  INNER JOIN K ON K.PersonelId = g.PersonelId

  OUTER APPLY (
      SELECT TOP (1)
        C.CalismaSekilId,
        C.CalismaSekliAdi,
        DATEADD(second,
                DATEDIFF(second,'19000101',C.BasTime),
                CAST(g.Gun AS datetime)) AS VBaslangic,
        CASE
          WHEN C.BitTime < C.BasTime THEN
               DATEADD(day, 1, DATEADD(second,
                      DATEDIFF(second,'19000101',C.BitTime),
                      CAST(g.Gun AS datetime)))
          ELSE DATEADD(second,
                      DATEDIFF(second,'19000101',C.BitTime),
                      CAST(g.Gun AS datetime))
        END AS VGercekBitis,
        CASE
          WHEN g.IlkGiris IS NULL THEN 999999
          ELSE ABS(DATEDIFF(minute,
                    DATEADD(second,
                           DATEDIFF(second,'19000101',C.BasTime),
                           CAST(g.Gun AS datetime)),
                    g.IlkGiris))
        END AS Score
      FROM (
          SELECT s.CalismaSekilId,
                 s.CalismaSekliAdi,
                 CAST(s.BaslangicZaman AS time) AS BasTime,
                 CAST(s.BitisZaman     AS time) AS BitTime
          FROM KS
          INNER JOIN S ON S.CalismaSekilId = KS.CalismaSekliId
          WHERE KS.PersonelId = g.PersonelId

          UNION ALL
          SELECT -1, N'Varsayılan (08:00-17:00)',
                 CAST('08:00:00' AS time),
                 CAST('17:00:00' AS time)
          WHERE NOT EXISTS (SELECT 1 FROM KS WHERE PersonelId = g.PersonelId)
      ) C
      ORDER BY Score ASC, C.CalismaSekilId
  ) AS Best

  OUTER APPLY (
      SELECT TOP (1)
          h2.KayitZamani AS SonCikisAday
      FROM dbo.KisiHareketler h2
      WHERE h2.PersonelId = g.PersonelId
        AND h2.AktifMi = 1
        AND h2.Tip = N'Çıkış'
        AND g.IlkGiris IS NOT NULL
        AND h2.KayitZamani > g.IlkGiris
        AND h2.KayitZamani <= DATEADD(minute, @CikisAramaBuffer, Best.VGercekBitis)
      ORDER BY h2.KayitZamani DESC
  ) NextExit

  CROSS APPLY (
      SELECT
        EfektifSonCikis =
            CASE
              WHEN g.SonCikis IS NOT NULL THEN g.SonCikis
              WHEN NextExit.SonCikisAday IS NOT NULL THEN NextExit.SonCikisAday
              ELSE NULL
            END
  ) E

  CROSS APPLY (
      SELECT
        CalismaRaw =
            CASE WHEN g.IlkGiris IS NOT NULL AND E.EfektifSonCikis IS NOT NULL
                 THEN DATEDIFF(minute, g.IlkGiris, E.EfektifSonCikis) - @MolaDakika
                 ELSE 0 END,
        VardiyaRaw =
            DATEDIFF(minute, Best.VBaslangic, Best.VGercekBitis) - @MolaDakika
  ) R
  CROSS APPLY (
      SELECT
        CalismaDakika    = CASE WHEN R.CalismaRaw > 0 THEN R.CalismaRaw ELSE 0 END,
        VardiyaDakika    = CASE WHEN R.VardiyaRaw > 0 THEN R.VardiyaRaw ELSE 0 END,
        FazlaMesaiDakika = CASE WHEN (R.CalismaRaw - R.VardiyaRaw) > 0
                                 THEN (R.CalismaRaw - R.VardiyaRaw) ELSE 0 END,
        EksikDakika      = CASE WHEN (R.VardiyaRaw - R.CalismaRaw) > 0
                                 THEN (R.VardiyaRaw - R.CalismaRaw) ELSE 0 END
  ) CA

  ORDER BY k.FirmaId, k.IsyeriId, [Adı Soyadı], g.Gun;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GunlukGecKalanlarRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_GunlukGecKalanlarRaporu]
    @FirmaIdList     NVARCHAR(MAX),
    @IsyeriIdList    NVARCHAR(MAX),
    @TarihBaslangic  DATE,
    @TarihBitis      DATE
AS
BEGIN
  SET NOCOUNT ON;

  IF (@TarihBaslangic IS NULL OR @TarihBitis IS NULL OR @TarihBaslangic > @TarihBitis)
  BEGIN
      RAISERROR('Tarih aralığı hatalı.', 16, 1);
      RETURN;
  END

  DECLARE @BasDT DATETIME = CAST(@TarihBaslangic AS DATETIME);
  DECLARE @BitDT DATETIME = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(@TarihBitis AS DATETIME)));

  -- CSV -> tablo (boşsa filtre uygulanmayacak)
  DECLARE @Firma TABLE (FirmaId INT PRIMARY KEY);
  IF (ISNULL(@FirmaIdList,'') <> '')
      INSERT INTO @Firma(FirmaId)
      SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
      FROM STRING_SPLIT(@FirmaIdList, ',')
      WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

  DECLARE @Isyeri TABLE (IsyeriId INT PRIMARY KEY);
  IF (ISNULL(@IsyeriIdList,'') <> '')
      INSERT INTO @Isyeri(IsyeriId)
      SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
      FROM STRING_SPLIT(@IsyeriIdList, ',')
      WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

  ;WITH Base AS (
      SELECT
        KH.Id          AS HareketId,
        KH.PersonelId,
        CAST(KH.Tarih AS DATE) AS Gun,
        KH.Tarih       AS Zaman,
        C.CihazAdi,
        C.Notlar
      FROM dbo.KisiHareketler KH
      INNER JOIN dbo.Kisiler  K  ON KH.PersonelId = K.PersonelId
      INNER JOIN dbo.Cihazlar C  ON KH.CihazId    = C.CihazId
      WHERE KH.AktifMi = 1
        AND KH.Tip = N'Giriş'
        AND KH.Tarih BETWEEN @BasDT AND @BitDT
        AND ISNULL(C.Notlar,N'') NOT LIKE N'%(Ara Kat)%'
        AND K.PuantajYapilirMi = 1
        -- kişi o gün çalışıyor olmalı
        AND K.IseGirisTarihi <= CAST(KH.Tarih AS DATE)
        AND (K.IstenCikisTarihi IS NULL OR K.IstenCikisTarihi >= CAST(KH.Tarih AS DATE))
        -- firma/işyeri filtre (boşsa pas)
        AND (ISNULL(@FirmaIdList,'')  = '' OR K.FirmaId  IN (SELECT FirmaId  FROM @Firma))
        AND (ISNULL(@IsyeriIdList,'') = '' OR K.IsyeriId IN (SELECT IsyeriId FROM @Isyeri))
  ),
  FirstIn AS (
      SELECT PersonelId, Gun, MIN(Zaman) AS IlkGiris
      FROM Base
      GROUP BY PersonelId, Gun
  ),
  KS AS (
      SELECT k.PersonelId,
             TRY_CAST(LTRIM(RTRIM(s.value)) AS int) AS CalismaSekliId
      FROM dbo.Kisiler k
      CROSS APPLY STRING_SPLIT(ISNULL(k.CalismaSekli,''), ',') s
      WHERE ISNULL(k.CalismaSekli,'') <> ''
  ),
  S AS (
      SELECT cs.CalismaSekilId,
             CAST(cs.BaslangicZaman         AS time) AS BasTime,
             CAST(cs.BaslangicToleransZaman AS time) AS BasTolTime
      FROM dbo.CalismaSekilleri cs
  )
  SELECT
      K.PersonelId                                     AS [Sicil No],
      K.Ad + N' ' + K.Soyad                            AS [Adı Soyadı],
      IY.IsyeriAdi                                     AS [İşyeri],
      FORMAT(FI.Gun, 'dd.MM.yyyy dddd', 'tr-TR')       AS [Tarih],
      FORMAT(Best.BeklenenDT,   'HH:mm')               AS [Beklenen Giriş Saati],
      FORMAT(Best.ToleransliDT, 'HH:mm')               AS [Toleranslı Giriş Saati],
      FORMAT(FI.IlkGiris,       'HH:mm')               AS [Gerçek Giriş Saati],
      DATEDIFF(MINUTE, Best.ToleransliDT, FI.IlkGiris) AS [Geç Kalma Süresi(Dakika)]
  FROM FirstIn FI
  INNER JOIN dbo.Kisiler K ON K.PersonelId = FI.PersonelId
  LEFT JOIN dbo.Isyerler IY ON IY.IsyeriId = K.IsyeriId AND IY.FirmaId = K.FirmaId
  OUTER APPLY (
      SELECT TOP (1)
          BeklenenDT =
            DATEADD(second, DATEDIFF(second,'19000101',C.BasTime), CAST(FI.Gun AS datetime)),
          ToleransliDT =
            DATEADD(second, DATEDIFF(second,'19000101',COALESCE(C.BasTolTime, C.BasTime)),
                    CAST(FI.Gun AS datetime)),
          Score = ABS(DATEDIFF(minute,
                    DATEADD(second, DATEDIFF(second,'19000101',C.BasTime), CAST(FI.Gun AS datetime)),
                    FI.IlkGiris))
      FROM (
          SELECT s.BasTime, s.BasTolTime
          FROM KS
          INNER JOIN S ON S.CalismaSekilId = KS.CalismaSekliId
          WHERE KS.PersonelId = FI.PersonelId

          UNION ALL
          SELECT CAST('08:00:00' AS time), CAST(NULL AS time)
          WHERE NOT EXISTS (SELECT 1 FROM KS WHERE PersonelId = FI.PersonelId)
      ) C
      ORDER BY Score ASC
  ) Best
  WHERE FI.IlkGiris > Best.ToleransliDT
  ORDER BY FI.Gun DESC, [Geç Kalma Süresi(Dakika)] DESC, FI.IlkGiris;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GunlukHareketiBulunanlarRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GunlukHareketiBulunanlarRaporu]
(
    @FirmaIdList     NVARCHAR(MAX),
    @IsyeriIdList    NVARCHAR(MAX),
    @TarihBaslangic  DATE,
    @TarihBitis      DATE
)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@TarihBaslangic IS NULL OR @TarihBitis IS NULL OR @TarihBaslangic > @TarihBitis)
    BEGIN
        RAISERROR('Tarih aralığı hatalı.', 16, 1);
        RETURN;
    END

    DECLARE @BasDT DATETIME = CAST(@TarihBaslangic AS DATETIME);
    DECLARE @BitDT DATETIME = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(@TarihBitis AS DATETIME)));

    DECLARE @Firma TABLE (FirmaId INT PRIMARY KEY);
    IF (ISNULL(@FirmaIdList,'') <> '')
        INSERT INTO @Firma(FirmaId)
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@FirmaIdList, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

    DECLARE @Isyeri TABLE (IsyeriId INT PRIMARY KEY);
    IF (ISNULL(@IsyeriIdList,'') <> '')
        INSERT INTO @Isyeri(IsyeriId)
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@IsyeriIdList, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

    ;WITH Base AS
    (
        SELECT
            KH.Id AS HareketId,
            KH.PersonelId,
            CAST(KH.Tarih AS DATE) AS Gun,
            KH.Tarih AS Zaman,
            KH.Tip,
            C.CihazAdi
        FROM dbo.KisiHareketler KH
        INNER JOIN dbo.Kisiler K  ON K.PersonelId = KH.PersonelId
        INNER JOIN dbo.Cihazlar C ON C.CihazId    = KH.CihazId
        WHERE
            KH.AktifMi = 1
            AND KH.Tip IN (N'Giriş', N'Çıkış')
            AND KH.Tarih BETWEEN @BasDT AND @BitDT
            AND ISNULL(C.Notlar,N'') NOT LIKE N'%(Ara Kat)%'
            AND K.PuantajYapilirMi = 1
            AND (ISNULL(@FirmaIdList,'')  = '' OR K.FirmaId  IN (SELECT FirmaId  FROM @Firma))
            AND (ISNULL(@IsyeriIdList,'') = '' OR K.IsyeriId IN (SELECT IsyeriId FROM @Isyeri))
            AND K.IseGirisTarihi <= CAST(KH.Tarih AS DATE)
            AND (K.IstenCikisTarihi IS NULL OR K.IstenCikisTarihi >= CAST(KH.Tarih AS DATE))
    ),
    FirstMove AS
    (
        SELECT *
        FROM
        (
            SELECT
                b.*,
                ROW_NUMBER() OVER
                (
                    PARTITION BY b.PersonelId, b.Gun
                    ORDER BY b.Zaman ASC, b.HareketId ASC
                ) AS rn
            FROM Base b
        ) x
        WHERE x.rn = 1
    )
    SELECT
        K.PersonelId                                            AS [Sicil No],
        K.Ad + N' ' + K.Soyad                                   AS [Adı Soyadı],
        IY.IsyeriAdi                                            AS [İşyeri],
        FORMAT(FM.Gun, 'dd.MM.yyyy dddd', 'tr-TR')              AS [Tarih],
        FORMAT(FM.Zaman, 'HH:mm')                               AS [İlk Hareket Saati],
        FM.Tip                                                  AS [İlk Hareket Tipi],
        FM.CihazAdi                                             AS [İlk Hareket Turnikesi]
    FROM FirstMove FM
    INNER JOIN dbo.Kisiler K ON K.PersonelId = FM.PersonelId
    LEFT JOIN dbo.Isyerler IY ON IY.IsyeriId = K.IsyeriId AND IY.FirmaId = K.FirmaId
    ORDER BY FM.Gun DESC, FM.Zaman ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GunlukIzinlilerRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GunlukIzinlilerRaporu]
(
    @FirmaIdList      NVARCHAR(MAX),
    @IsyeriIdList     NVARCHAR(MAX),
    @TarihBaslangic   DATE,
    @TarihBitis       DATE
)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@TarihBaslangic IS NULL OR @TarihBitis IS NULL OR @TarihBaslangic > @TarihBitis)
    BEGIN
        RAISERROR('Tarih aralığı hatalı.', 16, 1);
        RETURN;
    END

    DECLARE @Firma TABLE (FirmaId INT PRIMARY KEY);
    IF (ISNULL(@FirmaIdList,'') <> '')
    INSERT INTO @Firma(FirmaId)
    SELECT DISTINCT TRY_CAST(value AS INT)
    FROM STRING_SPLIT(@FirmaIdList, ',')
    WHERE TRY_CAST(value AS INT) IS NOT NULL;

    DECLARE @Isyeri TABLE (IsyeriId INT PRIMARY KEY);
    IF (ISNULL(@IsyeriIdList,'') <> '')
    INSERT INTO @Isyeri(IsyeriId)
    SELECT DISTINCT TRY_CAST(value AS INT)
    FROM STRING_SPLIT(@IsyeriIdList, ',')
    WHERE TRY_CAST(value AS INT) IS NOT NULL;

    ;WITH Gunler AS
    (
        SELECT @TarihBaslangic AS Tarih
        UNION ALL
        SELECT DATEADD(DAY, 1, Tarih)
        FROM Gunler
        WHERE Tarih < @TarihBitis
    )
    SELECT
        g.Tarih,
        f.FirmaAdi        AS [Firma],
        iy.IsyeriAdi      AS [İşyeri],
        k.PersonelId     AS [Sicil No],
        k.Ad + ' ' + k.Soyad AS [Adı Soyadı],
        it.Adi            AS [İzin Tipi],
        ki.Baslangic      AS [İzin Başlangıcı],
        ki.Bitis          AS [İzin Bitişi]
    FROM Gunler g
    INNER JOIN dbo.KisiIzinler ki
        ON ki.AktifMi = 1
        AND ki.SaatlikIzinMi = 0
        AND ki.Baslangic <= CONVERT(DATETIME, g.Tarih)
        AND ki.Bitis     >= DATEADD(DAY, 1, CONVERT(DATETIME, g.Tarih))
    INNER JOIN dbo.Kisiler k
        ON k.PersonelId = ki.PersonelId
        AND k.IseGirisTarihi <= g.Tarih
        AND (k.IstenCikisTarihi IS NULL OR k.IstenCikisTarihi >= g.Tarih)
    INNER JOIN dbo.Firmalar f
        ON f.FirmaId = k.FirmaId
    INNER JOIN dbo.Isyerler iy
        ON iy.IsyeriId = k.IsyeriId
    LEFT JOIN dbo.IzinTipleri it
        ON it.IzinTipId = ki.IzinId
        AND it.AktifMi = 1
    WHERE
        (ISNULL(@FirmaIdList,'') = '' OR k.FirmaId IN (SELECT FirmaId FROM @Firma))
        AND (ISNULL(@IsyeriIdList,'') = '' OR k.IsyeriId IN (SELECT IsyeriId FROM @Isyeri))
    ORDER BY g.Tarih, f.FirmaAdi, iy.IsyeriAdi, k.Ad, k.Soyad
    OPTION (MAXRECURSION 4000);
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GunlukYemekSayisi]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_GunlukYemekSayisi]
(
    @TarihBaslangic  DATE,
    @TarihBitis      DATE,
    @FirmaIdList     NVARCHAR(MAX) = NULL,   -- '101,102'  (opsiyonel)
    @IsyeriIdList    NVARCHAR(MAX) = NULL    -- '13,14'    (opsiyonel)
)
AS
BEGIN
    SET NOCOUNT ON;

    /* Liste parametrelerini tabloya dök */
    DECLARE @FirmaIds TABLE (FirmaId INT PRIMARY KEY);
    IF @FirmaIdList IS NOT NULL
        INSERT @FirmaIds(FirmaId)
        SELECT TRY_CAST(value AS INT)
        FROM STRING_SPLIT(@FirmaIdList, ',')
        WHERE TRY_CAST(value AS INT) IS NOT NULL;

    DECLARE @IsyeriIds TABLE (IsyeriId INT PRIMARY KEY);
    IF @IsyeriIdList IS NOT NULL
        INSERT @IsyeriIds(IsyeriId)
        SELECT TRY_CAST(value AS INT)
        FROM STRING_SPLIT(@IsyeriIdList, ',')
        WHERE TRY_CAST(value AS INT) IS NOT NULL;

    ;WITH Ham AS
    (
        SELECT
            CAST(y.Tarih AS DATE) AS Gun,
            y.PersonelId,
            k.FirmaId,
            k.IsyeriId,
            iy.IsyeriAdi
        FROM dbo.YemekhaneGecisHareketler y
        INNER JOIN dbo.Kisiler k   ON k.PersonelId = y.PersonelId
        INNER JOIN dbo.Cihazlar c  ON c.CihazId    = y.CihazId
        LEFT JOIN dbo.Isyerler iy  ON iy.IsyeriId  = k.IsyeriId AND iy.FirmaId = k.FirmaId
        WHERE y.Tarih >= @TarihBaslangic
          AND y.Tarih <  DATEADD(DAY, 1, @TarihBitis)
          AND c.CihazAdi COLLATE Turkish_CI_AI LIKE N'%yemekhane%'  -- sadece Yemekhane cihazları
          AND (@FirmaIdList  IS NULL OR k.FirmaId  IN (SELECT FirmaId  FROM @FirmaIds))
          AND (@IsyeriIdList IS NULL OR k.IsyeriId IN (SELECT IsyeriId FROM @IsyeriIds))
    ),
    Departmanli AS
    (
        SELECT
            Gun,
            IsyeriAdi,
            CASE 
                WHEN FirmaId = 101 AND IsyeriId = 14 THEN N'Akaryakıt'
                WHEN FirmaId = 101 AND IsyeriId = 13 THEN N'İstanbul Merkez'
                ELSE N'Merkez'
            END AS Departman
        FROM Ham
    )
    SELECT
        FORMAT(Gun, 'dd.MM.yyyy dddd', 'tr-TR') AS Tarih,
        IsyeriAdi AS [İşyeri],
        Departman,
        COUNT(*) AS [Toplam Yemek Adedi]               -- tekilleştirme yok: kaç geçiş varsa o
    FROM Departmanli
    GROUP BY Gun, IsyeriAdi, Departman
    ORDER BY Gun, IsyeriAdi, Departman;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_IlkGirisSonCikisRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_IlkGirisSonCikisRaporu]
    @FirmaIdList NVARCHAR(MAX),
    @IsyeriIdList NVARCHAR(MAX),
    @TarihBaslangic DATE,
    @TarihBitis DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FirmaIds TABLE (FirmaId INT);
    INSERT INTO @FirmaIds (FirmaId)
    SELECT TRY_CAST(value AS INT)
    FROM STRING_SPLIT(@FirmaIdList, ',')
    WHERE TRY_CAST(value AS INT) IS NOT NULL;

    DECLARE @IsyeriIds TABLE (IsyeriId INT);
    INSERT INTO @IsyeriIds (IsyeriId)
    SELECT TRY_CAST(value AS INT)
    FROM STRING_SPLIT(@IsyeriIdList, ',')
    WHERE TRY_CAST(value AS INT) IS NOT NULL;

    DECLARE @BaslangicTarihi DATETIME = DATEADD(SECOND, 0, CAST(@TarihBaslangic AS DATETIME));
    DECLARE @BitisTarihi    DATETIME = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(@TarihBitis AS DATETIME)));

    SELECT 
        k.PersonelId AS [Sicil No],
        k.Ad + ' ' + k.Soyad AS [Ad Soyad],
        iy.IsyeriAdi AS [İşyeri],
        FORMAT(CONVERT(DATE, kh.Tarih), 'dd.MM.yyyy dddd', 'tr-TR') AS Tarih,

        -- İLK GİRİŞ
        CASE 
            WHEN k.IsyeriId = 14 THEN
                ISNULL( FORMAT(MIN(kh.Tarih), 'HH:mm:ss'), 'HAREKETİ YOK' )
            ELSE
                ISNULL( FORMAT(MIN(CASE WHEN kh.Tip = N'Giriş' THEN kh.Tarih END), 'HH:mm:ss'), 'HAREKETİ YOK' )
        END AS [İlk Giriş],

        -- SON ÇIKIŞ (İşyeriId=14 ve tek hareketse 'TEK HAREKET')
        CASE 
            WHEN k.IsyeriId = 14 THEN
                CASE 
                    WHEN COUNT(*) = 1 THEN 'TEK HAREKET'
                    ELSE ISNULL( FORMAT(MAX(kh.Tarih), 'HH:mm:ss'), 'HAREKETİ YOK' )
                END
            ELSE
                ISNULL( FORMAT(MAX(CASE WHEN kh.Tip = N'Çıkış' THEN kh.Tarih END), 'HH:mm:ss'), 'HAREKETİ YOK' )
        END AS [Son Çıkış]

    FROM dbo.KisiHareketler kh
    INNER JOIN dbo.Kisiler k ON k.PersonelId = kh.PersonelId
    LEFT JOIN dbo.Isyerler iy ON iy.IsyeriId = k.IsyeriId AND iy.FirmaId = k.FirmaId
    WHERE 
        kh.Tarih BETWEEN @BaslangicTarihi AND @BitisTarihi
        AND kh.AktifMi = 1
        AND k.IstenCikisTarihi IS NULL
        AND k.FirmaId IN (SELECT FirmaId FROM @FirmaIds)
        AND k.IsyeriId IN (SELECT IsyeriId FROM @IsyeriIds)
    GROUP BY 
        k.PersonelId, k.Ad, k.Soyad, k.IsyeriId, iy.IsyeriAdi, CONVERT(DATE, kh.Tarih)
    ORDER BY 
        Tarih, [Ad Soyad];
END
GO
/****** Object:  StoredProcedure [dbo].[sp_IseBaslayanlarRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_IseBaslayanlarRaporu]
(
    @FirmaIdList      NVARCHAR(MAX),
    @IsyeriIdList     NVARCHAR(MAX),
    @TarihBaslangic   DATE,   -- Ayı buradan alacağız
    @TarihBitis       DATE    -- (kullanmayacağız, uyumluluk için dursun)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@TarihBaslangic IS NULL)
    BEGIN
        RAISERROR('Tarih başlangıç boş olamaz.', 16, 1);
        RETURN;
    END

    /* İçinde bulunulan ay (TarihBaslangic’in ayı) */
    DECLARE @AyBas DATE = DATEFROMPARTS(YEAR(@TarihBaslangic), MONTH(@TarihBaslangic), 1);
    DECLARE @AyBit DATE = EOMONTH(@TarihBaslangic);

    /* CSV -> tablo */
    DECLARE @F TABLE (FirmaId INT PRIMARY KEY);
    IF (ISNULL(@FirmaIdList,'') <> '')
        INSERT INTO @F(FirmaId)
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@FirmaIdList, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

    DECLARE @I TABLE (IsyeriId INT PRIMARY KEY);
    IF (ISNULL(@IsyeriIdList,'') <> '')
        INSERT INTO @I(IsyeriId)
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@IsyeriIdList, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

    SELECT
        k.PersonelId                                     AS [Sicil No],
        k.Ad + N' ' + k.Soyad                            AS [Adı Soyadı],
        f.FirmaAdi                                       AS [Firma],
        i.IsyeriAdi                                      AS [İşyeri],
        FORMAT(k.IseGirisTarihi, 'dd.MM.yyyy', 'tr-TR')  AS [İşe Başlama Tarihi]
    FROM dbo.Kisiler k
    LEFT JOIN dbo.Firmalar f
        ON f.FirmaId = k.FirmaId
    LEFT JOIN dbo.Isyerler i
        ON i.IsyeriId = k.IsyeriId
       AND i.FirmaId  = k.FirmaId
    WHERE
        k.PuantajYapilirMi = 1
        AND k.IseGirisTarihi BETWEEN @AyBas AND @AyBit

        /* Aynı ay içinde işe girip çıkanları gösterme */
        AND (
             k.IstenCikisTarihi IS NULL
             OR k.IstenCikisTarihi > @AyBit
        )

        AND (ISNULL(@FirmaIdList,'')  = '' OR k.FirmaId  IN (SELECT FirmaId  FROM @F))
        AND (ISNULL(@IsyeriIdList,'') = '' OR k.IsyeriId IN (SELECT IsyeriId FROM @I))
    ORDER BY k.IseGirisTarihi DESC, k.Ad, k.Soyad;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_IstenAyrilanlarRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_IstenAyrilanlarRaporu]
(
    @FirmaIdList      NVARCHAR(MAX),
    @IsyeriIdList     NVARCHAR(MAX),
    @TarihBaslangic   DATE,   -- Ayı buradan alacağız
    @TarihBitis       DATE    -- (kullanmayacağız, uyumluluk için dursun)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@TarihBaslangic IS NULL)
    BEGIN
        RAISERROR('Tarih başlangıç boş olamaz.', 16, 1);
        RETURN;
    END

    /* İçinde bulunulan ay (TarihBaslangic’in ayı) */
    DECLARE @AyBas DATE = DATEFROMPARTS(YEAR(@TarihBaslangic), MONTH(@TarihBaslangic), 1);
    DECLARE @AyBit DATE = EOMONTH(@TarihBaslangic);

    /* CSV -> tablo */
    DECLARE @F TABLE (FirmaId INT PRIMARY KEY);
    IF (ISNULL(@FirmaIdList,'') <> '')
        INSERT INTO @F(FirmaId)
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@FirmaIdList, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

    DECLARE @I TABLE (IsyeriId INT PRIMARY KEY);
    IF (ISNULL(@IsyeriIdList,'') <> '')
        INSERT INTO @I(IsyeriId)
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@IsyeriIdList, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL;

    SELECT
        k.PersonelId                                       AS [Sicil No],
        k.Ad + N' ' + k.Soyad                              AS [Adı Soyadı],
        f.FirmaAdi                                         AS [Firma],
        i.IsyeriAdi                                        AS [İşyeri],
        FORMAT(k.IstenCikisTarihi, 'dd.MM.yyyy', 'tr-TR')  AS [İşten Ayrılma Tarihi]
    FROM dbo.Kisiler k
    LEFT JOIN dbo.Firmalar f
        ON f.FirmaId = k.FirmaId
    LEFT JOIN dbo.Isyerler i
        ON i.IsyeriId = k.IsyeriId
       AND i.FirmaId  = k.FirmaId
    WHERE
        k.PuantajYapilirMi = 1
        AND k.IstenCikisTarihi IS NOT NULL
        AND CONVERT(date, k.IstenCikisTarihi) BETWEEN @AyBas AND @AyBit

        AND (ISNULL(@FirmaIdList,'')  = '' OR k.FirmaId  IN (SELECT FirmaId  FROM @F))
        AND (ISNULL(@IsyeriIdList,'') = '' OR k.IsyeriId IN (SELECT IsyeriId FROM @I))
    ORDER BY k.IstenCikisTarihi DESC, k.Ad, k.Soyad;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_MisafirKartlariRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_MisafirKartlariRaporu]
  @FirmaIdList    nvarchar(max) = NULL,   -- CSV: "101,102" (opsiyonel)
  @IsyeriIdList   nvarchar(max) = NULL,   -- Şimdilik kullanılmıyor
  @TarihBaslangic datetime2,              -- aralık başlangıcı
  @TarihBitis     datetime2               -- aralık bitişi
AS
BEGIN
  SET NOCOUNT ON;

  ;WITH F AS (
    SELECT TRY_CAST(value AS int) AS FirmaId
    FROM STRING_SPLIT(COALESCE(@FirmaIdList,''), ',')
    WHERE TRY_CAST(value AS int) IS NOT NULL
  )
  SELECT
      -- Kullanılan kart adı artık Kisiler'den
      COALESCE(
        NULLIF(LTRIM(RTRIM(ISNULL(p.Ad, N'') + N' ' + ISNULL(p.Soyad, N''))), N''),
        CAST(p.PersonelId AS nvarchar(30))
      ) AS [Kullanılan Kart Adı],
      ISNULL(f.FirmaAdi, CAST(p.FirmaId AS nvarchar(20))) AS [Firma Adı],
      iy.IsyeriAdi AS [İşyeri],
      ka.MisafirAdSoyad AS [Kullanan Adı Soyadı],
      ka.Baslangic AS [Kart Alım Zamanı],
      ka.Bitis AS [Kart Teslim Zamanı],
      CASE
        WHEN ka.Baslangic < @TarihBitis
         AND ISNULL(ka.Bitis,'9999-12-31') > @TarihBaslangic
        THEN DATEDIFF(
               MINUTE,
               CASE WHEN ka.Baslangic < @TarihBaslangic THEN @TarihBaslangic ELSE ka.Baslangic END,
               CASE WHEN ISNULL(ka.Bitis,'9999-12-31') > @TarihBitis THEN @TarihBitis ELSE ISNULL(ka.Bitis,'9999-12-31') END
             )
        ELSE 0
      END AS [Kullanım Süresi(Dakika)],
      CASE
        WHEN GETDATE() BETWEEN ka.Baslangic AND ISNULL(ka.Bitis,'9999-12-31')
          THEN 'Devam Ediyor'
        WHEN ISNULL(ka.Bitis,'9999-12-31') <= GETDATE()
          THEN 'Tamamlandı'
        ELSE 'Planlı'
      END AS [Kart Kullanım Durumu]
  FROM dbo.PuantajsizKartAtamalari ka
  JOIN dbo.Kisiler p ON p.PersonelId = ka.KartId
  LEFT JOIN dbo.Firmalar f ON f.FirmaId = p.FirmaId
  LEFT JOIN dbo.Isyerler iy ON iy.IsyeriId = p.IsyeriId AND iy.FirmaId = p.FirmaId
  WHERE
      ka.Baslangic < @TarihBitis
  AND ISNULL(ka.Bitis,'9999-12-31') > @TarihBaslangic
  AND (NOT EXISTS (SELECT 1 FROM F) OR p.FirmaId IN (SELECT FirmaId FROM F))
  ORDER BY [Kullanılan Kart Adı], ka.Baslangic;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_MolaRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_MolaRaporu]
    @FirmaIdList      NVARCHAR(MAX),
    @IsyeriIdList     NVARCHAR(MAX),
    @TarihBaslangic   DATE,
    @TarihBitis       DATE,
    @MinMolaDakika    INT = 0,     -- 0 dk altını sayma
    @MaxMolaDakika    INT = 180    -- 3 saatten fazlasını mola sayma
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Baslangic DATETIME = CAST(@TarihBaslangic AS DATETIME);
    DECLARE @Bitis     DATETIME = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(@TarihBitis AS DATETIME)));

    -- Firma / İşyeri listeleri
    DECLARE @FirmaIds  TABLE (FirmaId  INT);
    INSERT INTO @FirmaIds
    SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
    FROM STRING_SPLIT(@FirmaIdList, ',')
    WHERE ISNUMERIC(LTRIM(RTRIM(value))) = 1;

    DECLARE @IsyeriIds TABLE (IsyeriId INT);
    INSERT INTO @IsyeriIds
    SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
    FROM STRING_SPLIT(@IsyeriIdList, ',')
    WHERE ISNUMERIC(LTRIM(RTRIM(value))) = 1;

    ;WITH Base AS (
        SELECT
            KH.Id            AS HareketId,
            KH.PersonelId,
            KH.Tarih,              -- datetime
            KH.Tip,                -- 'Giriş' / 'Çıkış'
            KH.CihazId,
            C.CihazAdi,
            C.Notlar
        FROM dbo.KisiHareketler KH
        INNER JOIN dbo.Kisiler  K  ON KH.PersonelId = K.PersonelId
        INNER JOIN dbo.Cihazlar C  ON KH.CihazId    = C.CihazId
        WHERE KH.AktifMi = 1
          AND KH.Tarih BETWEEN @Baslangic AND @Bitis
          AND K.IstenCikisTarihi IS NULL
          AND K.FirmaId  IN (SELECT FirmaId  FROM @FirmaIds)
          AND K.IsyeriId IN (SELECT IsyeriId FROM @IsyeriIds)
          AND KH.Tip IN (N'Giriş', N'Çıkış')                  -- Yemekhane hariç
          AND ISNULL(C.Notlar, N'') NOT LIKE N'%(Ara Kat)%' -- Ara Kat cihazları hariç
    ),
    Exits AS (   -- mola başlangıçları
        SELECT
            PersonelId,
            Tarih     AS CikisZamani,
            CihazAdi  AS CikisTerminal,
            HareketId AS CikisHareketId
        FROM Base
        WHERE Tip = N'Çıkış'
    )
    SELECT
        K.PersonelId                        AS [Sicil No],
        K.Ad + N' ' + K.Soyad               AS [Adı Soyadı],
        IY.IsyeriAdi                        AS [İşyeri],

        FORMAT(E.CikisZamani, 'dd.MM.yyyy dddd', 'tr-TR') AS [Çıkış Tarihi],
        FORMAT(E.CikisZamani, 'HH:mm:ss')                 AS [Çıkış Saati],
        E.CikisTerminal                                    AS [Çıkış Turnikesi],

        FORMAT(N.GirisZamani, 'dd.MM.yyyy dddd', 'tr-TR') AS [Dönüş(Giriş) Tarihi],
        FORMAT(N.GirisZamani, 'HH:mm:ss')                 AS [Dönüş(Giriş) Saati],
        N.GirisTerminal                                    AS [Dönüş(Giriş) Turnikesi],

        DATEDIFF(MINUTE, E.CikisZamani, N.GirisZamani)     AS [Mola(Dakika)]
    FROM Exits E
    INNER JOIN dbo.Kisiler K
        ON K.PersonelId = E.PersonelId
    LEFT JOIN dbo.Isyerler IY
        ON IY.IsyeriId = K.IsyeriId
       AND IY.FirmaId  = K.FirmaId
    OUTER APPLY (  -- aynı gün içinde, eşiklere uyan ilk giriş
        SELECT TOP (1)
            b2.Tarih     AS GirisZamani,
            b2.CihazAdi  AS GirisTerminal
        FROM Base b2
        WHERE b2.PersonelId = E.PersonelId
          AND b2.Tip = N'Giriş'
          AND b2.Tarih > E.CikisZamani
          AND CAST(b2.Tarih AS date) = CAST(E.CikisZamani AS date)  -- GECEYE TAŞIMA
          AND DATEDIFF(MINUTE, E.CikisZamani, b2.Tarih) BETWEEN @MinMolaDakika AND @MaxMolaDakika
        ORDER BY b2.Tarih, b2.HareketId
    ) N
    WHERE N.GirisZamani IS NOT NULL                      -- sadece geçerli molalar
    ORDER BY K.PersonelId, E.CikisZamani;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_OnayKontrolMekanizmasi]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_OnayKontrolMekanizmasi]
  @KullaniciId INT,
  @SayfaAdi     NVARCHAR(100),
  @YetkiTipi    NVARCHAR(50)
AS
BEGIN
  SET NOCOUNT ON;
  IF EXISTS (SELECT 1 FROM dbo.KullaniciEkstraYetkilendirme 
             WHERE KullaniciId=@KullaniciId AND SayfaAdi=@SayfaAdi AND YetkiTipi=@YetkiTipi AND Allowed=0)
  BEGIN
     SELECT CAST(0 AS BIT) AS Allowed; RETURN;
  END
  IF EXISTS (SELECT 1 FROM dbo.KullaniciEkstraYetkilendirme  
             WHERE KullaniciId=@KullaniciId AND SayfaAdi=@SayfaAdi AND YetkiTipi=@YetkiTipi AND Allowed=1)
  BEGIN
     SELECT CAST(1 AS BIT) AS Allowed; RETURN;
  END
  IF EXISTS (SELECT 1 
             FROM dbo.Kullanicilar K 
             JOIN dbo.SayfaYetkileri SY ON SY.RolId = K.RolId
             WHERE K.KullaniciId=@KullaniciId 
               AND SY.SayfaAdi=@SayfaAdi 
               AND SY.YetkiTipi=@YetkiTipi)
  BEGIN
     SELECT CAST(1 AS BIT) AS Allowed; RETURN;
  END
  SELECT CAST(0 AS BIT) AS Allowed;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_Puantaj_Final_Upsert]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Puantaj_Final_Upsert]
  @PersonelId int,
  @Tarih date,
  @CalismaTipi nvarchar(20),
  @Saat decimal(6,2),
  @KullaniciId int = NULL
AS
BEGIN
  SET NOCOUNT ON;

  ;WITH k AS (
      SELECT p.PersonelId, p.Ad, p.Soyad
      FROM dbo.Kisiler p
      WHERE p.PersonelId = @PersonelId
  )
  MERGE dbo.FinalPuantajVerisi AS T
  USING (SELECT PersonelId, Ad, Soyad FROM k) S
     ON T.SicilNo = S.PersonelId AND T.Tarih = @Tarih
  WHEN MATCHED THEN
    UPDATE SET
      CalismaTipi = @CalismaTipi,
      Saat        = @Saat,
      KayitZamani = GETDATE()
  WHEN NOT MATCHED THEN
    INSERT (SicilNo, Ad, Soyad, Tarih, CalismaTipi, Saat, KayitZamani)
    VALUES (S.PersonelId, S.Ad, S.Soyad, @Tarih, @CalismaTipi, @Saat, GETDATE());
END
GO
/****** Object:  StoredProcedure [dbo].[sp_Puantaj_Onay_Upsert]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_Puantaj_Onay_Upsert]
    @PersonelId int,
    @Tarih date,
    @OnayDurumu int,              -- 0 Bekliyor, 1 Onay, 2 Ret, 3 Düzeltili
    @DuzenlenmisFMDakika int,
    @Aciklama nvarchar(400) = NULL,
    @KullaniciId int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    MERGE dbo.PuantajOnay AS T
    USING (SELECT @PersonelId AS PersonelId, @Tarih AS Tarih) AS S
    ON (T.PersonelId = S.PersonelId AND T.Tarih = S.Tarih)
    WHEN MATCHED THEN
        UPDATE SET
            OnayDurumu          = @OnayDurumu,
            DuzenlenmisFMDakika = @DuzenlenmisFMDakika,
            Aciklama            = @Aciklama,
            GuncellemeZamani    = GETDATE(),
            GuncelleyenKullaniciId = @KullaniciId
    WHEN NOT MATCHED THEN
        INSERT (PersonelId, Tarih, OnayDurumu, DuzenlenmisFMDakika, Aciklama, OlusturmaZamani, OlusturanKullaniciId)
        VALUES (@PersonelId, @Tarih, @OnayDurumu, @DuzenlenmisFMDakika, @Aciklama, GETDATE(), @KullaniciId);
END
GO
/****** Object:  StoredProcedure [dbo].[sp_PuantajTipleri_GetActive]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_PuantajTipleri_GetActive]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Kod, Ad, VarsayilanSaat
    FROM dbo.PuantajTipleri
    WHERE AktifMi = 1
    ORDER BY Ad;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ResmiTatilEkle]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_ResmiTatilEkle]
  @Tarih         date,
  @Ad            nvarchar(100),
  @CalismaSaati  decimal(5,2) = 7.50   -- yarım gün için 3.75 vb. gönderilebilir
AS
BEGIN
  SET NOCOUNT ON;

  -- Basit doğrulama (opsiyonel)
  IF (@CalismaSaati IS NOT NULL AND @CalismaSaati < 0)
  BEGIN
      RAISERROR(N'Çalışma saati negatif olamaz.', 16, 1);
      RETURN;
  END

  MERGE dbo.ResmiTatiller AS T
  USING (SELECT @Tarih AS Tarih,
                @Ad    AS Ad,
                @CalismaSaati AS CalismaSaati) AS S
     ON (T.Tarih = S.Tarih)
  WHEN MATCHED THEN
       UPDATE SET
           T.Ad = S.Ad,
           -- Parametre NULL ise mevcut saat değeri korunur
           T.CalismaSaati = COALESCE(S.CalismaSaati, T.CalismaSaati)
  WHEN NOT MATCHED THEN
       INSERT (Tarih, Ad, CalismaSaati)
       VALUES (S.Tarih, S.Ad, ISNULL(S.CalismaSaati, 7.50));
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ResmiTatiller_DoldurSabit]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_ResmiTatiller_DoldurSabit]
  @BasYil int,
  @BitYil int
AS
BEGIN
  SET NOCOUNT ON;
  SET XACT_ABORT ON;

  IF (@BitYil < @BasYil)
  BEGIN
    RAISERROR(N'BitYil, BasYil''dan küçük olamaz', 16, 1);
    RETURN;
  END

  ;WITH Sabit(Ay, Gun, Ad) AS (
      SELECT 1 , 1 , N'Yılbaşı' UNION ALL
      SELECT 4 ,23, N'Ulusal Egemenlik ve Çocuk Bayramı' UNION ALL
      SELECT 5 , 1, N'Emek ve Dayanışma Günü' UNION ALL
      SELECT 5 ,19, N'Atatürk''ü Anma, Gençlik ve Spor Bayramı' UNION ALL
      SELECT 7 ,15, N'Demokrasi ve Millî Birlik Günü' UNION ALL
      SELECT 8 ,30, N'Zafer Bayramı' UNION ALL
      SELECT 10,29, N'Cumhuriyet Bayramı'
  ),
  Yillar AS (
      SELECT @BasYil AS Yil
      UNION ALL
      SELECT Yil + 1 FROM Yillar WHERE Yil + 1 <= @BitYil
  )
  MERGE dbo.ResmiTatiller AS T
  USING (
      SELECT
          DATEFROMPARTS(y.Yil, s.Ay, s.Gun) AS Tarih,
          s.Ad,
          CONVERT(decimal(5,2), 7.50)      AS CalismaSaat
      FROM Yillar y
      CROSS JOIN Sabit s
  ) AS S
  ON  T.Tarih = S.Tarih
  WHEN MATCHED THEN
      UPDATE SET
          T.Ad           = S.Ad,
          T.CalismaSaati  = S.CalismaSaat
  WHEN NOT MATCHED THEN
      INSERT (Tarih, Ad, CalismaSaati)
      VALUES (S.Tarih, S.Ad, S.CalismaSaat)
  ;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_TumHareketlerRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_TumHareketlerRaporu]
    @FirmaIdList NVARCHAR(MAX),
    @IsyeriIdList NVARCHAR(MAX),
    @TarihBaslangic DATE,
    @TarihBitis DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Firma listesi tabloya dönüştürülüyor
    DECLARE @FirmaIds TABLE (FirmaId INT);
    INSERT INTO @FirmaIds
    SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
    FROM STRING_SPLIT(@FirmaIdList, ',')
    WHERE ISNUMERIC(LTRIM(RTRIM(value))) = 1;

    -- İşyeri listesi tabloya dönüştürülüyor
    DECLARE @IsyeriIds TABLE (IsyeriId INT);
    INSERT INTO @IsyeriIds
    SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
    FROM STRING_SPLIT(@IsyeriIdList, ',')
    WHERE ISNUMERIC(LTRIM(RTRIM(value))) = 1;

    -- Tarih aralığı
    DECLARE @Baslangic DATETIME = DATEADD(SECOND, 0, CAST(@TarihBaslangic AS DATETIME));
    DECLARE @Bitis DATETIME = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(@TarihBitis AS DATETIME)));

    -- Sorgu
    SELECT 
        K.PersonelId AS [Sicil No],
        K.Ad+' '+K.Soyad AS [Adı Soyadı],
        IY.IsyeriAdi AS [İşyeri],
        FORMAT(KH.Tarih, 'dd.MM.yyyy dddd', 'tr-TR') AS [Tarih],
        FORMAT(KH.Tarih, 'HH:mm:ss') AS [Saat],
        CASE 
            WHEN KH.Tip = 'Giriş' THEN 'GİRİŞ'
            WHEN KH.Tip = 'Çıkış' THEN 'ÇIKIŞ'
            ELSE 'ACCESS'
        END AS [Hareket Tipi],
        C.CihazAdi AS [Terminal Adı]
    FROM dbo.KisiHareketler KH
    INNER JOIN dbo.Kisiler K ON KH.PersonelId = K.PersonelId
    INNER JOIN dbo.Cihazlar C ON KH.CihazId = C.CihazId
    LEFT JOIN dbo.Isyerler IY ON IY.IsyeriId = K.IsyeriId AND IY.FirmaId = K.FirmaId
    WHERE 
        KH.Tarih BETWEEN @Baslangic AND @Bitis
        AND K.IstenCikisTarihi IS NULL
        AND KH.AktifMi = 1
        AND K.FirmaId IN (SELECT FirmaId FROM @FirmaIds)
        AND K.IsyeriId IN (SELECT IsyeriId FROM @IsyeriIds)
    ORDER BY KH.Tarih ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_YemekhaneGecisRaporu]    Script Date: 13.04.2026 10:28:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_YemekhaneGecisRaporu]
    @FirmaIdList NVARCHAR(MAX),
    @IsyeriIdList NVARCHAR(MAX),
    @TarihBaslangic DATE,
    @TarihBitis DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FirmaIds TABLE (FirmaId INT);
    INSERT INTO @FirmaIds (FirmaId)
    SELECT TRY_CAST(value AS INT)
    FROM STRING_SPLIT(COALESCE(@FirmaIdList,''), ',')
    WHERE TRY_CAST(value AS INT) IS NOT NULL;

    DECLARE @IsyeriIds TABLE (IsyeriId INT);
    INSERT INTO @IsyeriIds (IsyeriId)
    SELECT TRY_CAST(value AS INT)
    FROM STRING_SPLIT(COALESCE(@IsyeriIdList,''), ',')
    WHERE TRY_CAST(value AS INT) IS NOT NULL;

    DECLARE @BaslangicTarihi DATETIME = DATEADD(SECOND, 0, CAST(@TarihBaslangic AS DATETIME));
    DECLARE @BitisTarihi DATETIME = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(@TarihBitis AS DATETIME)));

    SELECT
        K.PersonelId AS [Sicil No],
        COALESCE(
            NULLIF(LTRIM(RTRIM(ISNULL(K.Ad, N'') + N' ' + ISNULL(K.Soyad, N''))), N''),
            K.PersonelId
        ) AS [Ad Soyad],
        IY.IsyeriAdi AS [İşyeri],
        C.CihazAdi AS [Turnike],
        FORMAT(YGH.KayitZamani, 'dd.MM.yyyy dddd', 'tr-TR') AS [Hareket Günü],
        FORMAT(YGH.KayitZamani, 'HH:mm:ss') AS [Kart Okuma Zamanı]
    FROM YemekhaneGecisHareketler AS YGH
    INNER JOIN Cihazlar AS C ON C.CihazId = YGH.CihazId
    INNER JOIN Kisiler  AS K ON K.PersonelId = YGH.PersonelId
                            AND K.IstenCikisTarihi IS NULL
                            AND (NOT EXISTS (SELECT 1 FROM @FirmaIds) OR K.FirmaId IN (SELECT FirmaId FROM @FirmaIds))
                            --AND (NOT EXISTS (SELECT 1 FROM @IsyeriIds) OR K.IsyeriId IN (SELECT IsyeriId FROM @IsyeriIds))
    LEFT JOIN dbo.Isyerler IY ON IY.IsyeriId = K.IsyeriId AND IY.FirmaId = K.FirmaId
    WHERE
        YGH.KayitZamani BETWEEN @BaslangicTarihi AND @BitisTarihi
    ORDER BY
        YGH.KayitZamani ASC;
END
GO
