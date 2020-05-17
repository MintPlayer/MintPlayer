import { TestBed } from '@angular/core/testing';

import { TagCategoryService } from './tag-category.service';

describe('TagCategoryService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: TagCategoryService = TestBed.get(TagCategoryService);
    expect(service).toBeTruthy();
  });
});
