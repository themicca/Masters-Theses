import { Component, ElementRef } from '@angular/core';

@Component({
  selector: 'app-edge-weight',
  templateUrl: './edge-weight.component.html',
  styleUrl: './edge-weight.component.css'
})
export class EdgeWeightComponent {
  private weightInputBox!: HTMLElement | null;
  private readonly MIN_WEIGHT = -2147483648;
  private readonly MAX_WEIGHT = 2147483647;

  promptEdgeWeight(graphContainer: ElementRef, lastClickedElement: joint.dia.Element | joint.dia.Link | null = null, paper: joint.dia.Paper, defaultWeight: number): void {
    const edge = lastClickedElement as joint.dia.Link;
    
    const linkView = paper.findViewByModel(edge) as joint.dia.LinkView;
    if (!linkView) return;

    const midPoint = linkView.getPointAtRatio(0.5);
    const paperOffset = graphContainer.nativeElement.getBoundingClientRect();
    
    this.removeWeightInputBox();
    
    this.weightInputBox = document.createElement('div');
    this.weightInputBox.style.position = 'fixed';
    this.weightInputBox.style.left = `${paperOffset.left + midPoint.x}px`;
    this.weightInputBox.style.top = `${paperOffset.top + midPoint.y}px`;
    this.weightInputBox.style.padding = '8px';
    this.weightInputBox.style.background = '#fff';
    this.weightInputBox.style.border = '1px solid #ccc';
    this.weightInputBox.style.borderRadius = '5px';
    this.weightInputBox.style.boxShadow = '0 2px 10px rgba(0,0,0,0.2)';
    this.weightInputBox.style.zIndex = '1000';
    
    const inputField = document.createElement('input');
    inputField.type = 'number';
    inputField.value = edge.attr('weight') || defaultWeight;
    inputField.style.width = '60px';
    inputField.style.marginRight = '5px';

    const errorMsg = document.createElement('div');
    errorMsg.style.color = 'red';
    errorMsg.style.fontSize = '12px';
    errorMsg.style.marginTop = '4px';
    errorMsg.textContent = '';

    const confirmButton = document.createElement('button');
    confirmButton.innerText = '✔';
    confirmButton.style.cursor = 'pointer';
    confirmButton.style.marginRight = '5px';

    const confirmEdgeWeight = () => {
      const rawValue = parseInt(inputField.value, 10);
      const isValid = this.validateInput(rawValue);
      if (!isValid) {
        errorMsg.textContent = this.getErrorMessage(rawValue);
        this.shakeElement(inputField);
      } else {
        edge.attr('weight', rawValue);
        edge.labels([
          {
            position: 0.5,
            attrs: { text: { text: rawValue.toString(), fill: 'black' } }
          }
        ]);
        this.removeWeightInputBox();
      }
    };

    confirmButton.onclick = confirmEdgeWeight;

    const cancelButton = document.createElement('button');
    cancelButton.innerText = '✖';
    cancelButton.style.cursor = 'pointer';
    cancelButton.onclick = () => this.removeWeightInputBox();
    
    this.weightInputBox.appendChild(inputField);
    this.weightInputBox.appendChild(confirmButton);
    this.weightInputBox.appendChild(cancelButton);
    this.weightInputBox.appendChild(errorMsg);
    document.body.appendChild(this.weightInputBox);

    setTimeout(() => {
      document.addEventListener('pointerdown', this.handleOutsideClick);
      document.addEventListener('keydown', this.handleEnterKey);
    }, 10);

    this.injectShakeStyles();
  }

  private validateInput(value: number): boolean {
    if (isNaN(value)) return false;
    if (value === 0) return false;
    if (value < this.MIN_WEIGHT || value > this.MAX_WEIGHT) return false;
    return true;
  }

  private getErrorMessage(value: number): string {
    if (isNaN(value)) {
      return 'Please enter a valid integer.';
    } else if (value === 0) {
      return 'Zero is not allowed. Please enter a non-zero value.';
    } else if (value < this.MIN_WEIGHT) {
      return `Value cannot be lower than ${this.MIN_WEIGHT}.`;
    } else if (value > this.MAX_WEIGHT) {
      return `Value cannot exceed ${this.MAX_WEIGHT}.`;
    }
    return 'Invalid value.';
  }

  private shakeElement(el: HTMLElement) {
    el.classList.remove('shake');
    void el.offsetWidth;
    el.classList.add('shake');
  }

  private handleEnterKey = (e: KeyboardEvent): void => {
    if (e.key === 'Enter') {
      if (this.weightInputBox) {
        const btn = this.weightInputBox.querySelector('button');
        if (btn) {
          (btn as HTMLButtonElement).click();
        }
      }
    }
  };

  private handleOutsideClick = (event: MouseEvent): void => {
    if (this.weightInputBox && !this.weightInputBox.contains(event.target as Node)) {
      const btn = this.weightInputBox.querySelector('button');
      if (btn) {
        (btn as HTMLButtonElement).click();
      }
    }
  };

  private removeWeightInputBox(): void {
    if (this.weightInputBox) {
      this.weightInputBox.remove();
      this.weightInputBox = null;
    }
    document.removeEventListener('pointerdown', this.handleOutsideClick);
  }

  private injectShakeStyles(): void {
    if (document.getElementById('shake-styles')) return;

    const style = document.createElement('style');
    style.id = 'shake-styles';
    style.innerHTML = `
      @keyframes shake {
        0% { transform: translateX(0); }
        20% { transform: translateX(-3px); }
        40% { transform: translateX(3px); }
        60% { transform: translateX(-3px); }
        80% { transform: translateX(3px); }
        100% { transform: translateX(0); }
      }
      .shake {
        animation: shake 0.3s;
      }
    `;
    document.head.appendChild(style);
  }
}
