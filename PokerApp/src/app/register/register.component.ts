// src/app/register/register.component.ts
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../auth.service'; // Import AuthService

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule]
})
export class RegisterComponent {
  registerUsername: string = '';
  registerPassword: string = '';
  registerError: string = '';

  constructor(private authService: AuthService) {}

  onRegister() {
    this.authService.register(this.registerUsername, this.registerPassword)
      .subscribe(
        (data: any) => {
          if (data.error) {
            this.registerError = data.error;
          } else {
            console.log('Success:', data);
            this.registerError = data.message || 'Registration successful!';
          }
        },
        (error) => {
          console.error('Error:', error);
          this.registerError = "Couldn't register.";
        }
      );
  }
}
