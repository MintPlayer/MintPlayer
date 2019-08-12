import { TestBed } from '@angular/core/testing';

import { ArtistService } from './artist.service';

describe('ArtistService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ArtistService = TestBed.get(ArtistService);
    expect(service).toBeTruthy();
  });
});
