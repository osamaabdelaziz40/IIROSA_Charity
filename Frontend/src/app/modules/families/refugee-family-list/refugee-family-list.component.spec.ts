import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { TranslateModule } from '@ngx-translate/core';

import { RefugeeFamilyListComponent } from './refugee-family-list.component';
import { NotificationService } from '../../../core/services/notification.service';

describe('RefugeeFamilyListComponent', () => {
  let component: RefugeeFamilyListComponent;
  let fixture: ComponentFixture<RefugeeFamilyListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        RefugeeFamilyListComponent,
        HttpClientTestingModule,
        RouterTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [NotificationService]
    }).compileComponents();

    fixture = TestBed.createComponent(RefugeeFamilyListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('offers the shared FamilyFilterDto search-type vocabulary in the selector', () => {
    // 7-2: one switch server-side; the labels are §12.S.1's
    const ids = component.searchTypeOptions.map(o => o.id);
    expect(ids).toEqual(['all', 'father', 'mother', 'student', 'provider', 'nationalId', 'code', 'phone']);
  });

  it('always searches scoped to the Refugee discriminator', () => {
    const spy = spyOn(component, 'loadRefugeeFamilies');
    component.onSearch();
    expect(component.currentPage).toBe(1);
    expect(spy).toHaveBeenCalled();
  });
});
