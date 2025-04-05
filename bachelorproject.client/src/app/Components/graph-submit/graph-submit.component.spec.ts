import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GraphSubmitComponent } from './graph-submit.component';

describe('NavbarComponent', () => {
  let component: GraphSubmitComponent;
  let fixture: ComponentFixture<GraphSubmitComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [GraphSubmitComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(GraphSubmitComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
