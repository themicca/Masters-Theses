import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EdgeWeightComponent } from './edge-weight.component';

describe('EdgeWeightComponent', () => {
  let component: EdgeWeightComponent;
  let fixture: ComponentFixture<EdgeWeightComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EdgeWeightComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EdgeWeightComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
