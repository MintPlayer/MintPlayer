import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SortableListComponent } from './sortable-list.component';

describe('SortableListComponent', () => {
  let component: SortableListComponent;
  let fixture: ComponentFixture<SortableListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SortableListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SortableListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
