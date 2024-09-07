import { Component, OnInit } from '@angular/core';
import { SignalRService } from '../signalr.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-lobby',
  templateUrl: './lobby.component.html',
  styleUrls: ['./lobby.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule]
})
export class LobbyComponent implements OnInit {
  players: any[] = [];
  username: string = '';
  isAdmin: boolean = false;

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
        this.signalRService.startGameConnection();
        this.signalRService.onLobbyRefresh(() => this.refreshPlayerList());
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
      const players = await this.signalRService.getPlayers();
      this.players = players;
      const readyPlayers = players.some(player => player.isReady && player.username === this.username);
      if (readyPlayers) {
        if (typeof window !== 'undefined') {
          window.location.href = '/game';
        }
      }
      this.isAdmin = players.some(player => player.username === this.username && player.isAdmin);
      document.getElementById('startButton')!.style.display = this.isAdmin ? 'inline' : 'none';
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

  async startGame(): Promise<void> {
    try {
      await this.signalRService.startGame();
    } catch (error) {
      console.error('Error starting game:', error);
    }
  }

  async kickPlayer(username: string): Promise<void> {
    try {
      await this.signalRService.kickPlayer(username);
    } catch (error) {
      console.error('Error kicking player:', error);
    }
  }
}
