import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

import { BeneficiaryFamilyFilter, BeneficiaryFamilyRow, CharityPaymentTrackingFilter, CharityPaymentTrackingRow, ExcludedOrphansFilter, ExcludedOrphanRow, FamilyEntryDetailRow, FamilyEntryTotals, FamilyUpdateTrackingFilter, FamilyUpdateTrackingRow, MissedPaymentsFilter, MissedPaymentRow, FamilyProjectsFilter, FamilyProjectRow, MezaCardsFilter, MezaCardsRow, NonRenewedReportsRequest, NonRenewedReportsResult, OrphanFileFilter, OrphanFileManifestRow, OrphansWithoutPaymentFilter, OrphansWithoutPaymentRow, PaymentsOutcomeFilter, PaymentsOutcomeReport, PaymentsOutcomeRow, ChequeNumbersFilter, ChequeNumbersReport, ChequeNumbersRow, ReceiptCardsFilter, ReceiptCardsReport, ReceiptCardRow, NewBeneficiariesFilter, NewBeneficiariesReport, NewBeneficiariesVariant, NewBeneficiaryRow, FollowUpSheetFilter, FollowUpSheetRow, GuardianIdentificationFilter, GuardianIdentificationRow, GuardianIdentificationSheet, MissingOutgoingAttachmentsFilter, MissingOutgoingAttachmentsRow, FamilyOrphansByDateFilter, FamilyOrphanRow, FamilyWithOrphans, OrphanDataFilter, OrphanDataRow, OrphansMissingFilesFilter, OrphansMissingFilesRow, OrphansMissingReportsFilter, OrphansMissingReportsSummaryRow, OrphansMissingReportsDetailRow, OrphanStatusReportFilter, OrphanStatusReportRow, ProviderChangesFilter, ProviderChangeRow, RefusedReportsFilter, RefusedReportsRow, ReportPagedResult, ReportSheetPayload, ReportsAwaitingApprovalFilter, ReportsAwaitingApprovalRow, WidowSponsorshipFilter, WidowSponsorshipRow } from '../models/report.model';

/**
 * Reports module service (UC-FAM-14 sheet payloads; EP-18 report queries from 18-1 on).
 * Sheet payloads keep their legacy POST /api/Reports/{reportKey}/export/pdf shape; EP-18
 * report keys are typed POST endpoints returning the raw paged envelope.
 */
@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private apiUrl = `${environment.apiUrl}/api/Reports`;

  constructor(private http: HttpClient) {}

  /** Build a printable sheet for one of the served report keys (whole selection, unpaged). */
  exportSheet<T>(reportKey: string, payload: {
    date?: string;
    charityId?: string;
    familyCode?: string;
  }): Observable<ReportSheetPayload<T>> {
    return this.http.post<ReportSheetPayload<T>>(`${this.apiUrl}/${reportKey}/export/pdf`, payload, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-RPT-01 (§23.S.3 بيانات الأيتام) — caller-scoped orphan master listing. The charity
   * scope resolves server-side from the token; charityId in the filter is an HQ-only narrow.
   */
  getOrphanData(filter: OrphanDataFilter): Observable<ReportPagedResult<OrphanDataRow>> {
    return this.http.post<ReportPagedResult<OrphanDataRow>>(`${this.apiUrl}/orphans`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-RPT-03 (§23.S.4 الايتام المستبعدين) — the excluded set; charityId in the filter is an
   * HQ-only narrow, charity callers are pinned server-side.
   */
  getExcludedOrphans(filter: ExcludedOrphansFilter): Observable<ReportPagedResult<ExcludedOrphanRow>> {
    return this.http.post<ReportPagedResult<ExcludedOrphanRow>>(`${this.apiUrl}/excluded-orphans`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-RPT-04 (§23.S.5 أيتام انتهت كفالتهم) — HQ-only. The domain has no ended state today;
   * the contract ships now, the set is empty until the state lands (recorded ruling).
   */
  getFinishedSponsorshipOrphans(filter: OrphanStatusReportFilter): Observable<ReportPagedResult<OrphanStatusReportRow>> {
    return this.http.post<ReportPagedResult<OrphanStatusReportRow>>(`${this.apiUrl}/finished-sponsorship-orphans`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-RPT-05 (§23.S.6 أيتام غير مكفولين) — HQ-only. Coded orphans with status Unsponsored;
   * an empty Code is UNCODED, not unsponsored (EP-08 ruling).
   */
  getUnsponsoredOrphans(filter: OrphanStatusReportFilter): Observable<ReportPagedResult<OrphanStatusReportRow>> {
    return this.http.post<ReportPagedResult<OrphanStatusReportRow>>(`${this.apiUrl}/unsponsored-orphans`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-RPT-06 (§23.S.9 ارامل مطلوب لهم كفاله) — the 22-column widow grid (family mother +
   * family residence/income). Query-only: the spec lists no استخراج command for this screen.
   */
  getWidowsAllowingSponsorship(filter: WidowSponsorshipFilter): Observable<ReportPagedResult<WidowSponsorshipRow>> {
    return this.http.post<ReportPagedResult<WidowSponsorshipRow>>(`${this.apiUrl}/widows-allowing-sponsorship`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-RPT-07 (§23.S.8 تقرير الكروت المسجله) — HQ-only. The row source is families carrying
   * registered guardian Meza cards; no card column exists yet, so the set is empty with a
   * recorded gap until the registration vertical lands the fields.
   */
  getMezaCards(filter: MezaCardsFilter): Observable<ReportPagedResult<MezaCardsRow>> {
    return this.http.post<ReportPagedResult<MezaCardsRow>>(`${this.apiUrl}/meza-cards`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-09 (§23.S.13) — the distinct assisted-families listing. */
  getBeneficiaryFamilies(filter: BeneficiaryFamilyFilter): Observable<ReportPagedResult<BeneficiaryFamilyRow>> {
    return this.http.post<ReportPagedResult<BeneficiaryFamilyRow>>(`${this.apiUrl}/beneficiary-family-details`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-11 (§23.S.7) — registered family projects (empty until the family-project linkage lands). */
  getRegisteredFamilyProjects(filter: FamilyProjectsFilter): Observable<ReportPagedResult<FamilyProjectRow>> {
    return this.http.post<ReportPagedResult<FamilyProjectRow>>(`${this.apiUrl}/registered-family-projects`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-12 (§23.S.19) — guardian change history, one row per provider assignment. */
  getProviderSponsorChanges(filter: ProviderChangesFilter): Observable<ReportPagedResult<ProviderChangeRow>> {
    return this.http.post<ReportPagedResult<ProviderChangeRow>>(`${this.apiUrl}/provider-sponsor-changes`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-15 (§23.S.14) — per-charity counts of coded orphans needing a report. */
  getOrphansMissingReports(filter: OrphansMissingReportsFilter): Observable<ReportPagedResult<OrphansMissingReportsSummaryRow>> {
    return this.http.post<ReportPagedResult<OrphansMissingReportsSummaryRow>>(`${this.apiUrl}/orphans-missing-reports`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-15 drill-down (ExtractDetails) — one charity's chase list; server clamps scope. */
  getOrphansMissingReportsDetail(charityId: string, page: number, pageSize: number): Observable<ReportPagedResult<OrphansMissingReportsDetailRow>> {
    return this.http.post<ReportPagedResult<OrphansMissingReportsDetailRow>>(`${this.apiUrl}/orphans-missing-reports/details`, {
      charityId,
      page,
      pageSize
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-16 (§23.S.15) — the missing-files worklist for the selected charity. */
  getOrphansMissingFiles(filter: OrphansMissingFilesFilter): Observable<ReportPagedResult<OrphansMissingFilesRow>> {
    return this.http.post<ReportPagedResult<OrphansMissingFilesRow>>(`${this.apiUrl}/orphans-missing-files`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-17 (§23.S.16) — the review queue: submitted, neither accepted nor refused. */
  getReportsAwaitingApproval(filter: ReportsAwaitingApprovalFilter): Observable<ReportPagedResult<ReportsAwaitingApprovalRow>> {
    return this.http.post<ReportPagedResult<ReportsAwaitingApprovalRow>>(`${this.apiUrl}/reports-awaiting-approval`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-18 (§23.S.17) — the refused worklist with the reason resolved. */
  getRefusedReports(filter: RefusedReportsFilter): Observable<ReportPagedResult<RefusedReportsRow>> {
    return this.http.post<ReportPagedResult<RefusedReportsRow>>(`${this.apiUrl}/refused-reports`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-RPT-13 (§23.S.10) — entry tracking off the one Families endpoint: totals or details.
   * The route id is the charity; 'all' maps to Guid.Empty (كل الجهات, HQ only — the server
   * pins a charity caller anyway).
   */
  getFamilyEntryTracking(params: {
    charityId?: string;
    date?: string;
    mode: 'totals' | 'details';
    page?: number;
    pageSize?: number;
  }): Observable<FamilyEntryTotals | ReportPagedResult<FamilyEntryDetailRow>> {
    const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';
    const id = params.charityId || EMPTY_GUID;
    let query = new HttpParams().set('mode', params.mode);
    if (params.date) {
      query = query.set('date', params.date);
    }
    if (params.page) {
      query = query.set('page', String(params.page));
    }
    if (params.pageSize) {
      query = query.set('pageSize', String(params.pageSize));
    }
    return this.http.get<FamilyEntryTotals | ReportPagedResult<FamilyEntryDetailRow>>(
      `${environment.apiUrl}/api/Families/${id}/follow-up`,
      { params: query, headers: this.getHeaders() }
    ).pipe(
      catchError(this.handleError)
    );
  }

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json'
    });
  }

  /**
   * UC-ORR-14 (§14.U.14 الأيتام بدون تقرير مجدد) — the chase list: coded orphans of the
   * (token-scoped) charity with no accepted report covering the window. countOnly collapses
   * the legacy _Number variant.
   */
  getNonRenewedReports(request: NonRenewedReportsRequest): Observable<NonRenewedReportsResult> {
    return this.http.post<NonRenewedReportsResult>(`${this.apiUrl}/non-renewed-reports`, request, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-RPT-20 (§23.S.12 متابعة الجمعيات) — the cross-charity tracking grid over one
   * payment batch: one row per charity present in the batch. HQ-only endpoint.
   */
  getCharityPaymentTracking(filter: CharityPaymentTrackingFilter): Observable<ReportPagedResult<CharityPaymentTrackingRow>> {
    return this.http.post<ReportPagedResult<CharityPaymentTrackingRow>>(`${this.apiUrl}/charity-payment-tracking`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-21 — POST /api/Reports/family-update-tracking (§23.S.12's print payload). */
  getFamilyUpdateTracking(filter: FamilyUpdateTrackingFilter): Observable<ReportPagedResult<FamilyUpdateTrackingRow>> {
    return this.http.post<ReportPagedResult<FamilyUpdateTrackingRow>>(`${this.apiUrl}/family-update-tracking`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-22 — POST /api/Reports/missed-payments (caller-scoped arrears read). */
  getMissedPayments(filter: MissedPaymentsFilter): Observable<ReportPagedResult<MissedPaymentRow>> {
    return this.http.post<ReportPagedResult<MissedPaymentRow>>(`${this.apiUrl}/missed-payments`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-24 — POST /api/Reports/orphan-files/export (the photograph manifest). */
  getOrphanFiles(filter: OrphanFileFilter): Observable<ReportPagedResult<OrphanFileManifestRow>> {
    return this.http.post<ReportPagedResult<OrphanFileManifestRow>>(`${this.apiUrl}/orphan-files/export`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-25 — POST /api/Reports/certificate-files/export (same shape, certificate kind). */
  getCertificateFiles(filter: OrphanFileFilter): Observable<ReportPagedResult<OrphanFileManifestRow>> {
    return this.http.post<ReportPagedResult<OrphanFileManifestRow>>(`${this.apiUrl}/certificate-files/export`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-28 — أيتام لم يصرف لهم: the zero-disbursement gaps of one payment batch. */
  getOrphansWithoutPayment(filter: OrphansWithoutPaymentFilter): Observable<ReportPagedResult<OrphansWithoutPaymentRow>> {
    return this.http.post<ReportPagedResult<OrphansWithoutPaymentRow>>(`${this.apiUrl}/orphans-without-payment`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-29 — المستلمون / غير المستلمين / الموقوفون of one cheque batch.
   *  One endpoint; the variant in the filter picks the list (§23.U.29 one-dataset rule). */
  getPaymentsOutcome(filter: PaymentsOutcomeFilter): Observable<PaymentsOutcomeReport> {
    return this.http.post<PaymentsOutcomeReport>(`${this.apiUrl}/payments-received`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-30 — أرقام الشيكات of one batch: rows with a cheque number recorded. */
  getChequeNumbers(filter: ChequeNumbersFilter): Observable<ChequeNumbersReport> {
    return this.http.post<ChequeNumbersReport>(`${this.apiUrl}/cheque-numbers`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-RPT-31 — كروت الاستلام of one batch: one card per (guardian, orphan) payment item. */
  getReceiptCards(filter: ReceiptCardsFilter): Observable<ReceiptCardsReport> {
    return this.http.post<ReceiptCardsReport>(`${this.apiUrl}/receipt-cards`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-RPT-35 — الأيتام والأرامل الجدد: the four legacy .rpt variants as one variant-keyed
   * read (orphans / orphansV2 / widows / widowsByFamily). HQ-only endpoint.
   */
  getNewBeneficiaries(filter: NewBeneficiariesFilter): Observable<NewBeneficiariesReport> {
    return this.http.post<NewBeneficiariesReport>(`${this.apiUrl}/new-beneficiaries`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-RPT-36 (§23.U.36 كشوف المتابعة والتسليم) — the legacy Crystal trio (متابعة / متابعة
   * الأسر / تسليم) as one variant-keyed GET. HQ-only endpoint roles; the charity narrow rides
   * the query string.
   */
  getFollowUpSheets(filter: FollowUpSheetFilter): Observable<ReportPagedResult<FollowUpSheetRow>> {
    let query = new HttpParams()
      .set('variant', filter.variant)
      .set('page', String(filter.page))
      .set('pageSize', String(filter.pageSize));
    if (filter.charityId) {
      query = query.set('charityId', filter.charityId);
    }
    if (filter.dateFrom) {
      query = query.set('dateFrom', filter.dateFrom);
    }
    if (filter.dateTo) {
      query = query.set('dateTo', filter.dateTo);
    }
    return this.http.get<ReportPagedResult<FollowUpSheetRow>>(`${this.apiUrl}/follow-up-sheets`, {
      params: query,
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-RPT-37 (§23.U.37 كشوف تعريف العائل والأرامل) — the identification sheets: every
   * guardian / the widows / one family's union, ONE variant-keyed POST returning the whole
   * selection. Supersedes the legacy {reportKey}/export/pdf pair (recorded). The producing
   * user resolves server-side from the token.
   */
  getGuardianIdentificationSheets(filter: GuardianIdentificationFilter): Observable<GuardianIdentificationSheet> {
    return this.http.post<GuardianIdentificationSheet>(`${this.apiUrl}/guardian-identification-sheets`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * §23.U.38 مرفقات الصادر الناقصة — paged audit of outgoing letters with zero attachment
   * rows (the v1 rule). GET with a query-bound filter; the screen is query/grid-only.
   */
  getMissingOutgoingAttachments(filter: MissingOutgoingAttachmentsFilter): Observable<ReportPagedResult<MissingOutgoingAttachmentsRow>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());
    if (filter.charityId) {
      params = params.set('charityId', filter.charityId);
    }
    if (filter.dateFrom) {
      params = params.set('dateFrom', filter.dateFrom);
    }
    if (filter.dateTo) {
      params = params.set('dateTo', filter.dateTo);
    }
    if (filter.outgoingCategoryId) {
      params = params.set('outgoingCategoryId', filter.outgoingCategoryId.toString());
    }
    return this.http.get<ReportPagedResult<MissingOutgoingAttachmentsRow>>(`${this.apiUrl}/missing-outgoing-attachments`, {
      headers: this.getHeaders(),
      params
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * §23.U.39 أيتام الأسر بتاريخ — one charity's families as at a date, each with its orphans
   * grouped under it (a page = families). POST: the as-at date rides the body. The document is
   * composed client-side (18-21's PDF path — the legacy /export/pdf is superseded).
   */
  getFamilyOrphansByDate(filter: FamilyOrphansByDateFilter): Observable<ReportPagedResult<FamilyWithOrphans>> {
    return this.http.post<ReportPagedResult<FamilyWithOrphans>>(`${this.apiUrl}/family-orphans`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any): Observable<never> {
    console.error('Report service error:', error);
    return throwError(() => {
      const errorMessage = error.error?.message || error.error?.title || 'An unexpected error occurred';
      return {
        message: errorMessage,
        status: error.status || 500,
        details: error.error?.errors || null
      };
    });
  }
}
