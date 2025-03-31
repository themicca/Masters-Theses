import { Component, ElementRef, ViewChild } from '@angular/core';
import { GraphResponseDataService } from '../../services/graph.response.data.service';
import * as joint from 'jointjs';
import { GraphRequest } from '../../models/graph-request.model';
import { StepState } from '../../models/graph-steps-result.model';
import { GraphNode } from '../../models/graph-node.model';
import { GraphEdge } from '../../models/graph-edge.model';

@Component({
  selector: 'app-graph-construct-from-backend',
  templateUrl: './graph-construct-from-backend.component.html',
  styleUrl: './graph-construct-from-backend.component.css'
})
export class GraphConstructFromBackendComponent {
  @ViewChild('resultContainer', { static: false }) resultContainer!: ElementRef;
  @ViewChild('stepsContainer', { static: false }) stepsContainer!: ElementRef;

  constructor(private graphResponseDataService: GraphResponseDataService) { }

  ngOnInit() {
    this.graphResponseDataService.graphData$.subscribe(graphData => {
      if (graphData) {
        console.log('Graph data arrived in Reconstructor:', graphData);
        this.buildStepPapers(graphData.steps, graphData.finalGraph.nodes, graphData.finalGraph.edges);
        this.reconstructGraph(graphData.finalGraph);
      }
    });
  }

  private buildStepPapers(steps: StepState[], nodes: GraphNode[], edges: GraphEdge[]) {
    this.stepsContainer.nativeElement.innerHTML = '';

    steps.forEach((step, index) => {
      const stepWrapper = document.createElement('div');
      stepWrapper.classList.add('step-wrapper');
      stepWrapper.innerHTML = `<h3>Step ${index + 1}</h3>`;
      this.stepsContainer.nativeElement.appendChild(stepWrapper);

      const paperDiv = document.createElement('div');
      paperDiv.style.width = '100%';
      paperDiv.style.height = '650px';
      paperDiv.style.marginBottom = '20px';
      stepWrapper.appendChild(paperDiv);

      const stepGraph = new joint.dia.Graph();
      const stepPaper = new joint.dia.Paper({
        el: paperDiv,
        model: stepGraph,
        width: '95%',
        height: 650,
        gridSize: 10,
        drawGrid: true,
        background: { color: '#f8f9fa' }
      });

      const nodeIds = Object.keys(step.nodeColors);
      const nodeMap = new Map<string, joint.dia.Element>();

      nodeIds.forEach(nodeId => {
        const nodeObj = nodes.find(n => n.id === nodeId);
        if (!nodeObj) return;
        const circle = new joint.shapes.standard.Circle();
        circle.position(nodeObj.x, nodeObj.y);
        circle.resize(80, 80);

        const color = step.nodeColors[nodeId] || '#3498db';
        circle.attr({
          body: { fill: color, stroke: '#2980b9', strokeWidth: 2 },
          label: { text: nodeObj.label, fill: '#ffffff', fontSize: 14 }
        });
        circle.addTo(stepGraph);
        nodeMap.set(nodeId, circle);
      });

      step.edges.forEach(edgeState => {
        const edgeObj: GraphEdge | undefined = edges.find((e: GraphEdge) => e.id === edgeState.edge);
        if (!edgeObj) return;
        const sourceEl = nodeMap.get(edgeObj.source);
        const targetEl = nodeMap.get(edgeObj.target);
        if (!sourceEl || !targetEl) return;

        const link = new joint.shapes.standard.Link();
        link.source(sourceEl);
        link.target(targetEl);
        link.attr('weight', edgeObj.weight);
        link.attr({
          line: {
            stroke: edgeState.color || 'blue',
            strokeWidth: 3
          }
        });
        link.addTo(stepGraph);
      });
    });
  }


  private reconstructGraph(graphData: GraphRequest) {
    const graph = new joint.dia.Graph();

    const paper = new joint.dia.Paper({
      el: this.resultContainer.nativeElement,
      model: graph,
      width: '95%',
      height: 500,
      gridSize: 10,
      drawGrid: true,
      background: { color: '#f8f9fa' }
    });

    const nodeMap = new Map<string, joint.dia.Element>();

    graphData.nodes.forEach((nodeObj) => {
      const circle = new joint.shapes.standard.Circle();
      circle.position(nodeObj.x, nodeObj.y);
      circle.resize(80, 80);
      circle.attr({
        body: { fill: '#3498db', stroke: '#2980b9', strokeWidth: 2 },
        label: { text: nodeObj.label, fill: '#ffffff', fontSize: 14 }
      });
      circle.addTo(graph);
      nodeMap.set(nodeObj.id, circle);
    });

    graphData.edges.forEach((edgeObj) => {
      const sourceEl = nodeMap.get(edgeObj.source);
      const targetEl = nodeMap.get(edgeObj.target);
      if (!sourceEl || !targetEl) return;
      const link = new joint.shapes.standard.Link();
      link.source(sourceEl);
      link.target(targetEl);
      link.attr('weight', edgeObj.weight);
      link.labels([{
        position: 0.5,
        attrs: {
          text: { text: edgeObj.weight.toString(), fill: 'black' }
        }
      }]);
      link.addTo(graph);
    });
  }
}
