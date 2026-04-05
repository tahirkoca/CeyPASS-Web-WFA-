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

export const personelService = {
  async list(params: {
    search?: string;
    firmaId?: number | null;
    isyeriId?: number | null;
    puantajYapilirMi?: boolean;
    page?: number;
    pageSize?: number;
  }): Promise<ApiResult<PagedResponse<any>>> {
    const response = await api.get("/Personel", { params, timeout: 15000 });
    return response.data;
  },

  async details(personelId: string): Promise<ApiResult<any>> {
    const response = await api.get(`/Personel/${encodeURIComponent(personelId)}`, { timeout: 15000 });
    return response.data;
  },

  async lookupsForFirma(firmaId?: number | null): Promise<ApiResult<any>> {
    return await cachedGet<ApiResult<any>>("/Personel/lookups", {
      params: { firmaId: firmaId ?? undefined },
      timeout: 15000,
      keyPrefix: "/Personel/lookups",
      softTtlMs: 5 * 60 * 1000,
      hardTtlMs: 24 * 60 * 60 * 1000,
    });
  },

  async create(request: any): Promise<ApiResult<any>> {
    const response = await api.post("/Personel", request, { timeout: 15000 });
    await invalidate("/Personel");
    await invalidate("/Personel/lookups");
    return response.data;
  },

  async update(request: any): Promise<ApiResult<any>> {
    const response = await api.put("/Personel", request, { timeout: 15000 });
    await invalidate("/Personel");
    await invalidate("/Personel/lookups");
    return response.data;
  },

  async istenCikar(request: { personelId: string; cikisTarihi?: string | null; firmaDisiKartNo?: string | null }): Promise<ApiResult<any>> {
    const response = await api.post("/Personel/isten-cikar", request, { timeout: 15000 });
    await invalidate("/Personel");
    await invalidate("/Personel/lookups");
    return response.data;
  },
};

