export interface GraphNode {
  id: string;
  label: string;
  x: number;
  y: number;
  isStart?: boolean;
  isEnd?: boolean; 
}
