export interface GraphRequest {
  graphName: string;
  graphNodes: string[];
  graphEdges: number[][];
  graphSrc: string | null;
  graphTarget: string | null;
  graphDirected: boolean;
  graphNodePositions: { x: number; y: number }[];
}
