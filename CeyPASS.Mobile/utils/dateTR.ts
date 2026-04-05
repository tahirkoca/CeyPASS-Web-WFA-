export function formatDateTR(v: any): string {
  if (!v) return "";

  // Date-only values must not shift by timezone.
  // Accept both "yyyy-MM-dd" and ISO like "yyyy-MM-ddTHH:mm:ss..." by using first 10 chars.
  if (typeof v === "string") {
    const s = v.trim();
    const datePart = s.length >= 10 ? s.slice(0, 10) : s;
    if (/^\d{4}-\d{2}-\d{2}$/.test(datePart)) {
      const [y, m, d] = datePart.split("-").map((x) => Number(x));
      if (y && m && d) return `${String(d).padStart(2, "0")}.${String(m).padStart(2, "0")}.${y}`;
    }
  }

  const d = new Date(v);
  if (Number.isNaN(d.getTime())) return (v ?? "").toString();
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const yyyy = d.getFullYear();
  return `${dd}.${mm}.${yyyy}`;
}

export function formatDateTimeTRLoose(v: any): string {
  if (!v) return "";
  const d = new Date(v);
  if (Number.isNaN(d.getTime())) return (v ?? "").toString();
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const yyyy = d.getFullYear();
  const hh = String(d.getHours()).padStart(2, "0");
  const mi = String(d.getMinutes()).padStart(2, "0");
  const ss = String(d.getSeconds()).padStart(2, "0");
  return `${dd}.${mm}.${yyyy} ${hh}:${mi}:${ss}`;
}

