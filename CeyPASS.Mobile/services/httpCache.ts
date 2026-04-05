import * as SecureStore from "expo-secure-store";

export type CacheEntry<T = any> = {
  fetchedAt: number; // epoch ms
  softTtlMs: number;
  hardTtlMs: number;
  data: T;
};

const KEY_PREFIX = "ceypass.httpcache.v1.";
const INDEX_KEY = "ceypass.httpcache.index.v1";

const mem = new Map<string, CacheEntry<any>>();
const inflight = new Map<string, Promise<any>>();

function now() {
  return Date.now();
}

function isFreshSoft(e: CacheEntry<any>) {
  return now() - e.fetchedAt <= e.softTtlMs;
}
function isFreshHard(e: CacheEntry<any>) {
  return now() - e.fetchedAt <= e.hardTtlMs;
}

function hashKey(input: string) {
  // Stable short hash (djb2) for SecureStore key length.
  let h = 5381;
  for (let i = 0; i < input.length; i++) h = ((h << 5) + h) ^ input.charCodeAt(i);
  return (h >>> 0).toString(16);
}

function toStoreKey(rawKey: string) {
  return `${KEY_PREFIX}${hashKey(rawKey)}`;
}

async function loadIndex(): Promise<Record<string, string[]>> {
  const raw = await SecureStore.getItemAsync(INDEX_KEY);
  if (!raw) return {};
  try {
    const parsed = JSON.parse(raw);
    if (!parsed || typeof parsed !== "object") return {};
    return parsed as Record<string, string[]>;
  } catch {
    return {};
  }
}

async function saveIndex(idx: Record<string, string[]>) {
  await SecureStore.setItemAsync(INDEX_KEY, JSON.stringify(idx));
}

async function indexAdd(prefix: string, rawKey: string) {
  if (!prefix) return;
  const idx = await loadIndex();
  const arr = Array.isArray(idx[prefix]) ? idx[prefix] : [];
  if (!arr.includes(rawKey)) arr.push(rawKey);
  idx[prefix] = arr;
  await saveIndex(idx);
}

export async function getCached<T>(rawKey: string): Promise<CacheEntry<T> | null> {
  if (!rawKey) return null;
  const inMem = mem.get(rawKey);
  if (inMem) return inMem as CacheEntry<T>;

  const stored = await SecureStore.getItemAsync(toStoreKey(rawKey));
  if (!stored) return null;
  try {
    const parsed = JSON.parse(stored) as CacheEntry<T>;
    if (!parsed || typeof parsed !== "object" || typeof parsed.fetchedAt !== "number") return null;
    mem.set(rawKey, parsed as any);
    return parsed;
  } catch {
    return null;
  }
}

export async function setCached<T>(rawKey: string, entry: CacheEntry<T>, opts?: { prefix?: string }) {
  mem.set(rawKey, entry as any);
  await SecureStore.setItemAsync(toStoreKey(rawKey), JSON.stringify(entry));
  if (opts?.prefix) await indexAdd(opts.prefix, rawKey);
}

export function withInflight<T>(rawKey: string, fn: () => Promise<T>): Promise<T> {
  const prev = inflight.get(rawKey);
  if (prev) return prev as Promise<T>;
  const p = fn().finally(() => {
    inflight.delete(rawKey);
  });
  inflight.set(rawKey, p as any);
  return p;
}

export async function invalidate(prefix: string) {
  if (!prefix) return;
  const idx = await loadIndex();
  const keys = Array.isArray(idx[prefix]) ? idx[prefix] : [];
  if (!keys.length) return;

  for (const rawKey of keys) {
    mem.delete(rawKey);
    try {
      await SecureStore.deleteItemAsync(toStoreKey(rawKey));
    } catch {
      // ignore
    }
  }
  delete idx[prefix];
  await saveIndex(idx);
}

export async function resolveCached<T>(args: {
  rawKey: string;
  prefix?: string;
  softTtlMs: number;
  hardTtlMs: number;
  forceRefresh?: boolean;
  fetcher: () => Promise<T>;
}): Promise<{ data: T; fromCache: boolean; revalidated: boolean }> {
  const { rawKey, prefix, softTtlMs, hardTtlMs, forceRefresh, fetcher } = args;

  if (!forceRefresh) {
    const cached = await getCached<T>(rawKey);
    if (cached && isFreshHard(cached)) {
      // stale-while-revalidate (soft TTL)
      if (!isFreshSoft(cached)) {
        // fire-and-forget refresh
        withInflight(rawKey, async () => {
          try {
            const fresh = await fetcher();
            await setCached(rawKey, { fetchedAt: now(), softTtlMs, hardTtlMs, data: fresh }, { prefix });
          } catch {
            // ignore background failures
          }
          return true as any;
        });
        return { data: cached.data, fromCache: true, revalidated: true };
      }
      return { data: cached.data, fromCache: true, revalidated: false };
    }
  }

  // hard-expired or force refresh: await fresh
  const data = await withInflight(rawKey, fetcher);
  await setCached(rawKey, { fetchedAt: now(), softTtlMs, hardTtlMs, data }, { prefix });
  return { data, fromCache: false, revalidated: false };
}

