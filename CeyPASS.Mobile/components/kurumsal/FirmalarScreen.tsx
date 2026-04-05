import React, { useEffect, useMemo, useState } from "react";
import { ActivityIndicator, Modal, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { StatusPopup } from "../StatusPopup";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";
import { organizasyonService, FirmaRow } from "../../services/organizasyonApi";

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

export function FirmalarScreen(props: { user: any; abilities: any; onOpenMenu: () => void }) {
  const actions = props.abilities?.actions?.Firmalar ?? props.abilities?.Actions?.Firmalar ?? {};
  const canCreate = !!(actions?.Create ?? actions?.create);
  const canUpdate = !!(actions?.Update ?? actions?.update);
  const canDelete = !!(actions?.Delete ?? actions?.delete);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<FirmaRow[]>([]);
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

  const header = (
    <PageHeader
      title="Firmalar"
      onOpenMenu={props.onOpenMenu}
      rightIcon="refresh"
      onRightPress={() => refresh(true)}
      rightIcon2={canCreate ? "plus" : "bell-outline"}
      onRightPress2={canCreate ? openCreate : () => quickMenu.open("notif")}
      rightBadge2={canCreate ? undefined : notif.unreadCount}
      rightA11yLabel2={canCreate ? "Firma ekle" : "Bildirimler ve hesap"}
    />
  );

  const refresh = async (force?: boolean) => {
    setLoading(true);
    setError(null);
    try {
      const resp = await organizasyonService.listFirmalar({ forceRefresh: !!force });
      if (!resp?.success) throw new Error(resp?.message || "Liste alınamadı.");
      setItems(resp.data ?? []);
    } catch (e: any) {
      setError(e?.message || "Hata oluştu.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refresh();
  }, []);

  const filtered = useMemo(() => {
    const qq = q.trim().toLowerCase();
    const rows = (items ?? []).map((x) => ({
      firmaId: pick<number>(x, "firmaId", "FirmaId") ?? 0,
      firmaAdi: (pick<string>(x, "firmaAdi", "FirmaAdi") ?? "").toString(),
      itBirimMail: (pick<string | null>(x, "itBirimMail", "ITBirimMail") ?? "").toString(),
    }));
    if (!qq) return rows.sort((a, b) => a.firmaAdi.localeCompare(b.firmaAdi, "tr"));
    return rows
      .filter((r) => `${r.firmaId} ${r.firmaAdi} ${r.itBirimMail}`.toLowerCase().includes(qq))
      .sort((a, b) => a.firmaAdi.localeCompare(b.firmaAdi, "tr"));
  }, [items, q]);

  // Create/Edit modal
  const [formVisible, setFormVisible] = useState(false);
  const [isEdit, setIsEdit] = useState(false);
  const [fId, setFId] = useState<string>("0");
  const [fAd, setFAd] = useState<string>("");
  const [fMail, setFMail] = useState<string>("");
  const [saving, setSaving] = useState(false);

  async function openCreate() {
    setIsEdit(false);
    setFAd("");
    setFMail("");
    setFId("0");
    setFormVisible(true);
    try {
      const n = await organizasyonService.nextFirmaId();
      if (n?.success && typeof n.data === "number") setFId(String(n.data));
    } catch {}
  }

  const openEdit = (row: { firmaId: number; firmaAdi: string; itBirimMail: string }) => {
    setIsEdit(true);
    setFId(String(row.firmaId));
    setFAd(row.firmaAdi);
    setFMail(row.itBirimMail ?? "");
    setFormVisible(true);
  };

  const submit = async () => {
    if (saving) return;
    const id = asInt(fId, 0);
    const ad = (fAd ?? "").trim();
    if (!ad) {
      showPopup("error", "Firma adı boş olamaz.");
      return;
    }
    if (!isEdit && id <= 0) {
      showPopup("error", "Firma Id geçersiz.");
      return;
    }
    setSaving(true);
    try {
      if (!isEdit) {
        const resp = await organizasyonService.createFirma({ firmaId: id, firmaAdi: ad, itBirimMail: (fMail ?? "").trim() || null });
        if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
        showPopup("success", resp?.message || "Firma kaydedildi.");
      } else {
        const resp = await organizasyonService.updateFirma(id, { firmaId: id, firmaAdi: ad, itBirimMail: (fMail ?? "").trim() || null });
        if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
        showPopup("success", resp?.message || "Firma güncellendi.");
      }
      setFormVisible(false);
      await refresh();
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setSaving(false);
    }
  };

  // Delete confirm
  const [delVisible, setDelVisible] = useState(false);
  const [delId, setDelId] = useState<number | null>(null);
  const [delLabel, setDelLabel] = useState<string>("");
  const [delSaving, setDelSaving] = useState(false);

  const askDelete = (row: { firmaId: number; firmaAdi: string }) => {
    setDelId(row.firmaId);
    setDelLabel(row.firmaAdi);
    setDelVisible(true);
  };

  const doDelete = async () => {
    if (delSaving) return;
    if (!delId) return;
    setDelSaving(true);
    try {
      const resp = await organizasyonService.deleteFirma(delId);
      if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
      showPopup("success", resp?.message || "Firma silindi.");
      setDelVisible(false);
      await refresh();
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setDelSaving(false);
    }
  };

  if (loading) {
    return (
      <View className="flex-1 bg-[#f8fafc]">
        {header}
        {quickMenu.modal}
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
        {header}
        {quickMenu.modal}
        <View className="flex-1 items-center justify-center px-6">
          <Text className="text-[#dc2626] font-extrabold text-center">{error}</Text>
          <TouchableOpacity onPress={() => refresh(true)} className="mt-4 px-4 py-3 rounded-xl bg-[#0f172a]">
            <Text className="text-white font-extrabold">Tekrar Dene</Text>
          </TouchableOpacity>
        </View>
      </View>
    );
  }

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} useModal={false} />
      {header}
      {quickMenu.modal}

      <ScrollView className="flex-1 px-4" contentContainerStyle={{ paddingBottom: 24 }}>
        <View className="mt-4 bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
          <View className="px-4 py-3 border-b border-[#f1f5f9]">
            <Text className="text-[#0f172a] font-extrabold">Filtre</Text>
            <TextInput
              value={q}
              onChangeText={setQ}
              placeholder="Ara (Id / Ad / Mail)"
              className="mt-2 px-4 py-3 rounded-xl bg-[#f8fafc] border border-[#e2e8f0] text-[#0f172a] font-semibold"
              placeholderTextColor="#94a3b8"
            />
          </View>
          <View className="px-4 py-3">
            <Text className="text-[#64748b] font-semibold text-[12px]">Toplam {filtered.length} kayıt</Text>
          </View>
        </View>

        <View className="mt-4">
          {!filtered.length ? (
            <View className="py-10 items-center">
              <Text className="text-[#64748b] font-semibold">Kayıt bulunamadı.</Text>
            </View>
          ) : (
            filtered.map((r) => (
              <View key={`f_${r.firmaId}`} className="mb-3 bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                  <View className="flex-1">
                    <Text className="text-[#0f172a] font-extrabold" numberOfLines={1}>
                      {r.firmaAdi}
                    </Text>
                    <Text className="text-[#64748b] font-semibold text-[12px]" numberOfLines={1}>
                      Id: {r.firmaId} {r.itBirimMail ? `• IT: ${r.itBirimMail}` : ""}
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
                  <Text className="text-[#0f172a] font-extrabold">{isEdit ? "Firma Düzenle" : "Yeni Firma"}</Text>
                  <TouchableOpacity onPress={() => setFormVisible(false)} className="px-3 py-2 rounded-xl bg-[#f1f5f9]">
                    <Text className="text-[#334155] font-extrabold">Kapat</Text>
                  </TouchableOpacity>
                </View>
                <ScrollView style={{ maxHeight: 560 }} contentContainerStyle={{ padding: 16 }}>
                  <Text className="text-[#64748b] font-semibold">Firma Id</Text>
                  <TextInput
                    value={fId}
                    onChangeText={setFId}
                    editable={!isEdit}
                    keyboardType="number-pad"
                    className={`mt-2 px-4 py-3 rounded-xl border ${
                      isEdit ? "bg-[#f1f5f9] border-[#e2e8f0]" : "bg-white border-[#e2e8f0]"
                    } text-[#0f172a] font-semibold`}
                  />

                  <Text className="mt-4 text-[#64748b] font-semibold">Firma Adı *</Text>
                  <TextInput
                    value={fAd}
                    onChangeText={setFAd}
                    placeholder="Firma adı"
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
                  />

                  <Text className="mt-4 text-[#64748b] font-semibold">IT Birim Mail</Text>
                  <TextInput
                    value={fMail}
                    onChangeText={setFMail}
                    placeholder="opsiyonel"
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
                    autoCapitalize="none"
                  />
                </ScrollView>
                <View className="p-4 border-t border-[#f1f5f9] flex-row gap-2">
                  <TouchableOpacity
                    onPress={() => setFormVisible(false)}
                    disabled={saving}
                    className={`flex-1 py-3 rounded-xl items-center ${saving ? "bg-[#e2e8f0]" : "bg-[#f1f5f9]"}`}
                  >
                    <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    onPress={submit}
                    disabled={saving}
                    className={`flex-1 py-3 rounded-xl items-center ${saving ? "bg-[#94a3b8]" : "bg-[#0f172a]"}`}
                  >
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
                  <Text className="text-[#334155] font-semibold">
                    "{delLabel}" firmasını silmek istiyor musunuz?
                  </Text>
                </View>
                <View className="p-4 border-t border-[#f1f5f9] flex-row gap-2">
                  <TouchableOpacity
                    onPress={() => setDelVisible(false)}
                    disabled={delSaving}
                    className={`flex-1 py-3 rounded-xl items-center ${delSaving ? "bg-[#e2e8f0]" : "bg-[#f1f5f9]"}`}
                  >
                    <Text className="text-[#334155] font-extrabold">İptal</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    onPress={doDelete}
                    disabled={delSaving}
                    className={`flex-1 py-3 rounded-xl items-center ${delSaving ? "bg-[#fca5a5]" : "bg-[#dc2626]"}`}
                  >
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

