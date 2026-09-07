import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { GeneralChecksService } from '../services/general-checks.service';
import { CheckDetail } from '../models/check.model';
import { AuthService } from '../../../core/services/auth.service';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';

/** UC-CHQ-03 — read-only cheque review, with edit and print entry points. */
@Component({
  selector: 'app-check-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, BreadcrumbComponent, SharedModule],
  templateUrl: './check-detail.component.html',
  styleUrls: ['./check-detail.component.scss']
})
export class CheckDetailComponent implements OnInit {
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'generalChecks.title', url: '/general-checks' },
    { label: 'generalChecks.checkDetails' }
  ];

  check: CheckDetail | null = null;
  loading = false;
  canEdit = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private generalChecksService: GeneralChecksService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.canEdit = this.authService.hasPermission('GeneralChecks.Edit');
    this.route.params.subscribe(params => this.loadCheck(params['id']));
  }

  private loadCheck(id: string): void {
    this.loading = true;
    this.generalChecksService.getCheckById(id).subscribe({
      next: check => {
        this.check = check;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.router.navigate(['/general-checks']);
      }
    });
  }

  /** UC-CHQ-10 — client-side print of the cheque data (window.print, epic-12 precedent). */
  printCheck(): void {
    window.print();
  }

  get flags(): string[] {
    if (!this.check) return [];
    const flags: string[] = [];
    if (this.check.isDamaged) flags.push('generalChecks.isDamaged');
    if (this.check.isReturned) flags.push('generalChecks.isReturned');
    if (this.check.isDispensed) flags.push('generalChecks.isDispensed');
    if (this.check.isDone) flags.push('generalChecks.isDone');
    return flags;
  }
}
