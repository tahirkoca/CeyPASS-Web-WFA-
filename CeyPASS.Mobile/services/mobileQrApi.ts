import api from "./api";

export const mobileQrApi = {
  okut: async (payload: { cihazId: number; enlem?: number; boylam?: number; isMocked?: boolean }) => {
    const response = await api.post("/MobileQr/Okut", payload);
    return response.data;
  },
};
