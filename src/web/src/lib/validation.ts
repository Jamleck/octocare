export function validateNdisNumber(value: string): string | null {
  if (!value) return 'NDIS number is required';
  if (!/^\d{9}$/.test(value)) return 'NDIS number must be exactly 9 digits';
  if (!value.startsWith('43')) return 'NDIS number must start with 43';
  return null;
}

export function validateRequired(value: string | undefined | null, fieldName: string): string | null {
  if (!value?.trim()) return `${fieldName} is required`;
  return null;
}

export function validateEmail(value: string | undefined | null): string | null {
  if (!value) return null;
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) return 'Invalid email address';
  return null;
}

export function validateDateNotFuture(value: string | undefined | null): string | null {
  if (!value) return null;
  const date = new Date(value);
  if (isNaN(date.getTime())) return 'Invalid date';
  if (date > new Date()) return 'Date cannot be in the future';
  return null;
}
