import * as SecureStore from "expo-secure-store";

export type PageFilterPrefs = {
  firmaId?: number | null;
  isyeriId?: number | null;
  boolA?: boolean | null;
  boolB?: boolean | null;
  extra?: string | null;
  dateA?: string | Date | null;
  dateB?: string | Date | null;
};

export type PageFilterKey = "Personeller" | "Raporlar" | "AylikPuantaj" | "KisiHareketler";

const KEY_PREFIX = "ceypass.filter.";

function storeKey(pageKey: string) {
  return `${KEY_PREFIX}${pageKey}`;
}

function toIsoDate(v: Date | string | null | undefined): string | null {
  if (v == null) return null;
  if (typeof v === "string") {
    const d = new Date(v);
    if (Number.isNaN(d.getTime())) return v;
    return d.toISOString();
  }
  if (Number.isNaN(v.getTime())) return null;
  return v.toISOString();
}

export function parsePrefDate(iso: string | Date | null | undefined): Date | null {
  if (iso == null) return null;
  if (iso instanceof Date) return Number.isNaN(iso.getTime()) ? null : iso;
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? null : d;
}

export async function load(pageKey: PageFilterKey): Promise<PageFilterPrefs | null> {
  try {
    const raw = await SecureStore.getItemAsync(storeKey(pageKey));
    if (!raw) return null;
    const parsed = JSON.parse(raw);
    if (!parsed || typeof parsed !== "object") return null;
    return parsed as PageFilterPrefs;
  } catch {
    return null;
  }
}

export async function save(pageKey: PageFilterKey, prefs: PageFilterPrefs): Promise<void> {
  try {
    const payload: PageFilterPrefs = {
      firmaId: prefs.firmaId ?? null,
      isyeriId: prefs.isyeriId ?? null,
      boolA: prefs.boolA ?? null,
      boolB: prefs.boolB ?? null,
      extra: prefs.extra ?? null,
      dateA: toIsoDate(prefs.dateA),
      dateB: toIsoDate(prefs.dateB),
    };
    await SecureStore.setItemAsync(storeKey(pageKey), JSON.stringify(payload));
  } catch {
    // best-effort persistence
  }
}

export const pageFilterPrefs = { load, save, parsePrefDate };
