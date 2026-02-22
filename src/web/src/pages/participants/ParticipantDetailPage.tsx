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
} from 'lucide-react';

export function ParticipantDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { participant, isLoading } = useParticipant(id!);
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
