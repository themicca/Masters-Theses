import { Component, ElementRef, Input, SimpleChanges } from '@angular/core';
import { v4 as uuidv4 } from 'uuid';
import * as joint from 'jointjs';

@Component({
  selector: 'app-edges-connection',
  templateUrl: './edges-connection.component.html',
  styleUrl: './edges-connection.component.css'
})
export class EdgesConnectionComponent {
  @Input() graph!: joint.dia.Graph;
  @Input() paper!: joint.dia.Paper;
  @Input() directed!: boolean;
  @Input() weighted!: boolean;
  public defaultWeight!: number;
  private sourceEdgeNode: joint.dia.Element | null = null;
  private isEdgeCreationMode: boolean = false;
  private cursorIcon!: HTMLElement;

  ngOnChanges(changes: SimpleChanges) {
    if (changes['paper'] && this.paper) {
      console.log('Paper received, setting up listeners...');
      this.setupListeners();
    }
  }

  setupListeners() {
    this.paper.on('element:pointerdown', (elementView: any) => {
      if (!this.isEdgeCreationMode) return;

      if (!this.sourceEdgeNode) {
        this.sourceEdgeNode = elementView.model;
      } else if (this.sourceEdgeNode !== elementView.model) {
        if (!this.doesLinkExist(this.sourceEdgeNode, elementView.model)) {
          this.createLink(this.sourceEdgeNode, elementView.model);
        }
        this.sourceEdgeNode = elementView.model;
      }
    });

    this.paper.on('blank:pointerdown', () => {
      this.sourceEdgeNode = null;
    });

    this.paper.on('blank:contextmenu', () => {
      if (this.isEdgeCreationMode) {
        this.exitEdgeCreationMode();
      }
    });
  }

  private doesLinkExist(source: joint.dia.Element, target: joint.dia.Element): boolean {
    return this.graph.getLinks().some(link => {
      return (
        (link.getSourceElement() === source && link.getTargetElement() === target) ||
        (!this.directed && link.getSourceElement() === target && link.getTargetElement() === source)
      );
    });
  }

  private createLink(source: joint.dia.Element, target: joint.dia.Element): void {
    const link = new joint.shapes.standard.Link({
      id: uuidv4()
    });
    link.source(source);
    link.target(target);
    link.attr('weight', this.defaultWeight);
    this.updateLinkStyle(link);

    link.addTo(this.graph);

    if (this.directed) {
      this.offsetParallelLinks(link, source, target);
    }
  }

  public updateLinkStyle(link: joint.dia.Link): void {
    link.attr({
      line: {
        stroke: '#2c3e50',
        strokeWidth: 3,
        strokeLinejoin: 'round',
        targetMarker: this.directed ? { type: 'path' } : { type: 'none' }
      }
    });

    if (this.weighted) {
      link.labels([{
        position: 0.5,
        attrs: {
          text: {
            text: link.attr('weight').toString(),
            fill: 'black'
          }
        }
      }]);
    }
    else {
      link.labels([]);
    }
  }

  private offsetParallelLinks(
    link: joint.dia.Link,
    source: joint.dia.Element,
    target: joint.dia.Element
  ): void {
    const oppositeLink = this.graph.getLinks().find(l =>
      l.getSourceElement() === target && l.getTargetElement() === source
    );
    if (!oppositeLink) return;
    
    const sourceCenter = source.getBBox().center();
    const targetCenter = target.getBBox().center();
    const midX = (sourceCenter.x + targetCenter.x) / 2;
    const midY = (sourceCenter.y + targetCenter.y) / 2;
    
    link.vertices([{ x: midX, y: midY + 20 }]);
    oppositeLink.vertices([{ x: midX, y: midY - 20 }]);
  }

  public enableEdgeCreationButton(sampleEdge: ElementRef): void {
    const sEdge = sampleEdge.nativeElement as HTMLElement;
    sEdge.addEventListener('click', () => {
      this.enterEdgeCreationMode();
    });
  }

  private enterEdgeCreationMode(): void {
    this.isEdgeCreationMode = true;
    this.cursorIcon.style.display = 'block';
  }

  private exitEdgeCreationMode(): void {
    this.isEdgeCreationMode = false;
    this.cursorIcon.style.display = 'none';
    if (this.sourceEdgeNode) {
      this.sourceEdgeNode = null;
    }
  }

  public initEdgeCursorIcon(): void {
    this.cursorIcon = document.createElement('div');
    this.cursorIcon.innerHTML = '🔗';
    this.cursorIcon.style.position = 'fixed';
    this.cursorIcon.style.display = 'none';
    this.cursorIcon.style.pointerEvents = 'none';
    this.cursorIcon.style.fontSize = '20px';
    this.cursorIcon.style.color = 'red';
    this.cursorIcon.style.zIndex = '20';
    document.body.appendChild(this.cursorIcon);

    document.addEventListener('mousemove', (event: MouseEvent) => {
      this.cursorIcon.style.left = `${event.clientX + 10}px`;
      this.cursorIcon.style.top = `${event.clientY + 10}px`;
    });
  }
}
