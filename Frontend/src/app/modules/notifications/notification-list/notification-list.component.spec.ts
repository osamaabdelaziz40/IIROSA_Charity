/**
 * Notification List Component spec (UC-NTF list / my notifications).
 * Tests are excluded from the project's verify loop by standing decision; this file
 * documents the screen's binding contract for whenever they are re-enabled.
 */

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { TranslateModule } from '@ngx-translate/core';

import { NotificationListComponent } from './notification-list.component';
import { NotificationLogService } from '../services/notification-log.service';
import { AuthService } from '../../../core/services/auth.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { NotificationService } from '../../../core/services/notification.service';
import { NotificationsLog, NotificationsLogPagedResult } from '../models/notification.model';

describe('NotificationListComponent', () => {
  let component: NotificationListComponent;
  let fixture: ComponentFixture<NotificationListComponent>;

  const page: NotificationsLogPagedResult = {
    items: [
      {
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
      }
    ],
    totalCount: 1,
    page: 1,
    pageSize: 20
  };

  const notificationLogSpy = jasmine.createSpyObj<NotificationLogService>('NotificationLogService', [
    'getNotifications',
    'getMyNotifications',
    'resendNotification'
  ]);

  const authSpy = jasmine.createSpyObj<AuthService>('AuthService', ['hasAnyRole']);
  const signalRSpy = jasmine.createSpyObj<SignalRService>('SignalRService', [], { notifications$: of() });
  const notificationSpy = jasmine.createSpyObj<NotificationService>('NotificationService', ['success', 'error', 'confirm']);

  beforeEach(async () => {
    notificationLogSpy.getNotifications.and.returnValue(of(page));
    notificationLogSpy.getMyNotifications.and.returnValue(of(page));
    notificationLogSpy.resendNotification.and.returnValue(of(page.items[0]));
    authSpy.hasAnyRole.and.returnValue(true);
    notificationSpy.confirm.and.resolveTo(true);

    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, TranslateModule.forRoot()],
      declarations: [NotificationListComponent],
      providers: [
        { provide: NotificationLogService, useValue: notificationLogSpy },
        { provide: AuthService, useValue: authSpy },
        { provide: SignalRService, useValue: signalRSpy },
        { provide: NotificationService, useValue: notificationSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('admins read the full register', () => {
    expect(component.canManage).toBeTrue();
    expect(notificationLogSpy.getNotifications).toHaveBeenCalled();
    expect(component.notifications.length).toBe(1);
  });

  it('non-admins read their own notifications', () => {
    authSpy.hasAnyRole.and.returnValue(false);
    component.ngOnInit();
    expect(notificationLogSpy.getMyNotifications).toHaveBeenCalled();
    expect(component.pageActions).toEqual([]);
  });

  it('resend posts and reloads the current page', async () => {
    await component.resendNotification(page.items[0].id);
    expect(notificationLogSpy.resendNotification).toHaveBeenCalledWith(page.items[0].id);
    expect(notificationSpy.success).toHaveBeenCalled();
  });

  it('a failed load reports the translated error', () => {
    notificationLogSpy.getNotifications.and.returnValue(throwError(() => ({ error: {} })));
    component.loadNotifications();
    expect(notificationSpy.error).toHaveBeenCalled();
  });
});
