import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DropdownSearchBoxComponent } from './dropdown-search-box.component';

describe('DropdownSearchBoxComponent', () => {
  let component: DropdownSearchBoxComponent;
  let fixture: ComponentFixture<DropdownSearchBoxComponent>;

  beforeEach(async(() => {
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
