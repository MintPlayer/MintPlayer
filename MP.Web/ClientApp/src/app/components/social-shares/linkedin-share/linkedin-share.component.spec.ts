import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { LinkedinShareComponent } from './linkedin-share.component';

describe('LinkedinShareComponent', () => {
  let component: LinkedinShareComponent;
  let fixture: ComponentFixture<LinkedinShareComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ LinkedinShareComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LinkedinShareComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
