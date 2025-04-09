import { Component, ElementRef, Input, OnDestroy, ViewChild } from '@angular/core';
import { GraphRequest } from '../../models/graph-request.model';
import { GraphService } from '../../services/graph.service';
import { Subscription } from 'rxjs';
import { GraphResponseDataService } from '../../services/graph.response.data.service';
import { v4 as uuidv4 } from 'uuid';
import { GraphNode } from '../../models/graph-node.model';
import { GraphEdge } from '../../models/graph-edge.model';
import { GraghStepsResult } from '../../models/graph-steps-result.model';

@Component({
  selector: 'app-graph-submit',
  templateUrl: './graph-submit.component.html',
  styleUrl: './graph-submit.component.css'
})
export class GraphSubmitComponent implements OnDestroy {
  @ViewChild('buttonDijkstra', { static: false }) buttonDijkstra!: ElementRef;
  @ViewChild('buttonKruskal', { static: false }) buttonKruskal!: ElementRef;
  @ViewChild('buttonEdmondsKarp', { static: false }) buttonEdmondsKarp!: ElementRef;
  @ViewChild('buttonFleury', { static: false }) buttonFleury!: ElementRef;
  @ViewChild('buttonHeldKarp', { static: false }) buttonHeldKarp!: ElementRef;
  @ViewChild('buttonGreedyMatching', { static: false }) buttonGreedyMatching!: ElementRef;
  @ViewChild('buttonGreedyColoring', { static: false }) buttonGreedyColoring!: ElementRef;
  @ViewChild('buttonWelshPowell', { static: false }) buttonWelshPowell!: ElementRef;

  @Input() graph!: joint.dia.Graph;
  @Input() graphSrc: joint.dia.Element | null = null;
  @Input() graphTarget: joint.dia.Element | null = null;
  @Input() directed!: boolean;
  @Input() weighted!: boolean;

  showModal = false;
  algo: string = "Algo";
  errors: string[] = [];

  private addGraphSubscription?: Subscription
  
  constructor(private graphService: GraphService, private graphResponseDataSerivce: GraphResponseDataService) { }

  private extractGraphData(): GraphRequest {
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
        sourceNodeId: sourceElement ? sourceElement.id.toString() : '',
        targetNodeId: targetElement ? targetElement.id.toString() : '',
        weight: link.attr('weight') || 1
      };
    });
    const graphModel: GraphRequest = {
      id: uuidv4(),
      name: "",
      nodes: nodes,
      edges: edges,
      src: this.graphSrc ? this.graphSrc.id.toString() : null,
      target: this.graphTarget ? this.graphTarget.id.toString() : null,
      isDirected: this.directed,
      isWeighted: this.weighted
    };
    return graphModel;
  }

  onSubmit(algo: string = 'Algo') {
    const graphData = this.extractGraphData();
    this.errors = this.graphService.validate(algo, this.graph, graphData);
    
    if (this.errors.length > 0) {
      this.algo = algo;
      this.showModal = true;
      return;
    }

    this.addGraphSubscription = this.graphService.runAlgo(algo, graphData).subscribe({
      next: (response: GraghStepsResult) => {
        console.log('Received Processed Graph:', response);
        this.graphResponseDataSerivce.updateGraphData(response, graphData);
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
