import React, { useEffect, useMemo, useState } from "react";
import { ActivityIndicator, Modal, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { StatusPopup } from "../StatusPopup";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";
import { organizasyonService, IsyeriItem, LookupItem } from "../../services/organizasyonApi";
import { useUiPrefs } from "../../services/uiPrefs";

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

function SelectModal(props: {
  visible: boolean;
  title: string;
  items: { key: string; label: string }[];
  onClose: () => void;
  onPick: (key: string) => void;
}) {
  return (
    <Modal visible={props.visible} transparent animationType="fade" onRequestClose={props.onClose}>
      <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={props.onClose}>
        <TouchableOpacity activeOpacity={1} onPress={() => {}}>
          <View className="bg-white rounded-2xl overflow-hidden">
            <View className="px-4 py-3 border-b border-[#f1f5f9]">
              <Text className="text-[#0f172a] font-extrabold text-[16px]">{props.title}</Text>
            </View>
            <ScrollView style={{ maxHeight: 520 }}>
              {props.items.map((it, idx) => (
                <TouchableOpacity
                  key={`${it.key}_${idx}`}
                  className="px-4 py-3 border-b border-[#f1f5f9]"
                  onPress={() => {
                    props.onPick(it.key);
                    props.onClose();
                  }}
                >
                  <Text className="text-[#0f172a] font-semibold">{it.label}</Text>
                </TouchableOpacity>
              ))}
            </ScrollView>
            <View className="p-3">
              <TouchableOpacity className="bg-[#f1f5f9] rounded-xl py-3 items-center" onPress={props.onClose}>
                <Text className="text-[#334155] font-extrabold">Kapat</Text>
              </TouchableOpacity>
            </View>
          </View>
        </TouchableOpacity>
      </TouchableOpacity>
    </Modal>
  );
}

export function IsyerleriScreen(props: { user: any; abilities: any; onOpenMenu: () => void }) {
  const { listRowPadClass } = useUiPrefs();
  const actions = props.abilities?.actions?.Isyerler ?? props.abilities?.Actions?.Isyerler ?? {};
  const canCreate = !!(actions?.Create ?? actions?.create);
  const canUpdate = !!(actions?.Update ?? actions?.update);
  const canDelete = !!(actions?.Delete ?? actions?.delete);

  const rolId: number | null | undefined = props.abilities?.rolId ?? props.abilities?.RolId;
  const isAdmin = !!(props.abilities?.isAdmin ?? props.abilities?.IsAdmin ?? (rolId === 1 || rolId === 2));

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [firmalar, setFirmalar] = useState<LookupItem[]>([]);
  const [aktifFirmaId, setAktifFirmaId] = useState<number | null>(null);
  const [selectedFirmaId, setSelectedFirmaId] = useState<number | null>(null);

  const [items, setItems] = useState<IsyeriItem[]>([]);
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
    <>
      <PageHeader
        title="İşyerleri"
        onOpenMenu={props.onOpenMenu}
        rightIcon="refresh"
        onRightPress={() => refresh(undefined, true)}
        rightIcon2={canCreate ? "plus" : "bell-outline"}
        onRightPress2={canCreate ? openCreate : () => quickMenu.open("notif")}
        rightBadge2={canCreate ? undefined : notif.unreadCount}
      />
      {quickMenu.modal}
    </>
  );

  const loadLookups = async (force?: boolean) => {
    const l = await organizasyonService.isyeriLookups({ forceRefresh: !!force });
    if (!l?.success) throw new Error(l?.message || "Lookups alınamadı.");
    const fs = l.data?.firmalar ?? [];
    setFirmalar(fs);
    const af = l.data?.aktifFirmaId ?? null;
    setAktifFirmaId(af);
    if (!isAdmin) setSelectedFirmaId(af);
    if (isAdmin && selectedFirmaId == null && af != null) setSelectedFirmaId(af);
  };

  const refresh = async (firmaIdParam?: number | null, force?: boolean) => {
    setLoading(true);
    setError(null);
    try {
      await loadLookups(force);
      const resp = await organizasyonService.listIsyerleri(
        { firmaId: isAdmin ? firmaIdParam ?? selectedFirmaId ?? undefined : undefined },
        { forceRefresh: !!force }
      );
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
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const firmaItems = useMemo(() => {
    const list = firmalar.map((f) => ({ key: String(f.id), label: f.ad }));
    return [{ key: "0", label: "-- Tümü --" }, ...list];
  }, [firmalar]);

  const filtered = useMemo(() => {
    const qq = q.trim().toLowerCase();
    const rows = (items ?? []).map((x) => ({
      firmaId: pick<number>(x, "firmaId", "FirmaId") ?? 0,
      isyeriId: pick<number>(x, "isyeriId", "IsyeriId") ?? 0,
      ad: (pick<string>(x, "ad", "Ad") ?? "").toString(),
    }));
    if (!qq) return rows.sort((a, b) => a.ad.localeCompare(b.ad, "tr"));
    return rows
      .filter((r) => `${r.firmaId} ${r.isyeriId} ${r.ad}`.toLowerCase().includes(qq))
      .sort((a, b) => a.ad.localeCompare(b.ad, "tr"));
  }, [items, q]);

  // Create/Edit modal
  const [formVisible, setFormVisible] = useState(false);
  const [isEdit, setIsEdit] = useState(false);
  const [fFirmaId, setFFirmaId] = useState<string>("0");
  const [fIsyeriId, setFIsyeriId] = useState<string>("0");
  const [fAd, setFAd] = useState<string>("");
  const [firmaFilterModal, setFirmaFilterModal] = useState(false);
  const [firmaFormModal, setFirmaFormModal] = useState(false);
  const [saving, setSaving] = useState(false);

  function openCreate() {
    setIsEdit(false);
    setFAd("");
    setFIsyeriId("0");
    setFFirmaId(String(isAdmin ? selectedFirmaId ?? aktifFirmaId ?? 0 : aktifFirmaId ?? 0));
    setFormVisible(true);
  }

  const openEdit = (row: { firmaId: number; isyeriId: number; ad: string }) => {
    setIsEdit(true);
    setFFirmaId(String(row.firmaId));
    setFIsyeriId(String(row.isyeriId));
    setFAd(row.ad);
    setFormVisible(true);
  };

  const submit = async () => {
    if (saving) return;
    const firmaId = asInt(fFirmaId, 0);
    const isyeriId = asInt(fIsyeriId, 0);
    const ad = (fAd ?? "").trim();
    if (firmaId <= 0) {
      showPopup("error", "Firma seçiniz.");
      return;
    }
    if (isyeriId <= 0) {
      showPopup("error", "İşyeri Id geçersiz.");
      return;
    }
    if (!ad) {
      showPopup("error", "İşyeri adı boş olamaz.");
      return;
    }
    setSaving(true);
    try {
      if (!isEdit) {
        const resp = await organizasyonService.createIsyeri({ firmaId, isyeriId, ad });
        if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
        showPopup("success", resp?.message || "İşyeri kaydedildi.");
      } else {
        const resp = await organizasyonService.updateIsyeri({ firmaId, isyeriId, ad });
        if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
        showPopup("success", resp?.message || "İşyeri güncellendi.");
      }
      setFormVisible(false);
      await refresh(selectedFirmaId);
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setSaving(false);
    }
  };

  // Delete confirm
  const [delVisible, setDelVisible] = useState(false);
  const [delFirmaId, setDelFirmaId] = useState<number | null>(null);
  const [delIsyeriId, setDelIsyeriId] = useState<number | null>(null);
  const [delLabel, setDelLabel] = useState<string>("");
  const [delSaving, setDelSaving] = useState(false);

  const askDelete = (row: { firmaId: number; isyeriId: number; ad: string }) => {
    setDelFirmaId(row.firmaId);
    setDelIsyeriId(row.isyeriId);
    setDelLabel(row.ad);
    setDelVisible(true);
  };

  const doDelete = async () => {
    if (delSaving) return;
    if (!delFirmaId || !delIsyeriId) return;
    setDelSaving(true);
    try {
      const resp = await organizasyonService.deleteIsyeri({ firmaId: delFirmaId, isyeriId: delIsyeriId });
      if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
      showPopup("success", resp?.message || "İşyeri silindi.");
      setDelVisible(false);
      await refresh(selectedFirmaId);
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setDelSaving(false);
    }
  };

  const selectedFirmaLabel = useMemo(() => {
    const id = selectedFirmaId ?? 0;
    if (!id) return "-- Tümü --";
    return firmalar.find((x) => x.id === id)?.ad ?? String(id);
  }, [firmalar, selectedFirmaId]);

  if (loading) {
    return (
      <View className="flex-1 bg-[#f8fafc]">
        {header}
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
        <View className="flex-1 items-center justify-center px-6">
          <Text className="text-[#dc2626] font-extrabold text-center">{error}</Text>
          <TouchableOpacity onPress={() => refresh(selectedFirmaId)} className="mt-4 px-4 py-3 rounded-xl bg-[#0f172a]">
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

      <ScrollView className="flex-1 px-4" contentContainerStyle={{ paddingBottom: 24 }}>
        <View className="mt-4 bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
          <View className="px-4 py-3 border-b border-[#f1f5f9]">
            <Text className="text-[#0f172a] font-extrabold">Filtre</Text>
            {isAdmin ? (
              <TouchableOpacity onPress={() => setFirmaFilterModal(true)} className="mt-2 px-4 py-3 rounded-xl bg-[#f8fafc] border border-[#e2e8f0]">
                <Text className="text-[#0f172a] font-extrabold">{selectedFirmaLabel}</Text>
                <Text className="text-[#64748b] font-semibold text-[12px]">Firma</Text>
              </TouchableOpacity>
            ) : null}
            <TextInput
              value={q}
              onChangeText={setQ}
              placeholder="Ara (FirmaId / İşyeriId / Ad)"
              className="mt-2 px-4 py-3 rounded-xl bg-[#f8fafc] border border-[#e2e8f0] text-[#0f172a] font-semibold"
              placeholderTextColor="#94a3b8"
            />
            <TouchableOpacity
              onPress={() => refresh(selectedFirmaId)}
              className="mt-3 px-4 py-3 rounded-xl bg-[#0f172a] items-center"
            >
              <Text className="text-white font-extrabold">Uygula</Text>
            </TouchableOpacity>
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
              <View key={`i_${r.firmaId}_${r.isyeriId}`} className="mb-3 bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
                <View className={`px-4 ${listRowPadClass} border-b border-[#f1f5f9] flex-row items-center justify-between`}>
                  <View className="flex-1">
                    <Text className="text-[#0f172a] font-extrabold" numberOfLines={1}>
                      {r.ad}
                    </Text>
                    <Text className="text-[#64748b] font-semibold text-[12px]" numberOfLines={1}>
                      FirmaId: {r.firmaId} • İşyeriId: {r.isyeriId}
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

      <SelectModal
        visible={firmaFilterModal}
        title="Firma Seç"
        items={firmaItems}
        onClose={() => setFirmaFilterModal(false)}
        onPick={(k) => {
          const v = asInt(k, 0);
          setSelectedFirmaId(v || null);
        }}
      />

      {formVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setFormVisible(false)}>
          <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setFormVisible(false)}>
            <TouchableOpacity activeOpacity={1} onPress={() => {}}>
              <View className="bg-white rounded-2xl overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                  <Text className="text-[#0f172a] font-extrabold">{isEdit ? "İşyeri Düzenle" : "Yeni İşyeri"}</Text>
                  <TouchableOpacity onPress={() => setFormVisible(false)} className="px-3 py-2 rounded-xl bg-[#f1f5f9]">
                    <Text className="text-[#334155] font-extrabold">Kapat</Text>
                  </TouchableOpacity>
                </View>
                <ScrollView style={{ maxHeight: 560 }} contentContainerStyle={{ padding: 16 }}>
                  <Text className="text-[#64748b] font-semibold">Firma</Text>
                  <TouchableOpacity
                    disabled={!isAdmin || isEdit}
                    onPress={() => setFirmaFormModal(true)}
                    className={`mt-2 px-4 py-3 rounded-xl border ${
                      !isAdmin || isEdit ? "bg-[#f1f5f9] border-[#e2e8f0]" : "bg-white border-[#e2e8f0]"
                    }`}
                  >
                    <Text className="text-[#0f172a] font-extrabold">
                      {firmalar.find((x) => x.id === asInt(fFirmaId, 0))?.ad ?? String(asInt(fFirmaId, 0))}
                    </Text>
                    <Text className="text-[#64748b] font-semibold text-[12px]">{isEdit ? "Değiştirilemez" : isAdmin ? "Seç" : ""}</Text>
                  </TouchableOpacity>

                  <Text className="mt-4 text-[#64748b] font-semibold">İşyeri Id *</Text>
                  <TextInput
                    value={fIsyeriId}
                    onChangeText={setFIsyeriId}
                    editable={!isEdit}
                    keyboardType="number-pad"
                    className={`mt-2 px-4 py-3 rounded-xl border ${
                      isEdit ? "bg-[#f1f5f9] border-[#e2e8f0]" : "bg-white border-[#e2e8f0]"
                    } text-[#0f172a] font-semibold`}
                  />

                  <Text className="mt-4 text-[#64748b] font-semibold">İşyeri Adı *</Text>
                  <TextInput
                    value={fAd}
                    onChangeText={setFAd}
                    placeholder="İşyeri adı"
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
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
                    onPress={() => {
                      submit();
                    }}
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

      <SelectModal
        visible={firmaFormModal}
        title="Firma Seç"
        items={firmalar.map((f) => ({ key: String(f.id), label: f.ad }))}
        onClose={() => setFirmaFormModal(false)}
        onPick={(k) => {
          setFFirmaId(String(asInt(k, 0)));
        }}
      />

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
                    "{delLabel}" işyerini silmek istiyor musunuz?
                  </Text>
                </View>
                <View className="p-4 border-t border-[#f1f5f9] flex-row gap-2">
                  <TouchableOpacity
                    onPress={() => setDelVisible(false)}
                    disabled={delSaving}
                    className={`flex-1 py-3 rounded-xl items-center ${delSaving ? "bg-[#e2e8f0]" : "bg-[#f1f5f9]"}`}
                  >
                    <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
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

