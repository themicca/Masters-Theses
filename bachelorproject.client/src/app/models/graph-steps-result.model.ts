import { GraphResult } from "./graph-result.model";

export interface StepState {
  nodeColors: { [nodeId: string]: string }; // e.g., { A: 'orange', B: 'green' }
  edgeColors: { [edgeId: string]: string };
}

export interface GraghStepsResult {
  steps: StepState[];
  finalGraph: GraphResult;
}
