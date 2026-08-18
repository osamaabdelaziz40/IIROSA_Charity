import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { OutgoingDto } from '../models/outgoing.model';
import { OutgoingService } from '../services/outgoing.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { FileViewerComponent } from '../../../shared/components/file-viewer';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { IncomingService } from '../services/incoming.service';
import { IncomingDto } from '../models/incoming.model';
import { RouterModule } from '@angular/router';

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
  relatedIncomingLetter: IncomingDto | null = null;
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
    { label: 'incomingOutgoing.outgoingLetters', url: '/incoming-outgoing/outgoing' },
    { label: 'incomingOutgoing.outgoingLetterDetails' }
  ];

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private outgoingService: OutgoingService,
    private incomingService: IncomingService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
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

        // Load related incoming letter if exists
        if (letter.incomingId) {
          this.loadRelatedIncomingLetter(letter.incomingId);
        }
      },
      error: (error: any) => {
        console.error('Error loading letter:', error);
        this.notification.error(this.translate.instant('incomingOutgoing.loadLetterFailed'));
        this.loading = false;
      }
    });
  }

  private loadRelatedIncomingLetter(incomingId: string): void {
    this.loadingRelated = true;
    this.incomingService.getIncomingLetter(incomingId).subscribe({
      next: (letter: IncomingDto) => {
        this.relatedIncomingLetter = letter;
        this.loadingRelated = false;
      },
      error: () => {
        this.loadingRelated = false;
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

    const confirmed = await this.notification.confirm(
      this.translate.instant('incomingOutgoing.confirmDelete', { subject: this.letter.subject })
    );

    if (confirmed) {
      this.outgoingService.deleteOutgoingLetter(this.letter.id).subscribe({
        next: () => {
          this.notification.success(this.translate.instant('incomingOutgoing.deleteSuccess'));
          this.router.navigate(['/incoming-outgoing/outgoing']);
        },
        error: (error: any) => {
          console.error('Error deleting letter:', error);
          this.notification.error(this.translate.instant('incomingOutgoing.deleteFailed'));
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/incoming-outgoing/outgoing']);
  }

  getCategoryBadgeClass(): string {
    if (!this.letter) return 'badge-secondary';
    const category = this.letter.outgoingCategoryName || 'other';

    const categoryMap: { [key: string]: string } = {
      'Official': 'badge-primary',
      'official': 'badge-primary',
      'Internal': 'badge-info',
      'internal': 'badge-info',
      'External': 'badge-success',
      'external': 'badge-success'
    };

    return categoryMap[category] || 'badge-secondary';
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

  /**
   * Check if this letter has child follow-up letters
   */
  hasChildLetters(): boolean {
    return !!(this.letter?.childOutgoings && this.letter.childOutgoings.length > 0);
  }
}
