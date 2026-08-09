import React, { useEffect, useMemo, useState } from "react";
import {
  ActivityIndicator,
  Image,
  ImageBackground,
  Modal,
  ScrollView,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { LinearGradient } from "expo-linear-gradient";
import { GlassCard } from "./GlassCard";
import { CustomInput } from "./CustomInput";
import { CustomButton } from "./CustomButton";
import { canliIzlemeAuth, canliIzlemeData, type FirmaOption } from "../services/canliIzlemeApi";
import { LoginBackground } from "../services/preload";
import { CanliIzlemeKartModal } from "./CanliIzlemeKartModal";

type Props = { onClose: () => void };

type KartModalState = {
  visible: boolean;
  kind: "misafir" | "arac";
  mode: "yeni" | "guncelle";
} | null;

function canShowKartButtons(authUser: any): boolean {
  const rol = (authUser?.rol ?? authUser?.Rol ?? "").toString();
  const isYemekhane = rol.toUpperCase() === "YEMEKHANE";
  const isDanisma =
    rol.toLocaleUpperCase("tr-TR").includes("DANIŞMA") ||
    rol.toUpperCase().includes("DANISMA");
  return !(isYemekhane && !isDanisma);
}

export function CanliIzlemeScreen({ onClose }: Props) {
  const [step, setStep] = useState<"login" | "live">("login");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [firmalar, setFirmalar] = useState<FirmaOption[]>([]);
  const [firmalarLoading, setFirmalarLoading] = useState(false);
  const [firmalarLoadedOnce, setFirmalarLoadedOnce] = useState(false);
  const [firmaId, setFirmaId] = useState<number | null>(null);

  const [kullanicilar, setKullanicilar] = useState<string[]>([]);
  const [kullaniciAdi, setKullaniciAdi] = useState("");
  const [sifre, setSifre] = useState("");

  const [firmaModal, setFirmaModal] = useState(false);
  const [kulModal, setKulModal] = useState(false);

  const [token, setToken] = useState<string | null>(null);
  const [authUser, setAuthUser] = useState<any | null>(null);
  const [refreshing, setRefreshing] = useState(false);
  const [sonGecisler, setSonGecisler] = useState<any[]>([]);
  const [sonHareketler, setSonHareketler] = useState<any[]>([]);

  const [selectedVisible, setSelectedVisible] = useState(false);
  const [selectedLoading, setSelectedLoading] = useState(false);
  const [selectedError, setSelectedError] = useState<string | null>(null);
  const [selectedDetail, setSelectedDetail] = useState<any | null>(null);
  const [kartModal, setKartModal] = useState<KartModalState>(null);
  const [kartToast, setKartToast] = useState<string | null>(null);

  const selectedFirmaName = useMemo(
    () => firmalar.find((f) => f.id === firmaId)?.ad ?? "Firma Seçin",
    [firmalar, firmaId]
  );

  const initials = useMemo(() => {
    const name = (authUser?.adSoyad ?? authUser?.AdSoyad ?? "").toString().trim();
    if (!name) return "CI";
    const parts = name.split(/\s+/).filter(Boolean);
    const a = parts[0]?.[0] ?? "";
    const b = parts.length > 1 ? parts[parts.length - 1]?.[0] ?? "" : "";
    return (a + b).toUpperCase();
  }, [authUser]);

  const fetchFirmalar = async () => {
    if (firmalarLoading) return;
    setFirmalarLoading(true);
    setError(null);
    try {
      const res = await canliIzlemeAuth.firmalar();
      if (res.success && res.data) setFirmalar(res.data);
      else {
        setFirmalar([]);
        setError(res.message ?? "Firmalar yüklenemedi.");
      }
      setFirmalarLoadedOnce(true);
    } catch (e: any) {
      setFirmalar([]);
      setFirmalarLoadedOnce(true);
      setError(e?.response?.data?.message ?? e?.message ?? "Firmalar yüklenemedi.");
    } finally {
      setFirmalarLoading(false);
    }
  };

  useEffect(() => {
    fetchFirmalar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (!firmaId) return;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const res = await canliIzlemeAuth.kullanicilar(firmaId);
        if (res.success && res.data) setKullanicilar(res.data);
        else setKullanicilar([]);
      } catch (e: any) {
        setKullanicilar([]);
        setError(e?.response?.data?.message ?? e?.message ?? "Kullanıcılar yüklenemedi.");
      } finally {
        setLoading(false);
      }
    })();
  }, [firmaId]);

  const login = async () => {
    if (!firmaId) return setError("Firma seçin.");
    if (!kullaniciAdi.trim()) return setError("Kullanıcı adı boş olamaz.");
    if (!sifre) return setError("Şifre boş olamaz.");
    setLoading(true);
    setError(null);
    try {
      const res = await canliIzlemeAuth.login(firmaId, kullaniciAdi.trim(), sifre);
      if (!res.success || !res.data?.token) {
        setError(res.message ?? "Giriş başarısız.");
        return;
      }
      setToken(res.data.token);
      setAuthUser(res.data.user);
      setStep("live");
    } catch (e: any) {
      setError(e?.response?.data?.message ?? e?.message ?? "Giriş başarısız.");
    } finally {
      setLoading(false);
    }
  };

  const fetchLive = async () => {
    if (!token) return;
    setRefreshing(true);
    try {
      const [p, m] = await Promise.all([
        canliIzlemeData.sonGecisler(token, 4),
        canliIzlemeData.sonHareketler(token, 10),
      ]);
      if (p.success && p.data) setSonGecisler(p.data);
      if (m.success && m.data) setSonHareketler(m.data);
    } finally {
      setRefreshing(false);
    }
  };

  useEffect(() => {
    if (step !== "live" || !token) return;
    fetchLive();
    const t = setInterval(fetchLive, 2000);
    return () => clearInterval(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [step, token]);

  const openSelected = async (kisiId: number) => {
    if (!token) return;
    if (!Number.isFinite(kisiId) || kisiId <= 0) {
      setSelectedVisible(true);
      setSelectedLoading(false);
      setSelectedDetail(null);
      setSelectedError("Kişi bilgisi alınamadı (kisiId bulunamadı).");
      return;
    }
    setSelectedVisible(true);
    setSelectedLoading(true);
    setSelectedError(null);
    setSelectedDetail(null);
    try {
      const res = await canliIzlemeData.kisiDetay(token, kisiId);
      if (!res.success) setSelectedError(res.message ?? "Kişi bilgisi alınamadı.");
      else setSelectedDetail(res.data);
    } catch (e: any) {
      setSelectedError(e?.response?.data?.message ?? e?.message ?? "Kişi bilgisi alınamadı.");
    } finally {
      setSelectedLoading(false);
    }
  };

  const logout = () => {
    setSelectedVisible(false);
    setSelectedDetail(null);
    setSelectedError(null);
    setToken(null);
    setAuthUser(null);
    setSonGecisler([]);
    setSonHareketler([]);
    setStep("login");
    setSifre("");
  };

  const close = () => {
    setToken(null);
    setAuthUser(null);
    onClose();
  };

  const Header = (
    <View
      className={`px-5 pt-12 pb-4 flex-row items-center justify-between ${
        step === "login" ? "bg-transparent" : "bg-white border-b border-[#f1f5f9]"
      }`}
      style={{ zIndex: 20 }}
    >
      <View className="flex-row items-center">
        <View
          className={`w-10 h-10 rounded-xl items-center justify-center mr-3 ${
            step === "login" ? "bg-white/20" : "bg-[#fee2e2]"
          }`}
        >
          <MaterialCommunityIcons
            name="broadcast"
            size={20}
            color={step === "login" ? "white" : "#dc2626"}
          />
        </View>
        <View>
          <Text className={`text-[16px] font-extrabold ${step === "login" ? "text-white" : "text-[#1e293b]"}`}>
            Canlı İzleme
          </Text>
          <Text className={`text-[11px] font-semibold ${step === "login" ? "text-white/80" : "text-[#64748b]"}`}>
            {step === "login" ? "Giriş" : "Canlı"}
          </Text>
        </View>
      </View>
      <TouchableOpacity onPress={close} className="p-2">
        <MaterialCommunityIcons name="close" size={22} color={step === "login" ? "white" : "#64748b"} />
      </TouchableOpacity>
    </View>
  );

  if (step === "login") {
    return (
      <ImageBackground
        source={LoginBackground}
        style={{ flex: 1, backgroundColor: "#991b1b" }}
        resizeMode="cover"
      >
        <LinearGradient colors={["rgba(0,0,0,0.55)", "rgba(0,0,0,0.4)"]} style={{ flex: 1 }}>
          {Header}
          <View pointerEvents="none" className="absolute top-[-12%] right-[-18%] w-[520px] h-[520px] bg-white/10 rounded-full" />
          <View pointerEvents="none" className="absolute bottom-[-10%] left-[-18%] w-[380px] h-[380px] bg-white/10 rounded-full" />

          <ScrollView
            className="flex-1 px-5"
            contentContainerStyle={{ flexGrow: 1, justifyContent: "center", paddingBottom: 40 }}
            showsVerticalScrollIndicator={false}
          >
            <GlassCard>
              {error ? (
                <View className="mb-4 rounded-xl bg-[#fef2f2] px-4 py-3 border border-[#fee2e2]">
                  <Text className="text-[#dc2626] font-semibold text-[12px]">{error}</Text>
                </View>
              ) : null}

              <Text className="text-[13px] font-semibold text-[#1e293b] mb-2 ml-1">Firma</Text>
              <TouchableOpacity
                onPress={async () => {
                  setFirmaModal(true);
                  if (!firmalarLoadedOnce || firmalar.length === 0) await fetchFirmalar();
                }}
                className="border-[2px] rounded-xl px-4 py-4 bg-[#f8fafc] border-[#e2e8f0] flex-row items-center justify-between"
                activeOpacity={0.8}
              >
                <Text className="text-[#1e293b] font-semibold">{selectedFirmaName}</Text>
                <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" />
              </TouchableOpacity>

              <View className="mt-5" />

              <Text className="text-[13px] font-semibold text-[#1e293b] mb-2 ml-1">Kullanıcı</Text>
              <TouchableOpacity
                onPress={() => setKulModal(true)}
                className="border-[2px] rounded-xl px-4 py-4 bg-[#f8fafc] border-[#e2e8f0] flex-row items-center justify-between"
                activeOpacity={0.8}
                disabled={!firmaId || kullanicilar.length === 0}
              >
                <Text className="text-[#1e293b] font-semibold">
                  {kullaniciAdi ? kullaniciAdi : firmaId ? "Kullanıcı seçin" : "Önce firma seçin"}
                </Text>
                <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" />
              </TouchableOpacity>

              <View className="mt-4" />
              <CustomInput
                label="Şifre"
                placeholder="Şifrenizi girin"
                value={sifre}
                onChangeText={setSifre}
                secureTextEntry
                icon="lock"
              />

              <CustomButton title="GİRİŞ YAP" onPress={login} loading={loading} className="mt-2" />

              {loading ? (
                <View className="mt-3 items-center">
                  <ActivityIndicator color="#dc2626" />
                </View>
              ) : null}
            </GlassCard>
          </ScrollView>

          <Modal transparent visible={firmaModal} animationType="fade">
            <View className="flex-1 bg-black/60 items-center justify-center px-6">
              <View className="w-full rounded-3xl bg-white p-5 max-h-[70%]">
                <View className="flex-row items-center justify-between mb-3">
                  <Text className="text-[15px] font-extrabold text-[#1e293b]">Firma Seç</Text>
                  <TouchableOpacity onPress={() => setFirmaModal(false)} className="p-2">
                    <MaterialCommunityIcons name="close" size={20} color="#64748b" />
                  </TouchableOpacity>
                </View>
                <ScrollView>
                  {firmalarLoading ? (
                    <View className="py-10 items-center">
                      <ActivityIndicator color="#dc2626" />
                      <Text className="mt-3 text-[#64748b] font-semibold">Yükleniyor...</Text>
                    </View>
                  ) : firmalar.length > 0 ? (
                    firmalar.map((f) => (
                      <TouchableOpacity
                        key={f.id}
                        onPress={() => {
                          setFirmaId(f.id);
                          setKullaniciAdi("");
                          setKullanicilar([]);
                          setFirmaModal(false);
                        }}
                        className="py-4 border-b border-[#f1f5f9]"
                      >
                        <Text className="font-semibold text-[#1e293b]">{f.ad}</Text>
                      </TouchableOpacity>
                    ))
                  ) : (
                    <View className="py-10 items-center">
                      <Text className="text-[#64748b] font-semibold">Firma bulunamadı.</Text>
                      <TouchableOpacity
                        className="mt-4 px-4 py-3 rounded-xl bg-[#fee2e2]"
                        onPress={fetchFirmalar}
                        activeOpacity={0.8}
                      >
                        <Text className="text-[#dc2626] font-extrabold">Tekrar Dene</Text>
                      </TouchableOpacity>
                    </View>
                  )}
                </ScrollView>
              </View>
            </View>
          </Modal>

          <Modal transparent visible={kulModal} animationType="fade">
            <View className="flex-1 bg-black/60 items-center justify-center px-6">
              <View className="w-full rounded-3xl bg-white p-5 max-h-[70%]">
                <View className="flex-row items-center justify-between mb-3">
                  <Text className="text-[15px] font-extrabold text-[#1e293b]">Kullanıcı Seç</Text>
                  <TouchableOpacity onPress={() => setKulModal(false)} className="p-2">
                    <MaterialCommunityIcons name="close" size={20} color="#64748b" />
                  </TouchableOpacity>
                </View>
                <ScrollView>
                  {kullanicilar.map((k) => (
                    <TouchableOpacity
                      key={k}
                      onPress={() => {
                        setKullaniciAdi(k);
                        setKulModal(false);
                      }}
                      className="py-4 border-b border-[#f1f5f9]"
                    >
                      <Text className="font-semibold text-[#1e293b]">{k}</Text>
                    </TouchableOpacity>
                  ))}
                </ScrollView>
              </View>
            </View>
          </Modal>
        </LinearGradient>
      </ImageBackground>
    );
  }

  return (
    <View className="flex-1 bg-[#f8fafc]">
      {Header}
      <ScrollView className="flex-1" contentContainerStyle={{ paddingBottom: 30 }}>
        <View className="px-5 pt-5">
          <View className="flex-row items-center justify-between mb-4">
            <View className="flex-row items-center">
              <View className="w-11 h-11 rounded-full bg-[#1e293b] items-center justify-center mr-3">
                <Text className="text-white font-extrabold">{initials}</Text>
              </View>
              <View>
                <Text className="text-[#1e293b] font-extrabold">
                  {(authUser?.adSoyad ?? authUser?.AdSoyad ?? "Kullanıcı").toString()}
                </Text>
                <Text className="text-[#64748b] font-semibold text-[11px]">{selectedFirmaName}</Text>
              </View>
            </View>
            <TouchableOpacity onPress={logout} className="px-3 py-2 rounded-xl bg-[#fee2e2]" activeOpacity={0.85}>
              <Text className="text-[#dc2626] font-extrabold text-[11px]">ÇIKIŞ</Text>
            </TouchableOpacity>
          </View>

          {canShowKartButtons(authUser) ? (
            <View className="flex-row flex-wrap justify-between mb-4">
              {(
                [
                  { kind: "misafir" as const, mode: "yeni" as const, label: "Misafir Kart Ver", bg: "#dbeafe", fg: "#1d4ed8" },
                  { kind: "misafir" as const, mode: "guncelle" as const, label: "Misafir Güncelle", bg: "#e2e8f0", fg: "#334155" },
                  { kind: "arac" as const, mode: "yeni" as const, label: "Araç Kartı Ver", bg: "#ffedd5", fg: "#c2410c" },
                  { kind: "arac" as const, mode: "guncelle" as const, label: "Araç Güncelle", bg: "#ffedd5", fg: "#9a3412" },
                ] as const
              ).map((b) => (
                <TouchableOpacity
                  key={`${b.kind}-${b.mode}`}
                  style={{ width: "48%", backgroundColor: b.bg }}
                  className="mb-2 rounded-xl px-3 py-3"
                  activeOpacity={0.85}
                  onPress={() => setKartModal({ visible: true, kind: b.kind, mode: b.mode })}
                >
                  <Text style={{ color: b.fg }} className="font-extrabold text-[11px] text-center">
                    {b.label}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>
          ) : null}

          {kartToast ? (
            <View className="mb-3 rounded-xl bg-[#ecfdf5] px-4 py-3 border border-[#a7f3d0]">
              <Text className="text-[#047857] font-semibold text-[12px]">{kartToast}</Text>
            </View>
          ) : null}

          <View className="flex-row items-center justify-between mb-3">
            <Text className="text-[15px] font-extrabold text-[#1e293b]">Son Geçişler</Text>
            {refreshing ? <ActivityIndicator color="#dc2626" /> : null}
          </View>

          <View className="flex-row flex-wrap justify-between">
            {sonGecisler.map((p, idx) => (
              <View key={idx} style={{ width: "48%" }} className="mb-3">
                <TouchableOpacity activeOpacity={0.85} onPress={() => openSelected(Number(p.personelId))}>
                  <GlassCard className="p-4">
                    <View className="items-center">
                      <Image
                        source={{
                          uri: p.fotoBase64
                            ? `data:image/jpeg;base64,${p.fotoBase64}`
                            : "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 240'%3E%3Crect width='200' height='240' fill='%23f1f5f9'/%3E%3Ccircle cx='100' cy='80' r='40' fill='%23cbd5e1'/%3E%3Cellipse cx='100' cy='195' rx='60' ry='45' fill='%23cbd5e1'/%3E%3C/svg%3E",
                        }}
                        style={{ width: 72, height: 72, borderRadius: 36, marginBottom: 10 }}
                      />
                      <Text className="text-[#1e293b] font-extrabold text-[12px]" numberOfLines={2}>
                        {p.adSoyad}
                      </Text>
                      <Text className="text-[#64748b] font-semibold text-[10px]" numberOfLines={1}>
                        {p.terminalAdi}
                      </Text>
                      <View
                        className="mt-2 px-3 py-1 rounded-full"
                        style={{ backgroundColor: p.girisMi ? "#dcfce7" : "#fee2e2" }}
                      >
                        <Text style={{ color: p.girisMi ? "#15803d" : "#dc2626" }} className="font-extrabold text-[10px]">
                          {p.girisMi ? "GİRİŞ" : "ÇIKIŞ"}
                        </Text>
                      </View>
                    </View>
                  </GlassCard>
                </TouchableOpacity>
              </View>
            ))}
          </View>

          <Text className="text-[15px] font-extrabold text-[#1e293b] mt-4 mb-3">Son Hareketler</Text>
          <View className="bg-white rounded-2xl border border-[#f1f5f9] overflow-hidden">
            {sonHareketler.map((m, idx) => (
              <TouchableOpacity
                key={idx}
                activeOpacity={0.8}
                onPress={() => {
                  const kisiId = Number((m.personelId ?? m.kisiId ?? m.PersonelId) as any);
                  openSelected(kisiId);
                }}
                className="px-4 py-3 border-b border-[#f1f5f9]"
              >
                <View className="flex-row items-center justify-between">
                  <Text className="text-[#1e293b] font-semibold" numberOfLines={1} style={{ flex: 1 }}>
                    {m.adSoyad}
                  </Text>
                  <Text className="text-[#64748b] font-semibold text-[11px] ml-3">{m.cihazAdi}</Text>
                </View>
                <Text className="text-[#94a3b8] text-[11px] font-semibold mt-1">
                  {new Date(m.tarih).toLocaleString("tr-TR")}
                </Text>
              </TouchableOpacity>
            ))}
          </View>
        </View>
      </ScrollView>

      <Modal transparent visible={selectedVisible} animationType="fade" onRequestClose={() => setSelectedVisible(false)}>
        <View className="flex-1 bg-black/60 items-center justify-end px-4 pb-6">
          <View className="w-full rounded-3xl bg-white overflow-hidden">
            <View className="px-5 py-4 flex-row items-center justify-between border-b border-[#f1f5f9]">
              <Text className="text-[#1e293b] font-extrabold text-[15px]">Seçili Kişi Bilgileri</Text>
              <TouchableOpacity onPress={() => setSelectedVisible(false)} className="p-2">
                <MaterialCommunityIcons name="close" size={20} color="#64748b" />
              </TouchableOpacity>
            </View>

            <View className="px-5 py-5">
              {selectedLoading ? (
                <View className="py-6 items-center">
                  <ActivityIndicator color="#dc2626" />
                  <Text className="mt-3 text-[#64748b] font-semibold">Yükleniyor...</Text>
                </View>
              ) : selectedError ? (
                <View className="rounded-xl bg-[#fef2f2] px-4 py-3 border border-[#fee2e2]">
                  <Text className="text-[#dc2626] font-semibold text-[12px]">{selectedError}</Text>
                </View>
              ) : selectedDetail ? (
                <>
                  <View className="items-center">
                    <Image
                      source={{
                        uri: selectedDetail.fotoBase64
                          ? `data:image/jpeg;base64,${selectedDetail.fotoBase64}`
                          : "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 240'%3E%3Crect width='200' height='240' fill='%23f1f5f9'/%3E%3Ccircle cx='100' cy='80' r='40' fill='%23cbd5e1'/%3E%3Cellipse cx='100' cy='195' rx='60' ry='45' fill='%23cbd5e1'/%3E%3C/svg%3E",
                      }}
                      style={{
                        width: 120,
                        height: 120,
                        borderRadius: 60,
                        marginBottom: 14,
                        borderWidth: 4,
                        borderColor: "#6366f1",
                      }}
                    />
                    <Text className="text-[#1e293b] font-extrabold text-[18px]" numberOfLines={2}>
                      {selectedDetail.adSoyad}
                    </Text>
                  </View>

                  <View className="mt-5">
                    <View className="flex-row items-center p-4 rounded-2xl bg-[#f1f5f9] mb-3">
                      <View
                        className="w-10 h-10 rounded-xl items-center justify-center mr-3"
                        style={{ backgroundColor: "#3b82f6" }}
                      >
                        <MaterialCommunityIcons name="briefcase" size={18} color="white" />
                      </View>
                      <View className="flex-1">
                        <Text className="text-[#64748b] font-semibold text-[11px]">Pozisyon / Ünvan</Text>
                        <Text className="text-[#334155] font-extrabold">{selectedDetail.unvan ?? "-"}</Text>
                      </View>
                    </View>

                    <View className="flex-row items-center p-4 rounded-2xl bg-[#f1f5f9]">
                      <View
                        className="w-10 h-10 rounded-xl items-center justify-center mr-3"
                        style={{ backgroundColor: "#10b981" }}
                      >
                        <MaterialCommunityIcons name="sitemap" size={18} color="white" />
                      </View>
                      <View className="flex-1">
                        <Text className="text-[#64748b] font-semibold text-[11px]">Departman</Text>
                        <Text className="text-[#334155] font-extrabold">{selectedDetail.departman ?? "-"}</Text>
                      </View>
                    </View>
                  </View>
                </>
              ) : (
                <Text className="text-[#64748b] font-semibold">Kişi seçilmedi.</Text>
              )}
            </View>
          </View>
        </View>
      </Modal>

      {token && kartModal ? (
        <CanliIzlemeKartModal
          visible={kartModal.visible}
          token={token}
          kind={kartModal.kind}
          mode={kartModal.mode}
          onClose={() => setKartModal(null)}
          onSaved={(msg) => {
            setKartToast(msg);
            setTimeout(() => setKartToast(null), 3000);
          }}
        />
      ) : null}
    </View>
  );
}

