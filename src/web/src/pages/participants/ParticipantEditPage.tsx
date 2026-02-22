import { Link, useParams, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { ParticipantForm, type ParticipantFormData } from '@/components/participants/ParticipantForm';
import { useParticipant, updateParticipant } from '@/hooks/useParticipants';
import { Skeleton } from '@/components/ui/skeleton';
import { ArrowLeft } from 'lucide-react';

export function ParticipantEditPage() {
  const { id } = useParams<{ id: string }>();
  const { participant, isLoading } = useParticipant(id!);
  const navigate = useNavigate();

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-5 w-32" />
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-64 w-full rounded-lg" />
      </div>
    );
  }

  if (!participant) {
    return <p className="text-muted-foreground">Participant not found.</p>;
  }

  const handleSubmit = async (data: ParticipantFormData) => {
    await updateParticipant(participant.id, {
      firstName: data.firstName,
      lastName: data.lastName,
      dateOfBirth: data.dateOfBirth,
      email: data.email || undefined,
      phone: data.phone || undefined,
      address: data.address || undefined,
      nomineeName: data.nomineeName || undefined,
      nomineeEmail: data.nomineeEmail || undefined,
      nomineePhone: data.nomineePhone || undefined,
      nomineeRelationship: data.nomineeRelationship || undefined,
    });
    navigate(`/participants/${participant.id}`);
  };

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to={`/participants/${participant.id}`}>
            <ArrowLeft className="mr-1 h-4 w-4" />
            {participant.firstName} {participant.lastName}
          </Link>
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">Edit Participant</h1>
        <p className="text-sm text-muted-foreground">
          Update details for {participant.firstName} {participant.lastName}.
        </p>
      </div>
      <ParticipantForm
        initialValues={{
          ndisNumber: participant.ndisNumber,
          firstName: participant.firstName,
          lastName: participant.lastName,
          dateOfBirth: participant.dateOfBirth,
          email: participant.email ?? '',
          phone: participant.phone ?? '',
          address: participant.address ?? '',
          nomineeName: participant.nomineeName ?? '',
          nomineeEmail: participant.nomineeEmail ?? '',
          nomineePhone: participant.nomineePhone ?? '',
          nomineeRelationship: participant.nomineeRelationship ?? '',
        }}
        onSubmit={handleSubmit}
        isEdit
      />
    </div>
  );
}
