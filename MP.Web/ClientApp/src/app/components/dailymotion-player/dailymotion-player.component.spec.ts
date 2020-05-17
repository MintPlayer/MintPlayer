import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DailymotionPlayerComponent } from './dailymotion-player.component';

describe('DailymotionPlayerComponent', () => {
  let component: DailymotionPlayerComponent;
  let fixture: ComponentFixture<DailymotionPlayerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DailymotionPlayerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DailymotionPlayerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
