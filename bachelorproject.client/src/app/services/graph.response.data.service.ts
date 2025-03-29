import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { GraphRequest } from '../models/graph-request.model';

@Injectable({
  providedIn: 'root'
})
export class GraphResponseDataService {
  private graphDataSubject = new BehaviorSubject<any>(null);
  graphData$ = this.graphDataSubject.asObservable();

  updateGraphData(graphData: GraphRequest) {
    this.graphDataSubject.next(graphData);
  }
}
