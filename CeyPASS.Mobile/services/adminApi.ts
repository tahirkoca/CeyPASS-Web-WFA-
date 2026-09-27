/** Süper admin: sürüm duyurusu e-postası API. */
import api from "./api";

export type ApiResult<T = any> = {
  success: boolean;
  message?: string;
  data?: T;
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

/** /Admin guncelleme-mail preview ve send. */
export const adminService = {
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
