/** SecureStore’da JWT, kullanıcı ve yetki (abilities) kalıcılığı. */
import * as SecureStore from "expo-secure-store";

/** Oturum anlık görüntüsü (token + menü yetkileri). */
export type SavedSession = {
  token: string;
  user: any;
  username: string;
  abilities?: {
    view: Record<string, boolean>;
    actions?: Record<string, Record<string, boolean>>;
    isSupervisor: boolean;
    rolId?: number | null;
    rolAdi?: string | null;
  };
  savedAt: number;
};

const KEY = "ceypass.session.v1";

/** Giriş sonrası oturumu cihazda sakla. */
export async function saveSession(session: Omit<SavedSession, "savedAt">) {
  const payload: SavedSession = { ...session, savedAt: Date.now() };
  await SecureStore.setItemAsync(KEY, JSON.stringify(payload));
}

/** Kayıtlı oturumu oku; bozuk JSON null döner. */
export async function loadSession(): Promise<SavedSession | null> {
  const raw = await SecureStore.getItemAsync(KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as SavedSession;
  } catch {
    return null;
  }
}

/** Çıkış: SecureStore oturum anahtarını sil. */
export async function clearSession() {
  await SecureStore.deleteItemAsync(KEY);
}
