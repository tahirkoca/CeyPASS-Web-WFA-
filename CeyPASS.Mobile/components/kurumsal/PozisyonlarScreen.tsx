import React, { useEffect, useMemo, useState } from "react";
import { ActivityIndicator, Modal, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { StatusPopup } from "../StatusPopup";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";
import { organizasyonService, LookupItem } from "../../services/organizasyonApi";

function asInt(value: any, def = 0) {
  const n = Number(value);
  return Number.isFinite(n) ? n : def;
}

export function PozisyonlarScreen(props: { user: any; abilities: any; onOpenMenu: () => void }) {
  const actions = props.abilities?.actions?.Pozisyonlar ?? props.abilities?.Actions?.Pozisyonlar ?? {};
  const canCreate = !!(actions?.Create ?? actions?.create);
  const canUpdate = !!(actions?.Update ?? actions?.update);
  const canDelete = !!(actions?.Delete ?? actions?.delete);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<LookupItem[]>([]);
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

  const refresh = async (force?: boolean) => {
    setLoading(true);
    setError(null);
    try {
      const resp = await organizasyonService.listPozisyonlar({ forceRefresh: !!force });
      if (!resp?.success) throw new Error(resp?.message || "Liste alınamadı.");
      setItems(resp.data ?? []);
    } catch (e: any) {
      setError(e?.message || "Hata oluştu.");
    } finally {
      setLoading(false);
    }
  };

  async function refreshForce() {
    await refresh(true);
  }

  useEffect(() => {
    refresh();
  }, []);

  const filtered = useMemo(() => {
    const qq = q.trim().toLowerCase();
    const rows = (items ?? []).map((x: any) => ({
      id: asInt(x?.id ?? x?.Id, 0),
      ad: (x?.ad ?? x?.Ad ?? "").toString(),
    }));
    if (!qq) return rows.sort((a, b) => a.ad.localeCompare(b.ad, "tr"));
    return rows
      .filter((r) => `${r.id} ${r.ad}`.toLowerCase().includes(qq))
      .sort((a, b) => a.ad.localeCompare(b.ad, "tr"));
  }, [items, q]);

  // Create/Edit modal
  const [formVisible, setFormVisible] = useState(false);
  const [isEdit, setIsEdit] = useState(false);
  const [fId, setFId] = useState<number>(0);
  const [fAd, setFAd] = useState<string>("");
  const [fAciklama, setFAciklama] = useState<string>("");
  const [saving, setSaving] = useState(false);

  const openCreate = () => {
    setIsEdit(false);
    setFId(0);
    setFAd("");
    setFAciklama("");
    setFormVisible(true);
  };

  const openEdit = async (id: number) => {
    setIsEdit(true);
    setFId(id);
    setFAd("");
    setFAciklama("");
    setFormVisible(true);
    try {
      const resp = await organizasyonService.getPozisyon(id);
      if (resp?.success && resp.data) {
        setFAd((resp.data.ad ?? "").toString());
        setFAciklama((resp.data.aciklama ?? "").toString());
      }
    } catch {}
  };

  const submit = async () => {
    if (saving) return;
    const ad = (fAd ?? "").trim();
    if (!ad) {
      showPopup("error", "Pozisyon adı boş olamaz.");
      return;
    }
    setSaving(true);
    try {
      if (!isEdit) {
        const resp = await organizasyonService.createPozisyon({ ad, aciklama: (fAciklama ?? "").trim() || "" });
        if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
        showPopup("success", resp?.message || "Pozisyon kaydedildi.");
      } else {
        const resp = await organizasyonService.updatePozisyon(fId, { ad, aciklama: (fAciklama ?? "").trim() || "" });
        if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
        showPopup("success", resp?.message || "Pozisyon güncellendi.");
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

  const askDelete = (row: { id: number; ad: string }) => {
    setDelId(row.id);
    setDelLabel(row.ad);
    setDelVisible(true);
  };

  const doDelete = async () => {
    if (delSaving) return;
    if (!delId) return;
    setDelSaving(true);
    try {
      const resp = await organizasyonService.deletePozisyon(delId);
      if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
      showPopup("success", resp?.message || "Pozisyon silindi.");
      setDelVisible(false);
      await refresh();
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setDelSaving(false);
    }
  };

  const renderTopBar = () => (
    <>
      <PageHeader
        title="Pozisyonlar"
        onOpenMenu={props.onOpenMenu}
        rightIcon="refresh"
        onRightPress={refreshForce}
        rightIcon2={canCreate ? "plus" : "bell-outline"}
        onRightPress2={canCreate ? openCreate : () => quickMenu.open("notif")}
        rightBadge2={canCreate ? undefined : notif.unreadCount}
        rightA11yLabel2={canCreate ? "Pozisyon ekle" : "Bildirimler ve hesap"}
      />
      {quickMenu.modal}
    </>
  );

  if (loading) {
    return (
      <View className="flex-1 bg-[#f8fafc]">
        {renderTopBar()}
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
        {renderTopBar()}
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
      {renderTopBar()}

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
              <View key={`p_${r.id}`} className="mb-3 bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                  <View className="flex-1">
                    <Text className="text-[#0f172a] font-extrabold" numberOfLines={1}>
                      {r.ad}
                    </Text>
                    <Text className="text-[#64748b] font-semibold text-[12px]" numberOfLines={1}>
                      Id: {r.id}
                    </Text>
                  </View>
                  <View className="flex-row gap-2">
                    {canUpdate ? (
                      <TouchableOpacity onPress={() => openEdit(r.id)} className="px-3 py-2 rounded-xl bg-[#fbbf24]">
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
                  <Text className="text-[#0f172a] font-extrabold">{isEdit ? "Pozisyon Düzenle" : "Yeni Pozisyon"}</Text>
                  <TouchableOpacity onPress={() => setFormVisible(false)} className="px-3 py-2 rounded-xl bg-[#f1f5f9]">
                    <Text className="text-[#334155] font-extrabold">Kapat</Text>
                  </TouchableOpacity>
                </View>
                <ScrollView style={{ maxHeight: 560 }} contentContainerStyle={{ padding: 16 }}>
                  {isEdit ? (
                    <>
                      <Text className="text-[#64748b] font-semibold">Pozisyon Id</Text>
                      <Text className="mt-2 px-4 py-3 rounded-xl bg-[#f1f5f9] border border-[#e2e8f0] text-[#0f172a] font-extrabold">
                        {fId}
                      </Text>
                    </>
                  ) : null}

                  <Text className={`${isEdit ? "mt-4" : ""} text-[#64748b] font-semibold`}>Pozisyon Adı *</Text>
                  <TextInput
                    value={fAd}
                    onChangeText={setFAd}
                    placeholder="Pozisyon adı"
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
                  />

                  <Text className="mt-4 text-[#64748b] font-semibold">Açıklama</Text>
                  <TextInput
                    value={fAciklama}
                    onChangeText={setFAciklama}
                    placeholder="opsiyonel"
                    multiline
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
                    style={{ minHeight: 90, textAlignVertical: "top" }}
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
                    "{delLabel}" pozisyonunu silmek istiyor musunuz?
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

