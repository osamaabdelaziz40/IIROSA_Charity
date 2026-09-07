/**
 * HQ Transfer List Component spec (epic 17, UC-TRF-01)
 * Tests are excluded from the delivery per the standing user decision — this file keeps the
 * 4-file component shape required by the project conventions.
 */

import { HqTransferListComponent } from './hq-transfer-list.component';

describe('HqTransferListComponent', () => {
  it('should create', () => {
    const component = new HqTransferListComponent(
      {} as any,
      {} as any,
      {} as any,
      {} as any,
      {} as any
    );
    expect(component).toBeTruthy();
  });
});
