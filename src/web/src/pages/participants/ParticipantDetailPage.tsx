import { useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Skeleton } from '@/components/ui/skeleton';
import { useParticipant, deactivateParticipant } from '@/hooks/useParticipants';
import { usePlans } from '@/hooks/usePlans';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  ArrowLeft,
  Pencil,
  UserX,
  User,
  Phone,
  Mail,
  MapPin,
  Calendar,
  Hash,
  Heart,
  FileText,
  Plus,
} from 'lucide-react';
import type { PlanStatus } from '@/types/api';

export function ParticipantDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { participant, isLoading } = useParticipant(id!);
  const { plans, isLoading: plansLoading } = usePlans(id!);
  const navigate = useNavigate();
  const [confirmOpen, setConfirmOpen] = useState(false);

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

  if (!participant) {
    return (
      <div className="flex flex-col items-center py-16">
        <User className="h-10 w-10 text-muted-foreground" />
        <p className="mt-3 text-lg font-medium">Participant not found</p>
        <Button variant="outline" className="mt-4" asChild>
          <Link to="/participants">
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Participants
          </Link>
        </Button>
      </div>
    );
  }

  const handleDeactivate = async () => {
    await deactivateParticipant(participant.id);
    navigate('/participants');
  };

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to="/participants">
            <ArrowLeft className="mr-1 h-4 w-4" />
            Participants
          </Link>
        </Button>

        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-primary">
              <User className="h-5 w-5" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">
                {participant.firstName} {participant.lastName}
              </h1>
              <p className="font-mono text-sm text-muted-foreground">{participant.ndisNumber}</p>
            </div>
            <Badge
              variant="secondary"
              className={
                participant.isActive
                  ? 'border-emerald-200 bg-emerald-50 text-emerald-700'
                  : 'text-muted-foreground'
              }
            >
              {participant.isActive ? 'Active' : 'Inactive'}
            </Badge>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" asChild>
              <Link to={`/participants/${participant.id}/edit`}>
                <Pencil className="mr-2 h-4 w-4" />
                Edit
              </Link>
            </Button>
            {participant.isActive && (
              <Button variant="outline" className="text-destructive hover:text-destructive" onClick={() => setConfirmOpen(true)}>
                <UserX className="mr-2 h-4 w-4" />
                Deactivate
              </Button>
            )}
          </div>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <User className="h-4 w-4 text-muted-foreground" />
              Personal Details
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <DetailRow icon={Hash} label="NDIS Number" value={participant.ndisNumber} mono />
            <DetailRow icon={Calendar} label="Date of Birth" value={new Date(participant.dateOfBirth).toLocaleDateString('en-AU')} />
            <DetailRow icon={Mail} label="Email" value={participant.email} />
            <DetailRow icon={Phone} label="Phone" value={participant.phone} />
            <DetailRow icon={MapPin} label="Address" value={participant.address} />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <Heart className="h-4 w-4 text-muted-foreground" />
              Nominee / Guardian
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {participant.nomineeName ? (
              <>
                <DetailRow icon={User} label="Name" value={participant.nomineeName} />
                <DetailRow icon={Heart} label="Relationship" value={participant.nomineeRelationship} />
                <DetailRow icon={Mail} label="Email" value={participant.nomineeEmail} />
                <DetailRow icon={Phone} label="Phone" value={participant.nomineePhone} />
              </>
            ) : (
              <div className="flex flex-col items-center py-6 text-center">
                <div className="rounded-full bg-muted p-2.5">
                  <Heart className="h-4 w-4 text-muted-foreground" />
                </div>
                <p className="mt-2 text-sm text-muted-foreground">No nominee recorded</p>
                <Button variant="link" size="sm" className="mt-1" asChild>
                  <Link to={`/participants/${participant.id}/edit`}>Add nominee details</Link>
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Plans Section */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-2 text-base">
              <FileText className="h-4 w-4 text-muted-foreground" />
              NDIS Plans
            </CardTitle>
            <Button variant="outline" size="sm" asChild>
              <Link to={`/participants/${participant.id}/plans/new`}>
                <Plus className="mr-1 h-3 w-3" />
                Create Plan
              </Link>
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {plansLoading ? (
            <div className="space-y-2">
              <Skeleton className="h-8 w-full" />
              <Skeleton className="h-8 w-full" />
            </div>
          ) : plans.length === 0 ? (
            <div className="flex flex-col items-center py-6 text-center">
              <div className="rounded-full bg-muted p-2.5">
                <FileText className="h-4 w-4 text-muted-foreground" />
              </div>
              <p className="mt-2 text-sm text-muted-foreground">No plans yet</p>
              <Button variant="link" size="sm" className="mt-1" asChild>
                <Link to={`/participants/${participant.id}/plans/new`}>Create a plan</Link>
              </Button>
            </div>
          ) : (
            <div className="rounded-lg border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Plan Number</TableHead>
                    <TableHead>Start Date</TableHead>
                    <TableHead>End Date</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Budget</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {plans.map((plan) => {
                    const totalBudget = plan.budgetCategories.reduce((sum, bc) => sum + bc.allocatedAmount, 0);
                    return (
                      <TableRow
                        key={plan.id}
                        className="cursor-pointer transition-colors hover:bg-accent/50"
                        onClick={() => navigate(`/plans/${plan.id}`)}
                      >
                        <TableCell className="font-mono text-sm">{plan.planNumber}</TableCell>
                        <TableCell>{new Date(plan.startDate).toLocaleDateString('en-AU')}</TableCell>
                        <TableCell>{new Date(plan.endDate).toLocaleDateString('en-AU')}</TableCell>
                        <TableCell>
                          <PlanStatusBadge status={plan.status} />
                        </TableCell>
                        <TableCell className="text-right font-medium">
                          ${totalBudget.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
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

      <Dialog open={confirmOpen} onOpenChange={setConfirmOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Deactivate Participant</DialogTitle>
            <DialogDescription>
              Are you sure you want to deactivate {participant.firstName} {participant.lastName}?
              This will remove them from active participant lists.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirmOpen(false)}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={handleDeactivate}>
              Deactivate
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

const planStatusConfig: Record<PlanStatus, { label: string; className: string }> = {
  draft: { label: 'Draft', className: 'border-yellow-200 bg-yellow-50 text-yellow-700' },
  active: { label: 'Active', className: 'border-emerald-200 bg-emerald-50 text-emerald-700' },
  expiring: { label: 'Expiring', className: 'border-orange-200 bg-orange-50 text-orange-700' },
  expired: { label: 'Expired', className: 'text-muted-foreground' },
  transitioned: { label: 'Transitioned', className: 'border-blue-200 bg-blue-50 text-blue-700' },
};

function PlanStatusBadge({ status }: { status: PlanStatus }) {
  const config = planStatusConfig[status] || planStatusConfig.draft;
  return (
    <Badge variant="secondary" className={config.className}>
      {config.label}
    </Badge>
  );
}

function DetailRow({
  icon: Icon,
  label,
  value,
  mono,
}: {
  icon: typeof User;
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
