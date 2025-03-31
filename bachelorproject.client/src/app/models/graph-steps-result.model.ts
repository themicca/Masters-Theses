import { GraphRequest } from "./graph-request.model";

export interface EdgeState {
  edge: string;
  color: string;
}

export interface StepState {
  nodeColors: { [nodeId: string]: string }; // e.g., { A: 'orange', B: 'green' }
  edges: EdgeState[];
}

export interface GraghStepsResult {
  steps: StepState[];
  finalGraph: GraphRequest;
}
