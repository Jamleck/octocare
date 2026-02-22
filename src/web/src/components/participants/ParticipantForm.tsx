import { useState } from 'react';
import { useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { validateNdisNumber, validateRequired, validateEmail, validateDateNotFuture } from '@/lib/validation';
import { ApiError } from '@/lib/api-error';
import { User, Heart, Loader2 } from 'lucide-react';

export interface ParticipantFormData {
  ndisNumber: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  phone: string;
  address: string;
  nomineeName: string;
  nomineeEmail: string;
  nomineePhone: string;
  nomineeRelationship: string;
}

interface ParticipantFormProps {
  initialValues?: Partial<ParticipantFormData>;
  onSubmit: (data: ParticipantFormData) => Promise<void>;
  isEdit?: boolean;
}

const emptyForm: ParticipantFormData = {
  ndisNumber: '',
  firstName: '',
  lastName: '',
  dateOfBirth: '',
  email: '',
  phone: '',
  address: '',
  nomineeName: '',
  nomineeEmail: '',
  nomineePhone: '',
  nomineeRelationship: '',
};

export function ParticipantForm({ initialValues, onSubmit, isEdit }: ParticipantFormProps) {
  const navigate = useNavigate();
  const [form, setForm] = useState<ParticipantFormData>({ ...emptyForm, ...initialValues });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [serverErrors, setServerErrors] = useState<Record<string, string[]>>({});
  const [saving, setSaving] = useState(false);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!isEdit) {
      const ndisError = validateNdisNumber(form.ndisNumber);
      if (ndisError) newErrors.ndisNumber = ndisError;
    }

    const firstNameError = validateRequired(form.firstName, 'First name');
    if (firstNameError) newErrors.firstName = firstNameError;

    const lastNameError = validateRequired(form.lastName, 'Last name');
    if (lastNameError) newErrors.lastName = lastNameError;

    const dobError = validateRequired(form.dateOfBirth, 'Date of birth') ?? validateDateNotFuture(form.dateOfBirth);
    if (dobError) newErrors.dateOfBirth = dobError;

    const emailError = validateEmail(form.email);
    if (emailError) newErrors.email = emailError;

    const nomineeEmailError = validateEmail(form.nomineeEmail);
    if (nomineeEmailError) newErrors.nomineeEmail = nomineeEmailError;

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setServerErrors({});
    if (!validate()) return;

    setSaving(true);
    try {
      await onSubmit(form);
    } catch (err) {
      if (err instanceof ApiError && err.validationErrors) {
        setServerErrors(err.validationErrors);
      }
    } finally {
      setSaving(false);
    }
  };

  const field = (name: keyof ParticipantFormData) =>
    errors[name] ?? serverErrors[name]?.[0] ?? null;

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <User className="h-4 w-4 text-muted-foreground" />
            Participant Details
          </CardTitle>
          <CardDescription>
            Core NDIS participant information. Fields marked with * are required.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="ndisNumber">NDIS Number *</Label>
            <Input
              id="ndisNumber"
              value={form.ndisNumber}
              onChange={(e) => setForm({ ...form, ndisNumber: e.target.value })}
              placeholder="9 digits starting with 43"
              maxLength={9}
              disabled={isEdit}
              className="max-w-xs font-mono"
            />
            {field('ndisNumber') && <p className="text-sm text-destructive">{field('ndisNumber')}</p>}
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="firstName">First Name *</Label>
              <Input
                id="firstName"
                value={form.firstName}
                onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                required
              />
              {field('firstName') && <p className="text-sm text-destructive">{field('firstName')}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="lastName">Last Name *</Label>
              <Input
                id="lastName"
                value={form.lastName}
                onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                required
              />
              {field('lastName') && <p className="text-sm text-destructive">{field('lastName')}</p>}
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="dateOfBirth">Date of Birth *</Label>
            <Input
              id="dateOfBirth"
              type="date"
              value={form.dateOfBirth}
              onChange={(e) => setForm({ ...form, dateOfBirth: e.target.value })}
              required
              className="max-w-xs"
            />
            {field('dateOfBirth') && <p className="text-sm text-destructive">{field('dateOfBirth')}</p>}
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
              />
              {field('email') && <p className="text-sm text-destructive">{field('email')}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="phone">Phone</Label>
              <Input
                id="phone"
                value={form.phone}
                onChange={(e) => setForm({ ...form, phone: e.target.value })}
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="address">Address</Label>
            <Input
              id="address"
              value={form.address}
              onChange={(e) => setForm({ ...form, address: e.target.value })}
            />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Heart className="h-4 w-4 text-muted-foreground" />
            Nominee / Guardian
          </CardTitle>
          <CardDescription>
            Optional. Add details of the participant's nominee or guardian if applicable.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="nomineeName">Name</Label>
              <Input
                id="nomineeName"
                value={form.nomineeName}
                onChange={(e) => setForm({ ...form, nomineeName: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="nomineeRelationship">Relationship</Label>
              <Input
                id="nomineeRelationship"
                value={form.nomineeRelationship}
                onChange={(e) => setForm({ ...form, nomineeRelationship: e.target.value })}
                placeholder="e.g. Mother, Spouse, Guardian"
              />
            </div>
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="nomineeEmail">Email</Label>
              <Input
                id="nomineeEmail"
                type="email"
                value={form.nomineeEmail}
                onChange={(e) => setForm({ ...form, nomineeEmail: e.target.value })}
              />
              {field('nomineeEmail') && <p className="text-sm text-destructive">{field('nomineeEmail')}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="nomineePhone">Phone</Label>
              <Input
                id="nomineePhone"
                value={form.nomineePhone}
                onChange={(e) => setForm({ ...form, nomineePhone: e.target.value })}
              />
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="flex gap-3">
        <Button type="submit" disabled={saving}>
          {saving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {saving ? 'Saving...' : isEdit ? 'Update Participant' : 'Create Participant'}
        </Button>
        <Button type="button" variant="outline" onClick={() => navigate(-1)}>
          Cancel
        </Button>
      </div>
    </form>
  );
}
