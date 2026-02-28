import { Link, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { ProviderForm, type ProviderFormData } from '@/components/providers/ProviderForm';
import { createProvider } from '@/hooks/useProviders';
import { ArrowLeft } from 'lucide-react';

export function ProviderCreatePage() {
  const navigate = useNavigate();

  const handleSubmit = async (data: ProviderFormData) => {
    const provider = await createProvider({
      name: data.name,
      abn: data.abn || undefined,
      contactEmail: data.contactEmail || undefined,
      contactPhone: data.contactPhone || undefined,
      address: data.address || undefined,
    });
    navigate(`/providers/${provider.id}`);
  };

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to="/providers">
            <ArrowLeft className="mr-1 h-4 w-4" />
            Providers
          </Link>
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">Add Provider</h1>
        <p className="text-sm text-muted-foreground">
          Enter the service provider's details and contact information.
        </p>
      </div>
      <ProviderForm onSubmit={handleSubmit} />
    </div>
  );
}
