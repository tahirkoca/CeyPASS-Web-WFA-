import React, { useEffect, useMemo, useState } from "react";
import { ActivityIndicator, Modal, Platform, Text, TouchableOpacity, View } from "react-native";
import * as FileSystem from "expo-file-system/legacy";
import { WebView } from "react-native-webview";
import { shareOrOpenPdf } from "../services/pdf";

export function PdfPreviewModal(props: {
  visible: boolean;
  title?: string;
  fileUri: string | null;
  onClose: () => void;
}) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [displayUri, setDisplayUri] = useState<string | null>(null);

  const canShow = props.visible && !!props.fileUri;

  useEffect(() => {
    let alive = true;
    (async () => {
      if (!canShow || !props.fileUri) return;
      try {
        setLoading(true);
        setError(null);
        setDisplayUri(null);

        // Avoid base64 (can crash on large PDFs). Prefer file/content URI.
        if (Platform.OS === "android") {
          const contentUri = await FileSystem.getContentUriAsync(props.fileUri);
          if (!alive) return;
          setDisplayUri(contentUri);
        } else {
          // iOS: file:// uri generally works for WebView.
          setDisplayUri(props.fileUri);
        }
      } catch (e: any) {
        if (!alive) return;
        setError(e?.message || "Önizleme hazırlanamadı.");
      } finally {
        if (!alive) return;
        setLoading(false);
      }
    })();
    return () => {
      alive = false;
    };
  }, [canShow, props.fileUri]);

  const title = useMemo(() => props.title || "PDF Önizleme", [props.title]);

  return (
    <Modal visible={props.visible} transparent animationType="fade" onRequestClose={props.onClose}>
      <View className="flex-1 bg-black/60">
        <View className="flex-row items-center justify-between px-4 pt-12 pb-3 bg-white">
          <Text className="text-[#1e293b] font-extrabold text-[14px]" numberOfLines={1}>
            {title}
          </Text>
          <TouchableOpacity onPress={props.onClose} className="px-3 py-2 bg-[#f1f5f9] rounded-xl">
            <Text className="text-[#334155] font-extrabold">Kapat</Text>
          </TouchableOpacity>
        </View>

        <View className="flex-1 bg-white">
          {loading ? (
            <View className="flex-1 items-center justify-center">
              <ActivityIndicator />
              <Text className="mt-2 text-[#64748b] font-semibold">Yükleniyor...</Text>
            </View>
          ) : error ? (
            <View className="flex-1 items-center justify-center px-6">
              <Text className="text-[#b91c1c] font-extrabold text-center">{error}</Text>
            </View>
          ) : displayUri ? (
            <WebView
              originWhitelist={["*"]}
              source={{ uri: displayUri }}
              style={{ flex: 1, backgroundColor: "white" }}
              javaScriptEnabled
              allowFileAccess
            />
          ) : (
            <View className="flex-1 items-center justify-center">
              <Text className="text-[#64748b] font-semibold">Önizleme yok.</Text>
            </View>
          )}
        </View>

        <View className="px-4 py-4 bg-white border-t border-[#f1f5f9]">
          <TouchableOpacity
            disabled={!props.fileUri}
            onPress={async () => {
              if (!props.fileUri) return;
              await shareOrOpenPdf(props.fileUri);
            }}
            className={`rounded-xl px-4 py-3 ${props.fileUri ? "bg-[#dc2626]" : "bg-[#f1f5f9]"}`}
          >
            <Text className={`font-extrabold text-center ${props.fileUri ? "text-white" : "text-[#94a3b8]"}`}>
              İndir / Paylaş
            </Text>
          </TouchableOpacity>
        </View>
      </View>
    </Modal>
  );
}

