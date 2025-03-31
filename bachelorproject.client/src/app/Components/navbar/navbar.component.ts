import { Component, Input, OnDestroy } from '@angular/core';
import { GraphRequest } from '../../models/graph-request.model';
import { GraphService } from '../../services/graph.service';
import { Subscription } from 'rxjs';
import { GraphResponseDataService } from '../../services/graph.response.data.service';
import { v4 as uuidv4 } from 'uuid';
import { GraphNode } from '../../models/graph-node.model';
import { GraphEdge } from '../../models/graph-edge.model';

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
    id: uuidv4(),
    name: "",
    nodes: [],
    edges: [],
    src: null,
    target: null,
    isDirected: false
  }
  
  constructor(private graphService: GraphService, private graphResponseDataSerivce: GraphResponseDataService) { }

  private extractGraphData(graphModel: GraphRequest): GraphRequest {
    const elements = this.graph.getElements();
    const nodes: GraphNode[] = elements.map((node: joint.dia.Element) => ({
      id: node.id.toString(),
      label: node.attr('label/text') || `Node-${node.id}`,
      x: node.position().x,
      y: node.position().y,
      isStart: node.attr('label/text')?.startsWith('(S) '),
      isEnd: node.attr('label/text')?.startsWith('(X) ')
    }));

    const links = this.graph.getLinks();
    const edges: GraphEdge[] = links.map((link: joint.dia.Link) => {
      const sourceElement = link.getSourceElement();
      const targetElement = link.getTargetElement();
      return {
        id: link.id.toString(),
        source: sourceElement ? sourceElement.id.toString() : '',
        target: targetElement ? targetElement.id.toString() : '',
        weight: link.attr('weight') || 1
      };
    });

    graphModel.name = "";
    graphModel.src = this.graphSrc ? this.graphSrc.id.toString() : null;
    graphModel.target = this.graphTarget ? this.graphTarget.id.toString() : null;
    graphModel.nodes = nodes;
    graphModel.edges = edges;
    graphModel.isDirected = this.directed;

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
