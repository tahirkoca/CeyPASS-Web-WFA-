import api from "./api";

export type ApiResult<T> = { success: boolean; message?: string; data?: T };

export const deviceTokenService = {
  async register(token: string, deviceType?: string): Promise<ApiResult<any>> {
    const res = await api.post("/DeviceToken/register", { token, deviceType: deviceType || undefined });
    return res.data;
  },
  async unregister(token: string): Promise<ApiResult<any>> {
    const res = await api.post("/DeviceToken/unregister", token, { headers: { "Content-Type": "application/json" } });
    return res.data;
  },
};

