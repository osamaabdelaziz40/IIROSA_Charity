import { User } from "../../../core/models";

/**
 * Currency Enum
 */
export enum Currency {
  EGP = 'EGP',
  SAR = 'SAR',
  USD = 'USD'
}

/**
 * Beneficiary Type Enum
 */
export enum BeneficiaryType {
  Individual = 'Individual',
  Company = 'Company',
  Charity = 'Charity',
  Supplier = 'Supplier',
  Employee = 'Employee'
}

/**
 * Check Status Enum
 */
export enum CheckStatus {
  Pending = 'Pending',
  Issued = 'Issued',
  Cleared = 'Cleared',
  Void = 'Void'
}

/**
 * Payment Reason Enum
 */
export enum PaymentReason {
  Salary = 'Salary',
  SupplierRefund = 'SupplierRefund',
  Expense = 'Expense',
  Other = 'Other'
}

/**
 * Void Reason Enum
 */
export enum VoidReason {
  Lost = 'Lost',
  Stopped = 'Stopped',
  Error = 'Error',
  Expired = 'Expired',
  Other = 'Other'
}

/**
 * Check Beneficiary
 */
export interface CheckBeneficiary {
  id?: number;
  beneficiaryType: BeneficiaryType;
  beneficiaryName: string;
  idNumber?: string;
  address?: string;
  phone?: string;
  email?: string;
  isActive: boolean;
  createdAt?: Date;
  createdBy?: string;
}

/**
 * Bank
 */
export interface Bank {
  id: number;
  bankName: string;
  bankNameAr?: string;
  bankNameEn?: string;
  branch?: string;
  accountNumber?: string;
  isActive: boolean;
}

/**
 * Check
 */
export interface Check {
  id?: number;

  // Check Information
  checkNumber: string;
  checkDate: Date;
  dueDate?: Date;
  currency: Currency;

  // Beneficiary Information
  beneficiaryType: BeneficiaryType;
  beneficiaryName: string;
  beneficiaryId?: number;
  beneficiaryAddress?: string;
  beneficiaryPhone?: string;
  beneficiaryEmail?: string;
  idNumber?: string;

  // Financial Information
  amount: number;
  amountInWords?: string;
  paymentReason: PaymentReason;
  paymentDescription?: string;

  // Bank Information
  bankId: number;
  bankName?: string;
  branch?: string;
  accountNumber?: string;

  // Status Information
  checkStatus: CheckStatus;
  issueDate?: Date;
  clearanceDate?: Date;
  bankReference?: string;
  clearanceNotes?: string;
  voidDate?: Date;
  voidReason?: VoidReason;
  voidNotes?: string;

  // Approval
  requiresApproval: boolean;
  approvedBy?: string;

  // Check Image
  checkImageId?: number;
  checkImageUrl?: string;

  // Audit
  createdAt?: Date;
  createdBy?: string;
  creator?: User;
  modifiedAt?: Date;
  modifiedBy?: string;
  modifier?: User;
}

/**
 * Check List Item
 */
export interface CheckListItem {
  id: number;
  checkNumber: string;
  checkDate: Date;
  dueDate?: Date;
  beneficiaryName: string;
  amount: number;
  currency: Currency;
  checkStatus: CheckStatus;
  bankName: string;
  createdBy: string;
  createdAt: Date;
}

/**
 * Check Filter
 */
export interface CheckFilter {
  searchTerm?: string;
  checkStatus?: CheckStatus;
  bankId?: number;
  currency?: Currency;
  dateFrom?: Date;
  dateTo?: Date;
  amountFrom?: number;
  amountTo?: number;
}

/**
 * Create/Update Check DTO
 */
export interface CheckDto {
  id?: number;

  // Check Information
  checkNumber: string;
  checkDate: Date;
  dueDate?: Date;
  currency: Currency;

  // Beneficiary Information
  beneficiaryType: BeneficiaryType;
  beneficiaryName: string;
  beneficiaryId?: number;
  beneficiaryAddress?: string;
  beneficiaryPhone?: string;
  beneficiaryEmail?: string;
  idNumber?: string;

  // Financial Information
  amount: number;
  amountInWords?: string;
  paymentReason: PaymentReason;
  paymentDescription?: string;

  // Bank Information
  bankId: number;
  branch?: string;
  accountNumber?: string;

  // Status Information
  checkStatus: CheckStatus;
  issueDate?: Date;

  // Approval
  requiresApproval: boolean;

  // Check Image (file ID)
  checkImageId?: number;
}

/**
 * Clearance Request
 */
export interface CheckClearanceRequest {
  clearanceDate: Date;
  bankReference?: string;
  clearanceNotes?: string;
}

/**
 * Void Request
 */
export interface CheckVoidRequest {
  voidReason: VoidReason;
  voidDate: Date;
  voidNotes: string;
}

/**
 * Reconciliation Item
 */
export interface ReconciliationItem {
  checkId: number;
  checkNumber: string;
  amount: number;
  currency: Currency;
  checkDate: Date;
  beneficiaryName: string;
  bankName: string;
  isReconciled: boolean;
  bankReference?: string;
  clearanceDate?: Date;
}

/**
 * Reconciliation Summary
 */
export interface ReconciliationSummary {
  totalChecks: number;
  reconciledChecks: number;
  unreconciledChecks: number;
  totalAmount: number;
  reconciledAmount: number;
  unreconciledAmount: number;
  items: ReconciliationItem[];
}

/**
 * Check Report Options
 */
export interface CheckReportOptions {
  dateFrom: Date;
  dateTo: Date;
  checkStatus?: CheckStatus;
  bankId?: number;
  currency?: Currency;
  groupBy?: 'Status' | 'Bank' | 'Beneficiary';
}

/**
 * Check Report
 */
export interface CheckReport {
  summaryStatistics: {
    totalChecks: number;
    totalAmount: number;
    byStatus: { [key: string]: { count: number; amount: number } };
    byBank: { [key: string]: { count: number; amount: number } };
    byCurrency: { [key: string]: { count: number; amount: number } };
  };
  detailedCheckList: CheckListItem[];
  clearedChecks: CheckListItem[];
  pendingChecks: CheckListItem[];
  voidChecks: CheckListItem[];
}

/**
 * Validation Errors
 */
export interface CheckValidationError {
  checkNumber?: string;
  checkDate?: string;
  beneficiaryName?: string;
  amount?: string;
  currency?: string;
  bankId?: string;
  voidNotes?: string;
}
