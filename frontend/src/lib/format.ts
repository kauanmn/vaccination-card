/**
 * Formata "yyyy-MM-dd" como "dd/MM/yyyy" sem passar por Date,
 * evitando deslocamento de fuso horário em datas puras.
 */
export function formatDate(isoDate: string): string {
  const [year, month, day] = isoDate.split("-");
  if (!year || !month || !day) return isoDate;
  return `${day}/${month}/${year}`;
}

/** Data local de hoje em "yyyy-MM-dd" (toISOString usaria UTC e poderia errar o dia). */
export function todayIso(): string {
  const now = new Date();
  const month = String(now.getMonth() + 1).padStart(2, "0");
  const day = String(now.getDate()).padStart(2, "0");
  return `${now.getFullYear()}-${month}-${day}`;
}
