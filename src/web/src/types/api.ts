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

// Claim types
export type ClaimStatus = 'draft' | 'submitted' | 'accepted' | 'partially_rejected' | 'rejected';
export type ClaimLineItemStatus = 'pending' | 'accepted' | 'rejected';

export interface Claim {
  id: string;
  batchNumber: string;
  status: ClaimStatus;
  totalAmount: number;
  ndiaReference?: string;
  submissionDate?: string;
  responseDate?: string;
  lineItems: ClaimLineItemDetail[];
  createdAt: string;
}

export interface ClaimLineItemDetail {
  id: string;
  invoiceLineItemId: string;
  supportItemNumber: string;
  description: string;
  serviceDate: string;
  quantity: number;
  rate: number;
  amount: number;
  invoiceNumber: string;
  providerName: string;
  participantName: string;
  status: ClaimLineItemStatus;
  rejectionReason?: string;
}

export interface CreateClaimRequest {
  invoiceLineItemIds: string[];
}

export interface RecordClaimOutcomeRequest {
  ndiaReference?: string;
  lineItems: ClaimLineItemOutcome[];
}

export interface ClaimLineItemOutcome {
  lineItemId: string;
  status: 'accepted' | 'rejected';
  rejectionReason?: string;
}

export interface ClaimPagedResult {
  items: Claim[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Budget Overview types
export interface BudgetOverview {
  planId: string;
  planNumber: string;
  totalAllocated: number;
  totalCommitted: number;
  totalSpent: number;
  totalPending: number;
  totalAvailable: number;
  utilisationPercentage: number;
  categories: BudgetCategoryProjection[];
}

export interface BudgetCategoryProjection {
  categoryId: string;
  supportCategory: string;
  supportPurpose: string;
  allocated: number;
  committed: number;
  spent: number;
  pending: number;
  available: number;
  utilisationPercentage: number;
}

// Payment Batch types
export type PaymentBatchStatus = 'draft' | 'generated' | 'sent' | 'confirmed';

export interface PaymentBatch {
  id: string;
  batchNumber: string;
  status: PaymentBatchStatus;
  totalAmount: number;
  itemCount: number;
  abaFileUrl?: string;
  createdAt: string;
  sentAt?: string;
  confirmedAt?: string;
}

export interface PaymentItem {
  id: string;
  providerId: string;
  providerName: string;
  amount: number;
  invoiceCount: number;
  invoiceIds: string;
}

export interface PaymentBatchDetail extends PaymentBatch {
  items: PaymentItem[];
}

export interface PaymentBatchPagedResult {
  items: PaymentBatch[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Budget Alert types
export type AlertType =
  | 'BudgetThreshold75'
  | 'BudgetThreshold90'
  | 'ProjectedOverspend'
  | 'ProjectedUnderspend'
  | 'PlanExpiry90Days'
  | 'PlanExpiry60Days'
  | 'PlanExpiry30Days'
  | 'ServiceGap';

export type AlertSeverity = 'Info' | 'Warning' | 'Critical';

export interface BudgetAlert {
  id: string;
  planId: string;
  budgetCategoryId?: string;
  alertType: AlertType;
  severity: AlertSeverity;
  message: string;
  isRead: boolean;
  isDismissed: boolean;
  createdAt: string;
  readAt?: string;
  data?: string;
}

export interface AlertSummary {
  total: number;
  unreadInfo: number;
  unreadWarning: number;
  unreadCritical: number;
}

// Plan Transition types
export type PlanTransitionStatus = 'Pending' | 'InProgress' | 'Completed';

export interface TransitionChecklistItem {
  label: string;
  completed: boolean;
}

export interface PlanTransition {
  id: string;
  oldPlanId: string;
  oldPlanNumber: string;
  newPlanId?: string;
  newPlanNumber?: string;
  status: PlanTransitionStatus;
  checklistItems: TransitionChecklistItem[];
  notes?: string;
  createdAt: string;
  completedAt?: string;
}

export interface InitiateTransitionRequest {
  oldPlanId: string;
}

export interface UpdateTransitionRequest {
  checklistItems: TransitionChecklistItem[];
  notes?: string;
}

// PRODA/PACE Integration types
export interface ProdaPlanInfo {
  planNumber: string;
  status: string;
  startDate: string;
  endDate: string;
  totalBudget: number;
}

export interface ProdaParticipantInfo {
  ndisNumber: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  phone?: string;
  email?: string;
}

export interface ProdaBudgetInfo {
  planNumber: string;
  categories: ProdaBudgetLine[];
}

export interface ProdaBudgetLine {
  category: string;
  purpose: string;
  allocated: number;
  used: number;
  available: number;
}

export interface SyncResult {
  inSync: boolean;
  discrepancies: SyncDiscrepancy[];
}

export interface SyncDiscrepancy {
  field: string;
  localValue: string;
  prodaValue: string;
  severity: string;
}

// Report types
export interface BudgetUtilisationReportRow {
  participantName: string;
  ndisNumber: string;
  planNumber: string;
  category: string;
  purpose: string;
  allocated: number;
  spent: number;
  available: number;
  utilisationPercent: number;
}

export interface OutstandingInvoiceRow {
  invoiceNumber: string;
  providerName: string;
  participantName: string;
  servicePeriodEnd: string;
  amount: number;
  status: string;
  daysOutstanding: number;
  ageBucket: string;
}

export interface ClaimStatusRow {
  batchNumber: string;
  status: string;
  totalAmount: number;
  lineItemCount: number;
  acceptedCount: number;
  rejectedCount: number;
  submissionDate?: string;
}

export interface ParticipantSummaryRow {
  name: string;
  ndisNumber: string;
  isActive: boolean;
  activePlanNumber?: string;
  planEnd?: string;
  totalAllocated: number;
  totalSpent: number;
  utilisationPercent: number;
}

export interface AuditTrailRow {
  timestamp: string;
  streamType: string;
  eventType: string;
  streamId: string;
  details: string;
}

// Participant Statement types
export interface ParticipantStatement {
  id: string;
  participantId: string;
  planId: string;
  periodStart: string;
  periodEnd: string;
  generatedAt: string;
  sentAt?: string;
}

export interface GenerateStatementRequest {
  planId: string;
  periodStart: string;
  periodEnd: string;
}

// Notification types
export type NotificationType =
  | 'InvoiceSubmitted'
  | 'InvoiceApproved'
  | 'InvoiceRejected'
  | 'PlanExpiring'
  | 'BudgetAlert'
  | 'ClaimSubmitted'
  | 'ClaimOutcome'
  | 'StatementGenerated'
  | 'General';

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  link?: string;
  createdAt: string;
  readAt?: string;
}

export interface NotificationPagedResult {
  items: Notification[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface UnreadCount {
  count: number;
}

// Email Template types
export interface EmailTemplate {
  id: string;
  name: string;
  subject: string;
  body: string;
  isActive: boolean;
  updatedAt: string;
}

export interface UpdateEmailTemplateRequest {
  subject: string;
  body: string;
}

export interface EmailTemplatePreview {
  subject: string;
  body: string;
}

// Communication Log types
export interface CommunicationLogEntry {
  id: string;
  recipientEmail: string;
  subject: string;
  templateName?: string;
  sentAt: string;
  status: string;
  errorMessage?: string;
  relatedEntityType?: string;
  relatedEntityId?: string;
}

export interface CommunicationLogPagedResult {
  items: CommunicationLogEntry[];
  totalCount: number;
  page: number;
  pageSize: number;
}
