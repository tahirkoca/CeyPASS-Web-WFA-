import React, { useEffect, useMemo, useState } from "react";
import { Modal, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { PersonelHeader } from "./PersonelHeader";
import { profilService } from "../../services/profilApi";
import { StatusPopup } from "../StatusPopup";

function fmtMoney(v: any) {
  const n = typeof v === "number" ? v : Number((v ?? "0").toString().replace(",", "."));
  if (!Number.isFinite(n)) return (v ?? "").toString();
  return n.toLocaleString("tr-TR", { style: "currency", currency: "TRY" });
}

function fmtDate(v: any) {
  if (!v) return "";
  const d = new Date(v);
  if (Number.isNaN(d.getTime())) return (v ?? "").toString();
  return d.toLocaleDateString("tr-TR");
}

export function AvanslarimScreen(props: { user: any; onOpenMenu?: () => void }) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<any[]>([]);
  const [miktar, setMiktar] = useState("");
  const [aciklama, setAciklama] = useState("");
  const [creating, setCreating] = useState(false);
  const [detailVisible, setDetailVisible] = useState(false);
  const [detailText, setDetailText] = useState("");
  const [editVisible, setEditVisible] = useState(false);
  const [editId, setEditId] = useState<number | null>(null);
  const [editMiktar, setEditMiktar] = useState("");
  const [editAciklama, setEditAciklama] = useState("");
  const [editing, setEditing] = useState(false);
  const [aktifPage, setAktifPage] = useState(1);
  const [gecmisPage, setGecmisPage] = useState(1);
  const pageSize = 5;
  const [popupVisible, setPopupVisible] = useState(false);
  const [popupType, setPopupType] = useState<"success" | "error">("success");
  const [popupMessage, setPopupMessage] = useState("");
  const showPopup = (type: "success" | "error", message: string) => {
    setPopupType(type);
    setPopupMessage(message);
    setPopupVisible(true);
  };

  const subtitle = useMemo(() => {
    return (props.user?.adSoyad ?? props.user?.AdSoyad ?? "").toString();
  }, [props.user]);

  async function load() {
    try {
      setLoading(true);
      setError(null);
      const res = await profilService.avanslarim();
      if (!res?.success) {
        setError(res?.message || "Avanslarım alınamadı.");
        setItems([]);
      } else {
        setItems(res.data ?? []);
      }
    } catch (e: any) {
      setError(e?.message || "Sunucuya bağlanılamadı.");
      setItems([]);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  const aktif = items.filter((x) => (x?.Durum ?? x?.durum ?? "").toString().toLowerCase().includes("bekliyor"));
  const gecmis = items.filter((x) => !aktif.includes(x));

  useEffect(() => {
    setAktifPage(1);
    setGecmisPage(1);
  }, [aktif.length, gecmis.length]);

  const aktifTotalPages = Math.max(1, Math.ceil(aktif.length / pageSize));
  const gecmisTotalPages = Math.max(1, Math.ceil(gecmis.length / pageSize));
  const aktifPaged = aktif.slice((aktifPage - 1) * pageSize, aktifPage * pageSize);
  const gecmisPaged = gecmis.slice((gecmisPage - 1) * pageSize, gecmisPage * pageSize);

  function Card(props2: { item: any }) {
    const t = props2.item;
    const miktar = t?.Miktar ?? t?.miktar ?? t?.Tutar ?? t?.tutar;
    const aciklama = (t?.Aciklama ?? t?.aciklama ?? "").toString();
    const durum = (t?.Durum ?? t?.durum ?? "").toString();
    const tarih = t?.Tarih ?? t?.tarih ?? t?.OlusturmaTarihi ?? t?.olusturmaTarihi;
    const isBekliyor = durum.toLowerCase().includes("bekliyor");
    const avansId = t?.AvansId ?? t?.avansId;

    return (
      <View className="border-t border-[#f1f5f9] py-3">
        <View className="flex-row items-center justify-between">
          <Text className="text-[#1e293b] font-extrabold">{fmtMoney(miktar)}</Text>
          <View className="px-2 py-1 rounded-lg bg-[#f1f5f9]">
            <Text className="text-[#334155] font-extrabold text-[11px]" numberOfLines={1}>
              {durum || " "}
            </Text>
          </View>
        </View>
        <Text className="text-[#64748b] font-semibold mt-1" numberOfLines={2}>
          {aciklama || " "}
        </Text>
        <Text className="text-[#94a3b8] font-semibold mt-1 text-[12px]">{fmtDate(tarih) || " "}</Text>

        <View className="mt-3 flex-row flex-wrap">
          <TouchableOpacity
            onPress={() => {
              setDetailText(aciklama || "Açıklama belirtilmemiş.");
              setDetailVisible(true);
            }}
            className="mr-2 mb-2 bg-[#f1f5f9] rounded-xl px-4 py-3 flex-row items-center"
          >
            <MaterialCommunityIcons name="information-outline" size={18} color="#334155" />
            <Text className="ml-2 text-[#334155] font-extrabold">Detay</Text>
          </TouchableOpacity>

          {isBekliyor ? (
            <>
              <TouchableOpacity
                onPress={() => {
                  setEditId(avansId);
                  setEditMiktar((miktar ?? "").toString());
                  setEditAciklama(aciklama);
                  setEditVisible(true);
                }}
                className="mr-2 mb-2 bg-[#f1f5f9] rounded-xl px-4 py-3 flex-row items-center"
              >
                <MaterialCommunityIcons name="pencil" size={18} color="#334155" />
                <Text className="ml-2 text-[#334155] font-extrabold">Güncelle</Text>
              </TouchableOpacity>
              <TouchableOpacity
                onPress={async () => {
                  try {
                    setError(null);
                    const r = await profilService.avansIptal(avansId);
                    if (!r?.success) {
                      const msg = r?.message || "İptal edilemedi.";
                      setError(msg);
                      showPopup("error", msg);
                    } else {
                      showPopup("success", r?.message || "İptal edildi.");
                      await load();
                    }
                  } catch (e: any) {
                    setError(e?.message || "Sunucuya bağlanılamadı.");
                    showPopup("error", e?.message || "Sunucuya bağlanılamadı.");
                  }
                }}
                className="mr-2 mb-2 bg-[#fee2e2] rounded-xl px-4 py-3 flex-row items-center"
              >
                <MaterialCommunityIcons name="trash-can-outline" size={18} color="#b91c1c" />
                <Text className="ml-2 text-[#b91c1c] font-extrabold">İptal</Text>
              </TouchableOpacity>
            </>
          ) : null}
        </View>
      </View>
    );
  }

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} useModal={false} autoCloseMs={1500} />
      <PersonelHeader title="Avanslarım" subtitle={subtitle} onOpenMenu={props.onOpenMenu} />

      <ScrollView className="flex-1 px-5 py-5" contentContainerStyle={{ paddingBottom: 24 }}>
        <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4 mb-4">
          <Text className="text-[#1e293b] font-extrabold text-[14px]">Yeni Avans Talebi</Text>
          <View className="mt-3">
            <Text className="text-[#64748b] font-semibold mb-2">Miktar</Text>
            <TextInput
              value={miktar}
              onChangeText={setMiktar}
              placeholder="örn 1000"
              placeholderTextColor="#94a3b8"
              keyboardType="numeric"
              className="border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc] text-[#1e293b] font-semibold"
            />
          </View>
          <View className="mt-3">
            <Text className="text-[#64748b] font-semibold mb-2">Açıklama</Text>
            <TextInput
              value={aciklama}
              onChangeText={setAciklama}
              placeholder="Açıklama"
              placeholderTextColor="#94a3b8"
              multiline
              className="border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc] text-[#1e293b] font-semibold"
              style={{ minHeight: 80, textAlignVertical: "top" }}
            />
          </View>
          <TouchableOpacity
            disabled={creating}
            onPress={async () => {
              const n = Number(miktar.replace(",", "."));
              if (!Number.isFinite(n) || n <= 0) {
                setError("Geçerli bir miktar giriniz.");
                return;
              }
              try {
                setCreating(true);
                setError(null);
                const r = await profilService.avansTalep({ miktar: n, aciklama: aciklama || undefined });
                if (!r?.success) {
                  const msg = r?.message || "Talep oluşturulamadı.";
                  setError(msg);
                  showPopup("error", msg);
                }
                else {
                  showPopup("success", r?.message || "Talep oluşturuldu.");
                  setMiktar("");
                  setAciklama("");
                  await load();
                }
              } catch (e: any) {
                setError(e?.message || "Sunucuya bağlanılamadı.");
                showPopup("error", e?.message || "Sunucuya bağlanılamadı.");
              } finally {
                setCreating(false);
              }
            }}
            className={`mt-4 rounded-xl px-4 py-3 flex-row items-center justify-center ${creating ? "bg-[#f1f5f9]" : "bg-[#dc2626]"}`}
          >
            <Text className={`font-extrabold ${creating ? "text-[#64748b]" : "text-white"}`}>
              {creating ? "Gönderiliyor..." : "Talep Oluştur"}
            </Text>
          </TouchableOpacity>
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
        ) : (
          <>
            <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4">
              <View className="flex-row items-center justify-between">
                <Text className="text-[#1e293b] font-extrabold text-[14px]">Aktif Talepler</Text>
                <View className="flex-row items-center">
                  <MaterialCommunityIcons name="clock-outline" size={16} color="#64748b" />
                  <Text className="ml-1 text-[#64748b] font-extrabold">{aktif.length}</Text>
                </View>
              </View>
              {aktif.length ? (
                <View className="mt-3">
                  {aktifPaged.map((it, idx) => (
                    <Card key={`${it?.AvansId ?? it?.avansId ?? idx}`} item={it} />
                  ))}
                  <View className="mt-3 flex-row items-center justify-between">
                    <Text className="text-[#64748b] font-semibold text-[12px]">
                      Toplam {aktif.length} • Sayfa {aktifPage}/{aktifTotalPages}
                    </Text>
                    <View className="flex-row">
                      <TouchableOpacity
                        disabled={aktifPage <= 1}
                        onPress={() => setAktifPage((p) => Math.max(1, p - 1))}
                        className={`px-3 py-2 rounded-xl mr-2 ${aktifPage <= 1 ? "bg-[#f8fafc]" : "bg-[#f1f5f9]"}`}
                      >
                        <Text className={`font-extrabold ${aktifPage <= 1 ? "text-[#94a3b8]" : "text-[#334155]"}`}>Önceki</Text>
                      </TouchableOpacity>
                      <TouchableOpacity
                        disabled={aktifPage >= aktifTotalPages}
                        onPress={() => setAktifPage((p) => Math.min(aktifTotalPages, p + 1))}
                        className={`px-3 py-2 rounded-xl ${aktifPage >= aktifTotalPages ? "bg-[#f8fafc]" : "bg-[#f1f5f9]"}`}
                      >
                        <Text className={`font-extrabold ${aktifPage >= aktifTotalPages ? "text-[#94a3b8]" : "text-[#334155]"}`}>Sonraki</Text>
                      </TouchableOpacity>
                    </View>
                  </View>
                </View>
              ) : (
                <Text className="mt-3 text-[#64748b] font-semibold">Bekleyen avans talebiniz yok.</Text>
              )}
            </View>

            <View className="mt-4 bg-white rounded-2xl border border-[#f1f5f9] p-4">
              <View className="flex-row items-center justify-between">
                <Text className="text-[#1e293b] font-extrabold text-[14px]">Geçmiş</Text>
                <View className="flex-row items-center">
                  <MaterialCommunityIcons name="history" size={16} color="#64748b" />
                  <Text className="ml-1 text-[#64748b] font-extrabold">{gecmis.length}</Text>
                </View>
              </View>
              {gecmis.length ? (
                <View className="mt-3">
                  {gecmisPaged.map((it, idx) => (
                    <Card key={`${it?.AvansId ?? it?.avansId ?? idx}`} item={it} />
                  ))}
                  <View className="mt-3 flex-row items-center justify-between">
                    <Text className="text-[#64748b] font-semibold text-[12px]">
                      Toplam {gecmis.length} • Sayfa {gecmisPage}/{gecmisTotalPages}
                    </Text>
                    <View className="flex-row">
                      <TouchableOpacity
                        disabled={gecmisPage <= 1}
                        onPress={() => setGecmisPage((p) => Math.max(1, p - 1))}
                        className={`px-3 py-2 rounded-xl mr-2 ${gecmisPage <= 1 ? "bg-[#f8fafc]" : "bg-[#f1f5f9]"}`}
                      >
                        <Text className={`font-extrabold ${gecmisPage <= 1 ? "text-[#94a3b8]" : "text-[#334155]"}`}>Önceki</Text>
                      </TouchableOpacity>
                      <TouchableOpacity
                        disabled={gecmisPage >= gecmisTotalPages}
                        onPress={() => setGecmisPage((p) => Math.min(gecmisTotalPages, p + 1))}
                        className={`px-3 py-2 rounded-xl ${gecmisPage >= gecmisTotalPages ? "bg-[#f8fafc]" : "bg-[#f1f5f9]"}`}
                      >
                        <Text className={`font-extrabold ${gecmisPage >= gecmisTotalPages ? "text-[#94a3b8]" : "text-[#334155]"}`}>Sonraki</Text>
                      </TouchableOpacity>
                    </View>
                  </View>
                </View>
              ) : (
                <Text className="mt-3 text-[#64748b] font-semibold">Kayıt bulunamadı.</Text>
              )}
            </View>
          </>
        )}
      </ScrollView>

      <Modal transparent visible={detailVisible} animationType="fade" onRequestClose={() => setDetailVisible(false)}>
        <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setDetailVisible(false)}>
          <TouchableOpacity activeOpacity={1} onPress={() => {}}>
            <View className="bg-white rounded-2xl p-4">
              <Text className="text-[#1e293b] font-extrabold text-[14px] mb-2">Talep Açıklaması</Text>
              <Text className="text-[#334155] font-semibold">{detailText}</Text>
              <TouchableOpacity onPress={() => setDetailVisible(false)} className="mt-4 bg-[#f1f5f9] rounded-xl px-4 py-3">
                <Text className="text-[#334155] font-extrabold text-center">Kapat</Text>
              </TouchableOpacity>
            </View>
          </TouchableOpacity>
        </TouchableOpacity>
      </Modal>

      <Modal transparent visible={editVisible} animationType="fade" onRequestClose={() => setEditVisible(false)}>
        <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setEditVisible(false)}>
          <TouchableOpacity activeOpacity={1} onPress={() => {}}>
            <View className="bg-white rounded-2xl p-4">
              <Text className="text-[#1e293b] font-extrabold text-[14px]">Avans Talebi Güncelle</Text>

              <View className="mt-3">
                <Text className="text-[#64748b] font-semibold mb-2">Miktar</Text>
                <TextInput
                  value={editMiktar}
                  onChangeText={setEditMiktar}
                  keyboardType="numeric"
                  className="border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc] text-[#1e293b] font-semibold"
                />
              </View>
              <View className="mt-3">
                <Text className="text-[#64748b] font-semibold mb-2">Açıklama</Text>
                <TextInput
                  value={editAciklama}
                  onChangeText={setEditAciklama}
                  multiline
                  className="border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc] text-[#1e293b] font-semibold"
                  style={{ minHeight: 80, textAlignVertical: "top" }}
                />
              </View>

              <View className="mt-4 flex-row">
                <TouchableOpacity onPress={() => setEditVisible(false)} className="flex-1 bg-[#f1f5f9] rounded-xl px-4 py-3 mr-2">
                  <Text className="text-[#334155] font-extrabold text-center">Kapat</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  disabled={editing}
                  onPress={async () => {
                    const id = editId;
                    const n = Number(editMiktar.replace(",", "."));
                    if (!id) return;
                    if (!Number.isFinite(n) || n <= 0) {
                      setError("Geçerli bir miktar giriniz.");
                      return;
                    }
                    try {
                      setEditing(true);
                      setError(null);
                      const r = await profilService.avansGuncelle({ avansId: id, miktar: n, aciklama: editAciklama || undefined });
                      if (!r?.success) setError(r?.message || "Güncellenemedi.");
                      else {
                        setEditVisible(false);
                        await load();
                      }
                    } catch (e: any) {
                      setError(e?.message || "Sunucuya bağlanılamadı.");
                    } finally {
                      setEditing(false);
                    }
                  }}
                  className={`flex-1 rounded-xl px-4 py-3 ${editing ? "bg-[#f1f5f9]" : "bg-[#dc2626]"}`}
                >
                  <Text className={`font-extrabold text-center ${editing ? "text-[#64748b]" : "text-white"}`}>
                    {editing ? "Kaydediliyor..." : "Kaydet"}
                  </Text>
                </TouchableOpacity>
              </View>
            </View>
          </TouchableOpacity>
        </TouchableOpacity>
      </Modal>
    </View>
  );
}

