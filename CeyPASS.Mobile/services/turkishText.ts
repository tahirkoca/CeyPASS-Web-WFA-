/** Türkçe (tr) locale ile küçük harf — İ/ı, I/ı, Ö/ö vb. */
export function toTrLower(s: unknown): string {
  return (s ?? "").toString().toLocaleLowerCase("tr");
}

/** Ad/sicil araması: boş needle her şeyi eşler. */
export function containsTrIgnoreCase(haystack: unknown, needle: unknown): boolean {
  const n = toTrLower(needle).trim();
  if (!n) return true;
  return toTrLower(haystack).includes(n);
}
