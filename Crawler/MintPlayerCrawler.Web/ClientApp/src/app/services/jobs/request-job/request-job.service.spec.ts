import { TestBed, inject } from '@angular/core/testing';

import { RequestJobService } from './request-job.service';

describe('RequestJobService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [RequestJobService]
    });
  });

  it('should be created', inject([RequestJobService], (service: RequestJobService) => {
    expect(service).toBeTruthy();
  }));
});
