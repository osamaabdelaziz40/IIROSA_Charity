import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  Check,
  CheckDto,
  CheckListItem,
  CheckFilter,
  CheckClearanceRequest,
  CheckVoidRequest,
  ReconciliationSummary,
  CheckReportOptions,
  CheckReport,
  CheckBeneficiary,
  Bank,
  Currency
} from '../models/check.model';
import { ApiService } from '../../../core/services/api.service';

@Injectable({
  providedIn: 'root'
})
export class GeneralChecksService {
  private endpoint = '/api/GeneralChecks';

  constructor(private apiService: ApiService) {}

  /**
   * Get all checks with optional filtering
   */
  getAllChecks(filter?: CheckFilter): Observable<CheckListItem[]> {
    return this.apiService.get<CheckListItem[]>(this.endpoint, filter);
  }

  /**
   * Get check by ID
   */
  getCheckById(id: number): Observable<Check> {
    return this.apiService.get<Check>(`${this.endpoint}/${id}`);
  }

  /**
   * Create new check
   */
  createCheck(check: CheckDto): Observable<Check> {
    return this.apiService.post<Check>(this.endpoint, check);
  }

  /**
   * Update check
   */
  updateCheck(id: number, check: CheckDto): Observable<Check> {
    return this.apiService.put<Check>(`${this.endpoint}/${id}`, check);
  }

  /**
   * Delete check
   */
  deleteCheck(id: number): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${id}`);
  }

  /**
   * Mark check as cleared
   */
  markAsCleared(id: number, request: CheckClearanceRequest): Observable<Check> {
    return this.apiService.post<Check>(`${this.endpoint}/${id}/clear`, request);
  }

  /**
   * Void check
   */
  voidCheck(id: number, request: CheckVoidRequest): Observable<Check> {
    return this.apiService.post<Check>(`${this.endpoint}/${id}/void`, request);
  }

  /**
   * Get unreconciled checks
   */
  getUnreconciledChecks(): Observable<ReconciliationSummary> {
    return this.apiService.get<ReconciliationSummary>(`${this.endpoint}/reconciliation/unreconciled`);
  }

  /**
   * Reconcile check manually
   */
  reconcileCheck(id: number, bankReference: string, clearanceDate: Date): Observable<Check> {
    return this.apiService.post<Check>(`${this.endpoint}/${id}/reconcile`, {
      bankReference,
      clearanceDate
    });
  }

  /**
   * Import bank statement for reconciliation
   */
  importBankStatement(file: File): Observable<ReconciliationSummary> {
    const formData = new FormData();
    formData.append('file', file);
    return this.apiService.post<ReconciliationSummary>(`${this.endpoint}/reconciliation/import`, formData);
  }

  /**
   * Generate check report
   */
  generateReport(options: CheckReportOptions): Observable<CheckReport> {
    return this.apiService.post<CheckReport>(`${this.endpoint}/reports/generate`, options);
  }

  /**
   * Export checks to Excel
   */
  exportToExcel(filter?: CheckFilter): Observable<Blob> {
    return this.apiService.get<Blob>(`${this.endpoint}/export/excel`, filter);
  }

  /**
   * Export report to Excel
   */
  exportReportToExcel(options: CheckReportOptions): Observable<Blob> {
    return this.apiService.post<Blob>(`${this.endpoint}/reports/export/excel`, options);
  }

  /**
   * Export report to PDF
   */
  exportReportToPDF(options: CheckReportOptions): Observable<Blob> {
    return this.apiService.post<Blob>(`${this.endpoint}/reports/export/pdf`, options);
  }

  /**
   * Get check beneficiaries
   */
  getBeneficiaries(): Observable<CheckBeneficiary[]> {
    return this.apiService.get<CheckBeneficiary[]>(`${this.endpoint}/beneficiaries`);
  }

  /**
   * Get check beneficiary by ID
   */
  getBeneficiaryById(id: number): Observable<CheckBeneficiary> {
    return this.apiService.get<CheckBeneficiary>(`${this.endpoint}/beneficiaries/${id}`);
  }

  /**
   * Create beneficiary
   */
  createBeneficiary(beneficiary: CheckBeneficiary): Observable<CheckBeneficiary> {
    return this.apiService.post<CheckBeneficiary>(`${this.endpoint}/beneficiaries`, beneficiary);
  }

  /**
   * Update beneficiary
   */
  updateBeneficiary(id: number, beneficiary: CheckBeneficiary): Observable<CheckBeneficiary> {
    return this.apiService.put<CheckBeneficiary>(`${this.endpoint}/beneficiaries/${id}`, beneficiary);
  }

  /**
   * Upload check image
   */
  uploadCheckImage(checkId: number, file: File): Observable<Check> {
    const formData = new FormData();
    formData.append('checkId', checkId.toString());
    formData.append('file', file);

    return this.apiService.post<Check>(`${this.endpoint}/${checkId}/image`, formData);
  }

  /**
   * Generate amount in words
   */
  generateAmountInWords(amount: number, currency: Currency): Observable<string> {
    return this.apiService.post<string>(`${this.endpoint}/utils/amount-in-words`, {
      amount,
      currency
    });
  }

  /**
   * Validate check data
   */
  validateCheck(check: CheckDto): Observable<boolean> {
    return this.apiService.post<boolean>(`${this.endpoint}/validate`, check);
  }
}
