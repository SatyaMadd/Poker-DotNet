import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5059'; 

  constructor(private http: HttpClient) {}

  register(username: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/register`, { Username: username, Password: password });
  }

  login(username: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/login`, { Username: username, Password: password });
  }
}
