import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { filter, map, mergeMap } from 'rxjs/operators';

/**
 * Service to manage dynamic page titles based on routing
 * Automatically updates browser tab title when navigating to different pages
 */
@Injectable({
  providedIn: 'root'
})
export class TitleService {
  private readonly defaultTitle = 'IIROSA';

  constructor(
    private title: Title,
    private router: Router,
    private activeRoute: ActivatedRoute,
    private translate: TranslateService
  ) {}

  /**
   * Initialize the title service
   * Subscribe to router navigation events and update title accordingly
   */
  initialize(): void {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      map(() => this.activeRoute),
      map(route => {
        while (route.firstChild) {
          route = route.firstChild;
        }
        return route;
      }),
      filter(route => route.outlet === 'primary'),
      mergeMap(route => route.data)
    ).subscribe(data => {
      this.updateTitle(data);
    });
  }

  /**
   * Update the document title based on route data
   * @param data Route data object containing title information
   */
  private updateTitle(data: any): void {
    if (!data) {
      this.setTitle(this.defaultTitle);
      return;
    }

    // If title is provided in route data, use it
    if (data.title) {
      if (typeof data.title === 'string') {
        // Try to translate the title
        const translatedTitle = this.translate.instant(data.title);
        this.setTitle(translatedTitle);
      }
    } else if (data.pageTitle) {
      // Alternative key name
      const translatedTitle = this.translate.instant(data.pageTitle);
      this.setTitle(translatedTitle);
    } else {
      // Use default title
      this.setTitle(this.defaultTitle);
    }
  }

  /**
   * Set the document title with a suffix
   * @param title The page title to set
   * @param suffix Optional suffix (defaults to ' - IIROSA')
   */
  setTitle(title: string, suffix: string = ' - IIROSA'): void {
    const fullTitle = title.includes('IIROSA') ? title : `${title}${suffix}`;
    this.title.setTitle(fullTitle);
  }

  /**
   * Set title directly without translation or suffix
   * @param title The exact title to set
   */
  setRawTitle(title: string): void {
    this.title.setTitle(title);
  }

  /**
   * Get the current document title
   */
  getCurrentTitle(): string {
    return this.title.getTitle();
  }
}
