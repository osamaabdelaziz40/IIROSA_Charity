/**
 * Housing Project Detail Component
 * Displays detailed information about a housing project
 * Access: Admin and Super Admin only
 */

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { HousingProjectService } from '../services/housing-project.service';
import {
  HousingProject,
  HousingType,
  ProjectStatus,
  getProjectTypeName,
  getHousingTypeName,
  getProjectStatusName,
  getProjectStageName,
  getStatusBadgeClass,
  isProjectCompleted
} from '../models/housing-project.model';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';

@Component({
  selector: 'app-housing-project-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    TranslateModule,
    BreadcrumbComponent
  ],
  templateUrl: './housing-project-detail.component.html',
  styleUrls: ['./housing-project-detail.component.scss']
})
export class HousingProjectDetailComponent implements OnInit {
  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'housingProjects.title', url: '/housing-projects' },
    { label: 'housingProjects.projectDetails' }
  ];
  // Data
  housingProject: HousingProject | null = null;
  loading: boolean = false;
  projectId: string = '';

  // Progress Update Form
  showProgressForm: boolean = false;
  progressNotes: string = '';
  updatingProgress: boolean = false;

  // Completion Form
  showCompletionForm: boolean = false;
  completionNotes: string = '';
  finalCost: number | null = null;
  completing: boolean = false;

  // Tabs
  activeTab: string = 'overview';

  // Helpers
  getProjectTypeName = getProjectTypeName;
  getHousingTypeName = getHousingTypeName;
  getProjectStatusName = getProjectStatusName;
  getProjectStageName = getProjectStageName;
  getStatusBadgeClass = getStatusBadgeClass;
  isProjectCompleted = isProjectCompleted;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private housingProjectService: HousingProjectService
  ) {}

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id') || '';
    if (this.projectId) {
      this.loadHousingProject();
    } else {
      this.router.navigate(['/housing-projects']);
    }
  }

  /**
   * Load housing project details
   */
  loadHousingProject(): void {
    this.loading = true;

    this.housingProjectService.getHousingProjectById(this.projectId).subscribe({
      next: (project) => {
        this.housingProject = project;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading housing project:', error);
        this.loading = false;
        this.router.navigate(['/housing-projects']);
      }
    });
  }

  /**
   * Toggle progress form
   */
  toggleProgressForm(): void {
    this.showProgressForm = !this.showProgressForm;
    if (!this.showProgressForm) {
      this.progressNotes = '';
    }
  }

  /**
   * Update project progress
   */
  updateProgress(): void {
    if (!this.housingProject) return;

    this.updatingProgress = true;

    this.housingProjectService.updateProgress(this.projectId, {
      projectStatus: this.housingProject.projectStatus,
      completionPercentage: this.housingProject.completionPercentage,
      currentStage: this.housingProject.currentStage,
      notes: this.progressNotes
    }).subscribe({
      next: () => {
        this.progressNotes = '';
        this.showProgressForm = false;
        this.loadHousingProject();
        this.updatingProgress = false;
      },
      error: (error) => {
        console.error('Error updating progress:', error);
        this.updatingProgress = false;
      }
    });
  }

  /**
   * Toggle completion form
   */
  toggleCompletionForm(): void {
    this.showCompletionForm = !this.showCompletionForm;
    if (!this.showCompletionForm) {
      this.completionNotes = '';
      this.finalCost = null;
    }
  }

  /**
   * Mark project as completed
   */
  markAsCompleted(): void {
    if (!this.housingProject) return;

    this.completing = true;

    this.housingProjectService.markAsCompleted(this.projectId, {
      actualEndDate: new Date().toISOString(),
      finalCost: this.finalCost || undefined,
      completionNotes: this.completionNotes
    }).subscribe({
      next: () => {
        this.completionNotes = '';
        this.finalCost = null;
        this.showCompletionForm = false;
        this.loadHousingProject();
        this.completing = false;
      },
      error: (error) => {
        console.error('Error marking project as completed:', error);
        this.completing = false;
      }
    });
  }

  /**
   * Change active tab
   */
  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  /**
   * Format date for display
   */
  formatDate(date: string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString();
  }

  /**
   * Format currency for display
   */
  formatCurrency(amount: number | undefined, currency: string): string {
    if (amount === undefined || amount === null) return '-';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency
    }).format(amount);
  }

  /**
   * Get progress bar color based on completion percentage
   */
  getProgressBarColor(): string {
    if (!this.housingProject) return 'bg-primary';

    const percentage = this.housingProject.completionPercentage;
    if (percentage >= 100) return 'bg-success';
    if (percentage >= 75) return 'bg-info';
    if (percentage >= 50) return 'bg-primary';
    if (percentage >= 25) return 'bg-warning';
    return 'bg-danger';
  }
}
