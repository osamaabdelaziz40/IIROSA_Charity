import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { PeriodicOrphanReportService } from '../../services/periodic-orphan-report.service';
import {
  CreatePeriodicOrphanReportDto,
  UpdatePeriodicOrphanReportDto,
  PeriodicOrphanReportDto
} from '../../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';

@Component({
  selector: 'app-periodic-report-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    LoadingComponent
  ],
  templateUrl: './periodic-report-form.component.html',
  styleUrls: ['./periodic-report-form.component.scss']
})
export class PeriodicReportFormComponent implements OnInit {
  reportForm: FormGroup;
  loading = false;
  isEditMode = false;
  reportId?: string;
  saving = false;

  // Orphans list (would be populated from service)
  orphans: any[] = [];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private periodicReportService: PeriodicOrphanReportService
  ) {
    this.reportForm = this.buildForm();
  }

  ngOnInit(): void {
    this.reportId = this.route.snapshot.params['id'];
    this.isEditMode = !!this.reportId;

    if (this.isEditMode && this.reportId) {
      this.loadReport(this.reportId);
    }
  }

  /**
   * Build the form with all sections from UC-6.11
   */
  private buildForm(): FormGroup {
    return this.fb.group({
      // Basic Information
      orphanId: ['', Validators.required],
      orphanPaymentId: [''],
      reportDate: [new Date().toISOString().split('T')[0], Validators.required],
      reportPeriodFrom: [''],
      reportPeriodTo: [''],
      reportNo: [''],

      // Religious & Behavioral (UC-6.11)
      prayerStatus: [''],
      mannersStatus: [''],
      hadeethStatus: [''],

      // Quran Education (UC-6.11)
      quranParts: [''],
      quranVerses: [''],

      // Health & Medical (UC-6.11)
      medicalStatus: [''],
      disease: [''],
      disability: [''],
      disabilityDescription: [''],
      diseaseDescription: [''],
      medicalReportImageId: [''],

      // Personal Development (UC-6.11)
      hobby: [''],
      course: [''],
      courseName: [''],
      sportName: [''],
      professionName: [''],
      achievement: [''],
      achievementArr: [''],
      wish: [''],
      wishArr: [''],
      orphanMessage: [''],

      // Education Details (UC-6.11)
      educationalStageId: [''],
      educationalLevelId: [''],
      grade: [''],
      school: [''],
      schoolType: [''],
      educationDegree: [''],
      highestEducationalLevel: [''],
      highestEducationalLevelYear: [''],
      isOrphanStudent: [false],
      educationalYear: [''],
      annualFeeForStudy: [''],
      studyingYears: [''],
      restStudyingYears: [''],
      graduationYear: [''],
      dropOut: [false],
      dropOutYear: [''],
      dropOutStageId: [''],
      faculty: [''],
      department: [''],
      specialization: [''],

      // Life Events (UC-6.11)
      married: [false],
      marriageDate: [''],
      orphanMarriageImageId: [''],
      dead: [false],
      deathDate: [''],
      orphanDeadImageId: [''],

      // Attachments (UC-6.11)
      orphanCertificateImageId: [''],
      orphanImageId: [''],
      missingDocuments: [false],
      missingDocumentsName: ['']
    });
  }

  /**
   * Load existing report for editing - UC-6.11
   */
  loadReport(id: string): void {
    this.loading = true;
    this.periodicReportService.getReport(id).subscribe({
      next: (report: PeriodicOrphanReportDto) => {
        this.patchForm(report);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading report:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Patch form with existing data
   */
  private patchForm(report: PeriodicOrphanReportDto): void {
    this.reportForm.patchValue({
      orphanId: report.orphanId,
      orphanPaymentId: report.orphanPaymentId,
      reportDate: report.reportDate.split('T')[0],
      reportPeriodFrom: report.reportPeriodFrom?.split('T')[0],
      reportPeriodTo: report.reportPeriodTo?.split('T')[0],
      reportNo: report.reportNo,
      prayerStatus: report.prayerStatus,
      mannersStatus: report.mannersStatus,
      hadeethStatus: report.hadeethStatus,
      quranParts: report.quranParts,
      quranVerses: report.quranVerses,
      medicalStatus: report.medicalStatus,
      disease: report.disease,
      disability: report.disability,
      disabilityDescription: report.disabilityDescription,
      diseaseDescription: report.diseaseDescription,
      medicalReportImageId: report.medicalReportImageId,
      hobby: report.hobby,
      course: report.course,
      courseName: report.courseName,
      sportName: report.sportName,
      professionName: report.professionName,
      achievement: report.achievement,
      achievementArr: report.achievementArr,
      wish: report.wish,
      wishArr: report.wishArr,
      orphanMessage: report.orphanMessage,
      educationalStageId: report.educationalStageId,
      educationalLevelId: report.educationalLevelId,
      grade: report.grade,
      school: report.school,
      schoolType: report.schoolType,
      educationDegree: report.educationDegree,
      highestEducationalLevel: report.highestEducationalLevel,
      highestEducationalLevelYear: report.highestEducationalLevelYear,
      isOrphanStudent: report.isOrphanStudent,
      educationalYear: report.educationalYear,
      annualFeeForStudy: report.annualFeeForStudy,
      studyingYears: report.studyingYears,
      restStudyingYears: report.restStudyingYears,
      graduationYear: report.graduationYear,
      dropOut: report.dropOut,
      dropOutYear: report.dropOutYear,
      dropOutStageId: report.dropOutStageId,
      faculty: report.faculty,
      department: report.department,
      specialization: report.specialization,
      married: report.married,
      marriageDate: report.marriageDate?.split('T')[0],
      orphanMarriageImageId: report.orphanMarriageImageId,
      dead: report.dead,
      deathDate: report.deathDate?.split('T')[0],
      orphanDeadImageId: report.orphanDeadImageId,
      orphanCertificateImageId: report.orphanCertificateImageId,
      orphanImageId: report.orphanImageId,
      missingDocuments: report.missingDocuments,
      missingDocumentsName: report.missingDocumentsName
    });
  }

  /**
   * Save report - UC-6.11
   */
  save(): void {
    if (this.reportForm.invalid) {
      this.reportForm.markAllAsTouched();
      return;
    }

    this.saving = true;

    if (this.isEditMode && this.reportId) {
      const updateDto: UpdatePeriodicOrphanReportDto = {
        ...this.reportForm.value,
        id: this.reportId
      };

      this.periodicReportService.updateReport(this.reportId, updateDto).subscribe({
        next: () => {
          this.saving = false;
          this.router.navigate(['../'], { relativeTo: this.route });
        },
        error: (error) => {
          console.error('Error updating report:', error);
          this.saving = false;
        }
      });
    } else {
      const createDto: CreatePeriodicOrphanReportDto = this.reportForm.value;

      this.periodicReportService.createReport(createDto).subscribe({
        next: () => {
          this.saving = false;
          this.router.navigate(['../'], { relativeTo: this.route });
        },
        error: (error) => {
          console.error('Error creating report:', error);
          this.saving = false;
        }
      });
    }
  }

  /**
   * Cancel and go back
   */
  cancel(): void {
    this.router.navigate(['../'], { relativeTo: this.route });
  }

  /**
   * Get form section title
   */
  getSectionTitle(section: string): string {
    return `periodicReports.form.sections.${section}`;
  }
}
