import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import {
  SeasonalAidCampaign,
  SeasonalAidBeneficiary,
  EligibleFamiliesFilter
} from '../models/seasonal-aid.model';
import { LookupBase } from '../../../shared/models/lookup.base.model';
import { PaginationComponent, BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';

/**
 * UC-PRJ-10 — كافة الأسر للمشروع: every family matching the campaign scope that is NOT yet
 * registered in it. This is the rotation list the campaign office reads before each round.
 */
@Component({
  selector: 'app-eligible-families',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, PaginationComponent, BreadcrumbComponent, SharedModule],
  templateUrl: './eligible-families.component.html',
  styleUrls: ['./eligible-families.component.scss']
})
export class EligibleFamiliesComponent implements OnInit {
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'seasonalAid.title', url: '/seasonal-aid' },
    { label: 'seasonalAid.eligibleFamilies' }
  ];

  campaign: SeasonalAidCampaign | null = null;
  families: SeasonalAidBeneficiary[] = [];
  loading = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 10;

  searchTerm = '';
  regionFilter: number | null = null;

  regionOptions: LookupBase[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private seasonalAidService: SeasonalAidService,
    private lookupManagementService: LookupManagementService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.loadCampaign(params['id']);
    });
  }

  loadCampaign(id: string): void {
    this.loading = true;
    this.seasonalAidService.getCampaignById(id).subscribe({
      next: campaign => {
        this.campaign = campaign;
        this.loading = false;
        this.loadFamilies(id);
        if (campaign.countryId) {
          this.lookupManagementService.getRegionsByCountry(campaign.countryId).subscribe({
            next: regions => (this.regionOptions = (regions || []).map(r => ({ id: r.id, name: r.nameAr || r.name }))),
            error: () => (this.regionOptions = [])
          });
        }
      },
      error: () => {
        this.loading = false;
        this.router.navigate(['/seasonal-aid']);
      }
    });
  }

  loadFamilies(id: string): void {
    this.loading = true;
    const filter: Partial<EligibleFamiliesFilter> = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortDescending: false
    };
    if (this.searchTerm) {
      filter.searchTerm = this.searchTerm;
    }
    if (this.regionFilter) {
      filter.regionId = this.regionFilter;
    }

    this.seasonalAidService.getEligibleFamilies(id, filter).subscribe({
      next: result => {
        this.families = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.families = [];
        this.totalCount = 0;
        this.loading = false;
      }
    });
  }

  onFilterChange(): void {
    this.currentPage = 1;
    if (this.campaign) {
      this.loadFamilies(this.campaign.id);
    }
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    if (this.campaign) {
      this.loadFamilies(this.campaign.id);
    }
  }

  goToRegistration(): void {
    if (this.campaign) {
      this.router.navigate(['/seasonal-aid', this.campaign.id, 'beneficiaries']);
    }
  }

  onBack(): void {
    if (this.campaign) {
      this.router.navigate(['/seasonal-aid', this.campaign.id]);
    }
  }

  trackByFamily(index: number, family: SeasonalAidBeneficiary): string {
    return family.familyId;
  }

  trackByRegion(index: number, region: LookupBase): number {
    return region.id;
  }
}
