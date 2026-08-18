import { Injectable, ElementRef, OnDestroy } from '@angular/core';

// Extend Window interface to include ApexCharts
declare global {
  interface Window {
    ApexCharts: any;
  }
}

@Injectable({
  providedIn: 'root'
})
export class ApexChartService implements OnDestroy {
  private charts: Map<string, any> = new Map();

  constructor() {
    // Wait for ApexCharts to be loaded from scripts
    this.waitForApexCharts();
  }

  private waitForApexCharts(): void {
    const checkInterval = setInterval(() => {
      if (typeof window !== 'undefined' && (window as any).ApexCharts) {
        clearInterval(checkInterval);
        console.log('ApexCharts loaded successfully');
      }
    }, 100);

    // Clear interval after 10 seconds
    setTimeout(() => clearInterval(checkInterval), 10000);
  }

  /**
   * Create a new chart
   * @param element The HTML element or selector for the chart
   * @param options Chart configuration options
   * @param chartId Unique identifier for the chart
   */
  createChart(element: string | ElementRef<any>, options: any, chartId?: string): any {
    if (typeof window === 'undefined' || !(window as any).ApexCharts) {
      console.error('ApexCharts is not loaded');
      return null;
    }

    let selector: string | HTMLElement;
    if (typeof element === 'string') {
      selector = element;
      // For string selectors, verify the element exists in the DOM
      const domElement = document.querySelector(selector as string);
      if (!domElement) {
        console.error(`Chart element not found in DOM: ${selector}`);
        return null;
      }
    } else {
      selector = element.nativeElement as HTMLElement;
    }

    try {
      const chart = new (window as any).ApexCharts(selector, options);
      chart.render();

      if (chartId) {
        this.charts.set(chartId, chart);
      }

      return chart;
    } catch (error) {
      console.error('Error creating ApexChart:', error);
      return null;
    }
  }

  /**
   * Update an existing chart
   * @param chartId The ID of the chart to update
   * @param options New chart options
   */
  updateChart(chartId: string, options: any): void {
    const chart = this.charts.get(chartId);
    if (chart) {
      chart.updateOptions(options);
    }
  }

  /**
   * Update chart series data
   * @param chartId The ID of the chart
   * @param series New series data
   */
  updateSeries(chartId: string, series: any[]): void {
    const chart = this.charts.get(chartId);
    if (chart) {
      chart.updateSeries(series);
    }
  }

  /**
   * Destroy a chart
   * @param chartId The ID of the chart to destroy
   */
  destroyChart(chartId: string): void {
    const chart = this.charts.get(chartId);
    if (chart && typeof chart.destroy === 'function') {
      try {
        chart.destroy();
        this.charts.delete(chartId);
      } catch (error) {
        console.warn('Error destroying chart:', error);
        this.charts.delete(chartId);
      }
    }
  }

  /**
   * Destroy all charts
   */
  destroyAllCharts(): void {
    this.charts.forEach((chart) => chart.destroy());
    this.charts.clear();
  }

  ngOnDestroy(): void {
    this.destroyAllCharts();
  }

  // Common chart configurations
  getDefaultChartOptions(): any {
    return {
      theme: {
        mode: 'dark'
      },
      chart: {
        background: 'transparent',
        toolbar: {
          show: false
        }
      },
      grid: {
        borderColor: 'rgba(255, 255, 255, 0.1)',
        strokeDashArray: 4
      },
      xaxis: {
        axisBorder: {
          show: false
        },
        axisTicks: {
          show: false
        },
        tooltip: {
          enabled: false
        }
      }
    };
  }

  // Column Chart Options
  getColumnChartOptions(series: any[], categories: string[]): any {
    return {
      ...this.getDefaultChartOptions(),
      series,
      chart: {
        type: 'bar',
        height: 350,
        stacked: false,
        columnWidth: '70%',
        zoom: { enabled: true },
        toolbar: { show: false },
        background: 'transparent'
      },
      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: '40%',
          borderRadius: 4
        }
      },
      xaxis: {
        categories,
        labels: {
          style: {
            colors: '#9ca3af'
          }
        }
      },
      yaxis: {
        labels: {
          style: {
            colors: '#9ca3af'
          }
        }
      },
      legend: {
        position: 'top',
        labels: {
          colors: '#9ca3af'
        }
      },
      fill: {
        opacity: 1
      }
    };
  }

  // Line Chart Options
  getLineChartOptions(series: any[], categories: string[]): any {
    return {
      ...this.getDefaultChartOptions(),
      series,
      chart: {
        height: 350,
        type: 'line',
        zoom: { enabled: false },
        toolbar: { show: false }
      },
      stroke: {
        curve: 'smooth',
        width: 3
      },
      xaxis: {
        categories,
        labels: {
          style: {
            colors: '#9ca3af'
          }
        }
      },
      yaxis: {
        labels: {
          style: {
            colors: '#9ca3af'
          }
        }
      },
      legend: {
        position: 'top',
        labels: {
          colors: '#9ca3af'
        }
      },
      tooltip: {
        theme: 'dark'
      }
    };
  }

  // Area Chart Options
  getAreaChartOptions(series: any[], categories: string[]): any {
    return {
      ...this.getDefaultChartOptions(),
      series,
      chart: {
        type: 'area',
        height: 350,
        stacked: true,
        toolbar: { show: false }
      },
      stroke: {
        curve: 'smooth',
        width: 0
      },
      xaxis: {
        categories,
        labels: {
          style: {
            colors: '#9ca3af'
          }
        }
      },
      yaxis: {
        labels: {
          style: {
            colors: '#9ca3af'
          }
        }
      },
      legend: {
        position: 'top',
        labels: {
          colors: '#9ca3af'
        }
      }
    };
  }

  // Donut Chart Options
  getDonutChartOptions(series: number[], labels: string[]): any {
    return {
      ...this.getDefaultChartOptions(),
      series,
      chart: {
        type: 'donut',
        height: 350
      },
      labels,
      plotOptions: {
        pie: {
          donut: {
            size: '70%'
          }
        }
      },
      legend: {
        position: 'bottom',
        labels: {
          colors: '#9ca3af'
        }
      },
      stroke: {
        show: true,
        colors: ['#1f2937']
      }
    };
  }

  // Radar Chart Options
  getRadarChartOptions(series: any[], categories: string[]): any {
    return {
      ...this.getDefaultChartOptions(),
      series,
      chart: {
        height: 350,
        type: 'radar',
        toolbar: { show: false }
      },
      plotOptions: {
        radar: {
          polygons: {
            strokeColors: 'rgba(255, 255, 255, 0.1)',
            connectorColors: 'rgba(255, 255, 255, 0.1)'
          }
        }
      },
      xaxis: {
        categories,
        labels: {
          style: {
            colors: '#9ca3af'
          }
        }
      },
      yaxis: {
        labels: {
          style: {
            colors: '#9ca3af'
          }
        }
      }
    };
  }
}
