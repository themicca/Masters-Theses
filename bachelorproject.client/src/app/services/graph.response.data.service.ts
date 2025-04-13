import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Graph } from '../models/graph.model';
import { GraphStepsResult } from '../models/graph-steps-result.model';

@Injectable({
  providedIn: 'root'
})
export class GraphResponseDataService {
  private graphDataSubject = new BehaviorSubject<any>(null);
  graphData$ = this.graphDataSubject.asObservable();

  updateGraphData(response: GraphStepsResult, graphData: Graph) {
    this.graphDataSubject.next({ response, graphData });
  }
}
