import "./global.css";
import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  Image,
  TouchableOpacity,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  Alert,
  ImageBackground
} from 'react-native';
import { SafeAreaProvider, SafeAreaView } from 'react-native-safe-area-context';
import { LinearGradient } from 'expo-linear-gradient';

import { StatusBar } from 'expo-status-bar';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { GlassCard } from './components/GlassCard';
import { CustomButton } from './components/CustomButton';
import { CustomInput } from './components/CustomInput';
import { StatusPopup } from './components/StatusPopup';
import { Dashboard } from './components/Dashboard';
import { ForgotPasswordModal } from './components/ForgotPasswordModal';
import { CanliIzlemeScreen } from './components/CanliIzlemeScreen';
import { SideMenu } from './components/SideMenu';
import { AboutScreen } from "./components/AboutScreen";
import { ProfilScreen } from './components/personel/ProfilScreen';
import { IzinlerimScreen } from './components/personel/IzinlerimScreen';
import { AvanslarimScreen } from './components/personel/AvanslarimScreen';
import { YetkiliPaneliScreen } from './components/personel/YetkiliPaneliScreen';
import { PersonellerScreen } from './components/personel/PersonellerScreen';
import { KisiHareketleriScreen } from './components/kurumsal/KisiHareketleriScreen';
import { IzinlerScreen } from './components/kurumsal/IzinlerScreen';
import { RaporlarScreen } from './components/kurumsal/RaporlarScreen';
import { FirmalarScreen } from './components/kurumsal/FirmalarScreen';
import { IsyerleriScreen } from './components/kurumsal/IsyerleriScreen';
import { DepartmanlarScreen } from './components/kurumsal/DepartmanlarScreen';
import { PozisyonlarScreen } from './components/kurumsal/PozisyonlarScreen';
import { PuantajScreen } from './components/puantaj/PuantajScreen';
import { VardiyalarScreen } from './components/ayarlar/VardiyalarScreen';
import { CalismaStatuleriScreen } from './components/ayarlar/CalismaStatuleriScreen';
import { CihazlarScreen } from './components/ayarlar/CihazlarScreen';
import { ResmiTatillerScreen } from './components/ayarlar/ResmiTatillerScreen';
import { AdminPanelScreen } from './components/admin/AdminPanelScreen';
import { IzinTalepleriScreen } from './components/talepler/IzinTalepleriScreen';
import { AvansTalepleriScreen } from './components/talepler/AvansTalepleriScreen';
import { authService, setAuthToken } from './services/api';
import { clearSession, loadSession, saveSession } from './services/session';
import { preloadLoginBackground } from './services/preload';
import { NotificationsProvider } from "./components/NotificationsProvider";
import * as Notifications from "expo-notifications";
import Constants from "expo-constants";
import { deviceTokenService } from "./services/deviceTokenApi";
import api, { getApiBaseUrl } from "./services/api";
import { WelcomeSplash } from "./components/WelcomeSplash";

export default function App() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [userData, setUserData] = useState<any>(null);
  const [abilities, setAbilities] = useState<any>(null);
  const [menuVisible, setMenuVisible] = useState(false);
  const [activePage, setActivePage] = useState<
    | "dashboard"
    | "profil"
    | "izinlerim"
    | "avanslarim"
    | "yetkiliPaneli"
    | "about"
    | "izinTalepleri"
    | "avansTalepleri"
    | "personeller"
    | "kisiHareketleri"
    | "izinler"
    | "raporlar"
    | "puantaj"
    | "firmalar"
    | "isyerleri"
    | "departmanlar"
    | "pozisyonlar"
    | "vardiyalar"
    | "calismaStatuleri"
    | "cihazlar"
    | "resmiTatiller"
    | "adminPanel"
    | "placeholder"
  >("dashboard");
  const [activeKey, setActiveKey] = useState<string>("dashboard");
  const [forgotVisible, setForgotVisible] = useState(false);
  const [canliVisible, setCanliVisible] = useState(false);
  const [pushRegistered, setPushRegistered] = useState(false);
  const [bootStatus, setBootStatus] = useState<"checking" | "offline" | "error" | "online">("checking");
  const [bootMessage, setBootMessage] = useState<string | null>(null);
  const [bootReady, setBootReady] = useState(false); // ensure splash is visible at least briefly

  // Popup State'leri
  const [popupVisible, setPopupVisible] = useState(false);
  const [popupType, setPopupType] = useState<'success' | 'error'>('success');
  const [popupMessage, setPopupMessage] = useState('');

  const showPopup = (type: 'success' | 'error', message: string) => {
    setPopupType(type);
    setPopupMessage(message);
    setPopupVisible(true);
  };

  useEffect(() => {
    let alive = true;
    let readyTimer: any = null;
    const check = async () => {
      try {
        if (readyTimer) {
          clearTimeout(readyTimer);
          readyTimer = null;
        }
        setBootReady(false);
        setBootStatus("checking");
        setBootMessage(null);
        // Reachability to API:
        // - Any HTTP response (even 401/403/404) means we're online.
        // - Only network/timeout should be treated as offline.
        getApiBaseUrl(); // validates config & primes baseURL via interceptor
        try {
          await api.get("/Auth/abilities", { timeout: 4500 });
        } catch (e: any) {
          // If server responded, we are online (auth may be missing).
          if (!e?.response) throw e;
        }
        if (!alive) return;
        // Preload background image so transitions don't flash.
        preloadLoginBackground();

        const s = await loadSession();
        if (!alive) return;
        if (s) {
          setUsername(s.username ?? "");
          setRememberMe(true);
          setUserData(s.user);
          setAbilities(s.abilities ?? null);
          setAuthToken(s.token);
          setIsLoggedIn(true);
        }
        setBootStatus("online");
        setBootMessage(null);
        // keep splash visible briefly so logo animation can be seen
        readyTimer = setTimeout(() => {
          if (!alive) return;
          setBootReady(true);
        }, 2350);
        return;
      } catch (e: any) {
        if (!alive) return;
        const msg = e?.message?.includes("API base URL") ? e.message : null;
        setBootMessage(msg);
        setBootStatus(msg ? "error" : "offline");
        setBootReady(false);
      }
    };
    check();
    return () => {
      alive = false;
      if (readyTimer) clearTimeout(readyTimer);
    };
  }, []);

  useEffect(() => {
    // Configure push behavior (foreground notifications)
    Notifications.setNotificationHandler({
      handleNotification: async () => ({
        shouldShowAlert: true,
        shouldShowBanner: true,
        shouldShowList: true,
        shouldPlaySound: false,
        shouldSetBadge: false,
      }),
    });
  }, []);

  useEffect(() => {
    if (!isLoggedIn) return;
    if (pushRegistered) return;
    let alive = true;
    (async () => {
      try {
        const perm = await Notifications.getPermissionsAsync();
        let granted = perm.status === "granted";
        if (!granted) {
          const req = await Notifications.requestPermissionsAsync();
          granted = req.status === "granted";
        }
        if (!granted) return;

        // Expo SDK 54: projectId might be required depending on environment.
        const projectId =
          (Constants as any)?.expoConfig?.extra?.eas?.projectId ??
          (Constants as any)?.easConfig?.projectId ??
          undefined;

        const tokenRes = await Notifications.getExpoPushTokenAsync(projectId ? { projectId } : ({} as any));
        const token = (tokenRes?.data ?? "").toString();
        if (!token) return;

        const deviceType = `${Platform.OS}`;
        await deviceTokenService.register(token, deviceType);
        if (!alive) return;
        setPushRegistered(true);
      } catch {
        // silent
      }
    })();
    return () => {
      alive = false;
    };
  }, [isLoggedIn, pushRegistered]);

  const bootBlock =
    bootStatus !== "online" || !bootReady ? (
      <SafeAreaProvider style={{ flex: 1 }}>
        <WelcomeSplash
          status={bootStatus}
          message={bootMessage}
          onRetry={() => {
            (async () => {
              try {
                setBootReady(false);
                setBootStatus("checking");
                setBootMessage(null);
                getApiBaseUrl();
                try {
                  await api.get("/Auth/abilities", { timeout: 4500 });
                } catch (e: any) {
                  if (!e?.response) throw e;
                }
                preloadLoginBackground();
                const s = await loadSession();
                if (s) {
                  setUsername(s.username ?? "");
                  setRememberMe(true);
                  setUserData(s.user);
                  setAbilities(s.abilities ?? null);
                  setAuthToken(s.token);
                  setIsLoggedIn(true);
                }
                setBootStatus("online");
                setTimeout(() => setBootReady(true), 2350);
              } catch (e: any) {
                const msg = e?.message?.includes("API base URL") ? e.message : null;
                setBootMessage(msg);
                setBootStatus(msg ? "error" : "offline");
                setBootReady(false);
              }
            })();
          }}
        />
      </SafeAreaProvider>
    ) : null;

  const handleLogin = async () => {
    if (!username || !password) {
      showPopup('error', 'Lütfen tüm alanları doldurun.');
      return;
    }

    setLoading(true);
    try {
      const response = await authService.login(username, password);

      if (response.success) {
        setUserData(response.data.user);
        setAuthToken(response.data.token);
        // fetch abilities (web-like menu shaping)
        let abil: any = null;
        try {
          const a = await authService.abilities();
          if (a?.success) abil = a.data;
        } catch { }
        setAbilities(abil);
        // Default landing like web: if no Dashboard but Profil exists, start from personel area.
        try {
          const hasSicil = !!(response.data.user?.sicilNo ?? response.data.user?.SicilNo);
          const canDashboard = !!abil?.view?.Dashboard;
          const canProfil = !!abil?.view?.Profil;
          if (!canDashboard && canProfil && hasSicil) {
            setActiveKey("profil");
            setActivePage("placeholder");
          } else {
            setActiveKey("dashboard");
            setActivePage("dashboard");
          }
        } catch { }
        if (rememberMe) {
          await saveSession({ token: response.data.token, user: response.data.user, username, abilities: abil ?? undefined });
        } else {
          await clearSession();
        }
        showPopup('success', `Hoş geldiniz, ${response.data.user.adSoyad}`);

        // Popup kapandıktan sonra Dashboard'a geç
        setTimeout(() => {
          setPopupVisible(false);
          setIsLoggedIn(true);
        }, 1500);
      } else {
        showPopup('error', response.message || 'Giriş yapılamadı.');
      }
    } catch (error: any) {
      const errorMsg =
        error?.message?.includes("EXPO_PUBLIC_API_BASE_URL") || error?.message?.includes("API base URL")
          ? error.message
          : error.response?.data?.message ||
            "Sunucuya bağlanılamadı. Lütfen API'nin çalıştığından emin olun.";
      showPopup('error', errorMsg);
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setUserData(null);
    setAbilities(null);
    setUsername('');
    setPassword('');
    setAuthToken(null);
    clearSession();
  };

  const handleCanliIzleme = async () => {
    // Ensure background is cached before opening the screen.
    await preloadLoginBackground();
    setCanliVisible(true);
  };

  if (bootBlock) return bootBlock;

  if (isLoggedIn) {
    const view = abilities?.view ?? abilities?.View ?? {};
    const canView = (k: string) => !!view?.[k];
    const hasSicil = !!(userData?.sicilNo ?? userData?.SicilNo);
    const canNavigate = (key: string) => {
      if (key === "about") return true;
      if (key === "profil" || key === "izinlerim" || key === "avanslarim" || key === "yetkiliPaneli") {
        // Personel area requires profil permission + sicil number
        if (!hasSicil) return false;
        return !!canView("Profil");
      }
      if (key === "dashboard") return !!canView("Dashboard");
      if (key === "personeller") return !!canView("Personeller");
      if (key === "kisiHareketleri") return !!canView("KisiHareketler");
      if (key === "izinler") return !!canView("Izinler");
      if (key === "raporlar") return !!canView("Raporlar");
      if (key === "puantaj") return !!canView("AylikPuantaj");
      if (key === "firmalar") return !!canView("Firmalar");
      if (key === "isyerleri") return !!canView("Isyerler");
      if (key === "departmanlar") return !!canView("Departmanlar");
      if (key === "pozisyonlar") return !!canView("Pozisyonlar");
      if (key === "vardiyalar") return !!canView("Vardiyalar");
      if (key === "calismaStatuleri") return !!canView("CalismaStatuleri");
      if (key === "cihazlar") return !!canView("Cihazlar");
      if (key === "resmiTatiller") return !!canView("ResmiTatiller");
      if (key === "izinTalepleri") return !!canView("IzinTalepleri");
      if (key === "avansTalepleri") return !!canView("Avans");
      if (key === "adminPanel") return (abilities?.rolId ?? abilities?.RolId) === 1;
      return false;
    };

    // Safety: never render forbidden pages (web-like).
    const safePage = canNavigate(activePage) ? activePage : canView("Profil") && hasSicil ? "profil" : "dashboard";
    return (
      <SafeAreaProvider style={{ flex: 1 }}>
        <NotificationsProvider>
          <SafeAreaView edges={['left','right','bottom']} style={{ flex: 1, backgroundColor: '#f8fafc' }}>
            <StatusBar style="dark" />
            {safePage === "dashboard" ? (
              <Dashboard user={userData} onLogout={handleLogout} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "profil" ? (
              <ProfilScreen
                user={userData}
                onOpenMenu={() => setMenuVisible(true)}
                onNavigate={(k) => {
                  if (!canNavigate(k)) return;
                  setActiveKey(k);
                  setActivePage(k as any);
                }}
              />
            ) : safePage === "izinlerim" ? (
              <IzinlerimScreen user={userData} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "avanslarim" ? (
              <AvanslarimScreen user={userData} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "yetkiliPaneli" ? (
              <YetkiliPaneliScreen user={userData} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "about" ? (
              <AboutScreen onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "izinTalepleri" ? (
              <IzinTalepleriScreen user={userData} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "avansTalepleri" ? (
              <AvansTalepleriScreen user={userData} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "personeller" ? (
              <PersonellerScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "kisiHareketleri" ? (
              <KisiHareketleriScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "izinler" ? (
              <IzinlerScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "raporlar" ? (
              <RaporlarScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "puantaj" ? (
              <PuantajScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "firmalar" ? (
              <FirmalarScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "isyerleri" ? (
              <IsyerleriScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "departmanlar" ? (
              <DepartmanlarScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "pozisyonlar" ? (
              <PozisyonlarScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "vardiyalar" ? (
              <VardiyalarScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "calismaStatuleri" ? (
              <CalismaStatuleriScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "cihazlar" ? (
              <CihazlarScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "resmiTatiller" ? (
              <ResmiTatillerScreen user={userData} abilities={abilities} onOpenMenu={() => setMenuVisible(true)} />
            ) : safePage === "adminPanel" ? (
              <AdminPanelScreen
                user={userData}
                abilities={abilities}
                onOpenMenu={() => setMenuVisible(true)}
                onNavigate={(k) => {
                  if (!canNavigate(k)) return;
                  setActiveKey(k);
                  setActivePage(k as any);
                }}
              />
            ) : (
              <View className="flex-1 items-center justify-center bg-[#f8fafc] px-6">
                <Text className="text-[#1e293b] font-extrabold text-[16px] text-center">Bu ekran yakında.</Text>
                <TouchableOpacity
                  className="mt-4 px-4 py-3 rounded-xl bg-[#f1f5f9]"
                  onPress={() => {
                    const target = canNavigate("dashboard") ? "dashboard" : canNavigate("profil") ? "profil" : "dashboard";
                    setActiveKey(target);
                    setActivePage(target as any);
                  }}
                >
                  <Text className="text-[#334155] font-extrabold">Dashboard'a dön</Text>
                </TouchableOpacity>
              </View>
            )}

            <SideMenu
              visible={menuVisible}
              abilities={abilities}
              user={userData}
              activeKey={activeKey}
              onClose={() => setMenuVisible(false)}
              onLogout={handleLogout}
              onSelect={(key) => {
                if (!canNavigate(key)) return;
                setActiveKey(key);
                setActivePage(key as any);
              }}
            />
          </SafeAreaView>
        </NotificationsProvider>
      </SafeAreaProvider>
    );
  }

  return (
    <SafeAreaProvider style={{ flex: 1 }}>
      <ImageBackground
        source={require('./assets/ceyport-tekirdag.png')}
        style={{ flex: 1, backgroundColor: '#991b1b' }}
        resizeMode="cover"
      >
        <LinearGradient
          colors={['rgba(0,0,0,0.5)', 'rgba(0,0,0,0.4)']}
          style={{ flex: 1 }}
        >
          {/* Web'deki float eden arka plan şekilleri benzeri */}
          <View className="absolute top-[-12%] right-[-18%] w-[520px] h-[520px] bg-white/10 rounded-full" />
          <View className="absolute bottom-[-10%] left-[-18%] w-[380px] h-[380px] bg-white/10 rounded-full" />

          <SafeAreaView className="flex-1">
            <StatusBar style="light" />

            <KeyboardAvoidingView
              behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
              style={{ flex: 1 }}
            >
              <ScrollView
                contentContainerStyle={{ flexGrow: 1, justifyContent: 'center' }}
                showsVerticalScrollIndicator={false}
                className="px-6"
              >
                {/* Web-Style Floating Canlı İzleme Link */}
                <View className="mb-4 mt-12 items-end z-10">
                  <TouchableOpacity
                    activeOpacity={0.8}
                    onPress={handleCanliIzleme}
                    className="flex-row items-center px-5 py-3 bg-white/20 border border-white/30 rounded-full"
                  >
                    <MaterialCommunityIcons name="broadcast" size={20} color="white" />
                    <Text className="ml-2 text-white font-semibold text-xs">Canlı İzleme Girişi</Text>
                  </TouchableOpacity>
                </View>

                {/* Login Container */}
                <GlassCard className="mt-2">
                  {/* Header with Red Gradient like web */}
                  <LinearGradient
                    colors={['#dc2626', '#b91c1c', '#991b1b']}
                    start={{ x: 0, y: 0 }}
                    end={{ x: 1, y: 1 }}
                    style={{ marginHorizontal: -32, marginTop: -32, marginBottom: 32, height: 128, justifyContent: 'center', alignItems: 'center' }}
                  >
                    <Image
                      source={require('./assets/logo.png')}
                      style={{ width: 220, height: 75, resizeMode: 'contain' }}
                    />
                  </LinearGradient>



                  <View className="mb-2">
                    <CustomInput
                      label="Giriş Bilgisi"
                      placeholder="Ad Soyad,TC,Sicil No veya E‑Posta giriniz"
                      value={username}
                      onChangeText={setUsername}
                      icon="account"
                    />

                    <CustomInput
                      label="Şifre"
                      placeholder="Şifrenizi girin"
                      value={password}
                      onChangeText={setPassword}
                      secureTextEntry
                      icon="lock"
                    />

                    <View className="flex-row items-center justify-between mt-2 mb-6 px-1">

                      <TouchableOpacity
                        onPress={() => setRememberMe(!rememberMe)}
                        className="flex-row items-center"
                      >
                        <View
                          className={`w-5 h-5 rounded border items-center justify-center ${rememberMe ? 'bg-[#dc2626] border-[#dc2626]' : 'border-[#e2e8f0] bg-[#f8fafc]'
                            }`}
                        >
                          {rememberMe && <MaterialCommunityIcons name="check" size={14} color="white" />}
                        </View>
                        <Text className="text-xs font-semibold text-[#1e293b] ml-2">Beni Hatırla</Text>
                      </TouchableOpacity>
                      <TouchableOpacity onPress={() => setForgotVisible(true)}>
                        <Text className="text-xs font-semibold text-[#dc2626]">Şifremi Unuttum</Text>
                      </TouchableOpacity>
                    </View>

                    <CustomButton
                      title="GİRİŞ YAP"
                      onPress={handleLogin}
                      loading={loading}
                      className="mt-4"
                    />
                  </View>

                  {/* In-Card Footer */}
                  <View className="mt-8 pt-6 border-t border-[#f1f5f9] items-center">
                    <Text className="text-[10px] text-[#64748b]">
                      © {new Date().getFullYear()} CeyPASS - Tüm hakları saklıdır
                    </Text>
                  </View>
                </GlassCard>
              </ScrollView>
            </KeyboardAvoidingView>
          </SafeAreaView>
        </LinearGradient>
      </ImageBackground>
      <StatusPopup
        visible={popupVisible}
        type={popupType}
        message={popupMessage}
        onClose={() => setPopupVisible(false)}
      />
      <ForgotPasswordModal
        visible={forgotVisible}
        initialUsername={username}
        onClose={() => setForgotVisible(false)}
        onDone={(u) => {
          setUsername(u);
          showPopup("success", "Şifreniz güncellendi. Yeni şifrenizle giriş yapabilirsiniz.");
        }}
      />
      {canliVisible ? (
        <View style={{ position: "absolute", top: 0, left: 0, right: 0, bottom: 0 }}>
          <CanliIzlemeScreen onClose={() => setCanliVisible(false)} />
        </View>
      ) : null}
    </SafeAreaProvider>
  );
}
