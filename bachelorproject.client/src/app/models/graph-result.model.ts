export interface GraphResult {
  nodeIds: string[],
  edgeIds: string[],
  totalWeight?: number | null,
  edgeResultWeights?: { [edgeId: string]: number | null },
  eulerType?: string | null,
  algoType: string
}
