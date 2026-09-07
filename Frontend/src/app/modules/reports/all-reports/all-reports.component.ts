import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { PaginationComponent } from '../../../shared/components';
import { AuthService } from '../../../core/services/auth.service';

/** One hub card — mirrors a routed report screen in reports-routing.module.ts. */
interface ReportCard {
  /** Router link to the report screen. */
  route: string;
  /** i18n key of the report title (same key the route's pageTitle uses). */
  titleKey: string;
  /** Feather icon class for the card. */
  icon: string;
  /** Permission that gates the target route — the card hides without it. */
  permission: string;
}

/**
 * All Reports hub (#/reports/all) — the card directory of the reporting vertical.
 *
 * Navigation surface only: every card forwards to a routed report screen that keeps its own
 * AuthGuard + PermissionGuard, so this page never re-implements authorisation. The cards are
 * filtered by the SAME permissions the sidebar entries use; the route itself is gated by
 * AuthGuard alone so a Families.FollowUp-only caller (the Reports section's other key) still
 * reaches their single card rather than a permission wall.
 *
 * The registry is intentionally a static list — it is the menu, not data.
 */
@Component({
  selector: 'app-all-reports',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    TranslateModule,
    PaginationComponent
  ],
  templateUrl: './all-reports.component.html',
  styleUrls: ['./all-reports.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AllReportsComponent implements OnInit {
  cards: ReportCard[] = [];

  currentPage = 1;
  readonly pageSize = 12;

  /** Order matches the sidebar section (UC-FAM-11 first, then the epic-18 UC-RPT screens). */
  private readonly registry: ReportCard[] = [
    // UC-FAM-11: متابعة إدخالات الأسر (legacy screen name kept verbatim)
    { route: '/reports/family-orphans', titleKey: 'families.followUp.title', icon: 'fe fe-activity', permission: 'Families.FollowUp' },
    // UC-RPT-01: بيانات الأيتام
    { route: '/reports/orphans', titleKey: 'reports.orphanData.title', icon: 'fe fe-users', permission: 'Reports.View' },
    // UC-RPT-03: الايتام المستبعدين
    { route: '/reports/excluded-orphans', titleKey: 'reports.excludedOrphans.title', icon: 'fe fe-user-x', permission: 'Reports.View' },
    // UC-RPT-04: أيتام انتهت كفالتهم
    { route: '/reports/finished-sponsorship-orphans', titleKey: 'reports.finishedSponsorship.title', icon: 'fe fe-check-circle', permission: 'Reports.View' },
    // UC-RPT-05: أيتام غير مكفولين
    { route: '/reports/unsponsored-orphans', titleKey: 'reports.unsponsoredOrphans.title', icon: 'fe fe-help-circle', permission: 'Reports.View' },
    // UC-RPT-06: ارامل مطلوب لهم كفاله
    { route: '/reports/widows-allowing-sponsorship', titleKey: 'reports.widowsSponsorship.title', icon: 'fe fe-heart', permission: 'Reports.View' },
    // UC-RPT-07: تقرير الكروت المسجله
    { route: '/reports/meza-cards', titleKey: 'reports.mezaCards.title', icon: 'fe fe-credit-card', permission: 'Reports.View' },
    // UC-RPT-09: بيانات أسر المساعدات
    { route: '/reports/beneficiary-family-details', titleKey: 'reports.beneficiaryFamilyDetails.title', icon: 'fe fe-home', permission: 'Reports.View' },
    // UC-RPT-11: مشاريع الأسر
    { route: '/reports/registered-family-projects', titleKey: 'reports.familyProjects.title', icon: 'fe fe-briefcase', permission: 'Reports.View' },
    // UC-RPT-12: تقارير تعديل المعيل
    { route: '/reports/provider-sponsor-changes', titleKey: 'reports.providerChanges.title', icon: 'fe fe-edit-3', permission: 'Reports.View' },
    // UC-RPT-13: متابعة إدخلات الأسر والأيتام
    { route: '/reports/family-orphans-entries', titleKey: 'reports.familyOrphans.title', icon: 'fe fe-list', permission: 'Reports.View' },
    // UC-RPT-15: أيتام مكودون مطلوب لهم تقرير
    { route: '/reports/orphans-missing-reports', titleKey: 'reports.orphansMissingReports.title', icon: 'fe fe-file-text', permission: 'Reports.View' },
    // UC-RPT-16: أيتام مطلوب لهم ملفات
    { route: '/reports/orphans-missing-files', titleKey: 'reports.orphansMissingFiles.title', icon: 'fe fe-folder', permission: 'Reports.View' },
    // UC-RPT-17: تقارير في انتظار الموافقة
    { route: '/reports/reports-awaiting-approval', titleKey: 'reports.awaitingApproval.title', icon: 'fe fe-clock', permission: 'Reports.View' },
    // UC-RPT-18: تقارير تم رفضها
    { route: '/reports/refused-reports', titleKey: 'reports.refusedReports.title', icon: 'fe fe-x-circle', permission: 'Reports.View' },
    // UC-RPT-19: أيتام بدون تقرير مجدد
    { route: '/reports/non-renewed-reports', titleKey: 'reports.nonRenewed.title', icon: 'fe fe-repeat', permission: 'Reports.View' },
    // UC-RPT-20: متابعة الجمعيات
    { route: '/reports/charity-payment-tracking', titleKey: 'reports.charityTracking.title', icon: 'fe fe-crosshair', permission: 'Reports.View' },
    // §23.S.11: أيتام مستحقون دفعات سابقة
    { route: '/reports/missed-payments', titleKey: 'reports.missedPayments.title', icon: 'fe fe-alert-circle', permission: 'Reports.View' },
    // §23.S.18 / UC-RPT-24: صور الأيتام
    { route: '/reports/orphan-files', titleKey: 'reports.orphanFiles.title', icon: 'fe fe-camera', permission: 'Reports.View' },
    // UC-RPT-26: طباعة الاستبانة
    { route: '/reports/survey-questionnaire', titleKey: 'reports.surveyQuestionnaire.title', icon: 'fe fe-clipboard', permission: 'Reports.View' },
    // UC-RPT-28: أيتام لم يصرف لهم
    { route: '/reports/orphans-without-payment', titleKey: 'reports.orphansWithoutPayment.title', icon: 'fe fe-dollar-sign', permission: 'Reports.View' },
    // UC-RPT-33: بيان الشيكات
    { route: '/reports/cheque-statement', titleKey: 'reports.chequeStatement.title', icon: 'fe fe-book-open', permission: 'Reports.View' },
    // UC-RPT-35: الأيتام والأرامل الجدد
    { route: '/reports/new-beneficiaries', titleKey: 'reports.newBeneficiaries.title', icon: 'fe fe-user-plus', permission: 'Reports.View' },
    // UC-RPT-36: كشوف المتابعة والتسليم
    { route: '/reports/follow-up-sheets', titleKey: 'reports.followUpSheets.title', icon: 'fe fe-file', permission: 'Reports.View' },
    // UC-RPT-38: مرفقات الصادر الناقصة
    { route: '/reports/missing-outgoing-attachments', titleKey: 'reports.missingOutgoingAttachments.title', icon: 'fe fe-inbox', permission: 'Reports.View' },
    // UC-RPT-39: أيتام الأسر بتاريخ
    { route: '/reports/family-orphans-by-date', titleKey: 'reports.familyOrphansByDate.title', icon: 'fe fe-calendar', permission: 'Reports.View' }
  ];

  constructor(private authService: AuthService) { }

  ngOnInit(): void {
    this.cards = this.registry.filter(card => this.authService.hasPermission(card.permission));
    this.currentPage = 1;
  }

  /** Rotating accent colors for the icon circles, mirroring the contacts-grid avatars */
  getIconColorClass(index: number): string {
    const colors = ['bg-primary', 'bg-success', 'bg-info', 'bg-warning', 'bg-danger'];
    return colors[index % colors.length];
  }

  get pagedCards(): ReportCard[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.cards.slice(start, start + this.pageSize);
  }

  onPageChange(page: number): void {
    this.currentPage = page;
  }

  trackByRoute(_index: number, card: ReportCard): string {
    return card.route;
  }
}
