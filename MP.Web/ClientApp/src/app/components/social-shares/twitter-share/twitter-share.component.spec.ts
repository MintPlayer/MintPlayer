import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TwitterShareComponent } from './twitter-share.component';

describe('TwitterShareComponent', () => {
  let component: TwitterShareComponent;
  let fixture: ComponentFixture<TwitterShareComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TwitterShareComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TwitterShareComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
