import { describe, it, expect, vi, beforeEach } from 'vitest';

const mockFetch = vi.fn();
vi.stubGlobal('fetch', mockFetch);

beforeEach(() => {
  vi.resetModules();
  vi.resetAllMocks();
  mockFetch.mockResolvedValue({
    ok: true,
    status: 200,
    json: () => Promise.resolve({ id: '1' }),
  });
});

describe('api-client', () => {
  it('passes AbortSignal to fetch when provided', async () => {
    const { get } = await import('@/lib/api-client');
    const controller = new AbortController();

    await get('/api/test', { signal: controller.signal });

    expect(mockFetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({ signal: controller.signal }),
    );
  });

  it('returns undefined for 204 No Content', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      status: 204,
      json: () => Promise.reject(new Error('no body')),
    });
    const { get } = await import('@/lib/api-client');
    const result = await get('/api/test');
    expect(result).toBeUndefined();
  });

  it('throws ApiError with status for non-ok response', async () => {
    mockFetch.mockResolvedValue({
      ok: false,
      status: 404,
      statusText: 'Not Found',
      json: () => Promise.resolve({ detail: 'Not found' }),
    });
    const { get } = await import('@/lib/api-client');
    await expect(get('/api/test')).rejects.toThrow('Not found');
  });

  it('includes Authorization header when token getter is set', async () => {
    const { get, setTokenGetter } = await import('@/lib/api-client');
    setTokenGetter(async () => 'test-token');

    await get('/api/test');

    expect(mockFetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({
        headers: expect.objectContaining({
          Authorization: 'Bearer test-token',
        }),
      }),
    );
  });

  it('proceeds without Authorization header when token getter fails', async () => {
    const { get, setTokenGetter } = await import('@/lib/api-client');
    setTokenGetter(async () => { throw new Error('token expired'); });

    await get('/api/test');

    const [, init] = mockFetch.mock.calls[0];
    expect(init.headers).not.toHaveProperty('Authorization');
  });
});
