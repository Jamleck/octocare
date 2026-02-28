import { Link, useParams, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { ProviderForm, type ProviderFormData } from '@/components/providers/ProviderForm';
import { useProvider, updateProvider } from '@/hooks/useProviders';
import { Skeleton } from '@/components/ui/skeleton';
import { ArrowLeft } from 'lucide-react';

export function ProviderEditPage() {
  const { id } = useParams<{ id: string }>();
  const { provider, isLoading } = useProvider(id!);
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

  if (!provider) {
    return <p className="text-muted-foreground">Provider not found.</p>;
  }

  const handleSubmit = async (data: ProviderFormData) => {
    await updateProvider(provider.id, {
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
          <Link to={`/providers/${provider.id}`}>
            <ArrowLeft className="mr-1 h-4 w-4" />
            {provider.name}
          </Link>
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">Edit Provider</h1>
        <p className="text-sm text-muted-foreground">
          Update details for {provider.name}.
        </p>
      </div>
      <ProviderForm
        initialValues={{
          name: provider.name,
          abn: provider.abn ?? '',
          contactEmail: provider.contactEmail ?? '',
          contactPhone: provider.contactPhone ?? '',
          address: provider.address ?? '',
        }}
        onSubmit={handleSubmit}
        isEdit
      />
    </div>
  );
}
