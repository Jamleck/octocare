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

// Service Agreement types
export type ServiceAgreementStatus = 'draft' | 'sent' | 'active' | 'expired' | 'terminated';
export type ServiceBookingStatus = 'active' | 'completed' | 'cancelled';

export interface ServiceAgreement {
  id: string;
  participantId: string;
  participantName: string;
  providerId: string;
  providerName: string;
  planId: string;
  planNumber: string;
  status: ServiceAgreementStatus;
  startDate: string;
  endDate: string;
  signedDocumentUrl?: string;
  items: ServiceAgreementItem[];
  bookings: ServiceBooking[];
  createdAt: string;
}

export interface ServiceAgreementItem {
  id: string;
  supportItemNumber: string;
  agreedRate: number;
  frequency?: string;
}

export interface ServiceBooking {
  id: string;
  budgetCategoryId: string;
  supportCategory: string;
  allocatedAmount: number;
  usedAmount: number;
  status: ServiceBookingStatus;
}

export interface CreateServiceAgreementRequest {
  providerId: string;
  planId: string;
  startDate: string;
  endDate: string;
  items: CreateServiceAgreementItemRequest[];
}

export interface CreateServiceAgreementItemRequest {
  supportItemNumber: string;
  agreedRate: number;
  frequency?: string;
}

export interface CreateServiceBookingRequest {
  budgetCategoryId: string;
  allocatedAmount: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Invoice types
export type InvoiceStatus = 'submitted' | 'under_review' | 'approved' | 'rejected' | 'disputed' | 'paid';

export interface Invoice {
  id: string;
  providerId: string;
  providerName: string;
  participantId: string;
  participantName: string;
  planId: string;
  planNumber: string;
  invoiceNumber: string;
  servicePeriodStart: string;
  servicePeriodEnd: string;
  totalAmount: number;
  status: InvoiceStatus;
  source: string;
  notes?: string;
  lineItems: InvoiceLineItem[];
  createdAt: string;
}

export interface InvoiceLineItem {
  id: string;
  supportItemNumber: string;
  description: string;
  serviceDate: string;
  quantity: number;
  rate: number;
  amount: number;
  budgetCategoryId?: string;
  supportCategory?: string;
  validationStatus: string;
  validationMessage?: string;
}

export interface CreateInvoiceRequest {
  providerId: string;
  participantId: string;
  planId: string;
  invoiceNumber: string;
  servicePeriodStart: string;
  servicePeriodEnd: string;
  notes?: string;
  lineItems: CreateInvoiceLineItemRequest[];
}

export interface CreateInvoiceLineItemRequest {
  supportItemNumber: string;
  description: string;
  serviceDate: string;
  quantity: number;
  rate: number;
  budgetCategoryId?: string;
}

export interface InvoicePagedResult {
  items: Invoice[];
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
