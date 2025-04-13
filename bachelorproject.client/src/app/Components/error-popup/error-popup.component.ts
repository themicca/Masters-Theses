import { Component } from '@angular/core';
import { GraphErrorsService } from '../../services/graph-errors.service';

@Component({
  selector: 'app-error-popup',
  templateUrl: './error-popup.component.html',
  styleUrl: './error-popup.component.css'
})
export class ErrorPopupComponent {
  showModal = false;
  algo: string = "";
  errors: string[] = [];

  constructor(private errorsService: GraphErrorsService) { }

  ngOnInit() {
    this.errorsService.errors$.subscribe(data => {
      if (!data || !data.errors) return;
      if (typeof data.errors === 'string') {
        this.errors = [data.errors];
      }
      else {
        this.errors = data.errors;
      }

      this.algo = data.algo || "";
      this.showModal = true;
    });
  }

  get headerMessage(): string {
    return this.algo && this.algo.trim() !== ""
      ? `Cannot run ${this.algo} for these reasons:`
      : "There were errors:";
  }
  
  closeModal() {
    this.showModal = false;
  }
}
