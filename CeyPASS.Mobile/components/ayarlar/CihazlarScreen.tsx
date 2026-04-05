import React, { useEffect, useMemo, useState } from "react";
import { ActivityIndicator, Modal, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { StatusPopup } from "../StatusPopup";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";
import { ayarlarService, Cihaz, CihazListDTO, CihazTip } from "../../services/ayarlarApi";

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

function asBool(value: any, def = false) {
  if (value === true || value === "true" || value === 1 || value === "1") return true;
  if (value === false || value === "false" || value === 0 || value === "0") return false;
  return def;
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

export function CihazlarScreen(props: { user: any; abilities: any; onOpenMenu: () => void }) {
  const actions = props.abilities?.actions?.Cihazlar ?? props.abilities?.Actions?.Cihazlar ?? {};
  const canCreate = !!(actions?.Create ?? actions?.create);
  const canUpdate = !!(actions?.Update ?? actions?.update);
  const canDelete = !!(actions?.Delete ?? actions?.delete);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<CihazListDTO[]>([]);
  const [tipler, setTipler] = useState<CihazTip[]>([]);
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
      const [listResp, tipResp] = await Promise.all([
        ayarlarService.listCihazlar({ sadeceAktif: false }),
        ayarlarService.cihazTipleri(),
      ]);
      if (!listResp?.success) throw new Error(listResp?.message || "Liste alınamadı.");
      if (!tipResp?.success) throw new Error(tipResp?.message || "Cihaz tipleri alınamadı.");
      setItems(listResp.data ?? []);
      setTipler(tipResp.data ?? []);
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
      const [listResp, tipResp] = await Promise.all([
        ayarlarService.listCihazlar({ sadeceAktif: false }, { forceRefresh: true }),
        ayarlarService.cihazTipleri({ forceRefresh: true }),
      ]);
      if (!listResp?.success) throw new Error(listResp?.message || "Liste alınamadı.");
      if (!tipResp?.success) throw new Error(tipResp?.message || "Cihaz tipleri alınamadı.");
      setItems(listResp.data ?? []);
      setTipler(tipResp.data ?? []);
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
    const mapped = (items ?? []).map((x) => {
      const id = pick<number>(x, "cihazId", "CihazId") ?? 0;
      const ad = (pick<string>(x, "cihazAdi", "CihazAdi") ?? "").toString();
      const ip = (pick<string>(x, "ipAdres", "IPAdres") ?? "").toString();
      const port = pick<number>(x, "port", "Port") ?? 0;
      const firma = (pick<string>(x, "firmaAdi", "FirmaAdi") ?? "").toString();
      const aktif = asBool(pick<any>(x, "aktifMi", "AktifMi"), true);
      return { id, ad, ip, port, firma, aktif, raw: x };
    });
    const filtered = !qq ? mapped : mapped.filter((r) => `${r.id} ${r.ad} ${r.ip} ${r.port} ${r.firma} ${r.aktif ? "aktif" : "pasif"}`.toLowerCase().includes(qq));
    return filtered.sort((a, b) => a.ad.localeCompare(b.ad, "tr"));
  }, [items, q]);

  // Form modal
  const [formVisible, setFormVisible] = useState(false);
  const [isEdit, setIsEdit] = useState(false);
  const [fId, setFId] = useState<number>(0);
  const [model, setModel] = useState<Cihaz>({});
  const [saving, setSaving] = useState(false);
  const [tipModal, setTipModal] = useState(false);

  const tipItems = useMemo(() => {
    return (tipler ?? []).map((t) => {
      const id = pick<number>(t, "cihazTipId", "CihazTipId") ?? 0;
      const ad = (pick<string>(t, "ad", "Ad") ?? "").toString();
      return { key: String(id), label: ad || String(id) };
    });
  }, [tipler]);

  const tipLabel = useMemo(() => {
    const tid = asInt((model as any)?.CihazTipId ?? (model as any)?.cihazTipId, 0);
    const found = (tipler ?? []).find((t) => (pick<number>(t, "cihazTipId", "CihazTipId") ?? 0) === tid);
    return (pick<string>(found, "ad", "Ad") ?? "").toString() || (tid ? String(tid) : "Seç");
  }, [model, tipler]);

  const openCreate = () => {
    setIsEdit(false);
    setFId(0);
    setModel({
      CihazAdi: "",
      IPAdres: "",
      Port: 4370,
      AktifMi: true,
    } as any);
    setFormVisible(true);
  };

  const openEdit = async (id: number) => {
    setIsEdit(true);
    setFId(id);
    setModel({});
    setFormVisible(true);
    try {
      const resp = await ayarlarService.getCihaz(id);
      if (resp?.success && resp.data) setModel(resp.data);
    } catch {}
  };

  const submit = async () => {
    if (saving) return;
    const cihazAdi = ((model as any)?.CihazAdi ?? (model as any)?.cihazAdi ?? "").toString().trim();
    const ip = ((model as any)?.IPAdres ?? (model as any)?.ipAdres ?? "").toString().trim();
    const port = asInt((model as any)?.Port ?? (model as any)?.port, 0);
    if (!cihazAdi) {
      showPopup("error", "Cihaz adı boş olamaz.");
      return;
    }
    if (!ip) {
      showPopup("error", "IP Adres boş olamaz.");
      return;
    }
    if (port <= 0) {
      showPopup("error", "Port geçersiz.");
      return;
    }
    setSaving(true);
    try {
      if (!isEdit) {
        const resp = await ayarlarService.createCihaz(model);
        if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
        showPopup("success", resp?.message || "Cihaz eklendi.");
      } else {
        const resp = await ayarlarService.updateCihaz(fId, model);
        if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
        showPopup("success", resp?.message || "Cihaz güncellendi.");
      }
      setFormVisible(false);
      await refresh();
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setSaving(false);
    }
  };

  // Activate / deactivate
  const [confirmVisible, setConfirmVisible] = useState(false);
  const [confirmMode, setConfirmMode] = useState<"pasif" | "aktif">("pasif");
  const [confirmId, setConfirmId] = useState<number | null>(null);
  const [confirmLabel, setConfirmLabel] = useState("");
  const [confirmSaving, setConfirmSaving] = useState(false);

  const ask = (mode: "pasif" | "aktif", row: any) => {
    setConfirmMode(mode);
    setConfirmId(row.id);
    setConfirmLabel(row.ad);
    setConfirmVisible(true);
  };

  const doConfirm = async () => {
    if (confirmSaving) return;
    if (!confirmId) return;
    setConfirmSaving(true);
    try {
      const resp = confirmMode === "pasif" ? await ayarlarService.pasifCihaz(confirmId) : await ayarlarService.aktifCihaz(confirmId);
      if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
      showPopup("success", resp?.message || (confirmMode === "pasif" ? "Cihaz pasif yapıldı." : "Cihaz aktif yapıldı."));
      setConfirmVisible(false);
      await refresh();
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setConfirmSaving(false);
    }
  };

  const topBar = (
    <>
      <PageHeader
        title="Cihazlar"
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
              placeholder="Ara (Ad / IP / Port / Firma / Durum)"
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
              <View key={`c_${r.id}`} className={`mb-3 rounded-2xl border overflow-hidden ${r.aktif ? "bg-white border-[#e2e8f0]" : "bg-[#f1f5f9] border-[#e2e8f0]"}`}>
                <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                  <View className="flex-1">
                    <Text className="text-[#0f172a] font-extrabold" numberOfLines={1}>
                      {r.ad}
                    </Text>
                    <Text className="text-[#64748b] font-semibold text-[12px]" numberOfLines={2}>
                      {r.ip}:{r.port} • {r.firma} • {r.aktif ? "Aktif" : "Pasif"}
                    </Text>
                  </View>
                  <View className="flex-row gap-2">
                    {canUpdate ? (
                      <TouchableOpacity onPress={() => openEdit(r.id)} className="px-3 py-2 rounded-xl bg-[#fbbf24]">
                        <MaterialCommunityIcons name="pencil" size={18} color="#0f172a" />
                      </TouchableOpacity>
                    ) : null}
                    {canDelete && r.aktif ? (
                      <TouchableOpacity onPress={() => ask("pasif", r)} className="px-3 py-2 rounded-xl bg-[#fee2e2]">
                        <MaterialCommunityIcons name="trash-can" size={18} color="#dc2626" />
                      </TouchableOpacity>
                    ) : null}
                    {canUpdate && !r.aktif ? (
                      <TouchableOpacity onPress={() => ask("aktif", r)} className="px-3 py-2 rounded-xl bg-[#dcfce7]">
                        <MaterialCommunityIcons name="check-circle" size={18} color="#16a34a" />
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
                  <Text className="text-[#0f172a] font-extrabold">{isEdit ? "Cihaz Düzenle" : "Yeni Cihaz"}</Text>
                  <TouchableOpacity onPress={() => setFormVisible(false)} className="px-3 py-2 rounded-xl bg-[#f1f5f9]">
                    <Text className="text-[#334155] font-extrabold">Kapat</Text>
                  </TouchableOpacity>
                </View>
                <ScrollView style={{ maxHeight: 620 }} contentContainerStyle={{ padding: 16 }}>
                  {isEdit ? (
                    <>
                      <Text className="text-[#64748b] font-semibold">Cihaz Id</Text>
                      <Text className="mt-2 px-4 py-3 rounded-xl bg-[#f1f5f9] border border-[#e2e8f0] text-[#0f172a] font-extrabold">{fId}</Text>
                    </>
                  ) : null}

                  <Text className={`${isEdit ? "mt-4" : ""} text-[#64748b] font-semibold`}>Cihaz Adı *</Text>
                  <TextInput
                    value={((model as any)?.CihazAdi ?? (model as any)?.cihazAdi ?? "").toString()}
                    onChangeText={(t) => setModel((m: any) => ({ ...m, CihazAdi: t }))}
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
                  />

                  <Text className="mt-4 text-[#64748b] font-semibold">IP Adres *</Text>
                  <TextInput
                    value={((model as any)?.IPAdres ?? (model as any)?.ipAdres ?? "").toString()}
                    onChangeText={(t) => setModel((m: any) => ({ ...m, IPAdres: t }))}
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
                    autoCapitalize="none"
                  />

                  <Text className="mt-4 text-[#64748b] font-semibold">Port *</Text>
                  <TextInput
                    value={String((model as any)?.Port ?? (model as any)?.port ?? 4370)}
                    onChangeText={(t) => setModel((m: any) => ({ ...m, Port: asInt(t, 0) }))}
                    keyboardType="number-pad"
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    placeholderTextColor="#94a3b8"
                  />

                  <Text className="mt-4 text-[#64748b] font-semibold">Cihaz Tipi</Text>
                  <TouchableOpacity onPress={() => setTipModal(true)} className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0]">
                    <Text className="text-[#0f172a] font-extrabold">{tipLabel}</Text>
                  </TouchableOpacity>
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

      <SelectModal
        visible={tipModal}
        title="Cihaz Tipi Seç"
        items={[{ key: "0", label: "Seç" }, ...tipItems]}
        onClose={() => setTipModal(false)}
        onPick={(k) => {
          const v = asInt(k, 0);
          setModel((m: any) => ({ ...m, CihazTipId: v || null }));
        }}
      />

      {confirmVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setConfirmVisible(false)}>
          <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setConfirmVisible(false)}>
            <TouchableOpacity activeOpacity={1} onPress={() => {}}>
              <View className="bg-white rounded-2xl overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9]">
                  <Text className="text-[#0f172a] font-extrabold">Onay</Text>
                </View>
                <View className="p-4">
                  <Text className="text-[#334155] font-semibold">
                    "{confirmLabel}" cihazını {confirmMode === "pasif" ? "pasif" : "aktif"} yapmak istiyor musunuz?
                  </Text>
                </View>
                <View className="p-4 border-t border-[#f1f5f9] flex-row gap-2">
                  <TouchableOpacity onPress={() => setConfirmVisible(false)} disabled={confirmSaving} className={`flex-1 py-3 rounded-xl items-center ${confirmSaving ? "bg-[#e2e8f0]" : "bg-[#f1f5f9]"}`}>
                    <Text className="text-[#334155] font-extrabold">İptal</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    onPress={doConfirm}
                    disabled={confirmSaving}
                    className={`flex-1 py-3 rounded-xl items-center ${
                      confirmSaving ? (confirmMode === "pasif" ? "bg-[#fca5a5]" : "bg-[#bbf7d0]") : confirmMode === "pasif" ? "bg-[#dc2626]" : "bg-[#16a34a]"
                    }`}
                  >
                    <Text className="text-white font-extrabold">{confirmSaving ? "İşleniyor..." : confirmMode === "pasif" ? "Pasif Yap" : "Aktif Yap"}</Text>
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

