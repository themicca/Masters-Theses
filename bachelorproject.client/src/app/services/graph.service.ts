import { Injectable } from '@angular/core';
import { GraphRequest } from '../models/graph-request.model';
import { Observable, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { GraphNode } from '../models/graph-node.model';
import { GraphEdge } from '../models/graph-edge.model';
import { GraghStepsResult } from '../models/graph-steps-result.model';

@Injectable({
  providedIn: 'root'
})
export class GraphService {
  private graph!: joint.dia.Graph;
  private src: string | null = null;
  private target: string | null = null;
  private nodes!: GraphNode[];
  private edges!: GraphEdge[];
  private directed!: boolean;
  private eulerType: string = "";
  
  errors: string[] = [];

  constructor(private http: HttpClient) { }

  runAlgo(algo: string, model: GraphRequest): Observable<GraghStepsResult> {
    switch (algo) {
      case "Dijkstra":
        return this.runDijkstra(model);
      case "Kruskal":
        return this.runKruskal(model);
      case "Edmonds-Karp":
        return this.runEdmondsKarp(model);
      case "Held-Karp":
        return this.runHeldKarp(model);
      case "Fleury":
        return this.runFleury(model);
      case "Greedy Matching":
        return this.runGreedyMatching(model);
      case "Greedy Coloring":
        return this.runGreedyColoring(model);
      case "Welsh-Powell":
        return this.runWelshPowell(model);
      default:
        return throwError(() => new Error(`Unknown algorithm: ${algo}`));
    }
  }

  runDijkstra(model: GraphRequest): Observable<GraghStepsResult> {
    return this.http.post<GraghStepsResult>('https://localhost:7130/api/Dijkstra', model);
  }

  runEdmondsKarp(model: GraphRequest): Observable<GraghStepsResult> {
    return this.http.post<GraghStepsResult>('https://localhost:7130/api/EdmondsKarp', model);
  }

  runHeldKarp(model: GraphRequest): Observable<GraghStepsResult> {
    return this.http.post<GraghStepsResult>('https://localhost:7130/api/HeldKarp', model);
  }

  runKruskal(model: GraphRequest): Observable<GraghStepsResult> {
    return this.http.post<GraghStepsResult>('https://localhost:7130/api/Kruskal', model);
  }

  runFleury(model: GraphRequest): Observable<GraghStepsResult> {
    model.src = this.src;
    model.eulerType = this.eulerType;
    return this.http.post<GraghStepsResult>('https://localhost:7130/api/Fleury', model);
  }

  runGreedyMatching(model: GraphRequest): Observable<GraghStepsResult> {
    return this.http.post<GraghStepsResult>('https://localhost:7130/api/GreedyMatching', model);
  }

  runGreedyColoring(model: GraphRequest): Observable<GraghStepsResult> {
    return this.http.post<GraghStepsResult>('https://localhost:7130/api/GreedyColoring', model);
  }

  runWelshPowell(model: GraphRequest): Observable<GraghStepsResult> {
    return this.http.post<GraghStepsResult>('https://localhost:7130/api/WelshPowell', model);
  }

  validate(algo: string, graph: joint.dia.Graph, model: GraphRequest): string[]
  {
    this.src = model.src;
    this.graph = graph;
    this.target = model.target;
    this.nodes = model.nodes;
    this.edges = model.edges;
    this.directed = model.isDirected;

    this.errors = [];

    switch (algo) {
      case "Dijkstra":
        this.validateDijkstra();
        break;
      case "Kruskal":
        this.validateKruskal();
        break;
      case "Edmonds-Karp":
        this.validateEdmondsKarp();
        break;
      case "Held-Karp":
        this.validateHeldKarp();
        break;
      case "Fleury":
        this.validateFleury();
        break;
      case "Greedy Matching":
        this.validateGreedyMatching();
        break;
      case "Greedy Coloring":
        this.validateGreedyColoring();
        break;
      case "Welsh-Powell":
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
    this.graph!.getLinks().some(link => {
      if (link.attr('weight') < 1) {
        this.errors.push('There are negative edges.');
        return true;
      }
      return false;
    });
  }

  validateHeldKarp() {
    if (!this.src) {
      this.errors.push('Start node is not set.');
    }
    this.graph!.getLinks().some(link => {
      if (link.attr('weight') < 1) {
        this.errors.push('There are negative edges.');
        return true;
      }
      return false;
    });
  }

  validateKruskal() {
    if (this.directed) {
      this.errors.push('Requires undirected graph.');
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
    this.graph!.getLinks().some(link => {
      if (link.attr('weight') < 1) {
        this.errors.push('There are negative edges.');
        return true;
      }
      return false;
    });
  }

  validateFleury() {
    this.src = null; // reset

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

  validateGreedyMatching() {
    if (this.directed) {
      this.errors.push('Requires undirected graph.');
    }
  }

  validateGreedyColoring() {
    if (this.directed) {
      this.errors.push('Requires undirected graph.');
    }
  }

  validateWelshPowell() {
  }

  /*addGraph(model: GraphRequest): Observable<void> {
    return this.http.post<void>('https://localhost:7130/api/Graphs', model)
  }

  solveGraph(model: GraphRequest): Observable<void> {
    return this.http.post<void>('https://localhost:7130/api/Djikstra', model)
  }*/
}
