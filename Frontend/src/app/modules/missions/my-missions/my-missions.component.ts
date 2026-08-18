/**
 * My Missions Component
 * Displays missions assigned to the current user
 * Implements UC-8.12 (View My Missions)
 * Extends MissionListComponent with pre-filter for current user
 */

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { MissionService } from '../services/mission.service';
import { MissionListComponent } from '../mission-list/mission-list.component';
import { MissionSearchRequest } from '../models/mission.model';

@Component({
  selector: 'app-my-missions',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, MissionListComponent],
  templateUrl: './my-missions.component.html',
  styleUrls: ['./my-missions.component.scss']
})
export class MyMissionsComponent extends MissionListComponent implements OnInit {
  constructor(
    missionService: MissionService,
    router: Router,
    translate: TranslateService
  ) {
    super(missionService, router, translate);
  }

  override ngOnInit(): void {
    // Always show my missions only
    this.showMyMissionsOnly = true;
    this.loadMissions();
    this.loadStatusCounts();
  }

  /**
   * Override to use getMyMissions endpoint
   */
  override loadMissions(): void {
    this.loading = true;

    const request: MissionSearchRequest = {
      search: this.search || undefined,
      missionTypeId: this.selectedMissionType,
      missionTimeTypeId: this.selectedMissionTimeType,
      isCompleted: this.selectedStatus === 'all' ? undefined : this.selectedStatus === 'completed',
      countryId: this.selectedCountry,
      regionId: this.selectedRegion,
      centerId: this.selectedCenter,
      dateFrom: this.dateFrom,
      dateTo: this.dateTo,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.missionService.getMyMissions(request).subscribe({
      next: (response) => {
        this.missions = response.items;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  /**
   * Prevent toggling my missions in this view
   */
  override toggleMyMissions(): void {
    // Always stay in my missions view
    this.showMyMissionsOnly = true;
  }

  /**
   * Override status counts for my missions only
   */
  override loadStatusCounts(): void {
    // For my missions, we might want different counts
    // For now, using the same endpoint
    super.loadStatusCounts();
  }

  /**
   * Get page title
   */
  get pageTitle(): string {
    return 'missions.myMissions';
  }

  /**
   * Get page description
   */
  get pageDescription(): string {
    return 'missions.myMissionsDescription';
  }
}
