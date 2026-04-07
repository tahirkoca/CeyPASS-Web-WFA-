import React, { useState, useEffect, useRef } from "react";
import { View, Text, TouchableOpacity, ActivityIndicator, Alert, Linking, Platform } from "react-native";
import { CameraView, useCameraPermissions } from "expo-camera";
import * as Location from "expo-location";
import * as Device from "expo-device";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { mobileQrApi } from "../../services/mobileQrApi";
import { LinearGradient } from "expo-linear-gradient";

interface QrGirisScreenProps {
  onBack: () => void;
}

export function QrGirisScreen({ onBack }: QrGirisScreenProps) {
  const [cameraPermission, requestCameraPermission] = useCameraPermissions();
  const [locationStatus, setLocationStatus] = useState<Location.PermissionStatus | null>(null);
  const [isProcessing, setIsProcessing] = useState(false);
  const [scanned, setScanned] = useState(false);
  const [statusMessage, setStatusMessage] = useState("");
  const isMounted = useRef(true);
  const isScanning = useRef(false);

  useEffect(() => {
    isMounted.current = true;
    checkPermissions();
    return () => { isMounted.current = false; };
  }, []);

  const checkPermissions = async () => {
    // 1. Kamera Kontrolü
    if (!cameraPermission?.granted) {
      const res = await requestCameraPermission();
      if (!res.granted) {
        Alert.alert("Kamera İzni", "QR kod okutabilmek için kamera izni vermeniz gerekmektedir.");
      }
    }

    // 2. Konum İzni Kontrolü
    const { status } = await Location.getForegroundPermissionsAsync();
    if (status !== 'granted') {
      const req = await Location.requestForegroundPermissionsAsync();
      setLocationStatus(req.status);
    } else {
      setLocationStatus(status);
    }

    // 3. GPS Servisi Açık mı?
    const enabled = await Location.hasServicesEnabledAsync();
    if (!enabled) {
      Alert.alert(
        "GPS Kapalı",
        "Cihazınızın konumu kapalı görünüyor. Güvenli giriş için lütfen telefon ayarlarından GPS/Konum servisini açın.",
        [{ text: "Ayarlara Git", onPress: () => Platform.OS === 'ios' ? Linking.openURL('App-Prefs:Privacy&path=LOCATION') : Linking.openSettings() }, { text: "İptal", style: "cancel" }]
      );
    }
  };

  const handleBarCodeScanned = async ({ data }: { data: string }) => {
    if (isScanning.current || scanned || isProcessing) return;
    isScanning.current = true;
    
    // ANLIK KİLİT: State asenkron olduğu için hızlı karelerde geç kalabilir.
    setScanned(true);
    setIsProcessing(true);
    
    // GPS Servis Kontrolü (Son Dakika)
    const gpsEnabled = await Location.hasServicesEnabledAsync();
    if (!gpsEnabled) {
      Alert.alert("Hata", "Lütfen cihazınızın konum (GPS) servisini açın.");
      setScanned(false);
      setIsProcessing(false);
      return;
    }

    setStatusMessage("Güvenlik doğrulanıyor...");

    try {
      if (!Device.isDevice) {
        throw new Error("Güvenlik ihlali: Bu işlem emülatör üzerinden yapılamaz.");
      }

      const { status } = await Location.getForegroundPermissionsAsync();
      if (status !== 'granted') {
        throw new Error("Konum izni verilmediği için işlem yapılamıyor.");
      }

      let payload;
      try {
        payload = JSON.parse(data);
      } catch (e) {
        throw new Error("Geçersiz QR kod.");
      }

      if (!payload.CihazId) {
        throw new Error("Geçersiz QR içeriği.");
      }

      // Konum Al
      const location = await Location.getCurrentPositionAsync({ 
        accuracy: Location.Accuracy.Balanced, // Daha hızlı olması için Balanced
      });
      
      const isMocked = location.mocked === true;

      setStatusMessage("Kapı için onay bekleniyor...");
      const result = await mobileQrApi.okut({
        cihazId: Number(payload.CihazId),
        enlem: location.coords.latitude,
        boylam: location.coords.longitude,
        isMocked: isMocked
      });

      if (result && result.success) {
        Alert.alert("Giriş Başarılı", result.message || "Kapı başarıyla açıldı.", [{ text: "Tamam", onPress: onBack }]);
      } else {
        throw new Error(result?.message || "Bir hata oluştu.");
      }
    } catch (err: any) {
      Alert.alert(
        "Hata", 
        err?.response?.data?.message || err.message,
        [
          { 
            text: "Tekrar Dene", 
            onPress: () => {
              // State'leri sıfırla
              setIsProcessing(false);
              setStatusMessage("");
              isScanning.current = false;
              // Küçük bir delay ile scanned'ı false yapalım ki Camera View değişikliği hemen fark etsin
              setTimeout(() => setScanned(false), 100);
            } 
          }, 
          { text: "Kapat", onPress: onBack }
        ]
      );
    } finally {
      if (isMounted.current) {
        setIsProcessing(false);
        // Eğer başarılıysa kilidi açık bırakıyoruz (onBack ile dönüldüğünde sıfırlanır)
      }
    }
  };

  if (!cameraPermission) return <View className="flex-1 bg-black" />;

  if (!cameraPermission.granted) {
    return (
      <View className="flex-1 bg-gray-100 justify-center items-center p-6">
        <MaterialCommunityIcons name="camera-off" size={64} color="#666" />
        <Text className="text-gray-800 text-lg font-bold mt-4">Kamera İzni Gerekli</Text>
        <TouchableOpacity className="bg-blue-600 px-6 py-3 rounded-full mt-6" onPress={requestCameraPermission}>
          <Text className="text-white font-semibold">İzin Ver</Text>
        </TouchableOpacity>
        <TouchableOpacity className="mt-4" onPress={onBack}><Text className="text-blue-600">Geri Dön</Text></TouchableOpacity>
      </View>
    );
  }

  return (
    <View className="flex-1 bg-black">
      <CameraView
        style={{ flex: 1 }}
        facing="back"
        barcodeScannerSettings={{ barcodeTypes: ["qr"] }}
        onBarcodeScanned={scanned ? undefined : handleBarCodeScanned}
      >
        <LinearGradient colors={["rgba(0,0,0,0.7)", "transparent", "rgba(0,0,0,0.8)"]} style={{ flex: 1 }}>
          <View className="pt-12 px-4 flex-row items-center">
            <TouchableOpacity onPress={onBack} className="w-10 h-10 rounded-full bg-white/20 items-center justify-center">
              <MaterialCommunityIcons name="arrow-left" size={24} color="#fff" />
            </TouchableOpacity>
            <Text className="text-white ml-4 text-lg font-bold">Güvenli QR Okuyucu</Text>
          </View>

          <View className="items-center justify-center flex-1">
             <View className="w-64 h-64 border-2 border-green-500/80 rounded-2xl bg-white/10 items-center justify-center">
                {isProcessing ? (
                  <View className="items-center">
                    <ActivityIndicator size="large" color="#4ade80" />
                    <Text className="text-green-400 font-bold mt-3 text-center px-4">{statusMessage}</Text>
                  </View>
                ) : (
                  <View className="w-full items-center">
                     <View className="h-[2px] w-48 bg-green-500/50 rounded-full mb-4" />
                     <Text className="text-white/60 text-[10px] font-bold">QR KODU HİZALAYIN</Text>
                  </View>
                )}
             </View>
          </View>
          
          <View className="pb-12 px-8">
             <View className="bg-black/40 p-4 rounded-2xl flex-row items-center border border-white/10">
                <MaterialCommunityIcons name="shield-check" size={20} color="#4ade80" />
                <Text className="text-white/70 text-[11px] ml-3 flex-1 font-medium">Bu işlem için anlık GPS verisi ve kamera doğrulaması yapılmaktadır.</Text>
             </View>
          </View>
        </LinearGradient>
      </CameraView>
    </View>
  );
}
