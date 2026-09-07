/**
 * Mission Register Component spec (epic 15, UC-MSN-09 — §20.S.3).
 * Tests are excluded from the project's verify loop by standing decision; this file
 * documents the screen's binding contract for whenever they are re-enabled.
 */

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslateModule } from '@ngx-translate/core';

import { MissionRegisterComponent } from './mission-register.component';
import { MissionService } from '../services/mission.service';
import { EmployeeService } from '../../employees/services/employee.service';
import { NotificationService } from '../../../core/services/notification.service';
import { MissionDetail } from '../models/mission.model';

describe('MissionRegisterComponent', () => {
  let component: MissionRegisterComponent;
  let fixture: ComponentFixture<MissionRegisterComponent>;

  const mission: MissionDetail = {
    id: '00000000-0000-0000-0000-000000000001',
    missionTarget: 'هدف المأمورية',
    missionDetails: 'التفاصيل',
    details: 'المهمة',
    missionTypeId: 1,
    missionTypeName: 'زيارة ميدانية',
    missionTimeTypeId: 1,
    missionTimeTypeName: 'صباحي',
    missionInterviewTypeId: 1,
    missionInterviewTypeName: 'مقابلة ميدانية',
    missionDate: '2026-08-23T00:00:00Z',
    isMissionCompleted: false,
    countryId: 1,
    regionId: 1,
    regionName: 'المنطقة',
    centerId: 1,
    centerName: 'المركز',
    missionLocation: 'الموقع',
    village: 'الحي',
    assignedToUserId: '00000000-0000-0000-0000-000000000002',
    assignedUserName: 'الموظف المسئول',
    entityName: 'الجهة المنظمة',
    conferenceName: 'اسم المؤتمر',
    createdOn: '2026-08-23T00:00:00Z'
  };

  const missionServiceSpy = jasmine.createSpyObj<MissionService>('MissionService', [
    'getMissionById',
    'registerMissionResult'
  ]);

  const employeeServiceSpy = jasmine.createSpyObj<EmployeeService>('EmployeeService', ['getEmployees']);

  const notificationSpy = jasmine.createSpyObj<NotificationService>('NotificationService', [
    'success',
    'error',
    'info'
  ]);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, TranslateModule.forRoot()],
      declarations: [MissionRegisterComponent],
      providers: [
        { provide: MissionService, useValue: missionServiceSpy },
        { provide: EmployeeService, useValue: employeeServiceSpy },
        { provide: NotificationService, useValue: notificationSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { params: { id: mission.id } }
          }
        }
      ]
    }).compileComponents();

    missionServiceSpy.getMissionById.and.returnValue(of(mission));
    missionServiceSpy.registerMissionResult.and.returnValue(of(mission));
    employeeServiceSpy.getEmployees.and.returnValue(of({ items: [], totalCount: 0 }));

    fixture = TestBed.createComponent(MissionRegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads the mission and pre-fills the editable fields', () => {
    expect(component.mission?.id).toBe(mission.id);
    expect(component.registerForm.get('missionTarget')?.value).toBe(mission.missionTarget);
    expect(component.registerForm.get('assignedToUserId')?.value).toBe(mission.assignedToUserId);
  });

  it('redirects away when a result is already registered', () => {
    const routerSpy = spyOn((component as any).router, 'navigate');
    missionServiceSpy.getMissionById.and.returnValue(of({ ...mission, isMissionCompleted: true }));
    component.ngOnInit();
    expect(notificationSpy.info).toHaveBeenCalled();
    expect(routerSpy).toHaveBeenCalledWith(['/missions', mission.id]);
  });

  it('makes the two completion checkboxes mutually exclusive', () => {
    component.onCompletedChange(true);
    expect(component.isCompleted).toBeTrue();
    expect(component.isNotCompleted).toBeFalse();

    component.onNotCompletedChange(true);
    expect(component.isNotCompleted).toBeTrue();
    expect(component.isCompleted).toBeFalse();
  });

  it('refuses to submit without an explicit outcome', () => {
    component.isCompleted = false;
    component.isNotCompleted = false;
    component.onSubmit();
    expect(notificationSpy.error).toHaveBeenCalled();
    expect(missionServiceSpy.registerMissionResult).not.toHaveBeenCalled();
  });

  it('posts the outcome on valid submit', () => {
    component.isCompleted = true;
    component.registerForm.get('reason')?.setValue('تمت المأمورية بنجاح');
    component.onSubmit();

    expect(missionServiceSpy.registerMissionResult).toHaveBeenCalledWith(
      mission.id,
      jasmine.objectContaining({ isCompleted: true, reason: 'تمت المأمورية بنجاح' })
    );
  });

  it('flags server field errors onto the form controls', () => {
    component.isCompleted = true;
    component.registerForm.get('reason')?.setValue('x');

    missionServiceSpy.registerMissionResult.and.returnValue(
      throwError(() => ({
        error: {
          message: 'Invalid',
          errors: { Reason: ['السبب مطلوب'] }
        }
      }))
    );

    component.onSubmit();

    expect(component.isFieldInvalid('reason')).toBeTrue();
    expect(component.getFieldError('reason')).toBe('السبب مطلوب');
  });
});
