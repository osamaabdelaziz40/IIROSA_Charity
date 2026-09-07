import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { IncomingDto, CorrespondenceStatusOption } from '../models/incoming.model';
import { IncomingService } from '../services/incoming.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { FileViewerComponent } from '../../../shared/components/file-viewer';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-incoming-letter-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeaderComponent, BreadcrumbComponent, TranslateModule, FileViewerComponent],
  templateUrl: './incoming-letter-detail.component.html',
  styleUrls: ['./incoming-letter-detail.component.scss']
})
export class IncomingLetterDetailComponent implements OnInit {
  letter: IncomingDto | null = null;
  loading = false;

  // The endpoint-served tri-state palette — the stored status value is the
  // Arabic term itself, so it renders raw; this map only supplies the color.
  private statusColors: { [key: string]: string } = {};

  // Page actions for header
  pageActions = [
    {
      label: 'common.edit',
      type: 'primary',
      icon: 'fe-edit',
      click: () => this.editLetter()
    },
    {
      // UC-COR-09 entry point — the §21.S.3 employee-attachment screen, opened on
      // this letter.
      label: 'incomingOutgoing.attachEmployeesTitle',
      type: 'secondary',
      icon: 'fe-users',
      click: () => this.router.navigate(['/incoming-outgoing/export/incoming'], {
        queryParams: { incomingId: this.letter?.id }
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
    { label: 'incomingOutgoing.incomingLetters', url: '/incoming-outgoing/incoming' },
    { label: 'incomingOutgoing.incomingLetterDetails' }
  ];

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private incomingService: IncomingService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    // UC-COR-07: deletes are the General Director's alone — SuperAdmin only. The
    // endpoint enforces the role regardless of what the UI shows.
    if (!this.authService.hasRole('SuperAdmin')) {
      this.pageActions = this.pageActions.filter(a => a.label !== 'common.delete');
    }
    this.loadStatusPalette();
    this.loadLetter();
  }

  private loadStatusPalette(): void {
    this.incomingService.getAvailableStatuses().subscribe({
      next: (statuses: CorrespondenceStatusOption[]) => {
        this.statusColors = statuses.reduce((map, s) => ({ ...map, [s.id]: `badge-${s.color}` }), {});
      },
      error: () => console.error('Error loading statuses')
    });
  }

  private loadLetter(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/incoming-outgoing/incoming']);
      return;
    }

    this.loading = true;
    this.incomingService.getIncomingLetter(id).subscribe({
      next: (letter: IncomingDto) => {
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
      this.router.navigate(['/incoming-outgoing/incoming', this.letter.id, 'edit']);
    }
  }

  async deleteLetter(): Promise<void> {
    if (!this.letter) return;

    const message = this.translate.instant('incomingOutgoing.deleteConfirm', { subject: this.letter.subject });
    const confirmed = await this.notification.confirm(message, this.translate.instant('common.delete'));

    if (confirmed) {
      this.incomingService.deleteIncomingLetter(this.letter.id).subscribe({
        next: () => {
          this.notification.success(this.translate.instant('incomingOutgoing.letterDeleted'));
          this.router.navigate(['/incoming-outgoing/incoming']);
        },
        error: (error: any) => {
          console.error('Error deleting letter:', error);
          this.notification.error(error.message || this.translate.instant('incomingOutgoing.deleteFailed'));
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/incoming-outgoing/incoming']);
  }

  getStatusBadgeClass(): string {
    return this.statusColors[this.letter?.status || ''] || 'badge-secondary';
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
