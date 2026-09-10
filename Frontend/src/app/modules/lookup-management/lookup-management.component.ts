import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LookupManagementService } from './services/lookup-management.service';
import { LookupTableSummaryDto } from './models/lookup.model';
import { PaginationComponent } from '../../shared/components';

@Component({
  selector: 'app-lookup-management',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, PaginationComponent],
  templateUrl: './lookup-management.component.html',
  styleUrls: ['./lookup-management.component.scss']
})
export class LookupManagementComponent implements OnInit {
  lookupTables: LookupTableSummaryDto[] = [];
  loading = false;

  currentPage = 1;
  readonly pageSize = 8;

  constructor(
    private lookupService: LookupManagementService,
    private translate: TranslateService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadLookupTablesSummary();
  }

  loadLookupTablesSummary(): void {
    this.loading = true;
    this.lookupService.getLookupTablesSummary().subscribe({
      next: (data) => {
        this.lookupTables = data;
        this.currentPage = 1;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading lookup tables:', error);
        this.loading = false;
      }
    });
  }

  exportLookupTable(tableName: string): void {
    const exportDto = {
      format: 'Excel' as const,
      includeInactive: false,
      language: 'Both' as const
    };

    this.lookupService.exportLookupTable(tableName, exportDto).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${tableName}_export_${new Date().toISOString().slice(0, 10)}.csv`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error exporting lookup table:', error);
      }
    });
  }

  getTableIcon(tableName: string): string {
    const icons: { [key: string]: string } = {
      'Countries': 'fe fe-globe',
      'Regions': 'fe fe-map',
      'Centers': 'fe fe-grid',
      'Departments': 'fe fe-users',
      'MissionTypes': 'fe fe-activity',
      'ProjectTypes': 'fe fe-folder',
      'Banks': 'fe fe-credit-card',
      'NGOTypes': 'fe fe-heart',
      'OfficeProjectTypes': 'fe fe-briefcase',
      'HousingBuildings': 'fe fe-home',
      'HousingFlats': 'fe fe-layout',
      'OutgoingCategories': 'fe fe-send',
      'HouseOwnerships': 'fe fe-key',
      'IncomeTypes': 'fe fe-dollar-sign',
      'FamilyProjectStatuses': 'fe fe-trending-up'
    };
    return icons[tableName] || 'fe fe-list';
  }

  /** Rotating accent colors for the icon circles, mirroring the contacts-grid avatars */
  getIconColorClass(index: number): string {
    const colors = ['bg-primary', 'bg-success', 'bg-info', 'bg-warning', 'bg-danger'];
    return colors[index % colors.length];
  }

  getStatusDotClass(table: LookupTableSummaryDto): string {
    if (table.itemCount === 0) return 'bg-secondary';
    if (table.inactiveItems > table.activeItems) return 'bg-warning';
    return 'bg-success';
  }

  navigateToTable(tableName: string): void {
    const routeMap: { [key: string]: string } = {
      'Countries': '/lookup-management/countries',
      'Regions': '/lookup-management/regions',
      'Centers': '/lookup-management/centers',
      'Departments': '/lookup-management/departments',
      'OfficeProjectTypes': '/lookup-management/office-project-types',
      'HousingBuildings': '/lookup-management/housing-buildings',
      'HousingFlats': '/lookup-management/housing-flats',
      'OutgoingCategories': '/lookup-management/outgoing-categories',
      'HouseOwnerships': '/lookup-management/house-ownerships',
      'IncomeTypes': '/lookup-management/income-types',
      'FamilyProjectStatuses': '/lookup-management/family-project-statuses'
      // TODO: Add routes for MissionTypes, ProjectTypes, Banks, NGOTypes when components are created
    };
    const route = routeMap[tableName];
    if (route) {
      this.router.navigate([route]);
    } else {
      console.warn(`Route not implemented for table: ${tableName}`);
    }
  }

  get pagedTables(): LookupTableSummaryDto[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.lookupTables.slice(start, start + this.pageSize);
  }

  onPageChange(page: number): void {
    this.currentPage = page;
  }

  trackByTableName(_: number, table: LookupTableSummaryDto): string {
    return table.tableName;
  }
}
