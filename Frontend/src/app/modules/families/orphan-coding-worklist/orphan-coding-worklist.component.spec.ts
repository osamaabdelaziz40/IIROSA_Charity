import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { OrphanCodingWorklistComponent } from './orphan-coding-worklist.component';

describe('OrphanCodingWorklistComponent', () => {
  let component: OrphanCodingWorklistComponent;
  let fixture: ComponentFixture<OrphanCodingWorklistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        OrphanCodingWorklistComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OrphanCodingWorklistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
