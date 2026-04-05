import React, { useEffect, useMemo, useRef, useState } from "react";
import { ActivityIndicator, FlatList, Modal, Platform, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import DateTimePicker from "@react-native-community/datetimepicker";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { StatusPopup } from "../StatusPopup";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";
import { izinService, KisiIzinListRow, IzinUpsertRequest } from "../../services/izinApi";

function pick<T = any>(obj: any, a: string, b?: string): T | undefined {
  if (!obj) return undefined;
  if (obj[a] !== undefined) return obj[a] as T;
  if (b && obj[b] !== undefined) return obj[b] as T;
  return undefined;
}

function fmtDateTR(value: any) {
  if (!value) return "-";
  const s = String(value);
  // if yyyy-MM-dd
  const m = s.match(/^(\d{4})-(\d{2})-(\d{2})/);
  if (m) return `${m[3]}.${m[2]}.${m[1]}`;
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return s;
  return d.toLocaleDateString("tr-TR", { year: "numeric", month: "2-digit", day: "2-digit" });
}

function fmtDateTimeTR(value: any) {
  if (!value) return "-";
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return String(value);
  return d.toLocaleString("tr-TR", { year: "numeric", month: "2-digit", day: "2-digit", hour: "2-digit", minute: "2-digit" });
}

function fmtIsoDate(d: Date) {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

function normalizeDateOnly(d: Date) {
  const x = new Date(d);
  x.setHours(12, 0, 0, 0);
  return x;
}

type LookupKisi = { personelId: string; adSoyad: string };
type LookupIzinTip = { izinTipId: number; ad: string };
type LookupFirma = { firmaId: number; firmaAdi: string };

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

function RowLabel({ label, value }: { label: string; value: string }) {
  return (
    <View className="flex-row justify-between py-2">
      <Text className="text-[#64748b] font-semibold">{label}</Text>
      <Text className="text-[#0f172a] font-extrabold">{value}</Text>
    </View>
  );
}

export function IzinlerScreen(props: { user: any; abilities: any; onOpenMenu: () => void }) {
  const actions = props.abilities?.actions?.Izinler ?? props.abilities?.Actions?.Izinler ?? {};
  const canCreate = !!(actions?.Create ?? actions?.create);
  const canUpdate = !!(actions?.Update ?? actions?.update);
  const canDelete = !!(actions?.Delete ?? actions?.delete);
  const quickMenu = useHeaderQuickMenu();
  const notif = useNotificationsContext();
  const header = (
    <>
      <PageHeader
        title="İzinler"
        onOpenMenu={props.onOpenMenu}
        rightIcon={canCreate ? "plus" : undefined}
        onRightPress={canCreate ? openFormCreate : undefined}
        rightIcon2="bell-outline"
        onRightPress2={() => quickMenu.open("notif")}
        rightBadge2={notif.unreadCount}
        rightA11yLabel2="Bildirimler ve hesap"
      />
      {quickMenu.modal}
    </>
  );

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [firmalar, setFirmalar] = useState<LookupFirma[]>([]);
  const [aktifFirma, setAktifFirma] = useState<LookupFirma | null>(null);
  const [kisiler, setKisiler] = useState<LookupKisi[]>([]);
  const [izinTipleri, setIzinTipleri] = useState<LookupIzinTip[]>([]);

  const [selectedFirmaId, setSelectedFirmaId] = useState<number | null>(null);
  const [personelId, setPersonelId] = useState<string>("ALL");
  const [izinTipId, setIzinTipId] = useState<number>(0);

  const [baslangic, setBaslangic] = useState<Date>(() => normalizeDateOnly(new Date(new Date().getFullYear(), new Date().getMonth(), 1)));
  const [bitis, setBitis] = useState<Date>(() => normalizeDateOnly(new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0)));
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(50);

  const [items, setItems] = useState<KisiIzinListRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [firmaModal, setFirmaModal] = useState(false);
  const [personelModal, setPersonelModal] = useState(false);
  const [izinTipModal, setIzinTipModal] = useState(false);
  const [pageSizeModal, setPageSizeModal] = useState(false);

  const [datePickerOpen, setDatePickerOpen] = useState<null | "bas" | "bit">(null);
  const [pickerTemp, setPickerTemp] = useState<Date>(new Date());

  const [popupVisible, setPopupVisible] = useState(false);
  const [popupType, setPopupType] = useState<"success" | "error">("success");
  const [popupMessage, setPopupMessage] = useState("");
  const showPopup = (type: "success" | "error", message: string) => {
    setPopupType(type);
    setPopupMessage(message);
    setPopupVisible(true);
  };

  const [formVisible, setFormVisible] = useState(false);
  const [formMode, setFormMode] = useState<"create" | "edit">("create");
  const [formId, setFormId] = useState<number | null>(null);
  const [fPersonelId, setFPersonelId] = useState<string>("");
  const [fIzinId, setFIzinId] = useState<number>(0);
  const [fSaatlik, setFSaatlik] = useState(false);
  const [fBasTarih, setFBasTarih] = useState<Date>(() => normalizeDateOnly(new Date()));
  const [fBitTarih, setFBitTarih] = useState<Date>(() => normalizeDateOnly(new Date()));
  const [fBasSaat, setFBasSaat] = useState<string>("");
  const [fBitSaat, setFBitSaat] = useState<string>("");
  const [fAciklama, setFAciklama] = useState<string>("");
  const [formSaving, setFormSaving] = useState(false);
  const [formPickerOpen, setFormPickerOpen] = useState<null | "bas" | "bit">(null);
  const [pendingFormPicker, setPendingFormPicker] = useState<null | "bas" | "bit">(null);
  const [formFirmaModal, setFormFirmaModal] = useState(false);
  const [formPersonelModal, setFormPersonelModal] = useState(false);
  const [formIzinTipModal, setFormIzinTipModal] = useState(false);

  const [deleteConfirmVisible, setDeleteConfirmVisible] = useState(false);
  const [deleteTargetId, setDeleteTargetId] = useState<number | null>(null);
  const [deleteTargetLabel, setDeleteTargetLabel] = useState<string>("");
  const [deleteSaving, setDeleteSaving] = useState(false);

  const firstLoadRef = useRef(false);

  const loadLookups = async (firmaId?: number | null) => {
    const r = await izinService.lookups({ firmaId: firmaId ?? selectedFirmaId });
    if (!r?.success) throw new Error(r?.message ?? "Lookups alınamadı.");
    const data = r.data ?? (r as any).Data ?? {};
    const f = (data.Firmalar ?? data.firmalar ?? []).map((x: any) => ({
      firmaId: Number(pick<any>(x, "firmaId", "FirmaId")),
      firmaAdi: (pick<any>(x, "firmaAdi", "FirmaAdi") ?? "").toString(),
    })).filter((x: any) => x.firmaId && x.firmaAdi);
    const af = data.AktifFirma ?? data.aktifFirma ?? null;
    const afObj = af ? { firmaId: Number(pick<any>(af, "firmaId", "FirmaId")), firmaAdi: (pick<any>(af, "firmaAdi", "FirmaAdi") ?? "").toString() } : null;
    const k = (data.Kisiler ?? data.kisiler ?? []).map((x: any) => ({
      personelId: (pick<any>(x, "personelId", "PersonelId") ?? "").toString(),
      adSoyad: (pick<any>(x, "adSoyad", "AdSoyad") ?? "").toString(),
    })).filter((x: any) => x.personelId && x.adSoyad);
    const it = (data.IzinTipleri ?? data.izinTipleri ?? []).map((x: any) => ({
      izinTipId: Number(pick<any>(x, "izinTipId", "IzinTipId")),
      ad: (pick<any>(x, "ad", "Ad") ?? "").toString(),
    })).filter((x: any) => x.izinTipId && x.ad);

    setFirmalar(f);
    setAktifFirma(afObj);
    setKisiler(k);
    setIzinTipleri(it);
    if (!selectedFirmaId && afObj?.firmaId) setSelectedFirmaId(afObj.firmaId);
  };

  const loadList = async () => {
    const r = await izinService.list({
      personelId,
      izinTipId,
      baslangic: fmtIsoDate(baslangic),
      bitis: fmtIsoDate(bitis),
      page,
      pageSize,
    });
    if (!r?.success) throw new Error(r?.message ?? "Liste alınamadı.");
    const payload = r.data ?? (r as any).Data ?? {};
    const pr = payload;
    setItems(pr?.items ?? pr?.Items ?? []);
    setTotalCount(Number(pr?.totalCount ?? pr?.TotalCount ?? 0) || 0);
    setTotalPages(Number(pr?.totalPages ?? pr?.TotalPages ?? 1) || 1);
  };

  useEffect(() => {
    if (firstLoadRef.current) return;
    firstLoadRef.current = true;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        await loadLookups(null);
        await loadList();
      } catch (e: any) {
        setError(e?.message ?? "Beklenmeyen hata");
      } finally {
        setLoading(false);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (!firstLoadRef.current) return;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        // firma değişince kişi listesi de güncellensin (web ile aynı)
        await loadLookups(selectedFirmaId);
        await loadList();
      } catch (e: any) {
        setError(e?.message ?? "Beklenmeyen hata");
        setItems([]);
      } finally {
        setLoading(false);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedFirmaId, personelId, izinTipId, baslangic.getTime(), bitis.getTime(), page, pageSize]);

  const personelLabel = useMemo(() => {
    if (!personelId || personelId === "ALL") return "-- TÜMÜ --";
    const f = kisiler.find((x) => x.personelId === personelId);
    return f?.adSoyad ?? personelId;
  }, [personelId, kisiler]);

  const izinTipLabel = useMemo(() => {
    if (!izinTipId || izinTipId === 0) return "-- TÜMÜ --";
    const f = izinTipleri.find((x) => x.izinTipId === izinTipId);
    return f?.ad ?? String(izinTipId);
  }, [izinTipId, izinTipleri]);

  const firmaLabel = useMemo(() => {
    const id = selectedFirmaId ?? aktifFirma?.firmaId ?? null;
    if (!id) return "--";
    const f = firmalar.find((x) => x.firmaId === id);
    return f?.firmaAdi ?? aktifFirma?.firmaAdi ?? `#${id}`;
  }, [selectedFirmaId, firmalar, aktifFirma]);

  const fPersonelLabel = useMemo(() => {
    if (!fPersonelId) return "-- Seçiniz --";
    const f = kisiler.find((x) => x.personelId === fPersonelId);
    return f?.adSoyad ?? fPersonelId;
  }, [fPersonelId, kisiler]);

  const fIzinTipLabel = useMemo(() => {
    if (!fIzinId) return "-- Seçiniz --";
    const f = izinTipleri.find((x) => x.izinTipId === fIzinId);
    return f?.ad ?? String(fIzinId);
  }, [fIzinId, izinTipleri]);

  function openFormCreate() {
    setFormMode("create");
    setFormId(null);
    setFPersonelId("");
    setFIzinId(0);
    setFSaatlik(false);
    setFBasTarih(normalizeDateOnly(new Date()));
    setFBitTarih(normalizeDateOnly(new Date()));
    setFBasSaat("");
    setFBitSaat("");
    setFAciklama("");
    setFormVisible(true);
  }

  const openFormEdit = (row: KisiIzinListRow) => {
    const id = Number(pick<any>(row, "kisiIzinId", "KisiIzinId"));
    if (!Number.isFinite(id) || id <= 0) return;
    setFormMode("edit");
    setFormId(id);
    // web edit: firma+personel read-only
    setFPersonelId((pick<any>(row, "sicilNo", "SicilNo") ?? "").toString());
    // prefill izin tipi by label match (best effort)
    const izinAdi = (pick<any>(row, "izinTipi", "IzinTipi") ?? "").toString().trim().toLowerCase();
    const match = izinTipleri.find((x) => x.ad.trim().toLowerCase() === izinAdi);
    setFIzinId(match?.izinTipId ?? 0);
    const bas = pick<any>(row, "izinBaslangic", "IzinBaslangic");
    const bit = pick<any>(row, "izinBitis", "IzinBitis");
    setFBasTarih(normalizeDateOnly(new Date(bas ?? new Date())));
    setFBitTarih(normalizeDateOnly(new Date(bit ?? new Date())));
    const saatlik = (pick<any>(row, "saatlikIzin", "SaatlikIzin") ?? "").toString().toLowerCase() === "evet";
    setFSaatlik(saatlik);
    setFBasSaat("");
    setFBitSaat("");
    setFAciklama((pick<any>(row, "aciklama", "Aciklama") ?? "").toString());
    setFormVisible(true);
  };

  useEffect(() => {
    if (!pendingFormPicker) return;
    const t = setTimeout(() => {
      setFormPickerOpen(pendingFormPicker);
      setPendingFormPicker(null);
    }, 200);
    return () => clearTimeout(t);
  }, [pendingFormPicker]);

  const submitForm = async () => {
    if (formSaving) return;
    if (!selectedFirmaId) {
      showPopup("error", "Firma seçiniz.");
      return;
    }
    if (!fPersonelId) {
      showPopup("error", "Personel seçiniz.");
      return;
    }
    if (!fIzinId) {
      showPopup("error", "İzin tipi seçiniz.");
      return;
    }
    if (fSaatlik && (!fBasSaat || !fBitSaat)) {
      showPopup("error", "Saatlik izin için başlangıç ve bitiş saati giriniz.");
      return;
    }

    const payload: IzinUpsertRequest = {
      firmaId: selectedFirmaId,
      personelId: fPersonelId,
      izinId: fIzinId,
      saatlikIzinMi: fSaatlik,
      baslangicTarih: fmtIsoDate(fBasTarih),
      bitisTarih: fmtIsoDate(fBitTarih),
      baslangicSaat: fSaatlik ? fBasSaat : null,
      bitisSaat: fSaatlik ? fBitSaat : null,
      aciklama: fAciklama,
    };

    try {
      setFormSaving(true);
      const r =
        formMode === "create"
          ? await izinService.create(payload)
          : await izinService.update(formId ?? 0, payload);
      if (!r?.success) throw new Error(r?.message ?? "İşlem başarısız.");
      setFormVisible(false);
      showPopup("success", r?.message ?? "İşlem başarılı.");
      setTimeout(() => loadList().catch(() => {}), 250);
    } catch (e: any) {
      showPopup("error", e?.message ?? "Hata");
    } finally {
      setFormSaving(false);
    }
  };

  const askDelete = (row: KisiIzinListRow) => {
    const id = Number(pick<any>(row, "kisiIzinId", "KisiIzinId"));
    if (!Number.isFinite(id) || id <= 0) return;
    setDeleteTargetId(id);
    const label = `${(pick<any>(row, "adSoyad", "AdSoyad") ?? "-").toString()} • ${(pick<any>(row, "sicilNo", "SicilNo") ?? "-").toString()}`;
    setDeleteTargetLabel(label);
    setDeleteConfirmVisible(true);
  };

  const doDelete = async () => {
    if (!deleteTargetId || deleteSaving) return;
    try {
      setDeleteSaving(true);
      const r = await izinService.pasifYap(deleteTargetId);
      if (!r?.success) throw new Error(r?.message ?? "İşlem başarısız.");
      setDeleteConfirmVisible(false);
      showPopup("success", r?.message ?? "Silindi.");
      setTimeout(() => loadList().catch(() => {}), 250);
    } catch (e: any) {
      showPopup("error", e?.message ?? "Hata");
    } finally {
      setDeleteSaving(false);
    }
  };

  const personelItems = useMemo(
    () => [
      { key: "ALL", label: "-- TÜMÜ --" },
      ...kisiler.map((k) => ({ key: k.personelId, label: k.adSoyad })),
    ],
    [kisiler]
  );
  const izinTipItems = useMemo(
    () => [
      { key: "0", label: "-- TÜMÜ --" },
      ...izinTipleri.map((t) => ({ key: String(t.izinTipId), label: t.ad })),
    ],
    [izinTipleri]
  );
  const pageSizeItems = useMemo(() => [20, 50, 100, 200].map((n) => ({ key: String(n), label: String(n) })), []);
  const firmaItems = useMemo(() => firmalar.map((f) => ({ key: String(f.firmaId), label: f.firmaAdi })), [firmalar]);

  if (loading && !items.length && !error) {
    return (
      <View className="flex-1 bg-[#f8fafc]">
        {header}
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator size="large" color="#dc2626" />
          <Text className="mt-3 text-[#64748b] font-semibold">Yükleniyor...</Text>
        </View>
      </View>
    );
  }

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} useModal={false} autoCloseMs={1500} />
      {header}

      <ScrollView className="flex-1 px-4" contentContainerStyle={{ paddingBottom: 24 }}>
        <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
          <View className="px-4 py-3 border-b border-[#f1f5f9]">
            <Text className="text-[#0f172a] font-extrabold">Filtreler</Text>
          </View>
          <View className="p-4">
            <TouchableOpacity onPress={() => setFirmaModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
              <RowLabel label="Firma" value={firmaLabel} />
            </TouchableOpacity>
            <View className="h-3" />
            <TouchableOpacity onPress={() => setPersonelModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
              <RowLabel label="Personel" value={personelLabel} />
            </TouchableOpacity>
            <View className="h-3" />
            <TouchableOpacity onPress={() => setIzinTipModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
              <RowLabel label="İzin Tipi" value={izinTipLabel} />
            </TouchableOpacity>
            <View className="h-3" />
            <TouchableOpacity
              onPress={() => {
                setPickerTemp(baslangic);
                setDatePickerOpen("bas");
              }}
              className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
            >
              <RowLabel label="Başlangıç" value={fmtDateTR(baslangic)} />
            </TouchableOpacity>
            <View className="h-3" />
            <TouchableOpacity
              onPress={() => {
                setPickerTemp(bitis);
                setDatePickerOpen("bit");
              }}
              className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
            >
              <RowLabel label="Bitiş" value={fmtDateTR(bitis)} />
            </TouchableOpacity>
            <View className="h-3" />
            <TouchableOpacity onPress={() => setPageSizeModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
              <RowLabel label="Sayfa başına" value={String(pageSize)} />
            </TouchableOpacity>
            <View className="h-3" />
            <TouchableOpacity
              onPress={() => {
                setPage(1);
                setTimeout(() => loadList().catch(() => {}), 50);
              }}
              className="bg-[#dc2626] rounded-xl py-3 items-center"
            >
              <Text className="text-white font-extrabold">Ara</Text>
            </TouchableOpacity>
          </View>
        </View>

        <View className="h-4" />

        <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
          <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
            <View className="flex-1">
              <Text className="text-[#0f172a] font-extrabold">İzinler</Text>
              <Text className="text-[#64748b] font-semibold text-[12px]">
                Toplam {totalCount} kayıt • Sayfa {page} / {totalPages}
              </Text>
            </View>
            {loading ? <ActivityIndicator size="small" color="#dc2626" /> : null}
          </View>

          {error ? (
            <View className="p-4">
              <Text className="text-[#b91c1c] font-extrabold">Hata</Text>
              <Text className="text-[#334155] font-semibold mt-1">{error}</Text>
            </View>
          ) : null}

          {!error && !items.length ? (
            <View className="p-4">
              <Text className="text-[#64748b] font-semibold text-center">Kayıt bulunamadı.</Text>
            </View>
          ) : null}

          {!error &&
            items.map((row, idx) => {
              const sicil = (pick<any>(row, "sicilNo", "SicilNo") ?? "-").toString();
              const ad = (pick<any>(row, "adSoyad", "AdSoyad") ?? "-").toString();
              const tip = (pick<any>(row, "izinTipi", "IzinTipi") ?? "-").toString();
              const bas = pick<any>(row, "izinBaslangic", "IzinBaslangic");
              const bit = pick<any>(row, "izinBitis", "IzinBitis");
              const sureGun = (pick<any>(row, "sureGun", "SureGun") ?? "-").toString();
              const sureSaat = Number(pick<any>(row, "sureSaat", "SureSaat") ?? 0);
              const saatlik = (pick<any>(row, "saatlikIzin", "SaatlikIzin") ?? "-").toString();
              const aciklama = (pick<any>(row, "aciklama", "Aciklama") ?? "").toString();
              const islenme = pick<any>(row, "islenmeTarihi", "IslenmeTarihi");
              const isSaatlik = saatlik.trim().toLowerCase() === "evet";

              return (
                <View key={`${String(pick<any>(row, "kisiIzinId", "KisiIzinId") ?? idx)}_${idx}`} className="px-4 py-3 border-b border-[#f1f5f9]">
                  <Text className="text-[#0f172a] font-extrabold" numberOfLines={1}>
                    {ad} • {sicil}
                  </Text>
                  <Text className="text-[#64748b] font-semibold text-[12px]" numberOfLines={2}>
                    {tip} • {fmtDateTR(bas)} - {fmtDateTR(bit)} • Süre: {isSaatlik ? `${sureSaat.toFixed(2)} saat` : `${sureGun} gün / ${sureSaat.toFixed(2)} saat`}
                  </Text>
                  <Text className="text-[#334155] font-semibold text-[12px] mt-1" numberOfLines={3}>
                    Saatlik: {saatlik} • İşlenme: {fmtDateTimeTR(islenme)}{aciklama ? ` • Açıklama: ${aciklama}` : ""}
                  </Text>

                  {(canUpdate || canDelete) ? (
                    <View className="flex-row gap-2 mt-3">
                      {canUpdate ? (
                        <TouchableOpacity className="flex-1 bg-[#e0f2fe] rounded-xl py-2 items-center" onPress={() => openFormEdit(row)}>
                          <Text className="text-[#0284c7] font-extrabold">Güncelle</Text>
                        </TouchableOpacity>
                      ) : null}
                      {canDelete ? (
                        <TouchableOpacity className="flex-1 bg-[#fee2e2] rounded-xl py-2 items-center" onPress={() => askDelete(row)}>
                          <Text className="text-[#dc2626] font-extrabold">Pasif Yap</Text>
                        </TouchableOpacity>
                      ) : null}
                    </View>
                  ) : null}
                </View>
              );
            })}

          <View className="p-4 flex-row items-center justify-between">
            <TouchableOpacity
              disabled={page <= 1}
              onPress={() => setPage((p) => Math.max(1, p - 1))}
              className={`px-4 py-3 rounded-xl ${page <= 1 ? "bg-[#f1f5f9]" : "bg-white border border-[#e2e8f0]"}`}
            >
              <Text className={`font-extrabold ${page <= 1 ? "text-[#94a3b8]" : "text-[#0f172a]"}`}>Önceki</Text>
            </TouchableOpacity>
            <Text className="text-[#64748b] font-extrabold">
              {page} / {totalPages}
            </Text>
            <TouchableOpacity
              disabled={page >= totalPages}
              onPress={() => setPage((p) => Math.min(totalPages, p + 1))}
              className={`px-4 py-3 rounded-xl ${page >= totalPages ? "bg-[#f1f5f9]" : "bg-white border border-[#e2e8f0]"}`}
            >
              <Text className={`font-extrabold ${page >= totalPages ? "text-[#94a3b8]" : "text-[#0f172a]"}`}>Sonraki</Text>
            </TouchableOpacity>
          </View>
        </View>
      </ScrollView>

      {firmaModal ? (
        <SelectModal
          visible={firmaModal}
          title="Firma Seç"
          items={firmaItems}
          onClose={() => setFirmaModal(false)}
          onPick={(key) => {
            const id = Number(key);
            if (Number.isFinite(id) && id > 0) {
              setSelectedFirmaId(id);
              setPersonelId("ALL");
              setPage(1);
            }
          }}
        />
      ) : null}

      {personelModal ? (
        <SelectModal
          visible={personelModal}
          title="Personel Seç"
          items={personelItems}
          onClose={() => setPersonelModal(false)}
          onPick={(key) => {
            setPersonelId(key);
            setPage(1);
          }}
        />
      ) : null}

      {izinTipModal ? (
        <SelectModal
          visible={izinTipModal}
          title="İzin Tipi"
          items={izinTipItems}
          onClose={() => setIzinTipModal(false)}
          onPick={(key) => {
            const id = Number(key);
            setIzinTipId(Number.isFinite(id) ? id : 0);
            setPage(1);
          }}
        />
      ) : null}

      {pageSizeModal ? (
        <SelectModal
          visible={pageSizeModal}
          title="Sayfa Başına"
          items={pageSizeItems}
          onClose={() => setPageSizeModal(false)}
          onPick={(key) => {
            const n = Number(key);
            if ([20, 50, 100, 200].includes(n)) {
              setPageSize(n);
              setPage(1);
            }
          }}
        />
      ) : null}

      {datePickerOpen ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setDatePickerOpen(null)}>
          <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setDatePickerOpen(null)}>
            <TouchableOpacity activeOpacity={1} onPress={() => {}}>
              <View className="bg-white rounded-2xl overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9]">
                  <Text className="text-[#0f172a] font-extrabold text-[16px]">Tarih Seç</Text>
                </View>
                <View style={{ padding: 12, backgroundColor: "white" }}>
                  <View style={{ height: Platform.OS === "ios" ? 360 : 216, justifyContent: "center" }}>
                    <DateTimePicker
                      value={pickerTemp}
                      onChange={(_, selected) => {
                        if (selected) setPickerTemp(normalizeDateOnly(selected));
                      }}
                      mode="date"
                      display={Platform.OS === "ios" ? "inline" : "default"}
                      locale="tr-TR"
                      themeVariant="light"
                      textColor="#0f172a"
                    />
                  </View>
                </View>
                <View className="p-3 flex-row gap-2">
                  <TouchableOpacity className="flex-1 bg-[#f1f5f9] rounded-xl py-3 items-center" onPress={() => setDatePickerOpen(null)}>
                    <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    className="flex-1 bg-[#dc2626] rounded-xl py-3 items-center"
                    onPress={() => {
                      const v = pickerTemp;
                      const kind = datePickerOpen;
                      setDatePickerOpen(null);
                      if (kind === "bas") setBaslangic(v);
                      else setBitis(v);
                    }}
                  >
                    <Text className="text-white font-extrabold">Seç</Text>
                  </TouchableOpacity>
                </View>
              </View>
            </TouchableOpacity>
          </TouchableOpacity>
        </Modal>
      ) : null}

      {formVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setFormVisible(false)}>
          <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setFormVisible(false)}>
            <TouchableOpacity activeOpacity={1} onPress={() => {}}>
              <View className="bg-white rounded-2xl overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                  <Text className="text-[#0f172a] font-extrabold text-[16px]">{formMode === "create" ? "Yeni İzin Ekle" : "İzin Güncelle"}</Text>
                  <TouchableOpacity onPress={() => setFormVisible(false)}>
                    <MaterialCommunityIcons name="close" size={22} color="#64748b" />
                  </TouchableOpacity>
                </View>
                <ScrollView className="p-4" style={{ maxHeight: 520 }}>
                  <TouchableOpacity
                    disabled={formMode === "edit"}
                    onPress={() => setFormFirmaModal(true)}
                    className={`px-3 py-3 rounded-xl border border-[#e2e8f0] ${formMode === "edit" ? "bg-[#f1f5f9]" : "bg-white"}`}
                  >
                    <RowLabel label="Firma" value={firmaLabel} />
                  </TouchableOpacity>
                  <View className="h-3" />
                  <TouchableOpacity
                    disabled={formMode === "edit"}
                    onPress={() => setFormPersonelModal(true)}
                    className={`px-3 py-3 rounded-xl border border-[#e2e8f0] ${formMode === "edit" ? "bg-[#f1f5f9]" : "bg-white"}`}
                  >
                    <RowLabel label="Personel" value={fPersonelLabel} />
                  </TouchableOpacity>
                  <View className="h-3" />
                  <TouchableOpacity onPress={() => setFormIzinTipModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
                    <RowLabel label="İzin Tipi" value={fIzinTipLabel} />
                  </TouchableOpacity>
                  <View className="h-3" />
                  <TouchableOpacity
                    onPress={() => {
                      setFormVisible(false);
                      setPickerTemp(fBasTarih);
                      setPendingFormPicker("bas");
                    }}
                    className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
                  >
                    <RowLabel label="Başlangıç" value={fmtDateTR(fBasTarih)} />
                  </TouchableOpacity>
                  <View className="h-3" />
                  <TouchableOpacity
                    onPress={() => {
                      setFormVisible(false);
                      setPickerTemp(fBitTarih);
                      setPendingFormPicker("bit");
                    }}
                    className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
                  >
                    <RowLabel label="Bitiş" value={fmtDateTR(fBitTarih)} />
                  </TouchableOpacity>

                  <View className="h-3" />
                  <TouchableOpacity
                    onPress={() => setFSaatlik((v) => !v)}
                    className={`px-3 py-3 rounded-xl border ${fSaatlik ? "bg-[#e0f2fe] border-[#0284c7]" : "bg-white border-[#e2e8f0]"}`}
                  >
                    <RowLabel label="Saatlik İzin" value={fSaatlik ? "Evet" : "Hayır"} />
                  </TouchableOpacity>

                  {fSaatlik ? (
                    <>
                      <View className="h-3" />
                      <TextInput
                        value={fBasSaat}
                        onChangeText={setFBasSaat}
                        placeholder="Başlangıç Saati (HH:mm)"
                        className="px-3 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a]"
                      />
                      <View className="h-3" />
                      <TextInput
                        value={fBitSaat}
                        onChangeText={setFBitSaat}
                        placeholder="Bitiş Saati (HH:mm)"
                        className="px-3 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a]"
                      />
                    </>
                  ) : null}

                  <View className="h-3" />
                  <TextInput
                    value={fAciklama}
                    onChangeText={setFAciklama}
                    placeholder="Açıklama"
                    multiline
                    className="px-3 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a]"
                    style={{ minHeight: 90, textAlignVertical: "top" }}
                  />
                </ScrollView>
                <View className="p-4 flex-row gap-2">
                  <TouchableOpacity
                    disabled={formSaving}
                    onPress={submitForm}
                    className={`flex-1 rounded-xl py-3 items-center ${formSaving ? "bg-[#86efac]" : "bg-[#16a34a]"}`}
                  >
                    <Text className="text-white font-extrabold">{formSaving ? "Kaydediliyor..." : "Kaydet"}</Text>
                  </TouchableOpacity>
                  <TouchableOpacity disabled={formSaving} onPress={() => setFormVisible(false)} className="flex-1 bg-[#f1f5f9] rounded-xl py-3 items-center">
                    <Text className={`font-extrabold ${formSaving ? "text-[#94a3b8]" : "text-[#334155]"}`}>İptal</Text>
                  </TouchableOpacity>
                </View>
              </View>
            </TouchableOpacity>
          </TouchableOpacity>
        </Modal>
      ) : null}

      {formPickerOpen ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setFormPickerOpen(null)}>
          <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setFormPickerOpen(null)}>
            <TouchableOpacity activeOpacity={1} onPress={() => {}}>
              <View className="bg-white rounded-2xl overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9]">
                  <Text className="text-[#0f172a] font-extrabold text-[16px]">Tarih Seç</Text>
                </View>
                <View style={{ padding: 12, backgroundColor: "white" }}>
                  <View style={{ height: Platform.OS === "ios" ? 360 : 216, justifyContent: "center" }}>
                    <DateTimePicker
                      value={pickerTemp}
                      onChange={(_, selected) => {
                        if (selected) setPickerTemp(normalizeDateOnly(selected));
                      }}
                      mode="date"
                      display={Platform.OS === "ios" ? "inline" : "default"}
                      locale="tr-TR"
                      themeVariant="light"
                      textColor="#0f172a"
                    />
                  </View>
                </View>
                <View className="p-3 flex-row gap-2">
                  <TouchableOpacity className="flex-1 bg-[#f1f5f9] rounded-xl py-3 items-center" onPress={() => setFormPickerOpen(null)}>
                    <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    className="flex-1 bg-[#dc2626] rounded-xl py-3 items-center"
                    onPress={() => {
                      const v = pickerTemp;
                      const kind = formPickerOpen;
                      setFormPickerOpen(null);
                      if (kind === "bas") setFBasTarih(v);
                      else setFBitTarih(v);
                      setTimeout(() => setFormVisible(true), 150);
                    }}
                  >
                    <Text className="text-white font-extrabold">Seç</Text>
                  </TouchableOpacity>
                </View>
              </View>
            </TouchableOpacity>
          </TouchableOpacity>
        </Modal>
      ) : null}

      {deleteConfirmVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setDeleteConfirmVisible(false)}>
          <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setDeleteConfirmVisible(false)}>
            <TouchableOpacity activeOpacity={1} onPress={() => {}}>
              <View className="bg-white rounded-2xl overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                  <Text className="text-[#0f172a] font-extrabold text-[16px]">Onay</Text>
                  <TouchableOpacity onPress={() => setDeleteConfirmVisible(false)}>
                    <MaterialCommunityIcons name="close" size={22} color="#64748b" />
                  </TouchableOpacity>
                </View>
                <View className="p-4">
                  <Text className="text-[#0f172a] font-extrabold">Bu izin pasif yapılsın mı?</Text>
                  <Text className="text-[#64748b] font-semibold mt-1">{deleteTargetLabel}</Text>
                  <View className="h-4" />
                  <TouchableOpacity
                    disabled={deleteSaving}
                    onPress={doDelete}
                    className={`rounded-xl py-3 items-center ${deleteSaving ? "bg-[#fecaca]" : "bg-[#dc2626]"}`}
                  >
                    <Text className="text-white font-extrabold">{deleteSaving ? "İşleniyor..." : "Pasif Yap"}</Text>
                  </TouchableOpacity>
                  <TouchableOpacity disabled={deleteSaving} onPress={() => setDeleteConfirmVisible(false)} className="mt-2 bg-[#f1f5f9] rounded-xl py-3 items-center">
                    <Text className={`font-extrabold ${deleteSaving ? "text-[#94a3b8]" : "text-[#334155]"}`}>İptal</Text>
                  </TouchableOpacity>
                </View>
              </View>
            </TouchableOpacity>
          </TouchableOpacity>
        </Modal>
      ) : null}

      {formFirmaModal ? (
        <SelectModal
          visible={formFirmaModal}
          title="Firma Seç"
          items={firmaItems}
          onClose={() => setFormFirmaModal(false)}
          onPick={(key) => {
            const id = Number(key);
            if (Number.isFinite(id) && id > 0) {
              setSelectedFirmaId(id);
              setFPersonelId("");
            }
          }}
        />
      ) : null}

      {formPersonelModal ? (
        <SelectModal
          visible={formPersonelModal}
          title="Personel Seç"
          items={personelItems.filter((x) => x.key !== "ALL")}
          onClose={() => setFormPersonelModal(false)}
          onPick={(key) => setFPersonelId(key)}
        />
      ) : null}

      {formIzinTipModal ? (
        <SelectModal
          visible={formIzinTipModal}
          title="İzin Tipi Seç"
          items={izinTipItems.filter((x) => x.key !== "0")}
          onClose={() => setFormIzinTipModal(false)}
          onPick={(key) => {
            const id = Number(key);
            setFIzinId(Number.isFinite(id) ? id : 0);
          }}
        />
      ) : null}
    </View>
  );
}

