import React, { useEffect, useRef } from 'react';
import { View, Text, StyleSheet, Animated, Modal, TouchableOpacity } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';

interface StatusPopupProps {
  visible: boolean;
  type: 'success' | 'error';
  message: string;
  onClose: () => void;
  useModal?: boolean;
  autoCloseMs?: number;
  onUndo?: () => void | Promise<void>;
  undoLabel?: string;
  undoLoading?: boolean;
}

export const StatusPopup: React.FC<StatusPopupProps> = ({
  visible,
  type,
  message,
  onClose,
  useModal = true,
  autoCloseMs,
  onUndo,
  undoLabel = 'Geri al',
  undoLoading = false,
}) => {
  const fadeAnim = useRef(new Animated.Value(0)).current;
  const scaleAnim = useRef(new Animated.Value(0.8)).current;

  useEffect(() => {
    if (visible) {
      Animated.parallel([
        Animated.timing(fadeAnim, {
          toValue: 1,
          duration: 300,
          useNativeDriver: true,
        }),
        Animated.spring(scaleAnim, {
          toValue: 1,
          friction: 8,
          useNativeDriver: true,
        }),
      ]).start();
    } else {
      fadeAnim.setValue(0);
      scaleAnim.setValue(0.8);
    }
  }, [visible]);

  useEffect(() => {
    if (!visible) return;
    const ms = Number(autoCloseMs);
    if (!Number.isFinite(ms) || ms <= 0) return;
    if (onUndo) return;
    const t = setTimeout(() => onClose(), ms);
    return () => clearTimeout(t);
  }, [visible, autoCloseMs, onClose, onUndo]);

  const body = (
    <View style={[styles.overlayBase, useModal ? styles.overlayModal : styles.overlayInline]}>
      <Animated.View style={[styles.container, { opacity: fadeAnim, transform: [{ scale: scaleAnim }] }]}>
        <View style={[styles.iconBox, { backgroundColor: type === 'success' ? '#22c55e' : '#ef4444' }]}>
          <MaterialCommunityIcons name={type === 'success' ? 'check-bold' : 'close-thick'} size={48} color="white" />
        </View>

        <Text style={styles.title}>{type === 'success' ? 'Başarılı!' : 'Hata Oluştu'}</Text>

        <Text style={styles.message}>{message}</Text>

        {type === 'success' && onUndo ? (
          <View style={styles.undoRow}>
            <TouchableOpacity
              style={[styles.button, styles.undoButton]}
              onPress={onUndo}
              disabled={undoLoading}
              activeOpacity={0.7}
            >
              <Text style={styles.undoButtonText}>{undoLoading ? 'Geri alınıyor...' : undoLabel}</Text>
            </TouchableOpacity>
            <TouchableOpacity
              style={[styles.button, styles.dismissButton]}
              onPress={onClose}
              activeOpacity={0.7}
            >
              <Text style={styles.buttonText}>Kapat</Text>
            </TouchableOpacity>
          </View>
        ) : (
          <TouchableOpacity
            style={[styles.button, { backgroundColor: type === 'success' ? '#22c55e' : '#ef4444' }]}
            onPress={onClose}
            activeOpacity={0.7}
          >
            <Text style={styles.buttonText}>Tamam</Text>
          </TouchableOpacity>
        )}
      </Animated.View>
    </View>
  );

  if (!useModal) {
    if (!visible) return null;
    return body;
  }

  return (
    <Modal transparent visible={visible} animationType="none" onRequestClose={onClose}>
      {body}
    </Modal>
  );
};

const styles = StyleSheet.create({
  overlayBase: {
    backgroundColor: 'rgba(0,0,0,0.6)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  overlayModal: {
    flex: 1,
  },
  overlayInline: {
    position: 'absolute',
    top: 0,
    right: 0,
    bottom: 0,
    left: 0,
    zIndex: 9999,
    elevation: 9999,
  },
  container: {
    width: '80%',
    backgroundColor: 'white',
    borderRadius: 24,
    padding: 30,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 10 },
    shadowOpacity: 0.2,
    shadowRadius: 20,
    elevation: 15,
  },
  iconBox: {
    width: 80,
    height: 80,
    borderRadius: 40,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 20,
  },
  title: {
    fontSize: 22,
    fontWeight: '800',
    color: '#1e293b',
    marginBottom: 10,
  },
  message: {
    fontSize: 16,
    color: '#64748b',
    textAlign: 'center',
    lineHeight: 22,
    marginBottom: 25,
  },
  button: {
    width: '100%',
    paddingVertical: 14,
    borderRadius: 12,
    alignItems: 'center',
  },
  undoRow: {
    width: '100%',
    gap: 10,
  },
  undoButton: {
    backgroundColor: '#0f172a',
  },
  undoButtonText: {
    color: 'white',
    fontWeight: '700',
    fontSize: 16,
  },
  dismissButton: {
    backgroundColor: '#22c55e',
  },
  buttonText: {
    color: 'white',
    fontWeight: '700',
    fontSize: 16,
  },
});
