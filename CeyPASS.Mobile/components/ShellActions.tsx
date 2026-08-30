import React, { createContext, useContext } from "react";

type ShellActions = {
  openTips: () => void;
  setStatusMessage: (msg: string | null) => void;
};

const ShellActionsContext = createContext<ShellActions | null>(null);

export function ShellActionsProvider(props: { value: ShellActions; children: React.ReactNode }) {
  return React.createElement(ShellActionsContext.Provider, { value: props.value }, props.children);
}

export function useShellActions(): ShellActions | null {
  return useContext(ShellActionsContext);
}
