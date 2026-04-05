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

