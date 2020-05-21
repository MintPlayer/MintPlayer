import { TestBed } from '@angular/core/testing';

import { HasChangesGuard } from './has-changes.guard';

describe('HasChangesGuard', () => {
  let guard: HasChangesGuard;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    guard = TestBed.inject(HasChangesGuard);
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });
});
