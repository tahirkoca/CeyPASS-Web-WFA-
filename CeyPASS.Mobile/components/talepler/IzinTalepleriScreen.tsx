import React, { useEffect, useMemo, useState } from "react";
import { Modal, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { PersonelHeader } from "../personel/PersonelHeader";
import { profilService } from "../../services/profilApi";
import { downloadPdfFromApi } from "../../services/pdf";
import { PdfPreviewModal } from "../PdfPreviewModal";
import { StatusPopup } from "../StatusPopup";

function fmtDT(v: any) {
  if (!v) return "";
  const d = new Date(v);
  if (Number.isNaN(d.getTime())) return (v ?? "").toString();
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const yyyy = d.getFullYear();
  const HH = String(d.getHours()).padStart(2, "0");
  const mi = String(d.getMinutes()).padStart(2, "0");
  return `${dd}.${mm}.${yyyy} ${HH}:${mi}`;
}

function badgeClass(kind: "ok" | "warn" | "bad") {
  if (kind === "ok") return "bg-[#dcfce7] text-[#166534]";
  if (kind === "bad") return "bg-[#fee2e2] text-[#991b1b]";
  return "bg-[#fef9c3] text-[#854d0e]";
}

function normalizeStatus(v: any): { text: string; kind: "ok" | "warn" | "bad" } {
  const s = (v ?? "Bekliyor").toString();
  const low = s.toLowerCase();
  if (low.includes("onay")) return { text: s, kind: "ok" };
  if (low.includes("redd")) return { text: s, kind: "bad" };
  return { text: s, kind: "warn" };
}

export function IzinTalepleriScreen(props: { user: any; onOpenMenu?: () => void }) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<any[]>([]);
  const [q, setQ] = useState("");
  const [popupVisible, setPopupVisible] = useState(false);
  const [popupType, setPopupType] = useState<"success" | "error">("success");
  const [popupMessage, setPopupMessage] = useState("");
  const showPopup = (type: "success" | "error", message: string) => {
    setPopupType(type);
    setPopupMessage(message);
    setPopupVisible(true);
  };

  const [actionVisible, setActionVisible] = useState(false);
  const [actionKind, setActionKind] = useState<"onayla" | "reddet" | "donus">("onayla");
  const [actionItem, setActionItem] = useState<any | null>(null);
  const [actionNote, setActionNote] = useState("");
  const [acting, setActing] = useState(false);

  const [pdfVisible, setPdfVisible] = useState(false);
  const [pdfUri, setPdfUri] = useState<string | null>(null);
  const [pdfTitle, setPdfTitle] = useState<string>("PDF");

  const subtitle = useMemo(() => (props.user?.adSoyad ?? props.user?.AdSoyad ?? "").toString(), [props.user]);

  async function load() {
    try {
      setLoading(true);
      setError(null);
      const res = await profilService.izinTalepleri();
      if (!res?.success) {
        setItems([]);
        setError(res?.message || "İzin talepleri alınamadı.");
      } else {
        setItems(res.data ?? []);
      }
    } catch (e: any) {
      setItems([]);
      setError(e?.message || "Sunucuya bağlanılamadı.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  const filtered = useMemo(() => {
    const s = q.trim().toLowerCase();
    if (!s) return items;
    return items.filter((t) => {
      const id = (t?.talepId ?? t?.TalepId ?? "").toString();
      const pid = (t?.personelId ?? t?.PersonelId ?? "").toString();
      const pName = (t?.personelAdSoyad ?? t?.PersonelAdSoyad ?? "").toString();
      const tip = (t?.izinTipAdi ?? t?.IzinTipAdi ?? "").toString();
      return (
        id.includes(s) ||
        pid.toLowerCase().includes(s) ||
        pName.toLowerCase().includes(s) ||
        tip.toLowerCase().includes(s)
      );
    });
  }, [items, q]);

  async function doAction() {
    const t = actionItem;
    if (!t) return;
    const id = Number(t?.talepId ?? t?.TalepId ?? 0);
    if (!Number.isFinite(id) || id <= 0) return;
    try {
      setActing(true);
      setError(null);
      let r: any = null;
      if (actionKind === "onayla") r = await profilService.izinTalepOnayla(id, actionNote || undefined);
      else if (actionKind === "reddet") r = await profilService.izinTalepReddet(id, actionNote || undefined);
      else r = await profilService.izinDonusImzasinaAc(id);

      if (!r?.success) {
        const msg = r?.message || "İşlem başarısız.";
        setError(msg);
        showPopup("error", msg);
      }
      else {
        showPopup("success", r?.message || "İşlem başarılı.");
        setActionVisible(false);
        setActionItem(null);
        setActionNote("");
        await load();
      }
    } catch (e: any) {
      setError(e?.message || "Sunucuya bağlanılamadı.");
      showPopup("error", e?.message || "Sunucuya bağlanılamadı.");
    } finally {
      setActing(false);
    }
  }

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} useModal={false} autoCloseMs={1500} />
      <PersonelHeader title="İzin Talepleri" subtitle={subtitle} onOpenMenu={props.onOpenMenu} />

      <ScrollView className="flex-1 px-5 py-5" contentContainerStyle={{ paddingBottom: 24 }}>
        <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4 mb-4">
          <Text className="text-[#64748b] font-semibold mb-2">Ara (ID / Personel / İzin Tipi)</Text>
          <View className="flex-row items-center border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc]">
            <MaterialCommunityIcons name="magnify" size={18} color="#64748b" />
            <TextInput
              value={q}
              onChangeText={setQ}
              placeholder="örn 11292 / Personel / Mazeret"
              placeholderTextColor="#94a3b8"
              className="flex-1 ml-2 text-[#1e293b] font-semibold"
            />
          </View>
          <Text className="mt-2 text-[#94a3b8] font-semibold text-[12px]">Toplam: {filtered.length}</Text>
        </View>

        {loading ? (
          <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4">
            <Text className="text-[#64748b] font-semibold">Yükleniyor...</Text>
          </View>
        ) : error ? (
          <View className="bg-white rounded-2xl border border-[#fee2e2] p-4">
            <Text className="text-[#b91c1c] font-extrabold">Hata</Text>
            <Text className="text-[#7f1d1d] font-semibold mt-1">{error}</Text>
            <TouchableOpacity onPress={load} className="mt-4 bg-[#fee2e2] rounded-xl px-4 py-3 self-start">
              <Text className="text-[#b91c1c] font-extrabold">Tekrar Dene</Text>
            </TouchableOpacity>
          </View>
        ) : filtered.length ? (
          filtered.map((t, idx) => {
            const talepId = Number(t?.talepId ?? t?.TalepId ?? 0);
            const pName = (t?.personelAdSoyad ?? t?.PersonelAdSoyad ?? t?.personelId ?? t?.PersonelId ?? "").toString();
            const tip = (t?.izinTipAdi ?? t?.IzinTipAdi ?? t?.izinTipId ?? t?.IzinTipId ?? "").toString();
            const bas = t?.baslangic ?? t?.Baslangic;
            const bit = t?.bitis ?? t?.Bitis;
            const saatlik = !!(t?.saatlikIzinMi ?? t?.SaatlikIzinMi);
            const ust = normalizeStatus(t?.ustYetkiliOnayDurumu ?? t?.UstYetkiliOnayDurumu);
            const ik = normalizeStatus(t?.ikOnayDurumu ?? t?.IkOnayDurumu);
            const islendi = !!(t?.sonucKisiIzinId ?? t?.SonucKisiIzinId);

            return (
              <View key={`${talepId || idx}`} className="bg-white rounded-2xl border border-[#f1f5f9] p-4 mb-4">
                <View className="flex-row items-center justify-between">
                  <Text className="text-[#1e293b] font-extrabold text-[14px]">Talep #{talepId || "-"}</Text>
                  {islendi ? (
                    <View className="px-2 py-1 rounded-lg bg-[#e0f2fe]">
                      <Text className="text-[#075985] font-extrabold text-[11px]">İşlendi</Text>
                    </View>
                  ) : (
                    <View className="px-2 py-1 rounded-lg bg-[#f1f5f9]">
                      <Text className="text-[#334155] font-extrabold text-[11px]">Bekliyor</Text>
                    </View>
                  )}
                </View>

                <Text className="mt-2 text-[#334155] font-extrabold">{pName}</Text>
                <Text className="mt-1 text-[#64748b] font-semibold">{tip}</Text>

                <View className="mt-3 flex-row flex-wrap">
                  <View className="mr-2 mb-2 px-2 py-1 rounded-lg bg-[#f1f5f9]">
                    <Text className="text-[#334155] font-extrabold text-[11px]">Başlangıç: {fmtDT(bas) || "-"}</Text>
                  </View>
                  <View className="mr-2 mb-2 px-2 py-1 rounded-lg bg-[#f1f5f9]">
                    <Text className="text-[#334155] font-extrabold text-[11px]">Bitiş: {fmtDT(bit) || "-"}</Text>
                  </View>
                  <View className="mr-2 mb-2 px-2 py-1 rounded-lg bg-[#f1f5f9]">
                    <Text className="text-[#334155] font-extrabold text-[11px]">Saatlik: {saatlik ? "Evet" : "Hayır"}</Text>
                  </View>
                </View>

                <View className="mt-2 flex-row">
                  <View className={`mr-2 px-2 py-1 rounded-lg ${badgeClass(ust.kind).split(" ")[0]}`}>
                    <Text className={`font-extrabold text-[11px] ${badgeClass(ust.kind).split(" ")[1]}`}>Üst: {ust.text}</Text>
                  </View>
                  <View className={`px-2 py-1 rounded-lg ${badgeClass(ik.kind).split(" ")[0]}`}>
                    <Text className={`font-extrabold text-[11px] ${badgeClass(ik.kind).split(" ")[1]}`}>İK: {ik.text}</Text>
                  </View>
                </View>

                <View className="mt-3 flex-row flex-wrap">
                  <TouchableOpacity
                    onPress={async () => {
                      if (!talepId) return;
                      try {
                        setPdfTitle(`İzin Talep #${talepId}`);
                        const uri = await downloadPdfFromApi(`/IzinKagit/pdf/${talepId}`, `IzinTalep_${talepId}.pdf`);
                        setPdfUri(uri);
                        setPdfVisible(true);
                      } catch (e: any) {
                        setError(e?.message || "PDF indirilemedi.");
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
                      setActionItem(t);
                      setActionNote("");
                      setActionVisible(true);
                    }}
                    className="mr-2 mb-2 bg-[#dcfce7] rounded-xl px-4 py-3 flex-row items-center"
                  >
                    <MaterialCommunityIcons name="check" size={18} color="#166534" />
                    <Text className="ml-2 text-[#166534] font-extrabold">Onayla</Text>
                  </TouchableOpacity>

                  <TouchableOpacity
                    onPress={() => {
                      setActionKind("reddet");
                      setActionItem(t);
                      setActionNote("");
                      setActionVisible(true);
                    }}
                    className="mr-2 mb-2 bg-[#fee2e2] rounded-xl px-4 py-3 flex-row items-center"
                  >
                    <MaterialCommunityIcons name="close" size={18} color="#991b1b" />
                    <Text className="ml-2 text-[#991b1b] font-extrabold">Reddet</Text>
                  </TouchableOpacity>

                  <TouchableOpacity
                    onPress={() => {
                      setActionKind("donus");
                      setActionItem(t);
                      setActionNote("");
                      setActionVisible(true);
                    }}
                    className="mr-2 mb-2 bg-[#e0f2fe] rounded-xl px-4 py-3 flex-row items-center"
                  >
                    <MaterialCommunityIcons name="lock-open-outline" size={18} color="#075985" />
                    <Text className="ml-2 text-[#075985] font-extrabold">Dönüş İmzasına Aç</Text>
                  </TouchableOpacity>
                </View>
              </View>
            );
          })
        ) : (
          <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4">
            <Text className="text-[#64748b] font-semibold">Kayıt bulunamadı.</Text>
          </View>
        )}
      </ScrollView>

      <Modal visible={actionVisible} transparent animationType="fade" onRequestClose={() => setActionVisible(false)}>
        <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setActionVisible(false)}>
          <TouchableOpacity activeOpacity={1} onPress={() => {}}>
            <View className="bg-white rounded-2xl p-4">
              <Text className="text-[#1e293b] font-extrabold text-[14px]">
                {actionKind === "onayla" ? "Talebi Onayla" : actionKind === "reddet" ? "Talebi Reddet" : "Dönüş İmzasına Aç"}
              </Text>
              {actionKind !== "donus" ? (
                <View className="mt-3">
                  <Text className="text-[#64748b] font-semibold mb-2">Açıklama (opsiyonel)</Text>
                  <TextInput
                    value={actionNote}
                    onChangeText={setActionNote}
                    placeholder="Not"
                    placeholderTextColor="#94a3b8"
                    multiline
                    className="border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc] text-[#1e293b] font-semibold"
                    style={{ minHeight: 80, textAlignVertical: "top" }}
                  />
                </View>
              ) : (
                <Text className="mt-2 text-[#64748b] font-semibold text-[12px]">
                  Bu işlem, personelin “Dönüş İmzası At” adımını tamamlayabilmesi için yetki verir.
                </Text>
              )}

              <View className="mt-4 flex-row">
                <TouchableOpacity onPress={() => setActionVisible(false)} className="flex-1 bg-[#f1f5f9] rounded-xl px-4 py-3 mr-2">
                  <Text className="text-[#334155] font-extrabold text-center">Kapat</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  disabled={acting}
                  onPress={doAction}
                  className={`flex-1 rounded-xl px-4 py-3 ${
                    acting ? "bg-[#f1f5f9]" : actionKind === "onayla" ? "bg-[#16a34a]" : actionKind === "reddet" ? "bg-[#dc2626]" : "bg-[#0284c7]"
                  }`}
                >
                  <Text className={`font-extrabold text-center ${acting ? "text-[#64748b]" : "text-white"}`}>
                    {acting ? "Gönderiliyor..." : "Onayla"}
                  </Text>
                </TouchableOpacity>
              </View>
            </View>
          </TouchableOpacity>
        </TouchableOpacity>
      </Modal>

      <PdfPreviewModal visible={pdfVisible} title={pdfTitle} fileUri={pdfUri} onClose={() => setPdfVisible(false)} />
    </View>
  );
}

