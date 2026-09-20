/** AppShell genel aksiyonları (ör. ipuçları sheet) context. */
import React, { createContext, useContext } from "react";

type ShellActions = {
  openTips: () => void;
  setStatusMessage: (msg: string | null) => void;
};

const ShellActionsContext = createContext<ShellActions | null>(null);

/** AppShell’den openTips / setStatusMessage sağlar. */
export function ShellActionsProvider(props: { value: ShellActions; children: React.ReactNode }) {
  return React.createElement(ShellActionsContext.Provider, { value: props.value }, props.children);
}

/** App kökünden enjekte edilen shell callback’leri. */
export function useShellActions(): ShellActions | null {
  return useContext(ShellActionsContext);
}
