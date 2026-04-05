import React, { useEffect, useMemo, useState } from "react";
import { Modal, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { PersonelHeader } from "./PersonelHeader";
import { profilService } from "../../services/profilApi";
import { downloadPdfFromApi } from "../../services/pdf";
import { PdfPreviewModal } from "../PdfPreviewModal";
import { StatusPopup } from "../StatusPopup";

function fmtDT(v: any) {
  if (!v) return "";
  const d = new Date(v);
  if (Number.isNaN(d.getTime())) return (v ?? "").toString();
  return d.toLocaleString("tr-TR");
}

export function YetkiliPaneliScreen(props: { user: any; onOpenMenu?: () => void }) {
  const subtitle = useMemo(() => (props.user?.adSoyad ?? props.user?.AdSoyad ?? "").toString(), [props.user]);
  const [tab, setTab] = useState<"bekleyen" | "bagli">("bekleyen");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [bekleyen, setBekleyen] = useState<any[]>([]);
  const [bagli, setBagli] = useState<any[]>([]);

  const [actionVisible, setActionVisible] = useState(false);
  const [actionKind, setActionKind] = useState<"onayla" | "reddet">("onayla");
  const [actionTalep, setActionTalep] = useState<any>(null);
  const [actionNote, setActionNote] = useState("");
  const [actionLoading, setActionLoading] = useState(false);
  const [popupVisible, setPopupVisible] = useState(false);
  const [popupType, setPopupType] = useState<"success" | "error">("success");
  const [popupMessage, setPopupMessage] = useState("");
  const showPopup = (type: "success" | "error", message: string) => {
    setPopupType(type);
    setPopupMessage(message);
    setPopupVisible(true);
  };

  const [pdfVisible, setPdfVisible] = useState(false);
  const [pdfUri, setPdfUri] = useState<string | null>(null);
  const [pdfTitle, setPdfTitle] = useState<string>("PDF");

  async function loadAll() {
    try {
      setLoading(true);
      setError(null);
      const [r1, r2] = await Promise.all([profilService.amirBekleyenler(), profilService.bagliPersonellerim()]);
      if (r1?.success) setBekleyen(r1.data ?? []);
      else setBekleyen([]);
      if (r2?.success) setBagli(r2.data ?? []);
      else setBagli([]);
    } catch (e: any) {
      setError(e?.message || "Sunucuya bağlanılamadı.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadAll();
  }, []);

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} useModal={false} autoCloseMs={1500} />
      <PersonelHeader title="Yetkili Paneli" subtitle={subtitle} onOpenMenu={props.onOpenMenu} />

      <View className="px-5 pt-4">
        <View className="flex-row bg-white rounded-2xl border border-[#f1f5f9] p-2">
          <TouchableOpacity
            onPress={() => setTab("bekleyen")}
            className={`flex-1 rounded-xl px-4 py-3 ${tab === "bekleyen" ? "bg-[#fee2e2]" : "bg-transparent"}`}
          >
            <Text className={`text-center font-extrabold ${tab === "bekleyen" ? "text-[#b91c1c]" : "text-[#334155]"}`}>
              Bekleyen Talepler {bekleyen.length ? `(${bekleyen.length})` : ""}
            </Text>
          </TouchableOpacity>
          <TouchableOpacity
            onPress={() => setTab("bagli")}
            className={`flex-1 rounded-xl px-4 py-3 ${tab === "bagli" ? "bg-[#fee2e2]" : "bg-transparent"}`}
          >
            <Text className={`text-center font-extrabold ${tab === "bagli" ? "text-[#b91c1c]" : "text-[#334155]"}`}>
              Bağlı Personellerim
            </Text>
          </TouchableOpacity>
        </View>
      </View>

      <ScrollView className="flex-1 px-5 py-5" contentContainerStyle={{ paddingBottom: 24 }}>
        {loading ? (
          <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4">
            <Text className="text-[#64748b] font-semibold">Yükleniyor...</Text>
          </View>
        ) : error ? (
          <View className="bg-white rounded-2xl border border-[#fee2e2] p-4">
            <Text className="text-[#b91c1c] font-extrabold">Hata</Text>
            <Text className="text-[#7f1d1d] font-semibold mt-1">{error}</Text>
            <TouchableOpacity onPress={loadAll} className="mt-4 bg-[#fee2e2] rounded-xl px-4 py-3 self-start">
              <Text className="text-[#b91c1c] font-extrabold">Tekrar Dene</Text>
            </TouchableOpacity>
          </View>
        ) : tab === "bekleyen" ? (
          <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4">
            <Text className="text-[#1e293b] font-extrabold text-[14px]">Bekleyen İzin Talepleri</Text>
            {bekleyen.length ? (
              <View className="mt-3">
                {bekleyen.map((t: any, idx: number) => (
                  <View key={`${t?.TalepId ?? t?.talepId ?? idx}`} className="border-t border-[#f1f5f9] py-3">
                    <Text className="text-[#1e293b] font-extrabold" numberOfLines={1}>
                      #{(t?.TalepId ?? t?.talepId).toString()} • {(t?.PersonelId ?? t?.personelId ?? "").toString()}
                    </Text>
                    <Text className="text-[#64748b] font-semibold mt-1" numberOfLines={2}>
                      {fmtDT(t?.Baslangic ?? t?.baslangic)} - {fmtDT(t?.Bitis ?? t?.bitis)}
                    </Text>
                    <View className="mt-3 flex-row flex-wrap">
                      <TouchableOpacity
                        onPress={async () => {
                          try {
                            const id = t?.TalepId ?? t?.talepId;
                            if (!id) return;
                            setPdfTitle(`İzin Talep #${id}`);
                            const uri = await downloadPdfFromApi(`/IzinKagit/pdf/${id}`, `IzinTalep_${id}.pdf`);
                            setPdfUri(uri);
                            setPdfVisible(true);
                          } catch (e: any) {
                            setError(e?.message || "PDF açılamadı.");
                          }
                        }}
                        className="mr-2 mb-2 bg-[#f1f5f9] rounded-xl px-4 py-3 flex-row items-center"
                      >
                        <MaterialCommunityIcons name="file-pdf-box" size={18} color="#334155" />
                        <Text className="ml-2 text-[#334155] font-extrabold">PDF</Text>
                      </TouchableOpacity>
                      <TouchableOpacity
                        onPress={() => {
                          setActionKind("onayla");
                          setActionTalep(t);
                          setActionNote("");
                          setActionVisible(true);
                        }}
                        className="mr-2 mb-2 bg-[#dcfce7] rounded-xl px-4 py-3 flex-row items-center"
                      >
                        <MaterialCommunityIcons name="check" size={18} color="#16a34a" />
                        <Text className="ml-2 text-[#166534] font-extrabold">Onayla</Text>
                      </TouchableOpacity>
                      <TouchableOpacity
                        onPress={() => {
                          setActionKind("reddet");
                          setActionTalep(t);
                          setActionNote("");
                          setActionVisible(true);
                        }}
                        className="mr-2 mb-2 bg-[#fee2e2] rounded-xl px-4 py-3 flex-row items-center"
                      >
                        <MaterialCommunityIcons name="close" size={18} color="#b91c1c" />
                        <Text className="ml-2 text-[#b91c1c] font-extrabold">Reddet</Text>
                      </TouchableOpacity>
                    </View>
                  </View>
                ))}
              </View>
            ) : (
              <Text className="mt-3 text-[#64748b] font-semibold">Bekleyen izin talebi bulunmuyor.</Text>
            )}
          </View>
        ) : (
          <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4">
            <Text className="text-[#1e293b] font-extrabold text-[14px]">Bağlı Personellerim</Text>
            <Text className="mt-2 text-[#64748b] font-semibold text-[12px]">
              Bağlı personellerinizin listesini buradan görüntüleyebilirsiniz.
            </Text>
            {bagli.length ? (
              <View className="mt-3">
                {bagli.map((p: any, idx: number) => (
                  <View key={`${p?.PersonelId ?? p?.personelId ?? idx}`} className="border-t border-[#f1f5f9] py-3">
                    <Text className="text-[#1e293b] font-extrabold" numberOfLines={1}>
                      {(p?.PersonelId ?? p?.personelId ?? "").toString()} • {((p?.Ad ?? p?.ad ?? "") + " " + (p?.Soyad ?? p?.soyad ?? "")).trim()}
                    </Text>
                    <Text className="text-[#64748b] font-semibold mt-1" numberOfLines={1}>
                      {(p?.Email ?? p?.email ?? "").toString() || "(Tanımsız)"}
                    </Text>
                  </View>
                ))}
              </View>
            ) : (
              <Text className="mt-3 text-[#64748b] font-semibold">Bağlı personel bulunamadı.</Text>
            )}
          </View>
        )}
      </ScrollView>

      <Modal transparent visible={actionVisible} animationType="fade" onRequestClose={() => setActionVisible(false)}>
        <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setActionVisible(false)}>
          <TouchableOpacity activeOpacity={1} onPress={() => {}}>
            <View className="bg-white rounded-2xl p-4">
              <Text className={`font-extrabold text-[14px] ${actionKind === "onayla" ? "text-[#166534]" : "text-[#b91c1c]"}`}>
                {actionKind === "onayla" ? "İzin Talebini Onayla" : "İzin Talebini Reddet"}
              </Text>
              <Text className="mt-2 text-[#64748b] font-semibold">
                Talep #{(actionTalep?.TalepId ?? actionTalep?.talepId ?? "").toString()}
              </Text>
              <View className="mt-3">
                <Text className="text-[#64748b] font-semibold mb-2">Açıklama / Not (Opsiyonel)</Text>
                <TextInput
                  value={actionNote}
                  onChangeText={setActionNote}
                  placeholder="Notunuzu yazabilirsiniz..."
                  placeholderTextColor="#94a3b8"
                  multiline
                  className="border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc] text-[#1e293b] font-semibold"
                  style={{ minHeight: 90, textAlignVertical: "top" }}
                />
              </View>
              <View className="mt-4 flex-row">
                <TouchableOpacity onPress={() => setActionVisible(false)} className="flex-1 bg-[#f1f5f9] rounded-xl px-4 py-3 mr-2">
                  <Text className="text-[#334155] font-extrabold text-center">Vazgeç</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  disabled={actionLoading}
                  onPress={async () => {
                    try {
                      setActionLoading(true);
                      const id = actionTalep?.TalepId ?? actionTalep?.talepId;
                      if (!id) return;
                      const r =
                        actionKind === "onayla"
                          ? await profilService.amirOnayla(id, actionNote || undefined)
                          : await profilService.amirReddet(id, actionNote || undefined);
                      if (!r?.success) {
                        const msg = r?.message || "İşlem başarısız.";
                        setError(msg);
                        showPopup("error", msg);
                      } else {
                        showPopup("success", r?.message || "İşlem başarılı.");
                      }
                      setActionVisible(false);
                      await loadAll();
                    } catch (e: any) {
                      const msg = e?.message || "Sunucuya bağlanılamadı.";
                      setError(msg);
                      showPopup("error", msg);
                    } finally {
                      setActionLoading(false);
                    }
                  }}
                  className={`flex-1 rounded-xl px-4 py-3 ${actionKind === "onayla" ? "bg-[#16a34a]" : "bg-[#dc2626]"}`}
                >
                  <Text className="text-white font-extrabold text-center">
                    {actionLoading ? "İşleniyor..." : actionKind === "onayla" ? "Onayla" : "Reddet"}
                  </Text>
                </TouchableOpacity>
              </View>
            </View>
          </TouchableOpacity>
        </TouchableOpacity>
      </Modal>

      <PdfPreviewModal
        visible={pdfVisible}
        title={pdfTitle}
        fileUri={pdfUri}
        onClose={() => setPdfVisible(false)}
      />
    </View>
  );
}

