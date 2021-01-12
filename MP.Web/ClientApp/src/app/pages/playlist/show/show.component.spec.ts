import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaylistShowComponent } from './show.component';

describe('ShowComponent', () => {
  let component: PlaylistShowComponent;
  let fixture: ComponentFixture<PlaylistShowComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [PlaylistShowComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlaylistShowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
