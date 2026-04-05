import React, { useMemo } from "react";
import { Image, ScrollView, Text, View } from "react-native";
import Constants from "expo-constants";
import { PageHeader } from "./PageHeader";
import { useHeaderQuickMenu } from "./HeaderQuickMenu";
import { useNotificationsContext } from "./NotificationsProvider";

function pick(obj: any, ...keys: string[]) {
  for (const k of keys) {
    const v = obj?.[k as any];
    if (v !== undefined && v !== null && String(v).trim() !== "") return v;
  }
  return undefined;
}

export function AboutScreen(props: { onOpenMenu?: () => void }) {
  const quickMenu = useHeaderQuickMenu();
  const notif = useNotificationsContext();

  const appTitle = "CeyPASS PDKS Mobil Uygulaması";

  const version = useMemo(() => {
    const v =
      pick((Constants as any)?.expoConfig, "version") ??
      pick((Constants as any)?.manifest2?.extra?.expoClient, "version") ??
      pick((Constants as any)?.manifest, "version") ??
      "1.0.0";
    return v.toString().trim();
  }, []);

  const author = "Tahir Koca";
  const build = useMemo(() => {
    const v = pick(Constants as any, "nativeBuildVersion");
    return (v ?? "").toString().trim();
  }, []);
  const nativeAppVersion = useMemo(() => {
    const v = pick(Constants as any, "nativeAppVersion");
    return (v ?? "").toString().trim();
  }, []);
  const iosBuildNumber = useMemo(() => {
    const v = pick((Constants as any)?.expoConfig?.ios, "buildNumber") ?? pick((Constants as any)?.manifest?.ios, "buildNumber");
    return (v ?? "").toString().trim();
  }, []);
  const androidVersionCode = useMemo(() => {
    const v = pick((Constants as any)?.expoConfig?.android, "versionCode") ?? pick((Constants as any)?.manifest?.android, "versionCode");
    return (v ?? "").toString().trim();
  }, []);

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <PageHeader
        title="Hakkında"
        onOpenMenu={props.onOpenMenu}
        rightIcon="bell-outline"
        onRightPress={() => quickMenu.open("notif")}
        rightBadge={notif.unreadCount}
        rightA11yLabel="Bildirimler ve hesap"
      />
      {quickMenu.modal}

      <ScrollView className="flex-1 px-5 py-5" contentContainerStyle={{ paddingBottom: 24 }}>
        <View className="mt-6 items-center bg-white rounded-3xl border border-[#f1f5f9] p-6">
          <Image source={require("../assets/logo.png")} style={{ width: 220, height: 80, resizeMode: "contain" }} />
          <Text className="mt-5 text-[#0f172a] font-extrabold text-[18px] text-center">{appTitle}</Text>
          <Text className="mt-2 text-[#64748b] font-semibold text-center">Versiyon: {version}</Text>

          <View className="mt-5 w-full">
            <CenterRow label="Geliştirici" value={author} />
            {nativeAppVersion ? <CenterRow label="Uygulama Sürümü" value={nativeAppVersion} /> : null}
            {iosBuildNumber ? <CenterRow label="iOS Build Number" value={iosBuildNumber} /> : null}
            {androidVersionCode ? <CenterRow label="Android Version Code" value={androidVersionCode} /> : null}
            {build ? <CenterRow label="Native Build" value={build} /> : null}
          </View>
        </View>
      </ScrollView>
    </View>
  );
}

function CenterRow(props: { label: string; value: string }) {
  return (
    <View className="py-2">
      <Text className="text-[#94a3b8] font-semibold text-center">{props.label}</Text>
      <Text className="mt-1 text-[#1e293b] font-extrabold text-center" numberOfLines={2} ellipsizeMode="tail">
        {props.value}
      </Text>
    </View>
  );
}

