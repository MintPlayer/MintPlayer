import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { MicrosoftLoginComponent } from './microsoft-login.component';

describe('MicrosoftLoginComponent', () => {
  let component: MicrosoftLoginComponent;
  let fixture: ComponentFixture<MicrosoftLoginComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ MicrosoftLoginComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MicrosoftLoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
