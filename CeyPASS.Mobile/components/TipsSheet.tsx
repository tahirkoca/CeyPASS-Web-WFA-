import React, { useEffect, useMemo } from "react";
import { BackHandler, Modal, Platform, ScrollView, Text, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";

export type TipItem = { icon?: keyof typeof MaterialCommunityIcons.glyphMap; title: string; detail: string };

const GLOBAL_TIPS: TipItem[] = [
  { icon: "menu", title: "Menü", detail: "Sol üst menüden sayfalar arasında geçin." },
  { icon: "help-circle-outline", title: "İpuçları", detail: "Menüdeki «İpuçları» ile bu ekrana özel kısayolları görün." },
];

function tipsForPage(pageKey: string | null | undefined): TipItem[] {
  const k = (pageKey ?? "").trim();
  const pageTips: TipItem[] = (() => {
    switch (k) {
      case "personeller":
        return [
          { icon: "magnify", title: "Ara", detail: "Ad, soyad veya sicil no yazıp Ara’ya basın." },
          { icon: "filter-outline", title: "Filtreler", detail: "Firma, durum, kart tipi ve işyeri ile listeyi daraltın." },
          { icon: "refresh", title: "Yenile", detail: "Ara veya sayfa değiştirerek listeyi yenileyin." },
          { icon: "plus", title: "Yeni personel", detail: "Yetkiniz varsa sağ alttaki + ile ekleyin." },
        ];
      case "kisiHareketleri":
        return [
          { icon: "filter-outline", title: "Filtreler", detail: "Tarih ve personel filtreleriyle hareketleri daraltın." },
          { icon: "refresh", title: "Yenile", detail: "Listeyi yeniden yüklemek için Ara / Yenile kullanın." },
        ];
      case "izinler":
      case "izinTalepleri":
        return [
          { icon: "filter-outline", title: "Filtreler", detail: "Firma, personel ve izin tipi ile süzün." },
          { icon: "calendar", title: "Tarih aralığı", detail: "Başlangıç / bitiş tarihlerini ayarlayın." },
          { icon: "refresh", title: "Yenile", detail: "Filtre sonrası Ara ile listeyi güncelleyin." },
        ];
      case "raporlar":
        return [
          { icon: "file-chart", title: "Rapor seçimi", detail: "Rapor tipini seçip filtreleri doldurun." },
          { icon: "eye-outline", title: "Önizleme", detail: "Oluşturulan raporu önizleyin / paylaşın." },
        ];
      case "puantaj":
        return [
          { icon: "calendar-month", title: "Ay / firma", detail: "Ay ve firma seçerek puantajı yükleyin." },
          { icon: "filter-outline", title: "Personel filtresi", detail: "İşyeri veya personel ile daraltın." },
          { icon: "refresh", title: "Yenile", detail: "Filtreleri uygulayınca liste yenilenir." },
        ];
      case "firmalar":
      case "isyerleri":
      case "departmanlar":
      case "pozisyonlar":
      case "vardiyalar":
      case "calismaStatuleri":
      case "cihazlar":
      case "resmiTatiller":
        return [
          { icon: "magnify", title: "Ara", detail: "Üstteki arama kutusundan kayıt süzün." },
          { icon: "plus", title: "Ekle / düzenle", detail: "Yetkiniz varsa satır veya + ile işlem yapın." },
          { icon: "refresh", title: "Yenile", detail: "Hata durumunda Yenile ile tekrar deneyin." },
        ];
      case "dashboard":
        return [
          { icon: "refresh", title: "Yenile", detail: "Aşağı çekerek veya yenile ile özeti güncelleyin." },
          { icon: "bell-outline", title: "Bildirimler", detail: "Üstteki zilden bildirimlere erişin." },
        ];
      case "profil":
      case "izinlerim":
      case "avanslarim":
      case "yetkiliPaneli":
      case "qrGiris":
        return [
          { icon: "account", title: "Personel alanı", detail: "Profil ve talepleriniz bu bölümde." },
          { icon: "refresh", title: "Yenile", detail: "Ekranı yeniden açarak veya yenile ile güncelleyin." },
        ];
      default:
        return [
          { icon: "information-outline", title: "İpuçları", detail: "Menüden «İpuçları» ile bu ekrana özel yardım metinlerini açabilirsiniz." },
        ];
    }
  })();
  return [...pageTips, ...GLOBAL_TIPS];
}

export function TipsSheet(props: {
  visible: boolean;
  pageKey?: string | null;
  pageTitle?: string | null;
  onClose: () => void;
}) {
  const tips = useMemo(() => tipsForPage(props.pageKey), [props.pageKey]);
  const title = props.pageTitle?.trim() ? `İpuçları — ${props.pageTitle.trim()}` : "İpuçları";

  useEffect(() => {
    if (!props.visible) return;

    const backSub = BackHandler.addEventListener("hardwareBackPress", () => {
      props.onClose();
      return true;
    });

    let removeEsc: (() => void) | undefined;
    if (Platform.OS === "web" && typeof document !== "undefined") {
      const onKeyDown = (e: KeyboardEvent) => {
        if (e.key === "Escape") props.onClose();
      };
      document.addEventListener("keydown", onKeyDown);
      removeEsc = () => document.removeEventListener("keydown", onKeyDown);
    }

    return () => {
      backSub.remove();
      removeEsc?.();
    };
  }, [props.visible, props.onClose]);

  return (
    <Modal visible={props.visible} transparent animationType="slide" onRequestClose={props.onClose}>
      <TouchableOpacity className="flex-1 bg-black/45 justify-end" activeOpacity={1} onPress={props.onClose}>
        <TouchableOpacity activeOpacity={1} onPress={() => {}} className="bg-white rounded-t-3xl max-h-[78%]">
            <View className="px-5 pt-4 pb-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
            <View className="flex-row items-center flex-1 pr-3">
              <MaterialCommunityIcons name="help-circle-outline" size={22} color="#dc2626" />
              <Text className="ml-2 text-[#0f172a] font-extrabold text-[16px]" numberOfLines={1}>
                {title}
              </Text>
            </View>
            <TouchableOpacity onPress={props.onClose} className="w-8 h-8 items-center justify-center rounded-lg">
              <Text className="text-[#64748b] font-extrabold text-[18px] leading-none">✕</Text>
            </TouchableOpacity>
          </View>
          <ScrollView className="px-5 py-3" contentContainerStyle={{ paddingBottom: 28 }}>
            {tips.map((t, i) => (
              <View key={`${t.title}_${i}`} className="flex-row items-start py-3 border-b border-[#f1f5f9]">
                <View className="w-9 h-9 rounded-xl bg-[#fef2f2] items-center justify-center">
                  <MaterialCommunityIcons name={t.icon || "lightbulb-outline"} size={18} color="#dc2626" />
                </View>
                <View className="ml-3 flex-1">
                  <Text className="text-[#0f172a] font-extrabold">{t.title}</Text>
                  <Text className="text-[#64748b] font-semibold mt-1 text-[13px]">{t.detail}</Text>
                </View>
              </View>
            ))}
          </ScrollView>
        </TouchableOpacity>
      </TouchableOpacity>
    </Modal>
  );
}
