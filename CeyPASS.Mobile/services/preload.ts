import { Asset } from "expo-asset";

export const LoginBackground = require("../assets/ceyport-tekirdag.png");

let loginBgPromise: Promise<void> | null = null;

export function preloadLoginBackground(): Promise<void> {
  if (!loginBgPromise) {
    loginBgPromise = Asset.fromModule(LoginBackground)
      .downloadAsync()
      .then(() => undefined)
      .catch(() => undefined);
  }
  return loginBgPromise;
}

