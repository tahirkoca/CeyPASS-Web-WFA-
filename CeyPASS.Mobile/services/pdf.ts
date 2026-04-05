import * as FileSystem from "expo-file-system/legacy";
import * as Sharing from "expo-sharing";
import { Linking } from "react-native";
import { getApiBaseUrl, getAuthToken } from "./api";

function sanitizeFilename(name: string) {
  return name.replace(/[\\/:*?"<>|]+/g, "_");
}

export async function openPdfFromApi(apiPath: string, fileName: string) {
  const uri = await downloadPdfFromApi(apiPath, fileName);
  await shareOrOpenPdf(uri);
}

export async function downloadPdfFromApi(apiPath: string, fileName: string): Promise<string> {
  const token = await getAuthToken();
  if (!token) throw new Error("Oturum bulunamadı. Lütfen tekrar giriş yapın.");

  const url = `${getApiBaseUrl()}${apiPath.startsWith("/") ? "" : "/"}${apiPath}`;
  const safeName = sanitizeFilename(fileName.endsWith(".pdf") ? fileName : `${fileName}.pdf`);
  const target = `${FileSystem.cacheDirectory}${safeName}`;

  const res = await FileSystem.downloadAsync(url, target, {
    headers: { Authorization: `Bearer ${token}` },
  });

  if (!res?.uri) throw new Error("PDF indirilemedi.");
  return res.uri;
}

export async function shareOrOpenPdf(fileUri: string) {
  if (await Sharing.isAvailableAsync()) {
    await Sharing.shareAsync(fileUri, {
      mimeType: "application/pdf",
      UTI: "com.adobe.pdf",
      dialogTitle: "PDF Aç",
    });
    return;
  }

  // Fallback: try to open directly (Android needs content://)
  const contentUri = await FileSystem.getContentUriAsync(fileUri);
  await Linking.openURL(contentUri);
}

