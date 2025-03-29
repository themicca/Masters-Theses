import { Injectable } from '@angular/core';
import { GraphRequest } from '../models/graph-request.model';
import { Observable, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class GraphService {
  private graph!: joint.dia.Graph;
  private src: string | null = null;
  private target: string | null = null;
  private nodes!: string[];
  private edges!: number[][];
  private directed!: boolean;
  
  errors: string[] = [];

  constructor(private http: HttpClient) { }

  runAlgo(algo: string, model: GraphRequest): Observable<void> {
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

  runDijkstra(model: GraphRequest): Observable<void> {
    return this.http.post<void>('https://localhost:7130/api/Dijkstra', model)
  }

  runEdmondsKarp(model: GraphRequest): Observable<void> {
    return this.http.post<void>('https://localhost:7130/api/EdmondsKarp', model)
  }

  runHeldKarp(model: GraphRequest): Observable<void> {
    return this.http.post<void>('https://localhost:7130/api/HeldKarp', model)
  }

  runKruskal(model: GraphRequest): Observable<void> {
    return this.http.post<void>('https://localhost:7130/api/Kruskal', model)
  }

  runFleury(model: GraphRequest): Observable<void> {
    model.graphSrc = this.src;
    return this.http.post<void>('https://localhost:7130/api/Fleury', model)
  }

  runGreedyMatching(model: GraphRequest): Observable<void> {
    return this.http.post<void>('https://localhost:7130/api/GreedyMatching', model)
  }

  runGreedyColoring(model: GraphRequest): Observable<void> {
    return this.http.post<void>('https://localhost:7130/api/GreedyColoring', model)
  }

  runWelshPowell(model: GraphRequest): Observable<void> {
    return this.http.post<void>('https://localhost:7130/api/WelshPowell', model)
  }

  validate(algo: string, graph: joint.dia.Graph, graphModel: GraphRequest): string[]
  {
    this.src = graphModel.graphSrc;
    this.graph = graph;
    this.target = graphModel.graphTarget;
    this.nodes = graphModel.graphNodes;
    this.edges = graphModel.graphEdges;
    this.directed = graphModel.graphDirected;

    this.errors = [];

    switch (algo) {
      case "Dijkstra":
        this.validateDijkstra()
        break;
      case "Kruskal":
        this.validateKruskal()
        break;
      case "Edmonds-Karp":
        this.validateEdmondsKarp()
        break;
      case "Held-Karp":
        this.validateHeldKarp()
        break;
      case "Fleury":
        this.validateFleury()
        break;
      case "Greedy Matching":
        this.validateGreedyMatching()
        break;
      case "Greedy Coloring":
        this.validateGreedyColoring()
        break;
      case "Welsh-Powell":
        this.validateWelshPowell()
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
    if (!this.target) {
      this.errors.push('End node is not set.');
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
    this.graph!.getLinks().some(link => {
      if (link.attr('weight') < 1) {
        this.errors.push('There are negative edges.');
        return true;
      }
      return false;
    });
  }

  validateFleury() {
    // Eulerian condition check
    let nodesCount = this.graph.getElements().length;

    if (!this.directed) {
      let oddCount = 0;
      for (let i = 0; i < nodesCount; i++) {
        let degree = 0;
        for (let j = 0; j < nodesCount; j++) {
          if (this.edges[i][j] !== 0) {
            degree++;
          }
        }
        if (degree % 2 !== 0) {
          oddCount++;
          this.src = this.nodes[i]
        }
      }
      if (oddCount !== 0 && oddCount !== 2) {
        this.errors.push('Graph is not Eulerian.');
        this.errors.push(`It has ${oddCount} vertices with an odd degree.`)
        this.errors.push('Expected 0 for an Eulerian circuit or 2 for an Eulerian path in an undirected graph.')
        return;
      }
      if (oddCount === 0) {
        for (let i = 0; i < nodesCount; i++) {
          let degree = 0;
          for (let j = 0; j < nodesCount; j++) {
            if (this.edges[i][j] !== 0) {
              degree++;
            }
          }
          if (degree > 0) {
            this.src = this.nodes[i];
            break;
          }
        }
      }
    } else {
      const inDegree: number[] = new Array(nodesCount).fill(0);
      const outDegree: number[] = new Array(nodesCount).fill(0);

      for (let i = 0; i < nodesCount; i++) {
        for (let j = 0; j < nodesCount; j++) {
          if (this.edges[i][j] !== 0) {
            outDegree[i]++;
            inDegree[j]++;
          }
        }
      }

      let startNodes = 0;
      let endNodes = 0;

      for (let i = 0; i < nodesCount; i++) {
        if (outDegree[i] - inDegree[i] === 1) {
          startNodes++;
          this.src = this.nodes[i];
        } else if (inDegree[i] - outDegree[i] === 1) {
          endNodes++;
        } else if (inDegree[i] !== outDegree[i]) {
          this.errors.push('Graph is not Eulerian.');
          this.errors.push(`Vertex ${this.nodes[i]} has in-degree ${inDegree[i]} and out-degree ${outDegree[i]}.`)
          this.errors.push('That is not allowed in directed graphs.')
          return;
        }
      }

      if (!((startNodes === 1 && endNodes === 1) || (startNodes === 0 && endNodes === 0))) {
        this.errors.push('Graph is not Eulerian.');
        this.errors.push('Expected exactly 1 vertex with out-degree one greater than in-degree and 1 with in-degree one greater than out-degree for an Eulerian path or all vertices balanced for an Eulerian circuit.')
        this.errors.push(`but found ${startNodes} start node(s) and ${endNodes} end node(s).`)
        return;
      }
      
      if (startNodes === 0) {
        for (let i = 0; i < nodesCount; i++) {
          if (outDegree[i] > 0) {
            this.src = this.nodes[i];
            break;
          }
        }
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
