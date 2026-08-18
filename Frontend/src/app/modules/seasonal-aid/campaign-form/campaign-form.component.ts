import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import { NotificationService } from '../../../core/services/notification.service';
import { TranslateService } from '@ngx-translate/core';
import {
  SeasonalCampaign,
  CampaignType,
  Currency,
  FamilyType
} from '../models/seasonal-aid.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';

@Component({
  selector: 'app-campaign-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent, BreadcrumbComponent, SharedModule],
  templateUrl: './campaign-form.component.html',
  styleUrls: ['./campaign-form.component.scss']
})
export class CampaignFormComponent implements OnInit {
  // Breadcrumb items
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
  isEditMode: boolean = false;
  campaignId: string | null = null;
  loading: boolean = false;
  saving: boolean = false;

  campaignTypes = Object.values(CampaignType);
  currencies = Object.values(Currency);
  familyTypes = Object.values(FamilyType);

  countries: string[] = ['Egypt', 'Saudi Arabia', 'United Arab Emirates', 'Qatar', 'Kuwait'];
  charities: { id: string; name: string }[] = [];

  // Page actions for header
  pageActions = [
    {
      label: 'common.cancel',
      type: 'secondary',
      icon: 'x',
      click: () => this.onCancel()
    }
  ];

  // Dropdown options getters
  get campaignTypeOptions() {
    return this.campaignTypes.map(type => ({
      id: type,
      name: this.getCampaignTypeLabel(type)
    }));
  }

  get currencyOptions() {
    return this.currencies.map(currency => ({
      id: currency,
      name: currency
    }));
  }

  get familyTypeOptions() {
    return this.familyTypes.map(type => ({
      id: type,
      name: this.getFamilyTypeLabel(type)
    }));
  }

  get countryOptions() {
    return this.countries.map(country => ({
      id: country,
      name: country
    }));
  }

  get charityOptions() {
    return this.charities.map(charity => ({
      id: charity.id,
      name: charity.name
    }));
  }

  getCampaignTypeLabel(type: CampaignType): string {
    const key = `seasonalAid.campaignTypes.${type}`;
    return this.translate.instant(key);
  }

  getFamilyTypeLabel(type: FamilyType): string {
    const key = `seasonalAid.familyTypes.${type}`;
    return this.translate.instant(key);
  }

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private seasonalAidService: SeasonalAidService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.campaignForm = this.createForm();
  }

  ngOnInit(): void {
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
      campaignName: ['', [Validators.required, Validators.maxLength(200)]],
      campaignType: [CampaignType.Ramadan, Validators.required],
      description: ['', [Validators.required, Validators.maxLength(1000)]],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      totalBudget: [0, [Validators.required, Validators.min(0)]],
      budgetCurrency: [Currency.EGP, Validators.required],
      perFamilyAllocation: [0, [Validators.required, Validators.min(0)]],
      country: ['', Validators.required],
      regions: ['', Validators.required],
      centers: ['', Validators.required],
      assignedCharityId: ['', Validators.required],
      maximumFamilies: [null, [Validators.required, Validators.min(1)]],
      familyType: [FamilyType.All, Validators.required],
      ageRangeFrom: [null, [Validators.required, Validators.min(0)]],
      ageRangeTo: [null, [Validators.required, Validators.min(0)]],
      isActive: [true], // Boolean field, not required
      isClosed: [false] // Boolean field, not required
    });
  }

  loadCampaign(id: string): void {
    this.loading = true;
    this.seasonalAidService.getCampaignById(id).subscribe({
      next: (campaign) => {
        this.patchForm(campaign);
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading campaign:', error);
        this.notification.error(`Failed to load campaign: ${error.message || 'Unknown error'}`);
        this.loading = false;
        this.router.navigate(['/seasonal-aid']);
      }
    });
  }

  patchForm(campaign: SeasonalCampaign): void {
    this.campaignForm.patchValue({
      campaignName: campaign.campaignName,
      campaignType: campaign.campaignType,
      description: campaign.description,
      startDate: new Date(campaign.startDate),
      endDate: new Date(campaign.endDate),
      totalBudget: campaign.totalBudget,
      budgetCurrency: campaign.budgetCurrency,
      perFamilyAllocation: campaign.perFamilyAllocation,
      country: campaign.country,
      regions: (campaign.regions && campaign.regions.length > 0) ? campaign.regions.join(', ') : '',
      centers: (campaign.centers && campaign.centers.length > 0) ? campaign.centers.join(', ') : '',
      assignedCharityId: campaign.assignedCharityId || '',
      maximumFamilies: campaign.maximumFamilies,
      familyType: campaign.familyType || FamilyType.All,
      ageRangeFrom: campaign.ageRangeFrom,
      ageRangeTo: campaign.ageRangeTo,
      isActive: campaign.isActive,
      isClosed: campaign.isClosed
    });
  }

  onSubmit(): void {
    if (this.campaignForm.invalid) {
      this.markFormGroupTouched(this.campaignForm);
      this.notification.error(this.translate.instant('validation.fixErrors'));
      return;
    }

    this.saving = true;
    const formValue = this.campaignForm.value;

    // Convert comma-separated regions/centers to arrays
    const regions = formValue.regions
      ? formValue.regions.split(',').map((r: string) => r.trim()).filter((r: string) => r)
      : [];

    const centers = formValue.centers
      ? formValue.centers.split(',').map((c: string) => c.trim()).filter((c: string) => c)
      : [];

    const campaign: SeasonalCampaign = {
      ...formValue,
      regions,
      centers,
      id: this.isEditMode && this.campaignId ? this.campaignId : '',
      createdDate: this.isEditMode ? new Date() : new Date(),
      createdBy: 'Current User',
      modifiedDate: this.isEditMode ? new Date() : undefined,
      modifiedBy: this.isEditMode ? 'Current User' : undefined
    };

    const operation = this.isEditMode
      ? this.seasonalAidService.updateCampaign(this.campaignId!, campaign)
      : this.seasonalAidService.createCampaign(campaign);

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
      error: (error: any) => {
        console.error('Error saving campaign:', error);
        this.notification.error(
          `Failed to save campaign: ${error.message || 'Unknown error'}`
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

  getErrorMessage(fieldName: string): string {
    const field = this.campaignForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['required']) {
      return `${fieldName} is required`;
    }
    if (field.errors['minlength']) {
      return `Minimum length is ${field.errors['minlength'].requiredLength}`;
    }
    if (field.errors['maxlength']) {
      return `Maximum length is ${field.errors['maxlength'].requiredLength}`;
    }
    if (field.errors['min']) {
      return `Minimum value is ${field.errors['min'].min}`;
    }
    if (field.errors['email']) {
      return 'Invalid email format';
    }

    return 'Invalid field';
  }
}
