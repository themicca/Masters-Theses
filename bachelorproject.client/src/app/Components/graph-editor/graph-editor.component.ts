import { Component, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import * as joint from 'jointjs';
import { GraphSelectionComponent } from '../graph-selection/graph-selection.component';
import { EdgeWeightComponent } from '../edge-weight/edge-weight.component';
import { EdgesConnectionComponent } from '../edges-connection/edges-connection.component';
import { v4 as uuidv4 } from 'uuid';
import { RenameNodesComponent } from '../rename-nodes/rename-nodes.component';
import { GraphSubmitComponent } from '../graph-submit/graph-submit.component';
import { TooltipComponent } from '../tooltip/tooltip.component';
import { LABEL_COLOR_WHITE, MAX_NODES, NODE_COLOR_FILL, NODE_COLOR_STROKE, PAPER_BACKGROUND_COLOR, PAPER_HEIGHT } from '../../utils/constants';
import { GraphErrorsService } from '../../services/graph-errors.service';

@Component({
  selector: 'app-graph-editor',
  templateUrl: './graph-editor.component.html',
  styleUrl: './graph-editor.component.css'
})
export class GraphEditorComponent implements AfterViewInit {
  @ViewChild('graphContainer', { static: false }) graphContainer!: ElementRef;
  @ViewChild('sampleNode', { static: false }) sampleNode!: ElementRef;
  @ViewChild('sampleEdge', { static: false }) sampleEdge!: ElementRef;
  @ViewChild('directionalCheckbox', { static: false }) directionalCheckbox!: ElementRef;
  @ViewChild('weightedCheckbox', { static: false }) weightedCheckbox!: ElementRef;
  @ViewChild('tooltipCheckbox', { static: false }) tooltipCheckbox!: ElementRef;
  @ViewChild('nodeContextMenu', { static: false }) nodeContextMenu!: ElementRef;
  @ViewChild('edgeContextMenu', { static: false }) edgeContextMenu!: ElementRef;

  @ViewChild(GraphSelectionComponent) selectionComponent!: GraphSelectionComponent;
  @ViewChild(EdgeWeightComponent) edgeWeightComponent!: EdgeWeightComponent;
  @ViewChild(EdgesConnectionComponent) edgesConnectionComponent!: EdgesConnectionComponent;
  @ViewChild(RenameNodesComponent) renameNodesComponent!: RenameNodesComponent;
  @ViewChild(GraphSubmitComponent) graphSubmitComponent!: GraphSubmitComponent
  @ViewChild(TooltipComponent) tooltipComponent!: TooltipComponent;

  protected graph!: joint.dia.Graph;
  protected paper!: joint.dia.Paper;
  private nodeRadius: number = 80;
  public lastClickedElement: joint.dia.Element | joint.dia.Link | null = null;
  protected directed: boolean = true;
  protected weighted: boolean = true;
  protected showTooltip: boolean = true;
  protected startNode: joint.dia.Element | null = null;
  protected endNode: joint.dia.Element | null = null;
  private readonly defaultWeight = 1;

  constructor(private errorsService: GraphErrorsService) { }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.initGraph();
      this.addListeners();
    });
    this.enableDragging();
    this.initEdgeCreation();
    this.setTooltip();
  }

  private initGraph(): void {
    this.graph = new joint.dia.Graph();

    this.paper = new joint.dia.Paper({
      el: this.graphContainer.nativeElement,
      model: this.graph,
      width: '100%',
      height: PAPER_HEIGHT,
      gridSize: 10,
      drawGrid: true,
      background: { color: PAPER_BACKGROUND_COLOR },
      interactive: true
    });
  }

  private addListeners() {
    [this.nodeContextMenu, this.edgeContextMenu].forEach(menuEl => {
      menuEl.nativeElement.addEventListener('contextmenu', (event: MouseEvent) => {
        event.preventDefault();
      });
    });
    
    this.graphContainer.nativeElement.addEventListener('contextmenu', (event: MouseEvent) => {
      event.preventDefault();
    });

    this.paper.on('element:contextmenu', (_, evt: MouseEvent) => {
      evt.preventDefault();
      this.showContextMenu(evt.clientX, evt.clientY, 'node');
    });
    
    this.paper.on('link:contextmenu', (_, evt: MouseEvent) => {
      evt.preventDefault();
      this.showContextMenu(evt.clientX, evt.clientY, 'edge');
    });

    this.paper.on('blank:contextmenu', () => {
      this.hideAllContextMenus();
    });
    
    this.paper.on('blank:pointerdown', () => {
      this.hideAllContextMenus();
      const active = document.activeElement as HTMLElement;
      if (active && active.tagName.toLowerCase() === 'input') {
        (active as HTMLInputElement).blur();
      }
    });

    document.addEventListener('click', () => {
      this.hideAllContextMenus();
    });

    document.addEventListener('keydown', (event: KeyboardEvent) => {
      if (event.key === 'Delete') {
        this.deleteSelected();
      }
      if (event.key === 'Escape') {
        this.hideAllContextMenus();
      }
    });

    this.paper.on('element:mouseenter', (elementView: any) => {
      this.selectionComponent.highlightNode(elementView.model);
    });

    this.paper.on('element:mouseleave', (elementView: any) => {
      this.selectionComponent.unhighlightNode(elementView.model);
    });

    this.paper.on('link:mouseenter', (linkView: any) => {
      this.selectionComponent.highlightLink(linkView.model);
    });

    this.paper.on('link:mouseleave', (linkView: any) => {
      this.selectionComponent.unhighlightLink(linkView.model);
    });

    this.paper.on('element:pointerdown', (_, evt, x, y) => {
      const selectedElements = this.selectionComponent.selectedElements;
      if (selectedElements.length <= 1) {
        return;
      }
      
      evt.stopPropagation();

      const initialPositions = selectedElements.map(elem => {
        const pos = elem.position();
        return { elem, x: pos.x, y: pos.y };
      });
      
      const connectedLinks = this.graph.getLinks().filter(link => {
        const srcElem = link.getSourceElement();
        const tgtElem = link.getTargetElement();
        return (
          (srcElem && selectedElements.includes(srcElem)) ||
          (tgtElem && selectedElements.includes(tgtElem))
        );
      });
      
      const initialLinkVertices = connectedLinks.map(link => {
        const verts = link.vertices() || [];
        const vertsCopy = verts.map(v => ({ x: v.x, y: v.y }));
        return { link, vertices: vertsCopy };
      });

      evt.data = {
        startX: x,
        startY: y,
        initialPositions,
        initialLinkVertices
      };
    });

    this.paper.on('element:pointermove', (_, evt, x, y) => {
      if (!evt.data || !evt.data.initialPositions) return;

      const dx = x - evt.data.startX;
      const dy = y - evt.data.startY;
      
      evt.data.initialPositions.forEach((item: any) => {
        const newX = item.x + dx;
        const newY = item.y + dy;
        item.elem.position(newX, newY);
      });
      
      evt.data.initialLinkVertices.forEach((linkData: any) => {
        const link = linkData.link;
        const oldVerts = linkData.vertices;
        if (!oldVerts || !oldVerts.length) return;

        const newVerts = oldVerts.map((v: any) => ({
          x: v.x + dx,
          y: v.y + dy
        }));
        link.vertices(newVerts);
      });
    });
  }

  private setTooltip() {
    this.tooltipComponent.buttonDijkstra = this.graphSubmitComponent.buttonDijkstra.nativeElement;
    this.tooltipComponent.buttonKruskal = this.graphSubmitComponent.buttonKruskal.nativeElement;
    this.tooltipComponent.buttonEdmondsKarp = this.graphSubmitComponent.buttonEdmondsKarp.nativeElement;
    this.tooltipComponent.buttonFleury = this.graphSubmitComponent.buttonFleury.nativeElement;
    this.tooltipComponent.buttonHeldKarp = this.graphSubmitComponent.buttonHeldKarp.nativeElement;
    this.tooltipComponent.buttonGreedyMatching = this.graphSubmitComponent.buttonGreedyMatching.nativeElement;
    this.tooltipComponent.buttonGreedyColoring = this.graphSubmitComponent.buttonGreedyColoring.nativeElement;
    this.tooltipComponent.buttonWelshPowell = this.graphSubmitComponent.buttonWelshPowell.nativeElement;
  }

  showTooltips() {
    this.showTooltip = !this.showTooltip;
    if (this.showTooltip) {;
      this.tooltipComponent.enableAllTooltips();
    }
    else {
      this.tooltipComponent.disableAllTooltips();
    }
  }
  
  private showContextMenu(x: number, y: number, type: 'node' | 'edge'): void {
    this.hideAllContextMenus();

    let menu: HTMLElement;
    if (type === 'node') {
      menu = this.nodeContextMenu.nativeElement;
    } else {
      menu = this.edgeContextMenu.nativeElement;
    }

    menu.style.left = `${x}px`;
    menu.style.top = `${y}px`;
    menu.style.display = 'block';
  }

  private hideAllContextMenus(): void {
    this.nodeContextMenu.nativeElement.style.display = 'none';
    this.edgeContextMenu.nativeElement.style.display = 'none';
  }

  promptEdgeWeight(): void {
    this.edgeWeightComponent.promptEdgeWeight(this.graphContainer, this.lastClickedElement, this.paper, this.defaultWeight, this.edgesConnectionComponent);
  }

  deleteSelected() {
    if (this.selectionComponent) {
      const result = this.selectionComponent.deleteSelected(this.startNode, this.endNode);

      this.startNode = result.startNode;
      this.endNode = result.endNode;

      this.hideAllContextMenus();
    }
  }

  setStartNode(): void {
    const node = this.lastClickedElement as joint.dia.Element;
    if (!node) return;
    if (this.startNode && node === this.startNode) {
      const oldOriginalText = this.startNode.attr('label/data-plain-text');
      if (oldOriginalText) {
        this.startNode.attr('label/text', oldOriginalText);
      }
      this.startNode = null;
      return;
    }

    if (this.endNode && this.endNode === node) {
      const oldOriginalText = this.endNode.attr('label/data-plain-text');
      if (oldOriginalText) {
        this.endNode.attr('label/text', oldOriginalText);
      }
      this.endNode = null;
    }
    
    if (this.startNode) {
      const oldOriginalText = this.startNode.attr('label/data-plain-text');
      if (oldOriginalText) {
        this.startNode.attr('label/text', oldOriginalText);
      }
    }
    
    this.startNode = node;
    const originalText = node.attr('label/text');
    node.attr('label/data-plain-text', originalText);
    
    node.attr('label/text', `(S) ${originalText}`);
    

    this.hideAllContextMenus();
  }

  setEndNode(): void {
    const node = this.lastClickedElement as joint.dia.Element;
    if (!node) return;

    if (this.endNode && node === this.endNode) {
      const oldOriginalText = this.endNode.attr('label/data-plain-text');
      if (oldOriginalText) {
        this.endNode.attr('label/text', oldOriginalText);
      }
      this.endNode = null;
      return;
    }

    if (this.startNode && this.startNode === node) {
      const oldOriginalText = this.startNode.attr('label/data-plain-text');
      if (oldOriginalText) {
        this.startNode.attr('label/text', oldOriginalText);
      }
      this.startNode = null;
    }

    if (this.endNode) {
      const oldOriginalText = this.endNode.attr('label/data-plain-text');
      if (oldOriginalText) {
        this.endNode.attr('label/text', oldOriginalText);
      }
    }

    
    this.endNode = node;
    const originalText = node.attr('label/text');
    node.attr('label/data-plain-text', originalText);
    
    node.attr('label/text', `(X) ${originalText}`);

    this.hideAllContextMenus();
  }

  toggleDirectional(): void {
    this.directed = !this.directed;
    console.log(`Graph is now ${this.directed ? 'Directional' : 'Non-Directional'}`);

    if (!this.directed) {
      const links = this.graph.getLinks();
      const processed = new Set<string>();
      links.forEach(link => {
        if (processed.has(link.id.toString())) {
          return;
        }
        const src = link.getSourceElement();
        const tgt = link.getTargetElement();
        if (src && tgt) {
          const oppositeLink = this.graph.getLinks().find(l =>
            l.id !== link.id &&
            !processed.has(l.id.toString()) &&
            l.getSourceElement() === tgt &&
            l.getTargetElement() === src
          );
          if (oppositeLink) {
            link.vertices([]);
            this.graph.removeCells([oppositeLink]);
            processed.add(link.id.toString());
            processed.add(oppositeLink.id.toString());
          }
        }
      });
    }

    setTimeout(() => {
      this.graph.getLinks().forEach(link => {
        this.edgesConnectionComponent.updateLinkStyle(link);
      });
    });
  }

  toggleWeighted(): void {
    this.weighted = !this.weighted;
    console.log(`Graph is now ${this.weighted ? 'Weighted' : 'Unweighted'}`);
    setTimeout(() => {
      this.graph.getLinks().forEach(link => {
        this.edgesConnectionComponent.updateLinkStyle(link);
      });
    });
  }
  
  renameNode(): void {
    const node = this.lastClickedElement as joint.dia.Element;
    if (!node) return;

    this.renameNodesComponent.inlineEditNodeLabel(node);
  }

  private initEdgeCreation(): void {
    this.edgesConnectionComponent.defaultWeight = this.defaultWeight;
    this.edgesConnectionComponent.initEdgeCursorIcon();
    this.edgesConnectionComponent.enableEdgeCreationButton(this.sampleEdge);
  }

  private enableDragging(): void {
    const sampleNode = this.sampleNode.nativeElement as HTMLElement;
    sampleNode.addEventListener('dragstart', (event) => {
      event.dataTransfer?.setData('text/plain', 'node');
    });
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();

    if (this.graph.getElements().length >= MAX_NODES) {
      const error = `Maximum node limit reached. Cannot add more than ${MAX_NODES} nodes.`;
      this.errorsService.updateErrors(error);
      return;
    }

    const rect = this.graphContainer.nativeElement.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;
    const nodeName = this.getUniqueNodeName('Node');
    this.addNode(x, y, nodeName);
  }

  private addNode(x: number, y: number, label: string): void {
    const circle = new joint.shapes.standard.Circle({
      id: uuidv4()
    });
    circle.position(x - this.nodeRadius / 2, y - this.nodeRadius / 2);
    circle.resize(this.nodeRadius, this.nodeRadius);
    circle.attr({
      body: { fill: NODE_COLOR_FILL, stroke: NODE_COLOR_STROKE, strokeWidth: 2, cursor: 'pointer', transition: '0.3s' },
      label: { text: label, fill: LABEL_COLOR_WHITE, fontSize: 14, cursor: 'default' }
    });
    circle.attr('label/data-plain-text', label)
    circle.addTo(this.graph);
  }

  private getUniqueNodeName(baseName: string): string {
    let name = baseName;
    let index = 1;

    while (this.graph.getElements().some(node => node.attr('label/text') === name)) {
      name = `${baseName} (${index++})`;
    }

    return name;
  }
}
