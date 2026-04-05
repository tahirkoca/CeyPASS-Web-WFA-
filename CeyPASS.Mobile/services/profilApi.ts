import api from "./api";

export type ApiResult<T> = { success: boolean; message?: string; data?: T };

export type ProfilModel = {
  sicilNo: string;
  personel: any;
  fotografDataUrl?: string | null;
  departmanAdi?: string | null;
  pozisyonAdi?: string | null;
  calismaSekliAdlari?: string[] | null;
  yemekHakkiVar?: boolean | null;
  gunlukYemekAdedi?: number | null;
  isSupervisor: boolean;
  hasPendingLeaves: boolean;
  totalPendingAdvances: number;
};

export type IzinlerimResponse = {
  talepler: any[];
  onayliIzinler: any[];
};

export type HareketlerimResponse = {
  items: any[];
  totalCount: number;
  page: number;
  pageSize: number;
};

export type NotificationHistoryResponse = {
  items: Array<{
    id: number;
    baslik?: string | null;
    mesaj?: string | null;
    okunduMu: boolean;
    tarih?: string | null;
    tipi?: string | null;
  }>;
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
};

export const profilService = {
  async getProfil(): Promise<ApiResult<ProfilModel>> {
    const res = await api.get("/Profil");
    return res.data;
  },
  async izinlerim(): Promise<ApiResult<IzinlerimResponse>> {
    const res = await api.get("/Profil/izinlerim");
    return res.data;
  },
  async avanslarim(): Promise<ApiResult<any[]>> {
    const res = await api.get("/Profil/avanslarim");
    return res.data;
  },
  async hareketlerim(page = 1, pageSize = 10): Promise<ApiResult<HareketlerimResponse>> {
    const res = await api.get("/Profil/hareketlerim", { params: { page, pageSize } });
    return res.data;
  },
  async bildirimGecmisi(page = 1, pageSize = 10): Promise<ApiResult<NotificationHistoryResponse>> {
    const res = await api.get("/Notification/history", { params: { page, pageSize } });
    return res.data;
  },
  async izinTipleri(): Promise<ApiResult<any[]>> {
    const res = await api.get("/Profil/izin-tipleri");
    return res.data;
  },
  async izinTalep(payload: {
    izinTipId: number;
    baslangic: string; // ISO
    bitis: string; // ISO
    saatlikIzinMi: boolean;
    aciklama?: string;
    izinAdres?: string;
    telefonNo?: string;
  }): Promise<ApiResult<any>> {
    const res = await api.post("/Profil/izin-talep", payload);
    return res.data;
  },
  async kullanimImzaAt(talepId: number): Promise<ApiResult<any>> {
    const res = await api.post("/Profil/kullanim-imza", { talepId });
    return res.data;
  },
  async avansTalep(payload: { miktar: number; aciklama?: string }): Promise<ApiResult<number>> {
    const res = await api.post("/Profil/avans-talep", payload);
    return res.data;
  },
  async avansGuncelle(payload: { avansId: number; miktar: number; aciklama?: string }): Promise<ApiResult<any>> {
    const res = await api.post("/Profil/avans-guncelle", payload);
    return res.data;
  },
  async avansIptal(avansId: number): Promise<ApiResult<any>> {
    const res = await api.post("/Profil/avans-iptal", { avansId });
    return res.data;
  },
  async amirBekleyenler(): Promise<ApiResult<any[]>> {
    const res = await api.get("/Profil/amir-onay-bekleyenler");
    return res.data;
  },
  async amirOnayla(talepId: number, aciklama?: string): Promise<ApiResult<any>> {
    const res = await api.post("/Profil/amir-onayla", { talepId, aciklama: aciklama || undefined });
    return res.data;
  },
  async amirReddet(talepId: number, aciklama?: string): Promise<ApiResult<any>> {
    const res = await api.post("/Profil/amir-reddet", { talepId, aciklama: aciklama || undefined });
    return res.data;
  },
  async bagliPersonellerim(): Promise<ApiResult<any[]>> {
    const res = await api.get("/Profil/bagli-personellerim");
    return res.data;
  },
  async subordinateSifreSifirla(personelId: string, yeniSifre: string): Promise<ApiResult<any>> {
    const res = await api.post("/Profil/subordinate-sifre-sifirla", { personelId, yeniSifre });
    return res.data;
  },
  async sifreDegistir(eskiSifre: string, yeniSifre: string): Promise<ApiResult<any>> {
    const res = await api.post("/Profil/sifre-degistir", { eskiSifre, yeniSifre });
    return res.data;
  },

  // --- Kurumsal Talepler (Web: İzin Talepleri / Avans Talepleri) ---
  async izinTalepleri(): Promise<ApiResult<any[]>> {
    const res = await api.get("/Izin/talepler");
    return res.data;
  },
  async izinTalepOnayla(talepId: number, aciklama?: string): Promise<ApiResult<any>> {
    const res = await api.post(`/Izin/onayla/${talepId}`, { aciklama: aciklama || undefined });
    return res.data;
  },
  async izinTalepReddet(talepId: number, aciklama?: string): Promise<ApiResult<any>> {
    const res = await api.post(`/Izin/reddet/${talepId}`, { aciklama: aciklama || undefined });
    return res.data;
  },
  async izinDonusImzasinaAc(talepId: number): Promise<ApiResult<any>> {
    const res = await api.post(`/Izin/donus-imzasina-ac/${talepId}`);
    return res.data;
  },

  async avansTalepleri(): Promise<ApiResult<any[]>> {
    const res = await api.get("/Avans");
    return res.data;
  },
  async avansOnayla(avansId: number, aciklama?: string): Promise<ApiResult<any>> {
    const res = await api.post("/Avans/onayla", { avansId, aciklama: aciklama || undefined });
    return res.data;
  },
  async avansReddet(avansId: number, aciklama?: string): Promise<ApiResult<any>> {
    const res = await api.post("/Avans/reddet", { avansId, aciklama: aciklama || undefined });
    return res.data;
  },
};

