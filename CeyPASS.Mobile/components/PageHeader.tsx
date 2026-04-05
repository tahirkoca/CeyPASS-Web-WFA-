import React from "react";
import { Text, TouchableOpacity, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { useSafeAreaInsets } from "react-native-safe-area-context";

export function PageHeader(props: {
  title: string;
  subtitle?: string;
  onOpenMenu?: () => void;
  rightIcon?: keyof typeof MaterialCommunityIcons.glyphMap;
  onRightPress?: () => void;
  rightA11yLabel?: string;
  rightIcon2?: keyof typeof MaterialCommunityIcons.glyphMap;
  onRightPress2?: () => void;
  rightA11yLabel2?: string;
  rightBadge?: number | null;
  rightBadge2?: number | null;
}) {
  const insets = useSafeAreaInsets();
  // iOS notch safe-area can feel like a blank strip; keep it safe but tighter.
  const topPad = Math.max(0, (insets.top ?? 0) - 12);
  return (
    <View style={{ paddingTop: topPad }} className="px-5 pb-2 bg-white border-b border-[#f1f5f9]">
      <View className="relative h-[44px] justify-center">
        {/* Left */}
        {props.onOpenMenu ? (
          <View className="absolute left-0 top-0 bottom-0 justify-center">
            <TouchableOpacity
              onPress={props.onOpenMenu}
              className="w-[44px] h-[44px] items-center justify-center bg-[#f1f5f9] rounded-xl"
              accessibilityLabel="Menü"
            >
              <MaterialCommunityIcons name="menu" size={22} color="#1e293b" />
            </TouchableOpacity>
          </View>
        ) : null}

        {/* Right */}
        <View className="absolute right-0 top-0 bottom-0 flex-row items-center justify-end">
          {props.rightIcon && props.onRightPress ? (
            <View className="relative">
              <TouchableOpacity
                onPress={props.onRightPress}
                className="w-[44px] h-[44px] items-center justify-center bg-[#f1f5f9] rounded-xl"
                accessibilityLabel={props.rightA11yLabel || "Aksiyon"}
              >
                <MaterialCommunityIcons name={props.rightIcon} size={22} color="#1e293b" />
              </TouchableOpacity>
              {props.rightBadge && props.rightBadge > 0 ? (
                <View className="absolute -top-1 -right-1 min-w-[18px] h-[18px] px-[5px] rounded-full bg-[#dc2626] items-center justify-center">
                  <Text className="text-white font-extrabold text-[10px]" numberOfLines={1}>
                    {props.rightBadge > 99 ? "99+" : String(props.rightBadge)}
                  </Text>
                </View>
              ) : null}
            </View>
          ) : null}
          {props.rightIcon2 && props.onRightPress2 ? (
            <View className={`relative ${props.rightIcon && props.onRightPress ? "ml-2" : ""}`}>
              <TouchableOpacity
                onPress={props.onRightPress2}
                className="w-[44px] h-[44px] items-center justify-center bg-[#f1f5f9] rounded-xl"
                accessibilityLabel={props.rightA11yLabel2 || "Aksiyon"}
              >
                <MaterialCommunityIcons name={props.rightIcon2} size={22} color="#1e293b" />
              </TouchableOpacity>
              {props.rightBadge2 && props.rightBadge2 > 0 ? (
                <View className="absolute -top-1 -right-1 min-w-[18px] h-[18px] px-[5px] rounded-full bg-[#dc2626] items-center justify-center">
                  <Text className="text-white font-extrabold text-[10px]" numberOfLines={1}>
                    {props.rightBadge2 > 99 ? "99+" : String(props.rightBadge2)}
                  </Text>
                </View>
              ) : null}
            </View>
          ) : null}
        </View>

        {/* Center (never blocks buttons) */}
        <View pointerEvents="none" className="items-center px-[56px]">
          <Text className="text-[#1e293b] font-extrabold text-[16px]" numberOfLines={1} ellipsizeMode="tail">
            {props.title}
          </Text>
          {props.subtitle ? (
            <Text className="text-[#64748b] font-semibold text-[12px]" numberOfLines={1} ellipsizeMode="tail">
              {props.subtitle}
            </Text>
          ) : null}
        </View>
      </View>
    </View>
  );
}

