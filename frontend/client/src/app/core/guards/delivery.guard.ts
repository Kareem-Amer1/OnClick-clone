import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { DeliveryService } from 'src/app/delivery/delivery.service';

@Injectable({
  providedIn: 'root'
})
export class DeliveryGuard implements CanActivate {
  constructor(private deliveryService: DeliveryService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> {
    return this.deliveryService.currentDeliveryPerson$.pipe(
      map(delivery => {
        if (delivery) {
          return true;
        }
        this.router.navigate(['/delivery/login'], { queryParams: { returnUrl: state.url } });
        return false;
      })
    );
  }
} 