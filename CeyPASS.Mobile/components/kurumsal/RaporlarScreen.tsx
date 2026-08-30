import React, { useEffect, useMemo, useRef, useState } from "react";
import { ActivityIndicator, FlatList, Modal, Platform, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import DateTimePicker from "@react-native-community/datetimepicker";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { StatusPopup } from "../StatusPopup";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";
import { raporService, ReportTable, RaporTanimi } from "../../services/raporApi";
import * as FileSystem from "expo-file-system/legacy";
import * as Sharing from "expo-sharing";
import { Buffer } from "buffer";
import { PdfPreviewModal } from "../PdfPreviewModal";
import { BusyOverlay } from "../BusyOverlay";
import { pageFilterPrefs, parsePrefDate } from "../../services/pageFilterPrefs";

function pick<T = any>(obj: any, a: string, b?: string): T | undefined {
  if (!obj) return undefined;
  if (obj[a] !== undefined) return obj[a] as T;
  if (b && obj[b] !== undefined) return obj[b] as T;
  return undefined;
}

function hasRaporParam(names: string[], param: string) {
  const want = (param.startsWith("@") ? param : `@${param}`).toLowerCase();
  return names.some((n) => {
    const x = (n.startsWith("@") ? n : `@${n}`).toLowerCase();
    return x === want;
  });
}

function raporMultiKind(names: string[]): "none" | "isyeri" | "cihaz" {
  if (hasRaporParam(names, "@CihazIdList")) return "cihaz";
  if (hasRaporParam(names, "@IsyeriIdList")) return "isyeri";
  return "none";
}

function fmtIsoDate(d: Date) {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

function fmtDateTR(value: any) {
  if (!value) return "-";
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return String(value);
  return d.toLocaleDateString("tr-TR", { year: "numeric", month: "2-digit", day: "2-digit" });
}

function normalizeDateOnly(d: Date) {
  const x = new Date(d);
  x.setHours(12, 0, 0, 0);
  return x;
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

function RowLabel({ label, value }: { label: string; value: string }) {
  return (
    <View className="flex-row justify-between py-2">
      <Text className="text-[#64748b] font-semibold">{label}</Text>
      <Text className="text-[#0f172a] font-extrabold">{value}</Text>
    </View>
  );
}

export function RaporlarScreen(props: { user: any; abilities: any; onOpenMenu: () => void }) {
  const actions = props.abilities?.actions?.Raporlar ?? props.abilities?.Actions?.Raporlar ?? {};
  const rolId: number | null | undefined = props.abilities?.rolId ?? props.abilities?.RolId;
  const isAdmin = rolId === 1 || rolId === 2 || props.abilities?.isAdmin === true || props.abilities?.IsAdmin === true;
  const canExport =
    !!(actions?.Export ?? actions?.export) ||
    // fallback: abilities may be cached without Export key; admin + view still should show buttons
    (isAdmin && !!(props.abilities?.view?.Raporlar ?? props.abilities?.View?.Raporlar));

  const [loading, setLoading] = useState(true);
  const [filtersHydrated, setFiltersHydrated] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [raporlar, setRaporlar] = useState<RaporTanimi[]>([]);
  const [procedureAdi, setProcedureAdi] = useState<string>("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(100);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [tBas, setTBas] = useState<Date>(() => normalizeDateOnly(new Date(new Date().getFullYear(), new Date().getMonth(), 1)));
  const [tBit, setTBit] = useState<Date>(() => normalizeDateOnly(new Date()));

  const [table, setTable] = useState<ReportTable | null>(null);
  const [q, setQ] = useState("");
  const [viewMode, setViewMode] = useState<"cards" | "table">("cards");
  const [detailVisible, setDetailVisible] = useState(false);
  const [detailRow, setDetailRow] = useState<(string | null)[] | null>(null);

  const [exportPreviewVisible, setExportPreviewVisible] = useState(false);
  const [exportFileUri, setExportFileUri] = useState<string | null>(null);
  const [exportTitle, setExportTitle] = useState<string>("Rapor");
  const [exportFormat, setExportFormat] = useState<"pdf" | "excel">("pdf");

  const [raporModal, setRaporModal] = useState(false);
  const [firmaModal, setFirmaModal] = useState(false);
  const [pageSizeModal, setPageSizeModal] = useState(false);
  const [isyeriModal, setIsyeriModal] = useState(false);
  const [firmaList, setFirmaList] = useState<{ id: number; ad: string }[]>([]);
  const [selectedFirmaId, setSelectedFirmaId] = useState<number | null>(null);
  const [isyeriList, setIsyeriList] = useState<{ id: number; ad: string }[]>([]);
  const [cihazList, setCihazList] = useState<{ id: number; ad: string }[]>([]);
  const [selectedIsyeriIds, setSelectedIsyeriIds] = useState<number[]>([]);
  const [selectedCihazIds, setSelectedCihazIds] = useState<number[]>([]);
  const [parametreler, setParametreler] = useState<string[]>([]);
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
  const quickMenu = useHeaderQuickMenu();
  const notif = useNotificationsContext();
  const header = (
    <>
      <PageHeader
        title="Raporlar"
        onOpenMenu={props.onOpenMenu}
        rightIcon="bell-outline"
        rightBadge={notif.unreadCount}
        onRightPress={() => quickMenu.open("notif")}
      />
      {quickMenu.modal}
    </>
  );

  const firstLoadRef = useRef(false);
  useEffect(() => {
    if (firstLoadRef.current) return;
    firstLoadRef.current = true;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const [r, firmalarRes, prefs] = await Promise.all([
          raporService.list(),
          raporService.firmalar(),
          pageFilterPrefs.load("Raporlar"),
        ]);
        if (!r?.success) throw new Error(r?.message ?? "Raporlar alınamadı.");
        const list = r.data ?? (r as any).Data ?? [];
        const raporList = Array.isArray(list) ? list : [];
        setRaporlar(raporList);

        let firms: { id: number; ad: string }[] = [];
        if (firmalarRes?.success) {
          const raw = firmalarRes.data ?? (firmalarRes as any).Data ?? [];
          firms = (Array.isArray(raw) ? raw : [])
            .map((x: any) => ({
              id: Number(pick<any>(x, "id", "Id") ?? -1),
              ad: String(pick<any>(x, "ad", "Ad") ?? ""),
            }))
            .filter((x) => x.ad && (x.id > 0 || (isAdmin && x.id === 0)));
        }
        setFirmaList(firms);

        const userFirma =
          Number(pick<any>(props.user, "firmaId", "FirmaId") ?? pick<any>(props.abilities, "aktifFirmaId", "AktifFirmaId") ?? 0) || 0;
        let prefer =
          prefs?.firmaId != null && firms.some((f) => f.id === prefs.firmaId)
            ? prefs.firmaId
            : firms.find((f) => f.id === userFirma && f.id > 0)?.id ?? null;
        if (prefer == null) prefer = firms.find((f) => f.id > 0)?.id ?? (isAdmin ? 0 : null);
        setSelectedFirmaId(prefer);

        const da = parsePrefDate(prefs?.dateA ?? null);
        const db = parsePrefDate(prefs?.dateB ?? null);
        if (da) setTBas(normalizeDateOnly(da));
        if (db) setTBit(normalizeDateOnly(db));

        if (prefs?.extra) {
          const hit = raporList.find((x: any) => {
            const proc = (pick<any>(x, "procedureAdi", "ProcedureAdi") ?? "").toString();
            const ad = (pick<any>(x, "raporAdi", "RaporAdi") ?? "").toString();
            return (
              proc.toLowerCase() === String(prefs.extra).toLowerCase() ||
              ad.toLowerCase() === String(prefs.extra).toLowerCase()
            );
          });
          if (hit) {
            const proc = (pick<any>(hit, "procedureAdi", "ProcedureAdi") ?? "").toString();
            if (proc) setProcedureAdi(proc);
          }
        }
      } catch (e: any) {
        setError(e?.message ?? "Beklenmeyen hata");
      } finally {
        setLoading(false);
        setFiltersHydrated(true);
      }
    })();
  }, []);

  useEffect(() => {
    if (!filtersHydrated) return;
    void pageFilterPrefs.save("Raporlar", {
      firmaId: selectedFirmaId,
      dateA: tBas,
      dateB: tBit,
      extra: procedureAdi || null,
    });
  }, [filtersHydrated, selectedFirmaId, tBas, tBit, procedureAdi]);

  const loadMultiForFirma = async (firmaId: number | null) => {
    if (firmaId == null || firmaId <= 0) {
      setIsyeriList([]);
      setCihazList([]);
      setSelectedIsyeriIds([]);
      setSelectedCihazIds([]);
      return;
    }
    try {
      const [iy, cz] = await Promise.all([raporService.isyerleri(firmaId), raporService.cihazlar(firmaId)]);
      if (iy?.success) {
        const raw = iy.data ?? (iy as any).Data ?? [];
        setIsyeriList(
          (Array.isArray(raw) ? raw : [])
            .map((x: any) => ({
              id: Number(pick<any>(x, "id", "Id") ?? 0),
              ad: String(pick<any>(x, "ad", "Ad") ?? ""),
            }))
            .filter((x) => x.id > 0 && x.ad)
        );
      } else setIsyeriList([]);
      if (cz?.success) {
        const raw = cz.data ?? (cz as any).Data ?? [];
        setCihazList(
          (Array.isArray(raw) ? raw : [])
            .map((x: any) => ({
              id: Number(pick<any>(x, "id", "Id") ?? 0),
              ad: String(pick<any>(x, "ad", "Ad") ?? ""),
            }))
            .filter((x) => x.id > 0 && x.ad)
        );
      } else setCihazList([]);
      setSelectedIsyeriIds([]);
      setSelectedCihazIds([]);
    } catch {
      setIsyeriList([]);
      setCihazList([]);
    }
  };

  useEffect(() => {
    loadMultiForFirma(selectedFirmaId).catch(() => {});
  }, [selectedFirmaId]);

  useEffect(() => {
    if (!procedureAdi) {
      setParametreler([]);
      return;
    }
    (async () => {
      try {
        const res = await raporService.parametreler(procedureAdi);
        const raw = res?.data ?? (res as any)?.Data ?? [];
        setParametreler(Array.isArray(raw) ? raw.map((x: any) => String(x)) : []);
        setSelectedIsyeriIds([]);
        setSelectedCihazIds([]);
      } catch {
        setParametreler([]);
      }
    })();
  }, [procedureAdi]);

  const raporItems = useMemo(() => {
    const active = (raporlar ?? []).filter((x) => !!(pick<any>(x, "aktifMi", "AktifMi")));
    return active.map((r) => ({
      key: (pick<any>(r, "procedureAdi", "ProcedureAdi") ?? "").toString(),
      label: (pick<any>(r, "raporAdi", "RaporAdi") ?? "").toString(),
    })).filter((x) => x.key && x.label);
  }, [raporlar]);

  const selectedRaporLabel = useMemo(() => {
    if (!procedureAdi) return "-- Rapor Seçiniz --";
    const found = raporItems.find((x) => x.key === procedureAdi);
    return found?.label ?? procedureAdi;
  }, [procedureAdi, raporItems]);

  const firmaItems = useMemo(
    () => firmaList.map((f) => ({ key: String(f.id), label: f.ad })),
    [firmaList]
  );

  const selectedFirmaLabel = useMemo(() => {
    if (selectedFirmaId == null) return "-- Firma Seçiniz --";
    const found = firmaList.find((f) => f.id === selectedFirmaId);
    return found?.ad ?? `#${selectedFirmaId}`;
  }, [selectedFirmaId, firmaList]);

  const pageSizeItems = useMemo(() => [50, 100, 200, 500].map((n) => ({ key: String(n), label: String(n) })), []);

  const multiKind = useMemo(() => {
    if (selectedFirmaId == null || selectedFirmaId <= 0) return "none" as const;
    return raporMultiKind(parametreler);
  }, [parametreler, selectedFirmaId]);

  const isyeriLabel = useMemo(() => {
    if (!selectedIsyeriIds.length) return "Tümü (yetkili işyerler)";
    const names = selectedIsyeriIds
      .map((id) => isyeriList.find((x) => x.id === id)?.ad ?? `#${id}`)
      .join(", ");
    return names || "Seçili işyerler";
  }, [selectedIsyeriIds, isyeriList]);

  const cihazLabel = useMemo(() => {
    if (!selectedCihazIds.length) return "Tümü (aktif cihazlar)";
    const names = selectedCihazIds
      .map((id) => cihazList.find((x) => x.id === id)?.ad ?? `#${id}`)
      .join(", ");
    return names || "Seçili cihazlar";
  }, [selectedCihazIds, cihazList]);

  const runReport = async (forcePage?: number) => {
    if (!procedureAdi) {
      showPopup("error", "Rapor seçiniz.");
      return;
    }
    if (selectedFirmaId == null || (selectedFirmaId === 0 && !isAdmin) || selectedFirmaId < 0) {
      showPopup("error", "Firma seçiniz.");
      return;
    }
    const p = forcePage ?? page;
    setLoading(true);
    setError(null);
    try {
      const r = await raporService.run({
        procedureAdi,
        firmaId: selectedFirmaId,
        isyeriIds: multiKind === "isyeri" && selectedIsyeriIds.length ? selectedIsyeriIds : undefined,
        cihazIds: multiKind === "cihaz" && selectedCihazIds.length ? selectedCihazIds : undefined,
        tarihBaslangic: fmtIsoDate(tBas),
        tarihBitis: fmtIsoDate(tBit),
        page: p,
        pageSize,
      });
      if (!r?.success) throw new Error(r?.message ?? "Rapor çalıştırılamadı.");
      const payload = r.data ?? (r as any).Data ?? {};
      const items = payload?.items ?? payload?.Items ?? [];
      const first = items?.[0] ?? null;
      setTable(first);
      setTotalCount(Number(payload?.totalCount ?? payload?.TotalCount ?? 0) || 0);
      setTotalPages(Number(payload?.totalPages ?? payload?.TotalPages ?? 1) || 1);
      setPage(Number(payload?.page ?? payload?.Page ?? p) || p);
    } catch (e: any) {
      setError(e?.message ?? "Hata");
      setTable(null);
      setTotalCount(0);
      setTotalPages(1);
    } finally {
      setLoading(false);
    }
  };

  const filteredRows = useMemo(() => {
    const cols = (table?.columns ?? table?.Columns ?? []) as string[];
    const rows = (table?.rows ?? table?.Rows ?? []) as (string | null)[][];
    const qq = q.trim().toLowerCase();
    if (!qq) return { cols, rows };
    const out = rows.filter((r) => r.some((cell) => (cell ?? "").toString().toLowerCase().includes(qq)));
    return { cols, rows: out };
  }, [table, q]);

  const cardPrimaryCols = useMemo(() => filteredRows.cols.slice(0, 4), [filteredRows.cols]);

  const openRowDetail = (row: (string | null)[]) => {
    setDetailRow(row);
    setDetailVisible(true);
  };

  const exportRun = async (format: "pdf" | "excel") => {
    if (!procedureAdi) {
      showPopup("error", "Rapor seçiniz.");
      return;
    }
    if (!canExport) {
      showPopup("error", "Export yetkiniz yok.");
      return;
    }
    try {
      const title = selectedRaporLabel || "Rapor";
      // Web ile aynı parametre isimleri
      const params: Record<string, any> = {
        "@FirmaIdList": selectedFirmaId != null && selectedFirmaId > 0 ? String(selectedFirmaId) : "",
        "@TarihBaslangic": fmtIsoDate(tBas),
        "@TarihBitis": fmtIsoDate(tBit),
      };
      if (selectedFirmaId == null || (selectedFirmaId === 0 && !isAdmin)) {
        showPopup("error", "Firma seçiniz.");
        return;
      }
      const resp = await raporService.export({
        procedureName: procedureAdi,
        exportTitle: title,
        format,
        isyeriIds: multiKind === "isyeri" && selectedIsyeriIds.length ? selectedIsyeriIds : undefined,
        cihazIds: multiKind === "cihaz" && selectedCihazIds.length ? selectedCihazIds : undefined,
        params,
      });

      const ext = format === "excel" ? "xlsx" : "pdf";
      const contentType = format === "excel"
        ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        : "application/pdf";
      const fileUri = `${FileSystem.cacheDirectory}${title}_${Date.now()}.${ext}`;
      const b64 = Buffer.from(resp.data).toString("base64");
      await FileSystem.writeAsStringAsync(fileUri, b64, { encoding: FileSystem.EncodingType.Base64 });
      // Preview-first UX (web parity: user sees output before downloading)
      setExportTitle(title);
      setExportFormat(format);
      setExportFileUri(fileUri);
      setExportPreviewVisible(true);
    } catch (e: any) {
      // axios arraybuffer error: try decode server message
      try {
        const data = e?.response?.data;
        if (data && data instanceof ArrayBuffer) {
          const text = new TextDecoder("utf-8").decode(new Uint8Array(data));
          try {
            const json = JSON.parse(text);
            showPopup("error", json?.message ?? json?.Message ?? text);
            return;
          } catch {
            showPopup("error", text);
            return;
          }
        }
      } catch {}
      showPopup("error", e?.message ?? "Export hatası");
    }
  };

  if (loading && !raporlar.length && !error) {
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
      <BusyOverlay visible={loading && !!raporlar.length} title="Yükleniyor..." message="Rapor hazırlanıyor" />

      {/* PDF preview uses existing reusable modal */}
      <PdfPreviewModal
        visible={exportPreviewVisible && exportFormat === "pdf"}
        title={exportTitle}
        fileUri={exportFileUri}
        onClose={() => setExportPreviewVisible(false)}
      />

      {/* Excel preview: show a lightweight data preview, then allow download/share */}
      <Modal
        visible={exportPreviewVisible && exportFormat === "excel"}
        transparent
        animationType="fade"
        onRequestClose={() => setExportPreviewVisible(false)}
      >
        <View className="flex-1 bg-black/60">
          <View className="flex-row items-center justify-between px-4 pt-12 pb-3 bg-white">
            <Text className="text-[#1e293b] font-extrabold text-[14px]" numberOfLines={1}>
              {exportTitle} (Excel)
            </Text>
            <TouchableOpacity onPress={() => setExportPreviewVisible(false)} className="px-3 py-2 bg-[#f1f5f9] rounded-xl">
              <Text className="text-[#334155] font-extrabold">Kapat</Text>
            </TouchableOpacity>
          </View>

          <View className="flex-1 bg-white px-4 py-4">
            <Text className="text-[#0f172a] font-extrabold mb-2">Önizleme</Text>
            <Text className="text-[#64748b] font-semibold text-[12px] mb-3">
              Excel dosyası indirilebilir. Aşağıda raporun ilk satırlarının önizlemesi yer alır.
            </Text>

            <ScrollView horizontal>
              <View>
                <View className="flex-row bg-[#f1f5f9] rounded-t-xl overflow-hidden">
                  {filteredRows.cols.slice(0, 8).map((c, idx) => (
                    <View
                      key={`ex_h_${c}_${idx}`}
                      style={{ width: 160, padding: 10, borderRightWidth: idx === Math.min(7, filteredRows.cols.length - 1) ? 0 : 1, borderColor: "#e2e8f0" }}
                    >
                      <Text className="text-[#0f172a] font-extrabold text-[12px]" numberOfLines={2}>
                        {c}
                      </Text>
                    </View>
                  ))}
                </View>
                {filteredRows.rows.slice(0, 20).map((r, ridx) => (
                  <View key={`ex_r_${ridx}`} className={`flex-row ${ridx % 2 === 0 ? "bg-white" : "bg-[#f8fafc]"}`}>
                    {r.slice(0, 8).map((cell, cidx) => (
                      <View
                        key={`ex_c_${ridx}_${cidx}`}
                        style={{ width: 160, padding: 10, borderTopWidth: 1, borderRightWidth: cidx === 7 ? 0 : 1, borderColor: "#e2e8f0" }}
                      >
                        <Text className="text-[#334155] font-semibold text-[12px]" numberOfLines={3}>
                          {(cell ?? "-").toString()}
                        </Text>
                      </View>
                    ))}
                  </View>
                ))}
              </View>
            </ScrollView>
          </View>

          <View className="px-4 py-4 bg-white border-t border-[#f1f5f9]">
            <TouchableOpacity
              disabled={!exportFileUri}
              onPress={async () => {
                if (!exportFileUri) return;
                if (await Sharing.isAvailableAsync()) {
                  await Sharing.shareAsync(exportFileUri, {
                    mimeType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    dialogTitle: exportTitle,
                  });
                } else {
                  showPopup("success", "Dosya oluşturuldu: " + exportFileUri);
                }
              }}
              className={`rounded-xl px-4 py-3 ${exportFileUri ? "bg-[#16a34a]" : "bg-[#f1f5f9]"}`}
            >
              <Text className={`font-extrabold text-center ${exportFileUri ? "text-white" : "text-[#94a3b8]"}`}>
                İndir / Paylaş
              </Text>
            </TouchableOpacity>
          </View>
        </View>
      </Modal>

      {header}

      <ScrollView className="flex-1 px-4" contentContainerStyle={{ paddingBottom: 24 }}>
        <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
          <View className="px-4 py-3 border-b border-[#f1f5f9]">
            <Text className="text-[#0f172a] font-extrabold">Rapor Seçimi ve Parametreler</Text>
          </View>
          <View className="p-4">
            <TouchableOpacity onPress={() => setFirmaModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
              <RowLabel label="Firma" value={selectedFirmaLabel} />
            </TouchableOpacity>
            <View className="h-3" />
            <TouchableOpacity onPress={() => setRaporModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
              <RowLabel label="Rapor Türü" value={selectedRaporLabel} />
            </TouchableOpacity>
            <View className="h-3" />
            <TouchableOpacity
              onPress={() => {
                setPickerTemp(tBas);
                setDatePickerOpen("bas");
              }}
              className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
            >
              <RowLabel label="Başlangıç Tarihi" value={fmtDateTR(tBas)} />
            </TouchableOpacity>
            <View className="h-3" />
            <TouchableOpacity
              onPress={() => {
                setPickerTemp(tBit);
                setDatePickerOpen("bit");
              }}
              className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
            >
              <RowLabel label="Bitiş Tarihi" value={fmtDateTR(tBit)} />
            </TouchableOpacity>
            <View className="h-3" />
            {multiKind === "isyeri" && isyeriList.length > 0 ? (
              <>
                <TouchableOpacity onPress={() => setIsyeriModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
                  <RowLabel label="İşyerleri" value={isyeriLabel} />
                </TouchableOpacity>
                <View className="h-3" />
              </>
            ) : null}
            {multiKind === "cihaz" && cihazList.length > 0 ? (
              <>
                <TouchableOpacity onPress={() => setIsyeriModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
                  <RowLabel label="Cihazlar" value={cihazLabel} />
                </TouchableOpacity>
                <View className="h-3" />
              </>
            ) : null}
            <TouchableOpacity onPress={() => setPageSizeModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
              <RowLabel label="Sayfa başına" value={String(pageSize)} />
            </TouchableOpacity>
            <View className="h-3" />
            <TouchableOpacity
              onPress={() => {
                setPage(1);
                runReport(1).catch(() => {});
              }}
              className="bg-[#dc2626] rounded-xl py-3 items-center"
            >
              <Text className="text-white font-extrabold">{loading ? "Hazırlanıyor..." : "Rapor Getir"}</Text>
            </TouchableOpacity>
          </View>
        </View>

        <View className="h-4" />

        <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
          <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
            <View className="flex-1">
              <Text className="text-[#0f172a] font-extrabold">{selectedRaporLabel || "Rapor"}</Text>
              <Text className="text-[#64748b] font-semibold text-[12px]">
                Toplam {totalCount} kayıt • Sayfa {page} / {totalPages}
              </Text>
            </View>
            <View className="flex-row gap-2 items-center">
              <TouchableOpacity
                onPress={() => setViewMode("cards")}
                className={`px-3 py-2 rounded-xl ${viewMode === "cards" ? "bg-[#0f172a]" : "bg-[#f1f5f9]"}`}
              >
                <Text className={`font-extrabold ${viewMode === "cards" ? "text-white" : "text-[#334155]"}`}>Kart</Text>
              </TouchableOpacity>
              <TouchableOpacity
                onPress={() => setViewMode("table")}
                className={`px-3 py-2 rounded-xl ${viewMode === "table" ? "bg-[#0f172a]" : "bg-[#f1f5f9]"}`}
              >
                <Text className={`font-extrabold ${viewMode === "table" ? "text-white" : "text-[#334155]"}`}>Tablo</Text>
              </TouchableOpacity>
              {canExport ? (
                <>
                  <TouchableOpacity onPress={() => exportRun("excel")} className="px-3 py-2 rounded-xl bg-[#16a34a]">
                    <Text className="text-white font-extrabold">Excel</Text>
                  </TouchableOpacity>
                  <TouchableOpacity onPress={() => exportRun("pdf")} className="px-3 py-2 rounded-xl bg-[#dc2626]">
                    <Text className="text-white font-extrabold">PDF</Text>
                  </TouchableOpacity>
                </>
              ) : null}
            </View>
          </View>

          {error ? (
            <View className="p-4">
              <Text className="text-[#b91c1c] font-extrabold">Hata</Text>
              <Text className="text-[#334155] font-semibold mt-1">{error}</Text>
            </View>
          ) : null}

          <View className="p-4">
            <TextInput
              value={q}
              onChangeText={setQ}
              placeholder="Raporlarda arama..."
              className="px-3 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a]"
            />
          </View>

          {viewMode === "cards" ? (
            <View className="px-4 pb-4">
              {!filteredRows.rows.length ? (
                <View className="py-6">
                  <Text className="text-[#64748b] font-semibold text-center">Veri yok</Text>
                </View>
              ) : (
                <FlatList
                  data={filteredRows.rows}
                  keyExtractor={(_, idx) => `card_${idx}`}
                  scrollEnabled={false}
                  renderItem={({ item: row, index }) => {
                    const pairs = cardPrimaryCols.map((c, i) => {
                      const colIdx = filteredRows.cols.indexOf(c);
                      const v = colIdx >= 0 ? row[colIdx] : null;
                      return { c, v };
                    });
                    return (
                      <TouchableOpacity
                        activeOpacity={0.85}
                        onPress={() => openRowDetail(row)}
                        className={`rounded-2xl border border-[#e2e8f0] p-4 mb-3 ${index % 2 === 0 ? "bg-white" : "bg-[#f8fafc]"}`}
                      >
                        {pairs.map((p, i) => (
                          <View key={`${p.c}_${i}`} className="flex-row justify-between mb-2">
                            <Text className="text-[#64748b] font-semibold mr-3 flex-1" numberOfLines={1}>
                              {p.c}
                            </Text>
                            <Text className="text-[#0f172a] font-extrabold flex-1 text-right" numberOfLines={2}>
                              {(p.v ?? "-").toString()}
                            </Text>
                          </View>
                        ))}
                        <View className="mt-2 flex-row items-center justify-end">
                          <Text className="text-[#0284c7] font-extrabold">Detay</Text>
                          <MaterialCommunityIcons name="chevron-right" size={18} color="#0284c7" />
                        </View>
                      </TouchableOpacity>
                    );
                  }}
                />
              )}
            </View>
          ) : (
            <ScrollView horizontal className="px-4 pb-4">
              <View>
                {/* Header */}
                <View className="flex-row bg-[#f1f5f9] rounded-t-xl overflow-hidden">
                  {filteredRows.cols.map((c, idx) => (
                    <View
                      key={`${c}_${idx}`}
                      style={{ width: 160, padding: 10, borderRightWidth: idx === filteredRows.cols.length - 1 ? 0 : 1, borderColor: "#e2e8f0" }}
                    >
                      <Text className="text-[#0f172a] font-extrabold text-[12px]" numberOfLines={2}>
                        {c}
                      </Text>
                    </View>
                  ))}
                </View>
                {/* Rows */}
                {filteredRows.rows.map((r, ridx) => (
                  <TouchableOpacity
                    key={`r_${ridx}`}
                    activeOpacity={0.9}
                    onPress={() => openRowDetail(r)}
                    className={`flex-row ${ridx % 2 === 0 ? "bg-white" : "bg-[#f8fafc]"}`}
                  >
                    {r.map((cell, cidx) => (
                      <View
                        key={`c_${ridx}_${cidx}`}
                        style={{ width: 160, padding: 10, borderTopWidth: 1, borderRightWidth: cidx === r.length - 1 ? 0 : 1, borderColor: "#e2e8f0" }}
                      >
                        <Text className="text-[#334155] font-semibold text-[12px]" numberOfLines={3}>
                          {(cell ?? "-").toString()}
                        </Text>
                      </View>
                    ))}
                  </TouchableOpacity>
                ))}
                {!filteredRows.rows.length ? (
                  <View className="py-6">
                    <Text className="text-[#64748b] font-semibold text-center">Veri yok</Text>
                  </View>
                ) : null}
              </View>
            </ScrollView>
          )}

          <View className="p-4 flex-row items-center justify-between">
            <TouchableOpacity
              disabled={page <= 1 || loading}
              onPress={() => {
                const next = Math.max(1, page - 1);
                setPage(next);
                runReport(next).catch(() => {});
              }}
              className={`px-4 py-3 rounded-xl ${page <= 1 || loading ? "bg-[#f1f5f9]" : "bg-white border border-[#e2e8f0]"}`}
            >
              <Text className={`font-extrabold ${page <= 1 || loading ? "text-[#94a3b8]" : "text-[#0f172a]"}`}>Önceki</Text>
            </TouchableOpacity>
            <Text className="text-[#64748b] font-extrabold">
              {page} / {totalPages}
            </Text>
            <TouchableOpacity
              disabled={page >= totalPages || loading}
              onPress={() => {
                const next = Math.min(totalPages, page + 1);
                setPage(next);
                runReport(next).catch(() => {});
              }}
              className={`px-4 py-3 rounded-xl ${page >= totalPages || loading ? "bg-[#f1f5f9]" : "bg-white border border-[#e2e8f0]"}`}
            >
              <Text className={`font-extrabold ${page >= totalPages || loading ? "text-[#94a3b8]" : "text-[#0f172a]"}`}>Sonraki</Text>
            </TouchableOpacity>
          </View>
        </View>
      </ScrollView>

      {raporModal ? (
        <SelectModal
          visible={raporModal}
          title="Rapor Seç"
          items={raporItems}
          onClose={() => setRaporModal(false)}
          onPick={(key) => {
            setProcedureAdi(key);
            setTable(null);
            setTotalCount(0);
            setTotalPages(1);
            setPage(1);
          }}
        />
      ) : null}

      {firmaModal ? (
        <SelectModal
          visible={firmaModal}
          title="Firma Seç"
          items={firmaItems}
          onClose={() => setFirmaModal(false)}
          onPick={(key) => {
            const id = Number(key);
            if (!Number.isFinite(id)) return;
            if (id === 0 && !isAdmin) return;
            setSelectedFirmaId(id);
            setTable(null);
            setTotalCount(0);
            setTotalPages(1);
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
            if ([50, 100, 200, 500].includes(n)) {
              setPageSize(n);
              setPage(1);
            }
          }}
        />
      ) : null}

      {isyeriModal ? (
        <Modal visible={isyeriModal} transparent animationType="fade" onRequestClose={() => setIsyeriModal(false)}>
          <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setIsyeriModal(false)}>
            <TouchableOpacity activeOpacity={1} onPress={() => {}}>
              <View className="bg-white rounded-2xl overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9]">
                  <Text className="text-[#0f172a] font-extrabold text-[16px]">{multiKind === "cihaz" ? "Cihazlar" : "İşyerleri"}</Text>
                  <Text className="text-[#64748b] font-semibold text-[12px] mt-1">
                    {multiKind === "cihaz"
                      ? "Hiç seçilmezse aktif tüm cihazlar dahil edilir."
                      : "Hiç seçilmezse yetkili tüm işyerleri dahil edilir."}
                  </Text>
                </View>
                <ScrollView style={{ maxHeight: 420 }}>
                  <TouchableOpacity
                    className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between"
                    onPress={() => (multiKind === "cihaz" ? setSelectedCihazIds([]) : setSelectedIsyeriIds([]))}
                  >
                    <Text className="text-[#0f172a] font-semibold">Tümü</Text>
                    <MaterialCommunityIcons
                      name={(multiKind === "cihaz" ? selectedCihazIds : selectedIsyeriIds).length === 0 ? "checkbox-marked" : "checkbox-blank-outline"}
                      size={22}
                      color={(multiKind === "cihaz" ? selectedCihazIds : selectedIsyeriIds).length === 0 ? "#dc2626" : "#94a3b8"}
                    />
                  </TouchableOpacity>
                  {(multiKind === "cihaz" ? cihazList : isyeriList).map((iy) => {
                    const selected = multiKind === "cihaz" ? selectedCihazIds : selectedIsyeriIds;
                    const checked = selected.includes(iy.id);
                    return (
                      <TouchableOpacity
                        key={`${multiKind}_${iy.id}`}
                        className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between"
                        onPress={() => {
                          const setter = multiKind === "cihaz" ? setSelectedCihazIds : setSelectedIsyeriIds;
                          setter((prev) =>
                            prev.includes(iy.id) ? prev.filter((x) => x !== iy.id) : [...prev, iy.id]
                          );
                        }}
                      >
                        <Text className="text-[#0f172a] font-semibold flex-1 pr-3">{iy.ad}</Text>
                        <MaterialCommunityIcons
                          name={checked ? "checkbox-marked" : "checkbox-blank-outline"}
                          size={22}
                          color={checked ? "#dc2626" : "#94a3b8"}
                        />
                      </TouchableOpacity>
                    );
                  })}
                </ScrollView>
                <View className="p-3">
                  <TouchableOpacity className="bg-[#dc2626] rounded-xl py-3 items-center" onPress={() => setIsyeriModal(false)}>
                    <Text className="text-white font-extrabold">Tamam</Text>
                  </TouchableOpacity>
                </View>
              </View>
            </TouchableOpacity>
          </TouchableOpacity>
        </Modal>
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
                      const k = datePickerOpen;
                      setDatePickerOpen(null);
                      if (k === "bas") setTBas(v);
                      else setTBit(v);
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

      {/* Row detail modal */}
      <Modal visible={detailVisible} transparent animationType="fade" onRequestClose={() => setDetailVisible(false)}>
        <View className="flex-1 bg-black/60">
          <View className="flex-row items-center justify-between px-4 pt-12 pb-3 bg-white">
            <Text className="text-[#1e293b] font-extrabold text-[14px]" numberOfLines={1}>
              Satır Detayı
            </Text>
            <TouchableOpacity onPress={() => setDetailVisible(false)} className="px-3 py-2 bg-[#f1f5f9] rounded-xl">
              <Text className="text-[#334155] font-extrabold">Kapat</Text>
            </TouchableOpacity>
          </View>
          <ScrollView className="flex-1 bg-white px-4 py-4">
            {(detailRow ?? []).map((v, idx) => {
              const c = filteredRows.cols[idx] ?? `Kolon ${idx + 1}`;
              return (
                <View key={`d_${idx}`} className="py-3 border-b border-[#f1f5f9]">
                  <Text className="text-[#64748b] font-semibold">{c}</Text>
                  <Text className="text-[#0f172a] font-extrabold mt-1">{(v ?? "-").toString()}</Text>
                </View>
              );
            })}
          </ScrollView>
        </View>
      </Modal>
    </View>
  );
}

