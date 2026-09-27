import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { NavbarTogglerComponent } from './navbar-toggler.component';

describe('NavbarTogglerComponent', () => {
  let component: NavbarTogglerComponent;
  let fixture: ComponentFixture<NavbarTogglerComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ NavbarTogglerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NavbarTogglerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
