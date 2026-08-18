import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.scss']
})
export class PaginationComponent implements OnChanges {
  @Input() currentPage: number = 1;
  @Input() pageSize: number = 20;
  @Input() totalCount: number = 0;
  @Input() maxPagesToShow: number = 5;
  @Output() pageChange = new EventEmitter<number>();

  totalPages: number = 0;
  pages: number[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    this.calculateTotalPages();
    this.calculatePages();
  }

  private calculateTotalPages(): void {
    this.totalPages = Math.ceil(this.totalCount / this.pageSize);
  }

  private calculatePages(): void {
    this.pages = [];
    if (this.totalPages === 0) return;

    const startPage = Math.max(1, this.currentPage - Math.floor(this.maxPagesToShow / 2));
    const endPage = Math.min(this.totalPages, startPage + this.maxPagesToShow - 1);

    if (endPage - startPage + 1 < this.maxPagesToShow) {
      const adjustedStartPage = Math.max(1, endPage - this.maxPagesToShow + 1);
      for (let i = adjustedStartPage; i <= endPage; i++) {
        this.pages.push(i);
      }
    } else {
      for (let i = startPage; i <= endPage; i++) {
        this.pages.push(i);
      }
    }
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.pageChange.emit(page);
    }
  }

  getMinValue(a: number, b: number): number {
    return Math.min(a, b);
  }

  getShowingStart(): number {
    return this.totalCount === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  getShowingEnd(): number {
    return this.getMinValue(this.currentPage * this.pageSize, this.totalCount);
  }
}
