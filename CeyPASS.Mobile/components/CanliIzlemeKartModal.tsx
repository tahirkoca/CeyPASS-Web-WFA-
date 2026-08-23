import React, { useEffect, useMemo, useState } from "react";
import {
  ActivityIndicator,
  FlatList,
  Modal,
  ScrollView,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import {
  canliIzlemeKart,
  type GecmisZiyaretci,
  type KartAtamaItem,
  type KartListItem,
} from "../services/canliIzlemeApi";
import { gosterim, looksMasked, mask, resolveForSave } from "../utils/tcKimlik";

type Kind = "misafir" | "arac";
type Mode = "yeni" | "guncelle";

type Props = {
  visible: boolean;
  token: string;
  kind: Kind;
  mode: Mode;
  onClose: () => void;
  onSaved: (message: string) => void;
};

function toIsoLocal(d: Date) {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:00`;
}

export function CanliIzlemeKartModal({ visible, token, kind, mode, onClose, onSaved }: Props) {
  const title =
    mode === "yeni"
      ? kind === "misafir"
        ? "Misafir Kart Atama - Yeni"
        : "Araç Kartı Ver"
      : kind === "misafir"
        ? "Misafir Kart - Güncelle"
        : "Araç Kartını Güncelle";

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [cards, setCards] = useState<KartListItem[]>([]);
  const [aktifler, setAktifler] = useState<KartAtamaItem[]>([]);
  const [gecmis, setGecmis] = useState<GecmisZiyaretci[]>([]);
  const [gecmisFilter, setGecmisFilter] = useState("");

  const [personelId, setPersonelId] = useState("");
  const [atamaId, setAtamaId] = useState<number | null>(null);
  const [adSoyad, setAdSoyad] = useState("");
  const [tc, setTc] = useState("");
  const [tamTc, setTamTc] = useState<string | null>(null);
  const [plaka, setPlaka] = useState("");
  const [kimeGeldigi, setKimeGeldigi] = useState("");
  const [aciklama, setAciklama] = useState("");
  const [girisSaati, setGirisSaati] = useState(toIsoLocal(new Date()));
  const [cikisSaati, setCikisSaati] = useState(toIsoLocal(new Date()));

  const [cardPicker, setCardPicker] = useState(false);
  const [atamaPicker, setAtamaPicker] = useState(false);

  const selectedCardName = useMemo(
    () => cards.find((c) => c.personelId === personelId)?.adSoyad ?? "Kart seçin",
    [cards, personelId]
  );

  const selectedAtamaName = useMemo(() => {
    const a = aktifler.find((x) => x.atamaId === atamaId);
    if (!a) return "Atama seçin";
    return `${a.kartAdi || a.kartId} - ${a.adSoyad}`;
  }, [aktifler, atamaId]);

  const loadGecmis = async (ad?: string) => {
    if (mode !== "yeni") return;
    try {
      const res = await canliIzlemeKart.gecmis(token, kind, ad);
      if (res.success && res.data) setGecmis(res.data);
      else setGecmis([]);
    } catch {
      setGecmis([]);
    }
  };

  useEffect(() => {
    if (!visible) return;
    setError(null);
    setGecmisFilter("");
    setAdSoyad("");
    setTc("");
    setTamTc(null);
    setPlaka("");
    setKimeGeldigi("");
    setAciklama("");
    setPersonelId("");
    setAtamaId(null);
    setGirisSaati(toIsoLocal(new Date()));
    setCikisSaati(toIsoLocal(new Date()));

    (async () => {
      setLoading(true);
      try {
        if (mode === "yeni") {
          const [cRes] = await Promise.all([canliIzlemeKart.kartlar(token, kind), loadGecmis("")]);
          if (cRes.success && cRes.data) {
            setCards(cRes.data);
            if (cRes.data.length > 0) setPersonelId(cRes.data[0].personelId);
          } else setCards([]);
        } else {
          const res = await canliIzlemeKart.aktif(token, kind);
          if (res.success && res.data) {
            setAktifler(res.data);
            if (res.data.length > 0) applyAtama(res.data[0]);
            else setAtamaId(null);
          } else setAktifler([]);
        }
      } catch (e: any) {
        setError(e?.response?.data?.message ?? e?.message ?? "Veriler yüklenemedi.");
      } finally {
        setLoading(false);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [visible, kind, mode, token]);

  useEffect(() => {
    if (!visible || mode !== "yeni") return;
    const t = setTimeout(() => loadGecmis(gecmisFilter.trim()), 250);
    return () => clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [gecmisFilter]);

  const showMaskedTc = (full?: string | null) => {
    const t = (full ?? "").trim();
    setTamTc(t || null);
    setTc(t ? mask(t) : "");
  };

  const onTcChange = (v: string) => {
    if (tamTc && v === mask(tamTc)) {
      setTc(v);
      return;
    }
    setTamTc(null);
    setTc(v);
  };

  const applyAtama = (a: KartAtamaItem) => {
    setAtamaId(a.atamaId);
    setAdSoyad(a.adSoyad || "");
    showMaskedTc(a.tcKimlikNo);
    setPlaka(a.plaka || "");
    setKimeGeldigi(a.ziyaretEdilenKisi || "");
    setAciklama(a.notlar || "");
    setGirisSaati(a.baslangic ? toIsoLocal(new Date(a.baslangic)) : toIsoLocal(new Date()));
  };

  const tryFillByTc = async () => {
    const t = tc.trim();
    if (!t || looksMasked(t) || mode !== "yeni") return;
    try {
      const res = await canliIzlemeKart.byTc(token, kind, t);
      if (!res.success || !res.data) return;
      if (res.data.adSoyad && !adSoyad.trim()) setAdSoyad(res.data.adSoyad);
      if (res.data.ziyaretEdilenKisi && !kimeGeldigi.trim()) setKimeGeldigi(res.data.ziyaretEdilenKisi);
      if (res.data.plaka && !plaka.trim()) setPlaka(res.data.plaka);
    } catch {
      /* sessiz */
    }
  };

  const selectGecmis = (item: GecmisZiyaretci) => {
    setAdSoyad(item.adSoyad || "");
    showMaskedTc(item.tcKimlikNo);
    setPlaka(item.plaka || "");
    setKimeGeldigi(item.ziyaretEdilenKisi || "");
    setGirisSaati(toIsoLocal(new Date()));
  };

  const save = async () => {
    setError(null);
    if (!adSoyad.trim()) {
      setError("Ad soyad zorunludur.");
      return;
    }
    let tcToSave: string;
    try {
      tcToSave = resolveForSave(tc, tamTc);
    } catch (e: any) {
      setError(e?.message ?? "T.C. Kimlik No geçersiz.");
      return;
    }
    if (kind === "arac" && !plaka.trim()) {
      setError("Plaka giriniz.");
      return;
    }
    setSaving(true);
    try {
      if (mode === "yeni") {
        if (!personelId) {
          setError("Kart seçiniz.");
          return;
        }
        const res = await canliIzlemeKart.create(token, kind, {
          personelId,
          adSoyad: adSoyad.trim(),
          girisSaati,
          aciklama,
          tcKimlikNo: tcToSave,
          ziyaretEdilenKisi: kimeGeldigi.trim() || undefined,
          plaka: kind === "arac" ? plaka.trim() : undefined,
        });
        if (!res.success) {
          setError(res.message ?? "Kayıt başarısız.");
          return;
        }
        onSaved(res.message ?? "Kayıt başarıyla oluşturuldu.");
        onClose();
      } else {
        if (!atamaId) {
          setError("Güncellenecek atama seçiniz.");
          return;
        }
        const res = await canliIzlemeKart.update(token, kind, atamaId, {
          adSoyad: adSoyad.trim(),
          girisSaati,
          cikisSaati,
          aciklama,
          tcKimlikNo: tcToSave,
          ziyaretEdilenKisi: kimeGeldigi.trim() || undefined,
          plaka: kind === "arac" ? plaka.trim() : undefined,
        });
        if (!res.success) {
          setError(res.message ?? "Güncelleme başarısız.");
          return;
        }
        onSaved(res.message ?? "Kayıt güncellendi.");
        onClose();
      }
    } catch (e: any) {
      setError(e?.response?.data?.message ?? e?.message ?? "İşlem başarısız.");
    } finally {
      setSaving(false);
    }
  };

  const Field = ({
    label,
    value,
    onChangeText,
    placeholder,
    onBlur,
    maxLength,
  }: {
    label: string;
    value: string;
    onChangeText: (v: string) => void;
    placeholder?: string;
    onBlur?: () => void;
    maxLength?: number;
  }) => (
    <View className="mb-3">
      <Text className="text-[12px] font-semibold text-[#64748b] mb-1">{label}</Text>
      <TextInput
        value={value}
        onChangeText={onChangeText}
        onBlur={onBlur}
        placeholder={placeholder}
        placeholderTextColor="#94a3b8"
        maxLength={maxLength}
        className="border-[1.5px] border-[#e2e8f0] rounded-xl px-3 py-3 bg-[#f8fafc] text-[#1e293b] font-semibold"
      />
    </View>
  );

  return (
    <Modal transparent visible={visible} animationType="slide" onRequestClose={onClose}>
      <View className="flex-1 bg-black/60 justify-end">
        <View className="bg-white rounded-t-3xl max-h-[92%]">
          <View className="px-5 py-4 flex-row items-center justify-between border-b border-[#f1f5f9]">
            <Text className="text-[#1e293b] font-extrabold text-[15px]" numberOfLines={1} style={{ flex: 1 }}>
              {title}
            </Text>
            <TouchableOpacity onPress={onClose} className="p-2">
              <MaterialCommunityIcons name="close" size={22} color="#64748b" />
            </TouchableOpacity>
          </View>

          {loading ? (
            <View className="py-16 items-center">
              <ActivityIndicator color="#dc2626" />
              <Text className="mt-3 text-[#64748b] font-semibold">Yükleniyor...</Text>
            </View>
          ) : (
            <ScrollView contentContainerStyle={{ padding: 16, paddingBottom: 30 }} keyboardShouldPersistTaps="handled">
              {error ? (
                <View className="mb-3 rounded-xl bg-[#fef2f2] px-4 py-3 border border-[#fee2e2]">
                  <Text className="text-[#dc2626] font-semibold text-[12px]">{error}</Text>
                </View>
              ) : null}

              {mode === "yeni" ? (
                <View className="mb-4">
                  <Text className="text-[13px] font-extrabold text-[#1e293b] mb-2">
                    {kind === "arac" ? "Geçmiş (isim / plaka)" : "Geçmiş Ziyaretçiler"}
                  </Text>
                  <TextInput
                    value={gecmisFilter}
                    onChangeText={setGecmisFilter}
                    placeholder={kind === "arac" ? "İsim veya plaka ara..." : "İsim ara..."}
                    placeholderTextColor="#94a3b8"
                    className="border-[1.5px] border-[#e2e8f0] rounded-xl px-3 py-2.5 bg-[#f8fafc] text-[#1e293b] font-semibold mb-2"
                  />
                  <View className="border border-[#e2e8f0] rounded-xl overflow-hidden" style={{ maxHeight: 160 }}>
                    <FlatList
                      data={gecmis}
                      keyExtractor={(item, idx) => `${item.adSoyad}-${idx}`}
                      nestedScrollEnabled
                      keyboardShouldPersistTaps="handled"
                      ListEmptyComponent={
                        <Text className="px-3 py-4 text-[#94a3b8] font-semibold text-[12px]">Kayıt yok</Text>
                      }
                      renderItem={({ item }) => (
                        <TouchableOpacity
                          onPress={() => selectGecmis(item)}
                          className="px-3 py-3 border-b border-[#f1f5f9]"
                          activeOpacity={0.8}
                        >
                          <Text className="text-[#1e293b] font-semibold">
                            {gosterim(item)}
                          </Text>
                        </TouchableOpacity>
                      )}
                    />
                  </View>
                </View>
              ) : null}

              {mode === "yeni" ? (
                <View className="mb-3">
                  <Text className="text-[12px] font-semibold text-[#64748b] mb-1">Kart</Text>
                  <TouchableOpacity
                    onPress={() => setCardPicker(true)}
                    className="border-[1.5px] border-[#e2e8f0] rounded-xl px-3 py-3 bg-[#f8fafc] flex-row justify-between items-center"
                  >
                    <Text className="text-[#1e293b] font-semibold" numberOfLines={1} style={{ flex: 1 }}>
                      {selectedCardName}
                    </Text>
                    <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" />
                  </TouchableOpacity>
                </View>
              ) : (
                <View className="mb-3">
                  <Text className="text-[12px] font-semibold text-[#64748b] mb-1">Atama</Text>
                  {aktifler.length === 0 ? (
                    <Text className="text-[#64748b] font-semibold text-[12px] mb-2">
                      Bugün aktif atama bulunamadı.
                    </Text>
                  ) : (
                    <TouchableOpacity
                      onPress={() => setAtamaPicker(true)}
                      className="border-[1.5px] border-[#e2e8f0] rounded-xl px-3 py-3 bg-[#f8fafc] flex-row justify-between items-center"
                    >
                      <Text className="text-[#1e293b] font-semibold" numberOfLines={1} style={{ flex: 1 }}>
                        {selectedAtamaName}
                      </Text>
                      <MaterialCommunityIcons name="chevron-down" size={20} color="#64748b" />
                    </TouchableOpacity>
                  )}
                </View>
              )}

              <Field
                label="T.C. Kimlik No *"
                value={tc}
                onChangeText={onTcChange}
                placeholder="11 hane"
                onBlur={tryFillByTc}
                maxLength={11}
              />
              {kind === "arac" ? (
                <Field label="Araç Plakası *" value={plaka} onChangeText={setPlaka} placeholder="Plaka giriniz" maxLength={20} />
              ) : null}
              <Field label="Ad Soyad" value={adSoyad} onChangeText={setAdSoyad} />
              <Field label="Kime Geldiği" value={kimeGeldigi} onChangeText={setKimeGeldigi} />
              <Field label="Giriş Saati (ISO)" value={girisSaati} onChangeText={setGirisSaati} />
              {mode === "guncelle" ? (
                <Field label="Çıkış Saati (ISO)" value={cikisSaati} onChangeText={setCikisSaati} />
              ) : null}
              <Field label="Açıklama" value={aciklama} onChangeText={setAciklama} />

              <TouchableOpacity
                onPress={save}
                disabled={saving || (mode === "guncelle" && aktifler.length === 0)}
                activeOpacity={0.85}
                className="mt-2 rounded-xl py-4 items-center"
                style={{ backgroundColor: kind === "arac" ? "#f59e0b" : "#2563eb", opacity: saving ? 0.7 : 1 }}
              >
                {saving ? (
                  <ActivityIndicator color="white" />
                ) : (
                  <Text className="text-white font-extrabold">{mode === "yeni" ? "Kaydet" : "Güncelle"}</Text>
                )}
              </TouchableOpacity>
            </ScrollView>
          )}
        </View>
      </View>

      <Modal transparent visible={cardPicker} animationType="fade" onRequestClose={() => setCardPicker(false)}>
        <View className="flex-1 bg-black/60 items-center justify-center px-6">
          <View className="w-full rounded-3xl bg-white p-5 max-h-[70%]">
            <View className="flex-row items-center justify-between mb-3">
              <Text className="text-[15px] font-extrabold text-[#1e293b]">Kart Seç</Text>
              <TouchableOpacity onPress={() => setCardPicker(false)} className="p-2">
                <MaterialCommunityIcons name="close" size={20} color="#64748b" />
              </TouchableOpacity>
            </View>
            <ScrollView>
              {cards.map((c) => (
                <TouchableOpacity
                  key={c.personelId}
                  onPress={() => {
                    setPersonelId(c.personelId);
                    setCardPicker(false);
                  }}
                  className="py-4 border-b border-[#f1f5f9]"
                >
                  <Text className="font-semibold text-[#1e293b]">{c.adSoyad}</Text>
                </TouchableOpacity>
              ))}
            </ScrollView>
          </View>
        </View>
      </Modal>

      <Modal transparent visible={atamaPicker} animationType="fade" onRequestClose={() => setAtamaPicker(false)}>
        <View className="flex-1 bg-black/60 items-center justify-center px-6">
          <View className="w-full rounded-3xl bg-white p-5 max-h-[70%]">
            <View className="flex-row items-center justify-between mb-3">
              <Text className="text-[15px] font-extrabold text-[#1e293b]">Atama Seç</Text>
              <TouchableOpacity onPress={() => setAtamaPicker(false)} className="p-2">
                <MaterialCommunityIcons name="close" size={20} color="#64748b" />
              </TouchableOpacity>
            </View>
            <ScrollView>
              {aktifler.map((a) => (
                <TouchableOpacity
                  key={a.atamaId}
                  onPress={() => {
                    applyAtama(a);
                    setAtamaPicker(false);
                  }}
                  className="py-4 border-b border-[#f1f5f9]"
                >
                  <Text className="font-semibold text-[#1e293b]">
                    {a.kartAdi || a.kartId} - {a.adSoyad}
                  </Text>
                </TouchableOpacity>
              ))}
            </ScrollView>
          </View>
        </View>
      </Modal>
    </Modal>
  );
}
