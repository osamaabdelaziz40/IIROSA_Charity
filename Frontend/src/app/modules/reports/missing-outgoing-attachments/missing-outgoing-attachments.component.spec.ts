import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { MissingOutgoingAttachmentsComponent } from './missing-outgoing-attachments.component';

describe('MissingOutgoingAttachmentsComponent', () => {
  let component: MissingOutgoingAttachmentsComponent;
  let fixture: ComponentFixture<MissingOutgoingAttachmentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        ReactiveFormsModule,
        TranslateModule.forRoot(),
        MissingOutgoingAttachmentsComponent
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MissingOutgoingAttachmentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
