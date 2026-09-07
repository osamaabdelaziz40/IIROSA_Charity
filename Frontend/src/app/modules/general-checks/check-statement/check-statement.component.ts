import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { GeneralChecksService } from '../services/general-checks.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { CheckStatement } from '../models/check.model';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';

/**
 * §16.S.3 — بيان الشيكات: charity, date range and bank filters, the
 * شيكات إيتام / شيكات أفراد radio, the grid, then بحث and طباعة.
 */
@Component({
  selector: 'app-check-statement',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    BreadcrumbComponent,
    SharedModule,
    DropDownComponent
  ],
  templateUrl: './check-statement.component.html',
  styleUrls: ['./check-statement.component.scss']
})
export class CheckStatementComponent implements OnInit {
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'generalChecks.title', url: '/general-checks' },
    { label: 'generalChecks.statement' }
  ];

  statement: CheckStatement | null = null;
  loading = false;
  searched = false;

  isHeadOffice = false;
  charityOptions: Array<{ id: string; name: string }> = [];
  bankOptions: Array<{ id: number; name: string }> = [];

  filterForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private generalChecksService: GeneralChecksService,
    private lookupManagementService: LookupManagementService,
    private charityService: CharityService,
    private authService: AuthService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      charityId: [''],
      dateFrom: [null],
      dateTo: [null],
      bankId: [''],
      // §16.S.3 radio: '' = both, 'Orphans' = شيكات إيتام, 'Individuals' = شيكات أفراد
      chequeType: ['']
    });
  }

  ngOnInit(): void {
    this.isHeadOffice = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);

    this.lookupManagementService.getBanks({ pageNumber: 1, pageSize: 1000, isActive: true } as any)
      .subscribe({
        next: result => {
          this.bankOptions = (result.items || []).map(b => ({ id: b.id, name: b.nameAr || b.name }));
        }
      });

    if (this.isHeadOffice) {
      this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true }).subscribe({
        next: response => {
          this.charityOptions = (response.items || []).map(c => ({ id: c.id, name: c.name }));
        }
      });
    }
  }

  /** بحث — run the statement with the current filters. */
  search(): void {
    this.loading = true;
    const formValue = this.filterForm.value;

    this.generalChecksService.getStatement({
      charityId: formValue.charityId || undefined,
      bankId: formValue.bankId || undefined,
      dateFrom: formValue.dateFrom || undefined,
      dateTo: formValue.dateTo || undefined,
      chequeType: formValue.chequeType || undefined,
      page: 1,
      pageSize: 200
    }).subscribe({
      next: statement => {
        this.statement = statement;
        this.searched = true;
        this.loading = false;
      },
      error: () => {
        this.statement = null;
        this.loading = false;
      }
    });
  }

  /** طباعة — client-side print of the statement (window.print, epic-12 precedent). */
  printStatement(): void {
    window.print();
  }

  get currencyTotals(): Array<{ currency: string; total: number }> {
    return Object.entries(this.statement?.totalByCurrency || {}).map(([currency, total]) => ({ currency, total }));
  }

  trackByCheck(index: number, check: { id: string }): string {
    return check.id;
  }

  trackByTotal(index: number, entry: { currency: string }): string {
    return entry.currency;
  }

  /** The charity name for the statement header — the selected option, or "all". */
  selectedCharityName(): string {
    const id = this.filterForm.value.charityId;
    return this.charityOptions.find(c => c.id === id)?.name || this.translate.instant('generalChecks.allCharities');
  }
}
