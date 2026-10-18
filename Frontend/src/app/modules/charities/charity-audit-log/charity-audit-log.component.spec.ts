import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TranslateModule } from '@ngx-translate/core';
import { CharityAuditLogComponent } from './charity-audit-log.component';

describe('CharityAuditLogComponent', () => {
  let component: CharityAuditLogComponent;
  let fixture: ComponentFixture<CharityAuditLogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CharityAuditLogComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CharityAuditLogComponent);
    component = fixture.componentInstance;
    component.charityId = '00000000-0000-0000-0000-000000000001';
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should clear the expanded row when loading a new page', () => {
    component.expandedId = 'some-id';
    component.loadPage(2);
    expect(component.expandedId).toBeNull();
    expect(component.currentPage).toBe(2);
  });

  it('should toggle the expanded row', () => {
    component.expandedId = null;
    component.toggleExpanded('a');
    expect(component.expandedId).toBe('a');
    component.toggleExpanded('a');
    expect(component.expandedId).toBeNull();
  });
});
