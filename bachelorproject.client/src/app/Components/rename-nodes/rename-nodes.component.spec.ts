import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RenameNodesComponent } from './rename-nodes.component';

describe('RenameNodesComponent', () => {
  let component: RenameNodesComponent;
  let fixture: ComponentFixture<RenameNodesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [RenameNodesComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(RenameNodesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
