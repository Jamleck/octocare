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

export function ParticipantDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { participant, isLoading } = useParticipant(id!);
  const navigate = useNavigate();
  const [confirmOpen, setConfirmOpen] = useState(false);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-48 w-full" />
      </div>
    );
  }

  if (!participant) {
    return <p className="text-muted-foreground">Participant not found.</p>;
  }

  const handleDeactivate = async () => {
    await deactivateParticipant(participant.id);
    navigate('/participants');
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-bold">
            {participant.firstName} {participant.lastName}
          </h1>
          <Badge variant={participant.isActive ? 'outline' : 'secondary'}>
            {participant.isActive ? 'Active' : 'Inactive'}
          </Badge>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" asChild>
            <Link to={`/participants/${participant.id}/edit`}>Edit</Link>
          </Button>
          {participant.isActive && (
            <Button variant="destructive" onClick={() => setConfirmOpen(true)}>
              Deactivate
            </Button>
          )}
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Personal Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <DetailRow label="NDIS Number" value={participant.ndisNumber} mono />
            <DetailRow label="Date of Birth" value={new Date(participant.dateOfBirth).toLocaleDateString('en-AU')} />
            <DetailRow label="Email" value={participant.email} />
            <DetailRow label="Phone" value={participant.phone} />
            <DetailRow label="Address" value={participant.address} />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Nominee / Guardian</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {participant.nomineeName ? (
              <>
                <DetailRow label="Name" value={participant.nomineeName} />
                <DetailRow label="Relationship" value={participant.nomineeRelationship} />
                <DetailRow label="Email" value={participant.nomineeEmail} />
                <DetailRow label="Phone" value={participant.nomineePhone} />
              </>
            ) : (
              <p className="text-sm text-muted-foreground">No nominee recorded.</p>
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

function DetailRow({ label, value, mono }: { label: string; value?: string | null; mono?: boolean }) {
  return (
    <div className="flex justify-between text-sm">
      <span className="text-muted-foreground">{label}</span>
      <span className={mono ? 'font-mono' : ''}>{value || '--'}</span>
    </div>
  );
}
