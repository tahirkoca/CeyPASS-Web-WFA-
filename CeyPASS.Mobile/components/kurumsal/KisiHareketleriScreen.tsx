import React, { useEffect, useMemo, useRef, useState } from "react";
import { ActivityIndicator, FlatList, Modal, Platform, ScrollView, Text, TextInput, TouchableOpacity, View } from "react-native";
import DateTimePicker from "@react-native-community/datetimepicker";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { kisiHareketService, KisiHareketRow } from "../../services/kisiHareketApi";
import { StatusPopup } from "../StatusPopup";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";

function pick<T = any>(obj: any, a: string, b?: string): T | undefined {
  if (!obj) return undefined;
  if (obj[a] !== undefined) return obj[a] as T;
  if (b && obj[b] !== undefined) return obj[b] as T;
  return undefined;
}

function fmtDateTimeTR(value: any) {
  if (!value) return "-";
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return String(value);
  return d.toLocaleString("tr-TR", { year: "numeric", month: "2-digit", day: "2-digit", hour: "2-digit", minute: "2-digit", second: "2-digit" });
}

function tipLabel(tipRaw: any) {
  const t = (tipRaw ?? "").toString().trim();
  if (!t) return "-";
  if (t === "G" || t.toLowerCase() === "giris" || t.toLowerCase() === "giriş" || t === "GİRİŞ") return "Giriş";
  if (t === "Ç" || t === "C" || t.toLowerCase() === "cikis" || t.toLowerCase() === "çıkış" || t === "ÇIKIŞ") return "Çıkış";
  return t;
}

function toLocalIsoNoZ(d: Date) {
  const pad = (n: number) => String(n).padStart(2, "0");
  const yyyy = d.getFullYear();
  const mm = pad(d.getMonth() + 1);
  const dd = pad(d.getDate());
  const hh = pad(d.getHours());
  const mi = pad(d.getMinutes());
  const ss = pad(d.getSeconds());
  return `${yyyy}-${mm}-${dd}T${hh}:${mi}:${ss}`;
}

type PersonelItem = { id: number; ad: string };

function parsePersonelList(raw: any): PersonelItem[] {
  const list = raw ?? [];
  if (!Array.isArray(list)) return [];
  const out: PersonelItem[] = [];
  for (const it of list) {
    const id = Number(pick<any>(it, "id", "Id"));
    const ad = (pick<any>(it, "ad", "Ad") ?? pick<any>(it, "adSoyad", "AdSoyad") ?? "").toString();
    if (Number.isFinite(id) && id > 0 && ad) out.push({ id, ad });
  }
  return out.sort((a, b) => a.ad.localeCompare(b.ad, "tr"));
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
            <ScrollView style={{ maxHeight: 420 }}>
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

function MultiPersonelModal(props: {
  visible: boolean;
  title: string;
  items: PersonelItem[];
  selectedIds: number[];
  onChange: (next: number[]) => void;
  onClose: () => void;
}) {
  const [q, setQ] = useState("");
  useEffect(() => {
    if (!props.visible) setQ("");
  }, [props.visible]);

  const filtered = useMemo(() => {
    const qq = q.trim().toLowerCase();
    if (!qq) return props.items;
    return props.items.filter((x) => x.ad.toLowerCase().includes(qq) || String(x.id).includes(qq));
  }, [q, props.items]);

  const selSet = useMemo(() => new Set(props.selectedIds), [props.selectedIds]);

  return (
    <Modal visible={props.visible} transparent animationType="fade" onRequestClose={props.onClose}>
      <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={props.onClose}>
        <TouchableOpacity activeOpacity={1} onPress={() => {}}>
          <View className="bg-white rounded-2xl overflow-hidden">
            <View className="px-4 py-3 border-b border-[#f1f5f9]">
              <Text className="text-[#0f172a] font-extrabold text-[16px]">{props.title}</Text>
              <TextInput
                value={q}
                onChangeText={setQ}
                placeholder="Ara (Ad/Soyad veya ID)"
                className="mt-3 px-3 py-2 rounded-xl bg-[#f8fafc] border border-[#e2e8f0] text-[#0f172a]"
              />
              <View className="flex-row gap-2 mt-3">
                <TouchableOpacity
                  className="flex-1 bg-[#f1f5f9] rounded-xl py-2 items-center"
                  onPress={() => props.onChange([])}
                >
                  <Text className="text-[#334155] font-extrabold">Temizle</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  className="flex-1 bg-[#fee2e2] rounded-xl py-2 items-center"
                  onPress={() => props.onClose()}
                >
                  <Text className="text-[#dc2626] font-extrabold">Bitti</Text>
                </TouchableOpacity>
              </View>
            </View>

            <View style={{ maxHeight: 520 }}>
              <FlatList
                data={filtered}
                keyExtractor={(p) => String(p.id)}
                keyboardShouldPersistTaps="handled"
                initialNumToRender={18}
                maxToRenderPerBatch={24}
                windowSize={8}
                removeClippedSubviews
                renderItem={({ item: p }) => {
                  const checked = selSet.has(p.id);
                  return (
                    <TouchableOpacity
                      className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center"
                      onPress={() => {
                        const next = new Set(selSet);
                        if (checked) next.delete(p.id);
                        else next.add(p.id);
                        props.onChange(Array.from(next.values()).sort((a, b) => a - b));
                      }}
                    >
                      <MaterialCommunityIcons
                        name={checked ? "checkbox-marked" : "checkbox-blank-outline"}
                        size={22}
                        color={checked ? "#16a34a" : "#64748b"}
                      />
                      <View className="ml-3 flex-1">
                        <Text className="text-[#0f172a] font-semibold" numberOfLines={1}>
                          {p.ad}
                        </Text>
                        <Text className="text-[#64748b] font-semibold text-[12px]">#{p.id}</Text>
                      </View>
                    </TouchableOpacity>
                  );
                }}
                ListEmptyComponent={
                  <View className="px-4 py-6">
                    <Text className="text-[#64748b] font-semibold text-center">Sonuç yok</Text>
                  </View>
                }
              />
            </View>
          </View>
        </TouchableOpacity>
      </TouchableOpacity>
    </Modal>
  );
}

function PersonelSingleModal(props: {
  visible: boolean;
  title: string;
  items: PersonelItem[];
  selectedId: number | null;
  onPick: (id: number) => void;
  onClose: () => void;
}) {
  const [q, setQ] = useState("");
  useEffect(() => {
    if (!props.visible) setQ("");
  }, [props.visible]);

  const filtered = useMemo(() => {
    const qq = q.trim().toLowerCase();
    if (!qq) return props.items;
    return props.items.filter((x) => x.ad.toLowerCase().includes(qq) || String(x.id).includes(qq));
  }, [q, props.items]);

  return (
    <Modal visible={props.visible} transparent animationType="fade" onRequestClose={props.onClose}>
      <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={props.onClose}>
        <TouchableOpacity activeOpacity={1} onPress={() => {}}>
          <View className="bg-white rounded-2xl overflow-hidden">
            <View className="px-4 py-3 border-b border-[#f1f5f9]">
              <Text className="text-[#0f172a] font-extrabold text-[16px]">{props.title}</Text>
              <TextInput
                value={q}
                onChangeText={setQ}
                placeholder="Ara (Ad/Soyad veya ID)"
                className="mt-3 px-3 py-2 rounded-xl bg-[#f8fafc] border border-[#e2e8f0] text-[#0f172a]"
              />
            </View>

            <View style={{ height: 520 }}>
              <FlatList
                data={filtered}
                keyExtractor={(p) => String(p.id)}
                keyboardShouldPersistTaps="handled"
                initialNumToRender={18}
                maxToRenderPerBatch={24}
                windowSize={8}
                removeClippedSubviews
                renderItem={({ item: p }) => {
                  const checked = props.selectedId === p.id;
                  return (
                    <TouchableOpacity
                      className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center"
                      onPress={() => {
                        props.onPick(p.id);
                        props.onClose();
                      }}
                    >
                      <MaterialCommunityIcons
                        name={checked ? "radiobox-marked" : "radiobox-blank"}
                        size={22}
                        color={checked ? "#16a34a" : "#64748b"}
                      />
                      <View className="ml-3 flex-1">
                        <Text className="text-[#0f172a] font-semibold" numberOfLines={1}>
                          {p.ad}
                        </Text>
                        <Text className="text-[#64748b] font-semibold text-[12px]">#{p.id}</Text>
                      </View>
                    </TouchableOpacity>
                  );
                }}
                ListEmptyComponent={
                  <View className="px-4 py-6">
                    <Text className="text-[#64748b] font-semibold text-center">Sonuç yok</Text>
                  </View>
                }
              />
            </View>

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

function RowLabel({ label, value }: { label: string; value: string }) {
  return (
    <View className="flex-row justify-between py-2">
      <Text className="text-[#64748b] font-semibold">{label}</Text>
      <Text className="text-[#0f172a] font-extrabold">{value}</Text>
    </View>
  );
}

export function KisiHareketleriScreen(props: { user: any; abilities: any; onOpenMenu: () => void }) {
  const actions = props.abilities?.actions?.KisiHareketler ?? props.abilities?.Actions?.KisiHareketler ?? {};
  const canCreate = !!(actions?.Create ?? actions?.create);
  const canUpdate = !!(actions?.Update ?? actions?.update);
  const canDelete = !!(actions?.Delete ?? actions?.delete);
  const quickMenu = useHeaderQuickMenu();
  const notif = useNotificationsContext();
  const header = (
    <>
      <PageHeader
        title="Kişi Hareketleri"
        onOpenMenu={props.onOpenMenu}
        rightIcon={canCreate ? "plus" : undefined}
        onRightPress={
          canCreate
            ? () => {
                setEklePersonelId(null);
                setEkleTip("GİRİŞ");
                setEkleTarih(new Date());
                setEkleVisible(true);
              }
            : undefined
        }
        rightIcon2="bell-outline"
        onRightPress2={() => quickMenu.open("notif")}
        rightBadge2={notif.unreadCount}
        rightA11yLabel2="Bildirimler ve hesap"
      />
      {quickMenu.modal}
    </>
  );
  const rolId: number | null | undefined = props.abilities?.rolId ?? props.abilities?.RolId;
  const isAdmin = !!(props.abilities?.isAdmin ?? props.abilities?.IsAdmin ?? (rolId === 1 || rolId === 2));

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [firmalar, setFirmalar] = useState<any[]>([]);
  const [aktifFirma, setAktifFirma] = useState<any | null>(null);
  const [personelList, setPersonelList] = useState<PersonelItem[]>([]);

  const [kartTipi, setKartTipi] = useState<"puantaj" | "puantajsiz">("puantaj");
  const [firmaId, setFirmaId] = useState<number | null>(null);
  const [personelIds, setPersonelIds] = useState<number[]>([]);

  const [sadeceAktif, setSadeceAktif] = useState(false);
  const [sadecePasif, setSadecePasif] = useState(false);
  const [sadeceYemekhane, setSadeceYemekhane] = useState(false);

  const [baslangic, setBaslangic] = useState<Date>(() => {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return d;
  });
  const [bitis, setBitis] = useState<Date>(() => {
    const d = new Date();
    d.setHours(23, 59, 59, 0);
    return d;
  });

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(50);

  const [items, setItems] = useState<KisiHareketRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [firmaModal, setFirmaModal] = useState(false);
  const [kartTipiModal, setKartTipiModal] = useState(false);
  const [pageSizeModal, setPageSizeModal] = useState(false);
  const [personelModal, setPersonelModal] = useState(false);

  const [pickerOpen, setPickerOpen] = useState<null | "baslangic" | "bitis" | "hareketTarih">(null);
  const [pickerTemp, setPickerTemp] = useState<Date>(new Date());
  const [pickerTarget, setPickerTarget] = useState<null | "filterBas" | "filterBit" | "ekle" | "edit">(null);

  const [popupVisible, setPopupVisible] = useState(false);
  const [popupType, setPopupType] = useState<"success" | "error">("success");
  const [popupMessage, setPopupMessage] = useState("");
  const showPopup = (type: "success" | "error", message: string) => {
    setPopupType(type);
    setPopupMessage(message);
    setPopupVisible(true);
  };

  const [ekleVisible, setEkleVisible] = useState(false);
  const [eklePersonelId, setEklePersonelId] = useState<number | null>(null);
  const [ekleTip, setEkleTip] = useState<"GİRİŞ" | "ÇIKIŞ">("GİRİŞ");
  const [ekleTarih, setEkleTarih] = useState<Date>(new Date());
  const [ekleSaving, setEkleSaving] = useState(false);
  const [eklePersonelPickVisible, setEklePersonelPickVisible] = useState(false);
  const [ekleTipModal, setEkleTipModal] = useState(false);
  const [pendingEklePersonelPick, setPendingEklePersonelPick] = useState(false);
  const [pendingEkleTipPick, setPendingEkleTipPick] = useState(false);
  const [pendingEkleTarihPick, setPendingEkleTarihPick] = useState(false);

  const [editVisible, setEditVisible] = useState(false);
  const [editId, setEditId] = useState<number | null>(null);
  const [editTip, setEditTip] = useState<"GİRİŞ" | "ÇIKIŞ">("GİRİŞ");
  const [editTarih, setEditTarih] = useState<Date>(new Date());
  const [editTipModal, setEditTipModal] = useState(false);
  const [editSaving, setEditSaving] = useState(false);
  const [pendingEditTipPick, setPendingEditTipPick] = useState(false);
  const [pendingEditTarihPick, setPendingEditTarihPick] = useState(false);
  const [pasifConfirmVisible, setPasifConfirmVisible] = useState(false);
  const [pasifTargetId, setPasifTargetId] = useState<number | null>(null);
  const [pasifTargetLabel, setPasifTargetLabel] = useState<string>("");
  const [pasifSaving, setPasifSaving] = useState(false);

  const firmaSelectItems = useMemo(
    () =>
      (firmalar ?? [])
        .map((f: any) => ({
          key: String(pick<any>(f, "firmaId", "FirmaId") ?? ""),
          label: (pick<any>(f, "firmaAdi", "FirmaAdi") ?? "").toString(),
        }))
        .filter((x) => x.key && x.label),
    [firmalar]
  );

  const personelSelectItems = useMemo(() => personelList.map((p) => ({ key: String(p.id), label: p.ad })), [personelList]);

  const loadLookups = async (desiredFirmaId?: number | null, desiredKartTipi?: "puantaj" | "puantajsiz") => {
    const fId = desiredFirmaId ?? firmaId;
    const kTip = desiredKartTipi ?? kartTipi;
    const res = await kisiHareketService.lookups({ firmaId: fId, kartTipi: kTip });
    if (!res?.success) throw new Error(res?.message ?? "Lookups alınamadı.");
    const data = res.data ?? (res as any).Data ?? {};
    const firm = data.Firmalar ?? data.firmalar ?? [];
    const act = data.AktifFirma ?? data.aktifFirma ?? null;
    const ppl = data.PersonelList ?? data.personelList ?? [];
    setFirmalar(Array.isArray(firm) ? firm : []);
    setAktifFirma(act);
    setPersonelList(parsePersonelList(ppl));

    if (!isAdmin) {
      const afId = Number(pick<any>(act, "firmaId", "FirmaId"));
      if (Number.isFinite(afId) && afId > 0) setFirmaId(afId);
    } else {
      if (!fId) {
        const afId = Number(pick<any>(act, "firmaId", "FirmaId"));
        if (Number.isFinite(afId) && afId > 0) setFirmaId(afId);
      }
    }
  };

  useEffect(() => {
    if (!pendingEklePersonelPick) return;
    // modal stacking fix: close Ekle, then open picker
    const t = setTimeout(() => {
      setEklePersonelPickVisible(true);
      setPendingEklePersonelPick(false);
    }, 200);
    return () => clearTimeout(t);
  }, [pendingEklePersonelPick]);

  useEffect(() => {
    if (!pendingEkleTipPick) return;
    const t = setTimeout(() => {
      setEkleTipModal(true);
      setPendingEkleTipPick(false);
    }, 200);
    return () => clearTimeout(t);
  }, [pendingEkleTipPick]);

  useEffect(() => {
    if (!pendingEkleTarihPick) return;
    const t = setTimeout(() => {
      setPickerTarget("ekle");
      setPickerTemp(ekleTarih);
      setPickerOpen("hareketTarih");
      setPendingEkleTarihPick(false);
    }, 200);
    return () => clearTimeout(t);
  }, [pendingEkleTarihPick, ekleTarih]);

  useEffect(() => {
    if (!pendingEditTipPick) return;
    const t = setTimeout(() => {
      setEditTipModal(true);
      setPendingEditTipPick(false);
    }, 200);
    return () => clearTimeout(t);
  }, [pendingEditTipPick]);

  useEffect(() => {
    if (!pendingEditTarihPick) return;
    const t = setTimeout(() => {
      setPickerTarget("edit");
      setPickerTemp(editTarih);
      setPickerOpen("hareketTarih");
      setPendingEditTarihPick(false);
    }, 200);
    return () => clearTimeout(t);
  }, [pendingEditTarihPick, editTarih]);

  const loadList = async () => {
    const res = await kisiHareketService.list({
      firmaId,
      kartTipi,
      personelIds: personelIds.length ? personelIds.join(",") : undefined,
      baslangic: toLocalIsoNoZ(baslangic),
      bitis: toLocalIsoNoZ(bitis),
      sadeceAktif,
      sadecePasif,
      sadeceYemekhane,
      page,
      pageSize,
    });
    if (!res?.success) throw new Error(res?.message ?? "Liste alınamadı.");
    const payload = res.data ?? (res as any).Data ?? {};
    const pr = payload;
    const its = pr?.items ?? pr?.Items ?? [];
    const tc = pr?.totalCount ?? pr?.TotalCount ?? 0;
    const tp = pr?.totalPages ?? pr?.TotalPages ?? 1;
    setItems(Array.isArray(its) ? its : []);
    setTotalCount(Number(tc) || 0);
    setTotalPages(Number(tp) || 1);
  };

  const firstLoadRef = useRef(false);
  useEffect(() => {
    if (firstLoadRef.current) return;
    firstLoadRef.current = true;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        await loadLookups(null, kartTipi);
        await loadList();
      } catch (e: any) {
        setError(e?.message ?? "Beklenmeyen hata");
      } finally {
        setLoading(false);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (!firstLoadRef.current) return;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        await loadLookups(firmaId, kartTipi);
        await loadList();
      } catch (e: any) {
        setError(e?.message ?? "Beklenmeyen hata");
        setItems([]);
        setTotalCount(0);
        setTotalPages(1);
      } finally {
        setLoading(false);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [firmaId, kartTipi, personelIds.join(","), sadeceAktif, sadecePasif, sadeceYemekhane, baslangic.getTime(), bitis.getTime(), page, pageSize]);

  const firmaLabel = useMemo(() => {
    const fid = firmaId ?? Number(pick<any>(aktifFirma, "firmaId", "FirmaId"));
    const list = firmalar ?? [];
    const found = list.find((x: any) => Number(pick<any>(x, "firmaId", "FirmaId")) === fid);
    const ad = found ? (pick<any>(found, "firmaAdi", "FirmaAdi") ?? "").toString() : (pick<any>(aktifFirma, "firmaAdi", "FirmaAdi") ?? "").toString();
    return ad || (fid ? `#${fid}` : "--");
  }, [firmaId, firmalar, aktifFirma]);

  const personelLabel = useMemo(() => {
    if (!personelIds.length) return "-- Personel Seçiniz --";
    if (personelIds.length === 1) {
      const p = personelList.find((x) => x.id === personelIds[0]);
      return p?.ad ?? `#${personelIds[0]}`;
    }
    return `${personelIds.length} personel seçildi`;
  }, [personelIds, personelList]);

  const openPicker = (kind: "baslangic" | "bitis" | "hareketTarih", initial: Date, target: "filterBas" | "filterBit" | "ekle" | "edit") => {
    setPickerTarget(target);
    setPickerTemp(initial);
    setPickerOpen(kind);
  };

  const handleEkle = async () => {
    if (ekleSaving) return;
    if (!eklePersonelId) {
      showPopup("error", "Lütfen personel seçin.");
      return;
    }
    try {
      setEkleSaving(true);
      const r = await kisiHareketService.ekle({ personelId: eklePersonelId, tip: ekleTip, tarih: toLocalIsoNoZ(ekleTarih) });
      if (!r?.success) throw new Error(r?.message ?? "Hareket eklenemedi.");
      setEkleVisible(false);
      showPopup("success", r?.message ?? "Hareket eklendi.");
      setTimeout(() => loadList().catch(() => {}), 250);
    } catch (e: any) {
      showPopup("error", e?.message ?? "Hata");
    } finally {
      setEkleSaving(false);
    }
  };

  const closeEkleFlow = () => {
    setEkleSaving(false);
    setPendingEklePersonelPick(false);
    setPendingEkleTipPick(false);
    setPendingEkleTarihPick(false);
    setEklePersonelPickVisible(false);
    setEkleTipModal(false);
    if (pickerTarget === "ekle") {
      setPickerOpen(null);
      setPickerTarget(null);
    }
    setEkleVisible(false);
  };

  const openEdit = (row: KisiHareketRow) => {
    const id = Number(pick<any>(row, "id", "Id"));
    if (!Number.isFinite(id) || id <= 0) return;
    setEditId(id);
    const tip = tipLabel(pick<any>(row, "tip", "Tip"));
    setEditTip(tip === "Çıkış" ? "ÇIKIŞ" : "GİRİŞ");
    const t = pick<any>(row, "tarih", "Tarih");
    const d = new Date(t ?? new Date());
    setEditTarih(Number.isNaN(d.getTime()) ? new Date() : d);
    setEditVisible(true);
  };

  const handleUpdate = async () => {
    if (!editId || editSaving) return;
    try {
      setEditSaving(true);
      const r = await kisiHareketService.guncelle(editId, { tip: editTip, tarih: toLocalIsoNoZ(editTarih) });
      if (!r?.success) throw new Error(r?.message ?? "Güncellenemedi.");
      setEditVisible(false);
      showPopup("success", r?.message ?? "Güncellendi.");
      setTimeout(() => loadList().catch(() => {}), 250);
    } catch (e: any) {
      showPopup("error", e?.message ?? "Hata");
    } finally {
      setEditSaving(false);
    }
  };

  const closeEditFlow = () => {
    setEditSaving(false);
    setPendingEditTipPick(false);
    setPendingEditTarihPick(false);
    setEditTipModal(false);
    if (pickerTarget === "edit") {
      setPickerOpen(null);
      setPickerTarget(null);
    }
    setEditVisible(false);
  };

  const handlePasif = async (row: KisiHareketRow) => {
    const id = Number(pick<any>(row, "id", "Id"));
    if (!Number.isFinite(id) || id <= 0) return;

    const ad = (pick<any>(row, "adSoyad", "AdSoyad") ?? "-").toString();
    const sicil = (pick<any>(row, "sicilNo", "SicilNo") ?? "-").toString();
    setPasifTargetId(id);
    setPasifTargetLabel(`${ad} • ${sicil}`);
    setPasifConfirmVisible(true);
  };

  const doPasifConfirm = async () => {
    if (!pasifTargetId || pasifSaving) return;
    try {
      setPasifSaving(true);
      const r = await kisiHareketService.pasifYap(pasifTargetId);
      if (!r?.success) throw new Error(r?.message ?? "İşlem başarısız.");
      setPasifConfirmVisible(false);
      showPopup("success", r?.message ?? "Pasif yapıldı.");
      setTimeout(() => loadList().catch(() => {}), 250);
    } catch (e: any) {
      showPopup("error", e?.message ?? "Hata");
    } finally {
      setPasifSaving(false);
    }
  };

  if (loading && !items.length && !error) {
    return (
      <View className="flex-1 bg-[#f8fafc]">
        {header}
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator size="large" color="#dc2626" />
          <Text className="mt-3 text-[#64748b] font-semibold">Yükleniyor...</Text>
        </View>
      </View>
    );
  }

  return (
    <View className="flex-1 bg-[#f8fafc]">
      <StatusPopup
        visible={popupVisible}
        type={popupType}
        message={popupMessage}
        onClose={() => setPopupVisible(false)}
        useModal={false}
        autoCloseMs={1500}
      />
      {header}

      <ScrollView className="flex-1 px-4" contentContainerStyle={{ paddingBottom: 24 }}>
        <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
          <View className="px-4 py-3 border-b border-[#f1f5f9]">
            <Text className="text-[#0f172a] font-extrabold">Filtreler</Text>
          </View>
          <View className="p-4">
            <TouchableOpacity
              disabled={!isAdmin}
              onPress={() => setFirmaModal(true)}
              className={`px-3 py-3 rounded-xl border ${isAdmin ? "bg-white border-[#e2e8f0]" : "bg-[#f1f5f9] border-[#e2e8f0]"}`}
            >
              <RowLabel label="Firma" value={firmaLabel} />
              {!isAdmin ? <Text className="text-[#94a3b8] font-semibold text-[12px]">Admin değil: aktif firma sabit</Text> : null}
            </TouchableOpacity>

            <View className="h-3" />

            <TouchableOpacity onPress={() => setKartTipiModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
              <RowLabel label="Kart Tipi" value={kartTipi === "puantajsiz" ? "Puantaj Yapılmayanlar" : "Puantaj Yapılanlar"} />
            </TouchableOpacity>

            <View className="h-3" />

            <TouchableOpacity
              onPress={() => openPicker("baslangic", baslangic, "filterBas")}
              className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
            >
              <RowLabel label="Başlangıç" value={fmtDateTimeTR(baslangic)} />
            </TouchableOpacity>

            <View className="h-3" />

            <TouchableOpacity
              onPress={() => openPicker("bitis", bitis, "filterBit")}
              className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
            >
              <RowLabel label="Bitiş" value={fmtDateTimeTR(bitis)} />
            </TouchableOpacity>

            <View className="h-3" />

            <TouchableOpacity onPress={() => setPersonelModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
              <RowLabel label="Personeller" value={personelLabel} />
              <Text className="text-[#94a3b8] font-semibold text-[12px] mt-1">Liste görmek için en az 1 personel seçin.</Text>
            </TouchableOpacity>

            <View className="h-3" />

            <View className="flex-row flex-wrap gap-2">
              <TouchableOpacity
                onPress={() => setSadeceAktif((v) => !v)}
                className={`px-3 py-2 rounded-xl border ${sadeceAktif ? "bg-[#dcfce7] border-[#16a34a]" : "bg-white border-[#e2e8f0]"}`}
              >
                <Text className={`font-extrabold ${sadeceAktif ? "text-[#16a34a]" : "text-[#334155]"}`}>Sadece Aktif</Text>
              </TouchableOpacity>
              <TouchableOpacity
                onPress={() => setSadecePasif((v) => !v)}
                className={`px-3 py-2 rounded-xl border ${sadecePasif ? "bg-[#e0f2fe] border-[#0284c7]" : "bg-white border-[#e2e8f0]"}`}
              >
                <Text className={`font-extrabold ${sadecePasif ? "text-[#0284c7]" : "text-[#334155]"}`}>Sadece Pasif</Text>
              </TouchableOpacity>
              <TouchableOpacity
                onPress={() => setSadeceYemekhane((v) => !v)}
                className={`px-3 py-2 rounded-xl border ${sadeceYemekhane ? "bg-[#fef9c3] border-[#ca8a04]" : "bg-white border-[#e2e8f0]"}`}
              >
                <Text className={`font-extrabold ${sadeceYemekhane ? "text-[#a16207]" : "text-[#334155]"}`}>Sadece Yemekhane</Text>
              </TouchableOpacity>
            </View>

            <View className="h-3" />

            <TouchableOpacity onPress={() => setPageSizeModal(true)} className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white">
              <RowLabel label="Sayfa başına" value={String(pageSize)} />
            </TouchableOpacity>

            <View className="h-3" />

            <TouchableOpacity
              onPress={() => {
                setPage(1);
                setTimeout(() => loadList().catch(() => {}), 50);
              }}
              className="bg-[#dc2626] rounded-xl py-3 items-center"
            >
              <Text className="text-white font-extrabold">Ara</Text>
            </TouchableOpacity>
          </View>
        </View>

        <View className="h-4" />

        <View className="bg-white rounded-2xl border border-[#e2e8f0] overflow-hidden">
          <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
            <View className="flex-1">
              <Text className="text-[#0f172a] font-extrabold">Hareketler</Text>
              <Text className="text-[#64748b] font-semibold text-[12px]">
                Toplam {totalCount} kayıt • Sayfa {page} / {totalPages}
              </Text>
            </View>
            {loading ? <ActivityIndicator size="small" color="#dc2626" /> : null}
          </View>

          {error ? (
            <View className="p-4">
              <Text className="text-[#b91c1c] font-extrabold">Hata</Text>
              <Text className="text-[#334155] font-semibold mt-1">{error}</Text>
            </View>
          ) : null}

          {!error && !items.length ? (
            <View className="p-4">
              <Text className="text-[#64748b] font-semibold text-center">Kayıt bulunamadı.</Text>
            </View>
          ) : null}

          {!error &&
            items.map((row, idx) => {
              const aktif = !!(pick<any>(row, "aktifMi", "AktifMi"));
              const tip = tipLabel(pick<any>(row, "tip", "Tip"));
              return (
                <View key={`${String(pick<any>(row, "id", "Id") ?? idx)}_${idx}`} className={`px-4 py-3 border-b border-[#f1f5f9] ${!aktif ? "bg-[#f1f5f9]" : "bg-white"}`}>
                  <Text className="text-[#0f172a] font-extrabold" numberOfLines={1}>
                    {(pick<any>(row, "adSoyad", "AdSoyad") ?? "-").toString()} • {(pick<any>(row, "sicilNo", "SicilNo") ?? "-").toString()}
                  </Text>
                  <Text className="text-[#64748b] font-semibold text-[12px]" numberOfLines={1}>
                    Turnike: {(pick<any>(row, "cihazAdi", "CihazAdi") ?? "-").toString()} • Tip: {tip} • Durum: {aktif ? "Aktif" : "Pasif"}
                  </Text>
                  <Text className="text-[#334155] font-semibold text-[12px] mt-1">
                    Tarih: {fmtDateTimeTR(pick<any>(row, "tarih", "Tarih"))} • Kayıt: {fmtDateTimeTR(pick<any>(row, "kayitZamani", "KayitZamani"))}
                  </Text>

                  {(canUpdate || canDelete) ? (
                    <View className="flex-row gap-2 mt-3">
                      {canUpdate ? (
                        <TouchableOpacity className="flex-1 bg-[#e0f2fe] rounded-xl py-2 items-center" onPress={() => openEdit(row)}>
                          <Text className="text-[#0284c7] font-extrabold">Güncelle</Text>
                        </TouchableOpacity>
                      ) : null}
                      {canDelete ? (
                        <TouchableOpacity className="flex-1 bg-[#fee2e2] rounded-xl py-2 items-center" onPress={() => handlePasif(row)}>
                          <Text className="text-[#dc2626] font-extrabold">Pasif Yap</Text>
                        </TouchableOpacity>
                      ) : null}
                    </View>
                  ) : null}
                </View>
              );
            })}

          <View className="p-4 flex-row items-center justify-between">
            <TouchableOpacity
              disabled={page <= 1}
              onPress={() => setPage((p) => Math.max(1, p - 1))}
              className={`px-4 py-3 rounded-xl ${page <= 1 ? "bg-[#f1f5f9]" : "bg-white border border-[#e2e8f0]"}`}
            >
              <Text className={`font-extrabold ${page <= 1 ? "text-[#94a3b8]" : "text-[#0f172a]"}`}>Önceki</Text>
            </TouchableOpacity>

            <Text className="text-[#64748b] font-extrabold">
              {page} / {totalPages}
            </Text>

            <TouchableOpacity
              disabled={page >= totalPages}
              onPress={() => setPage((p) => Math.min(totalPages, p + 1))}
              className={`px-4 py-3 rounded-xl ${page >= totalPages ? "bg-[#f1f5f9]" : "bg-white border border-[#e2e8f0]"}`}
            >
              <Text className={`font-extrabold ${page >= totalPages ? "text-[#94a3b8]" : "text-[#0f172a]"}`}>Sonraki</Text>
            </TouchableOpacity>
          </View>
        </View>
      </ScrollView>

      {firmaModal ? (
        <SelectModal
          visible={firmaModal}
          title="Firma Seç"
          onClose={() => setFirmaModal(false)}
          onPick={(key) => {
            const id = Number(key);
            if (Number.isFinite(id) && id > 0) {
              setFirmaId(id);
              setPersonelIds([]);
              setPage(1);
            }
          }}
          items={firmaSelectItems}
        />
      ) : null}

      {kartTipiModal ? (
        <SelectModal
          visible={kartTipiModal}
          title="Kart Tipi"
          onClose={() => setKartTipiModal(false)}
          onPick={(key) => {
            const k = key === "puantajsiz" ? "puantajsiz" : "puantaj";
            setKartTipi(k);
            setPersonelIds([]);
            setPage(1);
          }}
          items={[
            { key: "puantaj", label: "Puantaj Yapılanlar" },
            { key: "puantajsiz", label: "Puantaj Yapılmayanlar" },
          ]}
        />
      ) : null}

      {pageSizeModal ? (
        <SelectModal
          visible={pageSizeModal}
          title="Sayfa Başına"
          onClose={() => setPageSizeModal(false)}
          onPick={(key) => {
            const n = Number(key);
            if ([20, 50, 100, 200].includes(n)) {
              setPageSize(n);
              setPage(1);
            }
          }}
          items={[20, 50, 100, 200].map((n) => ({ key: String(n), label: String(n) }))}
        />
      ) : null}

      {personelModal ? (
        <MultiPersonelModal
          visible={personelModal}
          title="Personeller"
          items={personelList}
          selectedIds={personelIds}
          onChange={(next) => {
            setPersonelIds(next);
            setPage(1);
          }}
          onClose={() => setPersonelModal(false)}
        />
      ) : null}

      {/* DateTime picker modal */}
      {pickerOpen !== null ? (
        <Modal visible={pickerOpen !== null} transparent animationType="fade" onRequestClose={() => setPickerOpen(null)}>
        <TouchableOpacity
          className="flex-1 bg-black/50 justify-center px-6"
          activeOpacity={1}
          onPress={() => {
            const target = pickerTarget;
            setPickerOpen(null);
            setPickerTarget(null);
            if (target === "ekle") setTimeout(() => setEkleVisible(true), 150);
            if (target === "edit") setTimeout(() => setEditVisible(true), 150);
          }}
        >
          <TouchableOpacity activeOpacity={1} onPress={() => {}}>
            <View className="bg-white rounded-2xl overflow-hidden">
              <View className="px-4 py-3 border-b border-[#f1f5f9]">
                <Text className="text-[#0f172a] font-extrabold text-[16px]">Tarih Seç</Text>
              </View>
              <View style={{ padding: 12, backgroundColor: "white" }}>
                <View style={{ height: Platform.OS === "ios" ? 216 : 216, justifyContent: "center" }}>
                  <DateTimePicker
                    value={pickerTemp}
                    onChange={(_, selected) => {
                      if (selected) setPickerTemp(selected);
                    }}
                    mode="datetime"
                    display={Platform.OS === "ios" ? "spinner" : "default"}
                    locale="tr-TR"
                    themeVariant="light"
                  />
                </View>
              </View>
              <View className="p-3 flex-row gap-2">
                <TouchableOpacity
                  className="flex-1 bg-[#f1f5f9] rounded-xl py-3 items-center"
                  onPress={() => {
                    const target = pickerTarget;
                    setPickerOpen(null);
                    setPickerTarget(null);
                    if (target === "ekle") setTimeout(() => setEkleVisible(true), 150);
                    if (target === "edit") setTimeout(() => setEditVisible(true), 150);
                  }}
                >
                  <Text className="text-[#334155] font-extrabold">Vazgeç</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  className="flex-1 bg-[#dc2626] rounded-xl py-3 items-center"
                  onPress={() => {
                    const v = pickerTemp;
                    const target = pickerTarget;
                    setPickerOpen(null);
                    setPickerTarget(null);
                    if (target === "filterBas") setBaslangic(v);
                    else if (target === "filterBit") setBitis(v);
                    else if (target === "ekle") {
                      setEkleTarih(v);
                      setTimeout(() => setEkleVisible(true), 150);
                    } else if (target === "edit") {
                      setEditTarih(v);
                      setTimeout(() => setEditVisible(true), 150);
                    }
                  }}
                >
                  <Text className="text-white font-extrabold">Seç</Text>
                </TouchableOpacity>
              </View>
            </View>
          </TouchableOpacity>
        </TouchableOpacity>
        </Modal>
      ) : null}

      {/* Ekle modal */}
      {ekleVisible ? (
        <Modal visible={ekleVisible} transparent animationType="fade" onRequestClose={closeEkleFlow}>
        <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={closeEkleFlow}>
          <TouchableOpacity activeOpacity={1} onPress={() => {}}>
            <View className="bg-white rounded-2xl overflow-hidden">
              <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                <Text className="text-[#0f172a] font-extrabold text-[16px]">Yeni Hareket Ekle</Text>
                <TouchableOpacity onPress={closeEkleFlow}>
                  <MaterialCommunityIcons name="close" size={22} color="#64748b" />
                </TouchableOpacity>
              </View>
              <View className="p-4">
                <TouchableOpacity
                  onPress={() => {
                    setEkleVisible(false);
                    setPendingEklePersonelPick(true);
                  }}
                  className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
                >
                  <RowLabel
                    label="Personel"
                    value={
                      eklePersonelId
                        ? (personelList.find((x) => x.id === eklePersonelId)?.ad ?? `#${eklePersonelId}`)
                        : "-- Seçiniz --"
                    }
                  />
                </TouchableOpacity>
                <View className="h-3" />
                <TouchableOpacity
                  onPress={() => {
                    setEkleVisible(false);
                    setPendingEkleTipPick(true);
                  }}
                  className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
                >
                  <RowLabel label="Tip" value={ekleTip === "ÇIKIŞ" ? "Çıkış" : "Giriş"} />
                </TouchableOpacity>
                <View className="h-3" />
                <TouchableOpacity
                  onPress={() => {
                    setEkleVisible(false);
                    setPendingEkleTarihPick(true);
                  }}
                  className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
                >
                  <RowLabel label="Tarih" value={fmtDateTimeTR(ekleTarih)} />
                </TouchableOpacity>

                <View className="h-4" />
                <TouchableOpacity
                  disabled={ekleSaving}
                  onPress={handleEkle}
                  className={`rounded-xl py-3 items-center ${ekleSaving ? "bg-[#86efac]" : "bg-[#16a34a]"}`}
                >
                  <Text className="text-white font-extrabold">{ekleSaving ? "Kaydediliyor..." : "Kaydet"}</Text>
                </TouchableOpacity>
                <TouchableOpacity disabled={ekleSaving} onPress={closeEkleFlow} className="mt-2 bg-[#f1f5f9] rounded-xl py-3 items-center">
                  <Text className={`font-extrabold ${ekleSaving ? "text-[#94a3b8]" : "text-[#334155]"}`}>Vazgeç</Text>
                </TouchableOpacity>
              </View>
            </View>
          </TouchableOpacity>
        </TouchableOpacity>
        </Modal>
      ) : null}

      {eklePersonelPickVisible ? (
        <PersonelSingleModal
          visible={eklePersonelPickVisible}
          title="Personel Seç"
          onClose={() => {
            setEklePersonelPickVisible(false);
            setTimeout(() => setEkleVisible(true), 150);
          }}
          items={personelList}
          selectedId={eklePersonelId}
          onPick={(id) => {
            setEklePersonelId(id);
          }}
        />
      ) : null}

      {ekleTipModal ? (
        <SelectModal
          visible={ekleTipModal}
          title="Hareket Tipi"
          onClose={() => {
            setEkleTipModal(false);
            setTimeout(() => setEkleVisible(true), 150);
          }}
          onPick={(key) => {
            setEkleTip(key === "ÇIKIŞ" ? "ÇIKIŞ" : "GİRİŞ");
            setTimeout(() => setEkleVisible(true), 150);
          }}
          items={[
            { key: "GİRİŞ", label: "Giriş" },
            { key: "ÇIKIŞ", label: "Çıkış" },
          ]}
        />
      ) : null}

      {/* Edit modal */}
      {editVisible ? (
      <Modal visible={editVisible} transparent animationType="fade" onRequestClose={closeEditFlow}>
        <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={closeEditFlow}>
          <TouchableOpacity activeOpacity={1} onPress={() => {}}>
            <View className="bg-white rounded-2xl overflow-hidden">
              <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                <Text className="text-[#0f172a] font-extrabold text-[16px]">Hareket Güncelle</Text>
                <TouchableOpacity onPress={closeEditFlow}>
                  <MaterialCommunityIcons name="close" size={22} color="#64748b" />
                </TouchableOpacity>
              </View>
              <View className="p-4">
                <TouchableOpacity
                  onPress={() => {
                    setEditVisible(false);
                    setPendingEditTipPick(true);
                  }}
                  className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
                >
                  <RowLabel label="Tip" value={editTip === "ÇIKIŞ" ? "Çıkış" : "Giriş"} />
                </TouchableOpacity>
                <View className="h-3" />
                <TouchableOpacity
                  onPress={() => {
                    setEditVisible(false);
                    setPendingEditTarihPick(true);
                  }}
                  className="px-3 py-3 rounded-xl border border-[#e2e8f0] bg-white"
                >
                  <RowLabel label="Tarih" value={fmtDateTimeTR(editTarih)} />
                </TouchableOpacity>

                <View className="h-4" />
                <TouchableOpacity
                  disabled={editSaving}
                  onPress={handleUpdate}
                  className={`rounded-xl py-3 items-center ${editSaving ? "bg-[#93c5fd]" : "bg-[#0284c7]"}`}
                >
                  <Text className="text-white font-extrabold">{editSaving ? "Kaydediliyor..." : "Kaydet"}</Text>
                </TouchableOpacity>
                <TouchableOpacity disabled={editSaving} onPress={closeEditFlow} className="mt-2 bg-[#f1f5f9] rounded-xl py-3 items-center">
                  <Text className={`font-extrabold ${editSaving ? "text-[#94a3b8]" : "text-[#334155]"}`}>Vazgeç</Text>
                </TouchableOpacity>
              </View>
            </View>
          </TouchableOpacity>
        </TouchableOpacity>
      </Modal>
      ) : null}

      {editTipModal ? (
        <SelectModal
          visible={editTipModal}
          title="Hareket Tipi"
          onClose={() => {
            setEditTipModal(false);
            setTimeout(() => setEditVisible(true), 150);
          }}
          onPick={(key) => {
            setEditTip(key === "ÇIKIŞ" ? "ÇIKIŞ" : "GİRİŞ");
            setTimeout(() => setEditVisible(true), 150);
          }}
          items={[
            { key: "GİRİŞ", label: "Giriş" },
            { key: "ÇIKIŞ", label: "Çıkış" },
          ]}
        />
      ) : null}

      {/* Pasif yap confirm */}
      {pasifConfirmVisible ? (
        <Modal visible={pasifConfirmVisible} transparent animationType="fade" onRequestClose={() => setPasifConfirmVisible(false)}>
          <TouchableOpacity className="flex-1 bg-black/50 justify-center px-6" activeOpacity={1} onPress={() => setPasifConfirmVisible(false)}>
            <TouchableOpacity activeOpacity={1} onPress={() => {}}>
              <View className="bg-white rounded-2xl overflow-hidden">
                <View className="px-4 py-3 border-b border-[#f1f5f9] flex-row items-center justify-between">
                  <Text className="text-[#0f172a] font-extrabold text-[16px]">Onay</Text>
                  <TouchableOpacity onPress={() => setPasifConfirmVisible(false)}>
                    <MaterialCommunityIcons name="close" size={22} color="#64748b" />
                  </TouchableOpacity>
                </View>
                <View className="p-4">
                  <Text className="text-[#0f172a] font-extrabold">Bu hareket pasif yapılsın mı?</Text>
                  <Text className="text-[#64748b] font-semibold mt-1">{pasifTargetLabel}</Text>

                  <View className="h-4" />
                  <TouchableOpacity
                    disabled={pasifSaving}
                    onPress={doPasifConfirm}
                    className={`rounded-xl py-3 items-center ${pasifSaving ? "bg-[#fecaca]" : "bg-[#dc2626]"}`}
                  >
                    <Text className="text-white font-extrabold">{pasifSaving ? "İşleniyor..." : "Pasif Yap"}</Text>
                  </TouchableOpacity>
                  <TouchableOpacity disabled={pasifSaving} onPress={() => setPasifConfirmVisible(false)} className="mt-2 bg-[#f1f5f9] rounded-xl py-3 items-center">
                    <Text className={`font-extrabold ${pasifSaving ? "text-[#94a3b8]" : "text-[#334155]"}`}>İptal</Text>
                  </TouchableOpacity>
                </View>
              </View>
            </TouchableOpacity>
          </TouchableOpacity>
        </Modal>
      ) : null}
    </View>
  );
}

