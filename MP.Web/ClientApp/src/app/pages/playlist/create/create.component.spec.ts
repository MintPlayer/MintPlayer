import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { PlaylistCreateComponent } from './create.component';

describe('PlaylistCreateComponent', () => {
  let component: PlaylistCreateComponent;
  let fixture: ComponentFixture<PlaylistCreateComponent>;

  beforeEach(waitForAsync(() => {
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
