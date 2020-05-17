import { TestBed } from '@angular/core/testing';

import { MediumTypeService } from './medium-type.service';

describe('MediumTypeService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: MediumTypeService = TestBed.get(MediumTypeService);
    expect(service).toBeTruthy();
  });
});
