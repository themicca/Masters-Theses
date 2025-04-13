import { Edge } from "./edge.model";
import { Node } from "./node.model";

export interface Graph {
  id: string,
  name: string,
  nodes: Node[],
  edges: Edge[],
  src: string | null,
  target: string | null,
  isDirected: boolean,
  isWeighted: boolean
  eulerType?: string | null
}
