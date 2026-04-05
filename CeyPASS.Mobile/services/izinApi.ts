import api, { cachedGet } from "./api";
import { invalidate } from "./httpCache";

export type ApiResult<T = any> = {
  success: boolean;
  message?: string;
  data?: T;
};

export type PagedResponse<T = any> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type KisiIzinListRow = {
  kisiIzinId?: number;
  KisiIzinId?: number;
  sicilNo?: string;
  SicilNo?: string;
  adSoyad?: string;
  AdSoyad?: string;
  firmaAdi?: string;
  FirmaAdi?: string;
  izinTipi?: string;
  IzinTipi?: string;
  izinBaslangic?: string;
  IzinBaslangic?: string;
  izinBitis?: string;
  IzinBitis?: string;
  sureGun?: string;
  SureGun?: string;
  sureSaat?: number;
  SureSaat?: number;
  saatlikIzin?: string;
  SaatlikIzin?: string;
  aciklama?: string;
  Aciklama?: string;
  islenmeTarihi?: string;
  IslenmeTarihi?: string;
};

export type IzinUpsertRequest = {
  firmaId?: number | null;
  personelId: string;
  izinId: number;
  saatlikIzinMi: boolean;
  baslangicTarih: string; // yyyy-MM-dd
  bitisTarih: string; // yyyy-MM-dd
  baslangicSaat?: string | null; // HH:mm
  bitisSaat?: string | null; // HH:mm
  aciklama?: string | null;
};

export const izinService = {
  async lookups(params?: { firmaId?: number | null }): Promise<ApiResult<any>> {
    return await cachedGet<ApiResult<any>>("/Izin/lookups", {
      params,
      timeout: 15000,
      keyPrefix: "/Izin/lookups",
      softTtlMs: 5 * 60 * 1000,
      hardTtlMs: 24 * 60 * 60 * 1000,
    });
  },

  async list(params: {
    personelId?: string;
    izinTipId?: number;
    baslangic?: string; // yyyy-MM-dd
    bitis?: string; // yyyy-MM-dd
    page?: number;
    pageSize?: number;
  }): Promise<ApiResult<PagedResponse<KisiIzinListRow>>> {
    const response = await api.get("/Izin", { params, timeout: 15000 });
    return response.data;
  },

  async create(payload: IzinUpsertRequest): Promise<ApiResult<any>> {
    const response = await api.post("/Izin", payload, { timeout: 15000 });
    await invalidate("/Izin");
    await invalidate("/Izin/lookups");
    return response.data;
  },

  async update(id: number, payload: IzinUpsertRequest): Promise<ApiResult<any>> {
    const response = await api.put(`/Izin/${encodeURIComponent(String(id))}`, payload, { timeout: 15000 });
    await invalidate("/Izin");
    await invalidate("/Izin/lookups");
    return response.data;
  },

  async pasifYap(id: number): Promise<ApiResult<any>> {
    const response = await api.delete(`/Izin/${encodeURIComponent(String(id))}`, { timeout: 15000 });
    await invalidate("/Izin");
    return response.data;
  },
};

