import api, { cachedGet } from "./api";
import { invalidate } from "./httpCache";

export type ApiResult<T = any> = {
  success: boolean;
  message?: string;
  data?: T;
};

export type PuantajGunSatirDTO = {
  tarih: string;
  Tarih?: string;
  vardiyaTuru?: string;
  VardiyaTuru?: string;
  ilkGiris?: string | null;
  IlkGiris?: string | null;
  sonCikis?: string | null;
  SonCikis?: string | null;
  vardiyaBaslangic?: string | null;
  VardiyaBaslangic?: string | null;
  vardiyaBitis?: string | null;
  VardiyaBitis?: string | null;
  saatlikIzinDakika?: number;
  SaatlikIzinDakika?: number;
  erkenGirisDakika?: number;
  ErkenGirisDakika?: number;
  gecCikisDakika?: number;
  GecCikisDakika?: number;
  sistemFmDakika?: number;
  SistemFMDakika?: number;
  onayDurumu?: any;
  OnayDurumu?: any;
  duzenlenenFmDakika?: number;
  DuzenlenenFMDakika?: number;
  aciklama?: string;
  Aciklama?: string;
  calismaTipi?: string;
  CalismaTipi?: string;
  saat?: number;
  Saat?: number;
};

export type PuantajTipDTO = { kod?: string; Kod?: string; ad?: string; Ad?: string };

export type PuantajLookupsDto = {
  selectedYil: number;
  selectedAy: number;
  selectedFirmaId: number;
  selectedIsyeriId?: number | null;
  firmalar: { firmaId: number; firmaAdi: string }[];
  isyerleri: { isyeriId: number; ad: string }[];
  personeller: { personelId: string; adSoyad: string }[];
  puantajTipleri: PuantajTipDTO[];
  ekKayitGun: number;
  canUpdate: boolean;
  canApprove: boolean;
  canDelete: boolean;
  canExport: boolean;
};

export const puantajService = {
  async lookups(
    params: { firmaId?: number | null; isyeriId?: number | null; yil?: number | null; ay?: number | null },
    opts?: { forceRefresh?: boolean }
  ): Promise<ApiResult<PuantajLookupsDto>> {
    return await cachedGet<ApiResult<PuantajLookupsDto>>("/Puantaj/lookups", {
      params,
      timeout: 20000,
      keyPrefix: "/Puantaj/lookups",
      softTtlMs: 60 * 1000,
      hardTtlMs: 10 * 60 * 1000,
      forceRefresh: !!opts?.forceRefresh,
    });
  },

  async getAy(personelId: string | number, yil: number, ay: number): Promise<ApiResult<PuantajGunSatirDTO[]>> {
    const resp = await api.get(`/Puantaj/${encodeURIComponent(String(personelId))}`, { params: { yil, ay }, timeout: 30000 });
    return resp.data;
  },

  async tipler(opts?: { forceRefresh?: boolean }): Promise<ApiResult<PuantajTipDTO[]>> {
    return await cachedGet<ApiResult<PuantajTipDTO[]>>("/Puantaj/tipler", {
      timeout: 15000,
      keyPrefix: "/Puantaj/tipler",
      softTtlMs: 24 * 60 * 60 * 1000,
      hardTtlMs: 7 * 24 * 60 * 60 * 1000,
      forceRefresh: !!opts?.forceRefresh,
    });
  },

  async onayla(payload: {
    personelId: number;
    tarih: string;
    duzenlenmisFm: number;
    aciklama?: string;
    calismaTipi?: string;
    saatText?: string;
  }): Promise<ApiResult<any>> {
    const resp = await api.post(
      "/Puantaj/onayla",
      {
        PersonelId: payload.personelId,
        Tarih: payload.tarih,
        DuzenlenmisFm: payload.duzenlenmisFm,
        Aciklama: payload.aciklama ?? "",
        CalismaTipi: payload.calismaTipi ?? "",
        SaatText: payload.saatText ?? "",
      },
      { timeout: 30000 }
    );
    await invalidate("/Puantaj");
    return resp.data;
  },

  async reddet(payload: { personelId: number; tarih: string; aciklama?: string }): Promise<ApiResult<any>> {
    const resp = await api.post(
      "/Puantaj/reddet",
      { PersonelId: payload.personelId, Tarih: payload.tarih, Aciklama: payload.aciklama ?? "" },
      { timeout: 30000 }
    );
    await invalidate("/Puantaj");
    return resp.data;
  },

  async duzenle(payload: {
    personelId: number;
    tarih: string;
    duzenlenmisFm: number;
    aciklama?: string;
    calismaTipi?: string;
    saatText?: string;
  }): Promise<ApiResult<any>> {
    const resp = await api.post(
      "/Puantaj/duzenle",
      {
        PersonelId: payload.personelId,
        Tarih: payload.tarih,
        DuzenlenmisFm: payload.duzenlenmisFm,
        Aciklama: payload.aciklama ?? "",
        CalismaTipi: payload.calismaTipi ?? "",
        SaatText: payload.saatText ?? "",
      },
      { timeout: 30000 }
    );
    await invalidate("/Puantaj");
    return resp.data;
  },

  async topluOnayla(payload: { personelId: number; yil: number; ay: number }): Promise<ApiResult<any>> {
    const resp = await api.post("/Puantaj/toplu-onayla", { PersonelId: payload.personelId, Yil: payload.yil, Ay: payload.ay }, { timeout: 60000 });
    await invalidate("/Puantaj");
    return resp.data;
  },

  async getEkKayitGun(opts?: { forceRefresh?: boolean }): Promise<ApiResult<number>> {
    return await cachedGet<ApiResult<number>>("/Puantaj/ek-kayit-gun", {
      timeout: 15000,
      keyPrefix: "/Puantaj/ek-kayit-gun",
      softTtlMs: 5 * 60 * 1000,
      hardTtlMs: 24 * 60 * 60 * 1000,
      forceRefresh: !!opts?.forceRefresh,
    });
  },

  async setEkKayitGun(gun: number): Promise<ApiResult<any>> {
    const resp = await api.post("/Puantaj/ek-kayit-gun", { Gun: gun }, { timeout: 15000 });
    await invalidate("/Puantaj/ek-kayit-gun");
    await invalidate("/Puantaj/lookups");
    return resp.data;
  },

  async cokluSicileAktar(payload: { personelId: number; yil: number; ay: number }): Promise<ApiResult<any>> {
    const resp = await api.post(
      "/Puantaj/coklu-sicile-aktar",
      { PersonelId: payload.personelId, Yil: payload.yil, Ay: payload.ay },
      { timeout: 60000 }
    );
    await invalidate("/Puantaj");
    return resp.data;
  },

  // Excel export: response handling is implemented in the screen (file write/share).
  async exportExcel(payload: { yil: number; ay: number }): Promise<any> {
    const resp = await api.post("/Puantaj/export-excel", { Yil: payload.yil, Ay: payload.ay }, { timeout: 120000, responseType: "arraybuffer" as any });
    return resp;
  },
};

