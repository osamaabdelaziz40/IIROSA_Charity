import { Component, Input, AfterViewInit, ElementRef, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import { ApexChartService } from '../../../core/services/apex-chart.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-apex-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="apex-chart-wrapper" [style.height.px]="height">
      <div *ngIf="!isLoaded" class="chart-loading">
        <div class="spinner-border text-primary" role="status">
          <span class="sr-only">Loading...</span>
        </div>
        <small class="text-muted ml-2">Loading chart...</small>
      </div>
      <div class="apex-chart-container" [style.display]="isLoaded ? 'block' : 'none'"></div>
    </div>
  `,
  styles: [`
    .apex-chart-wrapper {
      width: 100%;
      position: relative;
    }
    .apex-chart-container {
      width: 100%;
      height: 100%;
      min-height: 300px;
    }
    .chart-loading {
      display: flex;
      align-items: center;
      justify-content: center;
      height: 100%;
      min-height: 300px;
    }
  `]
})
export class ApexChartComponent implements AfterViewInit, OnDestroy, OnChanges {
  @Input() chartType: 'line' | 'bar' | 'area' | 'donut' | 'radar' | 'column' = 'bar';
  @Input() series: any[] = [];
  @Input() categories: string[] = [];
  @Input() labels: string[] = [];
  @Input() height: number = 350;
  @Input() options: any = {};

  private chart: any = null;
  private chartId: string = `chart-${Math.random().toString(36).substr(2, 9)}`;
  private resizeObserver: ResizeObserver | null = null;
  private initAttempts: number = 0;
  private maxInitAttempts: number = 10;
  isLoaded: boolean = false;

  constructor(
    private elementRef: ElementRef,
    private chartService: ApexChartService
  ) {}

  ngAfterViewInit(): void {
    // Use ResizeObserver to wait for container to have dimensions
    this.waitForContainerDimensions();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.chart && (changes['series'] || changes['categories'])) {
      this.updateChart();
    }
  }

  private waitForContainerDimensions(): void {
    const container = this.elementRef.nativeElement.querySelector('.apex-chart-container');
    if (!container) {
      console.error('Chart container not found');
      return;
    }

    // Check if container already has dimensions
    const rect = container.getBoundingClientRect();
    if (rect.width > 0 && rect.height > 0) {
      this.initChart();
      return;
    }

    // Use ResizeObserver to wait for container to have dimensions
    if (typeof ResizeObserver !== 'undefined') {
      this.resizeObserver = new ResizeObserver((entries) => {
        for (const entry of entries) {
          const { width, height } = entry.contentRect;
          if (width > 0 && height > 0 && !this.isLoaded && this.initAttempts < this.maxInitAttempts) {
            this.initAttempts++;
            this.initChart();
            if (this.isLoaded && this.resizeObserver) {
              this.resizeObserver.disconnect();
              this.resizeObserver = null;
            }
          }
        }
      });

      this.resizeObserver.observe(container);

      // Fallback timeout in case ResizeObserver doesn't fire
      setTimeout(() => {
        if (!this.isLoaded && this.resizeObserver) {
          this.resizeObserver.disconnect();
          this.resizeObserver = null;
          this.initChart(); // Try anyway
        }
      }, 2000);
    } else {
      // Fallback for browsers without ResizeObserver
      let attempts = 0;
      const checkDimensions = () => {
        attempts++;
        const rect = container.getBoundingClientRect();
        if (rect.width > 0 && rect.height > 0 || attempts >= 10) {
          this.initChart();
        } else {
          setTimeout(checkDimensions, 200);
        }
      };
      checkDimensions();
    }
  }

  private initChart(): void {
    // Check if ApexCharts is available
    if (typeof window === 'undefined' || !(window as any).ApexCharts) {
      console.error('ApexCharts is not loaded yet');
      return;
    }

    const container = this.elementRef.nativeElement.querySelector('.apex-chart-container');
    if (!container) {
      console.error('Chart container not found');
      return;
    }

    // Ensure container is visible and has dimensions
    const rect = container.getBoundingClientRect();
    if (rect.width === 0 || rect.height === 0) {
      console.warn('Chart container has no dimensions, using default height');
    }

    container.id = this.chartId;
    container.style.height = `${this.height}px`;

    try {
      const chartOptions = this.getChartOptions();
      this.chart = this.chartService.createChart(`#${this.chartId}`, chartOptions, this.chartId);
      if (this.chart) {
        this.isLoaded = true;
        console.log(`Chart ${this.chartId} created successfully`);
      } else {
        console.error('Failed to create chart');
      }
    } catch (error) {
      console.error('Error creating chart:', error);
    }
  }

  private updateChart(): void {
    if (this.chart) {
      const chartOptions = this.getChartOptions();
      this.chart.updateOptions(chartOptions);
    }
  }

  private getChartOptions(): any {
    const baseOptions = {
      ...this.options,
      chart: {
        ...this.options.chart,
        height: this.height
      }
    };

    switch (this.chartType) {
      case 'column':
        return this.chartService.getColumnChartOptions(this.series, this.categories);
      case 'line':
        return this.chartService.getLineChartOptions(this.series, this.categories);
      case 'area':
        return this.chartService.getAreaChartOptions(this.series, this.categories);
      case 'donut':
        return this.chartService.getDonutChartOptions(this.series as number[], this.labels);
      case 'radar':
        return this.chartService.getRadarChartOptions(this.series, this.categories);
      default:
        return baseOptions;
    }
  }

  ngOnDestroy(): void {
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
      this.resizeObserver = null;
    }
    if (this.chartId && this.chart) {
      this.chartService.destroyChart(this.chartId);
    }
  }
}
