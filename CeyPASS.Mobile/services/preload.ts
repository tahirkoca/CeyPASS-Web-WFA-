/** Giriş ekranı arka plan görseli ve erken yükleme. */
import { Asset } from "expo-asset";

/** Login / Canlı İzleme arka plan PNG modülü. */
export const LoginBackground = require("../assets/ceyport-tekirdag.png");

let loginBgPromise: Promise<void> | null = null;

/** İlk açılışta arka planı indir (tek seferlik promise). */
export function preloadLoginBackground(): Promise<void> {
  if (!loginBgPromise) {
    loginBgPromise = Asset.fromModule(LoginBackground)
      .downloadAsync()
      .then(() => undefined)
      .catch(() => undefined);
  }
  return loginBgPromise;
}

