import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EdgesConnectionComponent } from './edges-connection.component';

describe('EdgesConnectionComponent', () => {
  let component: EdgesConnectionComponent;
  let fixture: ComponentFixture<EdgesConnectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EdgesConnectionComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EdgesConnectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
