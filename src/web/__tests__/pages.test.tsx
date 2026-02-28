import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderWithProviders, screen, waitFor } from '@/test-utils';

// Mock the api-client module so no real network requests are made.
// All hooks (useParticipants, useOrganisation, useMembers, etc.) call
// get/post/put/del from this module, so mocking it at the source
// prevents every API call.
vi.mock('@/lib/api-client', () => ({
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
  del: vi.fn(),
  setTokenGetter: vi.fn(),
  downloadBlob: vi.fn(),
}));

import { get, post } from '@/lib/api-client';

const mockGet = vi.mocked(get);
const mockPost = vi.mocked(post);

beforeEach(() => {
  // Default: all GET calls return empty paged results or empty arrays.
  // Different endpoints need different shapes.
  mockGet.mockImplementation((path: string) => {
    // Paged endpoints return { items: [], totalCount: 0 }
    if (
      path.startsWith('/api/participants') ||
      path.startsWith('/api/providers') ||
      path.startsWith('/api/invoices') ||
      path.startsWith('/api/claims') ||
      path.startsWith('/api/payments') ||
      path.startsWith('/api/notifications?')
    ) {
      return Promise.resolve({ items: [], totalCount: 0 } as never);
    }

    // Organisation endpoint returns an org object
    if (path.startsWith('/api/organisations/current/members')) {
      return Promise.resolve([] as never);
    }
    if (path.startsWith('/api/organisations/current')) {
      return Promise.resolve({
        id: 'org-1',
        name: 'Test Org',
        abn: '',
        contactEmail: '',
        contactPhone: '',
        address: '',
      } as never);
    }

    // Unread count
    if (path.startsWith('/api/notifications/unread-count')) {
      return Promise.resolve({ count: 0 } as never);
    }

    // Reports endpoints return empty arrays
    if (path.startsWith('/api/reports/')) {
      return Promise.resolve([] as never);
    }

    // Fallback: return empty object
    return Promise.resolve({} as never);
  });

  mockPost.mockResolvedValue({} as never);
});

afterEach(() => {
  vi.restoreAllMocks();
});

// ---------------------------------------------------------------
// Page smoke tests
// Each test verifies the component renders without throwing.
// ---------------------------------------------------------------

describe('Page smoke tests', () => {
  it('DashboardPage renders without crashing', async () => {
    const { DashboardPage } = await import('@/pages/DashboardPage');
    const { container } = renderWithProviders(<DashboardPage />);

    await waitFor(() => {
      expect(container.querySelector('.space-y-8')).toBeInTheDocument();
    });
  });

  it('OrgSettingsPage renders without crashing', async () => {
    const { OrgSettingsPage } = await import('@/pages/OrgSettingsPage');
    renderWithProviders(<OrgSettingsPage />);

    await waitFor(() => {
      expect(screen.getByText('Organisation Settings')).toBeInTheDocument();
    });
  });

  it('MembersPage renders without crashing', async () => {
    const { MembersPage } = await import('@/pages/MembersPage');
    renderWithProviders(<MembersPage />);

    await waitFor(() => {
      expect(screen.getByText('Team Members')).toBeInTheDocument();
    });
  });

  it('ParticipantsListPage renders without crashing', async () => {
    const { ParticipantsListPage } = await import(
      '@/pages/participants/ParticipantsListPage'
    );
    renderWithProviders(<ParticipantsListPage />);

    await waitFor(() => {
      expect(screen.getByText('Participants')).toBeInTheDocument();
    });
  });

  it('ProvidersListPage renders without crashing', async () => {
    const { ProvidersListPage } = await import(
      '@/pages/providers/ProvidersListPage'
    );
    renderWithProviders(<ProvidersListPage />);

    await waitFor(() => {
      expect(screen.getByText('Providers')).toBeInTheDocument();
    });
  });

  it('InvoicesListPage renders without crashing', async () => {
    const { InvoicesListPage } = await import(
      '@/pages/invoices/InvoicesListPage'
    );
    renderWithProviders(<InvoicesListPage />);

    await waitFor(() => {
      expect(screen.getByText('Invoices')).toBeInTheDocument();
    });
  });

  it('ClaimsListPage renders without crashing', async () => {
    const { ClaimsListPage } = await import('@/pages/claims/ClaimsListPage');
    renderWithProviders(<ClaimsListPage />);

    await waitFor(() => {
      expect(screen.getByText('Claims')).toBeInTheDocument();
    });
  });

  it('PaymentsListPage renders without crashing', async () => {
    const { PaymentsListPage } = await import(
      '@/pages/payments/PaymentsListPage'
    );
    renderWithProviders(<PaymentsListPage />);

    await waitFor(() => {
      expect(screen.getByText('Payments')).toBeInTheDocument();
    });
  });

  it('ReportsPage renders without crashing', async () => {
    const { ReportsPage } = await import('@/pages/reports/ReportsPage');
    renderWithProviders(<ReportsPage />);

    await waitFor(() => {
      expect(screen.getByText('Reports')).toBeInTheDocument();
    });
  });

  it('NotificationsPage renders without crashing', async () => {
    const { NotificationsPage } = await import(
      '@/pages/notifications/NotificationsPage'
    );
    renderWithProviders(<NotificationsPage />);

    await waitFor(() => {
      expect(screen.getByText('Notifications')).toBeInTheDocument();
    });
  });
});
