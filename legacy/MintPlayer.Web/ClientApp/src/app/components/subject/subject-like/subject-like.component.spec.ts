import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SubjectLikeComponent } from './subject-like.component';

describe('SubjectLikeComponent', () => {
  let component: SubjectLikeComponent;
  let fixture: ComponentFixture<SubjectLikeComponent>;

  beforeEach(waitForAsync(() => {
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
