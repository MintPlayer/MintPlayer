import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { YoutubePlayButtonComponent } from './youtube-play-button.component';

describe('YoutubePlayButtonComponent', () => {
  let component: YoutubePlayButtonComponent;
  let fixture: ComponentFixture<YoutubePlayButtonComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ YoutubePlayButtonComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(YoutubePlayButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
