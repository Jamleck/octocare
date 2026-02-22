import { useNavigate } from 'react-router';
import { ParticipantForm, type ParticipantFormData } from '@/components/participants/ParticipantForm';
import { createParticipant } from '@/hooks/useParticipants';

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
      <h1 className="text-2xl font-bold">Add Participant</h1>
      <ParticipantForm onSubmit={handleSubmit} />
    </div>
  );
}
