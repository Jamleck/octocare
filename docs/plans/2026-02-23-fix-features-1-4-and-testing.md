# Fix Features 1-4 & Comprehensive Testing Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Fix all bugs in features 1-4 (participants not displaying, silent errors, race conditions) and add comprehensive test coverage for both frontend and backend.

**Architecture:** Fix frontend data hooks with AbortController cleanup and search debounce, add error banners to all pages, harden the API client. Add backend integration tests using WebApplicationFactory with in-memory SQLite. Add frontend tests for API client, data hooks (mocked fetch), and component rendering.

**Tech Stack:** Vitest + Testing Library (frontend), xUnit + WebApplicationFactory + SQLite in-memory (backend)

---

## Part A: Bug Fixes

### Task 1: Add VITE_API_URL to .env.development

The `.env.development` file only sets `VITE_AUTH_BYPASS=true`. The API URL falls back to the hardcoded `http://localhost:5000` which may not match Aspire's dynamic port assignment. When running via Aspire, the URL is injected via `WithEnvironment("VITE_API_URL", ...)` so this mainly helps standalone `pnpm dev` usage.

**Files:**
- Modify: `src/web/.env.development`

**Step 1: Add the missing env var**

```
VITE_AUTH_BYPASS=true
VITE_API_URL=http://localhost:5000
```

**Step 2: Verify dev server starts**

Run: `cd src/web && pnpm dev` (confirm no errors)

**Step 3: Commit**

```bash
git add src/web/.env.development
git commit -m "fix: add VITE_API_URL to .env.development"
```

---

### Task 2: Harden API client — AbortSignal support, token error logging, request timeout

The API client silently swallows token errors and has no timeout. Add AbortSignal passthrough for hook cleanup and a 30s default timeout.

**Files:**
- Modify: `src/web/src/lib/api-client.ts`

**Step 1: Write failing test for abort support**

Create `src/web/__tests__/api-client.test.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest';

// We need to mock fetch globally
const mockFetch = vi.fn();
vi.stubGlobal('fetch', mockFetch);

// Reset modules to get a fresh api-client per test
beforeEach(() => {
  vi.resetAllMocks();
  mockFetch.mockResolvedValue({
    ok: true,
    status: 200,
    json: () => Promise.resolve({ id: '1' }),
  });
});

describe('api-client', () => {
  it('passes AbortSignal to fetch when provided', async () => {
    const { get } = await import('@/lib/api-client');
    const controller = new AbortController();

    await get('/api/test', { signal: controller.signal });

    expect(mockFetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({ signal: controller.signal }),
    );
  });

  it('returns undefined for 204 No Content', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      status: 204,
      json: () => Promise.reject(new Error('no body')),
    });
    const { get } = await import('@/lib/api-client');
    const result = await get('/api/test');
    expect(result).toBeUndefined();
  });

  it('throws ApiError with status for non-ok response', async () => {
    mockFetch.mockResolvedValue({
      ok: false,
      status: 404,
      statusText: 'Not Found',
      json: () => Promise.resolve({ detail: 'Not found' }),
    });
    const { get } = await import('@/lib/api-client');
    await expect(get('/api/test')).rejects.toThrow('Not found');
  });

  it('includes Authorization header when token getter is set', async () => {
    const { get, setTokenGetter } = await import('@/lib/api-client');
    setTokenGetter(async () => 'test-token');

    await get('/api/test');

    expect(mockFetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({
        headers: expect.objectContaining({
          Authorization: 'Bearer test-token',
        }),
      }),
    );
  });

  it('proceeds without Authorization header when token getter fails', async () => {
    const { get, setTokenGetter } = await import('@/lib/api-client');
    setTokenGetter(async () => { throw new Error('token expired'); });

    await get('/api/test');

    const [, init] = mockFetch.mock.calls[0];
    expect(init.headers).not.toHaveProperty('Authorization');
  });
});
```

**Step 2: Run test, verify it fails**

Run: `cd src/web && pnpm test`
Expected: FAIL (api-client doesn't accept options with signal)

**Step 3: Update api-client.ts to accept options and pass signal**

```typescript
import { ApiError } from './api-error';

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

let getTokenFn: (() => Promise<string>) | null = null;

export function setTokenGetter(fn: () => Promise<string>) {
  getTokenFn = fn;
}

export interface RequestOptions {
  signal?: AbortSignal;
}

async function request<T>(method: string, path: string, body?: unknown, options?: RequestOptions): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  if (getTokenFn) {
    try {
      const token = await getTokenFn();
      headers['Authorization'] = `Bearer ${token}`;
    } catch (err) {
      console.warn('Token acquisition failed:', err);
    }
  }

  const response = await fetch(`${API_URL}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
    signal: options?.signal,
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new ApiError(response.status, response.statusText, problem);
  }

  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

export function get<T>(path: string, options?: RequestOptions): Promise<T> {
  return request<T>('GET', path, undefined, options);
}

export function post<T>(path: string, body?: unknown, options?: RequestOptions): Promise<T> {
  return request<T>('POST', path, body, options);
}

export function put<T>(path: string, body: unknown, options?: RequestOptions): Promise<T> {
  return request<T>('PUT', path, body, options);
}

export function del<T>(path: string, options?: RequestOptions): Promise<T> {
  return request<T>('DELETE', path, undefined, options);
}
```

**Step 4: Run tests, verify they pass**

Run: `cd src/web && pnpm test`
Expected: ALL PASS

**Step 5: Commit**

```bash
git add src/web/src/lib/api-client.ts src/web/__tests__/api-client.test.ts
git commit -m "fix: harden API client with AbortSignal support and token error logging"
```

---

### Task 3: Add AbortController cleanup to all data hooks

All three hooks (`useParticipants`, `useMembers`, `useOrganisation`) can set state on unmounted components. Add AbortController cleanup and pass signal to API calls.

**Files:**
- Modify: `src/web/src/hooks/useParticipants.ts`
- Modify: `src/web/src/hooks/useMembers.ts`
- Modify: `src/web/src/hooks/useOrganisation.ts`

**Step 1: Update useParticipants with AbortController**

```typescript
import { useCallback, useEffect, useState } from 'react';
import { get, post, put } from '@/lib/api-client';
import type {
  Participant,
  CreateParticipantRequest,
  UpdateParticipantRequest,
  PagedResult,
} from '@/types/api';

export function useParticipants(page: number = 1, pageSize: number = 20, search?: string) {
  const [data, setData] = useState<PagedResult<Participant> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchParticipants() {
      try {
        setIsLoading(true);
        setError(null);
        const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
        if (search) params.set('search', search);
        const result = await get<PagedResult<Participant>>(`/api/participants?${params}`, { signal: controller.signal });
        setData(result);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch participants'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchParticipants();
    return () => controller.abort();
  }, [page, pageSize, search]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (search) params.set('search', search);
    get<PagedResult<Participant>>(`/api/participants?${params}`)
      .then(setData)
      .catch((err) => setError(err instanceof Error ? err : new Error('Failed to fetch participants')))
      .finally(() => setIsLoading(false));
  }, [page, pageSize, search]);

  return {
    participants: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    isLoading,
    error,
    refetch,
  };
}

export function useParticipant(id: string) {
  const [participant, setParticipant] = useState<Participant | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchParticipant() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<Participant>(`/api/participants/${id}`, { signal: controller.signal });
        setParticipant(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch participant'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchParticipant();
    return () => controller.abort();
  }, [id]);

  return { participant, isLoading, error };
}

export async function createParticipant(request: CreateParticipantRequest): Promise<Participant> {
  return post<Participant>('/api/participants', request);
}

export async function updateParticipant(id: string, request: UpdateParticipantRequest): Promise<Participant> {
  return put<Participant>(`/api/participants/${id}`, request);
}

export async function deactivateParticipant(id: string): Promise<void> {
  await post(`/api/participants/${id}/deactivate`);
}
```

**Step 2: Update useMembers with AbortController**

```typescript
import { useCallback, useEffect, useState } from 'react';
import { get, post, put } from '@/lib/api-client';
import type { Member, InviteMemberRequest, UpdateMemberRoleRequest } from '@/types/api';

export function useMembers() {
  const [members, setMembers] = useState<Member[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchMembers = useCallback(async (signal?: AbortSignal) => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await get<Member[]>('/api/organisations/current/members', { signal });
      setMembers(data);
    } catch (err) {
      if (err instanceof DOMException && err.name === 'AbortError') return;
      setError(err instanceof Error ? err : new Error('Failed to fetch members'));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    fetchMembers(controller.signal);
    return () => controller.abort();
  }, [fetchMembers]);

  const inviteMember = async (request: InviteMemberRequest) => {
    const member = await post<Member>('/api/organisations/current/members/invite', request);
    setMembers((prev) => [...prev, member]);
    return member;
  };

  const updateRole = async (userId: string, request: UpdateMemberRoleRequest) => {
    const member = await put<Member>(`/api/organisations/current/members/${userId}/role`, request);
    setMembers((prev) => prev.map((m) => (m.userId === userId ? member : m)));
    return member;
  };

  const deactivateMember = async (userId: string) => {
    await post(`/api/organisations/current/members/${userId}/deactivate`);
    setMembers((prev) => prev.map((m) => (m.userId === userId ? { ...m, isActive: false } : m)));
  };

  return { members, isLoading, error, inviteMember, updateRole, deactivateMember, refetch: () => fetchMembers() };
}
```

**Step 3: Update useOrganisation with AbortController**

```typescript
import { useCallback, useEffect, useState } from 'react';
import { get, put } from '@/lib/api-client';
import type { Organisation, UpdateOrganisationRequest } from '@/types/api';

export function useOrganisation() {
  const [organisation, setOrganisation] = useState<Organisation | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchOrganisation = useCallback(async (signal?: AbortSignal) => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await get<Organisation>('/api/organisations/current', { signal });
      setOrganisation(data);
    } catch (err) {
      if (err instanceof DOMException && err.name === 'AbortError') return;
      setError(err instanceof Error ? err : new Error('Failed to fetch organisation'));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    fetchOrganisation(controller.signal);
    return () => controller.abort();
  }, [fetchOrganisation]);

  const updateOrganisation = async (request: UpdateOrganisationRequest) => {
    const data = await put<Organisation>('/api/organisations/current', request);
    setOrganisation(data);
    return data;
  };

  return { organisation, isLoading, error, updateOrganisation, refetch: () => fetchOrganisation() };
}
```

**Step 4: TypeScript check**

Run: `cd src/web && npx tsc --noEmit`
Expected: No errors

**Step 5: Commit**

```bash
git add src/web/src/hooks/useParticipants.ts src/web/src/hooks/useMembers.ts src/web/src/hooks/useOrganisation.ts
git commit -m "fix: add AbortController cleanup to all data hooks"
```

---

### Task 4: Add search debounce to participants list

Every keystroke in the search box triggers an API call. Add a 300ms debounce.

**Files:**
- Create: `src/web/src/hooks/useDebounce.ts`
- Modify: `src/web/src/pages/participants/ParticipantsListPage.tsx`

**Step 1: Write test for useDebounce**

Create `src/web/__tests__/use-debounce.test.ts`:

```typescript
import { describe, it, expect, vi } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useDebounce } from '../src/hooks/useDebounce';

describe('useDebounce', () => {
  it('returns initial value immediately', () => {
    const { result } = renderHook(() => useDebounce('hello', 300));
    expect(result.current).toBe('hello');
  });

  it('debounces value changes', async () => {
    vi.useFakeTimers();
    const { result, rerender } = renderHook(
      ({ value }) => useDebounce(value, 300),
      { initialProps: { value: 'a' } },
    );

    rerender({ value: 'ab' });
    expect(result.current).toBe('a'); // not updated yet

    await act(async () => { vi.advanceTimersByTime(300); });
    expect(result.current).toBe('ab'); // now updated

    vi.useRealTimers();
  });
});
```

**Step 2: Run test, verify fail**

Run: `cd src/web && pnpm test`
Expected: FAIL (module not found)

**Step 3: Implement useDebounce**

```typescript
import { useEffect, useState } from 'react';

export function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedValue(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);

  return debouncedValue;
}
```

**Step 4: Update ParticipantsListPage to use debounced search**

In `src/web/src/pages/participants/ParticipantsListPage.tsx`, add:

```typescript
import { useDebounce } from '@/hooks/useDebounce';
```

Replace `useParticipants(page, pageSize, search || undefined)` with:

```typescript
const debouncedSearch = useDebounce(search, 300);
const { participants, totalCount, isLoading } = useParticipants(page, pageSize, debouncedSearch || undefined);
```

**Step 5: Run tests, verify pass**

Run: `cd src/web && pnpm test`
Expected: ALL PASS

**Step 6: Commit**

```bash
git add src/web/src/hooks/useDebounce.ts src/web/__tests__/use-debounce.test.ts src/web/src/pages/participants/ParticipantsListPage.tsx
git commit -m "fix: add 300ms search debounce to participants list"
```

---

### Task 5: Add error display to all data pages

Currently hooks catch errors but pages never show them. Add an error banner component and use it in all pages.

**Files:**
- Create: `src/web/src/components/ErrorBanner.tsx`
- Modify: `src/web/src/pages/participants/ParticipantsListPage.tsx`
- Modify: `src/web/src/pages/MembersPage.tsx`
- Modify: `src/web/src/pages/OrgSettingsPage.tsx`
- Modify: `src/web/src/pages/DashboardPage.tsx`

**Step 1: Create ErrorBanner component**

```tsx
import { AlertCircle } from 'lucide-react';

interface ErrorBannerProps {
  message: string;
  onRetry?: () => void;
}

export function ErrorBanner({ message, onRetry }: ErrorBannerProps) {
  return (
    <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4">
      <div className="flex items-center gap-3">
        <AlertCircle className="h-5 w-5 shrink-0 text-destructive" />
        <div className="flex-1">
          <p className="text-sm font-medium text-destructive">Something went wrong</p>
          <p className="mt-1 text-sm text-muted-foreground">{message}</p>
        </div>
        {onRetry && (
          <button
            onClick={onRetry}
            className="text-sm font-medium text-primary hover:underline"
          >
            Try again
          </button>
        )}
      </div>
    </div>
  );
}
```

**Step 2: Add ErrorBanner to ParticipantsListPage**

After the loading check and before the return, add:

```tsx
import { ErrorBanner } from '@/components/ErrorBanner';
// ...
const { participants, totalCount, isLoading, error, refetch } = useParticipants(...);
// After loading skeleton block:
if (error) {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold tracking-tight">Participants</h1>
      </div>
      <ErrorBanner message={error.message} onRetry={refetch} />
    </div>
  );
}
```

**Step 3: Add ErrorBanner to MembersPage**

Same pattern — destructure `error` and `refetch` from `useMembers()`, add error block after loading.

**Step 4: Add ErrorBanner to OrgSettingsPage**

Destructure `error` and `refetch` from `useOrganisation()`, add error block after loading.

**Step 5: Add ErrorBanner to DashboardPage**

The dashboard currently doesn't fetch data (KPIs are placeholders), but add the import for future use. Also add a `useParticipants` call to populate the "Active Participants" KPI card with real count.

Actually — the dashboard only shows `--` placeholders. For now, just add the ErrorBanner import to dashboard and wire up the participant count. Create this as a separate optional enhancement.

**Step 6: Run tests and type-check**

Run: `cd src/web && npx tsc --noEmit && pnpm test`
Expected: ALL PASS

**Step 7: Commit**

```bash
git add src/web/src/components/ErrorBanner.tsx src/web/src/pages/participants/ParticipantsListPage.tsx src/web/src/pages/MembersPage.tsx src/web/src/pages/OrgSettingsPage.tsx
git commit -m "fix: add error banners with retry to all data pages"
```

---

### Task 6: Wire up real participant count on dashboard

The dashboard KPI cards all show `--`. Wire up the actual participant count from the API.

**Files:**
- Modify: `src/web/src/pages/DashboardPage.tsx`

**Step 1: Import useParticipants and display real count**

Add to dashboard:
```tsx
import { useParticipants } from '@/hooks/useParticipants';
```

Inside the component, fetch page 1 with pageSize 1 to get totalCount cheaply:
```tsx
const { totalCount, isLoading: participantsLoading } = useParticipants(1, 1);
```

Replace the hardcoded `'--'` for Active Participants with:
```tsx
value: participantsLoading ? '...' : String(totalCount),
```

**Step 2: Type-check and test**

Run: `cd src/web && npx tsc --noEmit && pnpm test`

**Step 3: Commit**

```bash
git add src/web/src/pages/DashboardPage.tsx
git commit -m "feat: show real participant count on dashboard KPI card"
```

---

## Part B: Backend Integration Tests

### Task 7: Set up WebApplicationFactory test infrastructure

Add `Microsoft.AspNetCore.Mvc.Testing` and a test factory that uses SQLite in-memory for fast integration tests with the dev auth handler.

**Files:**
- Modify: `src/api/Octocare.Tests/Octocare.Tests.csproj`
- Create: `src/api/Octocare.Tests/Integration/OctocareTestFactory.cs`
- Create: `src/api/Octocare.Tests/Integration/IntegrationTestBase.cs`

**Step 1: Add NuGet packages**

Run:
```bash
cd src/api && dotnet add Octocare.Tests/Octocare.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
cd src/api && dotnet add Octocare.Tests/Octocare.Tests.csproj package Microsoft.EntityFrameworkCore.Sqlite
```

**Step 2: Create OctocareTestFactory**

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Octocare.Infrastructure.Data;

namespace Octocare.Tests.Integration;

public class OctocareTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<OctocareDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Remove Aspire Npgsql registrations
            var npgsqlDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("Npgsql") == true
                         || d.ImplementationType?.FullName?.Contains("Npgsql") == true)
                .ToList();
            foreach (var d in npgsqlDescriptors)
                services.Remove(d);

            // Add SQLite in-memory
            services.AddDbContext<OctocareDbContext>(options =>
            {
                options.UseSqlite("DataSource=:memory:");
            });
        });
    }
}
```

**Step 3: Create IntegrationTestBase**

```csharp
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Octocare.Infrastructure.Data;
using Octocare.Infrastructure.Data.Seeding;

namespace Octocare.Tests.Integration;

public abstract class IntegrationTestBase : IClassFixture<OctocareTestFactory>, IAsyncLifetime
{
    protected readonly OctocareTestFactory Factory;
    protected HttpClient Client = null!;

    protected IntegrationTestBase(OctocareTestFactory factory)
    {
        Factory = factory;
    }

    public async Task InitializeAsync()
    {
        Client = Factory.CreateClient();
        // Dev auth handler auto-authenticates; set header to pick user
        Client.DefaultRequestHeaders.Add("X-Dev-User", "admin");

        // Ensure DB is created and seeded
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OctocareDbContext>();
        await db.Database.EnsureCreatedAsync();
        var seeder = scope.ServiceProvider.GetRequiredService<DevDataSeeder>();
        await seeder.SeedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
```

**Step 4: Verify it builds**

Run: `cd src/api && dotnet build Octocare.slnx`
Expected: Build succeeded

**Step 5: Commit**

```bash
git add src/api/Octocare.Tests/Octocare.Tests.csproj src/api/Octocare.Tests/Integration/
git commit -m "feat: add WebApplicationFactory + SQLite integration test infrastructure"
```

---

### Task 8: Participants controller integration tests

Test the full request pipeline: HTTP → controller → service → EF Core → SQLite.

**Files:**
- Create: `src/api/Octocare.Tests/Integration/ParticipantsControllerTests.cs`

**Step 1: Write integration tests**

```csharp
using System.Net;
using System.Net.Http.Json;
using Octocare.Application.DTOs;

namespace Octocare.Tests.Integration;

public class ParticipantsControllerTests : IntegrationTestBase
{
    public ParticipantsControllerTests(OctocareTestFactory factory) : base(factory) { }

    [Fact]
    public async Task GetParticipants_ReturnsSeededData()
    {
        var response = await Client.GetAsync("/api/participants?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 5); // seeder creates 5
    }

    [Fact]
    public async Task GetParticipants_SearchByName_FiltersResults()
    {
        var response = await Client.GetAsync("/api/participants?page=1&pageSize=20&search=Sarah");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task GetParticipantById_ReturnsParticipant()
    {
        // First get the list to find an ID
        var listResponse = await Client.GetAsync("/api/participants?page=1&pageSize=1");
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResultDto>();
        var firstId = list!.Items[0].GetProperty("id").GetString();

        var response = await Client.GetAsync($"/api/participants/{firstId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetParticipantById_NotFound_Returns404()
    {
        var response = await Client.GetAsync($"/api/participants/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateParticipant_ReturnsCreated()
    {
        var request = new
        {
            ndisNumber = "439999999",
            firstName = "Test",
            lastName = "Participant",
            dateOfBirth = "1990-01-01"
        };

        var response = await Client.PostAsJsonAsync("/api/participants", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var participant = await response.Content.ReadFromJsonAsync<ParticipantDto>();
        Assert.NotNull(participant);
        Assert.Equal("439999999", participant.NdisNumber);
        Assert.Equal("Test", participant.FirstName);
    }

    [Fact]
    public async Task CreateParticipant_DuplicateNdis_Returns409()
    {
        var request = new
        {
            ndisNumber = "431234567", // already seeded
            firstName = "Duplicate",
            lastName = "Test",
            dateOfBirth = "1990-01-01"
        };

        var response = await Client.PostAsJsonAsync("/api/participants", request);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateParticipant_InvalidNdis_Returns400()
    {
        var request = new
        {
            ndisNumber = "12345",
            firstName = "Bad",
            lastName = "Ndis",
            dateOfBirth = "1990-01-01"
        };

        var response = await Client.PostAsJsonAsync("/api/participants", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Helper DTO for deserialization
    private record PagedResultDto(System.Text.Json.JsonElement[] Items, int TotalCount, int Page, int PageSize);
}
```

**Step 2: Run tests**

Run: `cd src/api && dotnet test Octocare.slnx --filter "FullyQualifiedName~ParticipantsController"`
Expected: Tests may need adjustments based on actual DTOs — iterate until passing.

**Step 3: Commit**

```bash
git add src/api/Octocare.Tests/Integration/ParticipantsControllerTests.cs
git commit -m "test: add participants controller integration tests"
```

---

### Task 9: Members and Organisation controller integration tests

**Files:**
- Create: `src/api/Octocare.Tests/Integration/MembersControllerTests.cs`
- Create: `src/api/Octocare.Tests/Integration/OrganisationsControllerTests.cs`

**Step 1: Write Members tests**

```csharp
using System.Net;
using System.Net.Http.Json;

namespace Octocare.Tests.Integration;

public class MembersControllerTests : IntegrationTestBase
{
    public MembersControllerTests(OctocareTestFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMembers_ReturnsSeededMembers()
    {
        var response = await Client.GetAsync("/api/organisations/current/members");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var members = await response.Content.ReadFromJsonAsync<object[]>();
        Assert.NotNull(members);
        Assert.True(members.Length >= 3); // seeder creates 3
    }

    [Fact]
    public async Task InviteMember_ReturnsCreated()
    {
        var request = new
        {
            email = $"new-{Guid.NewGuid():N}@test.com",
            firstName = "New",
            lastName = "Member",
            role = "finance"
        };

        var response = await Client.PostAsJsonAsync("/api/organisations/current/members/invite", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task InviteMember_InvalidRole_Returns400()
    {
        var request = new
        {
            email = "bad@test.com",
            firstName = "Bad",
            lastName = "Role",
            role = "superadmin"
        };

        var response = await Client.PostAsJsonAsync("/api/organisations/current/members/invite", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
```

**Step 2: Write Organisations tests**

```csharp
using System.Net;
using System.Net.Http.Json;

namespace Octocare.Tests.Integration;

public class OrganisationsControllerTests : IntegrationTestBase
{
    public OrganisationsControllerTests(OctocareTestFactory factory) : base(factory) { }

    [Fact]
    public async Task GetCurrent_ReturnsSeededOrg()
    {
        var response = await Client.GetAsync("/api/organisations/current");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCurrent_UpdatesName()
    {
        var request = new
        {
            name = "Updated Org Name",
            abn = "51824753556",
            contactEmail = "admin@acmepm.com.au"
        };

        var response = await Client.PutAsJsonAsync("/api/organisations/current", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCurrent_InvalidAbn_Returns400()
    {
        var request = new
        {
            name = "Test Org",
            abn = "12345"
        };

        var response = await Client.PutAsJsonAsync("/api/organisations/current", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
```

**Step 3: Run all integration tests**

Run: `cd src/api && dotnet test Octocare.slnx --filter "FullyQualifiedName~Integration"`
Expected: ALL PASS (may need iteration)

**Step 4: Commit**

```bash
git add src/api/Octocare.Tests/Integration/MembersControllerTests.cs src/api/Octocare.Tests/Integration/OrganisationsControllerTests.cs
git commit -m "test: add members and organisations controller integration tests"
```

---

## Part C: Frontend Component Tests

### Task 10: Set up vitest with jsdom and testing-library

**Files:**
- Modify: `src/web/vite.config.ts` (add test config)
- Create: `src/web/src/test-utils.tsx` (render wrapper with providers)

**Step 1: Add vitest test configuration**

Update `vite.config.ts`:

```typescript
import path from 'path'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test-setup.ts'],
    globals: true,
  },
})
```

**Step 2: Create test setup file**

Create `src/web/src/test-setup.ts`:

```typescript
import '@testing-library/jest-dom/vitest';
```

**Step 3: Create test render utility**

Create `src/web/src/test-utils.tsx`:

```tsx
import { render, type RenderOptions } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { DevAuthProvider } from '@/providers/DevAuthProvider';
import type { ReactElement } from 'react';

function AllProviders({ children }: { children: React.ReactNode }) {
  return (
    <DevAuthProvider>
      <MemoryRouter>{children}</MemoryRouter>
    </DevAuthProvider>
  );
}

export function renderWithProviders(
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>,
) {
  return render(ui, { wrapper: AllProviders, ...options });
}

export { screen, waitFor, within } from '@testing-library/react';
export { default as userEvent } from '@testing-library/user-event';
```

**Step 4: Install missing dev dependency if needed**

Run: `cd src/web && pnpm add -D @testing-library/user-event`

**Step 5: Run existing tests to verify setup doesn't break them**

Run: `cd src/web && pnpm test`
Expected: ALL PASS

**Step 6: Commit**

```bash
git add src/web/vite.config.ts src/web/src/test-setup.ts src/web/src/test-utils.tsx src/web/package.json src/web/pnpm-lock.yaml
git commit -m "feat: configure vitest with jsdom, testing-library, and render helpers"
```

---

### Task 11: Component tests for ParticipantsListPage

Test rendering, empty state, error state, and search interaction.

**Files:**
- Create: `src/web/__tests__/participants-list.test.tsx`

**Step 1: Write component tests**

```tsx
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen, waitFor } from '../src/test-utils';
import { ParticipantsListPage } from '../src/pages/participants/ParticipantsListPage';

const mockFetch = vi.fn();
vi.stubGlobal('fetch', mockFetch);

beforeEach(() => {
  vi.resetAllMocks();
});

function mockParticipantsResponse(items: object[], totalCount?: number) {
  mockFetch.mockResolvedValue({
    ok: true,
    status: 200,
    json: () => Promise.resolve({
      items,
      totalCount: totalCount ?? items.length,
      page: 1,
      pageSize: 20,
    }),
  });
}

describe('ParticipantsListPage', () => {
  it('shows empty state when no participants exist', async () => {
    mockParticipantsResponse([]);

    renderWithProviders(<ParticipantsListPage />);

    await waitFor(() => {
      expect(screen.getByText('No participants yet')).toBeInTheDocument();
    });
  });

  it('renders participant rows when data is returned', async () => {
    mockParticipantsResponse([
      {
        id: '1',
        ndisNumber: '431234567',
        firstName: 'Sarah',
        lastName: 'Johnson',
        dateOfBirth: '1985-03-15',
        isActive: true,
      },
    ]);

    renderWithProviders(<ParticipantsListPage />);

    await waitFor(() => {
      expect(screen.getByText('Sarah Johnson')).toBeInTheDocument();
      expect(screen.getByText('431234567')).toBeInTheDocument();
    });
  });

  it('shows error banner when fetch fails', async () => {
    mockFetch.mockResolvedValue({
      ok: false,
      status: 500,
      statusText: 'Internal Server Error',
      json: () => Promise.resolve({ detail: 'Server error' }),
    });

    renderWithProviders(<ParticipantsListPage />);

    await waitFor(() => {
      expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    });
  });

  it('renders Add Participant button', async () => {
    mockParticipantsResponse([]);
    renderWithProviders(<ParticipantsListPage />);

    await waitFor(() => {
      expect(screen.getByRole('link', { name: /add participant/i })).toBeInTheDocument();
    });
  });
});
```

**Step 2: Run tests**

Run: `cd src/web && pnpm test`
Expected: ALL PASS (may need iteration)

**Step 3: Commit**

```bash
git add src/web/__tests__/participants-list.test.tsx
git commit -m "test: add ParticipantsListPage component tests"
```

---

### Task 12: Component tests for MembersPage and OrgSettingsPage

**Files:**
- Create: `src/web/__tests__/members-page.test.tsx`
- Create: `src/web/__tests__/org-settings.test.tsx`

**Step 1: Write MembersPage tests**

```tsx
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen, waitFor } from '../src/test-utils';
import { MembersPage } from '../src/pages/MembersPage';

const mockFetch = vi.fn();
vi.stubGlobal('fetch', mockFetch);

beforeEach(() => { vi.resetAllMocks(); });

describe('MembersPage', () => {
  it('shows empty state when no members exist', async () => {
    mockFetch.mockResolvedValue({
      ok: true, status: 200,
      json: () => Promise.resolve([]),
    });

    renderWithProviders(<MembersPage />);

    await waitFor(() => {
      expect(screen.getByText('No team members yet')).toBeInTheDocument();
    });
  });

  it('renders member rows when data returned', async () => {
    mockFetch.mockResolvedValue({
      ok: true, status: 200,
      json: () => Promise.resolve([
        { userId: '1', email: 'admin@test.com', firstName: 'Admin', lastName: 'User', role: 'org_admin', isActive: true, joinedAt: '2024-01-01' },
      ]),
    });

    renderWithProviders(<MembersPage />);

    await waitFor(() => {
      expect(screen.getByText('Admin User')).toBeInTheDocument();
    });
  });
});
```

**Step 2: Write OrgSettingsPage tests**

```tsx
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen, waitFor } from '../src/test-utils';
import { OrgSettingsPage } from '../src/pages/OrgSettingsPage';

const mockFetch = vi.fn();
vi.stubGlobal('fetch', mockFetch);

beforeEach(() => { vi.resetAllMocks(); });

describe('OrgSettingsPage', () => {
  it('renders org details form', async () => {
    mockFetch.mockResolvedValue({
      ok: true, status: 200,
      json: () => Promise.resolve({
        id: '1', name: 'Acme PM', abn: '51824753556',
        contactEmail: 'admin@acme.com', isActive: true, createdAt: '2024-01-01',
      }),
    });

    renderWithProviders(<OrgSettingsPage />);

    await waitFor(() => {
      expect(screen.getByLabelText(/organisation name/i)).toHaveValue('Acme PM');
    });
  });

  it('shows error when fetch fails', async () => {
    mockFetch.mockResolvedValue({
      ok: false, status: 500, statusText: 'Error',
      json: () => Promise.resolve({ detail: 'Failed' }),
    });

    renderWithProviders(<OrgSettingsPage />);

    await waitFor(() => {
      expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    });
  });
});
```

**Step 3: Run all tests**

Run: `cd src/web && pnpm test`
Expected: ALL PASS

**Step 4: Commit**

```bash
git add src/web/__tests__/members-page.test.tsx src/web/__tests__/org-settings.test.tsx
git commit -m "test: add MembersPage and OrgSettingsPage component tests"
```

---

### Task 13: Final verification

**Step 1: Run all frontend tests**

Run: `cd src/web && pnpm test`
Expected: ALL PASS

**Step 2: Run all backend tests**

Run: `cd src/api && dotnet test Octocare.slnx`
Expected: ALL PASS

**Step 3: TypeScript check and build**

Run: `cd src/web && pnpm build`
Expected: Build success

**Step 4: Final commit with any remaining fixups**

```bash
git add -A
git commit -m "chore: final test and build verification"
```
