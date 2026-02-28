import { useCallback, useEffect, useState } from 'react';
import { get, put, post } from '@/lib/api-client';
import type {
  EmailTemplate,
  UpdateEmailTemplateRequest,
  EmailTemplatePreview,
} from '@/types/api';

export function useEmailTemplates() {
  const [templates, setTemplates] = useState<EmailTemplate[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchTemplates() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<EmailTemplate[]>('/api/email-templates', {
          signal: controller.signal,
        });
        setTemplates(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch email templates'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchTemplates();
    return () => controller.abort();
  }, []);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    get<EmailTemplate[]>('/api/email-templates')
      .then(setTemplates)
      .catch((err) =>
        setError(err instanceof Error ? err : new Error('Failed to fetch email templates')),
      )
      .finally(() => setIsLoading(false));
  }, []);

  return { templates, isLoading, error, refetch };
}

export async function updateEmailTemplate(
  id: string,
  request: UpdateEmailTemplateRequest,
): Promise<EmailTemplate> {
  return put<EmailTemplate>(`/api/email-templates/${id}`, request);
}

export async function previewEmailTemplate(
  id: string,
  variables: Record<string, string>,
): Promise<EmailTemplatePreview> {
  return post<EmailTemplatePreview>(`/api/email-templates/${id}/preview`, { variables });
}
