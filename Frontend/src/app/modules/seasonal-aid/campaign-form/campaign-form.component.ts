import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
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
export class CampaignFormComponent implements OnInit, OnDestroy {
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

  // Lookup dropdown data (LookupBase format for the shared drop-down component).
  // Region/center lists are children of the selected parent — the cascade mirrors
  // office-development-projects.
  countryOptions: LookupBase[] = [];
  regionOptions: LookupBase[] = [];
  centerOptions: LookupBase[] = [];

  campaignTypeOptions: LookupBase[] = [];

  currencyOptions: LookupBase[] = [
    { id: 'EGP', name: 'EGP' },
    { id: 'SAR', name: 'SAR' },
    { id: 'USD', name: 'USD' }
  ];

  familyTypeOptions: LookupBase[] = [];

  // Page actions for header
  pageActions = [
    {
      label: 'common.cancel',
      type: 'secondary',
      icon: 'fe fe-x',
      click: () => this.onCancel()
    }
  ];

  private langChangeSubscription: any;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private seasonalAidService: SeasonalAidService,
    private lookupManagementService: LookupManagementService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.campaignForm = this.createForm();
  }

  ngOnInit(): void {
    // The drop-down component renders option names raw, so translated values are
    // resolved here and re-resolved whenever the language flips.
    this.buildTranslatedOptions();
    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.buildTranslatedOptions();
    });

    this.loadCountries();

    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.campaignId = params['id'];
        this.loadCampaign(params['id']);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
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
      maximumFamilies: [null, Validators.min(1)],
      familyType: ['All'],
      minChildrenAge: [null, [Validators.min(0), Validators.max(18)]],
      maxChildrenAge: [null, [Validators.min(0), Validators.max(18)]],
      isActive: [true]
    });
  }

  /** Campaign/family-type labels resolved through the translator — the ids stay the stored values. */
  private buildTranslatedOptions(): void {
    this.campaignTypeOptions = (['Ramadan', 'EidAlFitr', 'EidAlAdha', 'Winter', 'SchoolSupplies', 'Other'] as const)
      .map(id => ({ id, name: this.translate.instant(`seasonalAid.campaignTypes.${id}`) }));

    const familyTypeKeys: Record<string, string> = {
      'All': 'All',
      'Orphan Families': 'OrphanFamilies',
      'Needy Families': 'NeedyFamilies'
    };
    this.familyTypeOptions = Object.keys(familyTypeKeys)
      .map(id => ({ id, name: this.translate.instant(`seasonalAid.familyTypes.${familyTypeKeys[id]}`) }));
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

  // Drop-down handlers — the component emits the selected ITEM, not its id
  // (same contract as office-development-projects).
  onCountryDropDownChanged(value: any): void {
    if (value && value.id !== undefined && value.id !== null) {
      const countryId = typeof value.id === 'string' ? parseInt(value.id, 10) : value.id;

      this.campaignForm.get('countryId')?.setValue(countryId, { emitEvent: false });
      this.campaignForm.patchValue({ regionId: null, centerId: null });
      this.centerOptions = [];

      this.loadRegions(countryId);
    } else {
      // Country cleared — the whole cascade resets
      this.campaignForm.patchValue({ countryId: null, regionId: null, centerId: null });
      this.regionOptions = [];
      this.centerOptions = [];
    }
  }

  onRegionDropDownChanged(value: any): void {
    if (value && value.id !== undefined && value.id !== null) {
      const regionId = typeof value.id === 'string' ? parseInt(value.id, 10) : value.id;

      this.campaignForm.get('regionId')?.setValue(regionId, { emitEvent: false });
      this.campaignForm.patchValue({ centerId: null });

      this.loadCenters(regionId);
    } else {
      this.campaignForm.patchValue({ regionId: null, centerId: null });
      this.centerOptions = [];
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
