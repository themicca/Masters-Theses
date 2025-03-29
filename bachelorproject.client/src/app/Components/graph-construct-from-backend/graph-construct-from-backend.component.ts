import { Component, ElementRef, ViewChild } from '@angular/core';
import { GraphResponseDataService } from '../../services/graph.response.data.service';
import * as joint from 'jointjs';
import { GraphRequest } from '../../models/graph-request.model';
import { StepState } from '../../models/graph-steps-result.model';

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
        this.buildStepPapers(graphData.steps, graphData.finalGraph.graphNodePositions);
        
        const originalNodeList = Object.keys(graphData.steps[0].nodeColors)
        this.reconstructGraph(graphData.finalGraph, originalNodeList);
      }
    });
  }

  private buildStepPapers(steps: StepState[], graphNodePositions: { x: number, y: number }[]) {
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
      
      const nodeNames = Object.keys(step.nodeColors);
      const nodeMap = new Map<string, joint.dia.Element>();
      console.log(index);
      nodeNames.forEach((nodeName, i) => {
        const circle = new joint.shapes.standard.Circle();
        let x = 100 + i * 150;
        let y = 50;
        
        if (graphNodePositions && graphNodePositions[i]) {
          x = graphNodePositions[i].x;
          y = graphNodePositions[i].y;
        }

        circle.position(x, y);
        circle.resize(80, 80);
        
        const color = step.nodeColors[nodeName] || '#3498db';
        circle.attr({
          body: { fill: color, stroke: '#2980b9', strokeWidth: 2 },
          label: { text: nodeName, fill: '#ffffff', fontSize: 14 }
        });
        circle.addTo(stepGraph);

        nodeMap.set(nodeName, circle);
      });
      step.edges.forEach(edgeState => {
        const sourceNode = nodeMap.get(edgeState.source);
        const targetNode = nodeMap.get(edgeState.target);
        if (!sourceNode || !targetNode) return;

        const link = new joint.shapes.standard.Link();
        link.source(sourceNode);
        link.target(targetNode);
        
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

  private reconstructGraph(graphData: GraphRequest, originalNodeList: string[]) {
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

    graphData.graphNodes.forEach((name, index) => {
      const circle = new joint.shapes.standard.Circle();

      const originalIndex = originalNodeList.indexOf(name);
      let x = 100 + index * 150;
      let y = 50;

      if (originalIndex !== -1 &&
        graphData.graphNodePositions &&
        graphData.graphNodePositions[originalIndex]) {
        x = graphData.graphNodePositions[originalIndex].x;
        y = graphData.graphNodePositions[originalIndex].y;
      }

      circle.position(x, y);
      circle.resize(80, 80);
      circle.attr({
        body: { fill: '#3498db', stroke: '#2980b9', strokeWidth: 2 },
        label: { text: name, fill: '#ffffff', fontSize: 14 }
      });
      circle.addTo(graph);

      nodeMap.set(name, circle);
    });
    
    const size = graphData.graphNodes.length;
    for (let i = 0; i < size; i++) {
      for (let j = 0; j < size; j++) {
        const weight = graphData.graphEdges[i][j];
        if (weight === 0 || weight >= 9999 || i === j) continue;

        const sourceName = graphData.graphNodes[i];
        const targetName = graphData.graphNodes[j];

        const sourceNode = nodeMap.get(sourceName);
        const targetNode = nodeMap.get(targetName);
        if (!sourceNode || !targetNode) continue;
        
        const link = new joint.shapes.standard.Link();
        link.source(sourceNode);
        link.target(targetNode);
        
        link.attr('weight', weight);
        
        link.labels([{
          position: 0.5,
          attrs: {
            text: { text: weight.toString(), fill: 'black' }
          }
        }]);

        link.addTo(graph);
      }
    }
  }
}
