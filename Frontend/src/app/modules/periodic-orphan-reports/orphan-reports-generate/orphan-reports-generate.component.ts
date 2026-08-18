/**
 * Orphan Reports Generate Component
 * Implements UC-6.1: Generate Orphan Report
 *
 * Provides report generation form with filters for:
 * - Period (From/To dates)
 * - Charity (Admin/Super Admin only)
 * - Region
 * - Center
 * - Sponsorship Status
 * - Age Range
 * - Gender
 * - Report Options (Family Details, Contact, Education, Health)
 */

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { OrphanReportService } from '../../services/orphan-report.service';
import {
  OrphanReportFilterDto,
  OrphanReportResultDto,
  OrphanReportExportDto
} from '../../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';

@Component({
  selector: 'app-orphan-reports-generate',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    LoadingComponent
  ],
  templateUrl: './orphan-reports-generate.component.html',
  styleUrls: ['./orphan-reports-generate.component.scss']
})
export class OrphanReportsGenerateComponent implements OnInit {
  // Form - UC-6.1
  reportForm: FormGroup;

  // Data
  loading = false;
  generating = false;
  reportResult?: OrphanReportResultDto;

  // Options (would be populated from services)
  charities: any[] = [];
  regions: any[] = [];
  centers: any[] = [];
  sponsorshipStatuses = ['Sponsored', 'Unsponsored', 'Pending', 'All'];
  genders = ['Male', 'Female', 'All'];

  // User role for conditional filters
  userRole: string = 'Charity'; // Would come from auth service
  isAdminOrSuperAdmin = false;

  constructor(
    private fb: FormBuilder,
    private orphanReportService: OrphanReportService
  ) {
    this.reportForm = this.buildForm();
    this.isAdminOrSuperAdmin = this.userRole === 'Admin' || this.userRole === 'SuperAdmin';
  }

  ngOnInit(): void {
    // Load lookup data
    this.loadLookupData();
  }

  /**
   * Build report filter form - UC-6.1
   */
  private buildForm(): FormGroup {
    return this.fb.group({
      // Period - UC-6.2
      fromDate: ['', Validators.required],
      toDate: ['', Validators.required],

      // Filters - UC-6.3 through UC-6.5
      charityId: [''],
      regionId: [''],
      centerId: [''],
      sponsorshipStatus: ['All'],
      ageFrom: [''],
      ageTo: [''],
      gender: ['All'],

      // Report Options - UC-6.6
      includeFamilyDetails: [false],
      includeContactInformation: [false],
      includeEducationDetails: [false],
      includeHealthDetails: [false],
      groupByCharity: [false] // Admin/Super Admin only
    });
  }

  /**
   * Load lookup data for dropdowns
   */
  private loadLookupData(): void {
    // TODO: Load from appropriate services
    // This.charities = this.charityService.getAll();
    // This.regions = this.lookupService.getRegions();
    // This.centers = this.lookupService.getCenters();
  }

  /**
   * Generate report - UC-6.1
   */
  generateReport(): void {
    if (this.reportForm.invalid) {
      this.reportForm.markAllAsTouched();
      return;
    }

    // Validate date range - UC-6.1 step 7
    const fromDate = this.reportForm.value.fromDate;
    const toDate = this.reportForm.value.toDate;

    if (new Date(fromDate) > new Date(toDate)) {
      alert('From date must be before To date');
      return;
    }

    this.generating = true;

    const filter: OrphanReportFilterDto = this.reportForm.value;

    // For Charity users, add their charity filter automatically - UC-6.1 step 8
    if (!this.isAdminOrSuperAdmin) {
      // filter.charityId = this.currentUser.charityId; // Would come from auth
    }

    this.orphanReportService.generateReport(filter).subscribe({
      next: (result: OrphanReportResultDto) => {
        this.reportResult = result;
        this.generating = false;
      },
      error: (error) => {
        console.error('Error generating report:', error);
        this.generating = false;
      }
    });
  }

  /**
   * Export report - UC-6.7
   */
  exportReport(format: 'excel' | 'pdf'): void {
    if (!this.reportResult) {
      return;
    }

    const filter: OrphanReportFilterDto = this.reportForm.value;
    const exportOptions: OrphanReportExportDto = {
      exportFormat: format,
      fileName: `OrphanReport_${this.reportForm.value.fromDate}_to_${this.reportForm.value.toDate}`
    };

    this.orphanReportService.exportReport(filter, exportOptions).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, exportOptions.fileName || `OrphanReport.${format === 'excel' ? 'xlsx' : 'pdf'}`);
      },
      error: (error) => {
        console.error('Error exporting report:', error);
      }
    });
  }

  /**
   * Download file helper
   */
  private downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }

  /**
   * Clear filters
   */
  clearFilters(): void {
    this.reportForm.reset({
      fromDate: '',
      toDate: '',
      charityId: '',
      regionId: '',
      centerId: '',
      sponsorshipStatus: 'All',
      ageFrom: '',
      ageTo: '',
      gender: 'All',
      includeFamilyDetails: false,
      includeContactInformation: false,
      includeEducationDetails: false,
      includeHealthDetails: false,
      groupByCharity: false
    });
    this.reportResult = undefined;
  }

  /**
   * Check if charity filter should be shown
   */
  get showCharityFilter(): boolean {
    return this.isAdminOrSuperAdmin; // UC-6.4: Only Admin/Super Admin
  }

  /**
   * Check if group by charity should be shown
   */
  get showGroupByCharity(): boolean {
    return this.isAdminOrSuperAdmin; // UC-6.1: Only Admin/Super Admin
  }

  /**
   * Get sponsorship status label
   */
  getSponsorshipStatusLabel(status: string): string {
    return `orphanReports.sponsorshipStatus.${status.toLowerCase()}`;
  }

  /**
   * Get gender label
   */
  getGenderLabel(gender: string): string {
    return `orphanReports.gender.${gender.toLowerCase()}`;
  }
}
