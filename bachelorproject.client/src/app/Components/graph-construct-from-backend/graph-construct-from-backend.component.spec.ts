import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GraphConstructFromBackendComponent } from './graph-construct-from-backend.component';

describe('GraphConstructFromBackendComponent', () => {
  let component: GraphConstructFromBackendComponent;
  let fixture: ComponentFixture<GraphConstructFromBackendComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [GraphConstructFromBackendComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(GraphConstructFromBackendComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
