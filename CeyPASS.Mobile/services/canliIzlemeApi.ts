import axios from "axios";
import { getApiBaseUrl } from "./api";

type ApiResult<T> = { success: boolean; message?: string; data?: T };

function createClient(token?: string) {
  const client = axios.create({
    baseURL: getApiBaseUrl(),
    headers: { "Content-Type": "application/json" },
  });
  if (token) client.defaults.headers.common.Authorization = `Bearer ${token}`;
  return client;
}

export type FirmaOption = { id: number; ad: string };

export type KartListItem = { personelId: string; adSoyad: string };

export type KartAtamaItem = {
  atamaId: number;
  kartId: string;
  adSoyad: string;
  tcKimlikNo?: string | null;
  ziyaretEdilenKisi?: string | null;
  plaka?: string | null;
  kartAdi?: string | null;
  baslangic: string;
  bitis?: string | null;
  notlar?: string | null;
};

export type GecmisZiyaretci = {
  adSoyad: string;
  tcKimlikNo?: string | null;
  ziyaretEdilenKisi?: string | null;
  plaka?: string | null;
  sonZiyaret?: string;
  gosterim?: string | null;
};

export type KartCreateBody = {
  personelId: string;
  adSoyad: string;
  girisSaati: string;
  aciklama?: string;
  tcKimlikNo?: string;
  ziyaretEdilenKisi?: string;
  plaka?: string;
};

export type KartUpdateBody = {
  adSoyad: string;
  girisSaati: string;
  cikisSaati?: string | null;
  aciklama?: string;
  tcKimlikNo?: string;
  ziyaretEdilenKisi?: string;
  plaka?: string;
};

export const canliIzlemeAuth = {
  async firmalar() {
    const res = await createClient().get<ApiResult<FirmaOption[]>>("/CanliIzleme/firmalar");
    return res.data;
  },
  async kullanicilar(firmaId: number) {
    const res = await createClient().get<ApiResult<string[]>>("/CanliIzleme/kullanicilar", { params: { firmaId } });
    return res.data;
  },
  async login(firmaId: number, kullaniciAdi: string, sifre: string) {
    const res = await createClient().post<ApiResult<{ token: string; expiration: string; user: any }>>(
      "/CanliIzleme/login",
      { firmaId, kullaniciAdi, sifre }
    );
    return res.data;
  },
};

export const canliIzlemeData = {
  async sonGecisler(token: string, take = 4) {
    const res = await createClient(token).get<ApiResult<any[]>>("/CanliIzleme/son-gecisler", { params: { take } });
    return res.data;
  },
  async sonHareketler(token: string, take = 10) {
    const res = await createClient(token).get<ApiResult<any[]>>("/CanliIzleme/son-hareketler", { params: { take } });
    return res.data;
  },
  async kisiDetay(token: string, kisiId: number) {
    const res = await createClient(token).get<ApiResult<any>>("/CanliIzleme/kisi-detay", { params: { kisiId } });
    return res.data;
  },
};

function kartBase(kind: "misafir" | "arac") {
  return kind === "misafir" ? "/CanliIzleme/misafir-kart" : "/CanliIzleme/arac-kart";
}

export const canliIzlemeKart = {
  async kartlar(token: string, kind: "misafir" | "arac") {
    const res = await createClient(token).get<ApiResult<KartListItem[]>>(`${kartBase(kind)}/kartlar`);
    return res.data;
  },
  async aktif(token: string, kind: "misafir" | "arac") {
    const res = await createClient(token).get<ApiResult<KartAtamaItem[]>>(`${kartBase(kind)}/aktif`);
    return res.data;
  },
  async create(token: string, kind: "misafir" | "arac", body: KartCreateBody) {
    const res = await createClient(token).post<ApiResult<{ atamaId: number }>>(kartBase(kind), body);
    return res.data;
  },
  async update(token: string, kind: "misafir" | "arac", id: number, body: KartUpdateBody) {
    const res = await createClient(token).put<ApiResult<object>>(`${kartBase(kind)}/${id}`, body);
    return res.data;
  },
  async byTc(token: string, kind: "misafir" | "arac", tc: string) {
    const res = await createClient(token).get<ApiResult<{
      adSoyad?: string;
      tcKimlikNo?: string;
      ziyaretEdilenKisi?: string;
      plaka?: string;
      aciklama?: string;
    } | null>>(`${kartBase(kind)}/by-tc`, { params: { tc } });
    return res.data;
  },
  async gecmis(token: string, kind: "misafir" | "arac", ad?: string) {
    const res = await createClient(token).get<ApiResult<GecmisZiyaretci[]>>(`${kartBase(kind)}/gecmis`, {
      params: { ad: ad || "" },
    });
    return res.data;
  },
};
