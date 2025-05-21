import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../account.service';
import { DeliveryService } from 'src/app/delivery/delivery.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  returnUrl: string;
  isDeliveryMode = false;

  constructor(
    private accountService: AccountService,
    private deliveryService: DeliveryService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.returnUrl = this.activatedRoute.snapshot.queryParams.returnUrl || '/shop';
    this.createLoginForm();
  }

  createLoginForm() {
    this.loginForm = new FormGroup({
      email: new FormControl('', [Validators.required, Validators
        .pattern('^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$')]),
      password: new FormControl('', Validators.required),
    });
  }

  setDeliveryMode() {
    this.isDeliveryMode = true;
  }

  setCustomerMode() {
    this.isDeliveryMode = false;
  }

  onSubmit() {
    if (this.isDeliveryMode) {
      this.deliveryService.login(this.loginForm.value).subscribe(() => {
        this.router.navigateByUrl('/delivery/home');
      }, error => {
        console.log(error);
      });
    } else {
      this.accountService.login(this.loginForm.value).subscribe(() => {
        this.router.navigateByUrl(this.returnUrl);
      }, error => {
        console.log(error);
      });
    }
  }
}