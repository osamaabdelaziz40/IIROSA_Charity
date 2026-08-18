import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, Params } from '@angular/router';
import {
  OfficeProject,
  BeneficiaryType
} from '../models/office-project.model';
import { OfficeProjectService } from '../services/office-project.service';
import { NotificationService } from '../../../core/services/notification.service';
import { TranslateModule } from '@ngx-translate/core';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule, BreadcrumbComponent],
  templateUrl: './project-detail.component.html',
  styleUrls: ['./project-detail.component.scss']
})
export class ProjectDetailComponent implements OnInit {
  project: OfficeProject | null = null;
  loading = false;
  error: string | null = null;

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'officeDevelopmentProjects.title', url: '/office-development-projects' },
    { label: 'officeDevelopmentProjects.projectDetails' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private projectService: OfficeProjectService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params: Params) => {
      const projectId = params['id'];
      if (projectId) {
        this.loadProject(projectId);
      }
    });
  }

  private loadProject(id: string): void {
    this.loading = true;
    this.error = null;

    this.projectService.getProjectById(id).subscribe({
      next: (project: OfficeProject) => {
        this.project = project;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading project:', error);
        this.error = `Failed to load project: ${error.message || 'Unknown error'}`;
        this.notification.error(this.error);
        this.loading = false;
      }
    });
  }

  onEdit(): void {
    if (this.project?.id) {
      this.router.navigate(['/office-development-projects', this.project.id, 'edit']);
    }
  }

  onDelete(): void {
    if (confirm('Are you sure you want to delete this project?')) {
      if (this.project?.id) {
        this.loading = true;
        this.projectService.deleteProject(this.project.id).subscribe({
          next: () => {
            this.notification.success('Project deleted successfully');
            this.router.navigate(['/office-development-projects']);
          },
          error: (error: any) => {
            console.error('Error deleting project:', error);
            this.notification.error(
              `Failed to delete project: ${error.message || 'Unknown error'}`
            );
            this.loading = false;
          }
        });
      }
    }
  }

  onMarkAsCompleted(): void {
    if (confirm('Are you sure you want to mark this project as completed?')) {
      const projectId = this.project?.id;
      if (projectId) {
        this.loading = true;
        this.projectService.markAsCompleted(projectId).subscribe({
          next: () => {
            this.notification.success('Project marked as completed');
            this.loadProject(projectId);
          },
          error: (error: any) => {
            console.error('Error marking project as completed:', error);
            this.notification.error(
              `Failed to mark project as completed: ${error.message || 'Unknown error'}`
            );
            this.loading = false;
          }
        });
      }
    }
  }

  onBack(): void {
    this.router.navigate(['/office-development-projects']);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString();
  }

  formatCurrency(amount: number | undefined): string {
    if (amount === undefined || amount === null) return '-';
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(amount);
  }

  getBeneficiaryTypeLabel(type: BeneficiaryType | string | undefined): string {
    if (!type) return '-';
    const typeStr = typeof type === 'string' ? type as BeneficiaryType : type;
    switch (typeStr) {
      case BeneficiaryType.Families:
        return 'officeDevelopmentProjects.beneficiaryTypeFamilies';
      case BeneficiaryType.Individuals:
        return 'officeDevelopmentProjects.beneficiaryTypeIndividuals';
      case BeneficiaryType.Both:
        return 'officeDevelopmentProjects.beneficiaryTypeBoth';
      default:
        return '-';
    }
  }

  canEdit(): boolean {
    return this.project !== null && !this.project.isFinished;
  }

  canMarkAsCompleted(): boolean {
    return this.project !== null && !this.project.isFinished;
  }

  downloadDocument(): void {
    if (this.project?.attachedFileId && this.project?.attachedFileName) {
      // TODO: Implement document download via attachment service
      this.notification.info('Document download feature will be implemented soon');
    }
  }

  downloadReport(): void {
    if (this.project?.projectReportFileId && this.project?.projectReportFileName) {
      // TODO: Implement report download via attachment service
      this.notification.info('Report download feature will be implemented soon');
    }
  }
}
