import api, { cachedGet } from "./api";

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

export type RaporTanimi = {
  raporAdi?: string;
  RaporAdi?: string;
  procedureAdi?: string;
  ProcedureAdi?: string;
  aktifMi?: boolean;
  AktifMi?: boolean;
};

export type ReportTable = {
  columns?: string[];
  Columns?: string[];
  rows?: (string | null)[][];
  Rows?: (string | null)[][];
};

export const raporService = {
  async list(): Promise<ApiResult<RaporTanimi[]>> {
    return await cachedGet<ApiResult<RaporTanimi[]>>("/Rapor/list", {
      timeout: 15000,
      keyPrefix: "/Rapor/list",
      softTtlMs: 10 * 60 * 1000,
      hardTtlMs: 24 * 60 * 60 * 1000,
    });
  },

  async run(payload: {
    procedureAdi: string;
    firmaId?: number | null;
    tarihBaslangic: string; // yyyy-MM-dd
    tarihBitis: string; // yyyy-MM-dd
    page?: number;
    pageSize?: number;
  }): Promise<ApiResult<PagedResponse<ReportTable>>> {
    const response = await api.post("/Rapor/run", payload, { timeout: 30000 });
    return response.data;
  },

  async export(payload: {
    procedureName: string;
    exportTitle: string;
    format: "pdf" | "excel";
    params: Record<string, any>;
  }): Promise<any> {
    // returns binary file
    const response = await api.post("/Rapor/export", payload, {
      timeout: 60000,
      responseType: "arraybuffer",
    });
    return response;
  },
};

