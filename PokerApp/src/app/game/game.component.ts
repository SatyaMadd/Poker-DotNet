import { Component, OnInit } from '@angular/core';
import { SignalRService } from '../signalr.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule]
})
export class GameComponent implements OnInit {
  players: any[] = [];
  commCards: any[] = [];
  playerCards: any[] = [];
  allPlayerCards: any[] = [];
  pot: number = 0;
  round: string = '';
  username: string = '';
  isAdmin: boolean = false;
  isPlayerTurn: boolean = false;
  betHasOccurred: boolean = false;
  showdown: boolean = false;

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
      this.signalRService.startGameConnection()
        .then(() => {
          this.signalRService.onRefresh(() => this.refresh());
          return this.refresh();
        })
        .catch(err => {
          console.error('Error starting game connection or refreshing:', err);
        });
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

  async refresh(): Promise<void> {
    try {
      const response = await this.signalRService.getGame();
      if (!response) {
        window.location.href = '/join';
      }
      const game = response.game;
      if (!game) {
        window.location.href = '/join';
      }
      this.players = response.players;
      this.commCards = game.commCards;
      this.pot = game.pot;
      this.round = game.round;
      this.showdown = game.showdown;
      this.betHasOccurred = game.betHasOccurred;
      this.isPlayerTurn = this.players.some(player => player.username === this.username && player.isTurn);
      this.isAdmin = this.players.some(player => player.username === this.username && player.isAdmin);
      this.playerCards = response.playerCards;
      this.allPlayerCards = response.allPlayerCards;
    } catch (error) {
      console.error('Error refreshing game:', error);
    }
  }

  async leaveGame(): Promise<void> {
    try {
      await this.signalRService.leave();
      window.location.href = '/join';
    } catch (error) {
      console.error('Error leaving game:', error);
    }
  }

  async check(): Promise<void> {
    try {
      await this.signalRService.check();
    } catch (error) {
      console.error('Error checking:', error);
    }
  }

  async call(): Promise<void> {
    try {
      await this.signalRService.call();
    } catch (error) {
      console.error('Error calling:', error);
    }
  }

  async bet(amount: string): Promise<void> {
    try {
      await this.signalRService.bet(parseInt(amount, 10));
    } catch (error) {
      console.error('Error betting:', error);
    }
  }

  async fold(): Promise<void> {
    try {
      await this.signalRService.fold();
    } catch (error) {
      console.error('Error folding:', error);
    }
  }

  async showCards(): Promise<void> {
    try {
      await this.signalRService.showCards();
    } catch (error) {
      console.error('Error showing cards:', error);
    }
  }

  async muckCards(): Promise<void> {
    try {
      await this.signalRService.muckCards();
    } catch (error) {
      console.error('Error mucking cards:', error);
    }
  }

  async gameKick(username: string): Promise<void> {
    try {
      await this.signalRService.kickPlayer(username);
    } catch (error) {
      console.error('Error kicking player:', error);
    }
  }
}