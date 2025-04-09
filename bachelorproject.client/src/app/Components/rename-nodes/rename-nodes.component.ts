import { Component, Input, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-rename-nodes',
  templateUrl: './rename-nodes.component.html',
  styleUrl: './rename-nodes.component.css'
})
export class RenameNodesComponent {
  @Input() paper!: joint.dia.Paper;
  @Input() startNode!: joint.dia.Element | null;
  @Input() endNode!: joint.dia.Element | null;

  ngOnChanges(changes: SimpleChanges) {
    if (changes['paper'] && this.paper) {
      this.paper.on('element:pointerup', (elementView: any, evt) => {
        if (evt.detail === 2) {
          this.inlineEditNodeLabel(elementView.model as joint.dia.Element);
        }
      });
    }
  }
  
  public inlineEditNodeLabel(node: joint.dia.Element): void {
    let currentLabel = node.attr('label/data-plain-text') || '';

    const bbox = node.getBBox();
    const paperPoint = this.paper.localToPagePoint({ x: bbox.x, y: bbox.y });

    const input = document.createElement('input');
    input.type = 'text';
    input.value = currentLabel;
    input.style.position = 'absolute';
    input.style.left = paperPoint.x + 'px';
    input.style.top = paperPoint.y + 'px';
    input.style.width = bbox.width + 'px';
    input.style.fontSize = '14px';
    input.style.zIndex = '999';

    document.body.appendChild(input);
    input.focus();

    let editingFinished = false;
    const finishEditing = () => {
      if (editingFinished) return;
      editingFinished = true;

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
      } else if (e.key === 'Escape') {
        editingFinished = true;
        if (document.body.contains(input)) {
          document.body.removeChild(input);
        }
      }
    });
  }
}
