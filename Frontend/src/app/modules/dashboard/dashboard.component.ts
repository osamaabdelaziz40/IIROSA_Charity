import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { BreadcrumbComponent, BreadcrumbItem, ApexChartComponent } from '../../shared/components';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, TranslateModule, BreadcrumbComponent, ApexChartComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' }
  ];

  // Dashboard statistics
  stats = {
    totalCharities: 152,
    totalOrphans: 2840,
    totalFamilies: 485,
    activeMissions: 24
  };

  // Recent activities
  recentActivities = [
    { id: 1, text: 'New charity registered', time: '5 minutes ago', type: 'success' },
    { id: 2, text: 'Orphan sponsorship updated', time: '15 minutes ago', type: 'info' },
    { id: 3, text: 'Housing project completed', time: '1 hour ago', type: 'primary' },
    { id: 4, text: 'Mission report submitted', time: '2 hours ago', type: 'warning' }
  ];

  // Upcoming events
  upcomingEvents = [
    { id: 1, title: 'Board Meeting', date: '2026-05-15', time: '10:00 AM' },
    { id: 2, title: 'Charity Audit', date: '2026-05-18', time: '09:00 AM' },
    { id: 3, title: 'Orphanage Visit', date: '2026-05-20', time: '02:00 PM' }
  ];

  // Chart data
  chartSeries = [
    { name: 'Charities', data: [32, 66, 44, 55, 41, 24, 67, 22, 43, 32, 66, 44] },
    { name: 'Orphans', data: [7, 30, 13, 23, 20, 12, 8, 13, 27, 7, 30, 13] }
  ];

  chartCategories = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

  lineChartSeries = [
    { name: 'Donations', data: [31, 28, 30, 51, 42, 109, 100, 31, 40, 28, 31, 58] },
    { name: 'Expenses', data: [11, 45, 20, 32, 34, 52, 41, 11, 32, 45, 11, 75] }
  ];

  areaChartSeries = [
    { name: 'Active Families', data: [31, 28, 30, 51, 42, 109, 100, 31, 40, 28, 31, 58] },
    { name: 'New Families', data: [11, 45, 20, 32, 34, 52, 41, 11, 32, 45, 11, 75] }
  ];

  donutChartSeries = [44, 55, 20, 41];
  donutChartLabels = ['Education', 'Healthcare', 'Housing', 'Food'];

  radarChartSeries = [
    { name: 'This Year', data: [80, 50, 30, 40, 100, 20] },
    { name: 'Last Year', data: [20, 30, 40, 80, 20, 80] }
  ];
  radarChartCategories = ['Q1', 'Q2', 'Q3', 'Q4', 'Q5', 'Q6'];
}
