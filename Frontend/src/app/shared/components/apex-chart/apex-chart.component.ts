import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Input,
  NgZone,
  OnChanges,
  OnDestroy,
  PLATFORM_ID,
  SimpleChanges,
  ViewChild,
  inject
} from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import type { ApexOptions } from 'apexcharts';
import type ApexCharts from 'apexcharts';

// ApexCharts reads window.ApexCharts for synced-charts support; publish the
// dynamically imported constructor there, as the upstream wrapper does.
declare global {
  interface Window {
    ApexCharts?: typeof ApexCharts;
  }
}

/**
 * Minimal ApexCharts wrapper (selector `apx-chart`), adapted from the
 * ng-apexcharts ChartComponent (MIT) — that package's 1.13.0 release ships a
 * broken typings reference and later releases need Angular >= 19, so the thin
 * wrapper is vendored here instead.
 *
 * Takes one full `[options]` object. All chart work runs outside the Angular
 * zone: ApexCharts mutates the DOM and fires listeners on its own, and letting
 * those back into change detection causes NG0100 expression-changed errors.
 */
@Component({
  selector: 'apx-chart',
  standalone: true,
  template: `<div #chart></div>`,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ApexChartComponent implements OnChanges, AfterViewInit, OnDestroy {
  @Input() options!: ApexOptions;

  @ViewChild('chart', { static: true }) private chartElement!: ElementRef<HTMLElement>;

  private chart: ApexCharts | null = null;
  private destroyed = false;
  private readonly zone = inject(NgZone);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  ngOnChanges(changes: SimpleChanges): void {
    // First change lands before the view exists — ngAfterViewInit creates the
    // chart; later options changes update it in place.
    if (!this.chart || !changes['options']) return;
    this.zone.runOutsideAngular(() => this.chart?.updateOptions(this.options, true, true));
  }

  ngAfterViewInit(): void {
    if (this.isBrowser) {
      void this.createElement();
    }
  }

  ngOnDestroy(): void {
    this.destroyed = true;
    this.chart?.destroy();
    this.chart = null;
  }

  private async createElement(): Promise<void> {
    const { default: ApexChartsCtor } = await import('apexcharts');
    window.ApexCharts ||= ApexChartsCtor;
    // The dynamic import can resolve after the component was torn down.
    if (this.destroyed) return;

    this.chart = this.zone.runOutsideAngular(
      () => new ApexChartsCtor(this.chartElement.nativeElement, this.options)
    );
    this.zone.runOutsideAngular(() => void this.chart?.render());
  }
}
