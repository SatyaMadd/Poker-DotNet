import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../auth.service'; // Adjust path if needed
import { CommonModule } from '@angular/common'; 
import { Router, NavigationEnd  } from '@angular/router'; // Import the Router

@Component({
  selector: 'main-header',
  templateUrl: './header.component.html',
  standalone: true,
  imports: [CommonModule],
  styleUrls: ['./header.component.css'] // Add styling
})
export class HeaderComponent implements OnInit {
  isLoggedIn: boolean = false;
  username: string = '';
  currentRoute: string = '';

  constructor(private authService: AuthService, private router: Router) {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.currentRoute = event.url;
      }
    });
   }

  ngOnInit(): void {
     // Check local storage on initialization
     this.checkLoginStatus();
     
    this.authService.isLoggedIn$.subscribe(loggedIn => {
      if (loggedIn) {
        this.username = this.authService.getUsername();
        this.isLoggedIn = loggedIn;
      }
    });
  }
  checkLoginStatus() {
    const isLoggedIn = localStorage.getItem('isLoggedIn'); 
    if (isLoggedIn === 'true') { 
      this.isLoggedIn = true;
      this.username = this.authService.getUsername(); 
    }
  }

  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.username = ""; 
    this.router.navigate(["/home"]);
  }
}
