/** T.C. kimlik: 11 rakam kontrolü (basit uzunluk). */
export function isValid(tc?: string | null): boolean {
  if (!tc || !tc.trim()) return false;
  const t = tc.trim();
  return t.length === 11 && /^\d+$/.test(t);
}

/** API’den maskeli TC gösterimi (* içerir). */
export function looksMasked(text?: string | null): boolean {
  return !!text && text.indexOf("*") >= 0;
}

/** İlk hane + yıldız maskesi (liste gösterimi). */
export function mask(tc?: string | null): string {
  if (!tc || !tc.trim()) return "";
  const t = tc.trim();
  if (t.length <= 1) return t;
  return t.charAt(0) + "*".repeat(t.length - 1);
}

/** Kayıt öncesi zorunlu geçerli TC; hata fırlatır. */
export function requireValid(tc?: string | null): string {
  const t = (tc ?? "").trim();
  if (!t) throw new Error("T.C. Kimlik No giriniz.");
  if (looksMasked(t) || !isValid(t)) throw new Error("T.C. Kimlik No 11 haneli olmalıdır.");
  return t;
}

/** Maskeli alanda tamTc, aksi halde gösterilen metin kayda gider. */
export function resolveForSave(displayText?: string | null, tamTc?: string | null): string {
  const shown = (displayText ?? "").trim();
  if (looksMasked(shown)) return requireValid(tamTc);
  return requireValid(shown);
}

/** Boş TC’ye izin veren kayıt çözümlemesi (misafir kart). */
export function resolveTcOptionalForSave(
  displayText?: string | null,
  tamTc?: string | null
): string | null {
  const shown = (displayText ?? "").trim();
  if (!shown) return null;
  if (looksMasked(shown)) {
    const t = (tamTc ?? "").trim();
    if (!t || looksMasked(t) || !isValid(t))
      throw new Error("T.C. Kimlik No 11 haneli olmalıdır.");
    return t;
  }
  if (!isValid(shown)) throw new Error("T.C. Kimlik No 11 haneli olmalıdır.");
  return shown;
}

/** Geçmiş ziyaretçi satır etiketi: gosterim veya ad+plaka. */
export function gosterim(item: { adSoyad?: string | null; plaka?: string | null; gosterim?: string | null }): string {
  if (item.gosterim) return item.gosterim;
  const ad = (item.adSoyad ?? "").trim();
  const plaka = (item.plaka ?? "").trim();
  if (!plaka) return ad;
  if (!ad) return plaka;
  return `${ad} (${plaka})`;
}
