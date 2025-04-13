import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GraphErrorsService {
  private errorsSubject = new BehaviorSubject<any>(null);
  errors$ = this.errorsSubject.asObservable();

  updateErrors(errors: string[] | string, algo?: string) {
    this.errorsSubject.next({errors, algo});
  }
}
