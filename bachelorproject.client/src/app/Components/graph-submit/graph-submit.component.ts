import { Component, ElementRef, Input, OnDestroy, ViewChild } from '@angular/core';
import { Graph } from '../../models/graph.model';
import { GraphService } from '../../services/graph.service';
import { Subscription } from 'rxjs';
import { GraphResponseDataService } from '../../services/graph.response.data.service';
import { v4 as uuidv4 } from 'uuid';
import { Node } from '../../models/node.model';
import { Edge } from '../../models/edge.model';
import { GraphStepsResult } from '../../models/graph-steps-result.model';
import { GraphErrorsService } from '../../services/graph-errors.service';

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
  
  errors: string[] = [];

  private addGraphSubscription?: Subscription

  constructor(private graphService: GraphService, private graphResponseDataSerivce: GraphResponseDataService, private errorsService: GraphErrorsService) { }

  private extractGraphData(): Graph {
    const elements = this.graph.getElements();
    const nodes: Node[] = elements.map((node: joint.dia.Element) => ({
      id: node.id.toString(),
      label: node.attr('label/text') || `Node-${node.id}`,
      x: node.position().x,
      y: node.position().y,
      isStart: node.attr('label/text')?.startsWith('(S) '),
      isEnd: node.attr('label/text')?.startsWith('(X) ')
    }));

    const links = this.graph.getLinks();
    const edges: Edge[] = links.map((link: joint.dia.Link) => {
      const sourceElement = link.getSourceElement();
      const targetElement = link.getTargetElement();
      return {
        id: link.id.toString(),
        sourceNodeId: sourceElement ? sourceElement.id.toString() : '',
        targetNodeId: targetElement ? targetElement.id.toString() : '',
        weight: link.attr('weight') || 1
      };
    });
    const graphModel: Graph = {
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
      this.errorsService.updateErrors(this.errors, algo);
      return;
    }

    this.addGraphSubscription = this.graphService.runAlgo(algo, graphData).subscribe({
      next: (response: GraphStepsResult) => {
        console.log('Received Processed Graph:', response);
        this.graphResponseDataSerivce.updateGraphData(response, graphData);
      },
      error: (error) => {
        console.error('Error sending graph data:', error);
        this.errors.push(error.error);
        this.errorsService.updateErrors(this.errors, algo)
      }
    });
  }

  ngOnDestroy(): void {
    this.addGraphSubscription?.unsubscribe();
  }
}
