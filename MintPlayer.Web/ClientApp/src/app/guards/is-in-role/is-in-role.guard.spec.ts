import { TestBed } from '@angular/core/testing';

import { IsInRoleGuard } from './is-in-role.guard';

describe('IsInRoleGuard', () => {
  let guard: IsInRoleGuard;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    guard = TestBed.inject(IsInRoleGuard);
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });
});
