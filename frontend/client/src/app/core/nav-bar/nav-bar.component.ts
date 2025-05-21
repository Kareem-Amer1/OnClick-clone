import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { AccountService } from 'src/app/account/account.service';
import { BasketService } from 'src/app/basket/basket.service';
import { IBasket } from 'src/app/shared/models/basket';
import { IUser } from 'src/app/shared/models/user';
import { DeliveryService } from 'src/app/delivery/delivery.service';
import { IDeliveryMethod } from 'src/app/shared/models/deliveryMethod';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.scss']
})
export class NavBarComponent implements OnInit {
  basket$: Observable<IBasket>;
  currentUser$: Observable<IUser>;
  currentDeliveryPerson$: Observable<IDeliveryMethod>;
  isDeliveryPerson$: Observable<boolean>;

  constructor(
    private basketService: BasketService, 
    private accountService: AccountService,
    private deliveryService: DeliveryService
  ) { }

  ngOnInit() {
    this.basket$ = this.basketService.basket$;
    this.currentUser$ = this.accountService.currentUser$;
    this.currentDeliveryPerson$ = this.deliveryService.currentDeliveryPerson$;
    this.isDeliveryPerson$ = this.currentDeliveryPerson$.pipe(
      map(delivery => !!delivery)
    );
  }

  logout() {
    this.accountService.logout();
  }

  logoutDelivery() {
    this.deliveryService.logout();
  }
}