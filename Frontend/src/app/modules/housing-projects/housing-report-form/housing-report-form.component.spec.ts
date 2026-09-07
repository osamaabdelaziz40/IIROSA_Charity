import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { HousingReportFormComponent } from './housing-report-form.component';
import { HousingProjectService } from '../services/housing-project.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AttachmentService } from '../../../core/services/attachment.service';

describe('HousingReportFormComponent', () => {
  let component: HousingReportFormComponent;
  let fixture: ComponentFixture<HousingReportFormComponent>;

  const mockActivatedRoute = {
    snapshot: {
      paramMap: new Map<string, string>([
        ['id', 'f1'],
        ['reportId', 'new']
      ]),
      queryParamMap: new Map<string, string>([
        ['beneficiary', 'b1'],
        ['type', 'Child']
      ])
    }
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        ReactiveFormsModule,
        TranslateModule.forRoot()
      ],
      declarations: [HousingReportFormComponent],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        {
          provide: Router,
          useValue: { navigate: jasmine.createSpy('navigate') }
        },
        HousingProjectService,
        LookupManagementService,
        AttachmentService,
        { provide: NotificationService, useValue: { success: () => {}, error: () => {}, warning: () => {} } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HousingReportFormComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('builds the §11.S.4 form with the mandatory photo control', () => {
    expect(component.reportForm.get('orphanImageId')).toBeTruthy();
    expect(component.reportForm.get('orphanImageId')?.validator).toBeTruthy();
    expect(component.reportForm.get('reportDate')?.validator).toBeTruthy();
  });

  it('carries the five §11.S.4 attachment slots with الصوره mandatory', () => {
    expect(component.imageSlots.length).toBe(5);
    const photo = component.imageSlots.find(s => s.control === 'orphanImageId');
    expect(photo?.required).toBeTrue();
  });

  it('clears the disease branch when الحالة الصحية leaves مريض', () => {
    component.reportForm.patchValue({ medicalStatus: 'مريض', disease: 'نوع' });
    component.reportForm.get('medicalStatus')?.setValue('سليم');
    expect(component.reportForm.get('disease')?.value).toBeNull();
    expect(component.isSick).toBeFalse();
  });

  it('clears the disability branch when الحالة الصحية leaves معاق', () => {
    component.reportForm.patchValue({ medicalStatus: 'معاق', disability: 'حركية', disabilityDescription: 'تفاصيل' });
    component.reportForm.get('medicalStatus')?.setValue('سليم');
    expect(component.reportForm.get('disability')?.value).toBeNull();
    expect(component.isDisabled).toBeFalse();
  });

  it('keeps الموافقه and رفض mutually exclusive', () => {
    component.reportForm.get('isRefused')?.setValue(true);
    component.reportForm.get('isAccepted')?.setValue(true);
    expect(component.reportForm.get('isRefused')?.value).toBeFalse();
  });

  it('clears the funding block when طلب كفالة طالب يتيم is unticked', () => {
    component.reportForm.patchValue({
      isOrphanStudent: true,
      annualFeeForStudy: 1200,
      studyingYears: 4,
      restStudyingYears: 2,
      graduationYear: 2028
    });
    component.reportForm.get('isOrphanStudent')?.setValue(false);
    expect(component.reportForm.get('annualFeeForStudy')?.value).toBeNull();
    expect(component.reportForm.get('graduationYear')?.value).toBeNull();
    expect(component.isStudentRequest).toBeFalse();
  });
});
