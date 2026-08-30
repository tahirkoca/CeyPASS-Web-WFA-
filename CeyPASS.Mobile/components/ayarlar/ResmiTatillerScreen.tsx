import React, { useEffect, useMemo, useState } from "react";
import { ActivityIndicator, Modal, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import DateTimePicker from "@react-native-community/datetimepicker";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { StatusPopup } from "../StatusPopup";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";
import { ayarlarService, ResmiTatilDTO } from "../../services/ayarlarApi";

function pick<T = any>(obj: any, a: string, b?: string): T | undefined {
  if (!obj) return undefined;
  if (obj[a] !== undefined) return obj[a] as T;
  if (b && obj[b] !== undefined) return obj[b] as T;
  return undefined;
}

function asInt(value: any, def = 0) {
  const n = Number(value);
  return Number.isFinite(n) ? n : def;
}

function fmtDateTR(value: any) {
  if (!value) return "-";
  const s = String(value);
  const m = s.match(/^(\d{4})-(\d{2})-(\d{2})/);
  if (m) return `${m[3]}.${m[2]}.${m[1]}`;
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return s;
  return d.toLocaleDateString("tr-TR", { year: "numeric", month: "2-digit", day: "2-digit" });
}

function normalizeDateOnly(d: Date) {
  const x = new Date(d);
  x.setHours(12, 0, 0, 0);
  return x;
}

function fmtIsoDate(d: Date) {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

export function ResmiTatillerScreen(props: { user: any; abilities: any; onOpenMenu: () => void }) {
  const actions = props.abilities?.actions?.ResmiTatiller ?? props.abilities?.Actions?.ResmiTatiller ?? {};
  const canCreate = !!(actions?.Create ?? actions?.create);
  const rolId: number | null | undefined = props.abilities?.rolId ?? props.abilities?.RolId;
  const isAdmin = !!(props.abilities?.isAdmin ?? props.abilities?.IsAdmin ?? (rolId === 1 || rolId === 2));
  const canApprove = !!(actions?.Approve ?? actions?.approve ?? (isAdmin && !!(props.abilities?.view?.ResmiTatiller ?? props.abilities?.View?.ResmiTatiller)));

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<ResmiTatilDTO[]>([]);

  const [yil, setYil] = useState(() => new Date().getFullYear());
  const [q, setQ] = useState("");

  const [popupVisible, setPopupVisible] = useState(false);
  const [popupType, setPopupType] = useState<"success" | "error">("success");
  const [popupMessage, setPopupMessage] = useState("");
  const showPopup = (type: "success" | "error", message: string) => {
    setPopupType(type);
    setPopupMessage(message);
    setPopupVisible(true);
  };
  const quickMenu = useHeaderQuickMenu();
  const notif = useNotificationsContext();

  const refresh = async (yy?: number) => {
    setLoading(true);
    setError(null);
    try {
      const selectedYil = yy ?? yil;
      const resp = await ayarlarService.listResmiTatiller({ yil: selectedYil });
      if (!resp?.success) throw new Error(resp?.message || "Liste alınamadı.");
      setItems(resp.data ?? []);
    } catch (e: any) {
      setError(e?.message || "Hata oluştu.");
    } finally {
      setLoading(false);
    }
  };

  async function refreshForce() {
    await refresh(yil);
  }

  useEffect(() => {
    refresh();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const rows = useMemo(() => {
    const qq = q.trim().toLowerCase();
    const mapped = (items ?? []).map((x) => {
      const tarih = pick<any>(x, "tarih", "Tarih");
      const ad = (pick<string>(x, "ad", "Ad") ?? "").toString();
      const calisma = pick<number | null>(x, "calismaSaati", "CalismaSaati") ?? null;
      return { tarih, ad, calisma };
    });
    const filtered = !qq ? mapped : mapped.filter((r) => `${fmtDateTR(r.tarih)} ${r.ad}`.toLowerCase().includes(qq));
    return filtered.sort((a, b) => String(a.tarih).localeCompare(String(b.tarih)));
  }, [items, q]);

  // create form
  const [formVisible, setFormVisible] = useState(false);
  const [fTarih, setFTarih] = useState<Date>(() => normalizeDateOnly(new Date()));
  const [fAd, setFAd] = useState("");
  const [fSaat, setFSaat] = useState<string>(""); // optional decimal
  const [saving, setSaving] = useState(false);
  const [pickerOpen, setPickerOpen] = useState(false);

  const openCreate = () => {
    setFTarih(normalizeDateOnly(new Date(yil, 0, 1)));
    setFAd("");
    setFSaat("");
    setFormVisible(true);
  };

  const submit = async () => {
    if (saving) return;
    const ad = (fAd ?? "").trim();
    if (!ad) {
      showPopup("error", "Tatil adı boş olamaz.");
      return;
    }
    const calismaSaat = (fSaat ?? "").trim() ? Number((fSaat ?? "").trim().replace(",", ".")) : null;
    if ((fSaat ?? "").trim() && !Number.isFinite(calismaSaat as any)) {
      showPopup("error", "Çalışma saati sayı olmalı.");
      return;
    }
    setSaving(true);
    try {
      const resp = await ayarlarService.saveResmiTatil({ tarih: fmtIsoDate(fTarih), ad, calismaSaat });
      if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
      showPopup("success", resp?.message || "Resmi tatil kaydedildi.");
      setFormVisible(false);
      setYil(fTarih.getFullYear());
      await refresh(fTarih.getFullYear());
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setSaving(false);
    }
  };

  // approve: doldur sabit
  const [bulkVisible, setBulkVisible] = useState(false);
  const [bStart, setBStart] = useState<string>(String(yil));
  const [bEnd, setBEnd] = useState<string>(String(yil));
  const [bulkSaving, setBulkSaving] = useState(false);

  const submitBulk = async () => {
    if (bulkSaving) return;
    const s = asInt(bStart, 0);
    const e = asInt(bEnd, 0);
    if (s < 2000 || e < 2000 || s > 2100 || e > 2100 || e < s) {
      showPopup("error", "Yıl aralığı geçersiz.");
      return;
    }
    setBulkSaving(true);
    try {
      const resp = await ayarlarService.doldurSabit({ baslangicYili: s, bitisYili: e });
      if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
      showPopup("success", resp?.message || "Sabit tatiller işlendi.");
      setBulkVisible(false);
      setYil(s);
      await refresh(s);
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setBulkSaving(false);
    }
  };

  const topBar = (
    <>
      <PageHeader
        title="Resmi Tatiller"
        onOpenMenu={props.onOpenMenu}
        rightIcon="refresh"
        onRightPress={refreshForce}
        rightIcon2={canApprove ? "upload" : canCreate ? "plus" : "bell-outline"}
        onRightPress2={canApprove ? () => setBulkVisible(true) : canCreate ? openCreate : () => quickMenu.open("notif")}
        rightBadge2={canApprove || canCreate ? undefined : notif.unreadCount}
      />
      {quickMenu.modal}
    </>
  );

  if (loading) {
    return (
      <View className="flex-1 bg-[#f8fafc]">
        <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} useModal={false} />
        {topBar}
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator />
          <Text className="mt-2 text-[#64748b] font-semibold">Yükleniyor...</Text>
        </View>
      </View>
    );
  }

  if (error) {
    return (
      <View className="flex-1 bg-[#f8fafc]">
        {topBar}
        <View className="flex-1 items-center justify-center px-6">
          <Text className="text-[#dc2626] font-extrabold text-center">{error}</Text>
          <TouchableOpacity onPress={() => refresh()} className="mt-4 px-4 py-3 rounded-xl bg-[#0f172a]">
            <Text className="text-white font-extrabold">Tekrar Dene</Text>
          </TouchableOpacity>
        </View>
      </View>
    );
  }

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} useModal={false} />
      {topBar}

      <ScrollView className="flex-1 px-4" contentContainerStyle={{ paddingBottom: 24 }}>
        <View className="mt-4 bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
          <View className="px-4 py-3 border-b border-[#f1f5f9]">
            <Text className="text-[#0f172a] font-extrabold">Filtre</Text>
            <View className="mt-2 flex-row gap-2">
              <TextInput
                value={String(yil)}
                onChangeText={(t) => setYil(asInt(t, yil))}
                keyboardType="number-pad"
                className="flex-1 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                placeholderTextColor="#94a3b8"
              />
              <TouchableOpacity onPress={() => refresh(yil)} className="px-4 py-3 rounded-xl bg-[#0f172a] items-center justify-center">
                <Text className="text-white font-extrabold">Filtrele</Text>
              </TouchableOpacity>
            </View>
            <TextInput
              value={q}
              onChangeText={setQ}
              placeholder="Ara (Tarih / Ad)"
              className="mt-2 px-4 py-3 rounded-xl bg-[#f8fafc] border border-[#e2e8f0] text-[#0f172a] font-semibold"
              placeholderTextColor="#94a3b8"
            />
          </View>
          <View className="px-4 py-3">
            <Text className="text-[#64748b] font-semibold text-[12px]">Toplam {rows.length} kayıt</Text>
          </View>
        </View>

        <View className="mt-4">
          {!rows.length ? (
            <View className="py-10 items-center">
              <Text className="text-[#64748b] font-semibold">{yil} yılı için resmi tatil bulunamadı.</Text>
            </View>
          ) : (
            rows.map((r, idx) => (
              <View key={`rt_${idx}`} className="mb-3 bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9]">
                  <Text className="text-[#0f172a] font-extrabold">{fmtDateTR(r.tarih)}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">{r.ad}</Text>
                  <Text className="text-[#64748b] font-semibold text-[12px] mt-1">
                    Çalışma Saati: {r.calisma == null ? "-" : Number(r.calisma).toFixed(2)}
                  </Text>
                </View>
              </View>
            ))
          )}
        </View>
      </ScrollView>

      {formVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setFormVisible(false)}>
          <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setFormVisible(false)}>
            <TouchableOpacity activeOpacity={1} onPress={() => {}}>
              <View className="bg-white rounded-2xl overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                  <Text className="text-[#0f172a] font-extrabold">Yeni Resmi Tatil</Text>
                  <TouchableOpacity onPress={() => setFormVisible(false)} className="px-3 py-2 rounded-xl bg-[#f1f5f9]">
                    <Text className="text-[#334155] font-extrabold">Kapat</Text>
                  </TouchableOpacity>
                </View>
                <ScrollView style={{ maxHeight: 560 }} contentContainerStyle={{ padding: 16 }}>
                  <Text className="text-[#64748b] font-semibold">Tarih *</Text>
                  <TouchableOpacity onPress={() => setPickerOpen(true)} className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0]">
                    <Text className="text-[#0f172a] font-extrabold">{fmtDateTR(fmtIsoDate(fTarih))}</Text>
                  </TouchableOpacity>
                  {pickerOpen ? (
                    <DateTimePicker
                      value={fTarih}
                      mode="date"
                      display="default"
                      onChange={(_, selected) => {
                        setPickerOpen(false);
                        if (selected) setFTarih(normalizeDateOnly(selected));
                      }}
                    />
                  ) : null}

                  <Text className="mt-4 text-[#64748b] font-semibold">Tatil Adı *</Text>
                  <TextInput
                    value={fAd}
                    onChangeText={setFAd}
                    placeholder="Tatil adı"
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
                  />

                  <Text className="mt-4 text-[#64748b] font-semibold">Çalışma Saati</Text>
                  <TextInput
                    value={fSaat}
                    onChangeText={setFSaat}
                    placeholder="opsiyonel (örn 7.50)"
                    keyboardType="decimal-pad"
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
                  />
                </ScrollView>
                <View className="p-4 border-t border-[#f1f5f9] flex-row gap-2">
                  <TouchableOpacity onPress={() => setFormVisible(false)} disabled={saving} className={`flex-1 py-3 rounded-xl items-center ${saving ? "bg-[#e2e8f0]" : "bg-[#f1f5f9]"}`}>
                    <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
                  </TouchableOpacity>
                  <TouchableOpacity onPress={submit} disabled={saving} className={`flex-1 py-3 rounded-xl items-center ${saving ? "bg-[#94a3b8]" : "bg-[#0f172a]"}`}>
                    <Text className="text-white font-extrabold">{saving ? "Kaydediliyor..." : "Kaydet"}</Text>
                  </TouchableOpacity>
                </View>
              </View>
            </TouchableOpacity>
          </TouchableOpacity>
        </Modal>
      ) : null}

      {bulkVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setBulkVisible(false)}>
          <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setBulkVisible(false)}>
            <TouchableOpacity activeOpacity={1} onPress={() => {}}>
              <View className="bg-white rounded-2xl overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                  <Text className="text-[#0f172a] font-extrabold">Sabit Tatilleri Aktar</Text>
                  <TouchableOpacity onPress={() => setBulkVisible(false)} className="px-3 py-2 rounded-xl bg-[#f1f5f9]">
                    <Text className="text-[#334155] font-extrabold">Kapat</Text>
                  </TouchableOpacity>
                </View>
                <ScrollView style={{ maxHeight: 360 }} contentContainerStyle={{ padding: 16 }}>
                  <Text className="text-[#64748b] font-semibold">Başlangıç Yılı *</Text>
                  <TextInput
                    value={bStart}
                    onChangeText={setBStart}
                    keyboardType="number-pad"
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
                  />
                  <Text className="mt-4 text-[#64748b] font-semibold">Bitiş Yılı *</Text>
                  <TextInput
                    value={bEnd}
                    onChangeText={setBEnd}
                    keyboardType="number-pad"
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
                  />
                </ScrollView>
                <View className="p-4 border-t border-[#f1f5f9] flex-row gap-2">
                  <TouchableOpacity onPress={() => setBulkVisible(false)} disabled={bulkSaving} className={`flex-1 py-3 rounded-xl items-center ${bulkSaving ? "bg-[#e2e8f0]" : "bg-[#f1f5f9]"}`}>
                    <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
                  </TouchableOpacity>
                  <TouchableOpacity onPress={submitBulk} disabled={bulkSaving} className={`flex-1 py-3 rounded-xl items-center ${bulkSaving ? "bg-[#fde68a]" : "bg-[#fbbf24]"}`}>
                    <Text className="text-[#0f172a] font-extrabold">{bulkSaving ? "İşleniyor..." : "Aktar"}</Text>
                  </TouchableOpacity>
                </View>
              </View>
            </TouchableOpacity>
          </TouchableOpacity>
        </Modal>
      ) : null}
    </View>
  );
}

