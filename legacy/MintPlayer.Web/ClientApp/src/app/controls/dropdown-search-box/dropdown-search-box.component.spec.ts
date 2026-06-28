import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { DropdownSearchBoxComponent } from './dropdown-search-box.component';

describe('DropdownSearchBoxComponent', () => {
  let component: DropdownSearchBoxComponent;
  let fixture: ComponentFixture<DropdownSearchBoxComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DropdownSearchBoxComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DropdownSearchBoxComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
