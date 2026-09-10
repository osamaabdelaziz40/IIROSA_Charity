import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { CollapsibleCardComponent } from './collapsible-card.component';

describe('CollapsibleCardComponent', () => {
  let component: CollapsibleCardComponent;
  let fixture: ComponentFixture<CollapsibleCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CollapsibleCardComponent, TranslateModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(CollapsibleCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create collapsed by default', () => {
    expect(component).toBeTruthy();
    expect(component.expanded).toBeFalse();
  });

  it('toggle() should flip expanded and emit expandedChange', () => {
    const emitted: boolean[] = [];
    component.expandedChange.subscribe(v => emitted.push(v));

    component.toggle();
    expect(component.expanded).toBeTrue();
    expect(emitted).toEqual([true]);

    component.toggle();
    expect(component.expanded).toBeFalse();
    expect(emitted).toEqual([true, false]);
  });

  it('open() should be idempotent and emit only once', () => {
    const emitted: boolean[] = [];
    component.expandedChange.subscribe(v => emitted.push(v));

    component.open();
    component.open();

    expect(component.expanded).toBeTrue();
    expect(emitted).toEqual([true]);
  });

  it('should give each instance a unique body id', () => {
    const second = TestBed.createComponent(CollapsibleCardComponent).componentInstance;
    expect(second.bodyId).not.toBe(component.bodyId);
  });
});
