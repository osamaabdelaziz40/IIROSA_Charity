/**
 * HQ Financial Transfers models (epic 17, UC-TRF-01…08)
 * The API returns raw DTOs — no {success, data} envelope; camelCase wire contract.
 */

/** Grid row of §22.S.1 — the 11 data columns (serial is computed client-side) */
export interface HqTransfer {
  id: string;
  countryName?: string | null;
  departmentName?: string | null;
  operationNumber: string;
  finYear: string;
  paymentNumber: number;
  dateFrom: string;
  dateTo: string;
  amountOfPayment: number;
  statement?: string | null;
  beneficiariesNumber: number;
  transactionNumber: string;
  transactionDate: string;
}

/** Paged list response as the API serializes it */
export interface HqTransferListResponse {
  items: HqTransfer[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/** §22.S.2 create payload — clean wire keys (the entity's FK_* columns are bridged server-side) */
export interface CreateHqTransferRequest {
  countryId: number;
  departmentId: number;
  operationNumber: string;
  finYear: string;
  paymentNumber: number;
  dateFrom: string;          // yyyy-MM-dd
  dateTo: string;            // yyyy-MM-dd
  amountOfPayment: number;
  statement?: string | null;
  beneficiariesNumber: number;
  transactionNumber: string;
  transactionDate: string;   // yyyy-MM-dd
}

/** Detail DTO — the create echo now; 17-3's GET {id} returns the same shape */
export interface HqTransferDetail {  id: string;
  countryId: number;
  countryName?: string | null;
  departmentId: number;
  departmentName?: string | null;
  operationNumber: string;
  finYear: string;
  paymentNumber: number;
  dateFrom: string;
  dateTo: string;
  amountOfPayment: number;
  statement?: string | null;
  beneficiariesNumber: number;
  transactionNumber: string;
  transactionDate: string;
  createdOn: string;
  createdBy?: string | null;
  updatedOn?: string | null;
  updatedBy?: string | null;
}

/** UC-TRF-04 payload — id in the body (board contract: PUT /api/HqTransfers) */
export interface UpdateHqTransferRequest extends CreateHqTransferRequest {
  id: string;
}

/** UC-TRF-06 — per-country transfer ceiling; maxTransferAmount NULL = unlimited */
export interface CountryMaxTransferAmount {
  countryId: number;
  countryName: string;
  maxTransferAmount: number | null;
}

/** UC-TRF-07 payload — maxTransferAmount null clears the ceiling (unlimited) */
export interface UpdateCountryMaxTransferRequest {
  countryId: number;
  maxTransferAmount: number | null;
}

/** UC-TRF-08 — one §22.S.3 allocation line (grid row read shape) */
export interface HqTransferDetailLine {
  id: string;
  transferNumber: string;
  amount: number;
  estimatedTransferDate?: string | null;
  isExecuted?: boolean | null;
  executionDate?: string | null;
  arrivalDate?: string | null;
  arrivalAmount?: number | null;
}

/** UC-TRF-08 — the details screen read: header summary + lines */
export interface HqTransferDetails {
  transferId: string;
  operationNumber: string;
  amountOfPayment: number;
  countryName?: string | null;
  lines: HqTransferDetailLine[];
}

/** UC-TRF-08 payload — id null = new line; execution/arrival data only once executed */
export interface SaveHqTransferDetailLineRequest {
  id: string | null;
  transferNumber: string;
  amount: number;
  estimatedTransferDate?: string | null;
  isExecuted?: boolean | null;
  executionDate?: string | null;
  arrivalDate?: string | null;
  arrivalAmount?: number | null;
}

/** Register statistics band above the §22.S.1 grid — caller-scoped server-side (country claim) */
export interface HqTransferStatistics {
  total: number;
  totalAmount: number;
  addedThisMonth: number;
}
