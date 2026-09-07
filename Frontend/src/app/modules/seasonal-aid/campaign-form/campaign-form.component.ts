import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { CharityService } from '../../charities/services/charity.service';
import {
  SeasonalAidCampaign,
  CreateSeasonalAidCampaignRequest,
  UpdateSeasonalAidCampaignRequest
} from '../models/seasonal-aid.model';
import { LookupBase } from '../../../shared/models/lookup.base.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';

@Component({
  selector: 'app-campaign-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent, BreadcrumbComponent, SharedModule],
  templateUrl: './campaign-form.component.html',
  styleUrls: ['./campaign-form.component.scss']
})
export class CampaignFormComponent implements OnInit {
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'seasonalAid.title', url: '/seasonal-aid' }
  ];

  get breadcrumbsWithAction(): BreadcrumbItem[] {
    return [
      ...this.breadcrumbs,
      { label: this.isEditMode ? 'seasonalAid.editCampaign' : 'seasonalAid.createCampaign' }
    ];
  }

  campaignForm: FormGroup;
  isEditMode = false;
  campaignId: string | null = null;
  loading = false;
  saving = false;

  // Lookup dropdown data (LookupBase format for the shared drop-down component)
  countryOptions: LookupBase[] = [];
  regionOptions: LookupBase[] = [];
  centerOptions: LookupBase[] = [];
  charityOptions: LookupBase[] = [];

  campaignTypeOptions: LookupBase[] = [
    { id: 'Ramadan', name: 'seasonalAid.campaignTypes.Ramadan' },
    { id: 'EidAlFitr', name: 'seasonalAid.campaignTypes.EidAlFitr' },
    { id: 'EidAlAdha', name: 'seasonalAid.campaignTypes.EidAlAdha' },
    { id: 'Winter', name: 'seasonalAid.campaignTypes.Winter' },
    { id: 'SchoolSupplies', name: 'seasonalAid.campaignTypes.SchoolSupplies' },
    { id: 'Other', name: 'seasonalAid.campaignTypes.Other' }
  ];

  currencyOptions: LookupBase[] = [
    { id: 'EGP', name: 'EGP' },
    { id: 'SAR', name: 'SAR' },
    { id: 'USD', name: 'USD' }
  ];

  familyTypeOptions: LookupBase[] = [
    { id: 'All', name: 'seasonalAid.familyTypes.All' },
    { id: 'Orphan Families', name: 'seasonalAid.familyTypes.OrphanFamilies' },
    { id: 'Needy Families', name: 'seasonalAid.familyTypes.NeedyFamilies' }
  ];

  // Page actions for header
  pageActions = [
    {
      label: 'common.cancel',
      type: 'secondary',
      icon: 'fe fe-x',
      click: () => this.onCancel()
    }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private seasonalAidService: SeasonalAidService,
    private lookupManagementService: LookupManagementService,
    private charityService: CharityService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.campaignForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadCountries();
    this.loadCharities();

    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.campaignId = params['id'];
        this.loadCampaign(params['id']);
      }
    });
  }

  createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      campaignType: ['Ramadan', Validators.required],
      description: ['', Validators.maxLength(2000)],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      totalBudget: [null, [Validators.required, Validators.min(0.01)]],
      budgetCurrency: ['EGP', [Validators.required, Validators.maxLength(3)]],
      perFamilyAllocation: [null, [Validators.required, Validators.min(0.01)]],
      countryId: [null],
      regionId: [null],
      centerId: [null],
      charityId: [''],
      maximumFamilies: [null, Validators.min(1)],
      familyType: ['All'],
      minChildrenAge: [null, [Validators.min(0), Validators.max(18)]],
      maxChildrenAge: [null, [Validators.min(0), Validators.max(18)]],
      isActive: [true]
    });
  }

  loadCampaign(id: string): void {
    this.loading = true;
    this.seasonalAidService.getCampaignById(id).subscribe({
      next: campaign => {
        this.patchForm(campaign);
        this.loading = false;
      },
      error: () => {
        this.notification.error(this.translate.instant('seasonalAid.campaignLoadFailed'));
        this.loading = false;
        this.router.navigate(['/seasonal-aid']);
      }
    });
  }

  patchForm(campaign: SeasonalAidCampaign): void {
    this.campaignForm.patchValue({
      name: campaign.name,
      campaignType: campaign.campaignType,
      description: campaign.description,
      startDate: this.toDateInput(campaign.startDate),
      endDate: this.toDateInput(campaign.endDate),
      totalBudget: campaign.totalBudget,
      budgetCurrency: campaign.budgetCurrency,
      perFamilyAllocation: campaign.perFamilyAllocation,
      countryId: campaign.countryId,
      regionId: campaign.regionId,
      centerId: campaign.centerId,
      charityId: campaign.charityId,
      maximumFamilies: campaign.maximumFamilies,
      familyType: campaign.familyType,
      minChildrenAge: campaign.minChildrenAge,
      maxChildrenAge: campaign.maxChildrenAge,
      isActive: campaign.isActive
    });

    // Cascade: country → regions, region → centers (edit mode loads with values set)
    if (campaign.countryId) {
      this.loadRegions(campaign.countryId);
    }
    if (campaign.regionId) {
      this.loadCenters(campaign.regionId);
    }
  }

  /** ISO yyyy-MM-dd for date inputs. */
  private toDateInput(value: string): string {
    if (!value) return '';
    return new Date(value).toISOString().slice(0, 10);
  }

  private loadCountries(): void {
    this.lookupManagementService.getCountries({ page: 1, pageSize: 1000, isActive: true }).subscribe({
      next: response => {
        this.countryOptions = (response.items || []).map(c => ({
          id: c.id,
          name: c.nameAr || c.name
        }));
      },
      error: () => (this.countryOptions = [])
    });
  }

  private loadRegions(countryId: number): void {
    this.lookupManagementService.getRegionsByCountry(countryId).subscribe({
      next: regions => {
        this.regionOptions = (regions || []).map(r => ({
          id: r.id,
          name: r.nameAr || r.name
        }));
      },
      error: () => (this.regionOptions = [])
    });
  }

  private loadCenters(regionId: number): void {
    this.lookupManagementService.getCentersByRegion(regionId).subscribe({
      next: centers => {
        this.centerOptions = (centers || []).map(c => ({
          id: c.id,
          name: c.nameAr || c.name
        }));
      },
      error: () => (this.centerOptions = [])
    });
  }

  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true }).subscribe({
      next: response => {
        this.charityOptions = (response.items || []).map(c => ({
          id: c.id,
          name: c.name
        }));
      },
      error: () => (this.charityOptions = [])
    });
  }

  onCountryChange(countryId: number): void {
    this.campaignForm.patchValue({ regionId: null, centerId: null });
    this.regionOptions = [];
    this.centerOptions = [];
    if (countryId) {
      this.loadRegions(countryId);
    }
  }

  onRegionChange(regionId: number): void {
    this.campaignForm.patchValue({ centerId: null });
    this.centerOptions = [];
    if (regionId) {
      this.loadCenters(regionId);
    }
  }

  onSubmit(): void {
    if (this.campaignForm.invalid) {
      this.markFormGroupTouched(this.campaignForm);
      this.notification.error(this.translate.instant('validation.fixErrors'));
      return;
    }

    this.saving = true;
    const formValue = this.campaignForm.value;

    const request: CreateSeasonalAidCampaignRequest = {
      name: formValue.name,
      campaignType: formValue.campaignType,
      description: formValue.description || null,
      startDate: formValue.startDate,
      endDate: formValue.endDate,
      totalBudget: formValue.totalBudget,
      budgetCurrency: formValue.budgetCurrency,
      perFamilyAllocation: formValue.perFamilyAllocation,
      countryId: formValue.countryId || null,
      regionId: formValue.regionId || null,
      centerId: formValue.centerId || null,
      charityId: formValue.charityId || null,
      maximumFamilies: formValue.maximumFamilies || null,
      familyType: formValue.familyType || null,
      minChildrenAge: formValue.minChildrenAge ?? null,
      maxChildrenAge: formValue.maxChildrenAge ?? null,
      isActive: !!formValue.isActive
    };

    const operation = this.isEditMode && this.campaignId
      ? this.seasonalAidService.updateCampaign(this.campaignId, { ...request, id: this.campaignId } as UpdateSeasonalAidCampaignRequest)
      : this.seasonalAidService.createCampaign(request);

    operation.subscribe({
      next: () => {
        this.notification.success(
          this.isEditMode
            ? this.translate.instant('seasonalAid.campaignUpdated')
            : this.translate.instant('seasonalAid.campaignCreated')
        );
        this.saving = false;
        this.router.navigate(['/seasonal-aid']);
      },
      error: () => {
        this.notification.error(
          this.isEditMode
            ? this.translate.instant('seasonalAid.campaignUpdateFailed')
            : this.translate.instant('seasonalAid.campaignCreateFailed')
        );
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/seasonal-aid']);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.campaignForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }
}
