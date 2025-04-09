import { Component, ElementRef, ViewChild } from '@angular/core';
import { GraphResponseDataService } from '../../services/graph.response.data.service';
import * as joint from 'jointjs';
import { GraphRequest } from '../../models/graph-request.model';
import { StepState } from '../../models/graph-steps-result.model';
import { GraphResult } from '../../models/graph-result.model';

@Component({
  selector: 'app-graph-construct-from-backend',
  templateUrl: './graph-construct-from-backend.component.html',
  styleUrl: './graph-construct-from-backend.component.css'
})
export class GraphConstructFromBackendComponent {
  @ViewChild('resultContainer', { static: false }) resultContainer!: ElementRef;
  @ViewChild('resultTextDiv', { static: false }) resultTextDiv!: ElementRef;
  @ViewChild('stepsContainer', { static: false }) stepsContainer!: ElementRef;
  @ViewChild('resultText', { static: false }) resultText!: ElementRef;

  constructor(private graphResponseDataService: GraphResponseDataService) { }

  ngOnInit() {
    this.graphResponseDataService.graphData$.subscribe(data => {
      if (data) {
        const { response, graphData: graphRequest } = data;
        console.log('Graph data arrived in Reconstructor:', data);

        this.buildStepPapers(response.steps, graphRequest, response.resultGraph.graphType);
        this.reconstructGraph(response.resultGraph, graphRequest, response.steps[response.steps.length - 1]);
      }
    });
  }

  private buildStepPapers(steps: StepState[], graphRequest: GraphRequest, graphType: string) {
    this.stepsContainer.nativeElement.innerHTML = '';

    steps.forEach((step, index) => {
      const stepWrapper = document.createElement('div');
      stepWrapper.classList.add('step-wrapper');
      stepWrapper.innerHTML = `<h3>${graphType} Step ${index + 1}</h3>`;
      this.stepsContainer.nativeElement.appendChild(stepWrapper);
      
      if (step.currentTotalWeight != null) {
        let innerText = `Current Total Weight: ${step.currentTotalWeight}`;
        if (graphType == "Edmonds-Karp") {
          innerText = `Current Total Flow: ${step.currentTotalWeight}`;
        }

        const totalWeightDiv = document.createElement('div');
        totalWeightDiv.innerText = innerText;
        totalWeightDiv.style.fontWeight = 'bold';
        totalWeightDiv.style.marginBottom = '5px';
        stepWrapper.appendChild(totalWeightDiv);
      }
      
      const paperDiv = document.createElement('div');
      paperDiv.style.width = '100%';
      paperDiv.style.height = '650px';
      paperDiv.style.marginBottom = '20px';
      paperDiv.style.margin = '0 auto';
      paperDiv.style.border = '1px solid #ccc';
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
      
      const nodeMap = new Map<string, joint.dia.Element>();
      graphRequest.nodes.forEach(nodeObj => {
        const circle = new joint.shapes.standard.Circle();
        circle.position(nodeObj.x, nodeObj.y);
        circle.resize(80, 80);
        const color = step.nodeColors[nodeObj.id] || '#3498db';
        circle.attr({
          body: { fill: color, stroke: '#2980b9', strokeWidth: 2 },
          label: { text: nodeObj.label, fill: '#ffffff', fontSize: 14 }
        });
        circle.addTo(stepGraph);
        nodeMap.set(nodeObj.id, circle);
      });
      
      graphRequest.edges.forEach(edgeObj => {
        const color = step.edgeColors[edgeObj.id] || 'black';
        const sourceEl = nodeMap.get(edgeObj.sourceNodeId);
        const targetEl = nodeMap.get(edgeObj.targetNodeId);
        if (!sourceEl || !targetEl) return;

        const originalWeight = edgeObj.weight;
        let labelText = originalWeight.toString();
        if (step.edgeCurrentWeights && step.edgeCurrentWeights[edgeObj.id] != null) {
          labelText = `${step.edgeCurrentWeights[edgeObj.id]}/${originalWeight}`;
        }

        const link = new joint.shapes.standard.Link();
        link.source(sourceEl);
        link.target(targetEl);
        if (graphRequest.isWeighted) {
          link.labels([{
            position: 0.5,
            attrs: {
              text: { text: labelText, fill: 'black' }
            }
          }]);
        }
        link.attr({
          line: {
            stroke: color,
            strokeWidth: 3,
            targetMarker: graphRequest.isDirected ? { type: 'path' } : { type: 'none' }
          }
        });
        link.addTo(stepGraph);
      });
    });
  }

  private reconstructGraph(resultGraph: GraphResult, graphRequest: GraphRequest, lastStep: StepState) {
    this.resultTextDiv.nativeElement.innerHTML = '';
    this.resultContainer.nativeElement.innerHTML = '';

    this.resultText.nativeElement.innerText = `${resultGraph.graphType} Result`;

    if (resultGraph.totalWeight != null) {
      let innerText = `Total Weight: ${resultGraph.totalWeight}`;
      if (resultGraph.graphType == "Edmonds-Karp") {
        innerText = `Total Flow: ${resultGraph.totalWeight}`;
      }

      const totalWeightHeader = document.createElement('div');
      totalWeightHeader.innerText = innerText;
      totalWeightHeader.style.fontWeight = 'bold';
      totalWeightHeader.style.marginBottom = '10px';
      this.resultTextDiv.nativeElement.appendChild(totalWeightHeader);
    }
    
    const paperDiv = document.createElement('div');
    paperDiv.style.width = '100%';
    paperDiv.style.height = '650px';
    this.resultContainer.nativeElement.appendChild(paperDiv);

    const graph = new joint.dia.Graph();
    const paper = new joint.dia.Paper({
      el: paperDiv,
      model: graph,
      width: '100%',
      height: 650,
      gridSize: 10,
      drawGrid: true,
      background: { color: '#f8f9fa' }
    });

    const nodeMap = new Map<string, joint.dia.Element>();

    resultGraph.nodeIds.forEach(nodeId => {
      const nodeObj = graphRequest.nodes.find(n => n.id === nodeId);
      if (!nodeObj) return;
      const circle = new joint.shapes.standard.Circle();
      circle.position(nodeObj.x, nodeObj.y);
      circle.resize(80, 80);
      circle.attr({
        body: { fill: resultGraph.graphType == "Welsh-Powell" ? lastStep.edgeColors[nodeId] : '#3498db', stroke: '#2980b9', strokeWidth: 2 },
        label: { text: nodeObj.label, fill: '#ffffff', fontSize: 14 }
      });
      circle.addTo(graph);
      nodeMap.set(nodeObj.id, circle);
    });

    resultGraph.edgeIds.forEach(edgeId => {
      const edgeObj = graphRequest.edges.find(e => e.id === edgeId);
      if (!edgeObj) return;
      const sourceEl = nodeMap.get(edgeObj.sourceNodeId);
      const targetEl = nodeMap.get(edgeObj.targetNodeId);
      if (!sourceEl || !targetEl) return;

      const originalWeight = edgeObj.weight;
      let labelText = originalWeight.toString();
      if (resultGraph.edgeResultWeights && resultGraph.edgeResultWeights[edgeObj.id] != null) {
        labelText = `${resultGraph.edgeResultWeights[edgeObj.id]}/${originalWeight}`;
      }

      const link = new joint.shapes.standard.Link();
      link.source(sourceEl);
      link.target(targetEl);
      if (graphRequest.isWeighted) {
        link.labels([{
          position: 0.5,
          attrs: {
            text: { text: labelText, fill: 'black' }
          }
        }]);
      }
      link.attr({
        line: {
          stroke: resultGraph.graphType == "Greedy Coloring" ? lastStep.edgeColors[edgeId] : 'black',
          strokeWidth: 3,
          targetMarker: graphRequest.isDirected ? { type: 'path' } : { type: 'none' }
        }
      });
      link.addTo(graph);
    });
  }
}
