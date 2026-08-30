import React, { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';

interface CustomInputProps {
  label: string;
  placeholder: string;
  value: string;
  onChangeText: (text: string) => void;
  secureTextEntry?: boolean;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
  className?: string;
  error?: string;
}

export const CustomInput: React.FC<CustomInputProps> = ({
  label,
  placeholder,
  value,
  onChangeText,
  secureTextEntry,
  icon,
  className,
  error,
}) => {
  const [isFocused, setIsFocused] = useState(false);
  const [isPasswordVisible, setIsPasswordVisible] = useState(false);
  const hasError = !!(error && error.trim());

  return (
    <View className={`mb-5 ${className}`}>
      <Text className="text-[13px] font-semibold text-[#1e293b] mb-2 ml-1">
        {label}
      </Text>
      <View 
        className={`relative flex-row items-center border-[2px] rounded-xl px-4 py-3.5 transition-all ${
          hasError
            ? 'bg-white border-[#dc2626]'
            : isFocused
              ? 'bg-white border-[#dc2626]'
              : 'bg-[#f8fafc] border-[#e2e8f0]'
        }`}
        style={isFocused || hasError ? { shadowColor: '#dc2626', shadowOpacity: 0.15, shadowRadius: 8, elevation: 4 } : {}}
      >
        <MaterialCommunityIcons 
          name={icon} 
          size={22} 
          color={isFocused || hasError ? '#dc2626' : '#64748b'} 
          style={{ marginRight: 12 }}
        />
        <TextInput
          className="flex-1 text-[#1e293b] text-[15px] font-medium p-0"
          placeholder={placeholder}
          placeholderTextColor="#64748b"
          value={value}
          onChangeText={onChangeText}
          numberOfLines={1}
          secureTextEntry={secureTextEntry && !isPasswordVisible}
          onFocus={() => setIsFocused(true)}
          onBlur={() => setIsFocused(false)}
        />
        {secureTextEntry && (
          <TouchableOpacity onPress={() => setIsPasswordVisible(!isPasswordVisible)} className="ml-2 p-1">
            <MaterialCommunityIcons 
              name={isPasswordVisible ? 'eye-off-outline' : 'eye-outline'} 
              size={22} 
              color={isFocused || hasError ? '#dc2626' : '#64748b'} 
            />
          </TouchableOpacity>
        )}
      </View>
      {hasError ? (
        <Text className="mt-1.5 ml-1 text-[12px] font-semibold text-[#dc2626]">{error}</Text>
      ) : null}
    </View>
  );
};
