import React, { useEffect, useMemo, useRef, useState } from "react";
import { ActivityIndicator, FlatList, LayoutAnimation, Modal, Platform, ScrollView, Text, TextInput, TouchableOpacity, UIManager, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { StatusPopup } from "../StatusPopup";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";
import { puantajService, PuantajGunSatirDTO, PuantajLookupsDto, PuantajTipDTO } from "../../services/puantajApi";
import * as FileSystem from "expo-file-system/legacy";
import * as Sharing from "expo-sharing";
import { getApiBaseUrl, getAuthToken } from "../../services/api";

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

function fmtTime(value: any) {
  if (value == null || value === "") return "-";
  const s = String(value);
  // TimeSpan often comes like "07:30:00"
  const m = s.match(/^(\d{1,2}):(\d{2})(:\d{2})?$/);
  if (m) return `${m[1].padStart(2, "0")}:${m[2]}`;
  return s.replace(/:\d{2}(\.\d+)?$/, "");
}

function toYmd(value: any): string {
  if (!value) return "";
  const s = String(value);
  const m = s.match(/^(\d{4})-(\d{2})-(\d{2})/);
  if (m) return `${m[1]}-${m[2]}-${m[3]}`;
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return "";
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

function onayDurumuText(v: any): string {
  const raw = v?.OnayDurumu ?? v?.onayDurumu ?? v;
  if (raw === 0 || raw === "Bekliyor") return "Bekliyor";
  if (raw === 1 || raw === "Onaylandı" || raw === "Onaylandi") return "Onaylandı";
  if (raw === 2 || raw === "Reddedildi") return "Reddedildi";
  if (raw === 3 || raw === "Düzeltildi" || raw === "Duzeltildi") return "Düzeltildi";
  return String(raw ?? "");
}

function setRowField(row: any, field: string, value: any) {
  // Preserve server casing: update both camelCase and PascalCase if present.
  const pascal = field.charAt(0).toUpperCase() + field.slice(1);
  const camel = field.charAt(0).toLowerCase() + field.slice(1);
  const next = { ...(row ?? {}) };
  if (pascal in next) (next as any)[pascal] = value;
  if (camel in next) (next as any)[camel] = value;
  if (!(pascal in next) && !(camel in next)) (next as any)[pascal] = value;
  return next;
}

function isMultiLockNote(aciklama: string) {
  return !!aciklama && aciklama.trim().toLowerCase().startsWith("çoklu sicil aktarım");
}

function getRowTheme(row: any, ekKayitGun: number) {
  const t = pick(row, "Tarih", "tarih");
  const editable = isRowEditable(t, ekKayitGun) && !isLockedRow(row);
  const status = onayDurumuText(row);

  if (!editable) {
    return {
      cardBg: "bg-[#f1f5f9]",
      border: "border-[#cbd5e1]",
      badgeBg: "bg-[#e2e8f0]",
      badgeText: "text-[#334155]",
      status,
      editable,
    };
  }

  if (status === "Onaylandı") {
    return {
      cardBg: "bg-[#ecfdf5]",
      border: "border-[#86efac]",
      badgeBg: "bg-[#dcfce7]",
      badgeText: "text-[#166534]",
      status,
      editable,
    };
  }
  if (status === "Reddedildi") {
    return {
      cardBg: "bg-[#fef2f2]",
      border: "border-[#fca5a5]",
      badgeBg: "bg-[#fee2e2]",
      badgeText: "text-[#991b1b]",
      status,
      editable,
    };
  }
  if (status === "Düzeltildi") {
    return {
      cardBg: "bg-[#eff6ff]",
      border: "border-[#93c5fd]",
      badgeBg: "bg-[#dbeafe]",
      badgeText: "text-[#1e40af]",
      status,
      editable,
    };
  }
  // Bekliyor
  return {
    cardBg: "bg-[#fffbeb]",
    border: "border-[#fcd34d]",
    badgeBg: "bg-[#fde68a]",
    badgeText: "text-[#1f2937]",
    status,
    editable,
  };
}

function isRowEditable(tarihValue: any, ekKayitGun: number) {
  const tarih = new Date(tarihValue);
  if (Number.isNaN(tarih.getTime())) return false;

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const currMonthBeg = new Date(today.getFullYear(), today.getMonth(), 1);
  const prevMonthBeg = new Date(currMonthBeg.getFullYear(), currMonthBeg.getMonth() - 1, 1);
  const prevMonthEnd = new Date(currMonthBeg.getFullYear(), currMonthBeg.getMonth(), 0);

  const t = new Date(tarih.getFullYear(), tarih.getMonth(), tarih.getDate());

  if (ekKayitGun < 0) ekKayitGun = 0;
  if (t >= today) return false;
  if (t >= new Date(currMonthBeg.getFullYear(), currMonthBeg.getMonth() + 1, 1)) return false;
  if (t >= currMonthBeg && t < today) return true;

  const deadline = new Date(prevMonthEnd);
  deadline.setDate(deadline.getDate() + ekKayitGun);
  if (t >= prevMonthBeg && t <= prevMonthEnd) return today <= deadline;
  return false;
}

function isLockedRow(row: any) {
  const tarih = new Date(pick(row, "Tarih", "tarih") ?? row?.Tarih ?? row?.tarih);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  if (Number.isNaN(tarih.getTime())) return true;
  if (new Date(tarih.getFullYear(), tarih.getMonth(), tarih.getDate()) >= today) return true;
  const od = onayDurumuText(row);
  const ac = (pick(row, "Aciklama", "aciklama") ?? "").toString();
  if (od === "Düzeltildi" && isMultiLockNote(ac)) return true;
  return false;
}

function getApiErrorMessage(e: any): string {
  const data = e?.response?.data;
  if (typeof data === "string" && data.trim()) return data;
  if (data?.message) return String(data.message);
  if (data?.Message) return String(data.Message);
  if (data?.error) return String(data.error);
  const status = e?.response?.status;
  if (status) return `İstek başarısız. (HTTP ${status})`;
  if (e?.message) return String(e.message);
  return "Hata oluştu.";
}

function validateSaatInput(val: string) {
  const s = (val ?? "").trim();
  if (!s) return { ok: true as const };
  // Accept digits + optional decimal separator.
  if (!/^\d+([.,]\d+)?$/.test(s)) return { ok: false as const, message: "Çalışma saati geçersiz (örn: 7,50 veya 7.50)." };
  return { ok: true as const };
}

function SelectModal(props: {
  visible: boolean;
  title: string;
  searchPlaceholder?: string;
  items: { key: string; label: string }[];
  onClose: () => void;
  onPick: (key: string) => void;
}) {
  const [q, setQ] = useState("");
  const filtered = useMemo(() => {
    const s = q.trim().toLocaleLowerCase("tr-TR");
    if (!s) return props.items;
    return props.items.filter((x) => x.label.toLocaleLowerCase("tr-TR").includes(s));
  }, [props.items, q]);

  useEffect(() => {
    if (!props.visible) setQ("");
  }, [props.visible]);

  return (
    <Modal visible={props.visible} transparent animationType="fade" onRequestClose={props.onClose}>
      <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={props.onClose}>
        <TouchableOpacity activeOpacity={1} onPress={() => {}}>
          <View className="bg-white rounded-2xl overflow-hidden">
            <View className="px-4 py-3 border-b border-[#f1f5f9]">
              <Text className="text-[#0f172a] font-extrabold text-[16px]">{props.title}</Text>
              <TextInput
                value={q}
                onChangeText={setQ}
                placeholder={props.searchPlaceholder ?? "Ara..."}
                className="mt-3 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
              />
            </View>
            <ScrollView style={{ maxHeight: 520 }}>
              {filtered.map((it, idx) => (
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
              {!filtered.length ? (
                <View className="px-4 py-6 items-center">
                  <Text className="text-[#64748b] font-semibold">Kayıt yok.</Text>
                </View>
              ) : null}
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

export function PuantajScreen(props: { user: any; abilities: any; onOpenMenu: () => void }) {
  const [loading, setLoading] = useState(true); // initial load only
  const [error, setError] = useState<string | null>(null);
  const [lookup, setLookup] = useState<PuantajLookupsDto | null>(null);
  const didInitRef = useRef(false);
  const [filterBusy, setFilterBusy] = useState(false);

  const [firmaId, setFirmaId] = useState<number | null>(null);
  const [isyeriId, setIsyeriId] = useState<number | null>(null);
  const [personelId, setPersonelId] = useState<string>("");
  const [yil, setYil] = useState<number>(new Date().getFullYear());
  const [ay, setAy] = useState<number>(new Date().getMonth() + 1);

  const [rows, setRows] = useState<PuantajGunSatirDTO[]>([]);
  const [rowsLoading, setRowsLoading] = useState(false);
  const [filterCollapsed, setFilterCollapsed] = useState(false);

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

  const [advVisible, setAdvVisible] = useState(false);
  const [firmaModal, setFirmaModal] = useState(false);
  const [isyeriModal, setIsyeriModal] = useState(false);
  const [personelModal, setPersonelModal] = useState(false);
  const [calismaTipiModal, setCalismaTipiModal] = useState(false);
  const [ayModal, setAyModal] = useState(false);

  const [detailVisible, setDetailVisible] = useState(false);
  const [selectedRow, setSelectedRow] = useState<any>(null);

  // Actions modals
  const [editVisible, setEditVisible] = useState(false);
  const [rejectVisible, setRejectVisible] = useState(false);
  const [bulkVisible, setBulkVisible] = useState(false);
  const [cokluVisible, setCokluVisible] = useState(false);
  const [ekGunVisible, setEkGunVisible] = useState(false);
  const [editMode, setEditMode] = useState<"edit" | "approve">("edit");

  const [saving, setSaving] = useState(false);
  const [exporting, setExporting] = useState(false);

  const [fmdk, setFmdk] = useState("0");
  const [aciklama, setAciklama] = useState("");
  const [calismaTipi, setCalismaTipi] = useState("");
  const [saatText, setSaatText] = useState("");

  const [ekGunText, setEkGunText] = useState("");

  const canUpdate = !!lookup?.canUpdate;
  const canApprove = !!lookup?.canApprove;
  const canDelete = !!lookup?.canDelete;
  const canExport = !!lookup?.canExport;

  const tipItems = useMemo(() => {
    const list: { key: string; label: string }[] = [];
    (lookup?.puantajTipleri ?? []).forEach((t: PuantajTipDTO) => {
      const kod = (t.kod ?? (t as any).Kod ?? "").toString();
      const ad = (t.ad ?? (t as any).Ad ?? "").toString();
      if (!kod) return;
      list.push({ key: kod, label: ad ? `${kod} - ${ad}` : kod });
    });
    return [{ key: "", label: "(Boş)" }, ...list];
  }, [lookup?.puantajTipleri]);

  const firmaItems = useMemo(
    () => (lookup?.firmalar ?? []).map((f) => ({ key: String(f.firmaId), label: `${f.firmaAdi}` })),
    [lookup?.firmalar]
  );
  const isyeriItems = useMemo(
    () => [{ key: "", label: "(Tümü)" }, ...(lookup?.isyerleri ?? []).map((i) => ({ key: String(i.isyeriId), label: i.ad }))],
    [lookup?.isyerleri]
  );
  const personelItems = useMemo(
    () => (lookup?.personeller ?? []).map((p) => ({ key: String(p.personelId), label: `${p.personelId} - ${p.adSoyad}` })),
    [lookup?.personeller]
  );

  const selectedFirmaLabel = useMemo(() => {
    if (!firmaId) return "Seçiniz...";
    return (lookup?.firmalar ?? []).find((x) => x.firmaId === firmaId)?.firmaAdi ?? String(firmaId);
  }, [firmaId, lookup?.firmalar]);

  const selectedIsyeriLabel = useMemo(() => {
    if (!isyeriId) return "(Tümü)";
    return (lookup?.isyerleri ?? []).find((x) => x.isyeriId === isyeriId)?.ad ?? String(isyeriId);
  }, [isyeriId, lookup?.isyerleri]);

  const selectedPersonelLabel = useMemo(() => {
    if (!personelId) return "Seçiniz...";
    const hit = (lookup?.personeller ?? []).find((x) => String(x.personelId) === String(personelId));
    return hit ? `${hit.personelId} - ${hit.adSoyad}` : String(personelId);
  }, [personelId, lookup?.personeller]);

  const monthItems = useMemo(() => {
    const months = [
      "Ocak",
      "Şubat",
      "Mart",
      "Nisan",
      "Mayıs",
      "Haziran",
      "Temmuz",
      "Ağustos",
      "Eylül",
      "Ekim",
      "Kasım",
      "Aralık",
    ];
    return months.map((name, idx) => ({ key: String(idx + 1), label: `${idx + 1} - ${name}` }));
  }, []);

  const loadLookups = async (
    p?: { firmaId?: number | null; isyeriId?: number | null; yil?: number | null; ay?: number | null },
    opts?: { silent?: boolean }
  ) => {
    const silent = !!opts?.silent;
    if (!silent) setLoading(true);
    else setFilterBusy(true);
    setError(null);
    try {
      const resp = await puantajService.lookups({
        firmaId: p?.firmaId ?? firmaId,
        isyeriId: p?.isyeriId ?? isyeriId,
        yil: p?.yil ?? yil,
        ay: p?.ay ?? ay,
      });
      if (!resp?.success) throw new Error(resp?.message || "Lookups alınamadı.");
      const data = resp.data as any;
      setLookup(data ?? null);

      const fy = asInt(data?.selectedYil ?? data?.SelectedYil, yil);
      const fa = asInt(data?.selectedAy ?? data?.SelectedAy, ay);
      const ff = asInt(data?.selectedFirmaId ?? data?.SelectedFirmaId, firmaId ?? 0);
      const fi = data?.selectedIsyeriId ?? data?.SelectedIsyeriId ?? null;

      setYil(fy);
      setAy(fa);
      if (ff) setFirmaId(ff);
      setIsyeriId(fi != null ? asInt(fi, 0) || null : null);

      // If current personel is invalid for selected filters, clear.
      const personelSet = new Set((data?.personeller ?? []).map((x: any) => String(x.personelId ?? x.PersonelId)));
      if (personelId && !personelSet.has(String(personelId))) {
        setPersonelId("");
        setRows([]);
      }
    } catch (e: any) {
      setError(getApiErrorMessage(e));
    } finally {
      if (!silent) setLoading(false);
      setFilterBusy(false);
    }
  };

  const loadRows = async () => {
    if (!personelId) {
      setRows([]);
      return;
    }
    setRowsLoading(true);
    try {
      const resp = await puantajService.getAy(personelId, yil, ay);
      if (!resp?.success) throw new Error(resp?.message || "Puantaj alınamadı.");
      setRows(resp.data ?? []);
      // UX: auto-collapse filters to focus on list
      if ((resp.data ?? []).length > 0) {
        try {
          LayoutAnimation.configureNext(LayoutAnimation.Presets.easeInEaseOut);
        } catch { }
        setFilterCollapsed(true);
      }
    } catch (e: any) {
      showPopup("error", getApiErrorMessage(e));
    } finally {
      setRowsLoading(false);
    }
  };

  const patchRowByTarih = (tarihValue: any, patch: (row: any) => any) => {
    const key = toYmd(tarihValue);
    if (!key) return;
    setRows((prev) =>
      (prev ?? []).map((r: any) => {
        const rk = toYmd(pick(r, "Tarih", "tarih"));
        if (rk !== key) return r;
        return patch(r);
      })
    );
    setSelectedRow((prev: any) => {
      if (!prev) return prev;
      const pk = toYmd(pick(prev, "Tarih", "tarih"));
      if (pk !== key) return prev;
      return patch(prev);
    });
  };

  useEffect(() => {
    didInitRef.current = true;
    loadLookups(undefined, { silent: false });
    // Enable LayoutAnimation on Android
    if (Platform.OS === "android") {
      try {
        // @ts-ignore
        UIManager.setLayoutAnimationEnabledExperimental?.(true);
      } catch { }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (!personelId) return;
    loadRows();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [personelId, yil, ay]);

  const openRow = (r: any) => {
    setSelectedRow(r);
    setDetailVisible(true);
  };

  const resetEditFieldsFromRow = (r: any) => {
    const dfm = asInt(pick(r, "DuzenlenenFMDakika", "duzenlenenFmDakika") ?? 0, 0);
    setFmdk(String(dfm));
    setAciklama((pick(r, "Aciklama", "aciklama") ?? "").toString());
    setCalismaTipi((pick(r, "CalismaTipi", "calismaTipi") ?? "").toString());
    const s = pick(r, "Saat", "saat");
    setSaatText(s != null && s !== "" ? String(s).replace(".", ",") : "");
  };

  const doApprove = async () => {
    if (!selectedRow || saving) return;
    const pId = asInt(personelId, 0);
    if (!pId) return;
    const tarih = (pick(selectedRow, "Tarih", "tarih") ?? "").toString();
    const dfm = asInt(fmdk, 0);
    const v = validateSaatInput(saatText);
    if (!v.ok) {
      showPopup("error", v.message);
      return;
    }
    setSaving(true);
    try {
      const resp = await puantajService.onayla({
        personelId: pId,
        tarih,
        duzenlenmisFm: dfm,
        aciklama,
        calismaTipi,
        saatText,
      });
      if (!resp?.success) throw new Error(resp?.message || "Onaylanamadı.");
      showPopup("success", resp?.message || "Puantaj onaylandı.");
      patchRowByTarih(tarih, (row) => {
        let next = row;
        next = setRowField(next, "OnayDurumu", "Onaylandı");
        next = setRowField(next, "DuzenlenenFMDakika", dfm);
        next = setRowField(next, "Aciklama", (aciklama ?? "").toString());
        next = setRowField(next, "CalismaTipi", (calismaTipi ?? "").toString());
        if (saatText?.trim()) {
          // Keep display-friendly decimal; server will normalize anyway.
          const normalized = Number(String(saatText).replace(",", "."));
          if (Number.isFinite(normalized)) next = setRowField(next, "Saat", normalized);
        }
        return next;
      });
      setEditVisible(false);
      setDetailVisible(false);
    } catch (e: any) {
      showPopup("error", getApiErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const doEdit = async () => {
    if (!selectedRow || saving) return;
    const pId = asInt(personelId, 0);
    if (!pId) return;
    const tarih = (pick(selectedRow, "Tarih", "tarih") ?? "").toString();
    const dfm = asInt(fmdk, 0);
    const v = validateSaatInput(saatText);
    if (!v.ok) {
      showPopup("error", v.message);
      return;
    }
    setSaving(true);
    try {
      const resp = await puantajService.duzenle({
        personelId: pId,
        tarih,
        duzenlenmisFm: dfm,
        aciklama,
        calismaTipi,
        saatText,
      });
      if (!resp?.success) throw new Error(resp?.message || "Güncellenemedi.");
      showPopup("success", resp?.message || "Puantaj düzenlendi.");
      const d = resp?.data ?? (resp as any)?.Data ?? null;
      patchRowByTarih(tarih, (row) => {
        let next = row;
        next = setRowField(next, "OnayDurumu", "Düzeltildi");
        if (d?.duzenlenenFmDakika ?? d?.DuzenlenenFmDakika) next = setRowField(next, "DuzenlenenFMDakika", asInt(d?.duzenlenenFmDakika ?? d?.DuzenlenenFmDakika, dfm));
        else next = setRowField(next, "DuzenlenenFMDakika", dfm);
        if (d?.calismaTipi ?? d?.CalismaTipi) next = setRowField(next, "CalismaTipi", (d?.calismaTipi ?? d?.CalismaTipi ?? "").toString());
        else next = setRowField(next, "CalismaTipi", (calismaTipi ?? "").toString());
        if (d?.saat ?? d?.Saat) next = setRowField(next, "Saat", Number(d?.saat ?? d?.Saat));
        else if (saatText?.trim()) {
          const normalized = Number(String(saatText).replace(",", "."));
          if (Number.isFinite(normalized)) next = setRowField(next, "Saat", normalized);
        }
        if (d?.aciklama ?? d?.Aciklama) next = setRowField(next, "Aciklama", (d?.aciklama ?? d?.Aciklama ?? "").toString());
        else next = setRowField(next, "Aciklama", (aciklama ?? "").toString());
        return next;
      });
      setEditVisible(false);
      setDetailVisible(false);
    } catch (e: any) {
      showPopup("error", getApiErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const doReject = async () => {
    if (!selectedRow || saving) return;
    const pId = asInt(personelId, 0);
    if (!pId) return;
    const tarih = (pick(selectedRow, "Tarih", "tarih") ?? "").toString();
    if (!String(aciklama ?? "").trim()) {
      showPopup("error", "Red sebebi giriniz.");
      return;
    }
    setSaving(true);
    try {
      const resp = await puantajService.reddet({ personelId: pId, tarih, aciklama });
      if (!resp?.success) throw new Error(resp?.message || "Reddedilemedi.");
      showPopup("success", resp?.message || "Puantaj reddedildi.");
      patchRowByTarih(tarih, (row) => {
        let next = row;
        next = setRowField(next, "OnayDurumu", "Reddedildi");
        next = setRowField(next, "Aciklama", (aciklama ?? "").toString());
        return next;
      });
      setRejectVisible(false);
      setDetailVisible(false);
    } catch (e: any) {
      showPopup("error", getApiErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const doBulkApprove = async () => {
    if (saving) return;
    const pId = asInt(personelId, 0);
    if (!pId) return;
    setSaving(true);
    try {
      const resp = await puantajService.topluOnayla({ personelId: pId, yil, ay });
      if (!resp?.success) throw new Error(resp?.message || "Toplu onay başarısız.");
      showPopup("success", resp?.message || "Toplu onay tamamlandı.");
      setBulkVisible(false);
      await loadRows();
    } catch (e: any) {
      showPopup("error", getApiErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const doCokluSicil = async () => {
    if (saving) return;
    const pId = asInt(personelId, 0);
    if (!pId) return;
    setSaving(true);
    try {
      const resp = await puantajService.cokluSicileAktar({ personelId: pId, yil, ay });
      if (!resp?.success) throw new Error(resp?.message || "Aktarım başarısız.");
      showPopup("success", resp?.message || "Aktarım tamamlandı.");
      setCokluVisible(false);
      await loadRows();
    } catch (e: any) {
      showPopup("error", getApiErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const doSetEkGun = async () => {
    if (saving) return;
    const gun = asInt(ekGunText, NaN as any);
    if (!Number.isFinite(gun)) {
      showPopup("error", "Ek kayıt günü sayı olmalı.");
      return;
    }
    setSaving(true);
    try {
      const resp = await puantajService.setEkKayitGun(gun);
      if (!resp?.success) throw new Error(resp?.message || "Kaydedilemedi.");
      showPopup("success", resp?.message || "Kaydedildi.");
      setEkGunVisible(false);
      await loadLookups({ firmaId, isyeriId, yil, ay });
    } catch (e: any) {
      showPopup("error", getApiErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const doExportExcel = async () => {
    if (!canExport || exporting) return;
    setExporting(true);
    try {
      const token = await getAuthToken();
      if (!token) throw new Error("Oturum bulunamadı.");

      const base = getApiBaseUrl();
      const fileName = `${yil}_${String(ay).padStart(2, "0")}_Puantaj.xlsx`;
      const url = `${base}/Puantaj/export-excel?yil=${encodeURIComponent(String(yil))}&ay=${encodeURIComponent(String(ay))}`;
      // Expo SDK 54+: avoid deprecated makeDirectoryAsync usage
      const baseDir = FileSystem.cacheDirectory ?? FileSystem.documentDirectory;
      if (!baseDir) throw new Error("Dosya sistemi yolu alınamadı.");
      const localUri = `${baseDir}${fileName}`;

      const res = await FileSystem.downloadAsync(url, localUri, {
        headers: { Authorization: `Bearer ${token}` },
      });

      if (res.status && res.status >= 400) {
        // downloadAsync doesn't throw for non-2xx; fetch the real API error via axios for user-friendly message.
        try {
          await puantajService.exportExcel({ yil, ay });
        } catch (e: any) {
          throw new Error(getApiErrorMessage(e) + ` (HTTP ${res.status})`);
        }
        throw new Error(`Excel indirilemedi. (HTTP ${res.status})`);
      }

      if (Sharing.isAvailableAsync) {
        const ok = await Sharing.isAvailableAsync();
        if (ok) {
          await Sharing.shareAsync(res.uri, {
            mimeType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            dialogTitle: "Puantaj Excel",
            UTI: "com.microsoft.excel.xlsx",
          } as any);
          showPopup("success", "Excel hazır.");
          return;
        }
      }

      showPopup("success", `Excel indirildi: ${res.uri}`);
    } catch (e: any) {
      showPopup("error", getApiErrorMessage(e));
    } finally {
      setExporting(false);
    }
  };

  const topBar = (
    <PageHeader
      title="Puantaj"
      onOpenMenu={props.onOpenMenu}
      rightIcon="refresh"
      onRightPress={() => (personelId ? loadRows() : loadLookups())}
      rightIcon2="bell-outline"
      onRightPress2={() => quickMenu.open("notif")}
      rightBadge2={notif.unreadCount}
      rightA11yLabel2="Bildirimler ve hesap"
    />
  );

  if (loading) {
    return (
      <View className="flex-1 bg-[#f8fafc]">
        <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} />
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
          <TouchableOpacity className="mt-4 px-4 py-3 rounded-xl bg-[#0f172a]" onPress={() => loadLookups()}>
            <Text className="text-white font-extrabold">Tekrar Dene</Text>
          </TouchableOpacity>
        </View>
      </View>
    );
  }

  const ekGun = lookup?.ekKayitGun ?? 0;

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} />
      {topBar}

      <View className="px-4 pt-4">
        <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
          <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
            <Text className="text-[#0f172a] font-extrabold">Filtre</Text>
            <View className="flex-row items-center gap-2">
              {(canUpdate && personelId) || canUpdate ? (
                <TouchableOpacity
                  disabled={filterBusy}
                  onPress={() => setAdvVisible(true)}
                  className={`px-3 py-2 rounded-xl ${filterBusy ? "bg-[#e2e8f0]" : "bg-[#f1f5f9]"}`}
                >
                  <Text className="text-[#334155] font-extrabold">Gelişmiş</Text>
                </TouchableOpacity>
              ) : null}
              <TouchableOpacity
                onPress={() => {
                  try {
                    LayoutAnimation.configureNext(LayoutAnimation.Presets.easeInEaseOut);
                  } catch { }
                  setFilterCollapsed((v) => !v);
                }}
                className="px-3 py-2 rounded-xl bg-[#0f172a]"
              >
                <MaterialCommunityIcons name={filterCollapsed ? "chevron-down" : "chevron-up"} size={20} color="#fff" />
              </TouchableOpacity>
            </View>
          </View>
          {filterCollapsed ? (
            <View className="p-4">
              <Text className="text-[#334155] font-semibold" numberOfLines={2}>
                {yil}/{String(ay).padStart(2, "0")} • {selectedFirmaLabel} • {selectedIsyeriLabel} • {selectedPersonelLabel}
              </Text>
              <Text className="mt-1 text-[#64748b] font-semibold text-[12px]">Filtreyi açmak için yukarıdaki oka dokunun.</Text>
            </View>
          ) : (
          <View className="p-4">
            <View className="flex-row gap-3">
              <View className="flex-1">
                <Text className="text-[#64748b] font-semibold text-[12px]">Yıl</Text>
                <TextInput
                  value={String(yil)}
                  onChangeText={(t) => {
                    const n = asInt(t, yil);
                    if (n >= 2000 && n <= 2100) setYil(n);
                  }}
                  keyboardType="numeric"
                  className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-extrabold"
                />
              </View>
              <View className="flex-1">
                <Text className="text-[#64748b] font-semibold text-[12px]">Ay</Text>
                <TouchableOpacity disabled={filterBusy} onPress={() => setAyModal(true)} className={`mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] ${filterBusy ? "opacity-60" : ""}`}>
                  <Text className="text-[#0f172a] font-extrabold" numberOfLines={1}>
                    {monthItems.find((x) => asInt(x.key, 0) === ay)?.label ?? `${ay}`}
                  </Text>
                </TouchableOpacity>
              </View>
            </View>

            <View className="mt-3">
              <Text className="text-[#64748b] font-semibold text-[12px]">Firma</Text>
              <TouchableOpacity disabled={filterBusy} onPress={() => setFirmaModal(true)} className={`mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] flex-row items-center justify-between ${filterBusy ? "opacity-60" : ""}`}>
                <Text className="text-[#0f172a] font-extrabold flex-1 pr-2" numberOfLines={1}>
                  {selectedFirmaLabel}
                </Text>
                <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" />
              </TouchableOpacity>
            </View>

            <View className="mt-3 flex-row gap-3">
              <View className="flex-1">
                <Text className="text-[#64748b] font-semibold text-[12px]">İşyeri</Text>
                <TouchableOpacity disabled={filterBusy} onPress={() => setIsyeriModal(true)} className={`mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] flex-row items-center justify-between ${filterBusy ? "opacity-60" : ""}`}>
                  <Text className="text-[#0f172a] font-extrabold flex-1 pr-2" numberOfLines={1}>
                    {selectedIsyeriLabel}
                  </Text>
                  <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" />
                </TouchableOpacity>
              </View>
              <View className="flex-1">
                <Text className="text-[#64748b] font-semibold text-[12px]">Personel</Text>
                <TouchableOpacity disabled={filterBusy} onPress={() => setPersonelModal(true)} className={`mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] flex-row items-center justify-between ${filterBusy ? "opacity-60" : ""}`}>
                  <Text className="text-[#0f172a] font-extrabold flex-1 pr-2" numberOfLines={1}>
                    {selectedPersonelLabel}
                  </Text>
                  <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" />
                </TouchableOpacity>
              </View>
            </View>

            <View className="mt-4 flex-row gap-2">
              <TouchableOpacity
                onPress={async () => {
                  await loadLookups({ firmaId, isyeriId, yil, ay }, { silent: true });
                  if (!personelId) {
                    showPopup("error", "Lütfen personel seçiniz.");
                    return;
                  }
                  await loadRows();
                }}
                disabled={filterBusy || rowsLoading}
                className={`flex-1 px-2 py-3 rounded-2xl items-center flex-row justify-center gap-2 ${filterBusy || rowsLoading ? "bg-[#93c5fd]" : "bg-[#2563eb]"}`}
              >
                <MaterialCommunityIcons name="magnify" size={20} color="#fff" />
                <Text className="text-white font-extrabold text-[13px]" numberOfLines={1}>
                  {filterBusy || rowsLoading ? "Yükleniyor..." : "Puantaj Getir"}
                </Text>
              </TouchableOpacity>

              <TouchableOpacity
                onPress={() => {
                  if (!canApprove) return;
                  if (!personelId) {
                    showPopup("error", "Lütfen personel seçiniz.");
                    return;
                  }
                  setBulkVisible(true);
                }}
                disabled={!canApprove || filterBusy}
                className={`px-3 py-3 rounded-2xl items-center justify-center ${!canApprove ? "bg-[#e2e8f0]" : filterBusy ? "bg-[#c4b5fd]" : "bg-[#6f42c1]"}`}
              >
                <View className="flex-row items-center gap-2">
                  <MaterialCommunityIcons name="check-all" size={18} color={!canApprove ? "#64748b" : "#fff"} />
                  <Text className={`${!canApprove ? "text-[#64748b]" : "text-white"} font-extrabold text-[13px]`} numberOfLines={1}>
                    Toplu Onayla
                  </Text>
                </View>
              </TouchableOpacity>

              <TouchableOpacity
                onPress={doExportExcel}
                disabled={!canExport || exporting || filterBusy}
                className={`px-3 py-3 rounded-2xl items-center justify-center ${
                  !canExport ? "bg-[#e2e8f0]" : exporting ? "bg-[#86efac]" : "bg-[#16a34a]"
                }`}
              >
                <Text className={`${!canExport ? "text-[#64748b]" : "text-white"} font-extrabold text-[13px]`} numberOfLines={1}>
                  {exporting ? "Excel..." : "Excel"}
                </Text>
              </TouchableOpacity>
            </View>

            <View className="mt-4 bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl p-3">
              <Text className="text-[#0f172a] font-extrabold text-[12px]">Durum Renkleri</Text>
              <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={{ gap: 10, paddingTop: 8 }}>
                <View className="flex-row items-center gap-2">
                  <View className="w-2.5 h-2.5 rounded-full bg-[#dcfce7] border border-[#86efac]" />
                  <Text className="text-[#334155] font-semibold text-[12px]" numberOfLines={1}>Onaylandı</Text>
                </View>
                <View className="flex-row items-center gap-2">
                  <View className="w-2.5 h-2.5 rounded-full bg-[#fee2e2] border border-[#fca5a5]" />
                  <Text className="text-[#334155] font-semibold text-[12px]" numberOfLines={1}>Reddedildi</Text>
                </View>
                <View className="flex-row items-center gap-2">
                  <View className="w-2.5 h-2.5 rounded-full bg-[#dbeafe] border border-[#93c5fd]" />
                  <Text className="text-[#334155] font-semibold text-[12px]" numberOfLines={1}>Düzeltildi</Text>
                </View>
                <View className="flex-row items-center gap-2">
                  <View className="w-2.5 h-2.5 rounded-full bg-[#fde68a] border border-[#fcd34d]" />
                  <Text className="text-[#334155] font-semibold text-[12px]" numberOfLines={1}>Bekliyor</Text>
                </View>
                <View className="flex-row items-center gap-2">
                  <View className="w-2.5 h-2.5 rounded-full bg-[#e2e8f0] border border-[#cbd5e1]" />
                  <Text className="text-[#334155] font-semibold text-[12px]" numberOfLines={1}>Kilitli</Text>
                </View>
              </ScrollView>
            </View>
          </View>
          )}
        </View>
      </View>

      <View className="flex-1 px-4 pt-4">
        <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden flex-1">
          <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
            <Text className="text-[#0f172a] font-extrabold">Günler</Text>
            <Text className="text-[#64748b] font-semibold text-[12px]">{rows.length ? `${rows.length} kayıt` : ""}</Text>
          </View>
          {rowsLoading ? (
            <View className="flex-1 items-center justify-center">
              <ActivityIndicator />
              <Text className="mt-2 text-[#64748b] font-semibold">Yükleniyor...</Text>
            </View>
          ) : !personelId ? (
            <View className="flex-1 items-center justify-center px-6">
              <Text className="text-[#64748b] font-semibold text-center">Puantajı görmek için personel seçiniz.</Text>
            </View>
          ) : !rows.length ? (
            <View className="flex-1 items-center justify-center px-6">
              <Text className="text-[#64748b] font-semibold text-center">Kayıt bulunamadı.</Text>
            </View>
          ) : (
            <FlatList
              data={rows}
              keyExtractor={(item, idx) => `${pick(item, "Tarih", "tarih") ?? idx}_${idx}`}
              contentContainerStyle={{ padding: 16, paddingBottom: 24 }}
              renderItem={({ item }) => {
                const tarih = pick(item, "Tarih", "tarih");
                const theme = getRowTheme(item, ekGun);

                const v = (pick(item, "VardiyaTuru", "vardiyaTuru") ?? "-").toString();
                const ilk = fmtTime(pick(item, "IlkGiris", "ilkGiris"));
                const son = fmtTime(pick(item, "SonCikis", "sonCikis"));
                const sFm = asInt(pick(item, "SistemFMDakika", "sistemFmDakika") ?? 0, 0);
                const dFm = asInt(pick(item, "DuzenlenenFMDakika", "duzenlenenFmDakika") ?? 0, 0);

                return (
                  <TouchableOpacity onPress={() => openRow(item)} className={`mb-3 p-4 rounded-2xl border ${theme.border} ${theme.cardBg}`}>
                    <View className="flex-row items-start justify-between">
                      <View className="flex-1 pr-2">
                        <Text className="text-[#0f172a] font-extrabold">{fmtDateTR(tarih)}</Text>
                        <Text className="text-[#64748b] font-semibold text-[12px]" numberOfLines={1}>
                          Vardiya: {v} • {ilk} - {son}
                        </Text>
                      </View>
                      <View className={`px-3 py-1 rounded-full ${theme.badgeBg}`}>
                        <Text className={`font-extrabold text-[12px] ${theme.badgeText}`}>{theme.status}</Text>
                      </View>
                    </View>

                    <View className="mt-3 flex-row justify-between">
                      <Text className="text-[#334155] font-semibold">Sistem FM: {sFm} dk</Text>
                      <Text className="text-[#0f172a] font-extrabold">Düz. FM: {dFm} dk</Text>
                    </View>

                    {!theme.editable ? (
                      <Text className="mt-2 text-[#64748b] font-semibold text-[12px]">Kilitli</Text>
                    ) : null}
                  </TouchableOpacity>
                );
              }}
            />
          )}
        </View>
      </View>

      <SelectModal
        visible={ayModal}
        title="Ay Seç"
        items={monthItems}
        onClose={() => setAyModal(false)}
        onPick={(k) => setAy(asInt(k, ay))}
      />

      {/* Personel Picker */}
      <SelectModal
        visible={personelModal}
        title="Personel Seç"
        searchPlaceholder="Sicil / Ad Soyad ara..."
        items={personelItems}
        onClose={() => setPersonelModal(false)}
        onPick={async (k) => {
          setPersonelId(k);
          setRows([]);
          await loadRows();
        }}
      />

      {/* Advanced Settings (actions) */}
      <Modal visible={advVisible} transparent animationType="fade" onRequestClose={() => setAdvVisible(false)}>
        <View className="flex-1 bg-black/50 justify-end">
          <View className="bg-white rounded-t-3xl overflow-hidden border-t border-[#e2e8f0]">
            <View className="px-4 py-4 border-b border-[#f1f5f9] flex-row items-center justify-between">
              <Text className="text-[#0f172a] font-extrabold text-[16px]">Gelişmiş Ayarlar</Text>
              <TouchableOpacity onPress={() => setAdvVisible(false)} className="px-3 py-2 bg-[#f1f5f9] rounded-xl">
                <Text className="text-[#334155] font-extrabold">Kapat</Text>
              </TouchableOpacity>
            </View>
            <ScrollView style={{ maxHeight: 520 }}>
              <View className="p-4">
                <View className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl p-4">
                  <Text className="text-[#0f172a] font-extrabold">Ek Kayıt Günü</Text>
                  <Text className="mt-1 text-[#64748b] font-semibold text-[12px]">
                    Mevcut değer: {lookup?.ekKayitGun ?? 0}
                  </Text>
                  <TouchableOpacity
                    disabled={!canUpdate}
                    onPress={() => {
                      setEkGunText(String(lookup?.ekKayitGun ?? 0));
                      setAdvVisible(false);
                      setEkGunVisible(true);
                    }}
                    className={`mt-3 px-4 py-3 rounded-xl items-center ${canUpdate ? "bg-[#0f172a]" : "bg-[#e2e8f0]"}`}
                  >
                    <Text className={`font-extrabold ${canUpdate ? "text-white" : "text-[#64748b]"}`}>Düzenle</Text>
                  </TouchableOpacity>
                </View>

                <View className="mt-4 bg-[#fff7ed] border border-[#fed7aa] rounded-2xl p-4">
                  <Text className="text-[#0f172a] font-extrabold">Çoklu Sicile Aktar</Text>
                  <Text className="mt-1 text-[#64748b] font-semibold text-[12px]">
                    Tüm puantaj işlemleri tamamlandıktan sonra, sistemde tanımlı çoklu sicil bağlantıları olan kayıtlar için{" "}
                    <Text className="text-[#0f172a] font-extrabold">Aktarım Gün Sayısı</Text> kadar gün hedef sicillere{" "}
                    <Text className="text-[#0f172a] font-extrabold">NG 7,5</Text> yazar; ana sicilin ay sonundaki ilgili gün kayıtları kaldırılır.
                  </Text>
                  <TouchableOpacity
                    onPress={() => {
                      if (!canUpdate) return;
                      if (!personelId) {
                        showPopup("error", "Lütfen personel seçiniz.");
                        return;
                      }
                      setAdvVisible(false);
                      setCokluVisible(true);
                    }}
                    className={`mt-3 px-4 py-3 rounded-xl items-center ${canUpdate ? "bg-[#f59e0b]" : "bg-[#fde68a]"}`}
                  >
                    <Text className="text-white font-extrabold">Çoklu Sicil Aktar</Text>
                  </TouchableOpacity>
                </View>
              </View>
            </ScrollView>
          </View>
        </View>
      </Modal>

      <SelectModal
        visible={firmaModal}
        title="Firma Seç"
        items={firmaItems}
        onClose={() => setFirmaModal(false)}
        onPick={async (k) => {
          const f = asInt(k, 0);
          setFirmaId(f || null);
          setIsyeriId(null);
          setPersonelId("");
          setRows([]);
          await loadLookups({ firmaId: f || null, isyeriId: null, yil, ay });
        }}
      />
      <SelectModal
        visible={isyeriModal}
        title="İşyeri Seç"
        items={isyeriItems}
        onClose={() => setIsyeriModal(false)}
        onPick={async (k) => {
          const i = k ? asInt(k, 0) : null;
          setIsyeriId(i);
          setPersonelId("");
          setRows([]);
          await loadLookups({ firmaId, isyeriId: i, yil, ay });
        }}
      />

      {/* Detail modal */}
      {detailVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setDetailVisible(false)}>
          <View className="flex-1 bg-black/60 justify-end">
            <View className="bg-white rounded-t-3xl overflow-hidden border-t border-[#e2e8f0]">
              <View className="px-4 py-4 border-b border-[#f1f5f9] flex-row items-center justify-between">
                <View>
                  <Text className="text-[#0f172a] font-extrabold text-[16px]">{fmtDateTR(pick(selectedRow, "Tarih", "tarih"))}</Text>
                  <Text className="text-[#64748b] font-semibold text-[12px]">{onayDurumuText(selectedRow)}</Text>
                </View>
                <TouchableOpacity onPress={() => setDetailVisible(false)} className="px-3 py-2 bg-[#f1f5f9] rounded-xl">
                  <Text className="text-[#334155] font-extrabold">Kapat</Text>
                </TouchableOpacity>
              </View>
              <ScrollView style={{ maxHeight: 520 }}>
                <View className="p-4">
                  <View className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl p-4">
                    <RowLabel label="Vardiya" value={(pick(selectedRow, "VardiyaTuru", "vardiyaTuru") ?? "-").toString()} />
                    <RowLabel label="İlk Giriş" value={fmtTime(pick(selectedRow, "IlkGiris", "ilkGiris"))} />
                    <RowLabel label="Son Çıkış" value={fmtTime(pick(selectedRow, "SonCikis", "sonCikis"))} />
                    <RowLabel label="Vardiya Baş." value={fmtTime(pick(selectedRow, "VardiyaBaslangic", "vardiyaBaslangic"))} />
                    <RowLabel label="Vardiya Bitiş" value={fmtTime(pick(selectedRow, "VardiyaBitis", "vardiyaBitis"))} />
                    <RowLabel label="Saatlik İzin (dk)" value={String(asInt(pick(selectedRow, "SaatlikIzinDakika", "saatlikIzinDakika") ?? 0, 0))} />
                    <RowLabel label="Erken Giriş (dk)" value={String(asInt(pick(selectedRow, "ErkenGirisDakika", "erkenGirisDakika") ?? 0, 0))} />
                    <RowLabel label="Geç Çıkış (dk)" value={String(asInt(pick(selectedRow, "GecCikisDakika", "gecCikisDakika") ?? 0, 0))} />
                    <RowLabel label="Sistem FM (dk)" value={String(asInt(pick(selectedRow, "SistemFMDakika", "sistemFmDakika") ?? 0, 0))} />
                    <RowLabel label="Düz. FM (dk)" value={String(asInt(pick(selectedRow, "DuzenlenenFMDakika", "duzenlenenFmDakika") ?? 0, 0))} />
                    <RowLabel label="Çalışma Tipi" value={(pick(selectedRow, "CalismaTipi", "calismaTipi") ?? "-").toString()} />
                    <RowLabel label="Çalışma Saati" value={String(pick(selectedRow, "Saat", "saat") ?? "-")} />
                    <RowLabel label="Açıklama" value={(pick(selectedRow, "Aciklama", "aciklama") ?? "").toString() || "-"} />
                  </View>

                  {(() => {
                    const editable = isRowEditable(pick(selectedRow, "Tarih", "tarih"), ekGun) && !isLockedRow(selectedRow);
                    const od = onayDurumuText(selectedRow);
                    const canRowApprove = canApprove && editable && od === "Bekliyor";
                    const canRowReject = canDelete && editable && od === "Bekliyor";
                    const canRowEdit = canUpdate && editable;
                    return (
                      <View className="mt-4">
                        <View className="flex-row gap-2">
                          {canRowApprove ? (
                            <TouchableOpacity
                              onPress={() => {
                                resetEditFieldsFromRow(selectedRow);
                                setEditMode("approve");
                                setEditVisible(true);
                              }}
                              className="flex-1 px-4 py-3 rounded-xl bg-[#16a34a] items-center"
                            >
                              <Text className="text-white font-extrabold">Onayla</Text>
                            </TouchableOpacity>
                          ) : null}
                          {canRowReject ? (
                            <TouchableOpacity
                              onPress={() => {
                                setAciklama((pick(selectedRow, "Aciklama", "aciklama") ?? "").toString());
                                setRejectVisible(true);
                              }}
                              className="flex-1 px-4 py-3 rounded-xl bg-[#dc2626] items-center"
                            >
                              <Text className="text-white font-extrabold">Reddet</Text>
                            </TouchableOpacity>
                          ) : null}
                        </View>
                        {canRowEdit ? (
                          <TouchableOpacity
                            onPress={() => {
                              resetEditFieldsFromRow(selectedRow);
                              setEditMode("edit");
                              setEditVisible(true);
                            }}
                            className="mt-3 px-4 py-3 rounded-xl bg-[#0f172a] items-center"
                          >
                            <Text className="text-white font-extrabold">Düzenle</Text>
                          </TouchableOpacity>
                        ) : null}
                      </View>
                    );
                  })()}
                </View>
              </ScrollView>
            </View>
          </View>
        </Modal>
      ) : null}

      {/* Edit/Approve modal */}
      {editVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setEditVisible(false)}>
          <View className="flex-1 bg-black/60 justify-end">
            <View className="bg-white rounded-t-3xl overflow-hidden border-t border-[#e2e8f0]">
              <View className="px-4 py-4 border-b border-[#f1f5f9] flex-row items-center justify-between">
                <Text className="text-[#0f172a] font-extrabold text-[16px]">Düzenle / Onayla</Text>
                <TouchableOpacity onPress={() => setEditVisible(false)} className="px-3 py-2 bg-[#f1f5f9] rounded-xl">
                  <Text className="text-[#334155] font-extrabold">Kapat</Text>
                </TouchableOpacity>
              </View>
              <ScrollView style={{ maxHeight: 520 }}>
                <View className="p-4">
                  <Text className="text-[#64748b] font-semibold">Düzenlenmiş FM (dk)</Text>
                  <TextInput value={fmdk} onChangeText={setFmdk} keyboardType="numeric" className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" />

                  <Text className="mt-4 text-[#64748b] font-semibold">Çalışma Tipi</Text>
                  <TouchableOpacity onPress={() => setCalismaTipiModal(true)} className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0]">
                    <Text className="text-[#0f172a] font-extrabold">{calismaTipi ? calismaTipi : "(Boş)"}</Text>
                  </TouchableOpacity>

                  <Text className="mt-4 text-[#64748b] font-semibold">Çalışma Saati</Text>
                  <TextInput value={saatText} onChangeText={setSaatText} placeholder="örn: 7,5" className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" />

                  <Text className="mt-4 text-[#64748b] font-semibold">Açıklama</Text>
                  <TextInput
                    value={aciklama}
                    onChangeText={setAciklama}
                    multiline
                    className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                    style={{ minHeight: 90, textAlignVertical: "top" }}
                  />

                  <View className="mt-4 flex-row gap-2">
                    {canUpdate && editMode === "edit" ? (
                      <TouchableOpacity onPress={doEdit} disabled={saving} className={`flex-1 px-4 py-3 rounded-xl items-center ${saving ? "bg-[#94a3b8]" : "bg-[#0f172a]"}`}>
                        <Text className="text-white font-extrabold">{saving ? "Kaydediliyor..." : "Kaydet"}</Text>
                      </TouchableOpacity>
                    ) : null}
                    {canApprove && editMode === "approve" ? (
                      <TouchableOpacity onPress={doApprove} disabled={saving} className={`flex-1 px-4 py-3 rounded-xl items-center ${saving ? "bg-[#86efac]" : "bg-[#16a34a]"}`}>
                        <Text className="text-white font-extrabold">{saving ? "Onaylanıyor..." : "Onayla"}</Text>
                      </TouchableOpacity>
                    ) : null}
                  </View>
                </View>
              </ScrollView>
            </View>
          </View>
        </Modal>
      ) : null}

      {/* Reject modal */}
      {rejectVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setRejectVisible(false)}>
          <View className="flex-1 bg-black/60 justify-end">
            <View className="bg-white rounded-t-3xl overflow-hidden border-t border-[#e2e8f0]">
              <View className="px-4 py-4 border-b border-[#f1f5f9] flex-row items-center justify-between">
                <Text className="text-[#0f172a] font-extrabold text-[16px]">Reddet</Text>
                <TouchableOpacity onPress={() => setRejectVisible(false)} className="px-3 py-2 bg-[#f1f5f9] rounded-xl">
                  <Text className="text-[#334155] font-extrabold">Kapat</Text>
                </TouchableOpacity>
              </View>
              <View className="p-4">
                <Text className="text-[#64748b] font-semibold">Açıklama</Text>
                <TextInput
                  value={aciklama}
                  onChangeText={setAciklama}
                  multiline
                  className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                  style={{ minHeight: 90, textAlignVertical: "top" }}
                />
                <TouchableOpacity onPress={doReject} disabled={saving} className={`mt-4 px-4 py-3 rounded-xl items-center ${saving ? "bg-[#fca5a5]" : "bg-[#dc2626]"}`}>
                  <Text className="text-white font-extrabold">{saving ? "Gönderiliyor..." : "Reddet"}</Text>
                </TouchableOpacity>
              </View>
            </View>
          </View>
        </Modal>
      ) : null}

      {/* Bulk approve confirm */}
      {bulkVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setBulkVisible(false)}>
          <View className="flex-1 bg-black/60 justify-center px-6">
            <View className="bg-white rounded-2xl overflow-hidden">
              <View className="px-4 py-3 border-b border-[#f1f5f9]">
                <Text className="text-[#0f172a] font-extrabold">Toplu Onay</Text>
              </View>
              <View className="p-4">
                <Text className="text-[#334155] font-semibold">Seçili personelin {yil}/{String(ay).padStart(2, "0")} ayındaki puantajını toplu onaylamak istiyor musunuz?</Text>
                <View className="mt-4 flex-row gap-2">
                  <TouchableOpacity onPress={() => setBulkVisible(false)} className="flex-1 px-4 py-3 rounded-xl bg-[#f1f5f9] items-center">
                    <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
                  </TouchableOpacity>
                  <TouchableOpacity onPress={doBulkApprove} disabled={saving} className={`flex-1 px-4 py-3 rounded-xl items-center ${saving ? "bg-[#86efac]" : "bg-[#16a34a]"}`}>
                    <Text className="text-white font-extrabold">{saving ? "Onaylanıyor..." : "Onayla"}</Text>
                  </TouchableOpacity>
                </View>
              </View>
            </View>
          </View>
        </Modal>
      ) : null}

      {/* Çoklu sicil confirm */}
      {cokluVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setCokluVisible(false)}>
          <View className="flex-1 bg-black/60 justify-center px-6">
            <View className="bg-white rounded-2xl overflow-hidden">
              <View className="px-4 py-3 border-b border-[#f1f5f9]">
                <Text className="text-[#0f172a] font-extrabold">Çoklu Sicile Aktar</Text>
              </View>
              <View className="p-4">
                <Text className="text-[#334155] font-semibold">
                  {yil}/{String(ay).padStart(2, "0")} ayı için, sistemde tanımlı çoklu sicil bağlantıları olan kayıtların hedef sicillerine{" "}
                  <Text className="text-[#0f172a] font-extrabold">Aktarım Gün Sayısı kadar gün NG 7,5</Text> aktarmak istiyor musunuz?
                </Text>
                <View className="mt-4 flex-row gap-2">
                  <TouchableOpacity onPress={() => setCokluVisible(false)} className="flex-1 px-4 py-3 rounded-xl bg-[#f1f5f9] items-center">
                    <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
                  </TouchableOpacity>
                  <TouchableOpacity onPress={doCokluSicil} disabled={saving} className={`flex-1 px-4 py-3 rounded-xl items-center ${saving ? "bg-[#fcd34d]" : "bg-[#f59e0b]"}`}>
                    <Text className="text-white font-extrabold">{saving ? "Aktarılıyor..." : "Aktar"}</Text>
                  </TouchableOpacity>
                </View>
              </View>
            </View>
          </View>
        </Modal>
      ) : null}

      {/* Ek kayıt günü modal */}
      {ekGunVisible ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setEkGunVisible(false)}>
          <View className="flex-1 bg-black/60 justify-center px-6">
            <View className="bg-white rounded-2xl overflow-hidden">
              <View className="px-4 py-3 border-b border-[#f1f5f9]">
                <Text className="text-[#0f172a] font-extrabold">Ek Kayıt Günü</Text>
              </View>
              <View className="p-4">
                <Text className="text-[#64748b] font-semibold">Değer</Text>
                <TextInput value={ekGunText} onChangeText={setEkGunText} keyboardType="numeric" className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" />
                <View className="mt-4 flex-row gap-2">
                  <TouchableOpacity onPress={() => setEkGunVisible(false)} className="flex-1 px-4 py-3 rounded-xl bg-[#f1f5f9] items-center">
                    <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
                  </TouchableOpacity>
                  <TouchableOpacity onPress={doSetEkGun} disabled={saving} className={`flex-1 px-4 py-3 rounded-xl items-center ${saving ? "bg-[#94a3b8]" : "bg-[#0f172a]"}`}>
                    <Text className="text-white font-extrabold">{saving ? "Kaydediliyor..." : "Kaydet"}</Text>
                  </TouchableOpacity>
                </View>
              </View>
            </View>
          </View>
        </Modal>
      ) : null}

      {/* Çalışma tipi modal (reuses SelectModal) */}
      <SelectModal
        visible={calismaTipiModal}
        title="Çalışma Tipi Seç"
        items={tipItems}
        onClose={() => setCalismaTipiModal(false)}
        onPick={(k) => setCalismaTipi(k)}
      />
    </View>
  );
}

