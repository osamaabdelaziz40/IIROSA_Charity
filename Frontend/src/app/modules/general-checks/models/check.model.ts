/**
 * General cheques wire models (chapter 16, UC-CHQ-01..10).
 *
 * These map 1:1 onto the backend DTOs in IIROSA.Application/DTOs/CheckManagement —
 * camelCase on the wire, Guid ids as strings, the FK_ prefix kept server-side.
 */

/** §16.S.1 / §16.S.3 — register and statement grid row. */
export interface CheckListItem {
  id: string;
  checkNumber: string;
  checkDate: string;
  beneficiaryName: string;
  amount: number;
  currency: string;
  chequeType: 'Orphans' | 'Individuals';
  bankName: string | null;
  charityId: string | null;
  isDamaged: boolean;
  isReturned: boolean;
  isDispensed: boolean;
  isDone: boolean;
  comment: string | null;
}

/** §16.S.2 — full cheque record for the view/edit screens. */
export interface CheckDetail {
  id: string;
  checkNumber: string;
  checkDate: string;
  currency: string;
  chequeType: 'Orphans' | 'Individuals';

  chequeBeneficiaryId: number | null;
  beneficiaryType: string | null;
  beneficiaryName: string;
  beneficiaryAddress: string | null;
  beneficiaryPhone: string | null;
  beneficiaryEmail: string | null;
  beneficiaryIdNumber: string | null;

  amount: number;
  amountInWords: string | null;

  bankId: number | null;
  bankName: string | null;
  bankBranch: string | null;
  accountNumber: string | null;

  charityId: string | null;
  charityName: string | null;
  isDamaged: boolean;
  isReturned: boolean;
  isDispensed: boolean;
  isDone: boolean;
  comment: string | null;

  createdOn: string;
  createdBy: string | null;
  updatedOn: string | null;
  updatedBy: string | null;
}

/** UC-CHQ-02 — issue a cheque. Mandatory: bank, beneficiaryName, checkDate, checkNumber, currency, amount. */
export interface CreateCheckRequest {
  bankId: number;
  beneficiaryName: string;
  checkDate: string;
  checkNumber: string;
  currency: string;
  amount: number;

  charityId?: string | null;
  chequeBeneficiaryId?: number | null;
  beneficiaryType?: string | null;
  beneficiaryAddress?: string | null;
  beneficiaryPhone?: string | null;
  beneficiaryEmail?: string | null;
  beneficiaryIdNumber?: string | null;
  bankBranch?: string | null;
  accountNumber?: string | null;
  amountInWords?: string | null;
  chequeType: 'Orphans' | 'Individuals';
  isDamaged: boolean;
  isReturned: boolean;
  isDispensed: boolean;
  isDone: boolean;
  comment?: string | null;
}

/** UC-CHQ-04 — update a cheque; the id travels in the body (legacy PUT /api/CheckManagement contract). */
export interface UpdateCheckRequest extends CreateCheckRequest {
  id: string;
}

/** Register/statement filter — camelCase names match the backend CheckFilterDto query params. */
export interface CheckFilter {
  charityId?: string;
  bankId?: number;
  dateFrom?: string;
  dateTo?: string;
  chequeType?: 'Orphans' | 'Individuals';
  searchText?: string;
  page: number;
  pageSize: number;
}

export interface CheckPagedResult {
  items: CheckListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/** UC-CHQ-07 — GET amount-in-words response. */
export interface AmountInWordsResponse {
  amount: number;
  currency: string;
  words: string;
}

/** UC-CHQ-09 — cheque statement بيان الشيكات. */
export interface CheckStatement {
  items: CheckListItem[];
  totalCount: number;
  totalPages: number;
  totalByCurrency: Record<string, number>;
  generatedOn: string;
}

/** UC-CHQ-05 — cheque beneficiary type-ahead row. */
export interface ChequeBeneficiaryOption {
  id: number;
  name: string;
  nameAr: string | null;
  nameEn: string | null;
  beneficiaryType: string | null;
  address: string | null;
  phone: string | null;
  email: string | null;
  idNumber: string | null;
  bankId: number | null;
  accountNumber: string | null;
}

/** UC-CHQ-06 — currency dropdown row. */
export interface CurrencyOption {
  code: string;
  nameAr: string | null;
  nameEn: string | null;
}

/** UC-CHQ-08 — bank cheque stationery offsets, mm from the leaf's top-right. */
export interface BankChequePositions {
  bankId: number;
  bankName: string;
  configured: boolean;
  dateX: number | null;
  dateY: number | null;
  payeeX: number | null;
  payeeY: number | null;
  amountX: number | null;
  amountY: number | null;
  amountWordsX: number | null;
  amountWordsY: number | null;
}
