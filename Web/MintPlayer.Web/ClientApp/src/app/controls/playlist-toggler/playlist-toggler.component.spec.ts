import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaylistTogglerComponent } from './playlist-toggler.component';

describe('PlaylistTogglerComponent', () => {
  let component: PlaylistTogglerComponent;
  let fixture: ComponentFixture<PlaylistTogglerComponent>;

  beforeEach(async(() => {
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
