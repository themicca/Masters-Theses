import { GraphResult } from "./graph-result.model";

export interface GraghStepsResult {
  steps: Step[],
  resultGraph: GraphResult,
}

export interface Step {
  nodeColors: { [nodeId: string]: string },
  edgeColors: { [edgeId: string]: string },
  edgeCurrentWeights: { [edgeId: string]: number | null },
  currentTotalWeight?: number | null
}
