/**
 * Schedule Recurring Report Component
 * Implements UC-6.8: Schedule Recurring Report
 *
 * Configure automatic report generation on periodic basis with:
 * - Report Name
 * - Frequency (Monthly, Quarterly, Annually)
 * - Day of Month
 * - Email Report To (comma-separated emails)
 * - Filters (same as ad-hoc report)
 *
 * Access: Charity, Admin, Super Admin
 */

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, EmailValidator } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { OrphanReportService } from '../services/orphan-report.service';
import {
  ScheduleRecurringReportDto,
  ScheduledReportDto,
  OrphanReportFilterDto
} from '../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';

import { BreadcrumbComponent } from '../../../shared/components/breadcrumb/breadcrumb.component';
@Component({
  selector: 'app-schedule-report',
  standalone: true,
  imports: [
    BreadcrumbComponent,

    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    LoadingComponent
  ],
  templateUrl: './schedule-report.component.html',
  styleUrls: ['./schedule-report.component.scss']
})
export class ScheduleReportComponent implements OnInit {
  // Form - UC-6.8
  scheduleForm: FormGroup;

  // Data
  loading = false;
  saving = false;
  scheduledReports: ScheduledReportDto[] = [];

  // Options
  frequencies = [
    { value: 'Monthly', label: 'orphanReports.monthly' },
    { value: 'Quarterly', label: 'orphanReports.quarterly' },
    { value: 'Annually', label: 'orphanReports.annually' }
  ];

  // Days of month (1-31)
  daysOfMonth: number[] = Array.from({ length: 31 }, (_, i) => i + 1);

  // User role
  isAdminOrSuperAdmin = false;

  constructor(
    private fb: FormBuilder,
    private orphanReportService: OrphanReportService
  ) {
    this.scheduleForm = this.buildForm();
  }

  ngOnInit(): void {
    this.loadScheduledReports();
  }

  /**
   * Build schedule form - UC-6.8
   */
  private buildForm(): FormGroup {
    return this.fb.group({
      // Basic Information - UC-6.8
      reportName: ['', Validators.required],
      frequency: ['Monthly', Validators.required],
      dayOfMonth: [1, Validators.required],
      emailRecipients: ['', [Validators.required, Validators.email]],
      active: [true],

      // Filters (same as ad-hoc report) - UC-6.8
      filters: this.fb.group({
        fromDate: [''],
        toDate: [''],
        charityId: [''],
        regionId: [''],
        centerId: [''],
        sponsorshipStatus: ['All'],
        ageFrom: [''],
        ageTo: [''],
        gender: ['All'],
        includeFamilyDetails: [false],
        includeContactInformation: [false],
        includeEducationDetails: [false],
        includeHealthDetails: [false],
        groupByCharity: [false]
      })
    });
  }

  /**
   * Load scheduled reports - UC-6.8
   */
  private loadScheduledReports(): void {
    this.loading = true;

    this.orphanReportService.getScheduledReports(1, 50).subscribe({
      next: (result) => {
        this.scheduledReports = result.items;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading scheduled reports:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Save schedule - UC-6.8
   */
  saveSchedule(): void {
    if (this.scheduleForm.invalid) {
      this.scheduleForm.markAllAsTouched();
      return;
    }

    // Validate day of month based on frequency
    const frequency = this.scheduleForm.value.frequency;
    const dayOfMonth = this.scheduleForm.value.dayOfMonth;

    if (frequency === 'Monthly' && (dayOfMonth < 1 || dayOfMonth > 31)) {
      alert('For monthly frequency, day must be between 1 and 31');
      return;
    }

    this.saving = true;

    const schedule: ScheduleRecurringReportDto = {
      reportName: this.scheduleForm.value.reportName,
      frequency: this.scheduleForm.value.frequency,
      dayOfMonth: this.scheduleForm.value.dayOfMonth,
      emailRecipients: this.scheduleForm.value.emailRecipients,
      filter: this.scheduleForm.value.filters as OrphanReportFilterDto
    };

    this.orphanReportService.scheduleRecurringReport(schedule).subscribe({
      next: () => {
        this.saving = false;
        this.scheduleForm.reset();
        this.loadScheduledReports();
      },
      error: (error) => {
        console.error('Error saving schedule:', error);
        this.saving = false;
      }
    });
  }

  /**
   * Delete scheduled report
   */
  deleteSchedule(scheduleId: string, reportName: string): void {
    if (confirm(`Are you sure you want to delete scheduled report "${reportName}"?`)) {
      this.orphanReportService.deleteScheduledReport(scheduleId).subscribe({
        next: () => {
          this.loadScheduledReports();
        },
        error: (error) => {
          console.error('Error deleting schedule:', error);
        }
      });
    }
  }

  /**
   * Toggle active status
   */
  toggleActive(schedule: ScheduledReportDto): void {
    // TODO: Implement toggle active functionality
    console.log('Toggle active:', schedule);
  }

  /**
   * Get frequency label
   */
  getFrequencyLabel(frequency: string): string {
    return `orphanReports.${frequency.toLowerCase()}`; // For translate
  }

  /**
   * Get next run date display
   */
  getNextRunDisplay(schedule: ScheduledReportDto): string {
    if (schedule.nextRunDate) {
      return new Date(schedule.nextRunDate).toLocaleDateString();
    }
    return '-';
  }

  /**
   * Format email recipients for display
   */
  formatEmails(emails: string): string {
    const emailList = emails.split(',').map(e => e.trim());
    if (emailList.length > 2) {
      return `${emailList[0]}, ${emailList[1]} +${emailList.length - 2} more`;
    }
    return emails;
  }

  /**
   * Check if day of month should be shown
   */
  get showDayOfMonth(): boolean {
    return this.scheduleForm.value.frequency === 'Monthly';
  }
}
