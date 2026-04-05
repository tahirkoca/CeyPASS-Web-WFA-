import api from "./api";

export type ApiResult<T> = { success: boolean; message?: string; data?: T };

export type NotificationHistoryItem = {
  id: number;
  baslik?: string | null;
  mesaj?: string | null;
  okunduMu: boolean;
  tarih?: string | null;
  tipi?: string | null;
};

export type NotificationHistoryResponse = {
  items: NotificationHistoryItem[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
};

export const notificationService = {
  async unreadCount(): Promise<ApiResult<number>> {
    const res = await api.get("/Notification/unread-count");
    return res.data;
  },
  async history(page = 1, pageSize = 10): Promise<ApiResult<NotificationHistoryResponse>> {
    const res = await api.get("/Notification/history", { params: { page, pageSize } });
    return res.data;
  },
  async markAsRead(id: number): Promise<ApiResult<any>> {
    const res = await api.post(`/Notification/read/${id}`);
    return res.data;
  },
  async markAllAsRead(): Promise<ApiResult<any>> {
    const res = await api.post("/Notification/read-all");
    return res.data;
  },
};

