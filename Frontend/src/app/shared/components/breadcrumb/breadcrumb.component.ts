import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './breadcrumb.component.html',
  styleUrls: ['./breadcrumb.component.scss']
})
export class BreadcrumbComponent implements OnInit {
  @Input() items: BreadcrumbItem[] = [];
  @Input() autoGenerate = false;
  @Input() homeLabel = 'common.home';
  @Input() homeUrl = '/dashboard';

  breadcrumbs: BreadcrumbItem[] = [];
  staticBreadcrumbs: BreadcrumbItem[] = [];

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    if (this.autoGenerate) {
      this.generateBreadcrumbs();
    } else {
      this.staticBreadcrumbs = this.items;
    }
  }

  private generateBreadcrumbs(): void {
    this.breadcrumbs = [];
    let route = this.activatedRoute.root;
    let url = '';

    const addBreadcrumb = (label: string, path: string) => {
      this.breadcrumbs.push({ label, url: path || undefined });
    };

    // Add home as first breadcrumb
    addBreadcrumb(this.homeLabel, this.homeUrl);

    while (route.children.length > 0) {
      route = route.children[0];
      url += '/' + route.snapshot.url.map(segment => segment.path).join('/');

      if (route.snapshot.data['breadcrumb']) {
        const breadcrumb = route.snapshot.data['breadcrumb'];
        addBreadcrumb(breadcrumb, url);
      } else if (route.snapshot.data['breadcrumbLabel']) {
        addBreadcrumb(route.snapshot.data['breadcrumbLabel'], url);
      }
    }
  }

  get displayBreadcrumbs(): BreadcrumbItem[] {
    return this.autoGenerate ? this.breadcrumbs : this.staticBreadcrumbs;
  }
}

export interface BreadcrumbItem {
  label: string;
  url?: string;
  icon?: string;
  translate?: boolean;
}
