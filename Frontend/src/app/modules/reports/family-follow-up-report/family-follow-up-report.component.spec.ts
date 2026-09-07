import { ChangeDetectorRef, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule } from '@ngx-translate/core';
import { NEVER, of, Subject } from 'rxjs';

import { FamilyFollowUpReportComponent } from './family-follow-up-report.component';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { FamilyService } from '../../families/services/family.service';
import { ReportService } from '../services/report.service';
import { ReportPdfService } from '../services/report-pdf.service';

/**
 * Review P20/P22 2026-08-26: the old spec still targeted the pre-18-37/18-41 API
 * (printIdentificationSheets('guardians'), familyCode, exportSheet('guardian-identification-sheets')).
 * Rewritten against the current contract: variant-keyed getGuardianIdentificationSheets,
 * OnPush markForCheck on landing subscribers, destroy$ teardown, truncated-sheet warning.
 */
describe('FamilyFollowUpReportComponent', () => {
  let component: FamilyFollowUpReportComponent;
  let fixture: ComponentFixture<FamilyFollowUpReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        FamilyFollowUpReportComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [
        NotificationService,
        { provide: AuthService, useValue: { hasAnyRole: () => true, hasPermission: () => true } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FamilyFollowUpReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('defaults the report date to today and waits for an explicit run', () => {
    expect(component.reportDate).toBe(new Date().toISOString().slice(0, 10));
    expect(component.hasRun).toBe(false);
    expect(component.rows).toEqual([]);
  });

  it('refuses to run without a date (client mirror of the validator)', () => {
    component.reportDate = '';
    const notification = TestBed.inject(NotificationService);
    spyOn(notification, 'error');

    component.run();

    expect(notification.error).toHaveBeenCalled();
    expect(component.loading).toBe(false);
  });

  it('refuses to print the tracking sheet without a date (UC-FAM-14)', () => {
    component.reportDate = '';
    const notification = TestBed.inject(NotificationService);
    spyOn(notification, 'error');
    const reportService = TestBed.inject(ReportService);
    const exportSpy = spyOn(reportService, 'exportSheet');

    component.printTrackingSheet();

    expect(notification.error).toHaveBeenCalled();
    expect(exportSpy).not.toHaveBeenCalled();
  });

  it('requests the identification sheet with variant, HQ charity scope and date (UC-RPT-37)', () => {
    const reportService = TestBed.inject(ReportService);
    // Never completes — no print pipeline runs in the test context.
    const sheetSpy = spyOn(reportService, 'getGuardianIdentificationSheets').and.returnValue(NEVER);

    component.charityFilter = 'charity-1';
    component.idVariant = 'AllGuardians';
    component.printIdentificationSheet();

    expect(sheetSpy).toHaveBeenCalledWith({
      variant: 'AllGuardians',
      charityId: 'charity-1',
      date: component.reportDate,
      familyId: undefined
    });
  });

  it('refuses the single-family identification sheet without a family (AC 7)', () => {
    const notification = TestBed.inject(NotificationService);
    spyOn(notification, 'error');
    const reportService = TestBed.inject(ReportService);
    const sheetSpy = spyOn(reportService, 'getGuardianIdentificationSheets');

    component.idVariant = 'SingleFamily';
    component.familyFilter = '';
    component.printIdentificationSheet();

    expect(notification.error).toHaveBeenCalled();
    expect(sheetSpy).not.toHaveBeenCalled();
  });

  it('marks the OnPush tree for check when a run lands (Review P20)', () => {
    const familyService = TestBed.inject(FamilyService);
    spyOn(familyService, 'getFollowUp').and.returnValue(
      of({ items: [{ code: 'FAM-1' }], totalCount: 1 }) as any
    );
    const cdr = fixture.debugElement.injector.get(ChangeDetectorRef);
    const markSpy = spyOn(cdr, 'markForCheck').and.callThrough();

    component.run();

    expect(component.hasRun).toBe(true);
    expect(component.rows.length).toBe(1);
    expect(markSpy).toHaveBeenCalled();
  });

  it('tears down in-flight subscriptions on destroy (Review P20)', () => {
    const familyService = TestBed.inject(FamilyService);
    const source = new Subject<{ items: unknown[]; totalCount: number }>();
    spyOn(familyService, 'getFollowUp').and.returnValue(source.asObservable() as any);

    component.run();
    component.ngOnDestroy();

    // The late landing must not mutate destroyed state.
    source.next({ items: [{ code: 'FAM-1' }], totalCount: 1 });
    expect(component.hasRun).toBe(false);
  });

  it('warns when the identification sheet comes back truncated (Review P22)', () => {
    const reportService = TestBed.inject(ReportService);
    spyOn(reportService, 'getGuardianIdentificationSheets').and.returnValue(
      of({
        rows: [{ orphanName: 'يتيم' } as any],
        truncated: true,
        generatedOn: new Date().toISOString(),
        charityName: 'جمعية'
      }) as any
    );
    const pdfService = TestBed.inject(ReportPdfService);
    spyOn(pdfService, 'printSheet');
    const notification = TestBed.inject(NotificationService);
    const warnSpy = spyOn(notification, 'warning');

    component.printIdentificationSheet();

    expect(warnSpy).toHaveBeenCalled();
    expect(pdfService.printSheet).toHaveBeenCalled();
  });
});
