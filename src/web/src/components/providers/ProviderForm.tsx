import { useState } from 'react';
import { useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { validateRequired, validateEmail } from '@/lib/validation';
import { ApiError } from '@/lib/api-error';
import { Building2, Loader2 } from 'lucide-react';

export interface ProviderFormData {
  name: string;
  abn: string;
  contactEmail: string;
  contactPhone: string;
  address: string;
}

interface ProviderFormProps {
  initialValues?: Partial<ProviderFormData>;
  onSubmit: (data: ProviderFormData) => Promise<void>;
  isEdit?: boolean;
}

const emptyForm: ProviderFormData = {
  name: '',
  abn: '',
  contactEmail: '',
  contactPhone: '',
  address: '',
};

export function ProviderForm({ initialValues, onSubmit, isEdit }: ProviderFormProps) {
  const navigate = useNavigate();
  const [form, setForm] = useState<ProviderFormData>({ ...emptyForm, ...initialValues });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [serverErrors, setServerErrors] = useState<Record<string, string[]>>({});
  const [saving, setSaving] = useState(false);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    const nameError = validateRequired(form.name, 'Provider name');
    if (nameError) newErrors.name = nameError;

    if (form.abn && !/^\d{11}$/.test(form.abn.replace(/\s/g, ''))) {
      newErrors.abn = 'ABN must be 11 digits';
    }

    const emailError = validateEmail(form.contactEmail);
    if (emailError) newErrors.contactEmail = emailError;

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

  const field = (name: keyof ProviderFormData) =>
    errors[name] ?? serverErrors[name]?.[0] ?? null;

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Building2 className="h-4 w-4 text-muted-foreground" />
            Provider Details
          </CardTitle>
          <CardDescription>
            Service provider information. Fields marked with * are required.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Provider Name *</Label>
            <Input
              id="name"
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              placeholder="e.g. Allied Health Plus"
              required
            />
            {field('name') && <p className="text-sm text-destructive">{field('name')}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="abn">ABN</Label>
            <Input
              id="abn"
              value={form.abn}
              onChange={(e) => setForm({ ...form, abn: e.target.value })}
              placeholder="11 digit Australian Business Number"
              maxLength={11}
              className="max-w-xs font-mono"
            />
            {field('abn') && <p className="text-sm text-destructive">{field('abn')}</p>}
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="contactEmail">Contact Email</Label>
              <Input
                id="contactEmail"
                type="email"
                value={form.contactEmail}
                onChange={(e) => setForm({ ...form, contactEmail: e.target.value })}
              />
              {field('contactEmail') && <p className="text-sm text-destructive">{field('contactEmail')}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="contactPhone">Contact Phone</Label>
              <Input
                id="contactPhone"
                value={form.contactPhone}
                onChange={(e) => setForm({ ...form, contactPhone: e.target.value })}
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

      <div className="flex gap-3">
        <Button type="submit" disabled={saving}>
          {saving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {saving ? 'Saving...' : isEdit ? 'Update Provider' : 'Create Provider'}
        </Button>
        <Button type="button" variant="outline" onClick={() => navigate(-1)}>
          Cancel
        </Button>
      </div>
    </form>
  );
}
