/**
 * Reports Routing Module (founded by epic 5, UC-FAM-11)
 * Route definitions for the reporting vertical. Epic 18 appends its screens here —
 * #/reports/family-orphans (this story) is the legacy screen name kept verbatim per spec §10.U.11.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Report Components
import { FamilyFollowUpReportComponent } from './family-follow-up-report/family-follow-up-report.component';
import { OrphanDataReportComponent } from './orphan-data-report/orphan-data-report.component';
import { ExcludedOrphansReportComponent } from './excluded-orphans-report/excluded-orphans-report.component';
import { FinishedSponsorshipReportComponent } from './finished-sponsorship-report/finished-sponsorship-report.component';
import { WidowsSponsorshipReportComponent } from './widows-sponsorship-report/widows-sponsorship-report.component';
import { MezaCardsReportComponent } from './meza-cards-report/meza-cards-report.component';
import { BeneficiaryFamilyDetailsComponent } from './beneficiary-family-details/beneficiary-family-details.component';
import { RegisteredFamilyProjectsComponent } from './registered-family-projects/registered-family-projects.component';
import { ProviderSponsorChangesComponent } from './provider-sponsor-changes/provider-sponsor-changes.component';
import { FamilyOrphansEntriesComponent } from './family-orphans-entries/family-orphans-entries.component';
import { OrphansMissingReportsComponent } from './orphans-missing-reports/orphans-missing-reports.component';
import { OrphansMissingFilesComponent } from './orphans-missing-files/orphans-missing-files.component';
import { ReportsAwaitingApprovalComponent } from './reports-awaiting-approval/reports-awaiting-approval.component';
import { RefusedReportsComponent } from './refused-reports/refused-reports.component';
import { BatchNonRenewedReportsComponent } from './non-renewed-reports/batch-non-renewed-reports.component';
import { CharityPaymentTrackingComponent } from './charity-payment-tracking/charity-payment-tracking.component';
import { MissedPaymentsComponent } from './missed-payments/missed-payments.component';
import { OrphanFilesComponent } from './orphan-files/orphan-files.component';
import { SurveyQuestionnaireComponent } from './survey-questionnaire/survey-questionnaire.component';
import { OrphansWithoutPaymentReportComponent } from './orphans-without-payment-report/orphans-without-payment-report.component';
import { ChequeStatementComponent } from './cheque-statement/cheque-statement.component';
import { NewBeneficiariesReportComponent } from './new-beneficiaries-report/new-beneficiaries-report.component';
import { FollowUpSheetsComponent } from './follow-up-sheets/follow-up-sheets.component';
import { MissingOutgoingAttachmentsComponent } from './missing-outgoing-attachments/missing-outgoing-attachments.component';
import { FamilyOrphansByDateReportComponent } from './family-orphans-by-date-report/family-orphans-by-date-report.component';
import { AllReportsComponent } from './all-reports/all-reports.component';

// Guards
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

const reportsRoutes: Routes = [
  {
    path: '',
    redirectTo: 'family-orphans',
    pathMatch: 'full'
  },
  {
    // All Reports hub — the card directory over this module's routes. Gated by AuthGuard alone
    // (the section's two keys are Reports.View OR Families.FollowUp, and the guard takes one
    // permission); the CARDS filter by permission and every target route keeps its own
    // PermissionGuard, so this page is navigation chrome, never an authorisation control.
    path: 'all',
    component: AllReportsComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'reports.allReports.title'
    }
  },
  {
    // UC-FAM-11: متابعة إدخالات الأسر — family-file register activity for one day.
    // The route name is the legacy screen name, kept verbatim (story Dev Notes).
    path: 'family-orphans',
    component: FamilyFollowUpReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'families.followUp.title',
      permission: 'Families.FollowUp'
    }
  },
  {
    // UC-RPT-01: بيانات الأيتام — the caller-scoped orphan master listing (EP-18 skeleton).
    path: 'orphans',
    component: OrphanDataReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.orphanData.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-03: الايتام المستبعدين — the excluded set, reported read-only (EP-18).
    path: 'excluded-orphans',
    component: ExcludedOrphansReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.excludedOrphans.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-04: أيتام انتهت كفالتهم — the ended-sponsorship set (HQ-only endpoint; empty
    // until the sponsorship state lands — recorded product ruling, never a patched column).
    path: 'finished-sponsorship-orphans',
    component: FinishedSponsorshipReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.finishedSponsorship.title',
      permission: 'Reports.View',
      orphanStatusVariant: 'finished'
    }
  },
  {
    // UC-RPT-05: أيتام غير مكفولين — coded orphans with status Unsponsored. Shares 18-4's
    // component/grid; only the route-data predicate differs (18-5 Task 3 ruling).
    path: 'unsponsored-orphans',
    component: FinishedSponsorshipReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.unsponsoredOrphans.title',
      permission: 'Reports.View',
      orphanStatusVariant: 'unsponsored'
    }
  },
  {
    // UC-RPT-06: ارامل مطلوب لهم كفاله — the 22-column widow grid, query-only (no استخراج).
    path: 'widows-allowing-sponsorship',
    component: WidowsSponsorshipReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.widowsSponsorship.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-07: تقرير الكروت المسجله — families with registered guardian Meza cards (HQ-only;
    // empty with a recorded gap until the card fields exist in the domain).
    path: 'meza-cards',
    component: MezaCardsReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.mezaCards.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-09: بيانات أسر المساعدات — distinct families benefiting from seasonal-aid
    // assistance, campaigns aggregated per row.
    path: 'beneficiary-family-details',
    component: BeneficiaryFamilyDetailsComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.beneficiaryFamilyDetails.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-11: مشاريع الأسر — HQ-only registered family projects (empty with a recorded
    // gap until a project entity carries the family link).
    path: 'registered-family-projects',
    component: RegisteredFamilyProjectsComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.familyProjects.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-12: تقارير تعديل المعيل — guardian change history; one row per provider
    // assignment, current guardian projected (no audit trail exists — recorded gap).
    path: 'provider-sponsor-changes',
    component: ProviderSponsorChangesComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.providerChanges.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-13: متابعة إدخلات الأسر والأيتام — entry tracking (totals + details). 5-11
    // (UC-FAM-11) owns the legacy `family-orphans` route per its story decision; this screen
    // takes the sibling path (deviation recorded — 18-39's class).
    path: 'family-orphans-entries',
    component: FamilyOrphansEntriesComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.familyOrphans.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-15: أيتام مكودون مطلوب لهم تقرير — per-charity counts of coded orphans with
    // no accepted report in the trailing 12 months (BR-11 semantics), with a per-charity
    // drill-down. A charity caller is server-clamped to its own rows.
    path: 'orphans-missing-reports',
    component: OrphansMissingReportsComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.orphansMissingReports.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-16: أيتام مطلوب لهم ملفات — coded orphans whose LATEST periodic report is
    // flagged MissingDocuments (anti-semi-join on ReportDate), with the family's address
    // and village for the chase. A charity caller is server-clamped to its own rows.
    path: 'orphans-missing-files',
    component: OrphansMissingFilesComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.orphansMissingFiles.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-17: تقارير في انتظار الموافقة — the HQ review queue (submitted, neither
    // accepted nor refused), oldest first. Pattern-holder for the shared §23.S.16/§23.S.17
    // grid; 18-18 forks it. The jump column opens /periodic-orphan-reports/{id}/review.
    path: 'reports-awaiting-approval',
    component: ReportsAwaitingApprovalComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.awaitingApproval.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-18: تقارير تم رفضها — the refused worklist (18-17's grid with the refused
    // filter and the reason resolved). The decision write path is NOT here — the jump
    // opens /periodic-orphan-reports/{id}/review, which authorises and records.
    path: 'refused-reports',
    component: RefusedReportsComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.refusedReports.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-19: أيتام بدون تقرير مجدد — the batch chase list. The board assigns no route
    // (a hosted function in the legacy system); this thin screen grants reachability
    // (decision recorded). Same endpoint as the §14.U.14 window list — batch mode.
    path: 'non-renewed-reports',
    component: BatchNonRenewedReportsComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.nonRenewed.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-20: متابعة الجمعيات — the cross-charity tracking grid over one payment batch
    // (HQ-only endpoint). 18-21's two update-tracking commands render disabled here.
    path: 'charity-payment-tracking',
    component: CharityPaymentTrackingComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.charityTracking.title',
      permission: 'Reports.View'
    }
  },
  {
    // §23.S.11 أيتام مستحقون دفعات سابقة — caller-scoped arrears grid with dynamic batch
    // columns (charity callers pinned server-side; the dropdown is an HQ-only narrow).
    path: 'missed-payments',
    component: MissedPaymentsComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.missedPayments.title',
      permission: 'Reports.View'
    }
  },
  {
    // §23.S.18 صور الأيتام — the photograph manifest over accepted reports in a date
    // window (UC-RPT-24; the certificates grid on the same legacy screen is 18-25's).
    path: 'orphan-files',
    component: OrphanFilesComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.orphanFiles.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-26: طباعة الاستبانة — the blank field-survey questionnaire (family +
    // widow variants) produced client-side through 18-21's browser-print service;
    // a static form, so there is no endpoint. Thin route granted for reachability
    // (the board assigns none — decision recorded in the story).
    path: 'survey-questionnaire',
    component: SurveyQuestionnaireComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.surveyQuestionnaire.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-28: أيتام لم يصرف لهم — the zero-disbursement gaps of one payment batch
    // (no cheque, no transfer, nothing received). HQ-only endpoint; the §23.S.3 orphans
    // screen's command lands here carrying its charity selection as ?charityId=.
    path: 'orphans-without-payment',
    component: OrphansWithoutPaymentReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.orphansWithoutPayment.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-33: بيان الشيكات — the bank reconciliation statement over the EXISTING
    // UC-CHQ-09 endpoint (no new Reports route; the endpoint stays the authorisation
    // control — its own roles include Accountant/FinancialOfficer beyond this gate).
    path: 'cheque-statement',
    component: ChequeStatementComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.chequeStatement.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-35: الأيتام والأرامل الجدد — the sponsorship-offer lists: the four legacy
    // .rpt variants (new orphans / widened / new widows / widows by family) collapsed to
    // one variant-keyed endpoint + one screen with four commands. HQ-only endpoint;
    // charity callers are pinned server-side regardless.
    path: 'new-beneficiaries',
    component: NewBeneficiariesReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.newBeneficiaries.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-36: كشوف المتابعة والتسليم — the legacy Crystal trio (متابعة / متابعة الأسر /
    // تسليم) collapsed to one variant-keyed GET + one screen. HQ-only endpoint; charity
    // callers are pinned server-side regardless. Route minted here (the board lists none).
    path: 'follow-up-sheets',
    component: FollowUpSheetsComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.followUpSheets.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-38: مرفقات الصادر الناقصة — HQ-only audit grid (the HQ role set is enforced
    // SERVER-side; the route and the menu entry both gate on Reports.View, which follows
    // the route-permission alignment ruling — Review P27 2026-08-26).
    path: 'missing-outgoing-attachments',
    component: MissingOutgoingAttachmentsComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.missingOutgoingAttachments.title',
      permission: 'Reports.View'
    }
  },
  {
    // UC-RPT-39: أيتام الأسر بتاريخ — distinct from 18-13's family-orphans (follow-up tracking)
    // by story ruling; do NOT rename that one.
    path: 'family-orphans-by-date',
    component: FamilyOrphansByDateReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'reports.familyOrphansByDate.title',
      permission: 'Reports.View'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(reportsRoutes)],
  exports: [RouterModule]
})
export class ReportsRoutingModule { }
