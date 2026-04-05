import api, { cachedGet } from "./api";
import { invalidate } from "./httpCache";

export type ApiResult<T = any> = {
  success: boolean;
  message?: string;
  data?: T;
};

export type LookupItem = { id: number; ad: string };

export type FirmaRow = {
  firmaId?: number;
  FirmaId?: number;
  firmaAdi?: string;
  FirmaAdi?: string;
  itBirimMail?: string | null;
  ITBirimMail?: string | null;
};

export type FirmaUpsertRequest = {
  firmaId?: number | null;
  firmaAdi: string;
  itBirimMail?: string | null;
};

export type IsyeriItem = {
  firmaId?: number;
  FirmaId?: number;
  isyeriId?: number;
  IsyeriId?: number;
  ad?: string;
  Ad?: string;
};

export type DepartmanDetail = { id: number; ad: string; aciklama?: string | null };
export type PozisyonDetail = { id: number; ad: string; aciklama?: string | null };

export const organizasyonService = {
  // Firmalar
  async listFirmalar(opts?: { forceRefresh?: boolean }): Promise<ApiResult<FirmaRow[]>> {
    return await cachedGet<ApiResult<FirmaRow[]>>("/Firma", {
      timeout: 15000,
      keyPrefix: "/Firma",
      softTtlMs: 5 * 60 * 1000,
      hardTtlMs: 24 * 60 * 60 * 1000,
      forceRefresh: !!opts?.forceRefresh,
    });
  },
  async nextFirmaId(): Promise<ApiResult<number>> {
    const resp = await api.get("/Firma/nextId", { timeout: 15000 });
    return resp.data;
  },
  async createFirma(payload: FirmaUpsertRequest): Promise<ApiResult<number>> {
    const resp = await api.post("/Firma", payload, { timeout: 15000 });
    await invalidate("/Firma");
    return resp.data;
  },
  async updateFirma(id: number, payload: FirmaUpsertRequest): Promise<ApiResult<any>> {
    const resp = await api.put(`/Firma/${encodeURIComponent(String(id))}`, payload, { timeout: 15000 });
    await invalidate("/Firma");
    return resp.data;
  },
  async deleteFirma(id: number): Promise<ApiResult<any>> {
    const resp = await api.delete(`/Firma/${encodeURIComponent(String(id))}`, { timeout: 15000 });
    await invalidate("/Firma");
    return resp.data;
  },

  // İşyerleri
  async isyeriLookups(opts?: { forceRefresh?: boolean }): Promise<ApiResult<{ firmalar: LookupItem[]; aktifFirmaId: number | null }>> {
    return await cachedGet<ApiResult<{ firmalar: LookupItem[]; aktifFirmaId: number | null }>>("/Isyeri/lookups", {
      timeout: 15000,
      keyPrefix: "/Isyeri/lookups",
      softTtlMs: 5 * 60 * 1000,
      hardTtlMs: 24 * 60 * 60 * 1000,
      forceRefresh: !!opts?.forceRefresh,
    });
  },
  async listIsyerleri(params?: { firmaId?: number | null }, opts?: { forceRefresh?: boolean }): Promise<ApiResult<IsyeriItem[]>> {
    return await cachedGet<ApiResult<IsyeriItem[]>>("/Isyeri", {
      params,
      timeout: 15000,
      keyPrefix: "/Isyeri",
      softTtlMs: 5 * 60 * 1000,
      hardTtlMs: 24 * 60 * 60 * 1000,
      forceRefresh: !!opts?.forceRefresh,
    });
  },
  async createIsyeri(payload: { firmaId: number; isyeriId: number; ad: string }): Promise<ApiResult<any>> {
    const resp = await api.post(
      "/Isyeri",
      { FirmaId: payload.firmaId, IsyeriId: payload.isyeriId, Ad: payload.ad },
      { timeout: 15000 }
    );
    await invalidate("/Isyeri");
    await invalidate("/Isyeri/lookups");
    return resp.data;
  },
  async updateIsyeri(payload: { firmaId: number; isyeriId: number; ad: string }): Promise<ApiResult<any>> {
    const resp = await api.put(
      `/Isyeri/${encodeURIComponent(String(payload.isyeriId))}`,
      { FirmaId: payload.firmaId, IsyeriId: payload.isyeriId, Ad: payload.ad },
      { timeout: 15000 }
    );
    await invalidate("/Isyeri");
    await invalidate("/Isyeri/lookups");
    return resp.data;
  },
  async deleteIsyeri(payload: { firmaId: number; isyeriId: number }): Promise<ApiResult<any>> {
    const resp = await api.delete(`/Isyeri/${encodeURIComponent(String(payload.firmaId))}/${encodeURIComponent(String(payload.isyeriId))}`, {
      timeout: 15000,
    });
    await invalidate("/Isyeri");
    await invalidate("/Isyeri/lookups");
    return resp.data;
  },

  // Departmanlar
  async listDepartmanlar(opts?: { forceRefresh?: boolean }): Promise<ApiResult<LookupItem[]>> {
    return await cachedGet<ApiResult<LookupItem[]>>("/Departman", {
      timeout: 15000,
      keyPrefix: "/Departman",
      softTtlMs: 5 * 60 * 1000,
      hardTtlMs: 24 * 60 * 60 * 1000,
      forceRefresh: !!opts?.forceRefresh,
    });
  },
  async getDepartman(id: number): Promise<ApiResult<DepartmanDetail>> {
    const resp = await api.get(`/Departman/${encodeURIComponent(String(id))}`, { timeout: 15000 });
    return resp.data;
  },
  async createDepartman(payload: { ad: string; aciklama?: string | null }): Promise<ApiResult<number>> {
    const resp = await api.post("/Departman", { Ad: payload.ad, Aciklama: payload.aciklama ?? "" }, { timeout: 15000 });
    await invalidate("/Departman");
    return resp.data;
  },
  async updateDepartman(id: number, payload: { ad: string; aciklama?: string | null }): Promise<ApiResult<any>> {
    const resp = await api.put(`/Departman/${encodeURIComponent(String(id))}`, { Ad: payload.ad, Aciklama: payload.aciklama ?? "" }, { timeout: 15000 });
    await invalidate("/Departman");
    return resp.data;
  },
  async deleteDepartman(id: number): Promise<ApiResult<any>> {
    const resp = await api.delete(`/Departman/${encodeURIComponent(String(id))}`, { timeout: 15000 });
    await invalidate("/Departman");
    return resp.data;
  },

  // Pozisyonlar
  async listPozisyonlar(opts?: { forceRefresh?: boolean }): Promise<ApiResult<LookupItem[]>> {
    return await cachedGet<ApiResult<LookupItem[]>>("/Pozisyon", {
      timeout: 15000,
      keyPrefix: "/Pozisyon",
      softTtlMs: 5 * 60 * 1000,
      hardTtlMs: 24 * 60 * 60 * 1000,
      forceRefresh: !!opts?.forceRefresh,
    });
  },
  async getPozisyon(id: number): Promise<ApiResult<PozisyonDetail>> {
    const resp = await api.get(`/Pozisyon/${encodeURIComponent(String(id))}`, { timeout: 15000 });
    return resp.data;
  },
  async createPozisyon(payload: { ad: string; aciklama?: string | null }): Promise<ApiResult<any>> {
    const resp = await api.post("/Pozisyon", { Ad: payload.ad, Aciklama: payload.aciklama ?? "" }, { timeout: 15000 });
    await invalidate("/Pozisyon");
    return resp.data;
  },
  async updatePozisyon(id: number, payload: { ad: string; aciklama?: string | null }): Promise<ApiResult<any>> {
    const resp = await api.put(`/Pozisyon/${encodeURIComponent(String(id))}`, { Ad: payload.ad, Aciklama: payload.aciklama ?? "" }, { timeout: 15000 });
    await invalidate("/Pozisyon");
    return resp.data;
  },
  async deletePozisyon(id: number): Promise<ApiResult<any>> {
    const resp = await api.delete(`/Pozisyon/${encodeURIComponent(String(id))}`, { timeout: 15000 });
    await invalidate("/Pozisyon");
    return resp.data;
  },
};

