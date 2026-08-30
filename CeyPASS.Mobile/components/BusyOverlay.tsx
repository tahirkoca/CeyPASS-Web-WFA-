import React from "react";
import { ActivityIndicator, Modal, Text, View } from "react-native";

type BusyOverlayProps = {
  visible: boolean;
  title?: string;
  message?: string;
};

export function BusyOverlay({ visible, title, message }: BusyOverlayProps) {
  if (!visible) return null;
  return (
    <Modal visible transparent animationType="fade" statusBarTranslucent>
      <View className="flex-1 bg-black/45 items-center justify-center px-8">
        <View className="w-full max-w-[320px] bg-white rounded-2xl px-5 py-6 items-center border border-[#e2e8f0]">
          <ActivityIndicator size="large" color="#dc2626" />
          {title ? (
            <Text className="mt-4 text-[#0f172a] font-extrabold text-[16px] text-center">{title}</Text>
          ) : null}
          {message ? (
            <Text className="mt-2 text-[#64748b] font-semibold text-[13px] text-center">{message}</Text>
          ) : null}
        </View>
      </View>
    </Modal>
  );
}
