import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription, debounceTime, distinctUntilChanged } from 'rxjs';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import {
  SeasonalAidCampaignListItem,
  SeasonalAidCampaignFilter,
  SeasonalAidCampaignStatistics
} from '../models/seasonal-aid.model';
import { PaginationComponent, BreadcrumbComponent, BreadcrumbItem, PageHeaderComponent } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';

@Component({
  selector: 'app-campaign-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    TranslateModule,
    PaginationComponent,
    BreadcrumbComponent,
    PageHeaderComponent,
    SharedModule,
    DropDownComponent
  ],
  templateUrl: './campaign-list.component.html',
  styleUrls: ['./campaign-list.component.scss']
})
export class CampaignListComponent implements OnInit, OnDestroy {
  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'seasonalAid.title' }
  ];

  // Page header actions
  pageActions = [
    {
      label: 'seasonalAid.addCampaign',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createCampaign()
    },
    {
      label: 'common.exportToExcel',
      type: 'success',
      icon: 'fe-file-plus',
      click: () => this.exportToExcel()
    }
  ];

  campaigns: SeasonalAidCampaignListItem[] = [];
  loading = false;
  totalCount = 0;
  exporting = false;

  // Register statistics band — null until the (silent-fail) load answers
  statistics: SeasonalAidCampaignStatistics | null = null;

  filterForm: FormGroup;

  currentPage = 1;
  pageSize = 10;

  // Dropdown option lists must stay stable between rebuilds: app-drop-down
  // destroys and re-initialises Select2 whenever the [initialData] reference
  // changes, so a getter returning a fresh array on every change-detection
  // pass creates an endless re-init loop that freezes the page. The labels are
  // pre-translated (app-drop-down renders raw text, not keys), so the arrays
  // are rebuilt only on actual language switches.
  campaignTypeOptions: Array<{ id: string; name: string }> = [];
  statusOptions: Array<{ id: string; name: string }> = [];

  /** Rebuilds translated option labels on language switch; torn down in ngOnDestroy. */
  private langChangeSubscription?: Subscription;

  constructor(
    private fb: FormBuilder,
    private seasonalAidService: SeasonalAidService,
    private translate: TranslateService,
    private router: Router
  ) {
    this.filterForm = this.fb.group({
      searchValue: [''],
      campaignType: [''],
      status: ['all']
    });
  }

  ngOnInit(): void {
    this.initializeCampaignTypeOptions();
    this.initializeStatusOptions();

    // The option labels are pre-translated (app-drop-down renders raw text), so a
    // language switch needs a rebuild — the translate pipe can't refresh them.
    this.langChangeSubscription = this.translate.onLangChange.subscribe(() => {
      this.initializeCampaignTypeOptions();
      this.initializeStatusOptions();
    });

    // The search box has no (input) handler in the template — drive reloads from
    // the form control instead, debounced so each burst of keystrokes is one call.
    this.filterForm.controls['searchValue'].valueChanges
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(() => this.onSearchChange());

    this.loadCampaigns();
    this.loadStatistics();
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  /** Campaign type options carry string ids — the '' sentinel means "no filter". */
  private initializeCampaignTypeOptions(): void {
    this.campaignTypeOptions = [
      { id: '', name: this.translate.instant('common.all') },
      { id: 'Ramadan', name: this.translate.instant('seasonalAid.campaignTypes.Ramadan') },
      { id: 'EidAlFitr', name: this.translate.instant('seasonalAid.campaignTypes.EidAlFitr') },
      { id: 'EidAlAdha', name: this.translate.instant('seasonalAid.campaignTypes.EidAlAdha') },
      { id: 'Winter', name: this.translate.instant('seasonalAid.campaignTypes.Winter') },
      { id: 'SchoolSupplies', name: this.translate.instant('seasonalAid.campaignTypes.SchoolSupplies') },
      { id: 'Other', name: this.translate.instant('seasonalAid.campaignTypes.Other') }
    ];
  }

  /** Status options carry string ids — the 'all' sentinel means "no filter". */
  private initializeStatusOptions(): void {
    this.statusOptions = [
      { id: 'all', name: this.translate.instant('common.all') },
      { id: 'active', name: this.translate.instant('seasonalAid.active') },
      { id: 'inactive', name: this.translate.instant('seasonalAid.inactive') },
      { id: 'closed', name: this.translate.instant('seasonalAid.closed') }
    ];
  }

  loadCampaigns(): void {
    this.loading = true;

    this.seasonalAidService.getCampaigns(this.buildFilter()).subscribe({
      next: result => {
        this.campaigns = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.campaigns = [];
        this.totalCount = 0;
        this.loading = false;
      }
    });
  }

  /**
   * Load the register statistics band — scoped server-side; describes the caller's
   * whole register, not the current search. Silent-fail: the band is optional chrome,
   * the grid is the payload.
   */
  loadStatistics(): void {
    this.seasonalAidService.getStatistics().subscribe({
      next: statistics => (this.statistics = statistics),
      error: (error: unknown) => console.error('Error loading campaign statistics:', error)
    });
  }

  /** Server-side filtering — search/type run server-side on every keystroke change. */
  private buildFilter(): SeasonalAidCampaignFilter {
    const formValues = this.filterForm.value;
    const filter: SeasonalAidCampaignFilter = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortDescending: false
    };

    if (formValues.searchValue) {
      filter.searchTerm = formValues.searchValue;
    }
    if (formValues.campaignType) {
      filter.campaignType = formValues.campaignType;
    }
    if (formValues.status && formValues.status !== 'all') {
      if (formValues.status === 'active') {
        filter.isActive = true;
        filter.isClosed = false;
      } else if (formValues.status === 'inactive') {
        filter.isActive = false;
      } else if (formValues.status === 'closed') {
        filter.isClosed = true;
      }
    }

    return filter;
  }

  onSearchChange(): void {
    this.currentPage = 1;
    this.loadCampaigns();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadCampaigns();
  }

  clearFilters(): void {
    // emitEvent: false — the explicit reload below covers it; letting the reset
    // emit would re-trigger the debounced searchValue subscription too.
    this.filterForm.reset({ searchValue: '', campaignType: '', status: 'all' }, { emitEvent: false });
    this.onFilterChange();
  }

  hasActiveFilters(): boolean {
    const formValues = this.filterForm.value;
    return !!(formValues.searchValue || formValues.campaignType || formValues.status !== 'all');
  }

  createCampaign(): void {
    this.router.navigate(['/seasonal-aid', 'create']);
  }

  trackByCampaign(index: number, campaign: SeasonalAidCampaignListItem): string {
    return campaign.id;
  }

  getStatusBadgeClass(campaign: SeasonalAidCampaignListItem): string {
    if (campaign.isClosed) return 'badge-secondary';
    if (campaign.isActive) return 'badge-success';
    return 'badge-warning';
  }

  getStatusText(campaign: SeasonalAidCampaignListItem): string {
    if (campaign.isClosed) return 'seasonalAid.closed';
    if (campaign.isActive) return 'seasonalAid.active';
    return 'seasonalAid.inactive';
  }

  getCampaignTypeClass(type: string): string {
    const classes: Record<string, string> = {
      Ramadan: 'badge-primary',
      EidAlFitr: 'badge-info',
      EidAlAdha: 'badge-info',
      Winter: 'badge-info',
      SchoolSupplies: 'badge-warning',
      Other: 'badge-light'
    };
    return classes[type] || 'badge-light';
  }

  getBudgetUtilization(campaign: SeasonalAidCampaignListItem): number {
    if (!campaign.totalBudget) return 0;
    return (campaign.distributedBudget / campaign.totalBudget) * 100;
  }

  getBudgetUtilizationClass(campaign: SeasonalAidCampaignListItem): string {
    const utilization = this.getBudgetUtilization(campaign);
    if (utilization >= 90) return 'text-danger';
    if (utilization >= 70) return 'text-warning';
    return 'text-success';
  }

  /**
   * ExcelJS client-side export of the current page — there is no server-side campaigns
   * export endpoint, and the backend report exports are NotImplemented.
   */
  exportToExcel(): void {
    if (this.exporting) return;
    this.exporting = true;

    import('exceljs').then(({ default: ExcelJS }) => {
      const workbook = new ExcelJS.Workbook();
      const sheet = workbook.addWorksheet('Campaigns');
      sheet.addRow(['#', 'Name', 'Type', 'Start', 'End', 'Budget', 'Currency',
        'Allocated', 'Distributed', 'Registered', 'Distributed Beneficiaries',
        'Status']);
      this.campaigns.forEach((c, i) => sheet.addRow([
        i + 1, c.name, c.campaignType,
        new Date(c.startDate).toLocaleDateString(),
        new Date(c.endDate).toLocaleDateString(),
        c.totalBudget, c.budgetCurrency, c.allocatedBudget, c.distributedBudget,
        c.registeredBeneficiariesCount, c.distributedBeneficiariesCount,
        this.getStatusText(c)
      ]));

      workbook.xlsx.writeBuffer().then(buffer => {
        const blob = new Blob([buffer], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `campaigns_${new Date().toISOString().slice(0, 10)}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.exporting = false;
      });
    }).catch(() => (this.exporting = false));
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadCampaigns();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadCampaigns();
  }
}
