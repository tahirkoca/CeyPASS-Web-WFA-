import api from "./api";

export type ApiResult<T = any> = {
  success: boolean;
  message?: string;
  data?: T;
};

export type PersonelAdSoyad = { id: number; adSoyad: string };
export type KullaniciAdminRow = {
  kullaniciId?: number;
  KullaniciId?: number;
  kullaniciAdi?: string;
  KullaniciAdi?: string;
  rolTanimi?: string | null;
  RolTanimi?: string | null;
  rolId?: number | null;
  RolId?: number | null;
  personelId?: number | null;
  PersonelId?: number | null;
};

export type UstYetkili = {
  personelId?: string;
  PersonelId?: string;
  ustYetkiliPersonelId?: string;
  UstYetkiliPersonelId?: string;
  olusturmaTarihi?: string;
  OlusturmaTarihi?: string;
};

export type AdminPanelDto = {
  kullanicilar: KullaniciAdminRow[];
  personeller: PersonelAdSoyad[];
  ustYetkililer: UstYetkili[];
};

export type GuncellemeNotifikasyonDTO = {
  versiyonNumarasi: string;
  yayinTarihi: string; // ISO
  guncellemeTipi: "Major" | "Minor" | "Bugfix" | string;
  yeniOzellikler: string[];
  iyilestirmeler: string[];
  hataDuzeltmeleri: string[];
  kritikDegisiklikler: string[];
  ekNotlar?: string;
};

export const adminService = {
  async panel(): Promise<ApiResult<AdminPanelDto>> {
    const resp = await api.get("/Admin/panel", { timeout: 15000 });
    return resp.data;
  },
  async setKullaniciPersonel(payload: { kullaniciId: number; personelId?: number | null }): Promise<ApiResult<any>> {
    const resp = await api.post("/Admin/kullanici-personel", { KullaniciId: payload.kullaniciId, PersonelId: payload.personelId ?? null }, { timeout: 15000 });
    return resp.data;
  },
  async setUstYetkili(payload: { personelId: string; ustYetkiliPersonelId?: string | null }): Promise<ApiResult<any>> {
    const resp = await api.post(
      "/Admin/ust-yetkili",
      { PersonelId: payload.personelId, UstYetkiliPersonelId: payload.ustYetkiliPersonelId ?? "" },
      { timeout: 15000 }
    );
    return resp.data;
  },
  async previewMail(payload: GuncellemeNotifikasyonDTO): Promise<ApiResult<string>> {
    const resp = await api.post(
      "/Admin/guncelleme-mail/preview",
      {
        VersiyonNumarasi: payload.versiyonNumarasi,
        YayinTarihi: payload.yayinTarihi,
        GuncellemeTipi: payload.guncellemeTipi,
        YeniOzellikler: payload.yeniOzellikler,
        Iyilestirmeler: payload.iyilestirmeler,
        HataDuzeltmeleri: payload.hataDuzeltmeleri,
        KritikDegisiklikler: payload.kritikDegisiklikler,
        EkNotlar: payload.ekNotlar ?? "",
      },
      { timeout: 30000 }
    );
    return resp.data;
  },
  async sendMail(payload: GuncellemeNotifikasyonDTO): Promise<ApiResult<any>> {
    const resp = await api.post(
      "/Admin/guncelleme-mail",
      {
        VersiyonNumarasi: payload.versiyonNumarasi,
        YayinTarihi: payload.yayinTarihi,
        GuncellemeTipi: payload.guncellemeTipi,
        YeniOzellikler: payload.yeniOzellikler,
        Iyilestirmeler: payload.iyilestirmeler,
        HataDuzeltmeleri: payload.hataDuzeltmeleri,
        KritikDegisiklikler: payload.kritikDegisiklikler,
        EkNotlar: payload.ekNotlar ?? "",
      },
      { timeout: 60000 }
    );
    return resp.data;
  },
};

