import React, { useEffect, useMemo, useState } from 'react';
import { Modal, TextInput, View, Text, ScrollView, TouchableOpacity, ActivityIndicator, StyleSheet, RefreshControl } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { dashboardService } from '../services/api';
import { PageHeader } from './PageHeader';
import { useHeaderQuickMenu } from './HeaderQuickMenu';
import { StatusPopup } from './StatusPopup';
import { notificationService } from '../services/notificationApi';
import { useNotificationsContext } from './NotificationsProvider';
import { useShellActions } from './ShellActions';

interface DashboardProps {
  user: any;
  onLogout: () => void;
  onOpenMenu?: () => void;
}

export const Dashboard: React.FC<DashboardProps> = ({ user, onLogout, onOpenMenu }) => {
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [data, setData] = useState<any>(null);

  const pageSize = 10;
  const [latePage, setLatePage] = useState(1);
  const [birthPage, setBirthPage] = useState(1);
  const [hirePage, setHirePage] = useState(1);
  const [resignPage, setResignPage] = useState(1);

  const [notifLoading, setNotifLoading] = useState(false);
  const [notifError, setNotifError] = useState<string | null>(null);
  const [notifPage, setNotifPage] = useState(1);
  const [notifTotalPages, setNotifTotalPages] = useState(1);
  const [notifItems, setNotifItems] = useState<any[]>([]);
  const quickMenu = useHeaderQuickMenu();
  const notif = useNotificationsContext();
  const shell = useShellActions();
  const [popupVisible, setPopupVisible] = useState(false);
  const [popupType, setPopupType] = useState<'success' | 'error'>('success');
  const [popupMessage, setPopupMessage] = useState('');

  const subtitle = useMemo(() => "Hoş geldiniz! İşte bugünün özeti.", []);

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
    } catch (e: any) {
      setNotifItems([]);
      setNotifTotalPages(1);
      setNotifError(e?.message || "Bildirimler alınamadı.");
    } finally {
      setNotifLoading(false);
    }
  };

  const fetchData = async (opts?: { keepNotifPage?: boolean }) => {
    try {
      const result = await dashboardService.getFullDashboard();
      if (result.success) {
        setData(result.data);
        // Reset list paginations on fresh load so user doesn't land on empty pages.
        setLatePage(1);
        setBirthPage(1);
        setHirePage(1);
        setResignPage(1);
      }
      const p = opts?.keepNotifPage ? notifPage : 1;
      if (!opts?.keepNotifPage) setNotifPage(1);
      await loadNotifications(p);
    } catch (error) {
      console.error("Dashboard veri hatası:", error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const onRefresh = () => {
    setRefreshing(true);
    fetchData({ keepNotifPage: true });
  };

  const StatCard = ({ title, value, icon, color, trend }: any) => (
    <View style={styles.statCardContainer}>
      <View style={[styles.statCard, { borderBottomColor: color }]}>
        <View style={[styles.iconCircle, { backgroundColor: color + '20' }]}>
          <MaterialCommunityIcons name={icon} size={28} color={color} />
        </View>
        <Text style={styles.statValue} numberOfLines={1}>
          {value || 0}
        </Text>
        <Text style={styles.statLabel} numberOfLines={2} ellipsizeMode="tail">
          {title}
        </Text>
        <View style={styles.statTrendBox}>
          <Text style={styles.statTrend} numberOfLines={2}>
            {trend || " "}
          </Text>
        </View>
      </View>
    </View>
  );

  const Paged = (arr: any[], page: number) => {
    const totalPages = Math.max(1, Math.ceil((arr?.length ?? 0) / pageSize));
    const safePage = Math.min(Math.max(1, page), totalPages);
    const start = (safePage - 1) * pageSize;
    const end = start + pageSize;
    return { totalPages, page: safePage, items: (arr ?? []).slice(start, end) };
  };

  const PageControls = (props: { page: number; totalPages: number; onPrev: () => void; onNext: () => void }) => (
    <View style={{ flexDirection: "row", justifyContent: "space-between", marginTop: 12 }}>
      <TouchableOpacity
        disabled={props.page <= 1}
        onPress={props.onPrev}
        style={[styles.pageBtn, { opacity: props.page <= 1 ? 0.5 : 1 }]}
      >
        <Text style={styles.pageBtnText}>Önceki</Text>
      </TouchableOpacity>
      <Text style={{ color: "#64748b", fontWeight: "700", alignSelf: "center" }}>
        Sayfa {props.page}/{props.totalPages}
      </Text>
      <TouchableOpacity
        disabled={props.page >= props.totalPages}
        onPress={props.onNext}
        style={[styles.pageBtn, { opacity: props.page >= props.totalPages ? 0.5 : 1 }]}
      >
        <Text style={styles.pageBtnText}>Sonraki</Text>
      </TouchableOpacity>
    </View>
  );

  const latePaged = useMemo(() => Paged(data?.lateList ?? [], latePage), [data?.lateList, latePage]);
  const birthPaged = useMemo(() => Paged(data?.birthdays ?? [], birthPage), [data?.birthdays, birthPage]);
  const hirePaged = useMemo(() => Paged(data?.newHires ?? [], hirePage), [data?.newHires, hirePage]);
  const resignPaged = useMemo(() => Paged(data?.resignations ?? [], resignPage), [data?.resignations, resignPage]);

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#dc2626" />
        <Text style={styles.loadingText}>Veriler Hazırlanıyor...</Text>
      </View>
    );
  }

  return (
    <ScrollView
      className="flex-1 bg-[#f8fafc]"
      showsVerticalScrollIndicator={false}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
    >
      <PageHeader
        title="Ana Sayfa"
        subtitle={subtitle}
        onOpenMenu={onOpenMenu}
        rightIcon="bell-outline"
        rightBadge={notif.unreadCount}
        onRightPress={() => quickMenu.open("notif")}
        rightIcon2={shell?.openTips ? "help-circle-outline" : undefined}
        onRightPress2={shell?.openTips}
      />
      {quickMenu.modal}
      <StatusPopup visible={popupVisible} type={popupType} message={popupMessage} onClose={() => setPopupVisible(false)} useModal={false} autoCloseMs={1500} />

      <View className="px-5 pb-10">
        <Text style={styles.sectionTitle}>Bugünün Özeti</Text>

        {/* Stats Grid - Web Icons & Colors */}
        <View style={styles.grid}>
          {/* Icon mapping aims to match Web Bootstrap icons closely */}
          <StatCard title="Giriş Yapanlar" value={data?.cards?.girisYapan} icon="account-check-outline" color="#3b82f6" trend="Bugün giriş/çıkış yapan" />
          <StatCard title="İçeridekiler" value={data?.cards?.iceridekiler} icon="office-building-outline" color="#22c55e" trend="Şu an işyerinde" />
          <StatCard title="Geç Kalanlar" value={data?.cards?.gecKalanlar} icon="clock-outline" color="#ef4444" trend="Mesaiye geç başlayan" />
          <StatCard title="Dışarıdakiler" value={data?.cards?.disaridakiler} icon="exit-run" color="#64748b" trend="Çıkış yapmış" />
          <StatCard title="Devamsızlar" value={data?.cards?.devamsizlar} icon="account-remove-outline" color="#a855f7" trend="Bugün gelmemiş" />
          <StatCard title="İzinliler" value={data?.cards?.izinli} icon="calendar-check-outline" color="#f97316" trend="Bugün izinli" />
          <StatCard title="İşe Başlayanlar" value={data?.cards?.iseBaslayan} icon="account-plus-outline" color="#06b6d4" trend="Bu ay başlayan" />
          <StatCard title="İşten Ayrılanlar" value={data?.cards?.istenAyrilan} icon="account-minus-outline" color="#ec4899" trend="Bu ay ayrılan" />
        </View>

        {/* Data Lists - Like Web Tables */}
        <View style={styles.listSection}>
          <View style={styles.listHeader}>
            <MaterialCommunityIcons name="clock-outline" size={20} color="#dc2626" />
            <Text style={styles.listTitle}>En Geç Gelen Personeller</Text>
            <View style={styles.badge}><Text style={styles.badgeText}>{data?.lateList?.length ?? 0}</Text></View>
          </View>
          {data?.lateList?.length ? (
            <>
            {latePaged.items.map((item: any, idx: number) => (
              <View key={idx} style={styles.listItem}>
                <Text style={styles.itemText}>{item.ad} {item.soyad}</Text>
                <View style={[styles.lateBadge, { backgroundColor: item.fazlaDakika >= 60 ? '#fee2e2' : '#fef3c7' }]}>
                  <Text style={[styles.lateBadgeText, { color: item.fazlaDakika >= 60 ? '#ef4444' : '#d97706' }]}>{item.fazlaDakika} dk</Text>
                </View>
              </View>
            ))}
            <PageControls
              page={latePaged.page}
              totalPages={latePaged.totalPages}
              onPrev={() => setLatePage((p) => Math.max(1, p - 1))}
              onNext={() => setLatePage((p) => Math.min(latePaged.totalPages, p + 1))}
            />
            </>
          ) : (
            <Text style={styles.emptyText}>Geç kalan personel yok</Text>
          )}
        </View>

        <View style={styles.listSection}>
          <View style={styles.listHeader}>
            <MaterialCommunityIcons name="gift-outline" size={20} color="#0ea5e9" />
            <Text style={styles.listTitle}>Bu Ay Doğum Günü Olanlar</Text>
            <View style={styles.badge}><Text style={styles.badgeText}>{data?.birthdays?.length ?? 0}</Text></View>
          </View>
          {data?.birthdays?.length ? (
            <>
              {birthPaged.items.map((item: any, idx: number) => (
                <View key={idx} style={styles.listItem}>
                  <Text style={styles.itemText}>{item.ad} {item.soyad}</Text>
                  <Text style={styles.itemSubText}>
                    {new Date(item.buYilDogumGunu).toLocaleDateString("tr-TR", { day: "2-digit", month: "long" })}
                  </Text>
                </View>
              ))}
              <PageControls
                page={birthPaged.page}
                totalPages={birthPaged.totalPages}
                onPrev={() => setBirthPage((p) => Math.max(1, p - 1))}
                onNext={() => setBirthPage((p) => Math.min(birthPaged.totalPages, p + 1))}
              />
            </>
          ) : (
            <Text style={styles.emptyText}>Bu ay doğum günü yok</Text>
          )}
        </View>

        <View style={styles.listSection}>
          <View style={styles.listHeader}>
            <MaterialCommunityIcons name="account-plus-outline" size={20} color="#16a34a" />
            <Text style={styles.listTitle}>Bu Ay İşe Başlayanlar</Text>
            <View style={styles.badge}><Text style={styles.badgeText}>{data?.newHires?.length ?? 0}</Text></View>
          </View>
          {data?.newHires?.length ? (
            <>
              {hirePaged.items.map((item: any, idx: number) => (
                <View key={idx} style={styles.listItem}>
                  <Text style={styles.itemText}>{item.ad} {item.soyad}</Text>
                  <Text style={styles.itemSubText}>{new Date(item.baslamaTarihi).toLocaleDateString("tr-TR")}</Text>
                </View>
              ))}
              <PageControls
                page={hirePaged.page}
                totalPages={hirePaged.totalPages}
                onPrev={() => setHirePage((p) => Math.max(1, p - 1))}
                onNext={() => setHirePage((p) => Math.min(hirePaged.totalPages, p + 1))}
              />
            </>
          ) : (
            <Text style={styles.emptyText}>Bu ay işe başlayan yok</Text>
          )}
        </View>

        <View style={styles.listSection}>
          <View style={styles.listHeader}>
            <MaterialCommunityIcons name="account-minus-outline" size={20} color="#dc2626" />
            <Text style={styles.listTitle}>Bu Ay İşten Ayrılanlar</Text>
            <View style={styles.badge}><Text style={styles.badgeText}>{data?.resignations?.length ?? 0}</Text></View>
          </View>
          {data?.resignations?.length ? (
            <>
              {resignPaged.items.map((item: any, idx: number) => (
                <View key={idx} style={styles.listItem}>
                  <Text style={styles.itemText}>{item.ad} {item.soyad}</Text>
                  <Text style={styles.itemSubText}>{new Date(item.ayrilmaTarihi).toLocaleDateString("tr-TR")}</Text>
                </View>
              ))}
              <PageControls
                page={resignPaged.page}
                totalPages={resignPaged.totalPages}
                onPrev={() => setResignPage((p) => Math.max(1, p - 1))}
                onNext={() => setResignPage((p) => Math.min(resignPaged.totalPages, p + 1))}
              />
            </>
          ) : (
            <Text style={styles.emptyText}>Bu ay ayrılan yok</Text>
          )}
        </View>

        <View style={styles.listSection}>
          <View style={styles.listHeader}>
            <MaterialCommunityIcons name="bell-outline" size={20} color="#0f172a" />
            <Text style={styles.listTitle}>Bildirim Geçmişi</Text>
          </View>
          {notifLoading ? (
            <Text style={styles.emptyText}>Bildirimler yükleniyor...</Text>
          ) : notifError ? (
            <Text style={[styles.emptyText, { color: "#b91c1c" }]}>{notifError}</Text>
          ) : notifItems.length ? (
            <>
              {notifItems.map((n: any, idx: number) => (
                <View key={`${n?.id ?? idx}`} style={[styles.listItem, { alignItems: "flex-start" }]}>
                  <View style={{ flex: 1 }}>
                    <Text style={styles.itemText} numberOfLines={1}>{(n?.baslik ?? "").toString()}</Text>
                    <Text style={styles.itemSubText} numberOfLines={2}>{(n?.mesaj ?? "").toString()}</Text>
                    <Text style={[styles.itemSubText, { marginTop: 4, color: "#94a3b8" }]} numberOfLines={1}>{(n?.tarih ?? "").toString()}</Text>
                  </View>
                </View>
              ))}
              <View style={{ flexDirection: "row", justifyContent: "space-between", marginTop: 12 }}>
                <TouchableOpacity
                  disabled={notifPage <= 1 || notifLoading}
                  onPress={async () => {
                    const p = Math.max(1, notifPage - 1);
                    setNotifPage(p);
                    await loadNotifications(p);
                  }}
                  style={[styles.pageBtn, { opacity: notifPage <= 1 || notifLoading ? 0.5 : 1 }]}
                >
                  <Text style={styles.pageBtnText}>Önceki</Text>
                </TouchableOpacity>
                <Text style={{ color: "#64748b", fontWeight: "700", alignSelf: "center" }}>
                  Sayfa {notifPage}/{notifTotalPages}
                </Text>
                <TouchableOpacity
                  disabled={notifPage >= notifTotalPages || notifLoading}
                  onPress={async () => {
                    const p = Math.min(notifTotalPages, notifPage + 1);
                    setNotifPage(p);
                    await loadNotifications(p);
                  }}
                  style={[styles.pageBtn, { opacity: notifPage >= notifTotalPages || notifLoading ? 0.5 : 1 }]}
                >
                  <Text style={styles.pageBtnText}>Sonraki</Text>
                </TouchableOpacity>
              </View>
            </>
          ) : (
            <Text style={styles.emptyText}>Henüz hiç bildiriminiz bulunmuyor.</Text>
          )}
        </View>
      </View>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: '#f8fafc' },
  loadingText: { marginTop: 15, color: '#64748b', fontWeight: 'bold' },
  sectionTitle: { fontSize: 18, fontWeight: '800', color: '#1e293b', marginTop: 25, marginBottom: 15 },
  grid: { flexDirection: 'row', flexWrap: 'wrap', justifyContent: 'space-between' },
  statCardContainer: { width: '48.5%', marginBottom: 16 },
  statCard: {
    height: 164,
    minHeight: 164,
    backgroundColor: "white",
    borderRadius: 24,
    borderWidth: 1,
    borderColor: "#f1f5f9",
    borderBottomWidth: 4,
    alignItems: "center",
    justifyContent: "center",
    paddingHorizontal: 16,
    paddingVertical: 16,
    shadowColor: "#0f172a",
    shadowOffset: { width: 0, height: 6 },
    shadowOpacity: 0.06,
    shadowRadius: 18,
    elevation: 3,
  },
  iconCircle: { width: 50, height: 50, borderRadius: 25, justifyContent: 'center', alignItems: 'center', marginBottom: 10 },
  statValue: { fontSize: 24, fontWeight: '900', color: '#1e293b' },
  statLabel: { fontSize: 12, fontWeight: '700', color: '#64748b', marginTop: 2 },
  statTrendBox: { minHeight: 28, alignItems: "center", justifyContent: "center", marginTop: 5 },
  statTrend: { fontSize: 10, color: '#94a3b8', fontWeight: '600', textAlign: "center" },
  listSection: { backgroundColor: 'white', borderRadius: 20, padding: 20, marginTop: 20, shadowColor: '#000', shadowOffset: { width: 0, height: 2 }, shadowOpacity: 0.05, shadowRadius: 10, elevation: 2 },
  listHeader: { flexDirection: 'row', alignItems: 'center', marginBottom: 15 },
  listTitle: { fontSize: 15, fontWeight: '800', color: '#1e293b', marginLeft: 10, flex: 1 },
  badge: { backgroundColor: '#f1f5f9', paddingHorizontal: 8, paddingVertical: 2, borderRadius: 8 },
  badgeText: { fontSize: 12, color: '#64748b', fontWeight: 'bold' },
  listItem: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', paddingVertical: 12, borderBottomWidth: 1, borderBottomColor: '#f1f5f9' },
  itemText: { fontSize: 14, fontWeight: '600', color: '#334155' },
  itemSubText: { fontSize: 12, color: '#64748b', fontWeight: '500' },
  lateBadge: { paddingHorizontal: 10, paddingVertical: 4, borderRadius: 8 },
  lateBadgeText: { fontSize: 12, fontWeight: 'bold' },
  emptyText: { fontSize: 13, color: '#64748b', fontWeight: '600', paddingVertical: 10 },
  pageBtn: { paddingHorizontal: 12, paddingVertical: 10, borderRadius: 12, backgroundColor: '#f1f5f9' },
  pageBtnText: { fontWeight: '800', color: '#334155' },
});
