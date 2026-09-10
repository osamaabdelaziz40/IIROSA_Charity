import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import type { ApexOptions } from 'apexcharts';
import { BreadcrumbComponent, BreadcrumbItem, ApexChartComponent } from '../../shared/components';
import { SelectedCharityService } from '../../core/services/selected-charity.service';

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

  // Header charity switcher selection — null (card hidden) when "All
  // Charities"/nothing is chosen.
  selectedCharity$ = this.selectedCharityService.selectedCharity$;

  constructor(private selectedCharityService: SelectedCharityService) {}

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

  // Chart options are one-time readonly fields: the data is static, and handing
  // apx-chart a fresh options object on every change-detection pass would force
  // ApexCharts to re-render each cycle.
  readonly monthlyChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: {
      type: 'bar',
      height: 350,
      background: 'transparent',
      toolbar: { show: false },
      zoom: { enabled: true }
    },
    series: [
      { name: 'Charities', data: [32, 66, 44, 55, 41, 24, 67, 22, 43, 32, 66, 44] },
      { name: 'Orphans', data: [7, 30, 13, 23, 20, 12, 8, 13, 27, 7, 30, 13] }
    ],
    plotOptions: {
      bar: {
        horizontal: false,
        columnWidth: '40%',
        borderRadius: 4
      }
    },
    grid: {
      borderColor: 'rgba(255, 255, 255, 0.1)',
      strokeDashArray: 4
    },
    xaxis: {
      categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
      axisBorder: { show: false },
      axisTicks: { show: false },
      labels: { style: { colors: '#9ca3af' } }
    },
    yaxis: { labels: { style: { colors: '#9ca3af' } } },
    legend: { position: 'top', labels: { colors: '#9ca3af' } },
    fill: { opacity: 1 }
  };

  readonly financialChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: {
      type: 'line',
      height: 350,
      background: 'transparent',
      toolbar: { show: false },
      zoom: { enabled: false }
    },
    series: [
      { name: 'Donations', data: [31, 28, 30, 51, 42, 109, 100, 31, 40, 28, 31, 58] },
      { name: 'Expenses', data: [11, 45, 20, 32, 34, 52, 41, 11, 32, 45, 11, 75] }
    ],
    stroke: {
      curve: 'smooth',
      width: 3
    },
    grid: {
      borderColor: 'rgba(255, 255, 255, 0.1)',
      strokeDashArray: 4
    },
    xaxis: {
      categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
      axisBorder: { show: false },
      axisTicks: { show: false },
      labels: { style: { colors: '#9ca3af' } }
    },
    yaxis: { labels: { style: { colors: '#9ca3af' } } },
    legend: { position: 'top', labels: { colors: '#9ca3af' } },
    tooltip: { theme: 'dark' }
  };

  readonly familyGrowthChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: {
      type: 'area',
      height: 350,
      stacked: true,
      background: 'transparent',
      toolbar: { show: false }
    },
    series: [
      { name: 'Active Families', data: [31, 28, 30, 51, 42, 109, 100, 31, 40, 28, 31, 58] },
      { name: 'New Families', data: [11, 45, 20, 32, 34, 52, 41, 11, 32, 45, 11, 75] }
    ],
    stroke: {
      curve: 'smooth',
      width: 0
    },
    grid: {
      borderColor: 'rgba(255, 255, 255, 0.1)',
      strokeDashArray: 4
    },
    xaxis: {
      categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
      axisBorder: { show: false },
      axisTicks: { show: false },
      labels: { style: { colors: '#9ca3af' } }
    },
    yaxis: { labels: { style: { colors: '#9ca3af' } } },
    legend: { position: 'top', labels: { colors: '#9ca3af' } }
  };

  readonly budgetChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: {
      type: 'donut',
      height: 350,
      background: 'transparent',
      toolbar: { show: false }
    },
    series: [44, 55, 20, 41],
    labels: ['Education', 'Healthcare', 'Housing', 'Food'],
    plotOptions: {
      pie: {
        donut: {
          size: '70%'
        }
      }
    },
    grid: {
      borderColor: 'rgba(255, 255, 255, 0.1)',
      strokeDashArray: 4
    },
    legend: { position: 'bottom', labels: { colors: '#9ca3af' } },
    stroke: {
      show: true,
      colors: ['#1f2937']
    }
  };

  readonly performanceChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: {
      type: 'radar',
      height: 350,
      background: 'transparent',
      toolbar: { show: false }
    },
    series: [
      { name: 'This Year', data: [80, 50, 30, 40, 100, 20] },
      { name: 'Last Year', data: [20, 30, 40, 80, 20, 80] }
    ],
    plotOptions: {
      radar: {
        polygons: {
          strokeColors: 'rgba(255, 255, 255, 0.1)',
          connectorColors: 'rgba(255, 255, 255, 0.1)'
        }
      }
    },
    xaxis: {
      categories: ['Q1', 'Q2', 'Q3', 'Q4', 'Q5', 'Q6'],
      labels: { style: { colors: '#9ca3af' } }
    },
    yaxis: { labels: { style: { colors: '#9ca3af' } } }
  };
}
