import React, { useEffect, useMemo, useRef, useState } from "react";
import { Modal, Platform, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { PersonelHeader } from "./PersonelHeader";
import { profilService, type IzinlerimResponse } from "../../services/profilApi";
import DateTimePicker from "@react-native-community/datetimepicker";
import { downloadPdfFromApi } from "../../services/pdf";
import { PdfPreviewModal } from "../PdfPreviewModal";
import { StatusPopup } from "../StatusPopup";

function fmtDate(v: any) {
  if (!v) return "";
  // Date-only fields must not shift by timezone.
  // Accept both "yyyy-MM-dd" and ISO like "yyyy-MM-ddTHH:mm:ss..." by using first 10 chars.
  if (typeof v === "string") {
    const s = v.trim();
    const datePart = s.length >= 10 ? s.slice(0, 10) : s;
    if (/^\d{4}-\d{2}-\d{2}$/.test(datePart)) {
      const [y, m, d] = datePart.split("-").map((x) => Number(x));
      if (y && m && d) return `${String(d).padStart(2, "0")}.${String(m).padStart(2, "0")}.${y}`;
    }
  }
  const d = new Date(v);
  if (Number.isNaN(d.getTime())) return (v ?? "").toString();
  return d.toLocaleDateString("tr-TR");
}

function normalizeDateOnly(d: Date) {
  // avoid timezone/DST shifts for date-only use
  return new Date(d.getFullYear(), d.getMonth(), d.getDate(), 12, 0, 0, 0);
}

function fmtDateTime(v: Date, saatlik: boolean) {
  const pad = (n: number) => (n < 10 ? `0${n}` : `${n}`);
  const dd = pad(v.getDate());
  const mm = pad(v.getMonth() + 1);
  const yyyy = v.getFullYear();
  if (!saatlik) return `${dd}.${mm}.${yyyy}`;
  return `${dd}.${mm}.${yyyy} ${pad(v.getHours())}:${pad(v.getMinutes())}`;
}

function toLocalIsoLike(d: Date, saatlik: boolean) {
  const pad = (n: number) => (n < 10 ? `0${n}` : `${n}`);
  const dd = pad(d.getDate());
  const mm = pad(d.getMonth() + 1);
  const yyyy = d.getFullYear();
  const hh = pad(d.getHours());
  const mi = pad(d.getMinutes());
  const ss = pad(d.getSeconds());
  if (!saatlik) return `${yyyy}-${mm}-${dd}T00:00:00`;
  return `${yyyy}-${mm}-${dd}T${hh}:${mi}:${ss}`;
}

export function IzinlerimScreen(props: { user: any; onOpenMenu?: () => void }) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [data, setData] = useState<IzinlerimResponse | null>(null);
  const [tipler, setTipler] = useState<any[]>([]);
  const [tipModal, setTipModal] = useState(false);
  const [izinTipId, setIzinTipId] = useState<number | null>(null);
  const [saatlik, setSaatlik] = useState(false);
  const lastChangedRef = useRef<"tip" | "saatlik" | null>(null);
  const syncingRef = useRef(false);
  const [baslangicDt, setBaslangicDt] = useState<Date | null>(null);
  const [bitisDt, setBitisDt] = useState<Date | null>(null);
  const [pickerVisible, setPickerVisible] = useState(false);
  const [pickerField, setPickerField] = useState<"baslangic" | "bitis">("baslangic");
  const [pickerTemp, setPickerTemp] = useState<Date>(new Date());
  const [izinAdres, setIzinAdres] = useState("");
  const [telefonNo, setTelefonNo] = useState("");
  const [aciklama, setAciklama] = useState("");
  const [creating, setCreating] = useState(false);
  const [pdfVisible, setPdfVisible] = useState(false);
  const [pdfUri, setPdfUri] = useState<string | null>(null);
  const [pdfTitle, setPdfTitle] = useState<string>("PDF");
  const [popupVisible, setPopupVisible] = useState(false);
  const [popupType, setPopupType] = useState<"success" | "error">("success");
  const [popupMessage, setPopupMessage] = useState("");
  const showPopup = (type: "success" | "error", message: string) => {
    setPopupType(type);
    setPopupMessage(message);
    setPopupVisible(true);
  };
  const [taleplerPage, setTaleplerPage] = useState(1);
  const [gecmisPage, setGecmisPage] = useState(1);
  const taleplerPageSize = 3;
  const gecmisPageSize = 3;

  const subtitle = useMemo(() => {
    return (props.user?.adSoyad ?? props.user?.AdSoyad ?? "").toString();
  }, [props.user]);

  const extractPhone = (u: any) => {
    const raw =
      u?.telefonNo ??
      u?.TelefonNo ??
      u?.telefon ??
      u?.Telefon ??
      u?.gsm ??
      u?.Gsm ??
      u?.gsmNo ??
      u?.GsmNo ??
      u?.cepTelefonu ??
      u?.CepTelefonu ??
      u?.cepTel ??
      u?.CepTel ??
      u?.CepTelNo ??
      u?.CepTelNо ?? // defensive for stray unicode
      u?.CepTelno ??
      u?.CepTelNO ??
      u?.CepTel2 ??
      u?.CepTel1 ??
      u?.CepTel0 ??
      u?.CepTel3 ??
      u?.CepTel4 ??
      u?.CepTel5 ??
      u?.CepTel6 ??
      u?.CepTel7 ??
      u?.CepTel8 ??
      u?.CepTel9 ??
      u?.CepTel_ ??
      u?.CepTelX ??
      u?.CepTelY ??
      u?.CepTelZ ??
      u?.CepTelW ??
      u?.CepTelV ??
      u?.CepTelU ??
      u?.CepTelT ??
      u?.CepTelS ??
      u?.CepTelR ??
      u?.CepTelQ ??
      u?.CepTelP ??
      u?.CepTelO ??
      u?.CepTelN ??
      u?.CepTelM ??
      u?.CepTelL ??
      u?.CepTelK ??
      u?.CepTelJ ??
      u?.CepTelI ??
      u?.CepTelH ??
      u?.CepTelG ??
      u?.CepTelF ??
      u?.CepTelE ??
      u?.CepTelD ??
      u?.CepTelC ??
      u?.CepTelB ??
      u?.CepTelA ??
      u?.CepTelx ??
      u?.CepTely ??
      u?.CepTelz ??
      u?.CepTelw ??
      u?.CepTelv ??
      u?.CepTelu ??
      u?.CepTelt ??
      u?.CepTels ??
      u?.CepTelr ??
      u?.CepTelq ??
      u?.CepTelp ??
      u?.CepTelo ??
      u?.CepTeln ??
      u?.CepTelm ??
      u?.CepTell ??
      u?.CepTelk ??
      u?.CepTelj ??
      u?.CepTeli ??
      u?.CepTelh ??
      u?.CepTelg ??
      u?.CepTelf ??
      u?.CepTele ??
      u?.CepTeld ??
      u?.CepTelc ??
      u?.CepTelb ??
      u?.CepTela ??
      u?.personel?.telefonNo ??
      u?.personel?.TelefonNo ??
      u?.personel?.cepTelefonu ??
      u?.personel?.CepTelefonu ??
      u?.personel?.cepTel ??
      u?.personel?.CepTel ??
      u?.personel?.CepTelNo ??
      u?.personel?.gsm ??
      u?.personel?.Gsm ??
      null;
    const s = (raw ?? "").toString().trim();
    return s || "";
  };

  const autoPhone = useMemo(() => {
    return extractPhone(props.user);
  }, [props.user]);

  useEffect(() => {
    // If phone exists in system, prefill once; user can still edit.
    if (!telefonNo && autoPhone) setTelefonNo(autoPhone);
  }, [autoPhone]); // intentionally not depending on telefonNo to avoid re-setting while typing

  useEffect(() => {
    // If not present on user payload, fetch profile once and try again.
    if (telefonNo) return;
    let alive = true;
    (async () => {
      try {
        const r = await profilService.getProfil();
        if (!alive) return;
        if (!r?.success) return;
        const p = r.data ?? ({} as any);
        const phone = extractPhone(p) || extractPhone(p?.personel);
        if (phone) setTelefonNo(phone);
      } catch {}
    })();
    return () => {
      alive = false;
    };
  }, []);

  async function load() {
    try {
      setLoading(true);
      setError(null);
      const res = await profilService.izinlerim();
      if (!res?.success) {
        setError(res?.message || "İzinlerim alınamadı.");
        setData(null);
      } else {
        setData(res.data ?? { talepler: [], onayliIzinler: [] });
      }
    } catch (e: any) {
      setError(e?.message || "Sunucuya bağlanılamadı.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
    (async () => {
      try {
        const r = await profilService.izinTipleri();
        if (r?.success) setTipler(r.data ?? []);
      } catch { }
    })();
  }, []);

  const talepler = data?.talepler ?? [];
  const onayli = data?.onayliIzinler ?? [];
  const selectedTip = tipler.find((x) => (x?.izinTipId ?? x?.IzinTipId) === izinTipId);
  const selectedTipAd = (selectedTip?.ad ?? selectedTip?.Ad ?? "").toString();

  useEffect(() => {
    // Web-like rules without trapping user:
    // - If user turns on "Saatlik", force IzinTipId=7 (Mazeret)
    // - If user picks Mazeret (7), force Saatlik=true
    // - If user picks a different izin tipi, allow it by forcing Saatlik=false
    if (syncingRef.current) return;

    const last = lastChangedRef.current;
    try {
      syncingRef.current = true;

      if (last === "saatlik") {
        if (saatlik && izinTipId !== 7) setIzinTipId(7);
        // If user toggles off saatlik, don't keep Mazeret forced-selected.
        if (!saatlik && izinTipId === 7) setIzinTipId(null);
        return;
      }

      if (last === "tip") {
        if (izinTipId === 7 && !saatlik) setSaatlik(true);
        if (izinTipId !== null && izinTipId !== 7 && saatlik) setSaatlik(false);
        return;
      }

      // initial auto-fix (no lastChanged yet)
      if (izinTipId === 7 && !saatlik) setSaatlik(true);
      if (saatlik && izinTipId !== 7) setIzinTipId(7);
    } finally {
      syncingRef.current = false;
    }
  }, [saatlik, izinTipId]);

  useEffect(() => {
    setTaleplerPage(1);
    setGecmisPage(1);
  }, [talepler.length, onayli.length]);

  const taleplerTotalPages = Math.max(1, Math.ceil(talepler.length / taleplerPageSize));
  const gecmisTotalPages = Math.max(1, Math.ceil(onayli.length / gecmisPageSize));
  const taleplerPaged = talepler.slice((taleplerPage - 1) * taleplerPageSize, taleplerPage * taleplerPageSize);
  const gecmisPaged = onayli.slice((gecmisPage - 1) * gecmisPageSize, gecmisPage * gecmisPageSize);

  function openPicker(field: "baslangic" | "bitis") {
    setPickerField(field);
    const v = field === "baslangic" ? baslangicDt : bitisDt;
    setPickerTemp(v ?? normalizeDateOnly(new Date()));
    setPickerVisible(true);
  }

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} useModal={false} autoCloseMs={1500} />
      <PersonelHeader title="İzinlerim" subtitle={subtitle} onOpenMenu={props.onOpenMenu} />

      <ScrollView className="flex-1 px-5 py-5" contentContainerStyle={{ paddingBottom: 24 }}>
        <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4 mb-4">
          <Text className="text-[#1e293b] font-extrabold text-[14px]">Yeni İzin Talebi</Text>

          <TouchableOpacity
            onPress={() => setTipModal(true)}
            className="mt-3 border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc] flex-row items-center justify-between"
          >
            <Text className="text-[#1e293b] font-semibold" numberOfLines={1}>
              {izinTipId ? `${selectedTipAd || `#${izinTipId}`}` : "İzin Tipi Seçiniz"}
            </Text>
            <MaterialCommunityIcons name="chevron-down" size={22} color="#64748b" />
          </TouchableOpacity>

          <View className="mt-3 flex-row items-center justify-between">
            <Text className="text-[#64748b] font-semibold">Saatlik İzin mi?</Text>
            <TouchableOpacity
              onPress={() => {
                lastChangedRef.current = "saatlik";
                setSaatlik((v) => !v);
              }}
              className={`px-3 py-2 rounded-xl ${saatlik ? "bg-[#fee2e2]" : "bg-[#f1f5f9]"}`}
            >
              <Text className={`font-extrabold ${saatlik ? "text-[#b91c1c]" : "text-[#334155]"}`}>
                {saatlik ? "Evet" : "Hayır"}
              </Text>
            </TouchableOpacity>
          </View>

          <View className="mt-3">
            <Text className="text-[#64748b] font-semibold mb-2">Başlangıç</Text>
            <TouchableOpacity
              onPress={() => openPicker("baslangic")}
              className="border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc] flex-row items-center justify-between"
            >
              <Text className="text-[#1e293b] font-semibold">
                {baslangicDt ? fmtDateTime(baslangicDt, saatlik) : "Seçiniz"}
              </Text>
              <MaterialCommunityIcons name="calendar" size={20} color="#64748b" />
            </TouchableOpacity>
          </View>

          <View className="mt-3">
            <Text className="text-[#64748b] font-semibold mb-2">Bitiş</Text>
            <TouchableOpacity
              onPress={() => openPicker("bitis")}
              className="border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc] flex-row items-center justify-between"
            >
              <Text className="text-[#1e293b] font-semibold">
                {bitisDt ? fmtDateTime(bitisDt, saatlik) : "Seçiniz"}
              </Text>
              <MaterialCommunityIcons name="calendar" size={20} color="#64748b" />
            </TouchableOpacity>
          </View>

          <View className="mt-3">
            <Text className="text-[#64748b] font-semibold mb-2">Adres</Text>
            <TextInput
              value={izinAdres}
              onChangeText={setIzinAdres}
              placeholder="İzin sırasında bulunacağınız adres"
              placeholderTextColor="#94a3b8"
              multiline
              className="border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc] text-[#1e293b] font-semibold"
              style={{ minHeight: 80, textAlignVertical: "top" }}
            />
          </View>

          <View className="mt-3">
            <Text className="text-[#64748b] font-semibold mb-2">Telefon</Text>
            <TextInput
              value={telefonNo}
              onChangeText={setTelefonNo}
              placeholder="Telefon"
              placeholderTextColor="#94a3b8"
              className="border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc] text-[#1e293b] font-semibold"
            />
          </View>

          <View className="mt-3">
            <Text className="text-[#64748b] font-semibold mb-2">Açıklama / Mazeret</Text>
            <TextInput
              value={aciklama}
              onChangeText={setAciklama}
              placeholder="Açıklama"
              placeholderTextColor="#94a3b8"
              multiline
              className="border border-[#e2e8f0] rounded-xl px-4 py-3 bg-[#f8fafc] text-[#1e293b] font-semibold"
              style={{ minHeight: 80, textAlignVertical: "top" }}
            />
          </View>

          <TouchableOpacity
            disabled={creating}
            onPress={async () => {
              if (!izinTipId) {
                setError("İzin tipi seçiniz.");
                return;
              }
              if (!baslangicDt || !bitisDt) {
                setError("Başlangıç ve bitiş seçiniz.");
                return;
              }
              try {
                setCreating(true);
                setError(null);
                const r = await profilService.izinTalep({
                  izinTipId,
                  baslangic: toLocalIsoLike(baslangicDt, saatlik),
                  bitis: toLocalIsoLike(bitisDt, saatlik),
                  saatlikIzinMi: saatlik,
                  aciklama: aciklama || undefined,
                  izinAdres: izinAdres || undefined,
                  telefonNo: telefonNo || undefined,
                });
                if (!r?.success) {
                  const msg = r?.message || "Talep oluşturulamadı.";
                  setError(msg);
                  showPopup("error", msg);
                } else {
                  showPopup("success", r?.message || "Talep oluşturuldu.");
                  setBaslangicDt(null);
                  setBitisDt(null);
                  setIzinAdres("");
                  setTelefonNo("");
                  setAciklama("");
                  await load();
                }
              } catch (e: any) {
                const msg = e?.message || "Sunucuya bağlanılamadı.";
                setError(msg);
                showPopup("error", msg);
              } finally {
                setCreating(false);
              }
            }}
            className={`mt-4 rounded-xl px-4 py-3 flex-row items-center justify-center ${creating ? "bg-[#f1f5f9]" : "bg-[#dc2626]"}`}
          >
            <Text className={`font-extrabold ${creating ? "text-[#64748b]" : "text-white"}`}>
              {creating ? "Gönderiliyor..." : "Talep Oluştur"}
            </Text>
          </TouchableOpacity>
        </View>

        {loading ? (
          <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4">
            <Text className="text-[#64748b] font-semibold">Yükleniyor...</Text>
          </View>
        ) : error ? (
          <View className="bg-white rounded-2xl border border-[#fee2e2] p-4">
            <Text className="text-[#b91c1c] font-extrabold">Hata</Text>
            <Text className="text-[#7f1d1d] font-semibold mt-1">{error}</Text>
            <TouchableOpacity onPress={load} className="mt-4 bg-[#fee2e2] rounded-xl px-4 py-3 self-start">
              <Text className="text-[#b91c1c] font-extrabold">Tekrar Dene</Text>
            </TouchableOpacity>
          </View>
        ) : (
          <>
            <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4">
              <View className="flex-row items-center justify-between">
                <Text className="text-[#1e293b] font-extrabold text-[14px]">Taleplerim</Text>
                <View className="flex-row items-center">
                  <MaterialCommunityIcons name="clipboard-text" size={16} color="#64748b" />
                  <Text className="ml-1 text-[#64748b] font-extrabold">{talepler.length}</Text>
                </View>
              </View>
              {talepler.length ? (
                <View className="mt-3">
                  {taleplerPaged.map((t: any, idx: number) => (
                    <View key={`${t?.TalepId ?? t?.talepId ?? idx}`} className="border-t border-[#f1f5f9] py-3">
                      <Text className="text-[#1e293b] font-extrabold" numberOfLines={1}>
                        {fmtDate(t?.Baslangic ?? t?.baslangic)} - {fmtDate(t?.Bitis ?? t?.bitis)}
                      </Text>
                      <Text className="text-[#64748b] font-semibold mt-1" numberOfLines={2}>
                        {t?.Aciklama ?? t?.aciklama ?? " "}
                      </Text>
                      <View className="mt-2 flex-row items-center">
                        <View className="px-2 py-1 rounded-lg bg-[#f1f5f9]">
                          <Text className="text-[#334155] font-extrabold text-[11px]">
                            {(t?.UstYetkiliOnayDurumu ?? t?.ustYetkiliOnayDurumu ?? "").toString() || " "}
                          </Text>
                        </View>
                        {t?.SaatlikIzinMi ?? t?.saatlikIzinMi ? (
                          <View className="ml-2 px-2 py-1 rounded-lg bg-[#fee2e2]">
                            <Text className="text-[#b91c1c] font-extrabold text-[11px]">Saatlik</Text>
                          </View>
                        ) : null}
                        {t?.KullanimImzaIstenen ?? t?.kullanimImzaIstenen ? (
                          <View className="ml-2 px-2 py-1 rounded-lg bg-[#fef9c3]">
                            <Text className="text-[#854d0e] font-extrabold text-[11px]">
                              {t?.KullanimImzaTarihi || t?.kullanimImzaTarihi ? "İmzalandı" : "Dönüş İmzası Bekliyor"}
                            </Text>
                          </View>
                        ) : null}
                      </View>

                      {(t?.KullanimImzaIstenen ?? t?.kullanimImzaIstenen) && !(t?.KullanimImzaTarihi ?? t?.kullanimImzaTarihi) ? (
                        <TouchableOpacity
                          onPress={async () => {
                            try {
                              setError(null);
                              const r = await profilService.kullanimImzaAt(t?.TalepId ?? t?.talepId);
                              if (!r?.success) setError(r?.message || "İmza kaydedilemedi.");
                              else await load();
                            } catch (e: any) {
                              setError(e?.message || "Sunucuya bağlanılamadı.");
                            }
                          }}
                          className="mt-3 bg-[#dc2626] rounded-xl px-4 py-3 self-start"
                        >
                          <Text className="text-white font-extrabold">Dönüş İmzası At</Text>
                        </TouchableOpacity>
                      ) : null}

                      <View className="mt-3 flex-row flex-wrap">
                        <TouchableOpacity
                          onPress={async () => {
                            try {
                              const id = t?.TalepId ?? t?.talepId;
                              if (!id) return;
                              setPdfTitle(`İzin Talep #${id}`);
                              const uri = await downloadPdfFromApi(`/IzinKagit/pdf/${id}`, `IzinTalep_${id}.pdf`);
                              setPdfUri(uri);
                              setPdfVisible(true);
                            } catch (e: any) {
                              setError(e?.message || "PDF açılamadı.");
                            }
                          }}
                          className="mr-2 mb-2 bg-[#f1f5f9] rounded-xl px-4 py-3 flex-row items-center"
                        >
                          <MaterialCommunityIcons name="file-pdf-box" size={18} color="#334155" />
                          <Text className="ml-2 text-[#334155] font-extrabold">PDF</Text>
                        </TouchableOpacity>
                      </View>
                    </View>
                  ))}
                  <View className="mt-3 flex-row items-center justify-between">
                    <Text className="text-[#64748b] font-semibold text-[12px]">
                      Toplam {talepler.length} • Sayfa {taleplerPage}/{taleplerTotalPages}
                    </Text>
                    <View className="flex-row">
                      <TouchableOpacity
                        disabled={taleplerPage <= 1}
                        onPress={() => setTaleplerPage((p) => Math.max(1, p - 1))}
                        className={`px-3 py-2 rounded-xl mr-2 ${taleplerPage <= 1 ? "bg-[#f8fafc]" : "bg-[#f1f5f9]"}`}
                      >
                        <Text className={`font-extrabold ${taleplerPage <= 1 ? "text-[#94a3b8]" : "text-[#334155]"}`}>Önceki</Text>
                      </TouchableOpacity>
                      <TouchableOpacity
                        disabled={taleplerPage >= taleplerTotalPages}
                        onPress={() => setTaleplerPage((p) => Math.min(taleplerTotalPages, p + 1))}
                        className={`px-3 py-2 rounded-xl ${taleplerPage >= taleplerTotalPages ? "bg-[#f8fafc]" : "bg-[#f1f5f9]"}`}
                      >
                        <Text className={`font-extrabold ${taleplerPage >= taleplerTotalPages ? "text-[#94a3b8]" : "text-[#334155]"}`}>Sonraki</Text>
                      </TouchableOpacity>
                    </View>
                  </View>
                </View>
              ) : (
                <Text className="mt-3 text-[#64748b] font-semibold">Talepler bulunamadı.</Text>
              )}
            </View>

            <View className="mt-4 bg-white rounded-2xl border border-[#f1f5f9] p-4">
              <View className="flex-row items-center justify-between">
                <Text className="text-[#1e293b] font-extrabold text-[14px]">Onaylı İzin Geçmişi</Text>
                <View className="flex-row items-center">
                  <MaterialCommunityIcons name="calendar-check" size={16} color="#64748b" />
                  <Text className="ml-1 text-[#64748b] font-extrabold">{onayli.length}</Text>
                </View>
              </View>
              {onayli.length ? (
                <View className="mt-3">
                  {gecmisPaged.map((r: any, idx: number) => (
                    <View key={`${r?.kisiIzinId ?? r?.KisiIzinId ?? idx}`} className="border-t border-[#f1f5f9] py-3">
                      <Text className="text-[#1e293b] font-extrabold" numberOfLines={1}>
                        {fmtDate(r?.baslangic ?? r?.Baslangic)} - {fmtDate(r?.bitis ?? r?.Bitis)}
                      </Text>
                      <Text className="text-[#64748b] font-semibold mt-1" numberOfLines={2}>
                        {r?.aciklama ?? r?.Aciklama ?? " "}
                      </Text>
                      <View className="mt-2 flex-row items-center justify-between">
                        <Text className="text-[#64748b] font-semibold text-[12px]">
                          Süre: {(r?.sureSaat ?? r?.SureSaat ?? 0).toString()} saat
                        </Text>
                        {r?.saatlikIzinMi ?? r?.SaatlikIzinMi ? (
                          <Text className="text-[#b91c1c] font-extrabold text-[12px]">Saatlik</Text>
                        ) : null}
                      </View>

                      <View className="mt-3 flex-row flex-wrap">
                        <TouchableOpacity
                          onPress={async () => {
                            try {
                              const id = r?.kisiIzinId ?? r?.KisiIzinId;
                              if (!id) return;
                              setPdfTitle(`İzin #${id}`);
                              const uri = await downloadPdfFromApi(`/IzinKagit/pdf-from-izin/${id}`, `Izin_${id}.pdf`);
                              setPdfUri(uri);
                              setPdfVisible(true);
                            } catch (e: any) {
                              setError(e?.message || "PDF açılamadı.");
                            }
                          }}
                          className="mr-2 mb-2 bg-[#f1f5f9] rounded-xl px-4 py-3 flex-row items-center"
                        >
                          <MaterialCommunityIcons name="file-pdf-box" size={18} color="#334155" />
                          <Text className="ml-2 text-[#334155] font-extrabold">PDF</Text>
                        </TouchableOpacity>
                      </View>
                    </View>
                  ))}
                  <View className="mt-3 flex-row items-center justify-between">
                    <Text className="text-[#64748b] font-semibold text-[12px]">
                      Toplam {onayli.length} • Sayfa {gecmisPage}/{gecmisTotalPages}
                    </Text>
                    <View className="flex-row">
                      <TouchableOpacity
                        disabled={gecmisPage <= 1}
                        onPress={() => setGecmisPage((p) => Math.max(1, p - 1))}
                        className={`px-3 py-2 rounded-xl mr-2 ${gecmisPage <= 1 ? "bg-[#f8fafc]" : "bg-[#f1f5f9]"}`}
                      >
                        <Text className={`font-extrabold ${gecmisPage <= 1 ? "text-[#94a3b8]" : "text-[#334155]"}`}>Önceki</Text>
                      </TouchableOpacity>
                      <TouchableOpacity
                        disabled={gecmisPage >= gecmisTotalPages}
                        onPress={() => setGecmisPage((p) => Math.min(gecmisTotalPages, p + 1))}
                        className={`px-3 py-2 rounded-xl ${gecmisPage >= gecmisTotalPages ? "bg-[#f8fafc]" : "bg-[#f1f5f9]"}`}
                      >
                        <Text className={`font-extrabold ${gecmisPage >= gecmisTotalPages ? "text-[#94a3b8]" : "text-[#334155]"}`}>Sonraki</Text>
                      </TouchableOpacity>
                    </View>
                  </View>
                </View>
              ) : (
                <Text className="mt-3 text-[#64748b] font-semibold">Kayıt bulunamadı.</Text>
              )}
            </View>
          </>
        )}
      </ScrollView>

      <Modal transparent visible={pickerVisible} animationType="fade" onRequestClose={() => setPickerVisible(false)}>
        <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setPickerVisible(false)}>
          <TouchableOpacity activeOpacity={1} onPress={() => {}}>
            <View className="bg-white rounded-2xl p-4">
              <Text className="text-[#1e293b] font-extrabold text-[14px] mb-3">
                {pickerField === "baslangic" ? "Başlangıç Seç" : "Bitiş Seç"}
              </Text>

              <View style={{ height: Platform.OS === "ios" ? (saatlik ? 216 : 360) : 216, width: "100%", justifyContent: "center", backgroundColor: "white" }}>
                <DateTimePicker
                  value={pickerTemp}
                  mode={saatlik ? "datetime" : "date"}
                  display={Platform.OS === "ios" ? (saatlik ? "spinner" : "inline") : "default"}
                  locale="tr-TR"
                  themeVariant="light"
                  textColor="#0f172a"
                  style={{ width: "100%", height: Platform.OS === "ios" ? (saatlik ? 216 : 360) : 216 }}
                  onChange={(_, d) => {
                    if (!d) return;
                    setPickerTemp(saatlik ? d : normalizeDateOnly(d));
                  }}
                />
              </View>

              <View className="mt-4 flex-row">
                <TouchableOpacity onPress={() => setPickerVisible(false)} className="flex-1 bg-[#f1f5f9] rounded-xl px-4 py-3 mr-2">
                  <Text className="text-[#334155] font-extrabold text-center">Vazgeç</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  onPress={() => {
                    if (pickerField === "baslangic") {
                      const picked = saatlik ? pickerTemp : normalizeDateOnly(pickerTemp);
                      setBaslangicDt(picked);
                      // keep bitiş >= başlangıç (basic safety)
                      if (bitisDt && bitisDt.getTime() < picked.getTime()) setBitisDt(picked);
                    } else {
                      const picked = saatlik ? pickerTemp : normalizeDateOnly(pickerTemp);
                      setBitisDt(picked);
                      if (baslangicDt && picked.getTime() < baslangicDt.getTime()) setBaslangicDt(picked);
                    }
                    setPickerVisible(false);
                  }}
                  className="flex-1 bg-[#dc2626] rounded-xl px-4 py-3"
                >
                  <Text className="text-white font-extrabold text-center">Seç</Text>
                </TouchableOpacity>
              </View>
            </View>
          </TouchableOpacity>
        </TouchableOpacity>
      </Modal>

      <Modal transparent visible={tipModal} animationType="fade" onRequestClose={() => setTipModal(false)}>
        <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setTipModal(false)}>
          <TouchableOpacity activeOpacity={1} onPress={() => {}}>
            <View className="bg-white rounded-2xl p-4 max-h-[70%]">
              <Text className="text-[#1e293b] font-extrabold text-[14px] mb-3">İzin Tipi Seç</Text>
              <ScrollView>
                {tipler.map((it) => {
                  const id = it?.izinTipId ?? it?.IzinTipId;
                  const ad = (it?.ad ?? it?.Ad ?? "").toString() || `#${id}`;
                  const active = id === izinTipId;
                  return (
                    <TouchableOpacity
                      key={id}
                      onPress={() => {
                        lastChangedRef.current = "tip";
                        setIzinTipId(id);
                        setTipModal(false);
                      }}
                      className={`px-3 py-3 rounded-xl mb-2 ${active ? "bg-[#fee2e2]" : "bg-[#f8fafc]"}`}
                    >
                      <Text className={`font-extrabold ${active ? "text-[#b91c1c]" : "text-[#1e293b]"}`}>{ad}</Text>
                    </TouchableOpacity>
                  );
                })}
              </ScrollView>
              <TouchableOpacity onPress={() => setTipModal(false)} className="mt-3 bg-[#f1f5f9] rounded-xl px-4 py-3">
                <Text className="text-[#334155] font-extrabold text-center">Kapat</Text>
              </TouchableOpacity>
            </View>
          </TouchableOpacity>
        </TouchableOpacity>
      </Modal>

      <PdfPreviewModal
        visible={pdfVisible}
        title={pdfTitle}
        fileUri={pdfUri}
        onClose={() => setPdfVisible(false)}
      />
    </View>
  );
}

