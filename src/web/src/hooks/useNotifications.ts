import { useCallback, useEffect, useState } from 'react';
import { get, put, post } from '@/lib/api-client';
import type { Notification, NotificationPagedResult, UnreadCount } from '@/types/api';

export function useNotifications(options?: {
  page?: number;
  pageSize?: number;
  unreadOnly?: boolean;
  type?: string;
}) {
  const { page = 1, pageSize = 20, unreadOnly, type } = options ?? {};
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchNotifications = useCallback(
    (signal?: AbortSignal) => {
      const params = new URLSearchParams();
      params.set('page', page.toString());
      params.set('pageSize', pageSize.toString());
      if (unreadOnly) params.set('unreadOnly', 'true');
      if (type) params.set('type', type);

      setIsLoading(true);
      setError(null);
      return get<NotificationPagedResult>(`/api/notifications?${params}`, { signal })
        .then((data) => {
          setNotifications(data.items);
          setTotalCount(data.totalCount);
        })
        .catch((err) => {
          if (err instanceof DOMException && err.name === 'AbortError') return;
          setError(err instanceof Error ? err : new Error('Failed to fetch notifications'));
        })
        .finally(() => setIsLoading(false));
    },
    [page, pageSize, unreadOnly, type],
  );

  useEffect(() => {
    const controller = new AbortController();
    fetchNotifications(controller.signal);
    return () => controller.abort();
  }, [fetchNotifications]);

  const refetch = useCallback(() => {
    fetchNotifications();
  }, [fetchNotifications]);

  return { notifications, totalCount, isLoading, error, refetch };
}

export function useUnreadCount() {
  const [count, setCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);

  const fetchCount = useCallback((signal?: AbortSignal) => {
    setIsLoading(true);
    return get<UnreadCount>('/api/notifications/unread-count', { signal })
      .then((data) => setCount(data.count))
      .catch((err) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        console.warn('Failed to fetch unread count:', err);
      })
      .finally(() => setIsLoading(false));
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    fetchCount(controller.signal);

    // Auto-refresh every 30 seconds
    const interval = setInterval(() => fetchCount(), 30000);

    return () => {
      controller.abort();
      clearInterval(interval);
    };
  }, [fetchCount]);

  const refetch = useCallback(() => {
    fetchCount();
  }, [fetchCount]);

  return { count, isLoading, refetch };
}

export async function markNotificationRead(id: string): Promise<Notification> {
  return put<Notification>(`/api/notifications/${id}/read`, {});
}

export async function markAllNotificationsRead(): Promise<void> {
  return post<void>('/api/notifications/mark-all-read');
}
