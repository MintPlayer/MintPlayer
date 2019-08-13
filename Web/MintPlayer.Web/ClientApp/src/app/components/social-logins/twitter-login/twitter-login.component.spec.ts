import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TwitterLoginComponent } from './twitter-login.component';

describe('TwitterLoginComponent', () => {
  let component: TwitterLoginComponent;
  let fixture: ComponentFixture<TwitterLoginComponent>;

  beforeEach(async(() => {
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
