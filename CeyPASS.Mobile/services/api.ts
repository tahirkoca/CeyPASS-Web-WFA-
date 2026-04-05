import axios from "axios";
import { loadSession } from "./session";
import Constants from "expo-constants";
import { resolveCached } from "./httpCache";

function normalizeBaseUrl(url: string) {
  const trimmed = url.trim().replace(/\/+$/, "");
  return trimmed.endsWith("/api/v1") ? trimmed : `${trimmed}/api/v1`;
}

/** Kestrel HTTP (launchSettings: http://0.0.0.0:5126) — fiziksel cihaz / emülatör LAN için. */
const DEV_API_HTTP_PORT = 5126;

function isLocalhostUrl(url: string): boolean {
  try {
    const u = new URL(url);
    const h = u.hostname.toLowerCase();
    return h === "localhost" || h === "127.0.0.1" || h === "::1";
  } catch {
    return /localhost|127\.0\.0\.1/i.test(url);
  }
}

/**
 * Expo Go / Metro geliştirme oturumunda packager'ın bağlı olduğu makine (PC IP veya Android emülatör 10.0.2.2).
 */
function getPackagerHost(): string | null {
  const c = Constants as any;
  const raw =
    (typeof c.expoConfig?.hostUri === "string" && c.expoConfig.hostUri) ||
    (typeof c.expoGoConfig?.debuggerHost === "string" && c.expoGoConfig.debuggerHost) ||
    (typeof c.manifest?.debuggerHost === "string" && c.manifest.debuggerHost) ||
    null;
  if (!raw) return null;
  const host = raw.split(":")[0]?.trim();
  if (!host || host === "localhost" || host === "127.0.0.1") return null;
  return host;
}

function devLanHttpApiBase(): string | null {
  if (typeof document !== "undefined") return null;
  const host = getPackagerHost();
  if (!host) return null;
  return `http://${host}:${DEV_API_HTTP_PORT}/api/v1`;
}

/**
 * Configure via env:
 * - EXPO_PUBLIC_API_BASE_URL=https://192.168.1.10:7061   (LAN)
 * - EXPO_PUBLIC_API_BASE_URL=https://my-dev-api.example  (tunnel-friendly)
 *
 * Notes:
 * - On a physical phone, "localhost" points to the phone itself, not your PC.
 * - If app.json uses localhost but you run in Expo Go, we resolve the dev PC IP from Metro and use HTTP :5126.
 * - Self-signed HTTPS on local dev can fail on devices unless trusted.
 */
export function getApiBaseUrl(): string {
  const envBase = process.env.EXPO_PUBLIC_API_BASE_URL;
  if (envBase) return normalizeBaseUrl(envBase);

  const extraBase = (Constants as any)?.expoConfig?.extra?.apiBaseUrl as string | undefined;

  // Native + extra localhost: use same machine as Metro (Expo Go QR'daki IP) + API HTTP port.
  if (extraBase && isLocalhostUrl(extraBase)) {
    const lan = devLanHttpApiBase();
    if (lan) return lan;
  }

  if (extraBase) return normalizeBaseUrl(extraBase);

  // Web dev: keep prior behavior.
  if (typeof document !== "undefined") {
    return "https://localhost:7061/api/v1";
  }

  const lan = devLanHttpApiBase();
  if (lan) return lan;

  throw new Error(
    "API base URL ayarlı değil. Fiziksel cihazda 'localhost' çalışmaz. " +
      "Çözüm: (1) EXPO_PUBLIC_API_BASE_URL, veya (2) Expo Go ile aynı Wi‑Fi'de API'yi çalıştırın (otomatik LAN), " +
      "veya (3) `app.json` → expo.extra.apiBaseUrl ile tam URL verin."
  );
}

const api = axios.create({
  baseURL: undefined,
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use((config) => {
  if (!config.baseURL) config.baseURL = getApiBaseUrl();
  return config;
});

let inMemoryToken: string | null = null;

export function setAuthToken(token: string | null) {
  inMemoryToken = token;
}

export async function getAuthToken(): Promise<string | null> {
  if (inMemoryToken) return inMemoryToken;
  try {
    const s = await loadSession();
    inMemoryToken = s?.token ?? null;
    return inMemoryToken;
  } catch {
    return null;
  }
}

api.interceptors.request.use(async (config) => {
  if (!config.baseURL) config.baseURL = getApiBaseUrl();

  // attach JWT if available
  if (!inMemoryToken) {
    const s = await loadSession();
    inMemoryToken = s?.token ?? null;
  }
  if (inMemoryToken) {
    config.headers = config.headers ?? {};
    (config.headers as any).Authorization = `Bearer ${inMemoryToken}`;
  }
  return config;
});

export const authService = {
  login: async (username: string, password: string) => {
    const response = await api.post("/Auth/login", {
      username,
      password,
    });
    return response.data;
  },
  abilities: async () => {
    const response = await api.get("/Auth/abilities");
    return response.data;
  },
  forgotPasswordStart: async (username: string) => {
    const response = await api.post("/Auth/forgot-password", { username });
    return response.data;
  },
  forgotPasswordConfirm: async (username: string, kod: string, yeniSifre: string, yeniSifreTekrar: string) => {
    const response = await api.post("/Auth/forgot-password/confirm", {
      username,
      kod,
      yeniSifre,
      yeniSifreTekrar,
    });
    return response.data;
  },
};

export const dashboardService = {
  getFullDashboard: async () => {
    const response = await api.get("/Dashboard/full");
    return response.data;
  },
};

function stableStringify(obj: any): string {
  if (!obj || typeof obj !== "object") return String(obj ?? "");
  if (Array.isArray(obj)) return `[${obj.map((x) => stableStringify(x)).join(",")}]`;
  const keys = Object.keys(obj).sort();
  return `{${keys.map((k) => `${k}:${stableStringify(obj[k])}`).join(",")}}`;
}

type CachedGetOptions = {
  params?: Record<string, any>;
  /** soft TTL: stale-while-revalidate window */
  softTtlMs?: number;
  /** hard TTL: if expired, block and refetch */
  hardTtlMs?: number;
  /** invalidate group */
  keyPrefix?: string;
  /** bypass cache */
  forceRefresh?: boolean;
  /** request timeout override */
  timeout?: number;
};

export async function cachedGet<T = any>(path: string, opts?: CachedGetOptions): Promise<T> {
  const baseURL = getApiBaseUrl();
  const params = opts?.params ?? undefined;
  const rawKey = `${baseURL}${path}?${stableStringify(params ?? {})}`;
  const softTtlMs = opts?.softTtlMs ?? 5 * 60 * 1000;
  const hardTtlMs = opts?.hardTtlMs ?? 24 * 60 * 60 * 1000;
  const prefix = opts?.keyPrefix ?? path;

  const res = await resolveCached<T>({
    rawKey,
    prefix,
    softTtlMs,
    hardTtlMs,
    forceRefresh: opts?.forceRefresh,
    fetcher: async () => {
      const response = await api.get(path, { params, timeout: opts?.timeout });
      return response.data as T;
    },
  });
  return res.data;
}

export default api;
