/**
 * Notification Form Component spec (UC-NTF push / edit).
 * Tests are excluded from the project's verify loop by standing decision; this file
 * documents the screen's binding contract for whenever they are re-enabled.
 */

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslateModule } from '@ngx-translate/core';

import { NotificationFormComponent } from './notification-form.component';
import { NotificationLogService } from '../services/notification-log.service';
import { UserManagementService } from '../../user-management/services/user-management.service';
import { CharityService } from '../../charities/services/charity.service';
import { NotificationService } from '../../../core/services/notification.service';
import { NotificationsLog } from '../models/notification.model';

describe('NotificationFormComponent', () => {
  let component: NotificationFormComponent;
  let fixture: ComponentFixture<NotificationFormComponent>;

  const stored: NotificationsLog = {
    id: '00000000-0000-0000-0000-000000000001',
    title: 'إشعار تجريبي',
    message: 'نص الإشعار',
    isUser: true,
    isCharity: false,
    recipientUserIds: ['00000000-0000-0000-0000-000000000002'],
    recipientCharityIds: [],
    notificationTypeId: 2,
    sentCount: 1,
    lastSentOn: '2026-09-10T09:00:00Z',
    createdOn: '2026-09-10T09:00:00Z'
  };

  const notificationLogSpy = jasmine.createSpyObj<NotificationLogService>('NotificationLogService', [
    'getNotificationById',
    'createNotification',
    'updateNotification',
    'resendNotification'
  ]);

  const userManagementSpy = jasmine.createSpyObj<UserManagementService>('UserManagementService', ['getUsers']);
  const charitySpy = jasmine.createSpyObj<CharityService>('CharityService', ['getCharities']);
  const notificationSpy = jasmine.createSpyObj<NotificationService>('NotificationService', ['success', 'error', 'confirm']);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, TranslateModule.forRoot()],
      declarations: [NotificationFormComponent],
      providers: [
        { provide: NotificationLogService, useValue: notificationLogSpy },
        { provide: UserManagementService, useValue: userManagementSpy },
        { provide: CharityService, useValue: charitySpy },
        { provide: NotificationService, useValue: notificationSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { paramMap: convertToParamMap({ id: stored.id }) }
          }
        }
      ]
    }).compileComponents();

    notificationLogSpy.getNotificationById.and.returnValue(of(stored));
    notificationLogSpy.createNotification.and.returnValue(of(stored));
    notificationLogSpy.updateNotification.and.returnValue(of(stored));
    notificationLogSpy.resendNotification.and.returnValue(of(stored));
    userManagementSpy.getUsers.and.returnValue(of({ items: [], totalCount: 0, page: 1, pageSize: 20 }));
    charitySpy.getCharities.and.returnValue(of({ items: [], totalCount: 0, page: 1, pageSize: 20 }));

    fixture = TestBed.createComponent(NotificationFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('edit mode loads the stored row into the form', () => {
    expect(component.isEditMode).toBeTrue();
    expect(component.notificationForm.value.title).toBe(stored.title);
    expect(component.notificationForm.value.message).toBe(stored.message);
    expect(component.notificationForm.value.isUser).toBeTrue();
    expect(component.notificationForm.value.recipientUserIds).toEqual(stored.recipientUserIds);
  });

  it('lowering the isUser flag disables and empties the user list', () => {
    component.notificationForm.get('isUser')!.setValue(false);
    const list = component.notificationForm.get('recipientUserIds')!;
    expect(list.disabled).toBeTrue();
    expect(list.value).toEqual([]);
  });

  it('resend posts to the resend endpoint and reports the audience', () => {
    component.onResend();
    expect(notificationLogSpy.resendNotification).toHaveBeenCalledWith(stored.id);
    expect(notificationSpy.success).toHaveBeenCalled();
  });

  it('a server validation map flags the matching controls', () => {
    component.isEditMode = false;
    notificationLogSpy.createNotification.and.returnValue(throwError(() => ({
      error: {
        message: 'invalid',
        errors: { Title: ['Title is required'], IsUser: ['Select at least one target'] }
      }
    })));
    component.notificationForm.patchValue({
      title: 't',
      message: 'm',
      isUser: true,
      recipientUserIds: ['00000000-0000-0000-0000-000000000002']
    });
    component.onSubmit();
    expect(component.notificationForm.get('title')!.errors?.['server']).toBe('Title is required');
    expect(component.notificationForm.get('isUser')!.errors?.['server']).toBe('Select at least one target');
  });

  it('a failed create surfaces the fallback message', () => {
    notificationLogSpy.createNotification.and.returnValue(throwError(() => ({ error: { message: 'boom' } })));
    component.isEditMode = false;
    component.notificationForm.patchValue({
      title: 't',
      message: 'm',
      isUser: true,
      recipientUserIds: ['00000000-0000-0000-0000-000000000002']
    });
    component.onSubmit();
    expect(notificationSpy.error).toHaveBeenCalledWith('boom');
  });
});
