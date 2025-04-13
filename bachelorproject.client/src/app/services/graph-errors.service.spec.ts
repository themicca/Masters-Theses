import { TestBed } from '@angular/core/testing';

import { GraphErrorsService } from './graph-errors.service';

describe('GraphErrorsService', () => {
  let service: GraphErrorsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GraphErrorsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
