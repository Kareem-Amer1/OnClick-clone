import { AfterViewInit, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { BasketService } from 'src/app/basket/basket.service';
import { IBasket } from 'src/app/shared/models/basket';
import { IOrder } from 'src/app/shared/models/order';
import { CheckoutService } from '../checkout.service';

declare var Stripe;

@Component({
  selector: 'app-checkout-payment',
  templateUrl: './checkout-payment.component.html',
  styleUrls: ['./checkout-payment.component.scss']
})
export class CheckoutPaymentComponent implements AfterViewInit, OnDestroy {
  @Input() checkoutForm: FormGroup;
  @ViewChild('cardNumber', { static: false }) cardNumberElement: ElementRef;
  @ViewChild('cardExpiry', { static: false }) cardExpiryElement: ElementRef;
  @ViewChild('cardCvc', { static: false }) cardCvcElement: ElementRef;
  stripe: any;
  cardNumber: any;
  cardExpiry: any;
  cardCvc: any;
  cardErrors: any;
  cardHandler = this.onChange.bind(this);
  loading = false;
  cardNumberValid = false;
  cardExpiryValid = false;
  cardCvcValid = false;

  constructor(
    private basketService: BasketService,
    private checkoutService: CheckoutService,
    private toastr: ToastrService,
    private router: Router
  ) { }

  ngAfterViewInit(): void {
    // Watch for payment method changes
    this.checkoutForm.get('paymentForm.paymentMethod').valueChanges.subscribe(value => {
      if (value === 'card') {
        this.initializeStripeElements();
      } else {
        this.destroyStripeElements();
      }
    });

    // Initialize if card is selected by default
    if (this.checkoutForm.get('paymentForm.paymentMethod').value === 'card') {
      this.initializeStripeElements();
    }
  }

  ngOnDestroy(): void {
    this.destroyStripeElements();
  }

  private destroyStripeElements() {
    if (this.cardNumber) {
      this.cardNumber.destroy();
      this.cardExpiry.destroy();
      this.cardCvc.destroy();
      this.cardNumber = null;
      this.cardExpiry = null;
      this.cardCvc = null;
    }
  }

  initializeStripeElements() {
    if (!this.cardNumberElement) {
      return; // Exit if elements are not yet available in the DOM
    }

    this.stripe = Stripe('pk_test_51RHnldE6dDSpfVJLyqCd6tnqGjQXcLsC77ynHsTfcipgmcK7GNhBZ0TZUFRIxRRM1UzSVIuQF3BUg0oZ6u9AOXzQ00Smie1vUW');
    const elements = this.stripe.elements();

    if (!this.cardNumber) {
      this.cardNumber = elements.create('cardNumber');
      this.cardNumber.mount(this.cardNumberElement.nativeElement);
      this.cardNumber.addEventListener('change', this.cardHandler);
    }

    if (!this.cardExpiry) {
      this.cardExpiry = elements.create('cardExpiry');
      this.cardExpiry.mount(this.cardExpiryElement.nativeElement);
      this.cardExpiry.addEventListener('change', this.cardHandler);
    }

    if (!this.cardCvc) {
      this.cardCvc = elements.create('cardCvc');
      this.cardCvc.mount(this.cardCvcElement.nativeElement);
      this.cardCvc.addEventListener('change', this.cardHandler);
    }
  }

  onChange(event) {
    if (event.error) {
      this.cardErrors = event.error.message;
    } else {
      this.cardErrors = null;
    }
    switch(event.elementType) {
      case 'cardNumber':
        this.cardNumberValid = event.complete;
        break;
      case 'cardExpiry':
        this.cardExpiryValid = event.complete;
        break;
      case 'cardCvc':
        this.cardCvcValid = event.complete;
        break;
    }
  }

  async submitOrder() {
    this.loading = true;
    const basket = this.basketService.getCurrentBasketValue();
    
    try {
      const orderToCreate = this.getOrderToCreate(basket);
      const createdOrder = await this.checkoutService.createOrder(orderToCreate).toPromise();
      
      if (this.checkoutForm.get('paymentForm').get('paymentMethod').value === 'card') {
        const paymentResult = await this.confirmPaymentWithStripe(basket);
        if (paymentResult.paymentIntent) {
          this.basketService.deleteLocalBasket(basket.id);
          const navigationExtras: NavigationExtras = { state: createdOrder };
          this.router.navigate(['checkout/success'], navigationExtras);
        } else {
          this.toastr.error(paymentResult.error.message);
        }
      } else {
        // Handle COD payment
        this.basketService.deleteLocalBasket(basket.id);
        const navigationExtras: NavigationExtras = { state: createdOrder };
        this.router.navigate(['checkout/success'], navigationExtras);
      }
    } catch (error) {
      console.log(error);
      this.toastr.error('An error occurred while processing your order');
    } finally {
      this.loading = false;
    }
  }

  private async confirmPaymentWithStripe(basket) {
    return this.stripe.confirmCardPayment(basket.clientSecret, {
      payment_method: {
        card: this.cardNumber,
        billing_details: {
          name: this.checkoutForm.get('paymentForm').get('nameOnCard').value
        }
      }
    });
  }

  private getOrderToCreate(basket: IBasket) {
    return {
      basketId: basket.id,
      deliveryMethodId: +this.checkoutForm.get('deliveryForm').get('deliveryMethod').value,
      shipToAddress: this.checkoutForm.get('addressForm').value,
      paymentMethod: this.checkoutForm.get('paymentForm').get('paymentMethod').value
    };
  }
}
