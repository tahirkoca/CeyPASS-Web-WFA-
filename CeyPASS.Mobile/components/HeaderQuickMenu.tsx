import React, { useMemo, useState } from "react";
import { Modal, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import { profilService } from "../services/profilApi";
import { notificationService } from "../services/notificationApi";

export function useHeaderQuickMenu() {
  const [visible, setVisible] = useState(false);
  const [tab, setTab] = useState<"notif" | "password">("notif");

  const [notifLoading, setNotifLoading] = useState(false);
  const [notifError, setNotifError] = useState<string | null>(null);
  const [notifPage, setNotifPage] = useState(1);
  const [notifTotalPages, setNotifTotalPages] = useState(1);
  const [notifItems, setNotifItems] = useState<any[]>([]);

  const [passOld, setPassOld] = useState("");
  const [passNew, setPassNew] = useState("");
  const [passNew2, setPassNew2] = useState("");
  const [passSaving, setPassSaving] = useState(false);
  const [passErr, setPassErr] = useState<string | null>(null);
  const [passOk, setPassOk] = useState<string | null>(null);

  const open = async (initialTab: "notif" | "password" = "notif") => {
    setTab(initialTab);
    setVisible(true);
    if (initialTab === "notif") await loadNotifications(1);
  };

  const close = () => setVisible(false);

  const loadNotifications = async (page: number, pageSize = 10) => {
    try {
      setNotifLoading(true);
      setNotifError(null);
      const res = await notificationService.history(page, pageSize);
      if (!res?.success) {
        setNotifItems([]);
        setNotifTotalPages(1);
        setNotifError(res?.message || "Bildirimler alınamadı.");
        return;
      }
      setNotifItems(res.data?.items ?? []);
      setNotifTotalPages(res.data?.totalPages ?? 1);
      setNotifPage(page);
    } catch (e: any) {
      setNotifItems([]);
      setNotifTotalPages(1);
      setNotifError(e?.message || "Bildirimler alınamadı.");
    } finally {
      setNotifLoading(false);
    }
  };

  const notifPageText = useMemo(() => `Sayfa ${notifPage}/${notifTotalPages}`, [notifPage, notifTotalPages]);

  const modal = (
    <Modal transparent visible={visible} animationType="fade" onRequestClose={close}>
      <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={close}>
        <TouchableOpacity activeOpacity={1} onPress={() => {}}>
          <View className="bg-white rounded-2xl overflow-hidden">
            <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
              <Text className="text-[#0f172a] font-extrabold text-[16px]">Kısayollar</Text>
              <TouchableOpacity onPress={close} className="px-3 py-2 bg-[#f1f5f9] rounded-xl">
                <Text className="text-[#334155] font-extrabold">Kapat</Text>
              </TouchableOpacity>
            </View>

            <View className="px-4 py-3 border-b border-[#f1f5f9]">
              <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={{ gap: 8 }}>
                <TouchableOpacity
                  onPress={async () => {
                    setTab("notif");
                    await loadNotifications(1);
                  }}
                  className={`px-3 py-2 rounded-xl border ${tab === "notif" ? "bg-[#0f172a] border-[#0f172a]" : "bg-[#f8fafc] border-[#e2e8f0]"}`}
                >
                  <Text className={`font-extrabold ${tab === "notif" ? "text-white" : "text-[#334155]"}`}>Bildirimler</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  onPress={() => {
                    setPassErr(null);
                    setPassOk(null);
                    setTab("password");
                  }}
                  className={`px-3 py-2 rounded-xl border ${tab === "password" ? "bg-[#0f172a] border-[#0f172a]" : "bg-[#f8fafc] border-[#e2e8f0]"}`}
                >
                  <Text className={`font-extrabold ${tab === "password" ? "text-white" : "text-[#334155]"}`}>Şifre</Text>
                </TouchableOpacity>
              </ScrollView>
            </View>

            {tab === "notif" ? (
              <View className="px-4 py-4" style={{ maxHeight: 520 }}>
                {notifLoading ? (
                  <Text className="text-[#64748b] font-semibold">Yükleniyor...</Text>
                ) : notifError ? (
                  <Text className="text-[#b91c1c] font-extrabold">{notifError}</Text>
                  ) : notifItems.length ? (
                  <ScrollView style={{ maxHeight: 420 }}>
                      {notifItems.map((n: any) => (
                        <TouchableOpacity
                          key={n.id}
                          className={`border-b border-[#f1f5f9] py-3 ${n.okunduMu ? "" : "bg-[#f8fafc]"}`}
                          onPress={async () => {
                            const id = Number(n.id);
                            if (!Number.isFinite(id) || id <= 0) return;
                            try {
                              await notificationService.markAsRead(id);
                            } catch {}
                            await loadNotifications(notifPage);
                          }}
                        >
                          <Text className="text-[#1e293b] font-extrabold" numberOfLines={1}>
                            {(n.baslik ?? "").toString() || " "}
                          </Text>
                          <Text className="text-[#64748b] font-semibold mt-1" numberOfLines={2}>
                            {(n.mesaj ?? "").toString() || " "}
                          </Text>
                          <Text className="text-[#94a3b8] font-semibold mt-1 text-[12px]">{(n.tarih ?? "").toString()}</Text>
                        </TouchableOpacity>
                      ))}
                  </ScrollView>
                ) : (
                  <Text className="text-[#64748b] font-semibold">Kayıt yok.</Text>
                )}

                <View className="mt-3 flex-row items-center justify-between">
                  <TouchableOpacity
                    disabled={notifLoading || !notifItems.length}
                    onPress={async () => {
                      try {
                        await notificationService.markAllAsRead();
                      } catch {}
                      await loadNotifications(1);
                    }}
                    className={`px-4 py-3 rounded-xl ${notifLoading || !notifItems.length ? "bg-[#f8fafc]" : "bg-[#fee2e2]"}`}
                  >
                    <Text className={`font-extrabold ${notifLoading || !notifItems.length ? "text-[#94a3b8]" : "text-[#b91c1c]"}`}>Tümünü Okundu</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    disabled={notifPage <= 1 || notifLoading}
                    onPress={async () => {
                      const p = Math.max(1, notifPage - 1);
                      await loadNotifications(p);
                    }}
                    className={`px-4 py-3 rounded-xl ${notifPage <= 1 || notifLoading ? "bg-[#f8fafc]" : "bg-[#f1f5f9]"}`}
                  >
                    <Text className={`font-extrabold ${notifPage <= 1 || notifLoading ? "text-[#94a3b8]" : "text-[#334155]"}`}>Önceki</Text>
                  </TouchableOpacity>
                  <Text className="text-[#64748b] font-semibold text-[12px]">{notifPageText}</Text>
                  <TouchableOpacity
                    disabled={notifPage >= notifTotalPages || notifLoading}
                    onPress={async () => {
                      const p = Math.min(notifTotalPages, notifPage + 1);
                      await loadNotifications(p);
                    }}
                    className={`px-4 py-3 rounded-xl ${notifPage >= notifTotalPages || notifLoading ? "bg-[#f8fafc]" : "bg-[#f1f5f9]"}`}
                  >
                    <Text className={`font-extrabold ${notifPage >= notifTotalPages || notifLoading ? "text-[#94a3b8]" : "text-[#334155]"}`}>Sonraki</Text>
                  </TouchableOpacity>
                </View>
              </View>
            ) : (
              <View className="px-4 py-4" style={{ maxHeight: 520 }}>
                <Text className="text-[#64748b] font-semibold mb-2">Eski Şifre</Text>
                <TextInput
                  value={passOld}
                  onChangeText={setPassOld}
                  placeholder="Eski şifreniz"
                  secureTextEntry
                  className="bg-white border border-[#e2e8f0] rounded-xl px-4 py-3 text-[#0f172a] font-semibold"
                />
                <Text className="text-[#64748b] font-semibold mb-2 mt-3">Yeni Şifre</Text>
                <TextInput
                  value={passNew}
                  onChangeText={setPassNew}
                  placeholder="Yeni şifre"
                  secureTextEntry
                  className="bg-white border border-[#e2e8f0] rounded-xl px-4 py-3 text-[#0f172a] font-semibold"
                />
                <Text className="text-[#64748b] font-semibold mb-2 mt-3">Yeni Şifre (Tekrar)</Text>
                <TextInput
                  value={passNew2}
                  onChangeText={setPassNew2}
                  placeholder="Yeni şifre tekrar"
                  secureTextEntry
                  className="bg-white border border-[#e2e8f0] rounded-xl px-4 py-3 text-[#0f172a] font-semibold"
                />

                {passErr ? <Text className="text-[#b91c1c] font-extrabold mt-3">{passErr}</Text> : null}
                {passOk ? <Text className="text-[#16a34a] font-extrabold mt-3">{passOk}</Text> : null}

                <View className="mt-4 flex-row">
                  <TouchableOpacity
                    disabled={passSaving}
                    onPress={async () => {
                      if (passSaving) return;
                      setPassErr(null);
                      setPassOk(null);
                      const o = (passOld ?? "").trim();
                      const n1 = (passNew ?? "").trim();
                      const n2 = (passNew2 ?? "").trim();
                      if (!o) return setPassErr("Eski şifre boş olamaz.");
                      if (!n1) return setPassErr("Yeni şifre boş olamaz.");
                      if (n1 !== n2) return setPassErr("Yeni şifreler eşleşmiyor.");
                      setPassSaving(true);
                      try {
                        const r = await profilService.sifreDegistir(o, n1);
                        if (!r?.success) return setPassErr(r?.message || "Şifre güncellenemedi.");
                        setPassOk(r?.message || "Şifreniz başarıyla güncellendi.");
                        setPassOld("");
                        setPassNew("");
                        setPassNew2("");
                      } catch (e: any) {
                        setPassErr(e?.response?.data?.message ?? e?.message ?? "Şifre güncellenemedi.");
                      } finally {
                        setPassSaving(false);
                      }
                    }}
                    className={`flex-1 rounded-xl py-3 items-center ${passSaving ? "bg-[#86efac]" : "bg-[#16a34a]"}`}
                  >
                    <Text className="text-white font-extrabold">{passSaving ? "Kaydediliyor..." : "Şifreyi Güncelle"}</Text>
                  </TouchableOpacity>
                </View>
              </View>
            )}
          </View>
        </TouchableOpacity>
      </TouchableOpacity>
    </Modal>
  );

  return { open, close, modal, refreshNotifications: () => loadNotifications(notifPage), setTab };
}

