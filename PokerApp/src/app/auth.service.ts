import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://poker-api-gngmc9cphsfjeeha.eastus-01.azurewebsites.net';
  private loggedInSubject = new BehaviorSubject<boolean>(false);
  isLoggedIn$ = this.loggedInSubject.asObservable();

  constructor(private http: HttpClient) {}

  register(username: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/register`, { Username: username, Password: password });
  }

  login(username: string, password: string): Observable<any> {
    localStorage.setItem('username', username);
    this.loggedInSubject.next(true);
    localStorage.setItem('isLoggedIn', 'true');
    return this.http.post(`${this.apiUrl}/Auth/login`, { Username: username, Password: password });
  }

  logout() {
    localStorage.removeItem('username');
    localStorage.removeItem('isLoggedIn');
    this.loggedInSubject.next(false);
  }

  getUsername(): string {
    return localStorage.getItem('username') || '';
  }
}
