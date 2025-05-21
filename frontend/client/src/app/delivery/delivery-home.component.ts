import { Component, OnInit } from '@angular/core';
import { DeliveryService } from './delivery.service';
import { IDeliveryMethod } from '../shared/models/deliveryMethod';

@Component({
  selector: 'app-delivery-home',
  templateUrl: './delivery-home.component.html',
  styleUrls: ['./delivery-home.component.scss']
})
export class DeliveryHomeComponent implements OnInit {
  deliveryPerson: IDeliveryMethod;

  constructor(private deliveryService: DeliveryService) { }

  ngOnInit() {
    this.deliveryService.currentDeliveryPerson$.subscribe(
      delivery => {
        this.deliveryPerson = delivery;
      }
    );
  }

  logout() {
    this.deliveryService.logout();
  }
} 