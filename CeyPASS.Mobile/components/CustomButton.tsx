import React from 'react';
import { TouchableOpacity, Text, ActivityIndicator, View } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { MaterialCommunityIcons } from '@expo/vector-icons';

interface CustomButtonProps {
  onPress: () => void;
  title: string;
  loading?: boolean;
  icon?: keyof typeof MaterialCommunityIcons.glyphMap;
  className?: string;
}

export const CustomButton: React.FC<CustomButtonProps> = ({ 
  onPress, 
  title, 
  loading, 
  icon, 
  className 
}) => {
  return (
    <TouchableOpacity 
      onPress={onPress} 
      activeOpacity={0.8}
      className={`overflow-hidden shadow-lg w-full ${className}`}
      style={{ borderRadius: 12 }}
    >
      <LinearGradient

        colors={['#dc2626', '#b91c1c', '#991b1b']}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        className="px-6"
        style={{ minHeight: 56 }}
      >
        <View
          style={{ flex: 1, minHeight: 56, alignItems: "center", justifyContent: "center", flexDirection: "row" }}
        >
          {loading ? (
            <ActivityIndicator color="white" />
          ) : (
            <>
              {icon ? <MaterialCommunityIcons name={icon} size={18} color="white" style={{ marginRight: 8 }} /> : null}
              <Text
                className="text-white font-bold tracking-wide text-[18px] text-center"
                style={{ lineHeight: 21 }}
              >
                {title}
              </Text>
            </>
          )}
        </View>
      </LinearGradient>
    </TouchableOpacity>

  );
};
