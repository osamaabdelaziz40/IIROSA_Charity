import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { ProviderSponsorChangesComponent } from './provider-sponsor-changes.component';

describe('ProviderSponsorChangesComponent', () => {
  let component: ProviderSponsorChangesComponent;
  let fixture: ComponentFixture<ProviderSponsorChangesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ProviderSponsorChangesComponent,
        ReactiveFormsModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProviderSponsorChangesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
