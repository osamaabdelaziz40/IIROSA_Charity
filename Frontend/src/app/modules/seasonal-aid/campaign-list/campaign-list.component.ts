import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import {
  SeasonalCampaign,
  CampaignType,
  CampaignFilter,
  DistributionStatus
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
    FormsModule,
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
export class CampaignListComponent implements OnInit {
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
  campaigns: SeasonalCampaign[] = [];
  filteredCampaigns: SeasonalCampaign[] = [];
  loading: boolean = false;

  // Filter form with shared components
  filterForm: FormGroup;

  currentPage: number = 1;
  pageSize: number = 10;

  campaignTypes = Object.values(CampaignType);
  distributionStatuses = Object.values(DistributionStatus);
  Math = Math;

  // Dropdown options
  get campaignTypeOptions() {
    return [
      { id: '', name: 'common.all' },
      ...this.campaignTypes.map(type => ({
        id: type,
        name: this.getCampaignTypeLabel(type)
      }))
    ];
  }

  get statusOptions() {
    return [
      { id: 'all', name: 'common.all' },
      { id: 'active', name: 'seasonalAid.active' },
      { id: 'closed', name: 'seasonalAid.closed' }
    ];
  }

  getCampaignTypeLabel(type: CampaignType): string {
    const key = `seasonalAid.campaignTypes.${type}`;
    return key; // Will be translated in the template
  }

  constructor(
    private fb: FormBuilder,
    private seasonalAidService: SeasonalAidService,
    private router: Router
  ) {
    // Initialize filter form
    this.filterForm = this.fb.group({
      searchValue: [''],
      campaignType: [''],
      status: ['all']
    });
  }

  ngOnInit(): void {
    this.loadCampaigns();
  }

  loadCampaigns(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    // Build filter from form values
    const filter: CampaignFilter = {};

    if (formValues.status && formValues.status !== 'all') {
      filter.status = formValues.status;
    }
    if (formValues.campaignType) {
      filter.campaignType = formValues.campaignType;
    }

    this.seasonalAidService.getCampaigns(filter).subscribe({
      next: (data) => {
        this.campaigns = data || [];
        this.applyFilters();
        this.loading = false;
      },
      error: () => {
        this.campaigns = [];
        this.filteredCampaigns = [];
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    let result = [...this.campaigns];
    const formValues = this.filterForm.value;

    if (formValues.searchValue) {
      const term = formValues.searchValue.toLowerCase();
      result = result.filter(c =>
        c.campaignName.toLowerCase().includes(term) ||
        (c.description && c.description.toLowerCase().includes(term)) ||
        (c.assignedCharityName && c.assignedCharityName.toLowerCase().includes(term))
      );
    }

    this.filteredCampaigns = result;
  }

  onSearchChange(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadCampaigns();
  }

  clearFilters(): void {
    this.filterForm.reset({
      searchValue: '',
      campaignType: '',
      status: 'all'
    });
    this.onFilterChange();
  }

  // Check if any filters are active
  hasActiveFilters(): boolean {
    const formValues = this.filterForm.value;
    return !!(
      formValues.searchValue ||
      formValues.campaignType ||
      formValues.status !== 'all'
    );
  }

  createCampaign(): void {
    this.router.navigate(['/seasonal-aid', 'create']);
  }

  getStatusBadgeClass(campaign: SeasonalCampaign): string {
    if (campaign.isClosed) return 'badge-secondary';
    if (campaign.isActive) return 'badge-success';
    return 'badge-warning';
  }

  getStatusText(campaign: SeasonalCampaign): string {
    if (campaign.isClosed) return 'Closed';
    if (campaign.isActive) return 'Active';
    return 'Inactive';
  }

  getCampaignTypeClass(type: CampaignType): string {
    const classes: { [key in CampaignType]: string } = {
      [CampaignType.Ramadan]: 'badge-primary',
      [CampaignType.EidAlFitr]: 'badge-info',
      [CampaignType.EidAlAdha]: 'badge-info',
      [CampaignType.Winter]: 'badge-info',
      [CampaignType.SchoolSupplies]: 'badge-warning',
      [CampaignType.Other]: 'badge-light'
    };
    return classes[type] || 'badge-light';
  }

  getBudgetUtilizationClass(campaign: SeasonalCampaign): string {
    const stats = this.calculateBudgetUtilization(campaign);
    if (stats >= 90) return 'text-danger';
    if (stats >= 70) return 'text-warning';
    return 'text-success';
  }

  calculateBudgetUtilization(campaign: SeasonalCampaign): number {
    if (!campaign.totalBudget || campaign.totalBudget === 0) return 0;
    return (campaign.perFamilyAllocation / campaign.totalBudget) * 100;
  }

  exportToExcel(): void {
    const formValues = this.filterForm.value;

    // Build filter from form values
    const filter: CampaignFilter = {};

    if (formValues.status && formValues.status !== 'all') {
      filter.status = formValues.status;
    }
    if (formValues.campaignType) {
      filter.campaignType = formValues.campaignType;
    }

    this.seasonalAidService.exportCampaignsList(filter).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `campaigns_${new Date().toISOString()}.xlsx`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    });
  }

  get paginatedCampaigns(): SeasonalCampaign[] {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    return this.filteredCampaigns.slice(start, end);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredCampaigns.length / this.pageSize);
  }

  onPageChange(page: number): void {
    this.currentPage = page;
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
  }
}
