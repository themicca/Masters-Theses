import { GraphEdge } from "./graph-edge.model";
import { GraphNode } from "./graph-node.model";

export interface GraphRequest {
  id: string;
  name: string;
  nodes: GraphNode[];
  edges: GraphEdge[];
  src: string | null;
  target: string | null;
  isDirected: boolean;
  isWeighted: boolean;
}
