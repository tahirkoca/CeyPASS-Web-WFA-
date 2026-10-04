/** Tüm kartların anlık serbest/kısıtlı listesi; toplu veya tekil kartKomut kuyruğu. */
import React, { useMemo, useState } from "react";
import { Alert, Modal, ScrollView, Text, TouchableOpacity, View } from "react-native";
import { canliIzlemeKart, type AtamaListeSatir } from "../services/canliIzlemeApi";

type Tip = "tumu" | "misafir" | "arac";

type Props = {
  visible: boolean;
  token: string;
  rows: AtamaListeSatir[];
  onClose: () => void;
  onChanged: () => void;
};

/** Kart durumları (tip filtresi: Tümü/Misafir/Araç); seçili satırlara cihaz komutu yazar. */
export function CanliIzlemeKartDurumModal({ visible, token, rows, onClose, onChanged }: Props) {
  const [tip, setTip] = useState<Tip>("tumu");
  const [selected, setSelected] = useState<Record<string, boolean>>({});
  const [busy, setBusy] = useState(false);
  const [local, setLocal] = useState<AtamaListeSatir[]>(rows);

  React.useEffect(() => {
    if (visible) {
      setLocal(rows);
      setSelected({});
      setTip("tumu");
    }
  }, [visible, rows]);

  const shown = useMemo(
    () => local.filter((r) => tip === "tumu" || r.tip === tip),
    [local, tip]
  );

  const enqueue = async (targets: AtamaListeSatir[], pasif: boolean, empty: string) => {
    if (targets.length === 0) {
      Alert.alert("Kart durumları", empty);
      return;
    }
    let hazirAtlanan = 0;
    if (pasif) {
      hazirAtlanan = targets.filter((r) => r.durum === "Hazir").length;
      targets = targets.filter((r) => r.durum !== "Hazir");
      if (targets.length === 0) {
        Alert.alert("Kart durumları", "HAZIR durumdaki kartlar kısıtlanamaz.");
        return;
      }
    }
    const atlananNotu = hazirAtlanan > 0 ? `\n${hazirAtlanan} HAZIR kart atlandı.` : "";
    const ok = await new Promise<boolean>((resolve) => {
      Alert.alert(
        pasif ? "Kartı Kısıtla" : "Kart Kısıtı Kaldır",
        `${targets.length} kart için komut yazılsın mı?${atlananNotu}`,
        [
          { text: "Vazgeç", style: "cancel", onPress: () => resolve(false) },
          { text: pasif ? "Kısıtla" : "Kısıtı kaldır", onPress: () => resolve(true) },
        ]
      );
    });
    if (!ok) return;
    setBusy(true);
    let done = 0;
    try {
      for (const r of targets) {
        const res = await canliIzlemeKart.kartKomut(token, r.personelId, pasif);
        if (!res.success) throw new Error(res.message || "Komut yazılamadı.");
        done++;
        setLocal((prev) => prev.map((x) => (x.personelId === r.personelId ? { ...x, cihazdaAktif: !pasif } : x)));
      }
      Alert.alert("Kart durumları", `${done} komut kuyruğa alındı.${atlananNotu}`);
      onChanged();
    } catch (e: any) {
      if (done > 0) onChanged();
      Alert.alert("Kart durumları", done > 0 ? `${done} kart yazıldı, sonra hata:\n${e.message}` : e.message);
    } finally {
      setBusy(false);
    }
  };

  const chip = (key: Tip, label: string) => (
    <TouchableOpacity
      key={key}
      onPress={() => setTip(key)}
      className="px-3 py-2 rounded-lg mr-2"
      style={{ backgroundColor: tip === key ? "#1e293b" : "#e2e8f0" }}
    >
      <Text style={{ color: tip === key ? "#fff" : "#334155" }} className="font-extrabold text-[11px]">
        {label}
      </Text>
    </TouchableOpacity>
  );

  const action = (label: string, bg: string, onPress: () => void) => (
    <TouchableOpacity disabled={busy} onPress={onPress} className="px-2 py-2 rounded-lg mr-2 mb-2" style={{ backgroundColor: bg, opacity: busy ? 0.6 : 1 }}>
      <Text className="text-white font-extrabold text-[10px]">{label}</Text>
    </TouchableOpacity>
  );

  return (
    <Modal visible={visible} animationType="slide" onRequestClose={onClose}>
      <View className="flex-1 bg-[#f8fafc] pt-12">
        <View className="px-4 pb-2 flex-row justify-between items-center">
          <Text className="text-[18px] font-extrabold text-[#0f172a]">Kart durumları</Text>
          <TouchableOpacity onPress={onClose}>
            <Text className="text-[#2563eb] font-extrabold">Kapat</Text>
          </TouchableOpacity>
        </View>
        <View className="px-4 flex-row mb-2">
          {chip("tumu", "Tümü")}
          {chip("misafir", "Misafir")}
          {chip("arac", "Araç")}
        </View>
        <View className="px-4 flex-row flex-wrap">
          {action("Tümünü seç", "#3b82f6", () => {
            const next = { ...selected };
            shown.forEach((r) => { next[r.personelId] = true; });
            setSelected(next);
          })}
          {action("Seçimi kaldır", "#64748b", () => {
            const next = { ...selected };
            shown.forEach((r) => { next[r.personelId] = false; });
            setSelected(next);
          })}
          {action("Seçilenleri kısıtla", "#ea580c", () => enqueue(shown.filter((r) => selected[r.personelId]), true, "Önce kart seçin."))}
          {action("Seçilenleri serbest bırak", "#0d9488", () => enqueue(shown.filter((r) => selected[r.personelId]), false, "Önce kart seçin."))}
          {action("Tüm serbestleri kısıtla", "#dc2626", () => enqueue(shown.filter((r) => r.cihazdaAktif), true, "Serbest kart yok."))}
          {action("Tüm kısıtlıları serbest bırak", "#16a34a", () => enqueue(shown.filter((r) => !r.cihazdaAktif), false, "Kısıtlı kart yok."))}
        </View>
        <ScrollView className="flex-1 px-4">
          {shown.map((r) => (
            <TouchableOpacity
              key={`${r.tip}-${r.personelId}`}
              onPress={() => setSelected((s) => ({ ...s, [r.personelId]: !s[r.personelId] }))}
              className="mb-2 p-3 rounded-xl bg-white border border-[#e2e8f0]"
            >
              <Text className="font-extrabold text-[#0f172a]">
                {selected[r.personelId] ? "☑ " : "☐ "}
                {r.kartAdi}
              </Text>
              <Text className="text-[#475569] text-[12px]">{r.tipLabel} • {r.kisiPlaka || "-"} • {r.durumText} • {r.cihazdaAktif ? "Serbest" : "Kısıtlı"}</Text>
            </TouchableOpacity>
          ))}
        </ScrollView>
      </View>
    </Modal>
  );
}
