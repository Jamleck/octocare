import { useCallback, useEffect, useState } from 'react';
import { get, post, put } from '@/lib/api-client';
import type { Member, InviteMemberRequest, UpdateMemberRoleRequest } from '@/types/api';

export function useMembers() {
  const [members, setMembers] = useState<Member[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchMembers = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await get<Member[]>('/api/organisations/current/members');
      setMembers(data);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch members'));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchMembers();
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

  return { members, isLoading, error, inviteMember, updateRole, deactivateMember, refetch: fetchMembers };
}
