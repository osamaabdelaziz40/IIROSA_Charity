import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { IncomingDto } from '../models/incoming.model';
import { IncomingService } from '../services/incoming.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { FileViewerComponent } from '../../../shared/components/file-viewer';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { OutgoingService } from '../services/outgoing.service';
import { OutgoingDto } from '../models/outgoing.model';
import { RouterModule } from '@angular/router';

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
  relatedOutgoingLetter: OutgoingDto | null = null;
  loadingRelated = false;

  // Page actions for header
  pageActions = [
    {
      label: 'common.edit',
      type: 'primary',
      icon: 'fe-edit',
      click: () => this.editLetter()
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
    private outgoingService: OutgoingService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadLetter();
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

        // Load related outgoing letter if exists
        if (letter.outgoingId) {
          this.loadRelatedOutgoingLetter(letter.outgoingId);
        }
      },
      error: (error: any) => {
        console.error('Error loading letter:', error);
        this.notification.error(this.translate.instant('incomingOutgoing.loadLetterFailed'));
        this.loading = false;
      }
    });
  }

  private loadRelatedOutgoingLetter(outgoingId: string): void {
    this.loadingRelated = true;
    this.outgoingService.getOutgoingLetter(outgoingId).subscribe({
      next: (letter: OutgoingDto) => {
        this.relatedOutgoingLetter = letter;
        this.loadingRelated = false;
      },
      error: () => {
        this.loadingRelated = false;
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

    const confirmed = await this.notification.confirm(
      this.translate.instant('incomingOutgoing.confirmDelete', { subject: this.letter.subject })
    );

    if (confirmed) {
      this.incomingService.deleteIncomingLetter(this.letter.id).subscribe({
        next: () => {
          this.notification.success(this.translate.instant('incomingOutgoing.deleteSuccess'));
          this.router.navigate(['/incoming-outgoing/incoming']);
        },
        error: (error: any) => {
          console.error('Error deleting letter:', error);
          this.notification.error(this.translate.instant('incomingOutgoing.deleteFailed'));
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/incoming-outgoing/incoming']);
  }

  getStatusBadgeClass(): string {
    if (!this.letter) return 'badge-secondary';
    const status = this.letter.status || 'Received';

    const statusMap: { [key: string]: string } = {
      'Received': 'badge-success',
      'Processing': 'badge-info',
      'Completed': 'badge-primary',
      'Closed': 'badge-secondary',
      'Pending': 'badge-warning'
    };

    return statusMap[status] || 'badge-secondary';
  }

  getStatusText(): string {
    if (!this.letter) return '';
    const status = this.letter.status || 'Received';
    return this.translate.instant(`incomingOutgoing.status${status}`);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleDateString('en-GB');
  }

  formatDateTime(date: Date | string | undefined): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleString('en-GB');
  }
}
