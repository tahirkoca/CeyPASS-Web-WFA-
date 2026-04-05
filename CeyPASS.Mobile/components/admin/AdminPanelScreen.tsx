import React, { useEffect, useMemo, useRef, useState } from "react";
import { ActivityIndicator, Modal, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { StatusPopup } from "../StatusPopup";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";
import { adminService, AdminPanelDto, GuncellemeNotifikasyonDTO } from "../../services/adminApi";
import { WebView } from "react-native-webview";

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

function normalizeDateOnly(d: Date) {
  const x = new Date(d);
  x.setHours(12, 0, 0, 0);
  return x;
}

function fmtIsoDate(d: Date) {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

function fmtDateTimeTRLoose(value: any) {
  if (!value) return "";
  const s = String(value);
  const d = new Date(s);
  if (!Number.isNaN(d.getTime())) {
    return d.toLocaleString("tr-TR", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    });
  }
  // Fallback: remove ISO "T"
  return s.replace("T", " ");
}

function getApiErrorMessage(e: any): string {
  const data = e?.response?.data;
  if (typeof data === "string" && data.trim()) return data;
  if (data?.message) return String(data.message);
  if (data?.Message) return String(data.Message);
  if (data?.error) return String(data.error);
  if (e?.message) return String(e.message);
  return "Hata oluştu.";
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

type TabKey =
  | "quick"
  | "guncellememail"
  | "kullanicilar"
  | "ustyetkili";

export function AdminPanelScreen(props: {
  user: any;
  abilities: any;
  onOpenMenu: () => void;
  onNavigate: (key: string) => void;
}) {
  const rolIdRaw: any = props.abilities?.rolId ?? props.abilities?.RolId ?? props.user?.rolId ?? props.user?.RolId ?? props.user?.rolID ?? props.user?.RolID;
  const rolIdNum = rolIdRaw == null ? NaN : Number(rolIdRaw);
  const rolId: number | null = Number.isFinite(rolIdNum) ? rolIdNum : null;
  const isAdmin = !!(props.abilities?.isAdmin ?? props.abilities?.IsAdmin ?? (rolId === 1 || rolId === 2));
  const isSuperAdmin = rolId === 1;

  const [tab, setTab] = useState<TabKey>("quick");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [data, setData] = useState<AdminPanelDto | null>(null);
  const scrollRef = useRef<ScrollView>(null);

  const [popupVisible, setPopupVisible] = useState(false);
  const [popupType, setPopupType] = useState<"success" | "error">("success");
  const [popupMessage, setPopupMessage] = useState("");
  const showPopup = (type: "success" | "error", message: string) => {
    setPopupType(type);
    setPopupMessage(message);
    setPopupVisible(true);
  };

  const refresh = async () => {
    setLoading(true);
    setError(null);
    try {
      const resp = await adminService.panel();
      if (!resp?.success) throw new Error(resp?.message || "Veri alınamadı.");
      setData(resp.data ?? null);
    } catch (e: any) {
      setError(e?.message || "Hata oluştu.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (isSuperAdmin) refresh();
    else {
      setLoading(false);
      setError("Admin paneline erişim yetkiniz yok.");
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const forceScrollTop = () => {
    try {
      scrollRef.current?.scrollTo({ y: 0, animated: false });
    } catch {}
  };

  useEffect(() => {
    // iOS sometimes restores previous scroll offset; force top (multi-shot).
    const t1 = setTimeout(forceScrollTop, 0);
    const t2 = setTimeout(forceScrollTop, 50);
    const t3 = setTimeout(forceScrollTop, 150);
    return () => {
      clearTimeout(t1);
      clearTimeout(t2);
      clearTimeout(t3);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tab, loading]);

  const personelItems = useMemo(() => {
    const list = (data?.personeller ?? []).map((p: any) => ({
      key: String(p.id ?? p.Id),
      label: `${p.id ?? p.Id} - ${(p.adSoyad ?? p.AdSoyad ?? "").toString()}`,
    }));
    return [{ key: "", label: "(Boşalt)" }, ...list];
  }, [data?.personeller]);

  const kullaniciRows = useMemo(() => {
    const rows = (data?.kullanicilar ?? []).map((u: any) => ({
      kullaniciId: asInt(u.kullaniciId ?? u.KullaniciId, 0),
      kullaniciAdi: (u.kullaniciAdi ?? u.KullaniciAdi ?? "").toString(),
      rol: (u.rolTanimi ?? u.RolTanimi ?? (u.rolId ?? u.RolId ?? "")).toString() || "-",
      personelId: u.personelId ?? u.PersonelId ?? null,
    }));
    return rows.sort((a, b) => a.kullaniciAdi.localeCompare(b.kullaniciAdi, "tr"));
  }, [data?.kullanicilar]);

  const ustYetkiliRows = useMemo(() => {
    const rows = (data?.ustYetkililer ?? []).map((x: any) => ({
      personelId: (x.personelId ?? x.PersonelId ?? "").toString(),
      ustYetkiliPersonelId: (x.ustYetkiliPersonelId ?? x.UstYetkiliPersonelId ?? "").toString(),
      olusturmaTarihi: (x.olusturmaTarihi ?? x.OlusturmaTarihi ?? "").toString(),
    }));
    return rows;
  }, [data?.ustYetkililer]);

  // Kullanıcı - personel modal
  const [kpModal, setKpModal] = useState(false);
  const [kpUserId, setKpUserId] = useState<number | null>(null);
  const [kpSaving, setKpSaving] = useState(false);

  const openKp = (kullaniciId: number) => {
    setKpUserId(kullaniciId);
    setKpModal(true);
  };

  const saveKp = async (personelKey: string) => {
    if (!kpUserId || kpSaving) return;
    setKpSaving(true);
    try {
      const personelId = personelKey ? asInt(personelKey, 0) : null;
      const resp = await adminService.setKullaniciPersonel({ kullaniciId: kpUserId, personelId });
      if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
      showPopup("success", resp?.message || "Güncellendi.");
      await refresh();
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setKpSaving(false);
      setKpModal(false);
      setKpUserId(null);
    }
  };

  // Üst yetkili form
  const [uyPersonelModal, setUyPersonelModal] = useState(false);
  const [uyUstModal, setUyUstModal] = useState(false);
  const [uyPersonelId, setUyPersonelId] = useState<string>("");
  const [uyUstId, setUyUstId] = useState<string>("");
  const [uySaving, setUySaving] = useState(false);

  const saveUstYetkili = async () => {
    if (uySaving) return;
    if (!uyPersonelId) {
      showPopup("error", "Personel seçiniz.");
      return;
    }
    setUySaving(true);
    try {
      const resp = await adminService.setUstYetkili({ personelId: uyPersonelId, ustYetkiliPersonelId: uyUstId || null });
      if (!resp?.success) throw new Error(resp?.message || "İşlem başarısız.");
      showPopup("success", resp?.message || "Güncellendi.");
      setUyPersonelId("");
      setUyUstId("");
      await refresh();
    } catch (e: any) {
      showPopup("error", e?.message || "Hata oluştu.");
    } finally {
      setUySaving(false);
    }
  };

  // Güncelleme mail form
  const [mVersiyon, setMVersiyon] = useState("1.0.0");
  const [mTarih, setMTarih] = useState<Date>(() => normalizeDateOnly(new Date()));
  const [mTip, setMTip] = useState<"Major" | "Minor" | "Bugfix">("Minor");
  const [mYeni, setMYeni] = useState("");
  const [mIyiles, setMIyiles] = useState("");
  const [mHata, setMHata] = useState("");
  const [mKritik, setMKritik] = useState("");
  const [mNot, setMNot] = useState("Bu güncelleme yapıldıktan sonra uygulamanın yeniden başlatılması gerekmektedir.");
  const [previewLoading, setPreviewLoading] = useState(false);
  const [sendLoading, setSendLoading] = useState(false);

  const [previewVisible, setPreviewVisible] = useState(false);
  const [previewHtml, setPreviewHtml] = useState("");
  const quickMenu = useHeaderQuickMenu();
  const notif = useNotificationsContext();

  const buildMailPayload = (): GuncellemeNotifikasyonDTO => {
    const toLines = (s: string) =>
      (s ?? "")
        .split(/\r?\n/)
        .map((x) => x.trim())
        .filter(Boolean);
    return {
      versiyonNumarasi: (mVersiyon ?? "").trim(),
      yayinTarihi: `${fmtIsoDate(mTarih)}T00:00:00`,
      guncellemeTipi: mTip,
      yeniOzellikler: toLines(mYeni),
      iyilestirmeler: toLines(mIyiles),
      hataDuzeltmeleri: toLines(mHata),
      kritikDegisiklikler: toLines(mKritik),
      ekNotlar: (mNot ?? "").trim(),
    };
  };

  const validateMailPayload = (p: GuncellemeNotifikasyonDTO): string | null => {
    if (!p.versiyonNumarasi?.trim()) return "Versiyon numarası giriniz.";
    if (!p.guncellemeTipi?.trim()) return "Güncelleme tipini seçiniz.";
    const total =
      (p.yeniOzellikler?.length ?? 0) +
      (p.iyilestirmeler?.length ?? 0) +
      (p.hataDuzeltmeleri?.length ?? 0) +
      (p.kritikDegisiklikler?.length ?? 0);
    if (total <= 0) return "En az bir kategoriye madde eklemelisiniz (her satır bir madde).";
    return null;
  };

  const doPreview = async () => {
    if (previewLoading || sendLoading) return;
    const payload = buildMailPayload();
    const validationError = validateMailPayload(payload);
    if (validationError) {
      showPopup("error", validationError);
      return;
    }
    setPreviewLoading(true);
    try {
      const resp = await adminService.previewMail(payload);
      if (!resp?.success) throw new Error(resp?.message || "Önizleme alınamadı.");
      setPreviewHtml(resp.data ?? "");
      setPreviewVisible(true);
    } catch (e: any) {
      showPopup("error", getApiErrorMessage(e));
    } finally {
      setPreviewLoading(false);
    }
  };

  const doSend = async () => {
    if (previewLoading || sendLoading) return;
    const payload = buildMailPayload();
    const validationError = validateMailPayload(payload);
    if (validationError) {
      showPopup("error", validationError);
      return;
    }
    setSendLoading(true);
    try {
      const resp = await adminService.sendMail(payload);
      if (!resp?.success) throw new Error(resp?.message || "Gönderilemedi.");
      showPopup("success", resp?.message || "Gönderildi.");
    } catch (e: any) {
      showPopup("error", getApiErrorMessage(e));
    } finally {
      setSendLoading(false);
    }
  };

  const topBar = (
    <>
      <PageHeader
        title="Admin Panel"
        onOpenMenu={props.onOpenMenu}
        rightIcon="refresh"
        onRightPress={refresh}
        rightIcon2="bell-outline"
        onRightPress2={() => quickMenu.open("notif")}
        rightBadge2={notif.unreadCount}
        rightA11yLabel2="Bildirimler ve hesap"
      />
      {quickMenu.modal}
    </>
  );

  const tabs = (
    <View className="bg-white border-b border-[#f1f5f9] py-3">
      <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={{ paddingHorizontal: 16, gap: 8 }}>
        {[
          { k: "quick", t: "Sekmeler" },
          { k: "guncellememail", t: "Güncelleme Maili" },
          { k: "kullanicilar", t: "Kullanıcı-Personel" },
          { k: "ustyetkili", t: "Üst Yetkili" },
        ].map((x) => {
          const active = tab === x.k;
          return (
            <TouchableOpacity
              key={x.k}
              onPress={() => setTab(x.k as TabKey)}
              className={`px-3 py-2 rounded-xl border ${active ? "bg-[#0f172a] border-[#0f172a]" : "bg-[#f8fafc] border-[#e2e8f0]"}`}
            >
              <Text className={`font-extrabold ${active ? "text-white" : "text-[#334155]"}`}>{x.t}</Text>
            </TouchableOpacity>
          );
        })}
      </ScrollView>
    </View>
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
        </View>
      </View>
    );
  }

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} />
      {topBar}
      {tabs}

      {tab === "quick" ? (
        <ScrollView
          ref={scrollRef}
          key={`tab_${tab}`}
          className="flex-1 px-4"
          contentInsetAdjustmentBehavior="never"
          contentContainerStyle={{ paddingTop: 16, paddingBottom: 24 }}
        >
          <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
            <View className="px-4 py-3 border-b border-[#f1f5f9]">
              <Text className="text-[#0f172a] font-extrabold">Hızlı Erişim</Text>
            </View>
            <View className="p-4">
              {[
                { key: "firmalar", label: "Firmalar" },
                { key: "isyerleri", label: "İşyerleri" },
                { key: "cihazlar", label: "Cihazlar" },
                { key: "departmanlar", label: "Departmanlar" },
                { key: "pozisyonlar", label: "Pozisyonlar" },
                { key: "resmiTatiller", label: "Resmi Tatiller" },
                { key: "calismaStatuleri", label: "Çalışma Statüleri" },
                { key: "vardiyalar", label: "Vardiyalar" },
              ].map((x) => (
                <TouchableOpacity
                  key={x.key}
                  onPress={() => props.onNavigate(x.key)}
                  className="mb-2 px-4 py-3 rounded-xl bg-[#f8fafc] border border-[#e2e8f0] flex-row items-center justify-between"
                >
                  <Text className="text-[#0f172a] font-extrabold">{x.label}</Text>
                  <MaterialCommunityIcons name="chevron-right" size={20} color="#64748b" />
                </TouchableOpacity>
              ))}
            </View>
          </View>
        </ScrollView>
      ) : null}

      {tab === "kullanicilar" ? (
        <ScrollView
          ref={scrollRef}
          key={`tab_${tab}`}
          className="flex-1 px-4"
          contentInsetAdjustmentBehavior="never"
          contentContainerStyle={{ paddingTop: 16, paddingBottom: 24 }}
        >
          <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
            <View className="px-4 py-3 border-b border-[#f1f5f9]">
              <Text className="text-[#0f172a] font-extrabold">Kullanıcı - Personel Eşlemesi</Text>
            </View>
            <View className="px-4 py-3">
              {kullaniciRows.map((u) => (
                <View key={`u_${u.kullaniciId}`} className="mb-3 p-4 rounded-2xl border border-[#e2e8f0] bg-white">
                  <Text className="text-[#0f172a] font-extrabold" numberOfLines={1}>
                    {u.kullaniciAdi}
                  </Text>
                  <Text className="text-[#64748b] font-semibold text-[12px]" numberOfLines={1}>
                    KullanıcıId: {u.kullaniciId} • Rol: {u.rol} • PersonelId: {u.personelId ?? "-"}
                  </Text>
                  <View className="mt-3 flex-row justify-end">
                    <TouchableOpacity onPress={() => openKp(u.kullaniciId)} className="px-4 py-3 rounded-xl bg-[#0f172a]">
                      <Text className="text-white font-extrabold">Eşle</Text>
                    </TouchableOpacity>
                  </View>
                </View>
              ))}
            </View>
          </View>

          <SelectModal
            visible={kpModal}
            title="Personel Seç"
            items={personelItems}
            onClose={() => setKpModal(false)}
            onPick={(k) => saveKp(k)}
          />
        </ScrollView>
      ) : null}

      {tab === "ustyetkili" ? (
        <ScrollView
          ref={scrollRef}
          key={`tab_${tab}`}
          className="flex-1 px-4"
          contentInsetAdjustmentBehavior="never"
          contentContainerStyle={{ paddingTop: 16, paddingBottom: 24 }}
        >
          <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
            <View className="px-4 py-3 border-b border-[#f1f5f9]">
              <Text className="text-[#0f172a] font-extrabold">Üst Yetkili Yönetimi</Text>
            </View>
            <View className="p-4">
              <Text className="text-[#64748b] font-semibold">Personel</Text>
              <TouchableOpacity onPress={() => setUyPersonelModal(true)} className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0]">
                <Text className="text-[#0f172a] font-extrabold">{uyPersonelId ? uyPersonelId : "Seçiniz..."}</Text>
              </TouchableOpacity>

              <Text className="mt-4 text-[#64748b] font-semibold">Atanacak Üst Yetkili</Text>
              <TouchableOpacity onPress={() => setUyUstModal(true)} className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0]">
                <Text className="text-[#0f172a] font-extrabold">{uyUstId ? uyUstId : "(Boşalt - Sil)"}</Text>
              </TouchableOpacity>

              <TouchableOpacity onPress={saveUstYetkili} disabled={uySaving} className={`mt-4 px-4 py-3 rounded-xl items-center ${uySaving ? "bg-[#94a3b8]" : "bg-[#0f172a]"}`}>
                <Text className="text-white font-extrabold">{uySaving ? "Kaydediliyor..." : "Kaydet"}</Text>
              </TouchableOpacity>
            </View>
          </View>

          <View className="mt-4 bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
            <View className="px-4 py-3 border-b border-[#f1f5f9]">
              <Text className="text-[#0f172a] font-extrabold">Mevcut Atamalar</Text>
            </View>
            <View className="p-4">
              {!ustYetkiliRows.length ? (
                <Text className="text-[#64748b] font-semibold">Kayıt yok.</Text>
              ) : (
                ustYetkiliRows.map((x, idx) => (
                  <View key={`uy_${idx}`} className="mb-3 p-4 rounded-2xl bg-[#f8fafc] border border-[#e2e8f0]">
                    <Text className="text-[#0f172a] font-extrabold">Alt: {x.personelId}</Text>
                    <Text className="text-[#334155] font-semibold mt-1">Üst: {x.ustYetkiliPersonelId}</Text>
                    {x.olusturmaTarihi ? (
                      <Text className="text-[#64748b] font-semibold text-[12px] mt-1">{fmtDateTimeTRLoose(x.olusturmaTarihi)}</Text>
                    ) : null}
                  </View>
                ))
              )}
            </View>
          </View>

          <SelectModal
            visible={uyPersonelModal}
            title="Personel Seç"
            items={(data?.personeller ?? [])
              .slice()
              .sort((a: any, b: any) => (a.adSoyad ?? a.AdSoyad ?? "").toString().localeCompare((b.adSoyad ?? b.AdSoyad ?? "").toString(), "tr"))
              .map((p: any) => ({ key: String(p.id ?? p.Id), label: `${p.id ?? p.Id} - ${(p.adSoyad ?? p.AdSoyad ?? "").toString()}` }))}
            onClose={() => setUyPersonelModal(false)}
            onPick={(k) => setUyPersonelId(k)}
          />
          <SelectModal
            visible={uyUstModal}
            title="Üst Yetkili Seç"
            items={[{ key: "", label: "(Boşalt - Sil)" }, ...(data?.personeller ?? [])
              .slice()
              .sort((a: any, b: any) => (a.adSoyad ?? a.AdSoyad ?? "").toString().localeCompare((b.adSoyad ?? b.AdSoyad ?? "").toString(), "tr"))
              .map((p: any) => ({ key: String(p.id ?? p.Id), label: `${p.id ?? p.Id} - ${(p.adSoyad ?? p.AdSoyad ?? "").toString()}` }))]}
            onClose={() => setUyUstModal(false)}
            onPick={(k) => setUyUstId(k)}
          />
        </ScrollView>
      ) : null}

      {tab === "guncellememail" ? (
        <ScrollView
          ref={scrollRef}
          key={`tab_${tab}`}
          className="flex-1 px-4"
          contentInsetAdjustmentBehavior="never"
          contentContainerStyle={{ paddingTop: 16, paddingBottom: 24 }}
        >
          <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
            <View className="px-4 py-3 border-b border-[#f1f5f9]">
              <Text className="text-[#0f172a] font-extrabold">Güncelleme Bildirimi</Text>
            </View>
            <View className="p-4">
              <Text className="text-[#64748b] font-semibold">Versiyon No *</Text>
              <TextInput value={mVersiyon} onChangeText={setMVersiyon} className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" />

              <Text className="mt-4 text-[#64748b] font-semibold">Yayın Tarihi *</Text>
              <TextInput
                value={fmtIsoDate(mTarih)}
                onChangeText={(t) => {
                  const m = (t ?? "").match(/^(\d{4})-(\d{2})-(\d{2})$/);
                  if (!m) return;
                  setMTarih(normalizeDateOnly(new Date(Number(m[1]), Number(m[2]) - 1, Number(m[3]))));
                }}
                placeholder="yyyy-MM-dd"
                className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold"
                autoCapitalize="none"
              />

              <Text className="mt-4 text-[#64748b] font-semibold">Güncelleme Tipi *</Text>
              <View className="mt-2 flex-row gap-2">
                {(["Major", "Minor", "Bugfix"] as const).map((t) => (
                  <TouchableOpacity key={t} onPress={() => setMTip(t)} className={`px-3 py-2 rounded-xl ${mTip === t ? "bg-[#0f172a]" : "bg-[#f1f5f9]"}`}>
                    <Text className={`font-extrabold ${mTip === t ? "text-white" : "text-[#334155]"}`}>{t}</Text>
                  </TouchableOpacity>
                ))}
              </View>

              <Text className="mt-4 text-[#64748b] font-semibold">✨ Yeni Özellikler</Text>
              <TextInput value={mYeni} onChangeText={setMYeni} multiline className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" style={{ minHeight: 90, textAlignVertical: "top" }} />

              <Text className="mt-4 text-[#64748b] font-semibold">⚡ İyileştirmeler</Text>
              <TextInput value={mIyiles} onChangeText={setMIyiles} multiline className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" style={{ minHeight: 90, textAlignVertical: "top" }} />

              <Text className="mt-4 text-[#64748b] font-semibold">🔧 Hata Düzeltmeleri</Text>
              <TextInput value={mHata} onChangeText={setMHata} multiline className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" style={{ minHeight: 90, textAlignVertical: "top" }} />

              <Text className="mt-4 text-[#64748b] font-semibold">⚠️ Kritik Değişiklikler</Text>
              <TextInput value={mKritik} onChangeText={setMKritik} multiline className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" style={{ minHeight: 90, textAlignVertical: "top" }} />

              <Text className="mt-4 text-[#64748b] font-semibold">📝 Ek Notlar</Text>
              <TextInput value={mNot} onChangeText={setMNot} multiline className="mt-2 px-4 py-3 rounded-xl bg-white border border-[#e2e8f0] text-[#0f172a] font-semibold" style={{ minHeight: 70, textAlignVertical: "top" }} />

              <View className="mt-4 flex-row gap-2 justify-end">
                <TouchableOpacity onPress={doPreview} disabled={previewLoading} className={`px-4 py-3 rounded-xl ${previewLoading ? "bg-[#e2e8f0]" : "bg-[#f1f5f9]"}`}>
                  <Text className="text-[#334155] font-extrabold">Önizleme</Text>
                </TouchableOpacity>
                <TouchableOpacity onPress={doSend} disabled={sendLoading} className={`px-4 py-3 rounded-xl ${sendLoading ? "bg-[#94a3b8]" : "bg-[#0f172a]"}`}>
                  <Text className="text-white font-extrabold">{sendLoading ? "Gönderiliyor..." : "Gönder"}</Text>
                </TouchableOpacity>
              </View>
            </View>
          </View>

          {previewVisible ? (
            <Modal visible transparent animationType="fade" onRequestClose={() => setPreviewVisible(false)}>
              <View className="flex-1 bg-black/60">
                <View className="flex-row items-center justify-between px-4 pt-12 pb-3 bg-white">
                  <Text className="text-[#1e293b] font-extrabold text-[14px]" numberOfLines={1}>
                    Önizleme (HTML)
                  </Text>
                  <TouchableOpacity onPress={() => setPreviewVisible(false)} className="px-3 py-2 bg-[#f1f5f9] rounded-xl">
                    <Text className="text-[#334155] font-extrabold">Kapat</Text>
                  </TouchableOpacity>
                </View>
            <View className="flex-1 bg-white">
              <WebView
                originWhitelist={["*"]}
                source={{ html: previewHtml || "<html><body>(boş)</body></html>" }}
                style={{ flex: 1, backgroundColor: "#fff" }}
              />
            </View>
              </View>
            </Modal>
          ) : null}
        </ScrollView>
      ) : null}
    </View>
  );
}

