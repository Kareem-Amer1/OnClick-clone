import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { DeliveryService } from './delivery.service';

@Component({
  selector: 'app-delivery-login',
  templateUrl: './delivery-login.component.html',
  styleUrls: ['./delivery-login.component.scss']
})
export class DeliveryLoginComponent implements OnInit {
  loginForm: FormGroup;

  constructor(
    private formBuilder: FormBuilder,
    private deliveryService: DeliveryService,
    private router: Router
  ) { }

  ngOnInit() {
    this.createLoginForm();
  }

  createLoginForm() {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      this.deliveryService.login(this.loginForm.value).subscribe(
        response => {
          if (response) {
            this.router.navigateByUrl('/delivery/home');
          }
        },
        error => {
          console.log(error);
        }
      );
    }
  }
} 