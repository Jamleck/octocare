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
    ],
  },
]);
