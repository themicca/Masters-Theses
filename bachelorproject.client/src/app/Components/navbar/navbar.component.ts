import { Component, Input, OnDestroy } from '@angular/core';
import { GraphRequest } from '../../models/graph-request.model';
import { GraphService } from '../../services/graph.service';
import { Subscription } from 'rxjs';
import { GraphResponseDataService } from '../../services/graph.response.data.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnDestroy {
  @Input() graph!: joint.dia.Graph;
  @Input() graphSrc: joint.dia.Element | null = null;
  @Input() graphTarget: joint.dia.Element | null = null;
  @Input() directed!: boolean;

  showModal = false;
  algo: string = "Algo";
  errors: string[] = [];

  private addGraphSubscription?: Subscription
  private graphModel: GraphRequest = {
    graphName: "",
    graphNodes: [],
    graphEdges: [],
    graphSrc: "",
    graphTarget: "",
    graphDirected: false,
    graphNodePositions: []
  }

  constructor(private graphService: GraphService, private graphResponseDataSerivce: GraphResponseDataService) { }

  private extractGraphData(graphModel: GraphRequest): GraphRequest {
    const nodes = this.graph.getElements().map(node =>
      node.attr('label/text') || `Node-${node.id}`
    );
    const nodeIds = this.graph.getElements().map(node => node.id);
    const nodeIndexMap = new Map(nodeIds.map((id, index) => [id, index]));
    
    const matrixSize = nodes.length;
    const edges: number[][] = Array.from({ length: matrixSize }, () => Array(matrixSize).fill(0));
    
    this.graph.getLinks().forEach(link => {
      const sourceId = link.getSourceElement()?.id;
      const targetId = link.getTargetElement()?.id;
      if (sourceId && targetId) {
        const sourceIndex = nodeIndexMap.get(sourceId)!;
        const targetIndex = nodeIndexMap.get(targetId)!;
        
        const weight = link.attr('weight') || 1;
        edges[sourceIndex][targetIndex] = weight;

        if (!this.directed) {
          edges[targetIndex][sourceIndex] = weight;
        }
      }
    });

    const nodePositions = this.graph.getElements().map(node => {
      return node.position();
    });

    graphModel.graphName = "";
    graphModel.graphSrc = this.graphSrc ? this.graphSrc.attr('label/text') : null;
    graphModel.graphTarget = this.graphTarget ? this.graphTarget.attr('label/text') : null;
    graphModel.graphNodes = nodes;
    graphModel.graphEdges = edges;
    graphModel.graphDirected = this.directed;
    graphModel.graphNodePositions = nodePositions;

    return graphModel;
  }

  onSubmit(algo: string = 'Algo') {
    const graphData = this.extractGraphData(this.graphModel);
    this.errors = this.graphService.validate(algo, this.graph, graphData);
    
    if (this.errors.length > 0) {
      this.algo = algo;
      this.showModal = true;
      return;
    }

    this.addGraphSubscription = this.graphService.runAlgo(algo, graphData).subscribe({
      next: (response: any) => {
        console.log('Received Processed Graph:', response);
        this.graphResponseDataSerivce.updateGraphData(response);
      },
      error: (error) => {
        console.error('Error sending graph data:', error);
        this.errors.push(error.error);
        this.algo = algo;
        this.showModal = true;
      }
    });
  }

  closeModal() {
    this.showModal = false;
  }

  ngOnDestroy(): void {
    this.addGraphSubscription?.unsubscribe();
  }
}
