import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaylistCreateComponent } from './create.component';

describe('CreateComponent', () => {
  let component: PlaylistCreateComponent;
  let fixture: ComponentFixture<PlaylistCreateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [PlaylistCreateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlaylistCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
