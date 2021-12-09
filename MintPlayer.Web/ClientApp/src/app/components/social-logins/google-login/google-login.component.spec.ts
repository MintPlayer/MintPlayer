import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { GoogleLoginComponent } from './google-login.component';

describe('GoogleLoginComponent', () => {
  let component: GoogleLoginComponent;
  let fixture: ComponentFixture<GoogleLoginComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ GoogleLoginComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GoogleLoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
