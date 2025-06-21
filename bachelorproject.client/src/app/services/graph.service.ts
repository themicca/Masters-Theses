import { Injectable } from '@angular/core';
import { Graph } from '../models/graph.model';
import { Observable, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Node } from '../models/node.model';
import { Edge } from '../models/edge.model';
import { GraphStepsResult } from '../models/graph-steps-result.model';
import { API_URL, DIJKSTRA_NAME, EDMONDS_KARP_NAME, FLEURY_NAME, GREEDY_COLORING_NAME, GREEDY_MATCHING_NAME, HELD_KARP_NAME, KRUSKAL_NAME, MAX_NODES_HELD_KARP, WELSH_POWELL_NAME } from '../utils/constants';

@Injectable({
  providedIn: 'root'
})
export class GraphService {
  private graph!: joint.dia.Graph;
  private src: string | null = null;
  private target: string | null = null;
  private nodes!: Node[];
  private edges!: Edge[];
  private directed!: boolean;
  private weighted!: boolean;
  private eulerType: string = "";
  
  errors: string[] = [];

  constructor(private http: HttpClient) { }

  runAlgo(algo: string, model: Graph): Observable<GraphStepsResult> {
    switch (algo) {
      case DIJKSTRA_NAME:
        return this.runDijkstra(model);
      case KRUSKAL_NAME:
        return this.runKruskal(model);
      case EDMONDS_KARP_NAME:
        return this.runEdmondsKarp(model);
      case HELD_KARP_NAME:
        return this.runHeldKarp(model);
      case FLEURY_NAME:
        return this.runFleury(model);
      case GREEDY_MATCHING_NAME:
        return this.runGreedyMatching(model);
      case GREEDY_COLORING_NAME:
        return this.runGreedyColoring(model);
      case WELSH_POWELL_NAME:
        return this.runWelshPowell(model);
      default:
        return throwError(() => new Error(`Unknown algorithm: ${algo}`));
    }
  }

  runDijkstra(model: Graph): Observable<GraphStepsResult> {
    return this.http.post<GraphStepsResult>(`${API_URL}/${DIJKSTRA_NAME}`, model);
  }

  runEdmondsKarp(model: Graph): Observable<GraphStepsResult> {
    return this.http.post<GraphStepsResult>(`${API_URL}/${EDMONDS_KARP_NAME}`, model);
  }

  runHeldKarp(model: Graph): Observable<GraphStepsResult> {
    return this.http.post<GraphStepsResult>(`${API_URL}/${HELD_KARP_NAME}`, model);
  }

  runKruskal(model: Graph): Observable<GraphStepsResult> {
    return this.http.post<GraphStepsResult>(`${API_URL}/${KRUSKAL_NAME}`, model);
  }

  runFleury(model: Graph): Observable<GraphStepsResult> {
    model.src = this.src;
    model.eulerType = this.eulerType;
    return this.http.post<GraphStepsResult>(`${API_URL}/${FLEURY_NAME}`, model);
  }

  runGreedyMatching(model: Graph): Observable<GraphStepsResult> {
    return this.http.post<GraphStepsResult>(`${API_URL}/${GREEDY_MATCHING_NAME}`, model);
  }

  runGreedyColoring(model: Graph): Observable<GraphStepsResult> {
    return this.http.post<GraphStepsResult>(`${API_URL}/${GREEDY_COLORING_NAME}`, model);
  }

  runWelshPowell(model: Graph): Observable<GraphStepsResult> {
    return this.http.post<GraphStepsResult>(`${API_URL}/${WELSH_POWELL_NAME}`, model);
  }

  validate(algo: string, graph: joint.dia.Graph, model: Graph): string[]
  {
    this.src = model.src;
    this.graph = graph;
    this.target = model.target;
    this.nodes = model.nodes;
    this.edges = model.edges;
    this.directed = model.isDirected;
    this.weighted = model.isWeighted;

    this.errors = [];

    switch (algo) {
      case DIJKSTRA_NAME:
        this.validateDijkstra();
        break;
      case KRUSKAL_NAME:
        this.validateKruskal();
        break;
      case EDMONDS_KARP_NAME:
        this.validateEdmondsKarp();
        break;
      case HELD_KARP_NAME:
        this.validateHeldKarp();
        break;
      case FLEURY_NAME:
        this.validateFleury();
        break;
      case GREEDY_MATCHING_NAME:
        this.validateGreedyMatching();
        break;
      case GREEDY_COLORING_NAME:
        this.validateGreedyColoring();
        break;
      case WELSH_POWELL_NAME:
        this.validateWelshPowell();
        break;
      default:
        this.errors.push('Invalid algorithm.');
    }

    return this.errors;
  }

  validateDijkstra() {
    if (!this.src) {
      this.errors.push('Start node is not set.');
    }
    if (!this.weighted) {
      this.errors.push('Requires weighted graph.');
    }
    this.graph!.getLinks().some(link => {
      if (link.attr('weight') < 1) {
        this.errors.push('There are negative edges.');
        return true;
      }
      return false;
    });
    if (!this.isStartToEndReachable())
      this.errors.push("End node is set, but no path from start to end exists.");
  }

  validateHeldKarp() {
    if (!this.src) {
      this.errors.push('Start node is not set.');
    }
    if (!this.weighted) {
      this.errors.push('Requires weighted graph.');
    }
    this.graph!.getLinks().some(link => {
      if (link.attr('weight') < 1) {
        this.errors.push('There are negative edges.');
        return true;
      }
      return false;
    });

    if (this.graph.getElements().length >= MAX_NODES_HELD_KARP) {
      this.errors.push(`Held-Karp cannot be run with more than ${MAX_NODES_HELD_KARP} nodes for performance reasons.`);
    }
    
    const matrix = this.buildAdjacencyMatrix(this.nodes, this.edges);

    const visited: boolean[] = new Array(matrix.length).fill(false);
    visited[0] = true;

    this.isGraphConnected();
    if (!this.hamiltonianCycle(matrix, 0, 0, visited, 1)) {
      this.errors.push('Graph does not contain a Hamiltonian Cycle.');
    }
  }

  validateKruskal() {
    if (this.directed) {
      this.errors.push('Requires undirected graph.');
    }
    if (!this.weighted) {
      this.errors.push('Requires weighted graph.');
    }
  }

  validateEdmondsKarp() {
    if (!this.src) {
      this.errors.push('Source node is not set.');
    }
    if (!this.target) {
      this.errors.push('Sink node is not set.');
    }
    if (!this.directed) {
      this.errors.push('Requires directed graph.');
    }
    if (!this.weighted) {
      this.errors.push('Requires weighted graph.');
    }
    this.graph!.getLinks().some(link => {
      if (link.attr('weight') < 1) {
        this.errors.push('There are negative edges.');
        return true;
      }
      return false;
    });
    if (!this.isStartToEndReachable())
      this.errors.push("No path from source to sink exists.");
  }

  validateFleury() {
    this.src = null;

    if (this.directed) {
      this.errors.push('Requires undirected graph.');
    }
    if (this.weighted) {
      this.errors.push('Requires unweighted graph.');
    }

    this.isGraphConnected();
    this.eulerGraphConditionsCheck();
  }

  validateGreedyMatching() {
    if (this.directed) {
      this.errors.push('Requires undirected graph.');
    }
    if (this.weighted) {
      this.errors.push('Requires unweighted graph.');
    }
  }

  validateGreedyColoring() {
    if (this.directed) {
      this.errors.push('Requires undirected graph.');
    }
    if (this.weighted) {
      this.errors.push('Requires unweighted graph.');
    }
  }

  validateWelshPowell() {
    if (this.directed) {
      this.errors.push('Requires undirected graph.');
    }
    if (this.weighted) {
      this.errors.push('Requires unweighted graph.');
    }
  }

  private eulerGraphConditionsCheck() {
    if (!this.directed) {
      let oddNodes: string[] = [];

      for (const node of this.nodes) {
        const degree = this.edges.filter(e =>
          e.sourceNodeId === node.id || e.targetNodeId === node.id
        ).length;

        if (degree % 2 !== 0) {
          oddNodes.push(node.id);
        }
      }

      if (oddNodes.length === 0) {
        this.eulerType = "cycle";
        const candidate = this.nodes.find(n =>
          this.edges.some(e => e.sourceNodeId === n.id || e.targetNodeId === n.id)
        );
        if (candidate) {
          this.src = candidate.id;
        }
      } else if (oddNodes.length === 2) {
        this.eulerType = "path";
        this.src = oddNodes[Math.floor(Math.random() * 2)];
      } else {
        this.errors.push('Graph is not Eulerian.');
        this.errors.push(`It has ${oddNodes.length} vertices with an odd degree.`)
        this.errors.push('Expected 0 for an Eulerian circuit or 2 for an Eulerian path in an undirected graph.')
      }

    } else {
      const inDegreeMap = new Map<string, number>();
      const outDegreeMap = new Map<string, number>();

      for (const node of this.nodes) {
        inDegreeMap.set(node.id, 0);
        outDegreeMap.set(node.id, 0);
      }

      for (const edge of this.edges) {
        outDegreeMap.set(edge.sourceNodeId, (outDegreeMap.get(edge.sourceNodeId) || 0) + 1);
        inDegreeMap.set(edge.targetNodeId, (inDegreeMap.get(edge.targetNodeId) || 0) + 1);
      }

      let startCandidates: string[] = [];
      let endCandidates: string[] = [];

      for (const node of this.nodes) {
        const inDeg = inDegreeMap.get(node.id) || 0;
        const outDeg = outDegreeMap.get(node.id) || 0;

        if (outDeg - inDeg === 1) {
          startCandidates.push(node.id);
        } else if (inDeg - outDeg === 1) {
          endCandidates.push(node.id);
        } else if (inDeg !== outDeg) {
          this.errors.push('Graph is not Eulerian.');
          this.errors.push(`Vertex ${node.label} has in-degree ${inDeg} and out-degree ${outDeg}.`)
          this.errors.push('That is not allowed in directed graphs.');
          return;
        }
      }

      if (startCandidates.length === 1 && endCandidates.length === 1) {
        this.eulerType = "path";
        this.src = startCandidates[0];
      } else if (startCandidates.length === 0 && endCandidates.length === 0) {
        this.eulerType = "cycle";
        const candidate = this.nodes.find(n => (outDegreeMap.get(n.id) || 0) > 0);
        if (candidate) {
          this.src = candidate.id;
        }
      } else {
        this.errors.push('Graph is not Eulerian.');
        this.errors.push('Expected exactly 1 vertex with out-degree one greater than in-degree and 1 with in-degree one greater than out-degree for an Eulerian path or all vertices balanced for an Eulerian circuit.');
        this.errors.push(`But found ${startCandidates.length} start node(s) and ${endCandidates.length} end node(s).`);
      }
    }
  }

  private buildAdjacencyMatrix(nodes: Node[], edges: Edge[]): number[][] {
    const n = nodes.length;
    const nodeIndexMap = new Map<string, number>();
    nodes.forEach((node, index) => {
      nodeIndexMap.set(node.id, index);
    });
    
    const matrix: number[][] = Array.from({ length: n }, () => Array(n).fill(0));
    
    edges.forEach(edge => {
      const sourceIndex = nodeIndexMap.get(edge.sourceNodeId);
      const targetIndex = nodeIndexMap.get(edge.targetNodeId);
      if (sourceIndex !== undefined && targetIndex !== undefined) {
        matrix[sourceIndex][targetIndex] = edge.weight;
        if (!this.directed) {
          matrix[targetIndex][sourceIndex] = edge.weight;
        }
      }
    });

    return matrix;
  }
  
  private hamiltonianCycle(
    matrix: number[][],
    start: number,
    current: number,
    visited: boolean[],
    count: number
  ): boolean {
    const n = matrix.length;
    
    if (count === n) {
      return matrix[current][start] !== 0;
    }

    for (let next = 0; next < n; next++) {
      if (!visited[next] && matrix[current][next] !== 0) {
        visited[next] = true;
        if (this.hamiltonianCycle(matrix, start, next, visited, count + 1)) {
          return true;
        }
        visited[next] = false;
      }
    }
    return false;
  }

  public isGraphConnected() {
    console.log(this.nodes.length);
    if (this.nodes.length <= 1) {
      return;
    }
    
    const visited = new Set<string>();
    const queue: string[] = [];
    
    const startNode = this.nodes[0].id;
    visited.add(startNode);
    queue.push(startNode);
    
    const adjacencyMap = new Map<string, string[]>();
    
    this.nodes.forEach(node => {
      adjacencyMap.set(node.id, []);
    });

    this.edges.forEach(edge => {
      adjacencyMap.get(edge.sourceNodeId)?.push(edge.targetNodeId);
      adjacencyMap.get(edge.targetNodeId)?.push(edge.sourceNodeId);
    });

    while (queue.length > 0) {
      const current = queue.shift()!;
      const neighbors = adjacencyMap.get(current) || [];
      for (const neighbor of neighbors) {
        if (!visited.has(neighbor)) {
          visited.add(neighbor);
          queue.push(neighbor);
        }
      }
    }
    
    if (visited.size != this.nodes.length)
      this.errors.push("Connected graph required.");
  }

  public isStartToEndReachable(): boolean {
    if (!this.src || !this.target) {
      return true;
    }

    const visited = new Set<string>();
    const queue: string[] = [];

    queue.push(this.src);
    visited.add(this.src);

    const adjacencyMap = new Map<string, string[]>();
    this.nodes.forEach(node => {
      adjacencyMap.set(node.id, []);
    });

    this.edges.forEach(edge => {
      adjacencyMap.get(edge.sourceNodeId)?.push(edge.targetNodeId);
    });

    while (queue.length > 0) {
      const current = queue.shift()!;
      if (current === this.target) {
        return true;
      }

      for (const neighbor of adjacencyMap.get(current) || []) {
        if (!visited.has(neighbor)) {
          visited.add(neighbor);
          queue.push(neighbor);
        }
      }
    }

    return false;
  }
}
