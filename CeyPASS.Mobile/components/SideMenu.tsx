import React, { useEffect, useMemo, useState } from "react";
import { Image, Modal, ScrollView, Text, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { profilService } from "../services/profilApi";

type MenuItem = { key: string; label: string; icon: keyof typeof MaterialCommunityIcons.glyphMap };

function normalizeRoleId(...candidates: any[]): number | null {
  for (const c of candidates) {
    if (c === null || c === undefined) continue;
    const n = Number(c);
    if (Number.isFinite(n) && n > 0) return n;
  }
  return null;
}

function buildMenu(abilities: any, user: any, hasSicilNo: boolean) {
  const view: Record<string, boolean> = abilities?.view ?? abilities?.View ?? {};
  const isSupervisor = abilities?.isSupervisor ?? false;
  const rolId: number | null = normalizeRoleId(
    abilities?.rolId,
    abilities?.RolId,
    user?.rolId,
    user?.RolId,
    user?.rolID,
    user?.RolID
  );

  const sections: { title: string; items: MenuItem[] }[] = [];

  const isPersonnel = !!(view.Profil && hasSicilNo);
  const isCorporate =
    !!view.Dashboard ||
    !!view.IzinTalepleri ||
    !!view.Avans ||
    !!view.Personeller ||
    !!view.KisiHareketler ||
    !!view.Izinler ||
    !!view.AylikPuantaj ||
    !!view.Raporlar ||
    !!view.Firmalar ||
    !!view.Isyerler ||
    !!view.Departmanlar ||
    !!view.Pozisyonlar ||
    !!view.Vardiyalar ||
    !!view.CalismaStatuleri ||
    !!view.Cihazlar ||
    !!view.ResmiTatiller ||
    rolId === 1;

  if (isPersonnel) {
    const items: MenuItem[] = [
      { key: "profil", label: "Profil", icon: "account" },
      { key: "izinlerim", label: "İzinlerim", icon: "calendar-check" },
      { key: "avanslarim", label: "Avanslarım", icon: "cash" },
    ];
    if (isSupervisor) items.push({ key: "yetkiliPaneli", label: "Yetkili Paneli", icon: "account-check" });
    sections.push({ title: "Personel", items });
  }

  // Always available for everyone (including personnel-only users)
  sections.push({
    title: "Diğer",
    items: [{ key: "about", label: "Hakkında", icon: "information-outline" }],
  });

  // Normal personel: sadece personel menüsü (kurumsal bölümleri gizle)
  if (isPersonnel && !isCorporate) return sections;

  const talepItems: MenuItem[] = [];
  if (view.IzinTalepleri) talepItems.push({ key: "izinTalepleri", label: "İzin Talepleri", icon: "clipboard-check" });
  if (view.Avans) talepItems.push({ key: "avansTalepleri", label: "Avans Talepleri", icon: "cash" });
  if (talepItems.length) sections.push({ title: "Talepler", items: talepItems });

  const mainItems: MenuItem[] = [];
  if (view.Dashboard) mainItems.push({ key: "dashboard", label: "Ana Sayfa", icon: "home" });
  if (view.Personeller) mainItems.push({ key: "personeller", label: "Personeller", icon: "account-group" });
  if (view.KisiHareketler) mainItems.push({ key: "kisiHareketleri", label: "Kişi Hareketleri", icon: "clock-outline" });
  if (view.Izinler) mainItems.push({ key: "izinler", label: "İzinler", icon: "calendar" });
  if (view.AylikPuantaj) mainItems.push({ key: "puantaj", label: "Puantaj", icon: "calendar-month" });
  if (view.Raporlar) mainItems.push({ key: "raporlar", label: "Raporlar", icon: "file-chart" });
  if (mainItems.length) sections.push({ title: "Ana Menü", items: mainItems });

  const orgItems: MenuItem[] = [];
  if (view.Firmalar) orgItems.push({ key: "firmalar", label: "Firmalar", icon: "office-building" });
  if (view.Isyerler) orgItems.push({ key: "isyerleri", label: "İşyerleri", icon: "map-marker" });
  if (view.Departmanlar) orgItems.push({ key: "departmanlar", label: "Departmanlar", icon: "sitemap" });
  if (view.Pozisyonlar) orgItems.push({ key: "pozisyonlar", label: "Pozisyonlar", icon: "badge-account" });
  if (orgItems.length) sections.push({ title: "Organizasyon", items: orgItems });

  const settingsItems: MenuItem[] = [];
  if (view.Vardiyalar) settingsItems.push({ key: "vardiyalar", label: "Vardiyalar", icon: "clock" });
  if (view.CalismaStatuleri) settingsItems.push({ key: "calismaStatuleri", label: "Çalışma Statüleri", icon: "toggle-switch" });
  if (view.Cihazlar) settingsItems.push({ key: "cihazlar", label: "Cihazlar", icon: "harddisk" });
  if (view.ResmiTatiller) settingsItems.push({ key: "resmiTatiller", label: "Resmi Tatiller", icon: "calendar-star" });
  if (settingsItems.length) sections.push({ title: "Ayarlar", items: settingsItems });

  if (rolId === 1) {
    sections.push({
      title: "Admin",
      items: [{ key: "adminPanel", label: "Admin Panel", icon: "shield-lock" }],
    });
  }

  return sections;
}

export function SideMenu(props: {
  visible: boolean;
  abilities: any;
  user: any;
  activeKey: string;
  onClose: () => void;
  onSelect: (key: string) => void;
  onLogout: () => void;
  onOpenTips?: () => void;
}) {
  const hasSicilNo = !!(props.user?.sicilNo ?? props.user?.SicilNo);
  const sections = buildMenu(props.abilities, props.user, hasSicilNo);
  const isSupervisor = !!(props.abilities?.isSupervisor ?? props.abilities?.IsSupervisor);
  const [profile, setProfile] = useState<any>(null);
  const [profileLoaded, setProfileLoaded] = useState(false);

  useEffect(() => {
    if (!props.visible) return;
    if (!hasSicilNo) return;
    if (profileLoaded) return;
    let alive = true;
    (async () => {
      try {
        const res = await profilService.getProfil();
        if (!alive) return;
        if (res?.success) setProfile(res.data ?? null);
      } catch {
        // silent
      } finally {
        if (alive) setProfileLoaded(true);
      }
    })();
    return () => {
      alive = false;
    };
  }, [props.visible, hasSicilNo, profileLoaded]);

  const displayName = useMemo(
    () => (props.user?.adSoyad ?? props.user?.AdSoyad ?? props.user?.kullaniciAdi ?? "Kullanıcı").toString().trim(),
    [props.user]
  );
  const roleName = useMemo(
    () =>
      (props.abilities?.rolAdi ?? props.user?.rol ?? props.user?.Rol ?? props.user?.rolAdi ?? props.user?.RolAdi ?? "")
        .toString()
        .trim(),
    [props.abilities, props.user]
  );
  const pozName = useMemo(() => {
    const v =
      profile?.pozisyonAdi ??
      profile?.PozisyonAdi ??
      profile?.personel?.PozisyonAdi ??
      profile?.personel?.pozisyonAdi ??
      props.user?.pozisyonAdi ??
      props.user?.PozisyonAdi ??
      props.user?.pozisyon ??
      props.user?.Pozisyon ??
      "";
    return (v ?? "").toString().trim();
  }, [profile, props.user]);
  const deptName = useMemo(() => {
    const v =
      profile?.departmanAdi ??
      profile?.DepartmanAdi ??
      profile?.personel?.DepartmanAdi ??
      profile?.personel?.departmanAdi ??
      props.user?.departmanAdi ??
      props.user?.DepartmanAdi ??
      props.user?.departman ??
      props.user?.Departman ??
      props.user?.birimAdi ??
      props.user?.BirimAdi ??
      "";
    return (v ?? "").toString().trim();
  }, [profile, props.user]);
  const normalizedRole = roleName.toLocaleLowerCase("tr-TR");
  const roleIsGenericPersonel = normalizedRole === "personel";
  // For Üst Yetkili (dual role), show position in the user card like web.
  const subtitle = isSupervisor ? pozName || deptName || " " : (!roleIsGenericPersonel && roleName) || pozName || deptName || " ";
  const avatarUri = useMemo(() => {
    const v = profile?.fotografDataUrl ?? profile?.FotografDataUrl ?? profile?.personel?.FotografDataUrl ?? profile?.personel?.fotografDataUrl ?? "";
    return (v ?? "").toString().trim();
  }, [profile]);
  const initial = useMemo(() => {
    const s = (displayName ?? "").trim();
    if (!s) return "?";
    const first = s[0]?.toLocaleUpperCase("tr-TR") ?? "?";
    return first;
  }, [displayName]);
  const nameFontSize = displayName.length > 22 ? 14 : displayName.length > 16 ? 15 : 16;

  return (
    <Modal visible={props.visible} transparent animationType="fade" onRequestClose={props.onClose}>
      <TouchableOpacity className="flex-1 bg-black/50" activeOpacity={1} onPress={props.onClose}>
        <View className="flex-1 flex-row">
          <TouchableOpacity activeOpacity={1} onPress={() => {}}>
            <View className="w-[300px] bg-white h-full pt-12 border-r border-[#f1f5f9]">
              <ScrollView className="flex-1 px-3 py-3">
                {props.onOpenTips ? (
                  <View className="mb-4">
                    <Text className="px-2 py-2 text-[#64748b] font-extrabold text-[11px] uppercase">
                      Hızlı erişim
                    </Text>
                    <TouchableOpacity
                      onPress={() => {
                        props.onClose();
                        props.onOpenTips?.();
                      }}
                      className="flex-row items-center px-3 py-3 rounded-xl mb-1"
                    >
                      <MaterialCommunityIcons name="help-circle-outline" size={20} color="#64748b" />
                      <Text className="ml-3 font-semibold text-[#1e293b]">İpuçları</Text>
                    </TouchableOpacity>
                  </View>
                ) : null}
                {sections.map((sec) => (
                  <View key={sec.title} className="mb-4">
                    <Text className="px-2 py-2 text-[#64748b] font-extrabold text-[11px] uppercase">
                      {sec.title}
                    </Text>
                    {sec.items.map((it) => {
                      const active = it.key === props.activeKey;
                      return (
                        <TouchableOpacity
                          key={it.key}
                          onPress={() => {
                            props.onSelect(it.key);
                            props.onClose();
                          }}
                          className={`flex-row items-center px-3 py-3 rounded-xl mb-1 ${
                            active ? "bg-[#fee2e2]" : "bg-transparent"
                          }`}
                        >
                          <MaterialCommunityIcons
                            name={it.icon}
                            size={20}
                            color={active ? "#dc2626" : "#64748b"}
                          />
                          <Text className={`ml-3 font-semibold ${active ? "text-[#dc2626]" : "text-[#1e293b]"}`}>
                            {it.label}
                          </Text>
                        </TouchableOpacity>
                      );
                    })}
                  </View>
                ))}
              </ScrollView>

              <View className="px-5 py-4 border-t border-[#f1f5f9]">
                <View className="flex-row items-center mb-3">
                  <View className="w-[44px] h-[44px] rounded-full bg-[#f1f5f9] items-center justify-center overflow-hidden border border-[#e2e8f0]">
                    {avatarUri ? (
                      <Image source={{ uri: avatarUri }} style={{ width: 44, height: 44 }} />
                    ) : (
                      <Text className="text-[#0f172a] font-extrabold" style={{ fontSize: 18 }}>
                        {initial}
                      </Text>
                    )}
                  </View>
                  <View className="ml-3 flex-1">
                    <Text
                      className="text-[#1e293b] font-extrabold"
                      style={{ fontSize: nameFontSize }}
                      numberOfLines={1}
                      ellipsizeMode="tail"
                    >
                      {displayName}
                    </Text>
                    <Text className="text-[#64748b] font-semibold text-[12px]" numberOfLines={1} ellipsizeMode="tail">
                      {subtitle}
                    </Text>
                  </View>
                </View>
                <TouchableOpacity
                  onPress={props.onLogout}
                  className="flex-row items-center justify-center bg-[#fee2e2] rounded-xl py-3"
                >
                  <MaterialCommunityIcons name="logout" size={20} color="#dc2626" />
                  <Text className="ml-2 text-[#dc2626] font-extrabold">Çıkış Yap</Text>
                </TouchableOpacity>
              </View>
            </View>
          </TouchableOpacity>
          <View className="flex-1" />
        </View>
      </TouchableOpacity>
    </Modal>
  );
}

