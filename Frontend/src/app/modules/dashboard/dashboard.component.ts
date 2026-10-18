import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import type { ApexOptions } from 'apexcharts';
import { EMPTY } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { BreadcrumbComponent, BreadcrumbItem, ApexChartComponent } from '../../shared/components';
import { SelectedCharityService } from '../../core/services/selected-charity.service';
import { DashboardService } from './services/dashboard.service';
import { DashboardStatistics } from './models/dashboard-statistics.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, TranslateModule, BreadcrumbComponent, ApexChartComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' }
  ];

  // Header charity switcher selection — null (card hidden) when "All
  // Charities"/nothing is chosen.
  selectedCharity$ = this.selectedCharityService.selectedCharity$;

  constructor(
    private selectedCharityService: SelectedCharityService,
    private dashboardService: DashboardService
  ) {}

  ngOnInit(): void {
    this.selectedCharity$.pipe(
      switchMap(charity => this.dashboardService.getStatistics(charity?.id ?? null).pipe(
        catchError(error => {
          console.error('Failed to load dashboard statistics', error);
          return EMPTY;
        })
      ))
    ).subscribe(statistics => this.applyStatistics(statistics));
  }

  // Dashboard statistics
  stats = {
    totalCharities: 0,
    totalOrphans: 0,
    totalFamilies: 0,
    activeMissions: 0
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

  // Chart inputs are replaced only when the dashboard statistics request completes.
  monthlyChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: {
      type: 'bar',
      height: 350,
      background: 'transparent',
      toolbar: { show: false },
      zoom: { enabled: true }
    },
    series: [
      { name: 'Charities', data: [] },
      { name: 'Orphans', data: [] }
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
      categories: [],
      axisBorder: { show: false },
      axisTicks: { show: false },
      labels: { style: { colors: '#9ca3af' } }
    },
    yaxis: { labels: { style: { colors: '#9ca3af' } } },
    legend: { position: 'top', labels: { colors: '#9ca3af' } },
    fill: { opacity: 1 }
  };

  financialChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: {
      type: 'line',
      height: 350,
      background: 'transparent',
      toolbar: { show: false },
      zoom: { enabled: false }
    },
    series: [
      { name: 'Allocated', data: [] },
      { name: 'Received', data: [] }
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
      categories: [],
      axisBorder: { show: false },
      axisTicks: { show: false },
      labels: { style: { colors: '#9ca3af' } }
    },
    yaxis: { labels: { style: { colors: '#9ca3af' } } },
    legend: { position: 'top', labels: { colors: '#9ca3af' } },
    tooltip: { theme: 'dark' }
  };

  familyGrowthChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: {
      type: 'area',
      height: 350,
      stacked: true,
      background: 'transparent',
      toolbar: { show: false }
    },
    series: [
      { name: 'Families Added', data: [] }
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
      categories: [],
      axisBorder: { show: false },
      axisTicks: { show: false },
      labels: { style: { colors: '#9ca3af' } }
    },
    yaxis: { labels: { style: { colors: '#9ca3af' } } },
    legend: { position: 'top', labels: { colors: '#9ca3af' } }
  };

  budgetChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: {
      type: 'donut',
      height: 350,
      background: 'transparent',
      toolbar: { show: false }
    },
    series: [],
    labels: [],
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

  performanceChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: {
      type: 'radar',
      height: 350,
      background: 'transparent',
      toolbar: { show: false }
    },
    series: [
      { name: 'Completed', data: [] },
      { name: 'Pending', data: [] }
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
      categories: [],
      labels: { style: { colors: '#9ca3af' } }
    },
    yaxis: { labels: { style: { colors: '#9ca3af' } } }
  };

  mostActiveCharitiesChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: { type: 'bar', height: 350, background: 'transparent', toolbar: { show: false } },
    series: [{ name: 'Interactions', data: [] }],
    plotOptions: { bar: { horizontal: true, borderRadius: 4, barHeight: '55%' } },
    xaxis: {
      categories: [],
      labels: { style: { colors: '#9ca3af' } }
    },
    grid: { borderColor: 'rgba(255, 255, 255, 0.1)', strokeDashArray: 4 },
    legend: { labels: { colors: '#9ca3af' } },
    dataLabels: { enabled: true, style: { colors: ['#fff'] } }
  };

  familiesByCharityChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: { type: 'bar', height: 350, background: 'transparent', toolbar: { show: false } },
    series: [{ name: 'Families', data: [] }],
    plotOptions: { bar: { horizontal: true, borderRadius: 4, barHeight: '55%' } },
    xaxis: {
      categories: [],
      labels: { style: { colors: '#9ca3af' } }
    },
    grid: { borderColor: 'rgba(255, 255, 255, 0.1)', strokeDashArray: 4 },
    dataLabels: { enabled: true, style: { colors: ['#fff'] } }
  };

  orphansByCharityChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: { type: 'bar', height: 350, background: 'transparent', toolbar: { show: false } },
    series: [{ name: 'Orphans', data: [] }],
    plotOptions: { bar: { horizontal: true, borderRadius: 4, barHeight: '55%' } },
    xaxis: {
      categories: [],
      labels: { style: { colors: '#9ca3af' } }
    },
    grid: { borderColor: 'rgba(255, 255, 255, 0.1)', strokeDashArray: 4 },
    dataLabels: { enabled: true, style: { colors: ['#fff'] } }
  };

  mostActiveUsersChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: { type: 'bar', height: 350, background: 'transparent', toolbar: { show: false } },
    series: [{ name: 'Interactions', data: [] }],
    plotOptions: { bar: { horizontal: true, borderRadius: 4, barHeight: '55%' } },
    xaxis: {
      categories: [],
      labels: { style: { colors: '#9ca3af' } }
    },
    grid: { borderColor: 'rgba(255, 255, 255, 0.1)', strokeDashArray: 4 },
    dataLabels: { enabled: true, style: { colors: ['#fff'] } }
  };

  orphanGenderChartOptions: ApexOptions = {
    theme: { mode: 'dark' },
    chart: { type: 'donut', height: 350, background: 'transparent', toolbar: { show: false } },
    series: [],
    labels: ['Male', 'Female'],
    plotOptions: { pie: { donut: { size: '68%' } } },
    dataLabels: { enabled: true },
    legend: { position: 'bottom', labels: { colors: '#9ca3af' } },
    stroke: { show: true, colors: ['#1f2937'] }
  };

  private applyStatistics(data: DashboardStatistics): void {
    this.stats = {
      totalCharities: data.totalCharities,
      totalOrphans: data.totalOrphans,
      totalFamilies: data.totalFamilies,
      activeMissions: data.activeMissions
    };
    this.monthlyChartOptions = {
      ...this.monthlyChartOptions,
      series: [
        { name: 'Charities', data: data.charitiesCreatedByMonth },
        { name: 'Orphans', data: data.orphansCreatedByMonth }
      ],
      xaxis: { ...(this.monthlyChartOptions.xaxis as object), categories: data.months }
    };
    this.financialChartOptions = {
      ...this.financialChartOptions,
      series: [
        { name: 'Allocated', data: data.allocatedPaymentsByMonth },
        { name: 'Received', data: data.receivedPaymentsByMonth }
      ],
      xaxis: { ...(this.financialChartOptions.xaxis as object), categories: data.months }
    };
    this.familyGrowthChartOptions = {
      ...this.familyGrowthChartOptions,
      series: [{ name: 'Families Added', data: data.familiesCreatedByMonth }],
      xaxis: { ...(this.familyGrowthChartOptions.xaxis as object), categories: data.months }
    };
    this.budgetChartOptions = {
      ...this.budgetChartOptions,
      series: data.paymentStatusValues,
      labels: data.paymentStatusLabels
    };
    this.performanceChartOptions = {
      ...this.performanceChartOptions,
      series: [
        { name: 'Completed', data: data.completedMissionsByQuarter },
        { name: 'Pending', data: data.pendingMissionsByQuarter }
      ],
      xaxis: { ...(this.performanceChartOptions.xaxis as object), categories: data.missionQuarterLabels }
    };
    this.mostActiveCharitiesChartOptions = this.withBarData(
      this.mostActiveCharitiesChartOptions, 'Interactions', data.activeCharityInteractions, data.activeCharityNames
    );
    this.familiesByCharityChartOptions = this.withBarData(
      this.familiesByCharityChartOptions, 'Families', data.familiesByCharity, data.familyCharityNames
    );
    this.orphansByCharityChartOptions = this.withBarData(
      this.orphansByCharityChartOptions, 'Orphans', data.orphansByCharity, data.orphanCharityNames
    );
    this.mostActiveUsersChartOptions = this.withBarData(
      this.mostActiveUsersChartOptions, 'Interactions', data.userInteractions, data.activeUserNames
    );
    this.orphanGenderChartOptions = {
      ...this.orphanGenderChartOptions,
      series: [data.maleOrphans, data.femaleOrphans]
    };
  }

  private withBarData(options: ApexOptions, name: string, values: number[], categories: string[]): ApexOptions {
    return {
      ...options,
      series: [{ name, data: values }],
      xaxis: { ...(options.xaxis as object), categories }
    };
  }
}
