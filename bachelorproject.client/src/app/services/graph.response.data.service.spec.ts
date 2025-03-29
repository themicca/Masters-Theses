import { TestBed } from '@angular/core/testing';

import { GraphResponseDataService } from './graph.response.data.service';

describe('GraphResponseDataService', () => {
  let service: GraphResponseDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GraphResponseDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
