import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PerformComponent } from './perform.component';

describe('PerformComponent', () => {
  let component: PerformComponent;
  let fixture: ComponentFixture<PerformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PerformComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PerformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
