/** Süper admin: sürüm duyurusu e-postası. */
import React, { useState } from "react";
import { Modal, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import { WebView } from "react-native-webview";
import { StatusPopup } from "../StatusPopup";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";
import { adminService, GuncellemeNotifikasyonDTO } from "../../services/adminApi";

function normalizeDateOnly(d: Date) {
  const x = new Date(d);
  x.setHours(12, 0, 0, 0);
  return x;
}

function fmtIsoDate(d: Date) {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

function getApiErrorMessage(e: any): string {
  const data = e?.response?.data;
  if (typeof data === "string" && data.trim()) return data;
  if (data?.message) return String(data.message);
  if (data?.Message) return String(data.Message);
  if (data?.error) return String(data.error);
  if (e?.message) return String(e.message);
  return "Hata oluştu.";
}

export function GuncellemeBildirimiScreen(props: { user: any; abilities: any; onOpenMenu: () => void }) {
  const rolIdRaw: any =
    props.abilities?.rolId ?? props.abilities?.RolId ?? props.user?.rolId ?? props.user?.RolId;
  const rolIdNum = rolIdRaw == null ? NaN : Number(rolIdRaw);
  const isSuperAdmin = rolIdNum === 1;

  const [popupVisible, setPopupVisible] = useState(false);
  const [popupType, setPopupType] = useState<"success" | "error">("success");
  const [popupMessage, setPopupMessage] = useState("");
  const showPopup = (type: "success" | "error", message: string) => {
    setPopupType(type);
    setPopupMessage(message);
    setPopupVisible(true);
  };

  const [mVersiyon, setMVersiyon] = useState("1.0.0");
  const [mTarih, setMTarih] = useState<Date>(() => normalizeDateOnly(new Date()));
  const [mTip, setMTip] = useState<"Major" | "Minor" | "Bugfix">("Minor");
  const [mYeni, setMYeni] = useState("");
  const [mIyiles, setMIyiles] = useState("");
  const [mHata, setMHata] = useState("");
  const [mKritik, setMKritik] = useState("");
  const [mNot, setMNot] = useState("Bu güncelleme yapıldıktan sonra uygulamanın yeniden başlatılması gerekmektedir.");
  const [previewLoading, setPreviewLoading] = useState(false);
  const [sendLoading, setSendLoading] = useState(false);
  const [previewVisible, setPreviewVisible] = useState(false);
  const [previewHtml, setPreviewHtml] = useState("");
  const quickMenu = useHeaderQuickMenu();
  const notif = useNotificationsContext();

  const buildMailPayload = (): GuncellemeNotifikasyonDTO => {
    const toLines = (s: string) =>
      (s ?? "")
        .split(/\r?\n/)
        .map((x) => x.trim())
        .filter(Boolean);
    return {
      versiyonNumarasi: (mVersiyon ?? "").trim(),
      yayinTarihi: `${fmtIsoDate(mTarih)}T00:00:00`,
      guncellemeTipi: mTip,
      yeniOzellikler: toLines(mYeni),
      iyilestirmeler: toLines(mIyiles),
      hataDuzeltmeleri: toLines(mHata),
      kritikDegisiklikler: toLines(mKritik),
      ekNotlar: (mNot ?? "").trim(),
    };
  };

  const validateMailPayload = (p: GuncellemeNotifikasyonDTO): string | null => {
    if (!p.versiyonNumarasi?.trim()) return "Versiyon numarası giriniz.";
    if (!p.guncellemeTipi?.trim()) return "Güncelleme tipini seçiniz.";
    const total =
      (p.yeniOzellikler?.length ?? 0) +
      (p.iyilestirmeler?.length ?? 0) +
      (p.hataDuzeltmeleri?.length ?? 0) +
      (p.kritikDegisiklikler?.length ?? 0);
    if (total <= 0) return "En az bir kategoriye madde eklemelisiniz (her satır bir madde).";
    return null;
  };

  const doPreview = async () => {
    if (previewLoading || sendLoading) return;
    const payload = buildMailPayload();
    const validationError = validateMailPayload(payload);
    if (validationError) {
      showPopup("error", validationError);
      return;
    }
    setPreviewLoading(true);
    try {
      const resp = await adminService.previewMail(payload);
      if (!resp?.success) throw new Error(resp?.message || "Önizleme alınamadı.");
      setPreviewHtml(resp.data ?? "");
      setPreviewVisible(true);
    } catch (e: any) {
      showPopup("error", getApiErrorMessage(e));
    } finally {
      setPreviewLoading(false);
    }
  };

  const doSend = async () => {
    if (previewLoading || sendLoading) return;
    const payload = buildMailPayload();
    const validationError = validateMailPayload(payload);
    if (validationError) {
      showPopup("error", validationError);
      return;
    }
    setSendLoading(true);
    try {
      const resp = await adminService.sendMail(payload);
      if (!resp?.success) throw new Error(resp?.message || "Gönderilemedi.");
      showPopup("success", resp?.message || "Gönderildi.");
    } catch (e: any) {
      showPopup("error", getApiErrorMessage(e));
    } finally {
      setSendLoading(false);
    }
  };

  const topBar = (
    <>
      <PageHeader
        title="Güncelleme Bildirimi"
        onOpenMenu={props.onOpenMenu}
        rightIcon2="bell-outline"
        onRightPress2={() => quickMenu.open("notif")}
        rightBadge2={notif.unreadCount}
      />
      {quickMenu.modal}
    </>
  );

  if (!isSuperAdmin) {
    return (
      <View className="flex-1 bg-[#f8fafc]">
        {topBar}
        <View className="flex-1 items-center justify-center px-6">
          <Text className="text-[#dc2626] font-extrabold text-center">Bu ekran yalnızca süper yönetici içindir.</Text>
        </View>
      </View>
    );
  }

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} />
      {topBar}

      <ScrollView className="flex-1 px-4" contentContainerStyle={{ paddingTop: 16, paddingBottom: 24 }}>
        <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
          <View className="px-4 py-3 border-b border-[#f1f5f9]">
            <Text className="text-[#0f172a] font-extrabold">Güncelleme Bildirimi</Text>
            <Text className="text-[#64748b] font-semibold text-[12px] mt-1">
              Yeni sürüm duyurusunu oluşturup ilgili alıcılara gönderin.
            </Text>
          </View>
          <View className="p-4">
            <Text className="text-[#64748b] font-semibold">Versiyon No *</Text>
            <TextInput
              value={mVersiyon}
              onChangeText={setMVersiyon}
              className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
            />

            <Text className="mt-4 text-[#64748b] font-semibold">Yayın Tarihi *</Text>
            <TextInput
              value={fmtIsoDate(mTarih)}
              onChangeText={(t) => {
                const m = (t ?? "").match(/^(\d{4})-(\d{2})-(\d{2})$/);
                if (!m) return;
                setMTarih(normalizeDateOnly(new Date(Number(m[1]), Number(m[2]) - 1, Number(m[3]))));
              }}
              placeholder="yyyy-MM-dd"
              className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
              autoCapitalize="none"
            />

            <Text className="mt-4 text-[#64748b] font-semibold">Güncelleme Tipi *</Text>
            <View className="mt-2 flex-row gap-2">
              {(["Major", "Minor", "Bugfix"] as const).map((t) => (
                <TouchableOpacity
                  key={t}
                  onPress={() => setMTip(t)}
                  className={`px-3 py-2 rounded-xl ${mTip === t ? "bg-[#0f172a]" : "bg-[#f1f5f9]"}`}
                >
                  <Text className={`font-extrabold ${mTip === t ? "text-white" : "text-[#334155]"}`}>{t}</Text>
                </TouchableOpacity>
              ))}
            </View>

            <Text className="mt-4 text-[#64748b] font-semibold">Yeni Özellikler</Text>
            <TextInput
              value={mYeni}
              onChangeText={setMYeni}
              multiline
              className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
              style={{ minHeight: 90, textAlignVertical: "top" }}
            />

            <Text className="mt-4 text-[#64748b] font-semibold">İyileştirmeler</Text>
            <TextInput
              value={mIyiles}
              onChangeText={setMIyiles}
              multiline
              className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
              style={{ minHeight: 90, textAlignVertical: "top" }}
            />

            <Text className="mt-4 text-[#64748b] font-semibold">Hata Düzeltmeleri</Text>
            <TextInput
              value={mHata}
              onChangeText={setMHata}
              multiline
              className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
              style={{ minHeight: 90, textAlignVertical: "top" }}
            />

            <Text className="mt-4 text-[#64748b] font-semibold">Kritik Değişiklikler</Text>
            <TextInput
              value={mKritik}
              onChangeText={setMKritik}
              multiline
              className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
              style={{ minHeight: 90, textAlignVertical: "top" }}
            />

            <Text className="mt-4 text-[#64748b] font-semibold">Ek Notlar</Text>
            <TextInput
              value={mNot}
              onChangeText={setMNot}
              multiline
              className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
              style={{ minHeight: 70, textAlignVertical: "top" }}
            />

            <View className="mt-4 flex-row gap-2 justify-end">
              <TouchableOpacity
                onPress={doPreview}
                disabled={previewLoading}
                className={`px-4 py-3 rounded-xl ${previewLoading ? "bg-[#e2e8f0]" : "bg-[#f1f5f9]"}`}
              >
                <Text className="text-[#334155] font-extrabold">Önizleme</Text>
              </TouchableOpacity>
              <TouchableOpacity
                onPress={doSend}
                disabled={sendLoading}
                className={`px-4 py-3 rounded-xl ${sendLoading ? "bg-[#94a3b8]" : "bg-[#0f172a]"}`}
              >
                <Text className="text-white font-extrabold">{sendLoading ? "Gönderiliyor..." : "Gönder"}</Text>
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </ScrollView>

      {previewVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setPreviewVisible(false)}>
          <View className="flex-1 bg-black/60">
            <View className="flex-row items-center justify-between px-4 pt-12 pb-3 bg-white">
              <Text className="text-[#1e293b] font-extrabold text-[14px]" numberOfLines={1}>
                Önizleme (HTML)
              </Text>
              <TouchableOpacity onPress={() => setPreviewVisible(false)} className="px-3 py-2 bg-[#f1f5f9] rounded-xl">
                <Text className="text-[#334155] font-extrabold">Kapat</Text>
              </TouchableOpacity>
            </View>
            <View className="flex-1 bg-white">
              <WebView
                originWhitelist={["*"]}
                source={{ html: previewHtml || "<html><body>(boş)</body></html>" }}
                style={{ flex: 1, backgroundColor: "#fff" }}
              />
            </View>
          </View>
        </Modal>
      ) : null}
    </View>
  );
}
