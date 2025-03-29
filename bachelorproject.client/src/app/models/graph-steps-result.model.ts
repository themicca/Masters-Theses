import { GraphRequest } from "./graph-request.model";

export interface EdgeState {
  source: string;
  target: string;
  color: string;
}

export interface StepState {
  nodeColors: { [nodeName: string]: string }; // e.g., { A: 'orange', B: 'green' }
  edges: EdgeState[];
}

export interface GraghStepsResult {
  steps: StepState[];
  finalGraph: GraphRequest; // or your existing GraphRequest interface
}
