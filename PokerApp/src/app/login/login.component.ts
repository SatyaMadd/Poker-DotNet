import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../auth.service'; 

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule]
})
export class LoginComponent {
  loginUsername: string = '';
  loginPassword: string = '';
  loginError: string = '';

  constructor(private authService: AuthService) {}

  onLogin() {
    this.authService.login(this.loginUsername, this.loginPassword)
      .subscribe({
        next: (data: any) => {
          if (data.error) {
            this.loginError = data.error;
          } else {
            console.log('Success:', data);
            this.loginError = '';
            localStorage.setItem('jwtToken', data.token);
            window.location.href = data.redirectUrl;
          }
        },
        error: (error) => {
          console.error('Error:', error);
          this.loginError = 'Login failed.';
        },
        complete: () => {
        }
      });
  }
}
