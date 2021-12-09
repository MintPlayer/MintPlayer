import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { TwitterLoginComponent } from './twitter-login.component';

describe('TwitterLoginComponent', () => {
  let component: TwitterLoginComponent;
  let fixture: ComponentFixture<TwitterLoginComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ TwitterLoginComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TwitterLoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
