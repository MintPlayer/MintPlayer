import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SubjectLikeComponent } from './subject-like.component';

describe('SubjectLikeComponent', () => {
  let component: SubjectLikeComponent;
  let fixture: ComponentFixture<SubjectLikeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SubjectLikeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SubjectLikeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
