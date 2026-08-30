import React, { createContext, useContext, useMemo } from "react";

type UiPrefsContextValue = {
  isCompact: boolean;
  listRowPadClass: string;
};

const UiPrefsContext = createContext<UiPrefsContextValue>({
  isCompact: false,
  listRowPadClass: "py-3",
});

export function UiPrefsProvider(props: { children: React.ReactNode }) {
  const value = useMemo<UiPrefsContextValue>(
    () => ({
      isCompact: false,
      listRowPadClass: "py-3",
    }),
    []
  );

  return React.createElement(UiPrefsContext.Provider, { value }, props.children);
}

export function useUiPrefs(): UiPrefsContextValue {
  return useContext(UiPrefsContext);
}
