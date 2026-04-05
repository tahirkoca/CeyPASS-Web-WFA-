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

export type KisiHareketRow = {
  id?: number;
  Id?: number;
  firma?: string;
  Firma?: string;
  sicilNo?: string;
  SicilNo?: string;
  adSoyad?: string;
  AdSoyad?: string;
  cihazAdi?: string;
  CihazAdi?: string;
  tarih?: string;
  Tarih?: string;
  tip?: string;
  Tip?: string;
  kayitZamani?: string;
  KayitZamani?: string;
  aktifMi?: boolean;
  AktifMi?: boolean;
};

export const kisiHareketService = {
  async lookups(params: { firmaId?: number | null; kartTipi?: "puantaj" | "puantajsiz" }): Promise<ApiResult<any>> {
    return await cachedGet<ApiResult<any>>("/KisiHareket/lookups", {
      params,
      timeout: 15000,
      keyPrefix: "/KisiHareket/lookups",
      softTtlMs: 5 * 60 * 1000,
      hardTtlMs: 24 * 60 * 60 * 1000,
    });
  },

  async list(params: {
    firmaId?: number | null;
    kartTipi?: "puantaj" | "puantajsiz";
    personelIds?: string;
    baslangic?: string;
    bitis?: string;
    sadeceAktif?: boolean;
    sadecePasif?: boolean;
    sadeceYemekhane?: boolean;
    page?: number;
    pageSize?: number;
  }): Promise<ApiResult<PagedResponse<KisiHareketRow>>> {
    const response = await api.get("/KisiHareket", { params, timeout: 15000 });
    return response.data;
  },

  async ekle(request: { personelId: number; tarih: string; tip: "GİRİŞ" | "ÇIKIŞ" }): Promise<ApiResult<any>> {
    const response = await api.post("/KisiHareket/ekle", request, { timeout: 15000 });
    await invalidate("/KisiHareket");
    return response.data;
  },

  async guncelle(id: number, request: { tarih: string; tip: "GİRİŞ" | "ÇIKIŞ" }): Promise<ApiResult<any>> {
    const response = await api.put(`/KisiHareket/${encodeURIComponent(String(id))}`, request, { timeout: 15000 });
    await invalidate("/KisiHareket");
    return response.data;
  },

  async pasifYap(id: number): Promise<ApiResult<any>> {
    const response = await api.delete(`/KisiHareket/${encodeURIComponent(String(id))}`, { timeout: 15000 });
    await invalidate("/KisiHareket");
    return response.data;
  },
};

