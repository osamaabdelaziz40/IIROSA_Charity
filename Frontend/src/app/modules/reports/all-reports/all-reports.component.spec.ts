import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { TranslateModule } from '@ngx-translate/core';

import { AllReportsComponent } from './all-reports.component';
import { AuthService } from '../../../core/services/auth.service';

describe('AllReportsComponent', () => {
  let component: AllReportsComponent;
  let fixture: ComponentFixture<AllReportsComponent>;
  let authService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    authService = jasmine.createSpyObj<AuthService>('AuthService', ['hasPermission']);

    await TestBed.configureTestingModule({
      imports: [
        RouterTestingModule,
        TranslateModule.forRoot(),
        AllReportsComponent
      ],
      providers: [
        { provide: AuthService, useValue: authService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AllReportsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    authService.hasPermission.and.returnValue(true);
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('shows every card when the caller holds both report permissions', () => {
    authService.hasPermission.and.returnValue(true);
    fixture.detectChanges();
    expect(component.cards.length).toBe(26);
  });

  it('hides the Families.FollowUp card when the caller lacks that permission', () => {
    authService.hasPermission.and.callFake(permission => permission !== 'Families.FollowUp');
    fixture.detectChanges();
    expect(component.cards.length).toBe(25);
    expect(component.cards.some(card => card.route === '/reports/family-orphans')).toBeFalse();
  });

  it('keeps only the follow-up card for a Families.FollowUp-only caller', () => {
    authService.hasPermission.and.callFake(permission => permission === 'Families.FollowUp');
    fixture.detectChanges();
    expect(component.cards.length).toBe(1);
    expect(component.cards[0].route).toBe('/reports/family-orphans');
  });
});
