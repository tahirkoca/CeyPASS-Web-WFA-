import React from 'react';
import { View, StyleSheet, Platform } from 'react-native';
import { BlurView } from 'expo-blur';

interface GlassCardProps {
  children: React.ReactNode;
  className?: string;
  style?: any;
}

export const GlassCard: React.FC<GlassCardProps> = ({ children, className, style }) => {
  const useBlur = Platform.OS !== "web";

  return (
    <View
      className={`rounded-[24px] overflow-hidden border border-white/20 ${className}`}
      style={[styles.shadow, style]}
    >
      {useBlur ? (
        <BlurView intensity={22} tint="light" style={styles.blurWrap}>
          <View style={styles.inner} className="p-8">
            {children}
          </View>
        </BlurView>
      ) : (
        <View style={[styles.inner, { backgroundColor: "rgba(255, 255, 255, 0.95)" }]} className="p-8">
          {children}
        </View>
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  shadow: {
    shadowColor: '#b91c1c',
    shadowOffset: { width: 0, height: 15 },
    shadowOpacity: 0.25,
    shadowRadius: 30,
    elevation: 20,
  },
  blurWrap: {
    flex: 0,
  },
  inner: {
    backgroundColor: "rgba(255, 255, 255, 0.85)",
  },
});

