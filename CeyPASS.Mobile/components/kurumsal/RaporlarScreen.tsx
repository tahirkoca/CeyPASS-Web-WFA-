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

function pick<T = any>(obj: any, a: string, b?: string): T | undefined {
  if (!obj) return undefined;
  if (obj[a] !== undefined) return obj[a] as T;
  if (b && obj[b] !== undefined) return obj[b] as T;
  return undefined;
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
        rightA11yLabel="Bildirimler ve hesap"
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
        const r = await raporService.list();
        if (!r?.success) throw new Error(r?.message ?? "Raporlar alınamadı.");
        const list = r.data ?? (r as any).Data ?? [];
        setRaporlar(Array.isArray(list) ? list : []);
      } catch (e: any) {
        setError(e?.message ?? "Beklenmeyen hata");
      } finally {
        setLoading(false);
      }
    })();
  }, []);

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

  const pageSizeItems = useMemo(() => [50, 100, 200, 500].map((n) => ({ key: String(n), label: String(n) })), []);

  const runReport = async (forcePage?: number) => {
    if (!procedureAdi) {
      showPopup("error", "Rapor seçiniz.");
      return;
    }
    const p = forcePage ?? page;
    setLoading(true);
    setError(null);
    try {
      const r = await raporService.run({
        procedureAdi,
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
        "@TarihBaslangic": fmtIsoDate(tBas),
        "@TarihBitis": fmtIsoDate(tBit),
      };
      const resp = await raporService.export({
        procedureName: procedureAdi,
        exportTitle: title,
        format,
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

