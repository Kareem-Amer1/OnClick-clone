import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { IDeliveryMethod } from '../shared/models/deliveryMethod';

@Injectable({
  providedIn: 'root'
})
export class DeliveryService {
  baseUrl = environment.apiUrl;
  private currentDeliveryPersonSource = new ReplaySubject<IDeliveryMethod>(1);
  currentDeliveryPerson$ = this.currentDeliveryPersonSource.asObservable();

  constructor(private http: HttpClient, private router: Router) { }

  loadCurrentDeliveryPerson() {
    const deliveryToken = localStorage.getItem('delivery_token');
    if (deliveryToken) {
      const deliveryPerson = JSON.parse(deliveryToken);
      this.currentDeliveryPersonSource.next(deliveryPerson);
    }
  }

  login(values: any) {
    return this.http.post(this.baseUrl + 'delivery/login', values).pipe(
      map((response: any) => {
        if (response.success) {
          const deliveryPerson = response.deliveryPerson;
          localStorage.setItem('delivery_token', JSON.stringify(deliveryPerson));
          this.currentDeliveryPersonSource.next(deliveryPerson);
          return true;
        }
        return false;
      })
    );
  }

  logout() {
    localStorage.removeItem('delivery_token');
    this.currentDeliveryPersonSource.next(null);
    this.router.navigateByUrl('/');
  }
} 