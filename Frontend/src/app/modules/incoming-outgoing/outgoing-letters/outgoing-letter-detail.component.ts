import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { OutgoingDto, OutgoingOrphanRow } from '../models/outgoing.model';
import { OutgoingService } from '../services/outgoing.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { FileViewerComponent } from '../../../shared/components/file-viewer';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-outgoing-letter-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeaderComponent, BreadcrumbComponent, TranslateModule, FileViewerComponent],
  templateUrl: './outgoing-letter-detail.component.html',
  styleUrls: ['./outgoing-letter-detail.component.scss']
})
export class OutgoingLetterDetailComponent implements OnInit {
  letter: OutgoingDto | null = null;
  loading = false;

  // Page actions for header
  pageActions = [
    {
      label: 'common.edit',
      type: 'primary',
      icon: 'fe-edit',
      click: () => this.editLetter()
    },
    {
      // UC-COR-18 entry point — the §21.S.6 orphan-report attachment screen, opened
      // on this letter.
      label: 'incomingOutgoing.attachOrphansTitle',
      type: 'secondary',
      icon: 'fe-users',
      click: () => this.router.navigate(['/incoming-outgoing/export/outgoing'], {
        queryParams: { outgoingId: this.letter?.id }
      })
    },
    {
      label: 'common.delete',
      type: 'danger',
      icon: 'fe-trash',
      click: () => this.deleteLetter()
    }
  ];

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'incomingOutgoing.outgoingLetters', url: '/incoming-outgoing/outgoing' },
    { label: 'incomingOutgoing.outgoingLetterDetails' }
  ];

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private outgoingService: OutgoingService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    // UC-COR-16: deletes are the General Director's alone — SuperAdmin only. The
    // endpoint enforces the role regardless of what the UI shows.
    if (!this.authService.hasRole('SuperAdmin')) {
      this.pageActions = this.pageActions.filter(a => a.label !== 'common.delete');
    }
    this.loadLetter();
  }

  private loadLetter(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/incoming-outgoing/outgoing']);
      return;
    }

    this.loading = true;
    this.outgoingService.getOutgoingLetter(id).subscribe({
      next: (letter: OutgoingDto) => {
        this.letter = letter;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading letter:', error);
        this.notification.error(this.translate.instant('incomingOutgoing.loadLetterFailed'));
        this.loading = false;
      }
    });
  }

  editLetter(): void {
    if (this.letter) {
      this.router.navigate(['/incoming-outgoing/outgoing', this.letter.id, 'edit']);
    }
  }

  async deleteLetter(): Promise<void> {
    if (!this.letter) return;

    const message = this.translate.instant('incomingOutgoing.deleteConfirm', { subject: this.letter.subject });
    const confirmed = await this.notification.confirm(message, this.translate.instant('common.delete'));

    if (confirmed) {
      this.outgoingService.deleteOutgoingLetter(this.letter.id).subscribe({
        next: () => {
          this.notification.success(this.translate.instant('incomingOutgoing.letterDeleted'));
          this.router.navigate(['/incoming-outgoing/outgoing']);
        },
        error: (error: any) => {
          console.error('Error deleting letter:', error);
          this.notification.error(error.message || this.translate.instant('incomingOutgoing.deleteFailed'));
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/incoming-outgoing/outgoing']);
  }

  get attachedOrphans(): OutgoingOrphanRow[] {
    return this.letter?.orphans || [];
  }

  trackByOrphanId(_index: number, orphan: OutgoingOrphanRow): string {
    return orphan.orphanId;
  }

  formatDate(date: string | undefined): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleDateString('en-GB');
  }

  formatDateTime(date: string | undefined): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleString('en-GB');
  }
}
