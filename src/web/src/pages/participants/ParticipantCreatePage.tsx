import { Link, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { ParticipantForm, type ParticipantFormData } from '@/components/participants/ParticipantForm';
import { createParticipant } from '@/hooks/useParticipants';
import { ArrowLeft } from 'lucide-react';

export function ParticipantCreatePage() {
  const navigate = useNavigate();

  const handleSubmit = async (data: ParticipantFormData) => {
    const participant = await createParticipant({
      ndisNumber: data.ndisNumber,
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
          <Link to="/participants">
            <ArrowLeft className="mr-1 h-4 w-4" />
            Participants
          </Link>
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">Add Participant</h1>
        <p className="text-sm text-muted-foreground">
          Enter the participant's NDIS details and contact information.
        </p>
      </div>
      <ParticipantForm onSubmit={handleSubmit} />
    </div>
  );
}
