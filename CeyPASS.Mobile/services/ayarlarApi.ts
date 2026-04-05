import api, { cachedGet } from "./api";
import { invalidate } from "./httpCache";

export type ApiResult<T = any> = {
  success: boolean;
  message?: string;
  data?: T;
};

export type LookupItem = { id: number; ad: string };

export type CalismaSekli = {
  id?: number;
  Id?: number;
  firmaId?: number;
  FirmaId?: number;
  ad?: string;
  Ad?: string;
  girisSaat?: string | null;
  GirisSaat?: string | null;
  cikisSaat?: string | null;
  CikisSaat?: string | null;
  // ... there may be more fields in entity; we pass through from API
  [key: string]: any;
};

export type CihazTip = { cihazTipId?: number; CihazTipId?: number; ad?: string; Ad?: string; [key: string]: any };

export type CihazListDTO = {
  cihazId?: number;
  CihazId?: number;
  cihazAdi?: string;
  CihazAdi?: string;
  ipAdres?: string;
  IPAdres?: string;
  port?: number;
  Port?: number;
  firmaAdi?: string;
  FirmaAdi?: string;
  aktifMi?: boolean;
  AktifMi?: boolean;
  [key: string]: any;
};

export type Cihaz = { cihazId?: number; CihazId?: number; firmaId?: number; FirmaId?: number; [key: string]: any };

export type ResmiTatilDTO = {
  tarih?: string;
  Tarih?: string;
  ad?: string;
  Ad?: string;
  calismaSaati?: number | null;
  CalismaSaati?: number | null;
  [key: string]: any;
};

export const ayarlarService = {
  // Vardiyalar (CalismaSekli)
  async listVardiyalar(opts?: { forceRefresh?: boolean }): Promise<ApiResult<CalismaSekli[]>> {
    return await cachedGet<ApiResult<CalismaSekli[]>>("/CalismaSekli", {
      timeout: 15000,
      keyPrefix: "/CalismaSekli",
      softTtlMs: 5 * 60 * 1000,
      hardTtlMs: 24 * 60 * 60 * 1000,
      forceRefresh: !!opts?.forceRefresh,
    });
  },
  async createVardiya(payload: CalismaSekli): Promise<ApiResult<number>> {
    const resp = await api.post("/CalismaSekli", payload, { timeout: 15000 });
    await invalidate("/CalismaSekli");
    return resp.data;
  },
  async updateVardiya(id: number, payload: CalismaSekli): Promise<ApiResult<any>> {
    const resp = await api.put(`/CalismaSekli/${encodeURIComponent(String(id))}`, payload, { timeout: 15000 });
    await invalidate("/CalismaSekli");
    return resp.data;
  },
  async deleteVardiya(id: number): Promise<ApiResult<any>> {
    const resp = await api.delete(`/CalismaSekli/${encodeURIComponent(String(id))}`, { timeout: 15000 });
    await invalidate("/CalismaSekli");
    return resp.data;
  },

  // Çalışma Statüleri
  async listCalismaStatuleri(opts?: { forceRefresh?: boolean }): Promise<ApiResult<LookupItem[]>> {
    return await cachedGet<ApiResult<LookupItem[]>>("/CalismaStatu", {
      timeout: 15000,
      keyPrefix: "/CalismaStatu",
      softTtlMs: 5 * 60 * 1000,
      hardTtlMs: 24 * 60 * 60 * 1000,
      forceRefresh: !!opts?.forceRefresh,
    });
  },
  async createCalismaStatu(ad: string): Promise<ApiResult<any>> {
    const resp = await api.post("/CalismaStatu", { Ad: ad }, { timeout: 15000 });
    await invalidate("/CalismaStatu");
    return resp.data;
  },
  async updateCalismaStatu(id: number, ad: string): Promise<ApiResult<any>> {
    const resp = await api.put(`/CalismaStatu/${encodeURIComponent(String(id))}`, { Ad: ad }, { timeout: 15000 });
    await invalidate("/CalismaStatu");
    return resp.data;
  },
  async deleteCalismaStatu(id: number): Promise<ApiResult<any>> {
    const resp = await api.delete(`/CalismaStatu/${encodeURIComponent(String(id))}`, { timeout: 15000 });
    await invalidate("/CalismaStatu");
    return resp.data;
  },

  // Cihazlar
  async listCihazlar(params?: { sadeceAktif?: boolean }, opts?: { forceRefresh?: boolean }): Promise<ApiResult<CihazListDTO[]>> {
    // consider this a "lookup-ish" list; keep TTL shorter since it's operational
    return await cachedGet<ApiResult<CihazListDTO[]>>("/Cihaz", {
      params,
      timeout: 15000,
      keyPrefix: "/Cihaz",
      softTtlMs: 60 * 1000,
      hardTtlMs: 10 * 60 * 1000,
      forceRefresh: !!opts?.forceRefresh,
    });
  },
  async cihazTipleri(opts?: { forceRefresh?: boolean }): Promise<ApiResult<CihazTip[]>> {
    return await cachedGet<ApiResult<CihazTip[]>>("/Cihaz/tipler", {
      timeout: 15000,
      keyPrefix: "/Cihaz/tipler",
      softTtlMs: 24 * 60 * 60 * 1000,
      hardTtlMs: 7 * 24 * 60 * 60 * 1000,
      forceRefresh: !!opts?.forceRefresh,
    });
  },
  async getCihaz(id: number): Promise<ApiResult<Cihaz>> {
    const resp = await api.get(`/Cihaz/${encodeURIComponent(String(id))}`, { timeout: 15000 });
    return resp.data;
  },
  async createCihaz(payload: Cihaz): Promise<ApiResult<number>> {
    const resp = await api.post("/Cihaz", payload, { timeout: 15000 });
    await invalidate("/Cihaz");
    return resp.data;
  },
  async updateCihaz(id: number, payload: Cihaz): Promise<ApiResult<any>> {
    const resp = await api.put(`/Cihaz/${encodeURIComponent(String(id))}`, payload, { timeout: 15000 });
    await invalidate("/Cihaz");
    return resp.data;
  },
  async pasifCihaz(id: number): Promise<ApiResult<any>> {
    const resp = await api.delete(`/Cihaz/${encodeURIComponent(String(id))}`, { timeout: 15000 });
    await invalidate("/Cihaz");
    return resp.data;
  },
  async aktifCihaz(id: number): Promise<ApiResult<any>> {
    const resp = await api.post(`/Cihaz/${encodeURIComponent(String(id))}/aktif`, {}, { timeout: 15000 });
    await invalidate("/Cihaz");
    return resp.data;
  },

  // Resmi tatiller
  async listResmiTatiller(params?: { yil?: number }): Promise<ApiResult<ResmiTatilDTO[]>> {
    const resp = await api.get("/ResmiTatil", { params, timeout: 15000 });
    return resp.data;
  },
  async saveResmiTatil(payload: { tarih: string; ad: string; calismaSaat?: number | null }): Promise<ApiResult<any>> {
    // API expects DateTime. We send ISO string.
    const resp = await api.post(
      "/ResmiTatil",
      { Tarih: payload.tarih, Ad: payload.ad, CalismaSaat: payload.calismaSaat ?? null },
      { timeout: 15000 }
    );
    return resp.data;
  },
  async doldurSabit(payload: { baslangicYili: number; bitisYili: number }): Promise<ApiResult<any>> {
    const resp = await api.post(
      "/ResmiTatil/doldur-sabit",
      { BaslangicYili: payload.baslangicYili, BitisYili: payload.bitisYili },
      { timeout: 15000 }
    );
    return resp.data;
  },
};

