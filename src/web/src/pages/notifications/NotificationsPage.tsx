import { useState } from 'react';
import { useNavigate } from 'react-router';
import { Bell, Check, CheckCheck } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  useNotifications,
  useUnreadCount,
  markNotificationRead,
  markAllNotificationsRead,
} from '@/hooks/useNotifications';
import type { Notification } from '@/types/api';

const notificationTypes: { value: string; label: string }[] = [
  { value: 'all', label: 'All Types' },
  { value: 'InvoiceSubmitted', label: 'Invoice Submitted' },
  { value: 'InvoiceApproved', label: 'Invoice Approved' },
  { value: 'InvoiceRejected', label: 'Invoice Rejected' },
  { value: 'PlanExpiring', label: 'Plan Expiring' },
  { value: 'BudgetAlert', label: 'Budget Alert' },
  { value: 'ClaimSubmitted', label: 'Claim Submitted' },
  { value: 'ClaimOutcome', label: 'Claim Outcome' },
  { value: 'StatementGenerated', label: 'Statement Generated' },
  { value: 'General', label: 'General' },
];

function getTypeBadgeVariant(type: string): 'default' | 'secondary' | 'destructive' | 'outline' {
  switch (type) {
    case 'InvoiceRejected':
      return 'destructive';
    case 'BudgetAlert':
    case 'PlanExpiring':
      return 'default';
    case 'InvoiceApproved':
    case 'ClaimOutcome':
      return 'secondary';
    default:
      return 'outline';
  }
}

function formatTypeLabel(type: string): string {
  return type.replace(/([A-Z])/g, ' $1').trim();
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleString();
}

export function NotificationsPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [typeFilter, setTypeFilter] = useState<string>('all');
  const [readFilter, setReadFilter] = useState<string>('all');

  const { notifications, totalCount, isLoading, refetch } = useNotifications({
    page,
    pageSize: 20,
    unreadOnly: readFilter === 'unread' ? true : undefined,
    type: typeFilter !== 'all' ? typeFilter : undefined,
  });
  const { count: unreadCount, refetch: refetchCount } = useUnreadCount();

  const totalPages = Math.ceil(totalCount / 20);

  const handleMarkRead = async (notification: Notification) => {
    if (!notification.isRead) {
      await markNotificationRead(notification.id);
      refetch();
      refetchCount();
    }
  };

  const handleMarkAllRead = async () => {
    await markAllNotificationsRead();
    refetch();
    refetchCount();
  };

  const handleNotificationClick = async (notification: Notification) => {
    await handleMarkRead(notification);
    if (notification.link) {
      navigate(notification.link);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Notifications</h1>
          <p className="text-muted-foreground">
            {unreadCount > 0
              ? `You have ${unreadCount} unread notification${unreadCount > 1 ? 's' : ''}`
              : 'All caught up!'}
          </p>
        </div>
        {unreadCount > 0 && (
          <Button variant="outline" size="sm" onClick={handleMarkAllRead}>
            <CheckCheck className="mr-2 h-4 w-4" />
            Mark all as read
          </Button>
        )}
      </div>

      <div className="flex items-center gap-3">
        <Select value={readFilter} onValueChange={(v) => { setReadFilter(v); setPage(1); }}>
          <SelectTrigger className="w-[160px]">
            <SelectValue placeholder="Filter by status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All</SelectItem>
            <SelectItem value="unread">Unread only</SelectItem>
          </SelectContent>
        </Select>

        <Select value={typeFilter} onValueChange={(v) => { setTypeFilter(v); setPage(1); }}>
          <SelectTrigger className="w-[200px]">
            <SelectValue placeholder="Filter by type" />
          </SelectTrigger>
          <SelectContent>
            {notificationTypes.map((t) => (
              <SelectItem key={t.value} value={t.value}>
                {t.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {isLoading ? (
        <Card>
          <CardContent className="flex items-center justify-center py-12">
            <p className="text-muted-foreground">Loading notifications...</p>
          </CardContent>
        </Card>
      ) : notifications.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Bell className="mb-4 h-12 w-12 text-muted-foreground/30" />
            <p className="text-lg font-medium text-muted-foreground">No notifications</p>
            <p className="text-sm text-muted-foreground/70">
              {readFilter === 'unread'
                ? 'All notifications have been read.'
                : 'You have no notifications yet.'}
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-2">
          {notifications.map((notification) => (
            <Card
              key={notification.id}
              className={`cursor-pointer transition-colors hover:bg-accent/50 ${
                !notification.isRead ? 'border-primary/20 bg-primary/5' : ''
              }`}
              onClick={() => handleNotificationClick(notification)}
            >
              <CardContent className="flex items-start gap-3 p-4">
                {!notification.isRead && (
                  <span className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-primary" />
                )}
                <div className={`flex-1 ${notification.isRead ? 'pl-4' : ''}`}>
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <p className="font-medium">{notification.title}</p>
                      <p className="mt-1 text-sm text-muted-foreground">{notification.message}</p>
                    </div>
                    <div className="flex shrink-0 flex-col items-end gap-1">
                      <Badge variant={getTypeBadgeVariant(notification.type)} className="text-[10px]">
                        {formatTypeLabel(notification.type)}
                      </Badge>
                      <span className="text-xs text-muted-foreground">
                        {formatDate(notification.createdAt)}
                      </span>
                    </div>
                  </div>
                  {notification.link && (
                    <p className="mt-1 text-xs text-primary">{notification.link}</p>
                  )}
                </div>
                {!notification.isRead && (
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8 shrink-0"
                    onClick={(e) => {
                      e.stopPropagation();
                      handleMarkRead(notification);
                    }}
                    title="Mark as read"
                  >
                    <Check className="h-4 w-4" />
                  </Button>
                )}
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            Showing {(page - 1) * 20 + 1}-{Math.min(page * 20, totalCount)} of {totalCount}
          </p>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={page <= 1}
              onClick={() => setPage(page - 1)}
            >
              Previous
            </Button>
            <span className="text-sm text-muted-foreground">
              Page {page} of {totalPages}
            </span>
            <Button
              variant="outline"
              size="sm"
              disabled={page >= totalPages}
              onClick={() => setPage(page + 1)}
            >
              Next
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
