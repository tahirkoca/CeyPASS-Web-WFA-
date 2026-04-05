import React, { useEffect, useRef } from "react";
import { ActivityIndicator, Animated, Easing, Image, Text, TouchableOpacity, View } from "react-native";

export function WelcomeSplash(props: {
  status: "checking" | "offline" | "error" | "online";
  message?: string | null;
  onRetry?: () => void;
}) {
  const isBlocking = props.status !== "online";
  const isOnline = props.status === "online";

  // Logo animasyonu: ortada görünüp büyüyerek kaybolsun (login'e geçiş hissi).
  const logoFade = useRef(new Animated.Value(0)).current;
  const logoScale = useRef(new Animated.Value(0.85)).current;

  useEffect(() => {
    // reset each time status changes
    logoFade.setValue(0);
    logoScale.setValue(0.85);

    // intro: fade in + küçük bir settle
    Animated.parallel([
      Animated.timing(logoFade, {
        toValue: 1,
        duration: 220,
        easing: Easing.out(Easing.quad),
        useNativeDriver: true,
      }),
      Animated.timing(logoScale, {
        toValue: 1.0,
        duration: 420,
        easing: Easing.out(Easing.cubic),
        useNativeDriver: true,
      }),
    ]).start();

    if (!isOnline) return;

    // exit: büyü + fade out (logo ortada kalır, yukarı kaymaz)
    const t = setTimeout(() => {
      Animated.parallel([
        Animated.timing(logoScale, {
          toValue: 2.35,
          duration: 520,
          easing: Easing.inOut(Easing.cubic),
          useNativeDriver: true,
        }),
        Animated.timing(logoFade, {
          toValue: 0,
          duration: 420,
          easing: Easing.in(Easing.quad),
          useNativeDriver: true,
        }),
      ]).start();
    }, 1650);
    return () => clearTimeout(t);
  }, [isOnline, logoFade, logoScale]);

  // Netflix benzeri: siyah zemin + logo ortada.
  const LOGO_W = 280;
  const LOGO_H = 160;

  return (
    <View style={{ flex: 1, backgroundColor: "#000" }}>

      <View style={{ flex: 1, alignItems: "center", justifyContent: "center", paddingHorizontal: 24 }}>
        {/* Splash logo */}
        <Animated.View
          style={{
            opacity: logoFade,
            transform: [{ scale: logoScale }],
            width: LOGO_W,
            height: LOGO_H,
            alignItems: "center",
            justifyContent: "center",
          }}
        >
          <Image
            source={require("../assets/sirket_logo.png")}
            style={{
              width: "100%",
              height: "100%",
              resizeMode: "contain",
            }}
          />
        </Animated.View>

        {isBlocking ? (
          <View style={{ marginTop: 54, alignItems: "center" }}>
            {props.status === "checking" ? (
              <>
                <ActivityIndicator size="large" color="#ffffff" />
                <Text style={{ marginTop: 16, color: "#fff", fontWeight: "800", fontSize: 16, textAlign: "center" }}>
                  Bağlantı kontrol ediliyor...
                </Text>
                <Text style={{ marginTop: 8, color: "rgba(255,255,255,0.7)", fontWeight: "600", textAlign: "center" }}>
                  Lütfen bekleyin.
                </Text>
              </>
            ) : (
              <>
                <Text style={{ color: "#fff", fontWeight: "800", fontSize: 16, textAlign: "center" }}>
                  {props.status === "offline" ? "İnternet bağlantısı yok" : "Bağlantı kurulamadı"}
                </Text>
                <Text style={{ marginTop: 8, color: "rgba(255,255,255,0.7)", fontWeight: "600", textAlign: "center" }}>
                  {(props.message ?? "").toString().trim() || "İnternet bağlantınızı kontrol edin ve tekrar deneyin."}
                </Text>

                {props.onRetry ? (
                  <TouchableOpacity
                    onPress={props.onRetry}
                    style={{
                      marginTop: 20,
                      backgroundColor: "rgba(255,255,255,0.08)",
                      borderWidth: 1,
                      borderColor: "rgba(255,255,255,0.2)",
                      borderRadius: 16,
                      paddingHorizontal: 28,
                      paddingVertical: 10,
                    }}
                    activeOpacity={0.8}
                  >
                    <Text style={{ color: "#fff", fontWeight: "800", textAlign: "center" }}>Tekrar Dene</Text>
                  </TouchableOpacity>
                ) : null}
              </>
            )}
          </View>
        ) : null}
      </View>
    </View>
  );
}

