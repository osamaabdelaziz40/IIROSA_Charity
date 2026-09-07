/**
 * HQ Transfer Form Component spec (epic 17, UC-TRF-02)
 * Tests are excluded from the delivery per the standing user decision — this file keeps the
 * 4-file component shape required by the project conventions.
 */

import { FormBuilder } from '@angular/forms';
import { HqTransferFormComponent } from './hq-transfer-form.component';

describe('HqTransferFormComponent', () => {
  it('should create', () => {
    const component = new HqTransferFormComponent(
      new FormBuilder(), // real — the constructor immediately builds the reactive form
      {} as any,
      {} as any,
      {} as any,
      {} as any,
      {} as any,
      {} as any
    );
    expect(component).toBeTruthy();
  });
});
