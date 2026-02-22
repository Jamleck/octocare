import { describe, it, expect } from 'vitest';
import { validateNdisNumber, validateRequired, validateEmail, validateDateNotFuture } from '../src/lib/validation';

describe('validateNdisNumber', () => {
  it('returns null for valid NDIS number', () => {
    expect(validateNdisNumber('431234567')).toBeNull();
  });

  it('returns error for empty string', () => {
    expect(validateNdisNumber('')).toBe('NDIS number is required');
  });

  it('returns error for wrong length', () => {
    expect(validateNdisNumber('43123456')).toBe('NDIS number must be exactly 9 digits');
  });

  it('returns error for non-numeric', () => {
    expect(validateNdisNumber('43abc4567')).toBe('NDIS number must be exactly 9 digits');
  });

  it('returns error for wrong prefix', () => {
    expect(validateNdisNumber('441234567')).toBe('NDIS number must start with 43');
  });
});

describe('validateRequired', () => {
  it('returns null for non-empty string', () => {
    expect(validateRequired('hello', 'Name')).toBeNull();
  });

  it('returns error for empty string', () => {
    expect(validateRequired('', 'Name')).toBe('Name is required');
  });

  it('returns error for whitespace-only string', () => {
    expect(validateRequired('   ', 'Name')).toBe('Name is required');
  });

  it('returns error for null', () => {
    expect(validateRequired(null, 'Name')).toBe('Name is required');
  });
});

describe('validateEmail', () => {
  it('returns null for valid email', () => {
    expect(validateEmail('test@example.com')).toBeNull();
  });

  it('returns null for empty string (optional)', () => {
    expect(validateEmail('')).toBeNull();
  });

  it('returns error for invalid email', () => {
    expect(validateEmail('notanemail')).toBe('Invalid email address');
  });
});

describe('validateDateNotFuture', () => {
  it('returns null for past date', () => {
    expect(validateDateNotFuture('2000-01-01')).toBeNull();
  });

  it('returns error for future date', () => {
    expect(validateDateNotFuture('2099-12-31')).toBe('Date cannot be in the future');
  });

  it('returns null for empty (optional)', () => {
    expect(validateDateNotFuture('')).toBeNull();
  });
});
