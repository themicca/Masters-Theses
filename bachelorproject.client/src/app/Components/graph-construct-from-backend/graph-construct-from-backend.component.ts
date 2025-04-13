import { Component, ElementRef, ViewChild } from '@angular/core';
import { GraphResponseDataService } from '../../services/graph.response.data.service';
import * as joint from 'jointjs';
import { Graph } from '../../models/graph.model';
import { Step } from '../../models/graph-steps-result.model';
import { GraphResult } from '../../models/graph-result.model';
import { EDGE_COLOR_STROKE, LABEL_COLOR_BLACK, LABEL_COLOR_WHITE, NODE_COLOR_FILL, NODE_COLOR_STROKE, PAPER_BACKGROUND_COLOR, PAPER_HEIGHT } from '../../utils/constants';
import { Utils } from '../../utils/utils';

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
        const { response, graphData: graph } = data;
        console.log('Graph data arrived in Reconstructor:', data);

        this.buildStepPapers(response.steps, graph, response.graphResult.algoType);
        this.reconstructGraph(response.graphResult, graph, response.steps[response.steps.length - 1]);
      }
    });
  }

  private buildStepPapers(steps: Step[], graph: Graph, algoType: string) {
    this.stepsContainer.nativeElement.innerHTML = '';

    steps.forEach((step, index) => {
      const stepWrapper = document.createElement('div');
      stepWrapper.classList.add('step-wrapper');
      stepWrapper.innerHTML = `<h3>${algoType}${graph.eulerType ? " " + graph.eulerType : ""} Step ${index + 1}</h3>`;
      this.stepsContainer.nativeElement.appendChild(stepWrapper);
      
      if (step.currentTotalWeight != null) {
        let innerText = `Current Total Weight: ${step.currentTotalWeight}`;
        if (algoType == "Edmonds-Karp") {
          innerText = `Current Total Flow: ${step.currentTotalWeight}`;
        }
        else if (algoType == "Welsh-Powell" || algoType == "Greedy Coloring") {
          innerText = `Current Used Colors: ${step.currentTotalWeight}`;
        }

        const totalWeightDiv = document.createElement('div');
        totalWeightDiv.innerText = innerText;
        totalWeightDiv.style.fontWeight = 'bold';
        totalWeightDiv.style.marginBottom = '5px';
        stepWrapper.appendChild(totalWeightDiv);
      }
      
      const paperDiv = document.createElement('div');
      paperDiv.style.width = '100%';
      paperDiv.style.height = `${PAPER_HEIGHT}px`;
      paperDiv.style.marginBottom = '20px';
      paperDiv.style.margin = '0 auto';
      paperDiv.style.border = '1px solid #ccc';
      stepWrapper.appendChild(paperDiv);
      
      const stepGraph = new joint.dia.Graph();
      const stepPaper = new joint.dia.Paper({
        el: paperDiv,
        model: stepGraph,
        width: '95%',
        height: PAPER_HEIGHT,
        gridSize: 10,
        drawGrid: true,
        background: { color: PAPER_BACKGROUND_COLOR }
      });
      
      const nodeMap = new Map<string, joint.dia.Element>();
      graph.nodes.forEach(nodeObj => {
        const circle = new joint.shapes.standard.Circle();
        circle.position(nodeObj.x, nodeObj.y);
        circle.resize(80, 80);
        const color = step.nodeColors[nodeObj.id] || NODE_COLOR_FILL;
        circle.attr({
          body: { fill: color, stroke: NODE_COLOR_STROKE, strokeWidth: 2 },
          label: { text: nodeObj.label, fill: LABEL_COLOR_WHITE, fontSize: 14 }
        });
        circle.addTo(stepGraph);
        nodeMap.set(nodeObj.id, circle);
      });
      
      graph.edges.forEach(edgeObj => {
        const color = step.edgeColors[edgeObj.id] || EDGE_COLOR_STROKE;
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
        if (graph.isWeighted) {
          link.labels([{
            position: 0.5,
            attrs: {
              text: { text: labelText, fill: LABEL_COLOR_BLACK }
            }
          }]);
        }
        link.attr({
          line: {
            stroke: color,
            strokeWidth: 3,
            targetMarker: graph.isDirected ? { type: 'path' } : { type: 'none' }
          }
        });
        link.addTo(stepGraph);
      });

      Utils.offSetOppositeLinks(stepGraph);
    });
  }

  private reconstructGraph(graphResult: GraphResult, graph: Graph, lastStep: Step) {
    this.resultTextDiv.nativeElement.innerHTML = '';
    this.resultContainer.nativeElement.innerHTML = '';

    this.resultText.nativeElement.innerText = `${graphResult.algoType}${graph.eulerType ? " " + graph.eulerType : ""} Result`;

    if (graphResult.totalWeight != null) {
      let innerText = `Total Weight: ${graphResult.totalWeight}`;
      if (graphResult.algoType == "Edmonds-Karp") {
        innerText = `Total Flow: ${graphResult.totalWeight}`;
      }
      else if (graphResult.algoType == "Welsh-Powell" || graphResult.algoType == "Greedy Coloring") {
        innerText = `Used Colors: ${graphResult.totalWeight}`;
      }

      const totalWeightHeader = document.createElement('div');
      totalWeightHeader.innerText = innerText;
      totalWeightHeader.style.fontWeight = 'bold';
      totalWeightHeader.style.marginBottom = '10px';
      this.resultTextDiv.nativeElement.appendChild(totalWeightHeader);
    }
    
    const paperDiv = document.createElement('div');
    paperDiv.style.width = '100%';
    paperDiv.style.height = `${PAPER_HEIGHT}px`;
    this.resultContainer.nativeElement.appendChild(paperDiv);

    const resultGraph = new joint.dia.Graph();
    const paper = new joint.dia.Paper({
      el: paperDiv,
      model: resultGraph,
      width: '100%',
      height: PAPER_HEIGHT,
      gridSize: 10,
      drawGrid: true,
      background: { color: PAPER_BACKGROUND_COLOR }
    });
    
    const nodeMap = new Map<string, joint.dia.Element>();

    graphResult.nodeIds.forEach(nodeId => {
      const nodeObj = graph.nodes.find(n => n.id === nodeId);
      if (!nodeObj) return;
      const circle = new joint.shapes.standard.Circle();
      circle.position(nodeObj.x, nodeObj.y);
      circle.resize(80, 80);
      circle.attr({
        body: { fill: graphResult.algoType == "Welsh-Powell" ? lastStep.nodeColors[nodeId] : NODE_COLOR_FILL, stroke: NODE_COLOR_STROKE, strokeWidth: 2 },
        label: { text: nodeObj.label, fill: LABEL_COLOR_WHITE, fontSize: 14 }
      });
      circle.addTo(resultGraph);
      nodeMap.set(nodeObj.id, circle);

      if (graphResult.algoType === "Held-Karp") {
        const traversalIndex = graphResult.nodeIds.indexOf(nodeId);
        let orderNumber = null;

        if (traversalIndex > 0) {
          orderNumber = traversalIndex;
        } else if (traversalIndex === 0) {
          orderNumber = graphResult.nodeIds.length - 1;
        }

        if (orderNumber !== null) {
          const text = new joint.shapes.standard.TextBlock();
          text.position(nodeObj.x + 25, nodeObj.y - 30);
          text.resize(40, 20);
          text.attr({
            label: {
              text: orderNumber.toString(),
              fill: LABEL_COLOR_BLACK,
              fontSize: 14
            }
          });
          text.addTo(resultGraph);
        }
      }
    });

    graphResult.edgeIds.forEach(edgeId => {
      const edgeObj = graph.edges.find(e => e.id === edgeId);
      if (!edgeObj) return;
      const sourceEl = nodeMap.get(edgeObj.sourceNodeId);
      const targetEl = nodeMap.get(edgeObj.targetNodeId);
      if (!sourceEl || !targetEl) return;

      const originalWeight = edgeObj.weight;
      let labelText = originalWeight.toString();
      if (graphResult.edgeResultWeights && graphResult.edgeResultWeights[edgeObj.id] != null) {
        labelText = `${graphResult.edgeResultWeights[edgeObj.id]}/${originalWeight}`;
      }

      const link = new joint.shapes.standard.Link();
      link.source(sourceEl);
      link.target(targetEl);
      if (graph.isWeighted) {
        link.labels([{
          position: 0.5,
          attrs: {
            text: { text: labelText, fill: LABEL_COLOR_BLACK }
          }
        }]);
      }
      link.attr({
        line: {
          stroke: graphResult.algoType == "Greedy Coloring" ? lastStep.edgeColors[edgeId] : EDGE_COLOR_STROKE,
          strokeWidth: 3,
          targetMarker: graph.isDirected ? { type: 'path' } : { type: 'none' }
        }
      });
      link.addTo(resultGraph);
    });

    Utils.offSetOppositeLinks(resultGraph);
  }
}
