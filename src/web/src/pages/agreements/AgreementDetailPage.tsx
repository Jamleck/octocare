import { useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import {
  useServiceAgreement,
  activateServiceAgreement,
  addBooking,
  cancelBooking,
} from '@/hooks/useServiceAgreements';
import { usePlans } from '@/hooks/usePlans';
import { ErrorBanner } from '@/components/ErrorBanner';
import {
  ArrowLeft,
  Play,
  Plus,
  FileText,
  Calendar,
  Building2,
  DollarSign,
  Layers,
  XCircle,
  ClipboardList,
} from 'lucide-react';
import type { ServiceAgreementStatus, ServiceBookingStatus, BudgetCategory } from '@/types/api';

const statusConfig: Record<ServiceAgreementStatus, { label: string; className: string }> = {
  draft: { label: 'Draft', className: 'border-yellow-200 bg-yellow-50 text-yellow-700' },
  sent: { label: 'Sent', className: 'border-blue-200 bg-blue-50 text-blue-700' },
  active: { label: 'Active', className: 'border-emerald-200 bg-emerald-50 text-emerald-700' },
  expired: { label: 'Expired', className: 'text-muted-foreground' },
  terminated: { label: 'Terminated', className: 'border-red-200 bg-red-50 text-red-700' },
};

const bookingStatusConfig: Record<ServiceBookingStatus, { label: string; className: string }> = {
  active: { label: 'Active', className: 'border-emerald-200 bg-emerald-50 text-emerald-700' },
  completed: { label: 'Completed', className: 'border-blue-200 bg-blue-50 text-blue-700' },
  cancelled: { label: 'Cancelled', className: 'text-muted-foreground' },
};

export function AgreementDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { agreement, isLoading, error, refetch } = useServiceAgreement(id!);
  const navigate = useNavigate();
  const [activating, setActivating] = useState(false);
  const [addBookingOpen, setAddBookingOpen] = useState(false);
  const [bookingError, setBookingError] = useState<string | null>(null);
  const [bookingSubmitting, setBookingSubmitting] = useState(false);
  const [cancellingBookingId, setCancellingBookingId] = useState<string | null>(null);

  // Booking form state
  const [newBudgetCategoryId, setNewBudgetCategoryId] = useState('');
  const [newAllocatedAmount, setNewAllocatedAmount] = useState('');

  // Fetch plan to get budget categories for booking form
  const { plans } = usePlans(agreement?.participantId ?? '');
  const currentPlan = plans.find((p) => p.id === agreement?.planId);
  const budgetCategories: BudgetCategory[] = currentPlan?.budgetCategories ?? [];

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-5 w-32" />
        <Skeleton className="h-8 w-64" />
        <div className="grid gap-6 md:grid-cols-2">
          <Skeleton className="h-48 rounded-lg" />
          <Skeleton className="h-48 rounded-lg" />
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <Button
          variant="ghost"
          size="sm"
          className="-ml-2 text-muted-foreground"
          onClick={() => navigate(-1)}
        >
          <ArrowLeft className="mr-1 h-4 w-4" />
          Back
        </Button>
        <ErrorBanner message={error.message} onRetry={refetch} />
      </div>
    );
  }

  if (!agreement) {
    return (
      <div className="flex flex-col items-center py-16">
        <ClipboardList className="h-10 w-10 text-muted-foreground" />
        <p className="mt-3 text-lg font-medium">Agreement not found</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate(-1)}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Go Back
        </Button>
      </div>
    );
  }

  const status = statusConfig[agreement.status] || statusConfig.draft;
  const isDraftOrSent = agreement.status === 'draft' || agreement.status === 'sent';
  const isActive = agreement.status === 'active';
  const totalBooked = agreement.bookings.reduce((sum, b) => sum + b.allocatedAmount, 0);
  const totalUsed = agreement.bookings.reduce((sum, b) => sum + b.usedAmount, 0);

  const handleActivate = async () => {
    try {
      setActivating(true);
      await activateServiceAgreement(agreement.id);
      refetch();
    } catch {
      // error will be shown on next fetch
    } finally {
      setActivating(false);
    }
  };

  const handleAddBooking = async (e: React.FormEvent) => {
    e.preventDefault();
    setBookingError(null);

    if (!newBudgetCategoryId || !newAllocatedAmount) {
      setBookingError('All fields are required.');
      return;
    }
    const amount = parseFloat(newAllocatedAmount);
    if (isNaN(amount) || amount <= 0) {
      setBookingError('Amount must be greater than zero.');
      return;
    }

    try {
      setBookingSubmitting(true);
      await addBooking(agreement.id, {
        budgetCategoryId: newBudgetCategoryId,
        allocatedAmount: amount,
      });
      setAddBookingOpen(false);
      setNewBudgetCategoryId('');
      setNewAllocatedAmount('');
      refetch();
    } catch (err) {
      setBookingError(err instanceof Error ? err.message : 'Failed to add booking.');
    } finally {
      setBookingSubmitting(false);
    }
  };

  const handleCancelBooking = async (bookingId: string) => {
    try {
      setCancellingBookingId(bookingId);
      await cancelBooking(agreement.id, bookingId);
      refetch();
    } catch {
      // error will be shown on next fetch
    } finally {
      setCancellingBookingId(null);
    }
  };

  const categoryLabel = (cat: string) => {
    const labels: Record<string, string> = {
      Core: 'Core',
      CapacityBuilding: 'Capacity Building',
      Capital: 'Capital',
    };
    return labels[cat] || cat;
  };

  const purposeLabel = (purpose: string) => {
    const labels: Record<string, string> = {
      DailyActivities: 'Daily Activities',
      TransportActivities: 'Transport',
      IncreasedSocialAndCommunityParticipation: 'Social & Community',
      AssistiveTechnology: 'Assistive Technology',
      HomeModifications: 'Home Modifications',
      CoordinationOfSupports: 'Coordination of Supports',
      ImprovedDailyLivingSkills: 'Improved Daily Living',
    };
    return labels[purpose] || purpose;
  };

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to={`/participants/${agreement.participantId}`}>
            <ArrowLeft className="mr-1 h-4 w-4" />
            {agreement.participantName}
          </Link>
        </Button>

        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-primary">
              <ClipboardList className="h-5 w-5" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Service Agreement</h1>
              <p className="text-sm text-muted-foreground">
                {agreement.providerName} &middot; {agreement.planNumber}
              </p>
            </div>
            <Badge variant="secondary" className={status.className}>
              {status.label}
            </Badge>
          </div>
          <div className="flex gap-2">
            {isDraftOrSent && (
              <Button onClick={handleActivate} disabled={activating}>
                <Play className="mr-2 h-4 w-4" />
                {activating ? 'Activating...' : 'Activate'}
              </Button>
            )}
          </div>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <FileText className="h-4 w-4 text-muted-foreground" />
              Agreement Details
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <DetailRow icon={Building2} label="Provider" value={agreement.providerName} />
            <DetailRow icon={FileText} label="Plan" value={agreement.planNumber} mono />
            <DetailRow
              icon={Calendar}
              label="Start Date"
              value={new Date(agreement.startDate).toLocaleDateString('en-AU')}
            />
            <DetailRow
              icon={Calendar}
              label="End Date"
              value={new Date(agreement.endDate).toLocaleDateString('en-AU')}
            />
            <DetailRow
              icon={DollarSign}
              label="Total Booked"
              value={`$${totalBooked.toLocaleString('en-AU', { minimumFractionDigits: 2 })}`}
            />
            <DetailRow
              icon={DollarSign}
              label="Total Used"
              value={`$${totalUsed.toLocaleString('en-AU', { minimumFractionDigits: 2 })}`}
            />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <Layers className="h-4 w-4 text-muted-foreground" />
              Service Items
            </CardTitle>
          </CardHeader>
          <CardContent>
            {agreement.items.length === 0 ? (
              <div className="flex flex-col items-center py-6 text-center">
                <div className="rounded-full bg-muted p-2.5">
                  <Layers className="h-4 w-4 text-muted-foreground" />
                </div>
                <p className="mt-2 text-sm text-muted-foreground">No service items</p>
              </div>
            ) : (
              <div className="space-y-3">
                {agreement.items.map((item) => (
                  <div
                    key={item.id}
                    className="flex items-center justify-between rounded-lg border p-3"
                  >
                    <div>
                      <p className="font-mono text-sm">{item.supportItemNumber}</p>
                      {item.frequency && (
                        <p className="text-xs text-muted-foreground">{item.frequency}</p>
                      )}
                    </div>
                    <span className="font-medium">
                      ${item.agreedRate.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                      /hr
                    </span>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Bookings Section */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-2 text-base">
              <DollarSign className="h-4 w-4 text-muted-foreground" />
              Bookings
            </CardTitle>
            {(isDraftOrSent || isActive) && (
              <Button variant="outline" size="sm" onClick={() => setAddBookingOpen(true)}>
                <Plus className="mr-1 h-3 w-3" />
                Add Booking
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {agreement.bookings.length === 0 ? (
            <div className="flex flex-col items-center py-6 text-center">
              <div className="rounded-full bg-muted p-2.5">
                <DollarSign className="h-4 w-4 text-muted-foreground" />
              </div>
              <p className="mt-2 text-sm text-muted-foreground">No bookings yet</p>
              {(isDraftOrSent || isActive) && (
                <Button
                  variant="link"
                  size="sm"
                  className="mt-1"
                  onClick={() => setAddBookingOpen(true)}
                >
                  Add a booking
                </Button>
              )}
            </div>
          ) : (
            <div className="rounded-lg border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Category</TableHead>
                    <TableHead className="text-right">Allocated</TableHead>
                    <TableHead className="text-right">Used</TableHead>
                    <TableHead className="text-right">Remaining</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {agreement.bookings.map((booking) => {
                    const remaining = booking.allocatedAmount - booking.usedAmount;
                    const bStatus =
                      bookingStatusConfig[booking.status] || bookingStatusConfig.active;
                    return (
                      <TableRow key={booking.id}>
                        <TableCell>{categoryLabel(booking.supportCategory)}</TableCell>
                        <TableCell className="text-right font-medium">
                          $
                          {booking.allocatedAmount.toLocaleString('en-AU', {
                            minimumFractionDigits: 2,
                          })}
                        </TableCell>
                        <TableCell className="text-right">
                          $
                          {booking.usedAmount.toLocaleString('en-AU', {
                            minimumFractionDigits: 2,
                          })}
                        </TableCell>
                        <TableCell className="text-right">
                          $
                          {remaining.toLocaleString('en-AU', {
                            minimumFractionDigits: 2,
                          })}
                        </TableCell>
                        <TableCell>
                          <Badge variant="secondary" className={bStatus.className}>
                            {bStatus.label}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          {booking.status === 'active' && (
                            <Button
                              variant="ghost"
                              size="sm"
                              className="text-destructive hover:text-destructive"
                              onClick={() => handleCancelBooking(booking.id)}
                              disabled={cancellingBookingId === booking.id}
                            >
                              <XCircle className="mr-1 h-3 w-3" />
                              {cancellingBookingId === booking.id ? 'Cancelling...' : 'Cancel'}
                            </Button>
                          )}
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Add Booking Dialog */}
      <Dialog open={addBookingOpen} onOpenChange={setAddBookingOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add Booking</DialogTitle>
            <DialogDescription>
              Allocate budget from a plan category to this service agreement.
            </DialogDescription>
          </DialogHeader>
          {bookingError && <ErrorBanner message={bookingError} />}
          <form onSubmit={handleAddBooking} className="space-y-4">
            <div className="space-y-2">
              <Label>Budget Category</Label>
              <Select value={newBudgetCategoryId} onValueChange={setNewBudgetCategoryId}>
                <SelectTrigger>
                  <SelectValue placeholder="Select budget category" />
                </SelectTrigger>
                <SelectContent>
                  {budgetCategories.map((bc) => (
                    <SelectItem key={bc.id} value={bc.id}>
                      {categoryLabel(bc.supportCategory)} &mdash; {purposeLabel(bc.supportPurpose)}{' '}
                      ($
                      {bc.allocatedAmount.toLocaleString('en-AU', { minimumFractionDigits: 2 })})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="allocatedAmount">Allocated Amount ($)</Label>
              <Input
                id="allocatedAmount"
                type="number"
                step="0.01"
                min="0.01"
                value={newAllocatedAmount}
                onChange={(e) => setNewAllocatedAmount(e.target.value)}
                placeholder="e.g. 5000.00"
              />
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setAddBookingOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={bookingSubmitting}>
                {bookingSubmitting ? 'Adding...' : 'Add Booking'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function DetailRow({
  icon: Icon,
  label,
  value,
  mono,
}: {
  icon: typeof FileText;
  label: string;
  value?: string | null;
  mono?: boolean;
}) {
  return (
    <div className="flex items-center gap-3 text-sm">
      <Icon className="h-4 w-4 shrink-0 text-muted-foreground" />
      <span className="min-w-[100px] text-muted-foreground">{label}</span>
      <span className={`${mono ? 'font-mono' : ''} ${value ? '' : 'text-muted-foreground'}`}>
        {value || '--'}
      </span>
    </div>
  );
}
