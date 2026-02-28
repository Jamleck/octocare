import { Link } from 'react-router';
import { useAuth } from '@/hooks/useAuth';
import { useParticipants } from '@/hooks/useParticipants';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import {
  Users,
  FileText,
  Send,
  Clock,
  UserPlus,
  ClipboardList,
  ArrowRight,
  Activity,
} from 'lucide-react';

const quickActions = [
  { label: 'Add Participant', to: '/participants/new', icon: UserPlus },
  { label: 'View Participants', to: '/participants', icon: ClipboardList },
  { label: 'Manage Team', to: '/members', icon: Users },
];

export function DashboardPage() {
  const { user } = useAuth();
  const { totalCount, isLoading: participantsLoading } = useParticipants(1, 1);

  const kpiCards = [
    {
      title: 'Active Participants',
      value: participantsLoading ? '...' : String(totalCount),
      icon: Users,
      color: 'text-primary bg-primary/10',
      description: 'Total managed',
    },
    {
      title: 'Pending Invoices',
      value: '--',
      icon: FileText,
      color: 'text-amber-600 bg-amber-50',
      description: 'Awaiting review',
    },
    {
      title: 'Claims This Month',
      value: '--',
      icon: Send,
      color: 'text-emerald-600 bg-emerald-50',
      description: 'Submitted to NDIA',
    },
    {
      title: 'Plans Expiring Soon',
      value: '--',
      icon: Clock,
      color: 'text-rose-600 bg-rose-50',
      description: 'Within 90 days',
    },
  ];

  const greeting = getGreeting();

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">
          {greeting}, {user?.name?.split(' ')[0] ?? 'there'}
        </h1>
        <p className="text-muted-foreground">
          Here's what's happening with your organisation today.
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {kpiCards.map((card) => (
          <Card key={card.title} className="relative overflow-hidden">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {card.title}
              </CardTitle>
              <div className={`rounded-lg p-2 ${card.color}`}>
                <card.icon className="h-4 w-4" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{card.value}</div>
              <p className="mt-1 text-xs text-muted-foreground">{card.description}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <Activity className="h-4 w-4 text-muted-foreground" />
              Recent Activity
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-col items-center justify-center py-8 text-center">
              <div className="rounded-full bg-muted p-3">
                <Activity className="h-5 w-5 text-muted-foreground" />
              </div>
              <p className="mt-3 text-sm font-medium">No recent activity</p>
              <p className="mt-1 text-xs text-muted-foreground">
                Activity from your organisation will appear here.
              </p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Quick Actions</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-2">
            {quickActions.map((action) => (
              <Button
                key={action.to}
                variant="outline"
                className="h-auto justify-start gap-3 px-3 py-3"
                asChild
              >
                <Link to={action.to}>
                  <div className="rounded-md bg-primary/10 p-1.5 text-primary">
                    <action.icon className="h-4 w-4" />
                  </div>
                  <span className="flex-1 text-left text-sm">{action.label}</span>
                  <ArrowRight className="h-4 w-4 text-muted-foreground" />
                </Link>
              </Button>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

function getGreeting(): string {
  const hour = new Date().getHours();
  if (hour < 12) return 'Good morning';
  if (hour < 17) return 'Good afternoon';
  return 'Good evening';
}
