import { GraphResult } from "./graph-result.model";

export interface GraghStepsResult {
  steps: StepState[],
  resultGraph: GraphResult,
}

export interface StepState {
  nodeColors: { [nodeId: string]: string },
  edgeColors: { [edgeId: string]: string },
  edgeCurrentWeights: { [edgeId: string]: number | null },
  currentTotalWeight?: number | null
}
