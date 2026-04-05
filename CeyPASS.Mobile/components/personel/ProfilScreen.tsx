import React, { useEffect, useMemo, useState } from "react";
import { Image, Modal, ScrollView, Text, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { PersonelHeader } from "./PersonelHeader";
import { profilService } from "../../services/profilApi";
import { ayarlarService } from "../../services/ayarlarApi";

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
  if (Number.isNaN(d.getTime())) return (v ?? "").toString();
  try {
    return d.toLocaleDateString("tr-TR");
  } catch {
    return d.toString();
  }
}

function vstr(v: any) {
  const s = (v ?? "").toString().trim();
  return s || "-";
}

function fmtDateTimeTRLoose(v: any) {
  if (!v) return "";
  const d = new Date(v);
  if (Number.isNaN(d.getTime())) return (v ?? "").toString();
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const yyyy = d.getFullYear();
  const hh = String(d.getHours()).padStart(2, "0");
  const mi = String(d.getMinutes()).padStart(2, "0");
  const ss = String(d.getSeconds()).padStart(2, "0");
  return `${dd}.${mm}.${yyyy} ${hh}:${mi}:${ss}`;
}

function pick(obj: any, ...keys: string[]) {
  for (const k of keys) {
    const v = obj?.[k as any];
    const s = (v ?? "").toString().trim();
    if (v !== undefined && v !== null && s !== "") return v;
  }
  return undefined;
}

function s(v: any): string {
  return (v ?? "").toString();
}
function parseCsv(v: any): number[] {
  const txt = s(v).trim();
  if (!txt) return [];
  return txt
    .split(",")
    .map((x) => Number(x.trim()))
    .filter((x) => Number.isFinite(x) && x > 0);
}

export function ProfilScreen(props: { user: any; onOpenMenu?: () => void; onNavigate?: (key: string) => void }) {
  const subtitle = useMemo(() => (props.user?.adSoyad ?? props.user?.AdSoyad ?? "").toString(), [props.user]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [profile, setProfile] = useState<any>(null);
  const [vardiyaModal, setVardiyaModal] = useState(false);
  const [movLoading, setMovLoading] = useState(false);
  const [movError, setMovError] = useState<string | null>(null);
  const [movItems, setMovItems] = useState<any[]>([]);
  const [movTotal, setMovTotal] = useState<number>(0);
  const [movPage, setMovPage] = useState<number>(1);
  const movPageSize = 10;
  const [vardiyaLoading, setVardiyaLoading] = useState(false);
  const [vardiyaMap, setVardiyaMap] = useState<Record<number, string>>({});

  useEffect(() => {
    let alive = true;
    (async () => {
      try {
        setLoading(true);
        setError(null);
        const res = await profilService.getProfil();
        if (!alive) return;
        if (res?.success) setProfile(res.data ?? null);
        else setError(res?.message || "Profil alınamadı.");
      } catch (e: any) {
        if (!alive) return;
        setError(e?.message || "Sunucuya bağlanılamadı.");
      } finally {
        if (alive) setLoading(false);
      }
    })();
    return () => {
      alive = false;
    };
  }, []);

  useEffect(() => {
    if (!profile) return;
    let alive = true;
    (async () => {
      try {
        setMovLoading(true);
        setMovError(null);
        const res = await profilService.hareketlerim(movPage, movPageSize);
        if (!alive) return;
        if (res?.success) {
          const data: any = res.data ?? {};
          const items = (data.items ?? data.Items ?? []) as any[];
          const total = Number(data.totalCount ?? data.TotalCount ?? 0) || 0;
          setMovItems(items);
          setMovTotal(total);
        } else {
          setMovItems([]);
          setMovTotal(0);
          setMovError(res?.message || "Hareketler alınamadı.");
        }
      } catch (e: any) {
        if (!alive) return;
        setMovItems([]);
        setMovTotal(0);
        setMovError(e?.message || "Hareketler alınamadı.");
      } finally {
        if (alive) setMovLoading(false);
      }
    })();
    return () => {
      alive = false;
    };
  }, [profile, movPage]);

  useEffect(() => {
    if (!profile) return;
    let alive = true;
    (async () => {
      try {
        setVardiyaLoading(true);
        const res = await ayarlarService.listVardiyalar();
        if (!alive) return;
        if (!res?.success) return;
        const items: any[] = (res.data ?? []) as any[];
        const map: Record<number, string> = {};
        for (const it of items) {
          const id = Number(it?.id ?? it?.Id ?? 0) || 0;
          const ad = (it?.ad ?? it?.Ad ?? "").toString().trim();
          if (id > 0 && ad) map[id] = ad;
        }
        setVardiyaMap(map);
      } catch {
        // silent
      } finally {
        if (alive) setVardiyaLoading(false);
      }
    })();
    return () => {
      alive = false;
    };
  }, [profile]);

  const p = profile?.personel ?? profile?.Personel ?? {};
  const sicilNo = vstr(pick(profile, "sicilNo", "SicilNo") ?? pick(props.user, "sicilNo", "SicilNo"));
  const ad = (pick(p, "Ad", "ad") ?? "").toString().trim();
  const soyad = (pick(p, "Soyad", "soyad") ?? "").toString().trim();
  const adSoyad = `${ad} ${soyad}`.trim() || vstr(pick(props.user, "adSoyad", "AdSoyad"));
  const tc = vstr(pick(p, "TcNo", "TCNo", "tcNo", "tCNo", "TCKimlikNo", "tcKimlikNo"));
  const kartNo = vstr(pick(p, "KartNo", "kartNo", "KartNumara", "kartNumara", "KartId", "kartId", "Kart", "kart"));
  const email = vstr(pick(p, "Email", "email"));
  const telefon = vstr(pick(p, "CepTel", "cepTel", "TelefonNo", "telefonNo", "Telefon", "telefon", "CepTelefonu", "cepTelefonu"));
  const dogumTarihi = vstr(fmtDateTR(pick(p, "DogumTarihi", "dogumTarihi")));
  const iseGiris = vstr(fmtDateTR(pick(p, "IseGirisTarihi", "iseGirisTarihi")));
  const departman = vstr(pick(profile, "departmanAdi", "DepartmanAdi") ?? pick(p, "DepartmanAdi", "departmanAdi"));
  const pozisyon = vstr(pick(profile, "pozisyonAdi", "PozisyonAdi") ?? pick(p, "PozisyonAdi", "pozisyonAdi"));
  const calismaStatu = vstr(pick(p, "CalismaStatusuText", "calismaStatusuText", "CalismaStatuAdi", "calismaStatuAdi", "CalismaStatu", "calismaStatu"));
  const vardiyaIds = parseCsv(pick(p, "CalismaSekliCsv", "calismaSekliCsv"));
  const vardiyaNames = useMemo(() => {
    const fromApi = (profile?.calismaSekliAdlari ?? profile?.CalismaSekliAdlari ?? []) as any;
    const apiNames = Array.isArray(fromApi) ? fromApi.map((x) => (x ?? "").toString().trim()).filter((x) => !!x) : [];
    if (apiNames.length) return apiNames;
    if (!vardiyaIds.length) return [];
    const names = vardiyaIds.map((id) => vardiyaMap[id]).filter((x) => !!x);
    if (names.length) return names;
    // fallback to ids as strings (still listable)
    return vardiyaIds.map((id) => String(id));
  }, [profile, vardiyaIds.join(","), vardiyaMap]);
  const vardiyaIsMulti = (vardiyaNames?.length ?? 0) > 1;
  const vardiyaRowValue = vardiyaLoading
    ? "Yükleniyor..."
    : vardiyaIsMulti
      ? "Çoklu Vardiya"
      : vardiyaNames.length === 1
        ? vardiyaNames[0]
        : "-";
  const avatarUri = (pick(profile, "fotografDataUrl", "FotografDataUrl") ?? pick(p, "FotografDataUrl", "fotografDataUrl") ?? "")
    .toString()
    .trim();
  const initial = (adSoyad.trim()[0]?.toLocaleUpperCase("tr-TR") ?? "?").toString();
  const yemekHakkiVar = profile?.yemekHakkiVar ?? profile?.YemekHakkiVar;
  const gunlukYemekAdedi = profile?.gunlukYemekAdedi ?? profile?.GunlukYemekAdedi;
  const hasPendingLeaves = !!(profile?.hasPendingLeaves ?? profile?.HasPendingLeaves);
  const totalPendingAdvances = Number(profile?.totalPendingAdvances ?? profile?.TotalPendingAdvances ?? 0) || 0;

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <PersonelHeader title="Profil" subtitle={subtitle} onOpenMenu={props.onOpenMenu} />

      <ScrollView className="flex-1 px-5 py-5" contentContainerStyle={{ paddingBottom: 24 }}>
        {loading ? (
          <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4">
            <Text className="text-[#64748b] font-semibold">Yükleniyor...</Text>
          </View>
        ) : error ? (
          <View className="bg-white rounded-2xl border border-[#fee2e2] p-4">
            <Text className="text-[#b91c1c] font-extrabold">Hata</Text>
            <Text className="text-[#7f1d1d] font-semibold mt-1">{error}</Text>
          </View>
        ) : (
          <>
            <View className="bg-white rounded-2xl border border-[#f1f5f9] p-4">
              <View className="flex-row items-center">
                <View className="w-[56px] h-[56px] rounded-full bg-[#f1f5f9] items-center justify-center overflow-hidden border border-[#e2e8f0]">
                  {avatarUri ? (
                    <Image source={{ uri: avatarUri }} style={{ width: 56, height: 56 }} />
                  ) : (
                    <Text className="text-[#0f172a] font-extrabold" style={{ fontSize: 22 }}>
                      {initial}
                    </Text>
                  )}
                </View>
                <View className="ml-3 flex-1">
                  <Text className="text-[#1e293b] font-extrabold text-[16px]" numberOfLines={1} ellipsizeMode="tail">
                    {adSoyad}
                  </Text>
                  <Text className="mt-1 text-[#64748b] font-semibold" numberOfLines={1} ellipsizeMode="tail">
                    {(pozisyon !== "-" ? pozisyon : departman) || " "}
                  </Text>
                </View>
              </View>
            </View>

            <View className="mt-4 bg-white rounded-2xl border border-[#f1f5f9] overflow-hidden">
              <Row label="Sicil No" value={sicilNo} />
              <Row label="TC Kimlik No" value={tc} />
              <Row label="Kart No" value={kartNo} />
              <Row label="E-Posta" value={email} />
              <Row label="Telefon" value={telefon} />
              <Row label="Doğum Tarihi" value={dogumTarihi} />
              <Row label="İşe Giriş Tarihi" value={iseGiris} />
              <Row label="Çalışma Statüsü" value={calismaStatu} />
              {vardiyaIsMulti && !vardiyaLoading ? (
                <RowPressable
                  label="Çalışma Şekilleri (Vardiyalar)"
                  value={vardiyaRowValue}
                  onPress={() => setVardiyaModal(true)}
                />
              ) : (
                <Row label="Çalışma Şekilleri (Vardiyalar)" value={vardiyaRowValue} />
              )}
              <Row label="Departman" value={departman} />
              <Row label="Pozisyon" value={pozisyon} />
              <Row label="Yemek Hakkı" value={yemekHakkiVar === true ? "Var" : yemekHakkiVar === false ? "Yok" : "-"} />
              <Row
                label="Günlük Yemek Adedi"
                value={gunlukYemekAdedi === null || gunlukYemekAdedi === undefined ? "-" : String(gunlukYemekAdedi)}
              />
            </View>

            <Modal transparent visible={vardiyaModal} animationType="fade" onRequestClose={() => setVardiyaModal(false)}>
              <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setVardiyaModal(false)}>
                <TouchableOpacity activeOpacity={1} onPress={() => {}}>
                  <View className="bg-white rounded-2xl overflow-hidden">
                    <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                      <Text className="text-[#0f172a] font-extrabold text-[16px]">Vardiyalar</Text>
                      <TouchableOpacity onPress={() => setVardiyaModal(false)} className="p-2">
                        <MaterialCommunityIcons name="close" size={20} color="#64748b" />
                      </TouchableOpacity>
                    </View>
                    <ScrollView style={{ maxHeight: 420 }}>
                      {vardiyaNames.map((name, idx) => (
                        <View key={`${name}_${idx}`} className="px-4 py-3 border-b border-[#f1f5f9]">
                          <Text className="text-[#0f172a] font-semibold">{name}</Text>
                        </View>
                      ))}
                    </ScrollView>
                    <View className="p-3">
                      <TouchableOpacity className="bg-[#f1f5f9] rounded-xl py-3 items-center" onPress={() => setVardiyaModal(false)}>
                        <Text className="text-[#334155] font-extrabold">Kapat</Text>
                      </TouchableOpacity>
                    </View>
                  </View>
                </TouchableOpacity>
              </TouchableOpacity>
            </Modal>

            <View className="mt-4 flex-row">
              <TouchableOpacity
                onPress={() => props.onNavigate?.("izinlerim")}
                className="flex-1 bg-[#f1f5f9] rounded-2xl px-4 py-3 flex-row items-center justify-center"
              >
                <MaterialCommunityIcons name="calendar-check" size={18} color="#334155" />
                <Text className="ml-2 text-[#334155] font-extrabold">İzinlerim</Text>
              </TouchableOpacity>
              <View className="w-3" />
              <TouchableOpacity
                onPress={() => props.onNavigate?.("avanslarim")}
                className="flex-1 bg-[#f1f5f9] rounded-2xl px-4 py-3 flex-row items-center justify-center"
              >
                <MaterialCommunityIcons name="eye" size={18} color="#334155" />
                <Text className="ml-2 text-[#334155] font-extrabold">Avanslarım</Text>
              </TouchableOpacity>
            </View>

            <View className="mt-4 bg-white rounded-2xl border border-[#f1f5f9] p-4">
              <Text className="text-[#1e293b] font-extrabold text-[14px]">Durum</Text>
              <View className="mt-3">
                <KV label="Bekleyen İzin" value={hasPendingLeaves ? "Var" : "Yok"} />
                <KV label="Bekleyen Avans" value={`${totalPendingAdvances}`} />
              </View>
            </View>

            <View className="mt-4 bg-white rounded-2xl border border-[#f1f5f9] overflow-hidden">
              <View className="px-4 py-3 flex-row items-center justify-between">
                <Text className="text-[#1e293b] font-extrabold text-[14px]">Giriş-Çıkış Hareketlerim (Son 7 Gün)</Text>
                <Text className="text-[#64748b] font-extrabold">{movTotal ? `${movTotal}` : ""}</Text>
              </View>
              <View className="px-4 pb-3 flex-row items-center justify-between">
                <TouchableOpacity
                  disabled={movPage <= 1 || movLoading}
                  onPress={() => setMovPage((p0) => Math.max(1, p0 - 1))}
                  className={`px-3 py-2 rounded-xl ${movPage <= 1 || movLoading ? "bg-[#f1f5f9]" : "bg-[#fee2e2]"}`}
                >
                  <Text className={`font-extrabold ${movPage <= 1 || movLoading ? "text-[#94a3b8]" : "text-[#b91c1c]"}`}>
                    Önceki
                  </Text>
                </TouchableOpacity>
                <Text className="text-[#64748b] font-extrabold">
                  Sayfa {movPage} / {Math.max(1, Math.ceil(movTotal / movPageSize))}
                </Text>
                <TouchableOpacity
                  disabled={movLoading || movPage >= Math.max(1, Math.ceil(movTotal / movPageSize))}
                  onPress={() => setMovPage((p0) => p0 + 1)}
                  className={`px-3 py-2 rounded-xl ${
                    movLoading || movPage >= Math.max(1, Math.ceil(movTotal / movPageSize)) ? "bg-[#f1f5f9]" : "bg-[#fee2e2]"
                  }`}
                >
                  <Text
                    className={`font-extrabold ${
                      movLoading || movPage >= Math.max(1, Math.ceil(movTotal / movPageSize)) ? "text-[#94a3b8]" : "text-[#b91c1c]"
                    }`}
                  >
                    Sonraki
                  </Text>
                </TouchableOpacity>
              </View>
              <View className="border-t border-[#f1f5f9]" />
              {movLoading ? (
                <View className="px-4 py-4">
                  <Text className="text-[#64748b] font-semibold">Yükleniyor...</Text>
                </View>
              ) : movError ? (
                <View className="px-4 py-4">
                  <Text className="text-[#b91c1c] font-extrabold">Hata</Text>
                  <Text className="text-[#7f1d1d] font-semibold mt-1">{movError}</Text>
                </View>
              ) : movItems.length ? (
                <View>
                  {movItems.slice(0, 10).map((it: any, idx: number) => {
                    const dt = fmtDateTimeTRLoose(it?.tarih ?? it?.Tarih ?? it?.TarihSaat ?? it?.tarihSaat ?? it?.date ?? it?.Date);
                    return (
                      <View key={`${it?.id ?? it?.Id ?? idx}`} className="px-4 py-3 border-t border-[#f1f5f9]">
                        <Text className="text-[#0f172a] font-extrabold">{dt || "-"}</Text>
                        {(it?.yon ?? it?.Yon ?? it?.tip ?? it?.Tip) ? (
                          <Text className="mt-1 text-[#64748b] font-semibold" numberOfLines={1}>
                            {(it?.yon ?? it?.Yon ?? it?.tip ?? it?.Tip ?? "").toString()}
                          </Text>
                        ) : null}
                      </View>
                    );
                  })}
                </View>
              ) : (
                <View className="px-4 py-4">
                  <Text className="text-[#64748b] font-semibold">Kayıt bulunamadı.</Text>
                </View>
              )}
            </View>
          </>
        )}
      </ScrollView>
    </View>
  );
}

function Row(props: { label: string; value: string }) {
  return (
    <View className="flex-row items-center justify-between px-4 py-3 border-t border-[#f1f5f9]">
      <Text className="text-[#64748b] font-semibold">{props.label}</Text>
      <Text className="text-[#1e293b] font-extrabold ml-3" numberOfLines={1} ellipsizeMode="tail">
        {props.value}
      </Text>
    </View>
  );
}

function RowPressable(props: { label: string; value: string; onPress: () => void }) {
  return (
    <TouchableOpacity
      onPress={props.onPress}
      className="flex-row items-center justify-between px-4 py-3 border-t border-[#f1f5f9]"
      activeOpacity={0.8}
    >
      <Text className="text-[#64748b] font-semibold">{props.label}</Text>
      <View className="flex-row items-center ml-3 flex-1 justify-end">
        <Text className="text-[#1e293b] font-extrabold" numberOfLines={1} ellipsizeMode="tail">
          {props.value}
        </Text>
        <MaterialCommunityIcons name="chevron-right" size={20} color="#94a3b8" />
      </View>
    </TouchableOpacity>
  );
}

function KV(props: { label: string; value: string }) {
  return (
    <View className="flex-row items-center justify-between py-2">
      <Text className="text-[#64748b] font-semibold">{props.label}</Text>
      <Text className="text-[#1e293b] font-extrabold ml-3">{props.value}</Text>
    </View>
  );
}

