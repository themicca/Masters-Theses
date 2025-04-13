import { Component, EventEmitter, Input, Output, SimpleChanges } from '@angular/core';
import { EDGE_COLOR_STROKE, HOVER_COLOR, NODE_COLOR_STROKE, SELECTED_COLOR } from '../../utils/constants';

@Component({
  selector: 'app-graph-selection',
  templateUrl: './graph-selection.component.html',
  styleUrl: './graph-selection.component.css'
})
export class GraphSelectionComponent {
  @Input() graph!: joint.dia.Graph;
  @Input() paper!: joint.dia.Paper;
  @Output() lastClickedElementChange = new EventEmitter<joint.dia.Element | joint.dia.Link | null>();
  private isMarqueeSelecting: boolean = false;
  private marqueeRect!: HTMLElement;
  private marqueeStartClientX = 0;
  private marqueeStartClientY = 0;
  private currentHoverSet = new Set<joint.dia.Cell>();
  public lastClickedElement: joint.dia.Element | joint.dia.Link | null = null;
  public selectedElements: Array<joint.dia.Element | joint.dia.Link> = [];
  
  ngOnChanges(changes: SimpleChanges) {
    if (changes['paper'] && this.paper) {
      this.setupListeners();
    }
  }
  private setupListeners() {
    this.paper.on('blank:pointerdown', (evt: MouseEvent) => {
      this.isMarqueeSelecting = true;
      this.marqueeStartClientX = evt.clientX;
      this.marqueeStartClientY = evt.clientY;
      this.createMarquee(evt.clientX, evt.clientY);
    });
    
    this.paper.on('blank:pointermove', (evt: MouseEvent) => {
      if (!this.isMarqueeSelecting) return;
      this.updateMarquee(evt.clientX, evt.clientY);
    });
    
    this.paper.on('blank:pointerup', (evt: MouseEvent) => {
      if (this.isMarqueeSelecting) {
        this.isMarqueeSelecting = false;
        this.selectMarqueeItems();
        this.destroyMarquee();
      }
    });

    this.paper.on('element:contextmenu', (elementView: any, evt: MouseEvent) => {
      evt.preventDefault();
      this.lastClickedElement = elementView.model;
      this.emitLastClickedElement();
      if (!this.selectedElements.includes(elementView.model)) {
        this.clearSelection();
        this.selectItem(elementView.model);
      }
    });

    this.paper.on('link:contextmenu', (linkView: any, evt: MouseEvent) => {
      evt.preventDefault();
      this.lastClickedElement = linkView.model;
      this.emitLastClickedElement();
      if (!this.selectedElements.includes(linkView.model)) {
        this.clearSelection();
        this.selectItem(linkView.model);
      }
    });

    this.paper.on('element:pointerclick', (elementView: any, evt: MouseEvent) => {
      if (!evt.shiftKey && !evt.ctrlKey) {
        this.clearSelection();
      }
      this.selectItem(elementView.model);
      this.lastClickedElement = elementView.model;
      this.emitLastClickedElement();
    });

    this.paper.on('link:pointerclick', (linkView: any, evt: MouseEvent) => {
      this.lastClickedElement = linkView.model;
      this.emitLastClickedElement();
      if (!evt.shiftKey && !evt.ctrlKey) {
        this.clearSelection();
      }
      this.selectItem(linkView.model);
    });
  }

  emitLastClickedElement() {
    this.lastClickedElementChange.emit(this.lastClickedElement);
  }

  private createMarquee(clientX: number, clientY: number) {
    this.marqueeStartClientX = clientX;
    this.marqueeStartClientY = clientY;
    
    this.marqueeRect = document.createElement('div');
    this.marqueeRect.style.position = 'fixed';
    this.marqueeRect.style.border = '2px dashed #666';
    this.marqueeRect.style.background = 'rgba(100,100,100,0.2)';
    this.marqueeRect.style.zIndex = '999';
    
    this.marqueeRect.style.left = clientX + 'px';
    this.marqueeRect.style.top = clientY + 'px';
    this.marqueeRect.style.width = '0px';
    this.marqueeRect.style.height = '0px';

    document.body.appendChild(this.marqueeRect);
  }

  private updateMarquee(currentX: number, currentY: number) {
    if (!this.marqueeRect) return;
    
    const startX = this.marqueeStartClientX;
    const startY = this.marqueeStartClientY;
    
    const left = Math.min(startX, currentX);
    const top = Math.min(startY, currentY);
    const width = Math.abs(currentX - startX);
    const height = Math.abs(currentY - startY);
    
    this.marqueeRect.style.left = left + 'px';
    this.marqueeRect.style.top = top + 'px';
    this.marqueeRect.style.width = width + 'px';
    this.marqueeRect.style.height = height + 'px';

    this.updateMarqueeHover(left, top, width, height);
  }

  private updateMarqueeHover(domLeft: number, domTop: number, domWidth: number, domHeight: number) {
    const domRight = domLeft + domWidth;
    const domBottom = domTop + domHeight;

    const paperTopLeft = this.paper.clientToLocalPoint(domLeft, domTop);
    const paperBottomRight = this.paper.clientToLocalPoint(domRight, domBottom);
    
    const newHoverSet = new Set<joint.dia.Cell>();
    
    this.graph.getCells().forEach(cell => {
      if (this.doesCellIntersectMarquee(cell, paperTopLeft, paperBottomRight)) {
        newHoverSet.add(cell);
        if (!this.currentHoverSet.has(cell)) {
          this.applyHoverFilter(cell);
        }
      }
    });
    
    this.currentHoverSet.forEach(cell => {
      if (!newHoverSet.has(cell)) {
        this.removeHoverFilter(cell);
      }
    });
    
    this.currentHoverSet = newHoverSet;
  }

  private doesCellIntersectMarquee(
    cell: joint.dia.Cell,
    topLeft: { x: number; y: number },
    bottomRight: { x: number; y: number }
  ): boolean {
    if (cell.isElement()) {
      const bbox = (cell as joint.dia.Element).getBBox();
      return this.bboxIntersects(bbox, topLeft, bottomRight);
    } else if (cell.isLink()) {
      const linkView = this.paper.findViewByModel(cell) as joint.dia.LinkView;
      if (!linkView) return false;
      const linkBbox = linkView.getBBox();
      return this.bboxIntersects(linkBbox, topLeft, bottomRight);
    }
    return false;
  }

  private bboxIntersects(
    bbox: { x: number; y: number; width: number; height: number },
    topLeft: { x: number; y: number },
    bottomRight: { x: number; y: number }
  ): boolean {
    const boxLeft = bbox.x;
    const boxTop = bbox.y;
    const boxRight = bbox.x + bbox.width;
    const boxBottom = bbox.y + bbox.height;

    const selLeft = topLeft.x;
    const selTop = topLeft.y;
    const selRight = bottomRight.x;
    const selBottom = bottomRight.y;
    
    const horizOverlap = (boxLeft <= selRight) && (boxRight >= selLeft);
    const vertOverlap = (boxTop <= selBottom) && (boxBottom >= selTop);
    return horizOverlap && vertOverlap;
  }
  
  private applyHoverFilter(cell: joint.dia.Cell): void {
    if (cell.isElement()) {
      this.highlightNode(cell);
    }
    else if (cell.isLink()) {
      this.highlightLink(cell);
    }
  }

  private removeHoverFilter(cell: joint.dia.Cell) {
    if (cell.isElement()) {
      this.unhighlightNode(cell);
    }
    else if (cell.isLink()) {
      this.unhighlightLink(cell);
    }
  }

  private destroyMarquee() {
    if (this.marqueeRect && this.marqueeRect.parentNode) {
      this.marqueeRect.parentNode.removeChild(this.marqueeRect);
    }
    this.marqueeRect = undefined as any;
  }

  private selectMarqueeItems() {
    this.clearSelection();
    
    this.currentHoverSet.forEach(cell => {
      this.removeHoverFilter(cell);
      this.selectItem(cell);
    });
    
    this.currentHoverSet.clear();
  }
  
  private selectItem(cell: joint.dia.Cell) {
    if (cell.isElement()) {
      this.unhighlightNode(cell);
      this.selectedElements.push(cell);
      this.selectNode(cell as joint.dia.Element);
    }
    if (cell.isLink()) {
      this.unhighlightLink(cell);
      this.selectedElements.push(cell);
      this.selectLink(cell as joint.dia.Link);
    }
  }

  private clearSelection() {
    const selection = [...this.selectedElements];
    this.selectedElements = [];
    selection.forEach(item => {
      if (item.isElement()) {
        this.unhighlightNode(item as joint.dia.Element);
      } else if (item.isLink()) {
        this.unhighlightLink(item as joint.dia.Link);
      }
    });
  }

  deleteSelected(startNode: joint.dia.Element | null, endNode: joint.dia.Element | null): { startNode: joint.dia.Element | null, endNode: joint.dia.Element | null } {
    if (this.selectedElements.length === 0) return { startNode, endNode };

    this.selectedElements.forEach(item => {
      if (item.isLink()) {
        const link = item as joint.dia.Link;
        const source = link.getSourceElement();
        const target = link.getTargetElement();

        if (source && target) {
          const oppositeLink = this.graph.getLinks().find(l =>
            l.getSourceElement() === target && l.getTargetElement() === source
          );
          if (oppositeLink) {
            oppositeLink.vertices([]);
          }
        }
      }

      if (item === startNode) {
        startNode = null;
      }
      if (item === endNode) {
        endNode = null;
      }
      item.remove();
    });
    
    this.selectedElements = [];

    return { startNode, endNode }
  }

  private selectNode(node: joint.dia.Element): void {
    node.attr('body/stroke', SELECTED_COLOR);
    node.attr('body/strokeWidth', 3);
  }

  public highlightNode(node: joint.dia.Element): void {
    node.attr('body/stroke', HOVER_COLOR);
    node.attr('body/strokeWidth', 3);
  }

  public unhighlightNode(node: joint.dia.Element): void {
    if (this.selectedElements.includes(node)) {
      node.attr('body/stroke', SELECTED_COLOR);
      node.attr('body/strokeWidth', 3);
    }
    else {
      node.attr('body/stroke', NODE_COLOR_STROKE);
      node.attr('body/strokeWidth', 2);
    }
  }

  private selectLink(link: joint.dia.Link): void {
    link.attr({
      line: {
        stroke: SELECTED_COLOR,
        strokeWidth: 3
      }
    });
  }

  public highlightLink(link: joint.dia.Link): void {
    link.attr({
      line: {
        stroke: HOVER_COLOR,
        strokeWidth: 3
      }
    });
  }

  public unhighlightLink(link: joint.dia.Link): void {
    if (this.selectedElements.includes(link)) {
      link.attr({
        line: {
          stroke: SELECTED_COLOR,
          strokeWidth: 3
        }
      });
    }
    else {
      link.attr({
        line: {
          stroke: EDGE_COLOR_STROKE,
          strokeWidth: 3
        }
      });
    }
  }
}
