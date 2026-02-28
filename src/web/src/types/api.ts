export interface Organisation {
  id: string;
  name: string;
  abn?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
  isActive: boolean;
  createdAt: string;
}

export interface UpdateOrganisationRequest {
  name: string;
  abn?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
}

export interface Member {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'org_admin' | 'plan_manager' | 'finance';
  isActive: boolean;
  joinedAt: string;
}

export interface InviteMemberRequest {
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface UpdateMemberRoleRequest {
  role: string;
}

export interface Participant {
  id: string;
  ndisNumber: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email?: string;
  phone?: string;
  address?: string;
  nomineeName?: string;
  nomineeEmail?: string;
  nomineePhone?: string;
  nomineeRelationship?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateParticipantRequest {
  ndisNumber: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email?: string;
  phone?: string;
  address?: string;
  nomineeName?: string;
  nomineeEmail?: string;
  nomineePhone?: string;
  nomineeRelationship?: string;
}

export interface UpdateParticipantRequest {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email?: string;
  phone?: string;
  address?: string;
  nomineeName?: string;
  nomineeEmail?: string;
  nomineePhone?: string;
  nomineeRelationship?: string;
}

export interface Provider {
  id: string;
  name: string;
  abn?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateProviderRequest {
  name: string;
  abn?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
}

export interface UpdateProviderRequest {
  name: string;
  abn?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
}

export type SupportCategory = 'Core' | 'CapacityBuilding' | 'Capital';
export type UnitOfMeasure = 'Hour' | 'Each' | 'Day' | 'Week' | 'Month' | 'Year';

export interface PriceGuideVersion {
  id: string;
  name: string;
  effectiveFrom: string;
  effectiveTo: string;
  isCurrent: boolean;
}

export interface SupportItem {
  id: string;
  itemNumber: string;
  name: string;
  supportCategory: SupportCategory;
  supportPurpose: string;
  unit: UnitOfMeasure;
  priceLimitNational: number;
  priceLimitRemote: number;
  priceLimitVeryRemote: number;
  isTtpEligible: boolean;
  cancellationRule: string;
  claimType: string;
}

export type PlanStatus = 'draft' | 'active' | 'expiring' | 'expired' | 'transitioned';

export interface Plan {
  id: string;
  participantId: string;
  participantName: string;
  planNumber: string;
  startDate: string;
  endDate: string;
  status: PlanStatus;
  budgetCategories: BudgetCategory[];
  createdAt: string;
}

export interface BudgetCategory {
  id: string;
  supportCategory: SupportCategory;
  supportPurpose: string;
  allocatedAmount: number;
}

export interface CreatePlanRequest {
  planNumber: string;
  startDate: string;
  endDate: string;
}

export interface UpdatePlanRequest {
  planNumber: string;
  startDate: string;
  endDate: string;
}

export interface CreateBudgetCategoryRequest {
  supportCategory: SupportCategory;
  supportPurpose: string;
  allocatedAmount: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
