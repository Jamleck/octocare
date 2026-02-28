import { createBrowserRouter, Navigate } from 'react-router';
import { AuthGuard } from '@/components/AuthGuard';
import { AuthCallback } from '@/components/AuthCallback';
import { AppLayout } from '@/layouts/AppLayout';
import { DashboardPage } from '@/pages/DashboardPage';
import { OrgSettingsPage } from '@/pages/OrgSettingsPage';
import { MembersPage } from '@/pages/MembersPage';
import { ParticipantsListPage } from '@/pages/participants/ParticipantsListPage';
import { ParticipantCreatePage } from '@/pages/participants/ParticipantCreatePage';
import { ParticipantDetailPage } from '@/pages/participants/ParticipantDetailPage';
import { ParticipantEditPage } from '@/pages/participants/ParticipantEditPage';
import { ProvidersListPage } from '@/pages/providers/ProvidersListPage';
import { ProviderCreatePage } from '@/pages/providers/ProviderCreatePage';
import { ProviderDetailPage } from '@/pages/providers/ProviderDetailPage';
import { ProviderEditPage } from '@/pages/providers/ProviderEditPage';
import { PlanCreatePage } from '@/pages/plans/PlanCreatePage';
import { PlanDetailPage } from '@/pages/plans/PlanDetailPage';
import { PlanEditPage } from '@/pages/plans/PlanEditPage';
import { AgreementCreatePage } from '@/pages/agreements/AgreementCreatePage';
import { AgreementDetailPage } from '@/pages/agreements/AgreementDetailPage';

export const router = createBrowserRouter([
  {
    path: '/callback',
    element: <AuthCallback />,
  },
  {
    element: (
      <AuthGuard>
        <AppLayout />
      </AuthGuard>
    ),
    children: [
      { index: true, element: <Navigate to="/dashboard" replace /> },
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'settings', element: <OrgSettingsPage /> },
      { path: 'members', element: <MembersPage /> },
      { path: 'participants', element: <ParticipantsListPage /> },
      { path: 'participants/new', element: <ParticipantCreatePage /> },
      { path: 'participants/:id', element: <ParticipantDetailPage /> },
      { path: 'participants/:id/edit', element: <ParticipantEditPage /> },
      { path: 'participants/:participantId/plans/new', element: <PlanCreatePage /> },
      { path: 'participants/:participantId/agreements/new', element: <AgreementCreatePage /> },
      { path: 'plans/:id', element: <PlanDetailPage /> },
      { path: 'plans/:id/edit', element: <PlanEditPage /> },
      { path: 'agreements/:id', element: <AgreementDetailPage /> },
      { path: 'providers', element: <ProvidersListPage /> },
      { path: 'providers/new', element: <ProviderCreatePage /> },
      { path: 'providers/:id', element: <ProviderDetailPage /> },
      { path: 'providers/:id/edit', element: <ProviderEditPage /> },
    ],
  },
]);
