import { ApiError } from './api-error';

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

let getTokenFn: (() => Promise<string>) | null = null;

export function setTokenGetter(fn: () => Promise<string>) {
  getTokenFn = fn;
}

export interface RequestOptions {
  signal?: AbortSignal;
}

async function request<T>(method: string, path: string, body?: unknown, options?: RequestOptions): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  if (getTokenFn) {
    try {
      const token = await getTokenFn();
      headers['Authorization'] = `Bearer ${token}`;
    } catch (err) {
      console.warn('Token acquisition failed:', err);
    }
  }

  const response = await fetch(`${API_URL}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
    signal: options?.signal,
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new ApiError(response.status, response.statusText, problem);
  }

  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

export function get<T>(path: string, options?: RequestOptions): Promise<T> {
  return request<T>('GET', path, undefined, options);
}

export function post<T>(path: string, body?: unknown, options?: RequestOptions): Promise<T> {
  return request<T>('POST', path, body, options);
}

export function put<T>(path: string, body: unknown, options?: RequestOptions): Promise<T> {
  return request<T>('PUT', path, body, options);
}

export function del<T>(path: string, options?: RequestOptions): Promise<T> {
  return request<T>('DELETE', path, undefined, options);
}
