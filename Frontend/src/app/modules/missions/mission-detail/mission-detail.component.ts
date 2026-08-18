/**
 * Mission Detail Component
 * Displays complete mission profile in read-only mode
 * Implements UC-8.11 (View Mission Details)
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { MissionService } from '../services/mission.service';
import { Mission } from '../models/mission.model';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';

@Component({
  selector: 'app-mission-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule, BreadcrumbComponent],
  templateUrl: './mission-detail.component.html',
  styleUrls: ['./mission-detail.component.scss']
})
export class MissionDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'missions.title', url: '/missions' },
    { label: 'missions.details' }
  ];

  mission: Mission | null = null;
  loading = false;
  error: string | null = null;

  constructor(
    private missionService: MissionService,
    private route: ActivatedRoute,
    private router: Router,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadMission();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Load mission details
   */
  loadMission(): void {
    const id = this.route.snapshot.params['id'];
    if (!id) {
      this.error = this.translate.instant('errors.notFound');
      return;
    }

    this.loading = true;

    this.missionService.getMissionById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (mission) => {
          this.mission = mission;
          this.loading = false;
        },
        error: () => {
          this.error = this.translate.instant('errors.serverError');
          this.loading = false;
        }
      });
  }

  /**
   * Navigate to edit mission
   */
  editMission(): void {
    if (this.mission && !this.mission.isMissionCompleted) {
      this.router.navigate(['/missions', this.mission.id, 'edit']);
    }
  }

  /**
   * Navigate back to list
   */
  goBack(): void {
    this.router.navigate(['/missions']);
  }

  /**
   * Mark mission as completed
   */
  markAsCompleted(): void {
    if (!this.mission) return;

    const confirmed = confirm(this.translate.instant('missions.confirmComplete'));
    if (!confirmed) return;

    this.missionService.markAsCompleted(this.mission.id, {})
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadMission();
        },
        error: () => {
          // Error handling
        }
      });
  }

  /**
   * Check if mission can be edited
   */
  canEdit(): boolean {
    return this.mission !== null && !this.mission.isMissionCompleted;
  }

  /**
   * Check if mission can be marked as completed
   */
  canMarkCompleted(): boolean {
    return this.mission !== null && !this.mission.isMissionCompleted;
  }

  /**
   * Get status class
   */
  getStatusClass(): string {
    if (!this.mission) return '';

    if (this.mission.isMissionCompleted) return 'status-completed';
    if (this.isOverdue()) return 'status-overdue';
    return 'status-pending';
  }

  /**
   * Get status text
   */
  getStatusText(): string {
    if (!this.mission) return '';

    if (this.mission.isMissionCompleted) return this.translate.instant('missions.completed');
    if (this.isOverdue()) return this.translate.instant('missions.overdue');
    return this.translate.instant('missions.pending');
  }

  /**
   * Check if mission is overdue
   */
  private isOverdue(): boolean {
    if (!this.mission || this.mission.isMissionCompleted) return false;
    const missionDate = new Date(this.mission.missionDate);
    return missionDate < new Date();
  }

  /**
   * Format date for display
   */
  formatDate(date: string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  /**
   * Check if section has content
   */
  hasSectionContent(...fields: (string | undefined)[]): boolean {
    return fields.some(field => field && field.trim().length > 0);
  }

  /**
   * Get mission time type name (fallback if not provided)
   */
  getMissionTimeTypeName(): string {
    return this.mission?.missionTimeTypeName || '-';
  }

  /**
   * Get mission type name (fallback if not provided)
   */
  getMissionTypeName(): string {
    return this.mission?.missionTypeName || '-';
  }
}
