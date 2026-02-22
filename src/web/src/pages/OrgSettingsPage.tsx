import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useOrganisation } from '@/hooks/useOrganisation';
import { Skeleton } from '@/components/ui/skeleton';
import type { UpdateOrganisationRequest } from '@/types/api';
import { ApiError } from '@/lib/api-error';

export function OrgSettingsPage() {
  const { organisation, isLoading, updateOrganisation } = useOrganisation();
  const [form, setForm] = useState<UpdateOrganisationRequest>({
    name: '',
    abn: '',
    contactEmail: '',
    contactPhone: '',
    address: '',
  });
  const [errors, setErrors] = useState<Record<string, string[]>>({});
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    if (organisation) {
      setForm({
        name: organisation.name,
        abn: organisation.abn ?? '',
        contactEmail: organisation.contactEmail ?? '',
        contactPhone: organisation.contactPhone ?? '',
        address: organisation.address ?? '',
      });
    }
  }, [organisation]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});
    setSaving(true);
    setSaved(false);
    try {
      await updateOrganisation(form);
      setSaved(true);
      setTimeout(() => setSaved(false), 3000);
    } catch (err) {
      if (err instanceof ApiError && err.validationErrors) {
        setErrors(err.validationErrors);
      }
    } finally {
      setSaving(false);
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Organisation Settings</h1>

      <Card>
        <CardHeader>
          <CardTitle>Organisation Details</CardTitle>
          <CardDescription>Update your organisation's information.</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Organisation Name</Label>
              <Input
                id="name"
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                required
              />
              {errors.Name && <p className="text-sm text-destructive">{errors.Name[0]}</p>}
            </div>

            <div className="space-y-2">
              <Label htmlFor="abn">ABN</Label>
              <Input
                id="abn"
                value={form.abn}
                onChange={(e) => setForm({ ...form, abn: e.target.value })}
                placeholder="11 digit ABN"
                maxLength={11}
              />
              {errors.Abn && <p className="text-sm text-destructive">{errors.Abn[0]}</p>}
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
                {errors.ContactEmail && (
                  <p className="text-sm text-destructive">{errors.ContactEmail[0]}</p>
                )}
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

            <div className="flex items-center gap-4">
              <Button type="submit" disabled={saving}>
                {saving ? 'Saving...' : 'Save Changes'}
              </Button>
              {saved && <p className="text-sm text-green-600">Changes saved.</p>}
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
