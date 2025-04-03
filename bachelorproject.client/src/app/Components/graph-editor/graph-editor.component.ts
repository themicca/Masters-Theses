import { Component, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import * as joint from 'jointjs';
import { GraphSelectionComponent } from '../graph-selection/graph-selection.component';
import { EdgeWeightComponent } from '../edge-weight/edge-weight.component';
import { EdgesConnectionComponent } from '../edges-connection/edges-connection.component';
import { v4 as uuidv4 } from 'uuid';

@Component({
  selector: 'app-graph-editor',
  templateUrl: './graph-editor.component.html',
  styleUrl: './graph-editor.component.css'
})
export class GraphEditorComponent implements AfterViewInit {
  @ViewChild('graphContainer', { static: false }) graphContainer!: ElementRef;
  @ViewChild('sampleNode', { static: false }) sampleNode!: ElementRef;
  @ViewChild('sampleEdge', { static: false }) sampleEdge!: ElementRef;
  @ViewChild('nodeContextMenu', { static: false }) nodeContextMenu!: ElementRef;
  @ViewChild('edgeContextMenu', { static: false }) edgeContextMenu!: ElementRef;
  @ViewChild(GraphSelectionComponent) selectionComponent!: GraphSelectionComponent;
  @ViewChild(EdgeWeightComponent) edgeWeightComponent!: EdgeWeightComponent;
  @ViewChild(EdgesConnectionComponent) edgesConnectionComponent!: EdgesConnectionComponent;

  protected graph!: joint.dia.Graph;
  protected paper!: joint.dia.Paper;
  private nodeRadius: number = 80;
  public lastClickedElement: joint.dia.Element | joint.dia.Link | null = null;
  protected directed: boolean = true;
  protected startNode: joint.dia.Element | null = null;
  protected endNode: joint.dia.Element | null = null;
  private tooltipElement!: HTMLElement;
  private readonly defaultWeight = 1;

  constructor() { }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.initGraph();
      this.addListeners();
      this.initTooltips();
    });
    this.enableDragging();
    this.initEdgeCreation();
  }

  private initGraph(): void {
    this.graph = new joint.dia.Graph();

    this.paper = new joint.dia.Paper({
      el: this.graphContainer.nativeElement,
      model: this.graph,
      width: '100%',
      height: 650,
      gridSize: 10,
      drawGrid: true,
      background: { color: '#f8f9fa' },
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
    });

    this.paper.on('element:mouseenter', (elementView: any) => {
      elementView.model.attr('body/filter', {
        name: 'brightness',
        args: { amount: 0.5 }
      });
    });

    this.paper.on('element:mouseleave', (elementView: any) => {
      elementView.model.removeAttr('body/filter');
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

    this.paper.on('element:pointerup', (elementView: any, evt) => {
      if (evt.detail === 2) {
        this.inlineEditNodeLabel(elementView.model as joint.dia.Element);
      }
    });
  }

  private initTooltips(): void {
    this.tooltipElement = document.createElement('div');
    this.tooltipElement.className = 'graph-tooltip';
    this.tooltipElement.style.display = 'none';
    this.tooltipElement.style.position = 'fixed';
    this.tooltipElement.style.backgroundColor = '#fff';
    this.tooltipElement.style.color = '#333';
    this.tooltipElement.style.border = '1px solid #ccc';
    this.tooltipElement.style.borderRadius = '4px';
    this.tooltipElement.style.padding = '8px 12px';
    this.tooltipElement.style.fontSize = '14px';
    this.tooltipElement.style.fontFamily = 'Arial, sans-serif';
    this.tooltipElement.style.boxShadow = '0 2px 5px rgba(0, 0, 0, 0.15)';
    this.tooltipElement.style.zIndex = '999999';
    document.body.appendChild(this.tooltipElement);
    
    const sampleNodeEl = this.sampleNode.nativeElement as HTMLElement;
    sampleNodeEl.addEventListener('mouseenter', (event: MouseEvent) => {
      this.tooltipElement.innerText = 'Drag this to the paper to make a node';
      this.tooltipElement.style.display = 'block';
    });
    sampleNodeEl.addEventListener('mouseleave', () => {
      this.tooltipElement.style.display = 'none';
    });
    sampleNodeEl.addEventListener('mousemove', (event: MouseEvent) => {
      this.tooltipElement.style.left = event.clientX + 10 + 'px';
      this.tooltipElement.style.top = event.clientY + 10 + 'px';
    });
    
    const sampleEdgeEl = this.sampleEdge.nativeElement as HTMLElement;
    sampleEdgeEl.addEventListener('mouseenter', (event: MouseEvent) => {
      this.tooltipElement.innerText = 'Click this to create an edge';
      this.tooltipElement.style.display = 'block';
    });
    sampleEdgeEl.addEventListener('mouseleave', () => {
      this.tooltipElement.style.display = 'none';
    });
    sampleEdgeEl.addEventListener('mousemove', (event: MouseEvent) => {
      this.tooltipElement.style.left = event.clientX + 10 + 'px';
      this.tooltipElement.style.top = event.clientY + 10 + 'px';
    });
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
    this.edgeWeightComponent.promptEdgeWeight(this.graphContainer, this.lastClickedElement, this.paper, this.defaultWeight);
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

  renameNode(): void {
    const node = this.lastClickedElement as joint.dia.Element;
    if (!node) return;

    this.inlineEditNodeLabel(node);
  }

  private inlineEditNodeLabel(node: joint.dia.Element): void {
    let currentLabel = node.attr('label/data-plain-text') || '';

    const bbox = node.getBBox();
    const paperPoint = this.paper.localToPagePoint({ x: bbox.x, y: bbox.y });
    
    const input = document.createElement('input');
    input.type = 'text';
    input.value = currentLabel;
    input.style.position = 'fixed';
    input.style.left = paperPoint.x + 'px';
    input.style.top = paperPoint.y + 'px';
    input.style.width = bbox.width + 'px';
    input.style.fontSize = '14px';
    input.style.zIndex = '999';

    document.body.appendChild(input);
    input.focus();
    
    const finishEditing = () => {
      let newLabel = input.value.trim() || currentLabel;

      node.attr('label/data-plain-text', newLabel);

      if (node === this.startNode)
        node.attr('label/text', `(S) ${newLabel}`);
      else if (node === this.endNode)
        node.attr('label/text', `(X) ${newLabel}`);
      else {
        node.attr('label/text', newLabel);
      }

      document.body.removeChild(input);
    };
    
    input.addEventListener('blur', finishEditing);
    
    input.addEventListener('keydown', (e) => {
      if (e.key === 'Enter') {
        finishEditing();
      }
    });
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
      body: { fill: '#3498db', stroke: '#2980b9', strokeWidth: 2, cursor: 'pointer', transition: '0.3s' },
      label: { text: label, fill: '#ffffff', fontSize: 14, cursor: 'default' }
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
