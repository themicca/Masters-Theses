import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { GraphRequest } from '../models/graph-request.model';
import { GraghStepsResult } from '../models/graph-steps-result.model';

@Injectable({
  providedIn: 'root'
})
export class GraphResponseDataService {
  private graphDataSubject = new BehaviorSubject<any>(null);
  graphData$ = this.graphDataSubject.asObservable();

  updateGraphData(response: GraghStepsResult, graphData: GraphRequest) {
    this.graphDataSubject.next({ response, graphData });
  }
}
