import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { OrphanCodingComponent } from './orphan-coding.component';

describe('OrphanCodingComponent', () => {
  let component: OrphanCodingComponent;
  let fixture: ComponentFixture<OrphanCodingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        OrphanCodingComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OrphanCodingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
