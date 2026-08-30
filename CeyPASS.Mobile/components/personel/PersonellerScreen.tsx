import React, { useEffect, useMemo, useState } from "react";
import { ActivityIndicator, Image, Modal, Platform, ScrollView, Switch, Text, TextInput, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import DateTimePicker from "@react-native-community/datetimepicker";
import * as ImagePicker from "expo-image-picker";
import { PersonelHeader } from "./PersonelHeader";
import { personelService } from "../../services/personelApi";
import { StatusPopup } from "../StatusPopup";
import { BusyOverlay } from "../BusyOverlay";
import { pageFilterPrefs } from "../../services/pageFilterPrefs";
type LookupItem = { Id?: number; id?: number; Ad?: string; ad?: string };
type FirmaItem = { FirmaId?: number; firmaId?: number; FirmaAdi?: string; firmaAdi?: string };
type CalismaSekliItem = { Id?: number; id?: number; Ad?: string; ad?: string };
type AktifFirma = { FirmaId?: number; firmaId?: number; FirmaAdi?: string; firmaAdi?: string };

function s(v: any): string {
  return (v ?? "").toString();
}
function pickId(x: any): number {
  return Number(x?.Id ?? x?.id ?? 0) || 0;
}
function pickAd(x: any): string {
  return s(x?.Ad ?? x?.ad ?? "");
}
function pickFirmaId(x: any): number {
  return Number(x?.FirmaId ?? x?.firmaId ?? 0) || 0;
}
function pickFirmaAdi(x: any): string {
  return s(x?.FirmaAdi ?? x?.firmaAdi ?? "");
}
function pick(obj: any, ...keys: string[]) {
  for (const k of keys) {
    const v = obj?.[k as any];
    if (v !== undefined && v !== null && s(v) !== "") return v;
  }
  return undefined;
}
function parseCsv(v: any): number[] {
  const txt = s(v);
  if (!txt) return [];
  return txt
    .split(",")
    .map((x) => Number(x.trim()))
    .filter((x) => Number.isFinite(x) && x > 0);
}
function joinCsv(ids: number[]) {
  return ids.filter((x) => Number.isFinite(x) && x > 0).join(",");
}
function uniqueById<T>(items: T[], getId: (x: T) => number): T[] {
  const seen = new Set<number>();
  const out: T[] = [];
  for (const it of items) {
    const id = getId(it);
    if (!id || seen.has(id)) continue;
    seen.add(id);
    out.push(it);
  }
  return out;
}
function normalizeDateOnly(d: Date) {
  // Force local-noon to avoid timezone/DST day shifts for date-only fields.
  return new Date(d.getFullYear(), d.getMonth(), d.getDate(), 12, 0, 0, 0);
}
function fmtDateTR(v: any) {
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
  if (Number.isNaN(d.getTime())) return "";
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const yyyy = d.getFullYear();
  return `${dd}.${mm}.${yyyy}`;
}
function fmtIsoDate(d: Date) {
  const yyyy = d.getFullYear();
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const dd = String(d.getDate()).padStart(2, "0");
  return `${yyyy}-${mm}-${dd}`;
}
function toDateOrNull(v: any): Date | null {
  if (!v) return null;
  if (typeof v === "string") {
    const s = v.trim();
    const datePart = s.length >= 10 ? s.slice(0, 10) : s;
    if (/^\d{4}-\d{2}-\d{2}$/.test(datePart)) {
      const [y, m, d] = datePart.split("-").map((x) => Number(x));
    if (!y || !m || !d) return null;
    return normalizeDateOnly(new Date(y, m - 1, d));
    }
  }
  const d = new Date(v);
  return Number.isNaN(d.getTime()) ? null : normalizeDateOnly(d);
}
function apiErrorMessage(e: any) {
  const data = e?.response?.data;
  const validationErrors = data?.errors;
  if (validationErrors && typeof validationErrors === "object") {
    try {
      const parts: string[] = [];
      for (const k of Object.keys(validationErrors)) {
        const arr = validationErrors[k];
        if (Array.isArray(arr)) for (const msg of arr) parts.push(`${k}: ${String(msg)}`);
      }
      if (parts.length) return `Validasyon hatası: ${parts.join(" • ")}`;
    } catch {}
  }
  return (
    data?.message ??
    data?.Message ??
    data?.detail ??
    data?.Detail ??
    data?.title ??
    data?.Title ??
    (typeof data === "string" ? data : undefined) ??
    e?.message ??
    "Sunucuya bağlanılamadı."
  );
}

function SelectModal(props: {
  visible: boolean;
  title: string;
  items: { key: string; label: string }[];
  onClose: () => void;
  onSelect: (key: string) => void;
}) {
  return (
    <Modal transparent visible={props.visible} animationType="fade" onRequestClose={props.onClose}>
      <View className="flex-1 bg-black/60 items-center justify-center px-6">
        <View className="w-full rounded-3xl bg-white p-5 max-h-[75%]">
          <View className="flex-row items-center justify-between mb-3">
            <Text className="text-[15px] font-extrabold text-[#1e293b]">{props.title}</Text>
            <TouchableOpacity onPress={props.onClose} className="p-2">
              <MaterialCommunityIcons name="close" size={20} color="#64748b" />
            </TouchableOpacity>
          </View>
          <ScrollView>
            {props.items.map((it, idx) => (
              <TouchableOpacity key={`${it.key}_${idx}`} onPress={() => props.onSelect(it.key)} className="py-4 border-b border-[#f1f5f9]">
                <Text className="font-semibold text-[#1e293b]">{it.label}</Text>
              </TouchableOpacity>
            ))}
          </ScrollView>
        </View>
      </View>
    </Modal>
  );
}

function MultiSelectModal(props: {
  visible: boolean;
  title: string;
  items: { id: number; label: string }[];
  selected: number[];
  onClose: () => void;
  onChange: (next: number[]) => void;
}) {
  const set = useMemo(() => new Set(props.selected), [props.selected]);
  return (
    <Modal transparent visible={props.visible} animationType="fade" onRequestClose={props.onClose}>
      <View className="flex-1 bg-black/60 items-center justify-center px-6">
        <View className="w-full rounded-3xl bg-white p-5 max-h-[80%]">
          <View className="flex-row items-center justify-between mb-3">
            <Text className="text-[15px] font-extrabold text-[#1e293b]">{props.title}</Text>
            <TouchableOpacity onPress={props.onClose} className="p-2">
              <MaterialCommunityIcons name="close" size={20} color="#64748b" />
            </TouchableOpacity>
          </View>
          <ScrollView>
            {props.items.map((it) => {
              const on = set.has(it.id);
              return (
                <TouchableOpacity
                  key={`${it.id}_${it.label}`}
                  onPress={() => {
                    const next = new Set(set);
                    if (on) next.delete(it.id);
                    else next.add(it.id);
                    props.onChange(Array.from(next));
                  }}
                  className="py-4 border-b border-[#f1f5f9] flex-row items-center"
                >
                  <MaterialCommunityIcons name={on ? "checkbox-marked" : "checkbox-blank-outline"} size={20} color={on ? "#dc2626" : "#94a3b8"} />
                  <Text className="ml-3 font-semibold text-[#1e293b]">{it.label}</Text>
                </TouchableOpacity>
              );
            })}
          </ScrollView>
          <TouchableOpacity onPress={props.onClose} className="mt-4 bg-[#dc2626] rounded-2xl py-3 items-center">
            <Text className="text-white font-extrabold">Tamam</Text>
          </TouchableOpacity>
        </View>
      </View>
    </Modal>
  );
}

function PersonelFormModal(props: {
  visible: boolean;
  title: string;
  lookups: any;
  initial?: any;
  errorText?: string | null;
  onClose: () => void;
  onSubmit: (payload: any) => Promise<void>;
}) {
  const [kisi, setKisi] = useState<any>({});
  const [firmaPersoneli, setFirmaPersoneli] = useState(true);
  const [puantajYapilabilir, setPuantajYapilabilir] = useState(true);
  const [yemekHakkiVar, setYemekHakkiVar] = useState(false);
  const [gunlukYemek, setGunlukYemek] = useState("");
  const [firmaDisiKartNo, setFirmaDisiKartNo] = useState("");
  const [ziyaretciMi, setZiyaretciMi] = useState(false);
  const [aracKartiMi, setAracKartiMi] = useState(false);
  const [taseronCalisanMi, setTaseronCalisanMi] = useState(false);
  const [vardiyaModal, setVardiyaModal] = useState(false);
  const [vardiyaIds, setVardiyaIds] = useState<number[]>([]);
  const [fotoBase64, setFotoBase64] = useState<string | null>(null);
  const [fotoDegisti, setFotoDegisti] = useState(false);
  const [dogumDate, setDogumDate] = useState<Date | null>(null);
  const [iseDate, setIseDate] = useState<Date>(new Date());
  const [pickerKind, setPickerKind] = useState<null | "dogum" | "ise">(null);
  const [fieldErrors, setFieldErrors] = useState<{ PersonelId?: string; Ad?: string; Soyad?: string }>({});

  const [localLookups, setLocalLookups] = useState<any>(props.lookups);
  useEffect(() => {
    setLocalLookups(props.lookups);
  }, [props.lookups]);

  const isyerleri: LookupItem[] = (localLookups?.Isyerleri ?? localLookups?.isyerleri ?? []) as any;
  const departmanlar: LookupItem[] = (localLookups?.Departmanlar ?? localLookups?.departmanlar ?? []) as any;
  const pozisyonlar: LookupItem[] = (localLookups?.Pozisyonlar ?? localLookups?.pozisyonlar ?? []) as any;
  const bolumler: LookupItem[] = (localLookups?.Bolumler ?? localLookups?.bolumler ?? []) as any;
  const statuler: LookupItem[] = (localLookups?.CalismaStatuleri ?? localLookups?.calismaStatuleri ?? []) as any;
  const vardiyalar: CalismaSekliItem[] = (localLookups?.CalismaSekilleri ?? localLookups?.calismaSekilleri ?? []) as any;
  const firmalar: FirmaItem[] = (localLookups?.Firmalar ?? localLookups?.firmalar ?? []) as any;
  const aktifFirma: any = localLookups?.AktifFirma ?? localLookups?.aktifFirma ?? null;

  const [selectModal, setSelectModal] = useState<{ kind: string; visible: boolean }>({ kind: "", visible: false });

  useEffect(() => {
    if (!props.visible) return;
    const init = props.initial ?? {};
    const nextKisi = {
      PersonelId: pick(init, "PersonelId", "personelId") ?? "",
      Ad: pick(init, "Ad", "ad") ?? "",
      Soyad: pick(init, "Soyad", "soyad") ?? "",
      TcKimlikNo: pick(init, "TcKimlikNo", "tcKimlikNo") ?? "",
      KartNo: pick(init, "KartNo", "kartNo") ?? "",
      CepTel: pick(init, "CepTel", "cepTel") ?? "",
      Email: pick(init, "Email", "email") ?? "",
      FirmaId: pick(init, "FirmaId", "firmaId") ?? 0,
      IsyeriId: pick(init, "IsyeriId", "isyeriId") ?? null,
      DepartmanId: pick(init, "DepartmanId", "departmanId") ?? null,
      PozisyonId: pick(init, "PozisyonId", "pozisyonId") ?? null,
      BolumId: pick(init, "BolumId", "bolumId") ?? null,
      CalismaStatusu: (() => {
        const id = pick(init, "CalismaStatusuId", "calismaStatusuId");
        return id ? String(id) : "";
      })(),
      CalismaStatusuText: pick(init, "CalismaStatusuText", "calismaStatusuText") ?? "",
      CalismaSekliCsv: pick(init, "CalismaSekliCsv", "calismaSekliCsv") ?? "",
      Fotograf: pick(init, "Fotograf", "fotograf") ?? "",
      TaseronKartNo: pick(init, "TaseronKartNo", "taseronKartNo") ?? "",
      ZiyaretciMi: !!pick(init, "ZiyaretciMi", "ziyaretciMi"),
      AracKartiMi: !!pick(init, "AracKartiMi", "aracKartiMi"),
      TaseronCalisanMi: !!pick(init, "TaseronCalisanMi", "taseronCalisanMi"),
    };
    // Defaults for create: use active firma and first çalışma statüsü (to match web required behavior)
    let firmaId = Number(nextKisi.FirmaId ?? 0) || 0;
    if (!firmaId) {
      const afId = Number(aktifFirma?.FirmaId ?? aktifFirma?.firmaId ?? 0) || 0;
      if (afId) firmaId = afId;
    }

    let calismaStatusu = s(nextKisi.CalismaStatusu);
    let calismaStatusuText = s(nextKisi.CalismaStatusuText);
    if (!calismaStatusu && statuler.length) {
      const first = statuler[0];
      const sid = pickId(first);
      const sad = pickAd(first);
      if (sid) calismaStatusu = String(sid);
      if (sad) calismaStatusuText = sad;
    }

    setKisi({
      ...nextKisi,
      FirmaId: firmaId,
      CalismaStatusu: calismaStatusu,
      CalismaStatusuText: calismaStatusuText,
    });
    setFirmaPersoneli(!!pick(init, "FirmaPersoneli", "firmaPersoneli", "FirmaPersoneli"));
    setPuantajYapilabilir(!!pick(init, "PuantajYapilabilir", "puantajYapilabilir"));
    setYemekHakkiVar(!!pick(init, "YemekHakkiVar", "yemekHakkiVar"));
    setGunlukYemek(s(pick(init, "GunlukYemekAdedi", "gunlukYemekAdedi") ?? ""));
    setFirmaDisiKartNo(s(pick(init, "TaseronKartNo", "taseronKartNo") ?? ""));
    setZiyaretciMi(!!pick(init, "ZiyaretciMi", "ziyaretciMi"));
    setAracKartiMi(!!pick(init, "AracKartiMi", "aracKartiMi"));
    setTaseronCalisanMi(!!pick(init, "TaseronCalisanMi", "taseronCalisanMi"));
    setVardiyaIds(parseCsv(pick(init, "CalismaSekliCsv", "calismaSekliCsv")));
    const existing = s(pick(init, "Fotograf", "fotograf") ?? "");
    setFotoBase64(existing ? existing : null);
    setFotoDegisti(false);
    setDogumDate(toDateOrNull(pick(init, "DogumTarihi", "dogumTarihi")));
    setIseDate(toDateOrNull(pick(init, "IseGirisTarihi", "iseGirisTarihi")) ?? new Date());
    setPickerKind(null);
    setFieldErrors({});
  }, [props.visible, props.initial, statuler.length, aktifFirma]);

  const vardiyaLabel = useMemo(() => {
    if (!vardiyaIds.length) return "Seçilmedi";
    const uniq = uniqueById(vardiyalar, (v) => pickId(v as any));
    const map = new Map(uniq.map((v) => [pickId(v as any), pickAd(v as any)]));
    const names = vardiyaIds.map((id) => map.get(id)).filter(Boolean);
    return names.slice(0, 3).join(", ") + (names.length > 3 ? ` (+${names.length - 3})` : "");
  }, [vardiyaIds, vardiyalar]);

  const selectItems = useMemo(() => {
    const kind = selectModal.kind;
    if (kind === "firma")
      return firmalar
        .map((f) => ({ key: String(pickFirmaId(f)), label: pickFirmaAdi(f) }))
        .filter((x) => x.key !== "0" && x.label);
    if (kind === "isyeri") return [{ key: "", label: "-- Seçiniz --" }, ...isyerleri.map((x) => ({ key: String(pickId(x)), label: pickAd(x) }))];
    if (kind === "departman") return [{ key: "", label: "-- Seçiniz --" }, ...departmanlar.map((x) => ({ key: String(pickId(x)), label: pickAd(x) }))];
    if (kind === "pozisyon") return [{ key: "", label: "-- Seçiniz --" }, ...pozisyonlar.map((x) => ({ key: String(pickId(x)), label: pickAd(x) }))];
    if (kind === "bolum") return [{ key: "", label: "-- Seçiniz --" }, ...bolumler.map((x) => ({ key: String(pickId(x)), label: pickAd(x) }))];
    // Persist DB value as id, show label as name
    if (kind === "statu") return [{ key: "", label: "-- Seçiniz --" }, ...statuler.map((x) => ({ key: String(pickId(x)), label: pickAd(x) })).filter((x) => x.key !== "0" && x.label)];
    return [];
  }, [selectModal.kind, firmalar, isyerleri, departmanlar, pozisyonlar, bolumler, statuler]);

  function currentLabelFor(kind: string) {
    if (kind === "firma") {
      const id = Number(kisi?.FirmaId ?? 0);
      const f = firmalar.find((x) => pickFirmaId(x) === id);
      return f ? pickFirmaAdi(f) : "-- Seçiniz --";
    }
    const get = (arr: LookupItem[], id: any) => {
      const nid = Number(id ?? 0);
      if (!nid) return "-- Seçiniz --";
      const it = arr.find((x) => pickId(x) === nid);
      return it ? pickAd(it) : "-- Seçiniz --";
    };
    if (kind === "isyeri") return get(isyerleri, kisi?.IsyeriId);
    if (kind === "departman") return get(departmanlar, kisi?.DepartmanId);
    if (kind === "pozisyon") return get(pozisyonlar, kisi?.PozisyonId);
    if (kind === "bolum") return get(bolumler, kisi?.BolumId);
    if (kind === "statu") return s(kisi?.CalismaStatusuText) || "-- Seçiniz --";
    return "-- Seçiniz --";
  }

  function applySelect(key: string) {
    const kind = selectModal.kind;
    if (kind === "firma") {
      const fid = Number(key) || 0;
      // Web behavior: firma changes -> reset dependent filters (isyeri, bolum) only
      setKisi((p: any) => ({ ...p, FirmaId: fid, IsyeriId: null, BolumId: null }));
      // Refresh lookups for the selected firma so labels never fall back to ids.
      if (fid > 0) {
        (async () => {
          try {
            const r = await personelService.lookupsForFirma(fid);
            if (r?.success) setLocalLookups(r.data);
          } catch { }
        })();
      }
    }
    else if (kind === "isyeri") setKisi((p: any) => ({ ...p, IsyeriId: key ? Number(key) : null }));
    else if (kind === "departman") setKisi((p: any) => ({ ...p, DepartmanId: key ? Number(key) : null }));
    else if (kind === "pozisyon") setKisi((p: any) => ({ ...p, PozisyonId: key ? Number(key) : null }));
    else if (kind === "bolum") setKisi((p: any) => ({ ...p, BolumId: key ? Number(key) : null }));
    else if (kind === "statu") {
      const id = Number(key) || 0;
      const it = statuler.find((x) => pickId(x) === id);
      setKisi((p: any) => ({ ...p, CalismaStatusu: id ? String(id) : "", CalismaStatusuText: it ? pickAd(it) : "" }));
    }
    setSelectModal({ kind: "", visible: false });
  }

  async function pickPhoto() {
    const perm = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (!perm.granted) return;
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ["images"],
      allowsEditing: true,
      aspect: [1, 1],
      quality: 0.8,
      base64: true,
    });
    if (result.canceled) return;
    const b64 = result.assets?.[0]?.base64;
    if (!b64) return;
    setFotoBase64(b64);
    setFotoDegisti(true);
  }

  async function submit() {
    const nextErrors: { PersonelId?: string; Ad?: string; Soyad?: string } = {};
    if (!s(kisi?.PersonelId).trim()) nextErrors.PersonelId = "Sicil No zorunludur.";
    if (!s(kisi?.Ad).trim()) nextErrors.Ad = "Ad zorunludur.";
    if (!s(kisi?.Soyad).trim()) nextErrors.Soyad = "Soyad zorunludur.";
    setFieldErrors(nextErrors);
    if (nextErrors.PersonelId || nextErrors.Ad || nextErrors.Soyad) return;

    const vardiyaCsvNow = joinCsv(vardiyaIds);
    const dogumIsoNow = dogumDate ? fmtIsoDate(dogumDate) : null;
    const iseIsoNow = iseDate ? fmtIsoDate(iseDate) : null;

    // Web-like behavior: always submit a fully-populated form.
    // If user changed only one field, all other fields are re-sent unchanged.
    const payload = {
      ...kisi,
      CalismaSekli: vardiyaCsvNow,
      CalismaSekliCsv: vardiyaCsvNow,
      DogumTarihi: dogumIsoNow,
      IseGirisTarihi: iseIsoNow,
      TaseronKartNo: (firmaDisiKartNo ?? "").toString(),
    };
    const req = {
      Kisi: payload,
      FirmaPersoneli: firmaPersoneli,
      PuantajYapilabilir: puantajYapilabilir,
      YemekHakkiVar: yemekHakkiVar,
      GunlukYemekLimiti: Number(gunlukYemek) || 0,
      GunlukYemekAdedi: Number(gunlukYemek) || 0,
      FirmaDisiKartNo: firmaDisiKartNo,
      ZiyaretciMi: ziyaretciMi,
      AracKartiMi: aracKartiMi,
      TaseronCalisanMi: taseronCalisanMi,
      FotoDegisti: fotoDegisti,
      FotografBase64: fotoDegisti ? fotoBase64 : undefined,
    };
    await props.onSubmit(req);
  }

  return (
    <Modal visible={props.visible} animationType="slide" onRequestClose={props.onClose}>
      <View className="flex-1 bg-[#f8fafc]">
        <View className="px-5 pt-12 pb-4 bg-white border-b border-[#f1f5f9] flex-row items-center justify-between">
          <Text className="text-[#1e293b] font-extrabold text-[16px]">{props.title}</Text>
          <TouchableOpacity onPress={props.onClose} className="p-2">
            <MaterialCommunityIcons name="close" size={22} color="#64748b" />
          </TouchableOpacity>
        </View>
        <ScrollView className="flex-1 px-5 py-5" contentContainerStyle={{ paddingBottom: 24 }}>
          {props.errorText ? (
            <View className="bg-[#fee2e2] rounded-3xl border border-[#fecaca] p-4 mb-4">
              <Text className="text-[#991b1b] font-extrabold mb-1">Hata</Text>
              <Text className="text-[#991b1b] font-semibold">{props.errorText}</Text>
            </View>
          ) : null}

          <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5 mb-4">
            <Text className="text-[#1e293b] font-extrabold mb-4">Fotoğraf</Text>
            <View className="items-center">
              {fotoBase64 ? (
                <Image source={{ uri: `data:image/jpeg;base64,${fotoBase64}` }} style={{ width: 160, height: 160, borderRadius: 16 }} />
              ) : (
                <View className="w-[160px] h-[160px] rounded-2xl bg-[#f1f5f9] items-center justify-center border border-[#e2e8f0]">
                  <MaterialCommunityIcons name="account" size={56} color="#94a3b8" />
                  <Text className="text-[#64748b] font-semibold mt-2">Fotoğraf yok</Text>
                </View>
              )}
              <TouchableOpacity onPress={pickPhoto} className="mt-4 bg-[#dc2626] rounded-2xl py-3 px-4 items-center">
                <Text className="text-white font-extrabold">Fotoğraf Seç</Text>
              </TouchableOpacity>
            </View>
          </View>

          <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5">
            <Text className="text-[#1e293b] font-extrabold mb-4">Temel Bilgiler</Text>
            <Text className="text-[#64748b] font-semibold mb-2">Sicil No *</Text>
            <TextInput
              className={`bg-[#f8fafc] border rounded-2xl px-4 py-3 ${fieldErrors.PersonelId ? "border-[#dc2626] mb-1" : "border-[#e2e8f0] mb-3"}`}
              value={s(kisi?.PersonelId)}
              onChangeText={(t) => {
                setKisi((p: any) => ({ ...p, PersonelId: t }));
                if (fieldErrors.PersonelId) setFieldErrors((e) => ({ ...e, PersonelId: undefined }));
              }}
            />
            {fieldErrors.PersonelId ? <Text className="text-[#dc2626] font-semibold text-[12px] mb-3">{fieldErrors.PersonelId}</Text> : null}
            <Text className="text-[#64748b] font-semibold mb-2">Ad *</Text>
            <TextInput
              className={`bg-[#f8fafc] border rounded-2xl px-4 py-3 ${fieldErrors.Ad ? "border-[#dc2626] mb-1" : "border-[#e2e8f0] mb-3"}`}
              value={s(kisi?.Ad)}
              onChangeText={(t) => {
                setKisi((p: any) => ({ ...p, Ad: t }));
                if (fieldErrors.Ad) setFieldErrors((e) => ({ ...e, Ad: undefined }));
              }}
            />
            {fieldErrors.Ad ? <Text className="text-[#dc2626] font-semibold text-[12px] mb-3">{fieldErrors.Ad}</Text> : null}
            <Text className="text-[#64748b] font-semibold mb-2">Soyad *</Text>
            <TextInput
              className={`bg-[#f8fafc] border rounded-2xl px-4 py-3 ${fieldErrors.Soyad ? "border-[#dc2626] mb-1" : "border-[#e2e8f0] mb-3"}`}
              value={s(kisi?.Soyad)}
              onChangeText={(t) => {
                setKisi((p: any) => ({ ...p, Soyad: t }));
                if (fieldErrors.Soyad) setFieldErrors((e) => ({ ...e, Soyad: undefined }));
              }}
            />
            {fieldErrors.Soyad ? <Text className="text-[#dc2626] font-semibold text-[12px] mb-3">{fieldErrors.Soyad}</Text> : null}
            <Text className="text-[#64748b] font-semibold mb-2">TC Kimlik No</Text>
            <TextInput className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 mb-3" keyboardType="numeric" value={s(kisi?.TcKimlikNo)} onChangeText={(t) => setKisi((p: any) => ({ ...p, TcKimlikNo: t }))} />
            <Text className="text-[#64748b] font-semibold mb-2">Kart No</Text>
            <TextInput className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 mb-3" value={s(kisi?.KartNo)} onChangeText={(t) => setKisi((p: any) => ({ ...p, KartNo: t }))} />

            <Text className="text-[#64748b] font-semibold mb-2">Doğum Tarihi</Text>
            <TouchableOpacity className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 mb-3 flex-row items-center justify-between" onPress={() => setPickerKind("dogum")}>
              <Text className="text-[#1e293b] font-semibold">{dogumDate ? fmtDateTR(dogumDate) : "Seçilmedi"}</Text>
              <MaterialCommunityIcons name="calendar" size={18} color="#64748b" />
            </TouchableOpacity>
            <Text className="text-[#64748b] font-semibold mb-2">İşe Giriş Tarihi *</Text>
            <TouchableOpacity className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 flex-row items-center justify-between" onPress={() => setPickerKind("ise")}>
              <Text className="text-[#1e293b] font-semibold">{iseDate ? fmtDateTR(iseDate) : "Seçilmedi"}</Text>
              <MaterialCommunityIcons name="calendar" size={18} color="#64748b" />
            </TouchableOpacity>
          </View>

          <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5 mt-4">
            <Text className="text-[#1e293b] font-extrabold mb-4">İletişim Bilgileri</Text>
            <Text className="text-[#64748b] font-semibold mb-2">Cep Telefonu</Text>
            <TextInput className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 mb-3" value={s(kisi?.CepTel)} onChangeText={(t) => setKisi((p: any) => ({ ...p, CepTel: t }))} />
            <Text className="text-[#64748b] font-semibold mb-2">E-posta</Text>
            <TextInput className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3" keyboardType="email-address" autoCapitalize="none" value={s(kisi?.Email)} onChangeText={(t) => setKisi((p: any) => ({ ...p, Email: t }))} />
          </View>

          <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5 mt-4">
            <Text className="text-[#1e293b] font-extrabold mb-4">Organizasyon</Text>
            {firmalar?.length ? (
              <>
                <Text className="text-[#64748b] font-semibold mb-2">Firma *</Text>
                <TouchableOpacity className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 mb-3 flex-row items-center justify-between" onPress={() => setSelectModal({ kind: "firma", visible: true })}>
                  <Text className="text-[#1e293b] font-semibold" numberOfLines={1}>{currentLabelFor("firma")}</Text>
                  <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" />
                </TouchableOpacity>
              </>
            ) : null}
            {[
              ["isyeri", "İşyeri"],
              ["departman", "Departman"],
              ["pozisyon", "Pozisyon"],
              ["bolum", "Bölüm"],
              ["statu", "Çalışma Statüsü"],
            ].map(([k, label]) => (
              <View key={k} className="mb-3">
                <Text className="text-[#64748b] font-semibold mb-2">{label}</Text>
                <TouchableOpacity className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 flex-row items-center justify-between" onPress={() => setSelectModal({ kind: k, visible: true })}>
                  <Text className="text-[#1e293b] font-semibold" numberOfLines={1}>{currentLabelFor(k)}</Text>
                  <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" />
                </TouchableOpacity>
              </View>
            ))}
          </View>

          <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5 mt-4">
            <Text className="text-[#1e293b] font-extrabold mb-4">Çalışma Şekilleri (Vardiyalar)</Text>
            <TouchableOpacity className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 flex-row items-center justify-between" onPress={() => setVardiyaModal(true)}>
              <Text className="text-[#1e293b] font-semibold" numberOfLines={1}>{vardiyaLabel}</Text>
              <MaterialCommunityIcons name="chevron-right" size={20} color="#64748b" />
            </TouchableOpacity>
          </View>

          <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5 mt-4">
            <Text className="text-[#1e293b] font-extrabold mb-4">Özel Ayarlar</Text>
            {[
              ["Firma Personeli", firmaPersoneli, setFirmaPersoneli],
              ["Puantaj Yapılabilir", puantajYapilabilir, setPuantajYapilabilir],
              ["Yemek Hakkı Var", yemekHakkiVar, setYemekHakkiVar],
              ["Ziyaretçi", ziyaretciMi, setZiyaretciMi],
              ["Araç Kartı", aracKartiMi, setAracKartiMi],
              ["Taşeron Çalışan", taseronCalisanMi, setTaseronCalisanMi],
            ].map(([label, val, fn]) => (
              <View key={label as string} className="flex-row items-center justify-between py-2">
                <Text className="text-[#1e293b] font-semibold">{label as string}</Text>
                <Switch value={val as boolean} onValueChange={fn as any} />
              </View>
            ))}
            <Text className="text-[#64748b] font-semibold mt-3 mb-2">Günlük Yemek Adedi/Limiti</Text>
            <TextInput className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3" keyboardType="numeric" value={gunlukYemek} onChangeText={setGunlukYemek} />
            <Text className="text-[#64748b] font-semibold mt-3 mb-2">Firma Dışı Kart No</Text>
            <TextInput className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3" value={firmaDisiKartNo} onChangeText={setFirmaDisiKartNo} />
          </View>

          <TouchableOpacity onPress={submit} className="mt-5 bg-[#dc2626] rounded-2xl py-4 items-center">
            <Text className="text-white font-extrabold">Kaydet</Text>
          </TouchableOpacity>
          <TouchableOpacity onPress={props.onClose} className="mt-3 bg-[#f1f5f9] rounded-2xl py-4 items-center">
            <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
          </TouchableOpacity>
        </ScrollView>

        <SelectModal
          visible={selectModal.visible}
          title="Seçiniz"
          items={selectItems}
          onClose={() => setSelectModal({ kind: "", visible: false })}
          onSelect={(k) => applySelect(k)}
        />

        <MultiSelectModal
          visible={vardiyaModal}
          title="Vardiya Seç"
          items={uniqueById(vardiyalar, (v) => pickId(v as any)).map((v) => ({ id: pickId(v as any), label: pickAd(v as any) }))}
          selected={vardiyaIds}
          onClose={() => setVardiyaModal(false)}
          onChange={setVardiyaIds}
        />

        {pickerKind ? (
          <Modal transparent visible animationType="fade" onRequestClose={() => setPickerKind(null)}>
            <View className="flex-1 bg-black/60 items-center justify-center px-6">
              <View className="w-full rounded-3xl bg-white p-5">
                <View className="flex-row items-center justify-between mb-3">
                  <Text className="text-[15px] font-extrabold text-[#1e293b]">Tarih Seç</Text>
                  <TouchableOpacity onPress={() => setPickerKind(null)} className="p-2">
                    <MaterialCommunityIcons name="close" size={20} color="#64748b" />
                  </TouchableOpacity>
                </View>
                <View style={{ height: Platform.OS === "ios" ? 360 : 216, width: "100%", justifyContent: "center" }}>
                  <DateTimePicker
                    value={pickerKind === "dogum" ? (dogumDate ?? new Date(1990, 0, 1)) : (iseDate ?? new Date())}
                    mode="date"
                    display={Platform.OS === "ios" ? "inline" : "default"}
                    locale="tr-TR"
                    themeVariant="light"
                    textColor="#0f172a"
                    style={{ width: "100%", height: Platform.OS === "ios" ? 360 : 216 }}
                    onChange={(_, selected) => {
                      if (Platform.OS !== "ios") setPickerKind(null);
                      if (!selected) return;
                      const nd = normalizeDateOnly(selected);
                      if (pickerKind === "dogum") setDogumDate(nd);
                      else setIseDate(nd);
                    }}
                  />
                </View>
                {Platform.OS === "ios" ? (
                  <TouchableOpacity onPress={() => setPickerKind(null)} className="mt-4 bg-[#dc2626] rounded-2xl py-3 items-center">
                    <Text className="text-white font-extrabold">Tamam</Text>
                  </TouchableOpacity>
                ) : null}
              </View>
            </View>
          </Modal>
        ) : null}
      </View>
    </Modal>
  );
}

export function PersonellerScreen(props: { user: any; abilities?: any; onOpenMenu?: () => void }) {
  const subtitle = useMemo(() => (props.user?.adSoyad ?? props.user?.AdSoyad ?? "").toString(), [props.user]);
  const [loading, setLoading] = useState(true);
  const [filtersHydrated, setFiltersHydrated] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lookups, setLookups] = useState<any>(null);
  const [popupVisible, setPopupVisible] = useState(false);
  const [popupType, setPopupType] = useState<"success" | "error">("success");
  const [popupMessage, setPopupMessage] = useState("");
  const [popupUndo, setPopupUndo] = useState<(() => Promise<void>) | null>(null);
  const [undoLoading, setUndoLoading] = useState(false);
  const closePopup = () => {
    setPopupVisible(false);
    setPopupUndo(null);
    setUndoLoading(false);
  };
  const showPopup = (type: "success" | "error", message: string, undo?: () => Promise<void>) => {
    setPopupType(type);
    setPopupMessage(message);
    setPopupUndo(undo ?? null);
    setPopupVisible(true);
  };

  const [kartTipi, setKartTipi] = useState<"puantaj" | "puantajsiz">("puantaj");
  const [calismaDurumu, setCalismaDurumu] = useState<"aktif" | "cikan">("aktif");
  const sadeceIstenCikanlar = calismaDurumu === "cikan";
  const [firmaId, setFirmaId] = useState<number | null>(null);
  const [isyeriId, setIsyeriId] = useState<number | null>(null);
  const [q, setQ] = useState("");
  const [pageSize, setPageSize] = useState(20);
  const [page, setPage] = useState(1);

  const [list, setList] = useState<any[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  // (debug status removed)

  const [filterModal, setFilterModal] = useState<{ kind: string; visible: boolean }>({ kind: "", visible: false });

  const [detailVisible, setDetailVisible] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [detail, setDetail] = useState<any>(null);

  const [createVisible, setCreateVisible] = useState(false);
  const [editVisible, setEditVisible] = useState(false);
  const [acting, setActing] = useState(false);

  // iOS modal stacking: open child modal after closing detail modal
  const [pendingEditOpen, setPendingEditOpen] = useState(false);
  const [pendingIstenOpen, setPendingIstenOpen] = useState(false);
  const [pendingIstenPickerOpen, setPendingIstenPickerOpen] = useState(false);

  const [istenVisible, setIstenVisible] = useState(false);
  const [istenDateObj, setIstenDateObj] = useState<Date>(new Date());
  const [istenDate, setIstenDate] = useState<string>(fmtIsoDate(new Date()));
  const [istenKartNo, setIstenKartNo] = useState("");
  const [istenPickerVisible, setIstenPickerVisible] = useState(false);
  const [aktifEtVisible, setAktifEtVisible] = useState(false);
  const [aktifEtPuantaj, setAktifEtPuantaj] = useState(true);

  const isyerleri: LookupItem[] = (lookups?.Isyerleri ?? lookups?.isyerleri ?? []) as any;
  const firmalar: FirmaItem[] = (lookups?.Firmalar ?? lookups?.firmalar ?? []) as any;
  const aktifFirma: AktifFirma | null = (lookups?.AktifFirma ?? lookups?.aktifFirma ?? null) as any;
  const vardiyalar: CalismaSekliItem[] = (lookups?.CalismaSekilleri ?? lookups?.calismaSekilleri ?? []) as any;

  const puantajYapilirMi = kartTipi !== "puantajsiz";

  const can = useMemo(() => {
    const a = props.abilities;
    const actions = a?.actions ?? a?.Actions ?? {};
    const p = actions?.Personeller ?? actions?.["Personeller"] ?? {};
    return {
      create: !!(p?.Create ?? p?.create),
      update: !!(p?.Update ?? p?.update),
      del: !!(p?.Delete ?? p?.delete),
    };
  }, [props.abilities]);

  async function loadLookups() {
    try {
      const r = await personelService.lookupsForFirma(firmaId);
      if (r?.success) {
        setLookups(r.data);
        return r.data;
      }
    } catch {}
    return null;
  }

  async function loadList(nextPage = 1, nextPageSize = pageSize, opts?: { manageLoading?: boolean; isyeriIdOverride?: number | null }) {
    const manageLoading = opts?.manageLoading !== false;
    const listIsyeriId = opts?.isyeriIdOverride !== undefined ? opts.isyeriIdOverride : isyeriId;
    try {
      if (manageLoading) setLoading(true);
      setError(null);
      const r = await personelService.list({
        search: q || undefined,
        firmaId: firmaId ?? undefined,
        isyeriId: listIsyeriId ?? undefined,
        puantajYapilirMi: sadeceIstenCikanlar ? undefined : puantajYapilirMi,
        sadeceIstenCikanlar,
        page: nextPage,
        pageSize: nextPageSize,
      });
      if (!r?.success) {
        setList([]);
        setTotalPages(1);
        setTotalCount(0);
        const msg = r?.message || "Personel listesi alınamadı.";
        setError(msg);
        return;
      }
      const payload: any = r.data ?? {};
      const items = payload?.items ?? payload?.Items ?? [];
      const totalCount = Number(payload?.totalCount ?? payload?.TotalCount ?? 0) || 0;
      const totalPages = Number(payload?.totalPages ?? payload?.TotalPages ?? 1) || 1;
      const pageNo = Number(payload?.page ?? payload?.Page ?? nextPage) || nextPage;
      const size = Number(payload?.pageSize ?? payload?.PageSize ?? nextPageSize) || nextPageSize;
      setList(Array.isArray(items) ? items : []);
      setTotalCount(totalCount);
      setTotalPages(totalPages);
      setPage(pageNo);
      setPageSize(size);
    } catch (e: any) {
      const msg = apiErrorMessage(e);
      setError(msg);
      setList([]);
      setTotalPages(1);
      setTotalCount(0);
    } finally {
      if (manageLoading) setLoading(false);
    }
  }

  useEffect(() => {
    (async () => {
      const prefs = await pageFilterPrefs.load("Personeller");
      if (prefs) {
        if (typeof prefs.firmaId === "number" && prefs.firmaId > 0) setFirmaId(prefs.firmaId);
        if (typeof prefs.isyeriId === "number" && prefs.isyeriId > 0) setIsyeriId(prefs.isyeriId);
        if (prefs.boolA === true) setCalismaDurumu("cikan");
        else if (prefs.boolA === false) setCalismaDurumu("aktif");
        if (prefs.boolB === true) setKartTipi("puantaj");
        else if (prefs.boolB === false) setKartTipi("puantajsiz");
      }
      setFiltersHydrated(true);
    })();
  }, []);

  useEffect(() => {
    if (!filtersHydrated) return;
    void pageFilterPrefs.save("Personeller", {
      firmaId,
      isyeriId,
      boolA: calismaDurumu === "cikan",
      boolB: kartTipi === "puantaj",
    });
    (async () => {
      setLoading(true);
      try {
        const data = await loadLookups();
        let effectiveIsyeri = isyeriId;
        if (data && isyeriId != null) {
          const list = (data?.Isyerleri ?? data?.isyerleri ?? []) as LookupItem[];
          if (!list.some((x) => pickId(x) === isyeriId)) {
            effectiveIsyeri = null;
            setIsyeriId(null);
            return; // next effect run with cleared isyeri
          }
        }
        await loadList(1, pageSize, { manageLoading: false, isyeriIdOverride: effectiveIsyeri });
      } finally {
        setLoading(false);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filtersHydrated, kartTipi, calismaDurumu, firmaId, isyeriId, pageSize]);

  useEffect(() => {
    if (!pendingEditOpen) return;
    if (detailVisible) return; // wait for close
    const t = setTimeout(() => {
      setEditVisible(true);
      setPendingEditOpen(false);
    }, 200);
    return () => clearTimeout(t);
  }, [pendingEditOpen, detailVisible]);

  useEffect(() => {
    if (!pendingIstenOpen) return;
    if (detailVisible) return; // wait for close
    const t = setTimeout(() => {
      const d = new Date();
      setIstenDateObj(d);
      setIstenDate(fmtIsoDate(d));
      setIstenKartNo("");
      setIstenVisible(true);
      setPendingIstenOpen(false);
    }, 200);
    return () => clearTimeout(t);
  }, [pendingIstenOpen, detailVisible]);

  useEffect(() => {
    if (!pendingIstenPickerOpen) return;
    if (istenVisible) return; // wait for close
    const t = setTimeout(() => {
      setIstenPickerVisible(true);
      setPendingIstenPickerOpen(false);
    }, 200);
    return () => clearTimeout(t);
  }, [pendingIstenPickerOpen, istenVisible]);

  async function openDetails(personelId: string) {
    try {
      setDetailVisible(true);
      setDetailLoading(true);
      setDetail(null);
      const r = await personelService.details(personelId);
      if (!r?.success) setError(r?.message || "Detay alınamadı.");
      else setDetail(r.data);
    } catch (e: any) {
      setError(apiErrorMessage(e));
    } finally {
      setDetailLoading(false);
    }
  }

  const filterItems = useMemo(() => {
    if (filterModal.kind === "calismaDurumu") {
      return [
        { key: "aktif", label: "Aktif Çalışanlar" },
        { key: "cikan", label: "İşten Çıkanlar" },
      ];
    }
    if (filterModal.kind === "kartTipi") {
      return [
        { key: "puantaj", label: "Puantaj Yapılan Kartlar" },
        { key: "puantajsiz", label: "Puantaj Yapılmayan Kartlar" },
      ];
    }
    if (filterModal.kind === "firma") {
      return [{ key: "", label: "Aktif Firma" }, ...firmalar.map((f) => ({ key: String(pickFirmaId(f)), label: pickFirmaAdi(f) }))];
    }
    if (filterModal.kind === "isyeri") {
      return [{ key: "", label: "Tümü" }, ...isyerleri.map((x) => ({ key: String(pickId(x)), label: pickAd(x) }))];
    }
    if (filterModal.kind === "pageSize") {
      return [
        { key: "10", label: "10" },
        { key: "20", label: "20" },
        { key: "50", label: "50" },
        { key: "100", label: "100" },
      ];
    }
    return [];
  }, [filterModal.kind, firmalar, isyerleri]);

  function applyFilter(key: string) {
    if (filterModal.kind === "calismaDurumu") {
      setCalismaDurumu((key as any) === "cikan" ? "cikan" : "aktif");
      setDetail(null);
      setDetailVisible(false);
      setEditVisible(false);
    }
    else if (filterModal.kind === "kartTipi") {
      setKartTipi((key as any) || "puantaj");
      setDetail(null);
      setDetailVisible(false);
      setEditVisible(false);
    }
    else if (filterModal.kind === "firma") {
      setFirmaId(key ? Number(key) : null);
      setIsyeriId(null);
      setDetail(null);
      setDetailVisible(false);
      setEditVisible(false);
    }
    else if (filterModal.kind === "isyeri") {
      setIsyeriId(key ? Number(key) : null);
      setDetail(null);
      setDetailVisible(false);
      setEditVisible(false);
    }
    else if (filterModal.kind === "pageSize") setPageSize(Number(key) || 20);
    setFilterModal({ kind: "", visible: false });
  }

  async function doCreate(req: any) {
    try {
      setActing(true);
      setError(null);
      const r = await personelService.create(req);
      if (!r?.success) {
        const msg = r?.message || "Personel eklenemedi.";
        setError(msg);
        showPopup("error", msg);
      }
      else {
        showPopup("success", r?.message || "Personel eklendi.");
        setCreateVisible(false);
        await loadList(1, pageSize);
      }
    } catch (e: any) {
      const msg = apiErrorMessage(e);
      setError(msg);
      showPopup("error", msg);
    } finally {
      setActing(false);
    }
  }

  async function doEdit(req: any) {
    try {
      setActing(true);
      setError(null);
      const original = s(pick(detail, "PersonelId", "personelId") ?? "");
      const r = await personelService.update({ ...req, OriginalPersonelId: original });
      if (!r?.success) {
        const msg = r?.message || "Personel güncellenemedi.";
        setError(msg);
        showPopup("error", msg);
      }
      else {
        showPopup("success", r?.message || "Personel güncellendi.");
        setEditVisible(false);
        await openDetails(original);
        await loadList(page, pageSize);
      }
    } catch (e: any) {
      const msg = apiErrorMessage(e);
      setError(msg);
      showPopup("error", msg);
    } finally {
      setActing(false);
    }
  }

  async function doTekrarAktifEt() {
    const pid = s(pick(detail, "PersonelId", "personelId") ?? "");
    if (!pid) return;
    try {
      setActing(true);
      setError(null);
      const r = await personelService.tekrarAktifEt({ personelId: pid, puantajYapilirMi: aktifEtPuantaj });
      if (!r?.success) {
        const msg = r?.message || "Personel tekrar aktif edilemedi.";
        setError(msg);
        showPopup("error", msg);
      } else {
        const data = r?.data ?? {};
        const msg = s(data?.message ?? data?.Message ?? r?.message) || "Personel tekrar aktif edildi.";
        showPopup("success", msg);
        setAktifEtVisible(false);
        setDetailVisible(false);
        setCalismaDurumu("aktif");
        await loadList(1, pageSize);
      }
    } catch (e: any) {
      const msg = apiErrorMessage(e);
      setError(msg);
      showPopup("error", msg);
    } finally {
      setActing(false);
    }
  }

  async function doIstenCikar() {
    const pid = s(pick(detail, "PersonelId", "personelId") ?? "");
    if (!pid) return;
    try {
      setActing(true);
      setError(null);
      const r = await personelService.istenCikar({
        personelId: pid,
        cikisTarihi: istenDate || null,
        firmaDisiKartNo: istenKartNo || null,
      });
      if (!r?.success) {
        const msg = r?.message || "İşten çıkarma başarısız.";
        setError(msg);
        showPopup("error", msg);
      }
      else {
        const undoPid = pid;
        const puantajForUndo = !!pick(detail, "PuantajYapilabilir", "puantajYapilabilir");
        showPopup("success", r?.message || "İşlem başarılı.", async () => {
          setUndoLoading(true);
          try {
            const ur = await personelService.tekrarAktifEt({ personelId: undoPid, puantajYapilirMi: puantajForUndo });
            if (!ur?.success) throw new Error(ur?.message || "Geri alma başarısız.");
            closePopup();
            showPopup("success", "Geri alındı.");
            setIstenVisible(false);
            await openDetails(undoPid);
            await loadList(page, pageSize);
          } catch (err: any) {
            showPopup("error", apiErrorMessage(err));
          } finally {
            setUndoLoading(false);
          }
        });
        setIstenVisible(false);
        await openDetails(pid);
        await loadList(page, pageSize);
      }
    } catch (e: any) {
      const msg = apiErrorMessage(e);
      setError(msg);
      showPopup("error", msg);
    } finally {
      setActing(false);
    }
  }

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={closePopup} useModal={false} autoCloseMs={popupUndo ? 7000 : 1500} onUndo={popupUndo ? () => popupUndo() : undefined} undoLoading={undoLoading} />
      <BusyOverlay visible={loading} title="Yükleniyor..." message="Personel listesi hazırlanıyor" />
      <PersonelHeader title="Personeller" subtitle={subtitle} onOpenMenu={props.onOpenMenu} />

      <ScrollView className="flex-1" contentContainerStyle={{ paddingBottom: 30 }}>
        <View className="px-5 pt-5">
          <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5">
            <Text className="text-[#1e293b] font-extrabold mb-3">Filtreler</Text>

            {(firmalar?.length || aktifFirma) ? (
              <>
                <Text className="text-[#64748b] font-semibold mb-2">Firma</Text>
                <TouchableOpacity
                  className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 flex-row items-center justify-between mb-3"
                  onPress={() => (firmalar?.length ? setFilterModal({ kind: "firma", visible: true }) : null)}
                  disabled={!firmalar?.length}
                >
                  <Text className="text-[#1e293b] font-semibold" numberOfLines={1}>
                    {firmaId
                      ? (pickFirmaAdi(firmalar.find((f) => pickFirmaId(f) === firmaId)) || "Aktif Firma")
                      : (pickFirmaAdi(aktifFirma) || "Aktif Firma")}
                  </Text>
                  {firmalar?.length ? <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" /> : <View style={{ width: 20, height: 20 }} />}
                </TouchableOpacity>
              </>
            ) : null}

            <Text className="text-[#64748b] font-semibold mb-2">Durum</Text>
            <TouchableOpacity className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 flex-row items-center justify-between mb-3" onPress={() => setFilterModal({ kind: "calismaDurumu", visible: true })}>
              <Text className="text-[#1e293b] font-semibold" numberOfLines={1}>{calismaDurumu === "cikan" ? "İşten Çıkanlar" : "Aktif Çalışanlar"}</Text>
              <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" />
            </TouchableOpacity>

            <Text className="text-[#64748b] font-semibold mb-2">Kart tipi</Text>
            <TouchableOpacity
              className={`bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 flex-row items-center justify-between mb-3 ${sadeceIstenCikanlar ? "opacity-50" : ""}`}
              onPress={() => !sadeceIstenCikanlar && setFilterModal({ kind: "kartTipi", visible: true })}
              disabled={sadeceIstenCikanlar}
            >
              <Text className="text-[#1e293b] font-semibold" numberOfLines={1}>{kartTipi === "puantajsiz" ? "Puantaj Yapılmayan Kartlar" : "Puantaj Yapılan Kartlar"}</Text>
              <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" />
            </TouchableOpacity>

            <Text className="text-[#64748b] font-semibold mb-2">İşyeri</Text>
            <TouchableOpacity className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 flex-row items-center justify-between mb-3" onPress={() => setFilterModal({ kind: "isyeri", visible: true })}>
              <Text className="text-[#1e293b] font-semibold" numberOfLines={1}>{isyeriId ? (pickAd(isyerleri.find((x) => pickId(x) === isyeriId)) || "Tümü") : "Tümü"}</Text>
              <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" />
            </TouchableOpacity>

            <Text className="text-[#64748b] font-semibold mb-2">Ara (Ad/Soyad/Sicil)</Text>
            <TextInput className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 mb-3" value={q} onChangeText={setQ} placeholder="İsim veya sicil no..." placeholderTextColor="#94a3b8" />

            <View className="flex-row items-center justify-between">
              <TouchableOpacity className="flex-1 bg-[#dc2626] rounded-2xl py-3 items-center mr-2" onPress={() => loadList(1, pageSize)} disabled={loading}>
                <Text className="text-white font-extrabold">Ara</Text>
              </TouchableOpacity>
              <TouchableOpacity className="bg-[#f1f5f9] rounded-2xl py-3 px-4 items-center" onPress={() => { setQ(""); setFirmaId(null); setIsyeriId(null); setKartTipi("puantaj"); setCalismaDurumu("aktif"); setPageSize(20); loadList(1, 20); }}>
                <Text className="text-[#334155] font-extrabold">Temizle</Text>
              </TouchableOpacity>
            </View>

            <View className="mt-4 flex-row items-center justify-between">
              <Text className="text-[#64748b] font-semibold">Sayfa başına</Text>
              <TouchableOpacity className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-2 flex-row items-center" onPress={() => setFilterModal({ kind: "pageSize", visible: true })}>
                <Text className="text-[#1e293b] font-extrabold">{pageSize}</Text>
                <MaterialCommunityIcons name="chevron-down" size={18} color="#64748b" style={{ marginLeft: 6 }} />
              </TouchableOpacity>
            </View>
          </View>

          <View className="mt-4 bg-white rounded-3xl border border-[#f1f5f9] p-5">
            <View className="flex-row items-center justify-between mb-3">
              <Text className="text-[#1e293b] font-extrabold">Liste</Text>
              <Text className="text-[#64748b] font-semibold text-[12px]">Toplam {totalCount}</Text>
            </View>

            {error ? (
              <View className="mb-3 bg-[#fee2e2] rounded-2xl p-3">
                <Text className="text-[#991b1b] font-semibold">{error}</Text>
              </View>
            ) : null}

            {loading ? (
              <Text className="text-[#64748b] font-semibold">Yükleniyor...</Text>
            ) : list.length ? (
              <View>
                {list.map((p, idx) => {
                  const pid = s(p?.PersonelId ?? p?.personelId);
                  const name = s(p?.AdSoyad ?? p?.adSoyad);
                  return (
                    <TouchableOpacity key={`${pid}_${idx}`} onPress={() => openDetails(pid)} className="border-t border-[#f1f5f9] py-4 flex-row items-center justify-between">
                      <View>
                        <Text className="text-[#1e293b] font-extrabold">{name || "-"}</Text>
                        <Text className="text-[#64748b] font-semibold text-[12px]">Sicil: {pid || "-"}</Text>
                      </View>
                      <MaterialCommunityIcons name="chevron-right" size={22} color="#94a3b8" />
                    </TouchableOpacity>
                  );
                })}
              </View>
            ) : (
              <Text className="text-[#64748b] font-semibold">
                {isyeriId
                  ? "Seçili işyerde kayıt bulunamadı."
                  : q?.trim()
                    ? "Arama kriterine uygun kayıt bulunamadı."
                    : "Kayıt bulunamadı."}
              </Text>
            )}

            <View className="mt-4 flex-row items-center justify-between">
              <TouchableOpacity className={`px-4 py-3 rounded-2xl ${page <= 1 ? "bg-[#f1f5f9]" : "bg-white border border-[#e2e8f0]"}`} disabled={page <= 1 || loading} onPress={() => loadList(Math.max(1, page - 1), pageSize)}>
                <Text className="text-[#334155] font-extrabold">Önceki</Text>
              </TouchableOpacity>
              <Text className="text-[#64748b] font-semibold">{page} / {totalPages}</Text>
              <TouchableOpacity className={`px-4 py-3 rounded-2xl ${page >= totalPages ? "bg-[#f1f5f9]" : "bg-white border border-[#e2e8f0]"}`} disabled={page >= totalPages || loading} onPress={() => loadList(Math.min(totalPages, page + 1), pageSize)}>
                <Text className="text-[#334155] font-extrabold">Sonraki</Text>
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </ScrollView>

      {can.create ? (
        <TouchableOpacity onPress={() => setCreateVisible(true)} className="absolute right-5 bottom-6 w-14 h-14 rounded-full bg-[#dc2626] items-center justify-center shadow" activeOpacity={0.9}>
          <MaterialCommunityIcons name="plus" size={26} color="white" />
        </TouchableOpacity>
      ) : null}

      <SelectModal visible={filterModal.visible} title="Seçiniz" items={filterItems} onClose={() => setFilterModal({ kind: "", visible: false })} onSelect={(k) => applyFilter(k)} />

      <Modal visible={detailVisible} animationType="slide" onRequestClose={() => setDetailVisible(false)}>
        <View className="flex-1 bg-[#f8fafc]">
          <View className="px-5 pt-12 pb-4 bg-white border-b border-[#f1f5f9] flex-row items-center justify-between">
            <Text className="text-[#1e293b] font-extrabold text-[16px]">Personel Detay</Text>
            <TouchableOpacity onPress={() => setDetailVisible(false)} className="p-2">
              <MaterialCommunityIcons name="close" size={22} color="#64748b" />
            </TouchableOpacity>
          </View>
          <ScrollView className="flex-1 px-5 py-5" contentContainerStyle={{ paddingBottom: 24 }}>
            {detailLoading ? (
              <View className="items-center justify-center py-10">
                <ActivityIndicator color="#dc2626" />
                <Text className="text-[#64748b] font-semibold mt-3">Yükleniyor...</Text>
              </View>
            ) : detail ? (
              <View>
                <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5 mb-4">
                  <Text className="text-[#1e293b] font-extrabold mb-3">Fotoğraf</Text>
                  <View className="items-center">
                    {s(pick(detail, "Fotograf", "fotograf") ?? "") ? (
                      <Image source={{ uri: `data:image/jpeg;base64,${s(pick(detail, "Fotograf", "fotograf") ?? "")}` }} style={{ width: 220, height: 220, borderRadius: 16 }} />
                    ) : (
                      <View className="w-[220px] h-[220px] rounded-2xl bg-[#f1f5f9] items-center justify-center border border-[#e2e8f0]">
                        <MaterialCommunityIcons name="account" size={72} color="#94a3b8" />
                        <Text className="text-[#64748b] font-semibold mt-2">Fotoğraf yok</Text>
                      </View>
                    )}
                  </View>
                </View>
                <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5">
                  <Text className="text-[#1e293b] font-extrabold mb-2">{(s(pick(detail, "Ad", "ad") ?? "") + " " + s(pick(detail, "Soyad", "soyad") ?? "")).trim() || s(pick(detail, "AdSoyad", "adSoyad") ?? "-")}</Text>
                  <Text className="text-[#64748b] font-semibold">Sicil No: {s(pick(detail, "PersonelId", "personelId") ?? "") || "-"}</Text>
                </View>
                <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5 mt-4">
                  <Text className="text-[#1e293b] font-extrabold mb-3">Temel Bilgiler</Text>
                  <Text className="text-[#334155] font-semibold">TC Kimlik No: {s(pick(detail, "TcKimlikNo", "tcKimlikNo") ?? "") || "-"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Kart No: {s(pick(detail, "KartNo", "kartNo") ?? "") || "-"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Doğum Tarihi: {fmtDateTR(pick(detail, "DogumTarihi", "dogumTarihi")) || "-"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">İşe Giriş Tarihi: {fmtDateTR(pick(detail, "IseGirisTarihi", "iseGirisTarihi")) || "-"}</Text>
                  {fmtDateTR(pick(detail, "IstenCikisTarihi", "istenCikisTarihi")) ? (
                    <Text className="text-[#dc2626] font-extrabold mt-1">İşten Çıkış Tarihi: {fmtDateTR(pick(detail, "IstenCikisTarihi", "istenCikisTarihi"))}</Text>
                  ) : null}
                </View>

                <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5 mt-4">
                  <Text className="text-[#1e293b] font-extrabold mb-3">İletişim Bilgileri</Text>
                  <Text className="text-[#334155] font-semibold">Cep Telefonu: {s(pick(detail, "CepTel", "cepTel") ?? "") || "-"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">E-posta: {s(pick(detail, "Email", "email") ?? "") || "-"}</Text>
                </View>
                <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5 mt-4">
                  <Text className="text-[#1e293b] font-extrabold mb-3">Organizasyon Bilgileri</Text>
                  <Text className="text-[#334155] font-semibold">Firma: {s(pick(detail, "FirmaAdi", "firmaAdi") ?? "") || "-"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">İşyeri: {s(pick(detail, "IsyeriAdi", "isyeriAdi") ?? "") || "-"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Departman: {s(pick(detail, "DepartmanAdi", "departmanAdi") ?? "") || "-"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Pozisyon: {s(pick(detail, "PozisyonAdi", "pozisyonAdi") ?? "") || "-"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Bölüm: {s(pick(detail, "BolumAdi", "bolumAdi") ?? "") || "-"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Çalışma Statüsü: {s(pick(detail, "CalismaStatusuText", "calismaStatusuText") ?? "") || "-"}</Text>
                </View>

                <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5 mt-4">
                  <Text className="text-[#1e293b] font-extrabold mb-3">Çalışma Şekilleri (Vardiyalar)</Text>
                  {(() => {
                    const csv = s(pick(detail, "CalismaSekliCsv", "calismaSekliCsv") ?? "");
                    const ids = parseCsv(csv);
                    if (!ids.length) return <Text className="text-[#64748b] font-semibold">-</Text>;
                    const uniq = uniqueById(vardiyalar, (v) => pickId(v as any));
                    const map = new Map(uniq.map((v) => [pickId(v as any), pickAd(v as any)]));
                    const names = ids.map((id) => map.get(id)).filter(Boolean);
                    return <Text className="text-[#334155] font-semibold">{names.join(", ")}</Text>;
                  })()}
                </View>

                <View className="bg-white rounded-3xl border border-[#f1f5f9] p-5 mt-4">
                  <Text className="text-[#1e293b] font-extrabold mb-3">Özel Ayarlar</Text>
                  <Text className="text-[#334155] font-semibold">Firma Personeli: {pick(detail, "FirmaPersoneli", "firmaPersoneli") ? "Evet" : "Hayır"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Puantaj Yapılabilir: {pick(detail, "PuantajYapilabilir", "puantajYapilabilir") ? "Evet" : "Hayır"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Yemek Hakkı Var: {pick(detail, "YemekHakkiVar", "yemekHakkiVar") ? "Evet" : "Hayır"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Günlük Yemek Adedi: {s(pick(detail, "GunlukYemekAdedi", "gunlukYemekAdedi") ?? "") || "0"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Firma Dışı Kart No: {s(pick(detail, "TaseronKartNo", "taseronKartNo") ?? "") || "-"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Ziyaretçi: {pick(detail, "ZiyaretciMi", "ziyaretciMi") ? "Evet" : "Hayır"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Araç Kartı: {pick(detail, "AracKartiMi", "aracKartiMi") ? "Evet" : "Hayır"}</Text>
                  <Text className="text-[#334155] font-semibold mt-1">Taşeron Çalışan: {pick(detail, "TaseronCalisanMi", "taseronCalisanMi") ? "Evet" : "Hayır"}</Text>
                </View>
                <View className="mt-4 flex-row">
                  {sadeceIstenCikanlar && can.update ? (
                    <TouchableOpacity
                      onPress={() => {
                        setAktifEtPuantaj(true);
                        setDetailVisible(false);
                        setTimeout(() => setAktifEtVisible(true), 200);
                      }}
                      className="flex-1 bg-[#16a34a] rounded-2xl py-4 items-center"
                      disabled={acting}
                    >
                      <Text className="text-white font-extrabold">Aktif et</Text>
                    </TouchableOpacity>
                  ) : null}
                  {!sadeceIstenCikanlar && can.update ? (
                    <TouchableOpacity
                      onPress={() => {
                        // close detail first to avoid stacking issues (esp. iOS)
                        setDetailVisible(false);
                        setPendingEditOpen(true);
                      }}
                      className="flex-1 bg-[#fbbf24] rounded-2xl py-4 items-center mr-2"
                      disabled={acting}
                    >
                      <Text className="text-[#1e293b] font-extrabold">Düzenle</Text>
                    </TouchableOpacity>
                  ) : null}
                  {!sadeceIstenCikanlar && can.del && kartTipi !== "puantajsiz" ? (
                    <TouchableOpacity
                      onPress={() => {
                        setDetailVisible(false);
                        setPendingIstenOpen(true);
                      }}
                      className="flex-1 bg-[#fee2e2] rounded-2xl py-4 items-center"
                      disabled={acting}
                    >
                      <Text className="text-[#dc2626] font-extrabold">İşten Çıkar</Text>
                    </TouchableOpacity>
                  ) : null}
                </View>
              </View>
            ) : (
              <Text className="text-[#64748b] font-semibold">Detay bulunamadı.</Text>
            )}
          </ScrollView>
        </View>
      </Modal>

      <PersonelFormModal
        visible={createVisible}
        title={acting ? "Kaydediliyor..." : "Yeni Personel"}
        lookups={lookups}
        initial={{ FirmaId: 0, PuantajYapilirMi: true, IseGirisTarihi: fmtIsoDate(new Date()) }}
        errorText={createVisible ? error : null}
        onClose={() => setCreateVisible(false)}
        onSubmit={doCreate}
      />

      <PersonelFormModal
        visible={editVisible}
        title={acting ? "Kaydediliyor..." : "Personel Düzenle"}
        lookups={lookups}
        initial={detail}
        errorText={editVisible ? error : null}
        onClose={() => setEditVisible(false)}
        onSubmit={doEdit}
      />

      <Modal transparent visible={aktifEtVisible} animationType="fade" onRequestClose={() => setAktifEtVisible(false)}>
        <View className="flex-1 bg-black/60 items-center justify-center px-6">
          <View className="w-full rounded-3xl bg-white p-5">
            <View className="flex-row items-center justify-between mb-3">
              <Text className="text-[15px] font-extrabold text-[#1e293b]">Tekrar Aktif Et</Text>
              <TouchableOpacity onPress={() => setAktifEtVisible(false)} className="p-2">
                <MaterialCommunityIcons name="close" size={20} color="#64748b" />
              </TouchableOpacity>
            </View>
            <Text className="text-[#64748b] font-semibold mb-3">Puantaj yapılan bir kart mı?</Text>
            <TouchableOpacity className="flex-row items-center mb-2" onPress={() => setAktifEtPuantaj(true)}>
              <MaterialCommunityIcons name={aktifEtPuantaj ? "radiobox-marked" : "radiobox-blank"} size={22} color="#16a34a" />
              <Text className="ml-2 text-[#1e293b] font-semibold">Evet</Text>
            </TouchableOpacity>
            <TouchableOpacity className="flex-row items-center mb-4" onPress={() => setAktifEtPuantaj(false)}>
              <MaterialCommunityIcons name={!aktifEtPuantaj ? "radiobox-marked" : "radiobox-blank"} size={22} color="#16a34a" />
              <Text className="ml-2 text-[#1e293b] font-semibold">Hayır</Text>
            </TouchableOpacity>
            <View className="flex-row">
              <TouchableOpacity onPress={() => setAktifEtVisible(false)} className="flex-1 bg-[#f1f5f9] rounded-2xl py-3 items-center mr-2">
                <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
              </TouchableOpacity>
              <TouchableOpacity onPress={doTekrarAktifEt} disabled={acting} className="flex-1 bg-[#16a34a] rounded-2xl py-3 items-center">
                <Text className="text-white font-extrabold">{acting ? "İşleniyor..." : "Aktif et"}</Text>
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </Modal>

      <Modal transparent visible={istenVisible} animationType="fade" onRequestClose={() => setIstenVisible(false)}>
        <View className="flex-1 bg-black/60 items-center justify-center px-6">
          <View className="w-full rounded-3xl bg-white p-5">
            <View className="flex-row items-center justify-between mb-3">
              <Text className="text-[15px] font-extrabold text-[#1e293b]">İşten Çıkar</Text>
              <TouchableOpacity onPress={() => setIstenVisible(false)} className="p-2">
                <MaterialCommunityIcons name="close" size={20} color="#64748b" />
              </TouchableOpacity>
            </View>
            <Text className="text-[#64748b] font-semibold mb-2">Çıkış Tarihi</Text>
            <TouchableOpacity
              className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 mb-3 flex-row items-center justify-between"
              onPress={() => {
                // avoid modal-on-modal (iOS): close this modal, then open picker
                setIstenVisible(false);
                setPendingIstenPickerOpen(true);
              }}
            >
              <Text className="text-[#1e293b] font-semibold">{fmtDateTR(istenDateObj) || "-"}</Text>
              <MaterialCommunityIcons name="calendar" size={18} color="#64748b" />
            </TouchableOpacity>
            <Text className="text-[#64748b] font-semibold mb-2">Firma Dışı Kart No (opsiyonel)</Text>
            <TextInput className="bg-[#f8fafc] border border-[#e2e8f0] rounded-2xl px-4 py-3 mb-4" value={istenKartNo} onChangeText={setIstenKartNo} />
            <View className="flex-row">
              <TouchableOpacity onPress={() => setIstenVisible(false)} className="flex-1 bg-[#f1f5f9] rounded-2xl py-3 items-center mr-2">
                <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
              </TouchableOpacity>
              <TouchableOpacity onPress={doIstenCikar} disabled={acting} className="flex-1 bg-[#dc2626] rounded-2xl py-3 items-center">
                <Text className="text-white font-extrabold">{acting ? "İşleniyor..." : "İşten çıkar"}</Text>
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </Modal>

      <Modal
        transparent
        visible={istenPickerVisible}
        animationType="fade"
        onRequestClose={() => {
          setIstenPickerVisible(false);
          // restore parent modal for continuity
          setTimeout(() => setIstenVisible(true), 150);
        }}
      >
        <View className="flex-1 bg-black/60 items-center justify-center px-6">
          <View className="w-full rounded-3xl bg-white p-5">
            <View className="flex-row items-center justify-between mb-3">
              <Text className="text-[15px] font-extrabold text-[#1e293b]">Çıkış Tarihi Seç</Text>
              <TouchableOpacity
                onPress={() => {
                  setIstenPickerVisible(false);
                  setTimeout(() => setIstenVisible(true), 150);
                }}
                className="p-2"
              >
                <MaterialCommunityIcons name="close" size={20} color="#64748b" />
              </TouchableOpacity>
            </View>
            <View style={{ height: Platform.OS === "ios" ? 360 : 216, width: "100%", justifyContent: "center" }}>
              <DateTimePicker
                value={istenDateObj}
                mode="date"
                display={Platform.OS === "ios" ? "inline" : "default"}
                locale="tr-TR"
                themeVariant="light"
                textColor="#0f172a"
                style={{ width: "100%", height: Platform.OS === "ios" ? 360 : 216 }}
                onChange={(_, selected) => {
                  if (Platform.OS !== "ios") {
                    setIstenPickerVisible(false);
                    setTimeout(() => setIstenVisible(true), 150);
                  }
                  if (!selected) return;
                  const nd = normalizeDateOnly(selected);
                  setIstenDateObj(nd);
                  setIstenDate(fmtIsoDate(nd));
                }}
              />
            </View>
            {Platform.OS === "ios" ? (
              <TouchableOpacity
                onPress={() => {
                  setIstenPickerVisible(false);
                  setTimeout(() => setIstenVisible(true), 150);
                }}
                className="mt-4 bg-[#dc2626] rounded-2xl py-3 items-center"
              >
                <Text className="text-white font-extrabold">Tamam</Text>
              </TouchableOpacity>
            ) : null}
          </View>
        </View>
      </Modal>
    </View>
  );
}

