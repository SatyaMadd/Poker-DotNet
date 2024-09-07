import { Component, OnInit } from '@angular/core';
import { SignalRService } from '../signalr.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-waiting-room',
  templateUrl: './waiting-room.component.html',
  styleUrls: ['./waiting-room.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule]
})
export class WaitingRoomComponent implements OnInit {
  players: any[] = [];
  username: string = '';

  constructor(private signalRService: SignalRService) {}

  ngOnInit(): void {
    const token = this.getToken();
    if (!this.isAuthenticated() || !token) {
      if (typeof window !== 'undefined') {
        window.location.href = '/home';
      }
    } else {
      this.username = this.parseJwt(token).sub;
      this.signalRService.startJoinConnection();
      this.signalRService.onWaitingRoomRefresh(() => this.refreshPlayerList());
      this.refreshPlayerList();
    }
  }

  private getToken(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem('jwtToken');
    }
    return null;
  }

  private parseJwt(token: string): any {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    return JSON.parse(jsonPayload);
  }

  isAuthenticated(): boolean {
    return this.getToken() !== null;
  }

  async refreshPlayerList(): Promise<void> {
    try {
      const players = await this.signalRService.getWaitingRoomPlayers();
      this.players = players;
    } catch (error) {
      console.error('Error fetching players:', error);
    }
  }

  async leaveGame(): Promise<void> {
    try {
      await this.signalRService.leaveGame();
      if (typeof window !== 'undefined') {
        window.location.href = '/join';
      }
    } catch (error) {
      console.error('Error leaving game:', error);
    }
  }
}
