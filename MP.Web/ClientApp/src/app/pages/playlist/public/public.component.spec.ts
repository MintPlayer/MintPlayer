import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { PlaylistPublicComponent } from './public.component';

describe('PublicComponent', () => {
  let component: PlaylistPublicComponent;
  let fixture: ComponentFixture<PlaylistPublicComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ PlaylistPublicComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlaylistPublicComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
