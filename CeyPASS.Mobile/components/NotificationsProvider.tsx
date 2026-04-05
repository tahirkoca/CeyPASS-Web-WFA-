import React, { createContext, useContext, useMemo, useState } from "react";
import { StatusPopup } from "./StatusPopup";
import { useNotifications, NotificationItem } from "./useNotifications";

type Ctx = {
  unreadCount: number;
  topItems: NotificationItem[];
  refresh: () => Promise<void>;
};

const NotificationsContext = createContext<Ctx | null>(null);

export function NotificationsProvider(props: { children: React.ReactNode }) {
  const [toastVisible, setToastVisible] = useState(false);
  const [toastMessage, setToastMessage] = useState("");

  const n = useNotifications({
    pollMs: 15000,
    onNewNotification: (x) => {
      setToastMessage(((x?.baslik ?? "Yeni Bildirim") as any)?.toString?.() ?? "Yeni Bildirim");
      setToastVisible(true);
      setTimeout(() => setToastVisible(false), 1500);
    },
  });

  const value = useMemo<Ctx>(
    () => ({
      unreadCount: n.unreadCount,
      topItems: n.items,
      refresh: async () => {
        await n.actions.refresh();
      },
    }),
    [n.unreadCount, n.items, n.actions]
  );

  return (
    <NotificationsContext.Provider value={value}>
      {props.children}
      <StatusPopup
        visible={toastVisible}
        type="success"
        message={toastMessage}
        onClose={() => setToastVisible(false)}
        useModal={false}
        autoCloseMs={1500}
      />
    </NotificationsContext.Provider>
  );
}

export function useNotificationsContext() {
  const ctx = useContext(NotificationsContext);
  if (!ctx) throw new Error("useNotificationsContext must be used within NotificationsProvider");
  return ctx;
}

