import React, { useMemo, useState } from "react";
import { Modal, Text, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { CustomInput } from "./CustomInput";
import { CustomButton } from "./CustomButton";
import { authService } from "../services/api";

type Step = "start" | "confirm";

export function ForgotPasswordModal(props: {
  visible: boolean;
  initialUsername?: string;
  onClose: () => void;
  onDone?: (username: string) => void;
}) {
  const [step, setStep] = useState<Step>("start");
  const [loading, setLoading] = useState(false);
  const [username, setUsername] = useState(props.initialUsername ?? "");
  const [kod, setKod] = useState("");
  const [yeniSifre, setYeniSifre] = useState("");
  const [yeniSifreTekrar, setYeniSifreTekrar] = useState("");
  const [error, setError] = useState<string | null>(null);
  const title = useMemo(
    () => (step === "start" ? "Şifre Sıfırlama" : "Kodu Doğrula"),
    [step]
  );

  const close = () => {
    setError(null);
    setLoading(false);
    setStep("start");
    setKod("");
    setYeniSifre("");
    setYeniSifreTekrar("");
    props.onClose();
  };

  const start = async () => {
    const u = username.trim();
    if (!u) return setError("Kullanıcı adını girin.");
    setLoading(true);
    setError(null);
    try {
      const res = await authService.forgotPasswordStart(u);
      if (!res?.success) {
        setError(res?.message ?? "İşlem başarısız.");
        return;
      }
      setStep("confirm");
    } catch (e: any) {
      setError(e?.response?.data?.message ?? e?.message ?? "İşlem başarısız.");
    } finally {
      setLoading(false);
    }
  };

  const confirm = async () => {
    const u = username.trim();
    if (!u) return setError("Kullanıcı adını girin.");
    if (!kod.trim()) return setError("Doğrulama kodunu girin.");
    if (!yeniSifre) return setError("Yeni şifreyi girin.");
    if (yeniSifre !== yeniSifreTekrar) return setError("Şifreler uyuşmuyor.");
    setLoading(true);
    setError(null);
    try {
      const res = await authService.forgotPasswordConfirm(u, kod.trim(), yeniSifre, yeniSifreTekrar);
      if (!res?.success) {
        setError(res?.message ?? "Şifre güncellenemedi.");
        return;
      }
      props.onDone?.(u);
      close();
    } catch (e: any) {
      setError(e?.response?.data?.message ?? e?.message ?? "Şifre güncellenemedi.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal visible={props.visible} transparent animationType="fade" onRequestClose={close}>
      <View className="flex-1 items-center justify-center bg-black/60 px-6">
        <View className="w-full rounded-3xl bg-white p-6">
          <View className="flex-row items-center justify-between mb-4">
            <View className="flex-row items-center">
              <View className="w-10 h-10 rounded-full bg-[#fee2e2] items-center justify-center mr-3">
                <MaterialCommunityIcons name="lock-reset" size={20} color="#dc2626" />
              </View>
              <Text className="text-[16px] font-extrabold text-[#1e293b]">{title}</Text>
            </View>
            <TouchableOpacity onPress={close} className="p-2">
              <MaterialCommunityIcons name="close" size={20} color="#64748b" />
            </TouchableOpacity>
          </View>

          {error ? (
            <View className="mb-3 rounded-xl bg-[#fef2f2] px-4 py-3 border border-[#fee2e2]">
              <Text className="text-[#dc2626] font-semibold text-[12px]">{error}</Text>
            </View>
          ) : null}

          <CustomInput
            label="Kullanıcı Adı"
            placeholder="Ad Soyad,TC,Sicil No veya E‑Posta giriniz"
            value={username}
            onChangeText={setUsername}
            icon="account"
          />

          {step === "confirm" ? (
            <>
              <CustomInput
                label="Doğrulama Kodu"
                placeholder="E-postanıza gelen kod"
                value={kod}
                onChangeText={setKod}
                icon="shield-key"
              />
              <CustomInput
                label="Yeni Şifre"
                placeholder="Yeni şifrenizi girin"
                value={yeniSifre}
                onChangeText={setYeniSifre}
                secureTextEntry
                icon="lock"
              />
              <CustomInput
                label="Yeni Şifre (Tekrar)"
                placeholder="Yeni şifrenizi tekrar girin"
                value={yeniSifreTekrar}
                onChangeText={setYeniSifreTekrar}
                secureTextEntry
                icon="lock-check"
              />
            </>
          ) : null}

          <CustomButton
            title={step === "start" ? "KOD GÖNDER" : "ŞİFREYİ GÜNCELLE"}
            onPress={step === "start" ? start : confirm}
            loading={loading}
            className="mt-2"
          />
        </View>
      </View>
    </Modal>
  );
}

