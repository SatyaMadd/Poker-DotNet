import { Component, OnInit } from '@angular/core';
import { SignalRService } from '../signalr.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-join',
  templateUrl: './join.component.html',
  styleUrls: ['./join.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule]
})
export class JoinComponent implements OnInit {
  games: any[] = [];

  constructor(private signalRService: SignalRService) {}

  ngOnInit(): void {
    if (!this.isAuthenticated()) {
      if (typeof window !== 'undefined') {
        window.location.href = '/home';
      }
    } else {
      this.signalRService.startJoinConnection()
        .then(() => {
          this.signalRService.onGameListRefresh(() => this.refreshAvailableGames());
          return this.refreshAvailableGames();
        })
        .catch(err => {
          console.error('Error starting game connection or refreshing:', err);
        });
    }
  }

  isAuthenticated(): boolean {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem('jwtToken') !== null;
    }
    return false;
  }

  async refreshAvailableGames(): Promise<void> {
    try {
      this.games = await this.signalRService.getAvailableGames();
    } catch (error) {
      console.error('Error:', error);
    }
  }

  async createGame(gameName: string): Promise<void> {
    try {
      if (!gameName) {
        alert('Please enter a game name.');
        return;
      }
      const response = await this.signalRService.createGame(gameName);
      if (response) {
        if (typeof window !== 'undefined') {
          window.location.href = '/lobby';
        }
      }
    } catch (error) {
      console.error('Error:', error);
    }
  }

  async joinGame(gameId: number): Promise<void> {
    try {
      const location = await this.signalRService.joinGame(gameId);
      if (typeof window !== 'undefined') {
        if (location === 'Lobby') {
          window.location.href = '/lobby';
        } else if(location === 'WaitingRoom') {
          window.location.href = '/waitingRoom';
        }else{
          this.refreshAvailableGames();
        }
      }
    } catch (error) {
      console.error('Error:', error);
    }
  }
}
