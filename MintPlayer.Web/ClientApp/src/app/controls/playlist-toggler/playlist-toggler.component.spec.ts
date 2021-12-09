import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { PlaylistTogglerComponent } from './playlist-toggler.component';

describe('PlaylistTogglerComponent', () => {
  let component: PlaylistTogglerComponent;
  let fixture: ComponentFixture<PlaylistTogglerComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ PlaylistTogglerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlaylistTogglerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
