import { AppState, AppStateStatus } from "react-native";
import { useEffect, useMemo, useRef, useState } from "react";
import { notificationService } from "../services/notificationApi";

export type NotificationItem = {
  id: number;
  baslik?: string | null;
  mesaj?: string | null;
  okunduMu: boolean;
  tarih?: string | null;
  tipi?: string | null;
};

function asInt(v: any, def = 0) {
  const n = Number(v);
  return Number.isFinite(n) ? n : def;
}

export function useNotifications(opts?: {
  pollMs?: number;
  onNewNotification?: (n: NotificationItem) => void;
}) {
  const pollMs = asInt(opts?.pollMs, 15000) || 15000;

  const [unreadCount, setUnreadCount] = useState(0);
  const [items, setItems] = useState<NotificationItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const lastTopIdRef = useRef<number>(0);
  const intervalRef = useRef<any>(null);
  const appStateRef = useRef<AppStateStatus>(AppState.currentState);

  const fetchNow = async () => {
    setLoading(true);
    setError(null);
    try {
      const [uc, hist] = await Promise.all([notificationService.unreadCount(), notificationService.history(1, 10)]);
      if (uc?.success) setUnreadCount(asInt(uc.data, 0));
      if (hist?.success) {
        const list = (hist.data?.items ?? []) as any[];
        const mapped = list.map((x) => ({
          id: asInt(x?.id ?? x?.Id, 0),
          baslik: (x?.baslik ?? x?.Baslik ?? null) as any,
          mesaj: (x?.mesaj ?? x?.Mesaj ?? null) as any,
          okunduMu: !!(x?.okunduMu ?? x?.OkunduMu),
          tarih: (x?.tarih ?? x?.Tarih ?? null) as any,
          tipi: (x?.tipi ?? x?.Tipi ?? null) as any,
        }));
        setItems(mapped);

        const topId = mapped[0]?.id ?? 0;
        if (topId && topId !== lastTopIdRef.current) {
          // Fire only when app is active and we already had a baseline.
          if (lastTopIdRef.current && appStateRef.current === "active") {
            opts?.onNewNotification?.(mapped[0]);
          }
          lastTopIdRef.current = topId;
        }
      }
    } catch (e: any) {
      setError(e?.response?.data?.message ?? e?.message ?? "Bildirimler alınamadı.");
    } finally {
      setLoading(false);
    }
  };

  const stopPolling = () => {
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }
  };

  const startPolling = () => {
    stopPolling();
    intervalRef.current = setInterval(() => {
      fetchNow().catch(() => {});
    }, pollMs);
  };

  useEffect(() => {
    let alive = true;
    (async () => {
      if (!alive) return;
      await fetchNow();
      if (!alive) return;
      startPolling();
    })();

    const sub = AppState.addEventListener("change", (next) => {
      appStateRef.current = next;
      if (next === "active") {
        fetchNow().catch(() => {});
        startPolling();
      } else {
        stopPolling();
      }
    });

    return () => {
      alive = false;
      stopPolling();
      sub.remove();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const actions = useMemo(
    () => ({
      refresh: fetchNow,
      markAsRead: async (id: number) => {
        await notificationService.markAsRead(id);
        await fetchNow();
      },
      markAllAsRead: async () => {
        await notificationService.markAllAsRead();
        await fetchNow();
      },
    }),
    []
  );

  return { unreadCount, items, loading, error, actions };
}

