import React, { useEffect, useMemo, useState } from "react";
import { ActivityIndicator, Modal, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { StatusPopup } from "../StatusPopup";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";
import { ayarlarService, CalismaSekli } from "../../services/ayarlarApi";

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

function fmtTimeSpan(value: any) {
  if (value == null) return "00:00";
  if (typeof value === "string") {
    // "HH:mm:ss" or "HH:mm"
    const m = value.match(/^(\d{1,2}):(\d{2})(?::(\d{2}))?$/);
    if (m) return `${m[1].padStart(2, "0")}:${m[2]}`;
    return value;
  }
  // Some serializers may send { ticks: ... } etc; fallback:
  try {
    return String(value);
  } catch {
    return "00:00";
  }
}

function toTimeSpanInput(value: string) {
  const m = (value ?? "").trim().match(/^(\d{1,2}):(\d{2})$/);
  if (!m) return null;
  const hh = Math.min(23, Math.max(0, asInt(m[1], 0)));
  const mm = Math.min(59, Math.max(0, asInt(m[2], 0)));
  return `${String(hh).padStart(2, "0")}:${String(mm).padStart(2, "0")}:00`;
}

export function VardiyalarScreen(props: { user: any; abilities: any; onOpenMenu: () => void }) {
  const actions = props.abilities?.actions?.Vardiyalar ?? props.abilities?.Actions?.Vardiyalar ?? {};
  const canCreate = !!(actions?.Create ?? actions?.create);
  const canUpdate = !!(actions?.Update ?? actions?.update);
  const canDelete = !!(actions?.Delete ?? actions?.delete);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<CalismaSekli[]>([]);
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

  const refresh = async () => {
    setLoading(true);
    setError(null);
    try {
      const resp = await ayarlarService.listVardiyalar();
      if (!resp?.success) throw new Error(resp?.message || "Liste alınamadı.");
      setItems(resp.data ?? []);
    } catch (e: any) {
      setError(e?.message || "Hata oluştu.");
    } finally {
      setLoading(false);
    }
  };
  async function refreshForce() {
    setLoading(true);
    setError(null);
    try {
      const resp = await ayarlarService.listVardiyalar({ forceRefresh: true });
      if (!resp?.success) throw new Error(resp?.message || "Liste alınamadı.");
      setItems(resp.data ?? []);
    } catch (e: any) {
      setError(e?.message || "Hata oluştu.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    refresh();
  }, []);

  const rows = useMemo(() => {
    const qq = q.trim().toLowerCase();
    const mapped = (items ?? []).map((x: any) => {
      const id = pick<number>(x, "id", "Id") ?? 0;
      const ad = (pick<string>(x, "ad", "Ad") ?? "").toString();
      const bas = fmtTimeSpan(pick<any>(x, "baslangic", "Baslangic"));
      const bit = fmtTimeSpan(pick<any>(x, "bitis", "Bitis"));
      const t1 = fmtTimeSpan(pick<any>(x, "baslangicTolerans", "BaslangicTolerans"));
      const t2 = fmtTimeSpan(pick<any>(x, "bitisTolerans", "BitisTolerans"));
      const yem = fmtTimeSpan(pick<any>(x, "yemekAktiflestirme", "YemekAktiflestirme"));
      return { id, ad, bas, bit, t1, t2, yem, raw: x as CalismaSekli };
    });
    const filtered = !qq ? mapped : mapped.filter((r) => `${r.id} ${r.ad}`.toLowerCase().includes(qq));
    return filtered.sort((a, b) => a.ad.localeCompare(b.ad, "tr"));
  }, [items, q]);

  // Form
  const [formVisible, setFormVisible] = useState(false);
  const [isEdit, setIsEdit] = useState(false);
  const [fId, setFId] = useState<number>(0);
  const [fAd, setFAd] = useState("");
  const [fBas, setFBas] = useState("08:00");
  const [fBit, setFBit] = useState("17:00");
  const [fT1, setFT1] = useState("00:00");
  const [fT2, setFT2] = useState("00:00");
  const [fYem, setFYem] = useState("00:00");
  const [saving, setSaving] = useState(false);

  const openCreate = () => {
    setIsEdit(false);
    setFId(0);
    setFAd("");
    setFBas("08:00");
    setFBit("17:00");
    setFT1("00:00");
    setFT2("00:00");
    setFYem("00:00");
    setFormVisible(true);
  };

  const openEdit = (r: any) => {
    setIsEdit(true);
    setFId(r.id);
    setFAd(r.ad);
    setFBas(r.bas);
    setFBit(r.bit);
    setFT1(r.t1);
    setFT2(r.t2);
    setFYem(r.yem);
    setFormVisible(true);
  };

  const submit = async () => {
    if (saving) return;
    const ad = (fAd ?? "").trim();
    if (!ad) {
      showPopup("error", "Vardiya adı boş olamaz.");
      return;
    }
    const bas = toTimeSpanInput(fBas);
    const bit = toTimeSpanInput(fBit);
    const t1 = toTimeSpanInput(fT1);
    const t2 = toTimeSpanInput(fT2);
    const yem = toTimeSpanInput(fYem);
    if (!bas || !bit || !t1 || !t2 || !yem) {
      showPopup("error", "Saat formatı HH:mm olmalı.");
      return;
    }
    setSaving(true);
    try {
      const payload: any = {
        Ad: ad,
        Baslangic: bas,
        Bitis: bit,
        BaslangicTolerans: t1,
        BitisTolerans: t2,
        YemekAktiflestirme: yem,
      };
      if (!isEdit) {
        const resp = await ayarlarService.createVardiya(payload);
        if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
        showPopup("success", resp?.message || "Vardiya eklendi.");
      } else {
        const resp = await ayarlarService.updateVardiya(fId, payload);
        if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
        showPopup("success", resp?.message || "Vardiya güncellendi.");
      }
      setFormVisible(false);
      await refresh();
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setSaving(false);
    }
  };

  const [delVisible, setDelVisible] = useState(false);
  const [delId, setDelId] = useState<number | null>(null);
  const [delLabel, setDelLabel] = useState("");
  const [delSaving, setDelSaving] = useState(false);
  const askDelete = (r: any) => {
    setDelId(r.id);
    setDelLabel(r.ad);
    setDelVisible(true);
  };
  const doDelete = async () => {
    if (delSaving) return;
    if (!delId) return;
    setDelSaving(true);
    try {
      const resp = await ayarlarService.deleteVardiya(delId);
      if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
      showPopup("success", resp?.message || "Vardiya silindi.");
      setDelVisible(false);
      await refresh();
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setDelSaving(false);
    }
  };

  const topBar = (
    <>
      <PageHeader
        title="Vardiyalar"
        onOpenMenu={props.onOpenMenu}
        rightIcon="refresh"
        onRightPress={refreshForce}
        rightIcon2="bell-outline"
        onRightPress2={() => quickMenu.open("notif")}
        rightBadge2={notif.unreadCount}
        rightA11yLabel2="Bildirimler ve hesap"
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
          <TouchableOpacity onPress={refresh} className="mt-4 px-4 py-3 rounded-xl bg-[#0f172a]">
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
            <TextInput
              value={q}
              onChangeText={setQ}
              placeholder="Ara (Id / Ad)"
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
              <Text className="text-[#64748b] font-semibold">Kayıt bulunamadı.</Text>
            </View>
          ) : (
            rows.map((r) => (
              <View key={`v_${r.id}`} className="mb-3 bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                  <View className="flex-1">
                    <Text className="text-[#0f172a] font-extrabold" numberOfLines={1}>
                      {r.ad}
                    </Text>
                    <Text className="text-[#64748b] font-semibold text-[12px]" numberOfLines={2}>
                      {r.bas} - {r.bit} • Tol: {r.t1}/{r.t2} • Yemek: {r.yem}
                    </Text>
                  </View>
                  <View className="flex-row gap-2">
                    {canUpdate ? (
                      <TouchableOpacity onPress={() => openEdit(r)} className="px-3 py-2 rounded-xl bg-[#fbbf24]">
                        <MaterialCommunityIcons name="pencil" size={18} color="#0f172a" />
                      </TouchableOpacity>
                    ) : null}
                    {canDelete ? (
                      <TouchableOpacity onPress={() => askDelete(r)} className="px-3 py-2 rounded-xl bg-[#fee2e2]">
                        <MaterialCommunityIcons name="trash-can" size={18} color="#dc2626" />
                      </TouchableOpacity>
                    ) : null}
                  </View>
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
                  <Text className="text-[#0f172a] font-extrabold">{isEdit ? "Vardiya Düzenle" : "Yeni Vardiya"}</Text>
                  <TouchableOpacity onPress={() => setFormVisible(false)} className="px-3 py-2 rounded-xl bg-[#f1f5f9]">
                    <Text className="text-[#334155] font-extrabold">Kapat</Text>
                  </TouchableOpacity>
                </View>
                <ScrollView style={{ maxHeight: 560 }} contentContainerStyle={{ padding: 16 }}>
                  {isEdit ? (
                    <>
                      <Text className="text-[#64748b] font-semibold">Id</Text>
                      <Text className="mt-2 px-4 py-3 rounded-xl bg-[#f1f5f9] border border-[#e2e8f0] text-[#0f172a] font-extrabold">{fId}</Text>
                    </>
                  ) : null}

                  <Text className={`${isEdit ? "mt-4" : ""} text-[#64748b] font-semibold`}>Ad *</Text>
                  <TextInput
                    value={fAd}
                    onChangeText={setFAd}
                    placeholder="Vardiya adı"
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
                  />

                  <View className="mt-4 flex-row gap-2">
                    <View className="flex-1">
                      <Text className="text-[#64748b] font-semibold">Başlangıç (HH:mm)</Text>
                      <TextInput value={fBas} onChangeText={setFBas} className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" />
                    </View>
                    <View className="flex-1">
                      <Text className="text-[#64748b] font-semibold">Bitiş (HH:mm)</Text>
                      <TextInput value={fBit} onChangeText={setFBit} className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" />
                    </View>
                  </View>

                  <View className="mt-4 flex-row gap-2">
                    <View className="flex-1">
                      <Text className="text-[#64748b] font-semibold">Başlangıç Tol. (HH:mm)</Text>
                      <TextInput value={fT1} onChangeText={setFT1} className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" />
                    </View>
                    <View className="flex-1">
                      <Text className="text-[#64748b] font-semibold">Bitiş Tol. (HH:mm)</Text>
                      <TextInput value={fT2} onChangeText={setFT2} className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" />
                    </View>
                  </View>

                  <Text className="mt-4 text-[#64748b] font-semibold">Yemek Aktifleş. (HH:mm)</Text>
                  <TextInput value={fYem} onChangeText={setFYem} className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" />
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

      {delVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setDelVisible(false)}>
          <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setDelVisible(false)}>
            <TouchableOpacity activeOpacity={1} onPress={() => {}}>
              <View className="bg-white rounded-2xl overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9]">
                  <Text className="text-[#0f172a] font-extrabold">Silme Onayı</Text>
                </View>
                <View className="p-4">
                  <Text className="text-[#334155] font-semibold">"{delLabel}" vardiyasını silmek istiyor musunuz?</Text>
                </View>
                <View className="p-4 border-t border-[#f1f5f9] flex-row gap-2">
                  <TouchableOpacity onPress={() => setDelVisible(false)} disabled={delSaving} className={`flex-1 py-3 rounded-xl items-center ${delSaving ? "bg-[#e2e8f0]" : "bg-[#f1f5f9]"}`}>
                    <Text className="text-[#334155] font-extrabold">İptal</Text>
                  </TouchableOpacity>
                  <TouchableOpacity onPress={doDelete} disabled={delSaving} className={`flex-1 py-3 rounded-xl items-center ${delSaving ? "bg-[#fca5a5]" : "bg-[#dc2626]"}`}>
                    <Text className="text-white font-extrabold">{delSaving ? "Siliniyor..." : "Sil"}</Text>
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

