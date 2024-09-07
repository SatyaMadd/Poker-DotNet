import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private joinConnection: signalR.HubConnection;
  private gameConnection: signalR.HubConnection;

  constructor() {
    const token = this.getToken();
    console.log('Token:', token); 

    this.joinConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5059/joinhub', {
        accessTokenFactory: () => {
          console.log('Access Token Factory called');
          return token ? token : '';
        }
      })
      .build();

    this.joinConnection.onclose(error => {
      console.error('Connection closed:', error); 
    });

    this.gameConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5059/gamehub', {
        accessTokenFactory: () => {
          console.log('Access Token Factory called');
          return token ? token : '';
        }
      })
      .build();
    this.gameConnection.onclose(error => {
      console.error('Connection closed:', error); 
    });
  }

  private getToken(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem('jwtToken');
    }
    return null;
  }

  public startJoinConnection(): Promise<void> {
    return this.joinConnection.start()
      .then(() => console.log('Connection started'))
      .catch(err => console.error('Error while starting connection:', err.toString()));
  }

  public startGameConnection(): Promise<void> {
    return this.gameConnection.start()
      .then(() => console.log('Connection started'))
      .catch(err => console.error('Error while starting connection:', err.toString()));
  }

  public onGameListRefresh(callback: () => void): void {
    this.joinConnection.on('GameListRefresh', callback);
  }

  public onLobbyRefresh(callback: () => void): void {
    this.joinConnection.on('LobbyRefresh', callback);
  }

  public onWaitingRoomRefresh(callback: () => void): void {
    this.joinConnection.on('WaitingRoomRefresh', callback);
  }

  public onRefresh(callback: () => void): void {
    this.gameConnection.on('Refresh', callback);
  }
  
  public async getAvailableGames(): Promise<any[]> {
    if (this.joinConnection.state === signalR.HubConnectionState.Connected) {
      try {
        return await this.joinConnection.invoke('GetAvailableGames');
      } catch (error) {
        console.error('Error getting available games:', error); 
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
      return [];
    }
  }

  public async createGame(gameName: string): Promise<boolean> {
    if (this.joinConnection.state === signalR.HubConnectionState.Connected) {
      try {
        return await this.joinConnection.invoke('CreateGame', gameName);
      } catch (error) {
        console.error('Error creating game:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
      return false;
    }
  }

  public async joinGame(gameId: number): Promise<string> {
    if (this.joinConnection.state === signalR.HubConnectionState.Connected) {
      try {
        return await this.joinConnection.invoke('JoinGame', gameId);
      } catch (error) {
        console.error('Error joining game:', error); 
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
      return '';
    }
  }

  public async getPlayers(): Promise<any[]> {
    if (this.joinConnection.state === signalR.HubConnectionState.Connected) {
      try {
        return await this.joinConnection.invoke('GetPlayers');
      } catch (error) {
        console.error('Error getting players:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
      return [];
    }
  }

  public async leaveGame(): Promise<void> {
    if (this.joinConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.joinConnection.invoke('Leave');
      } catch (error) {
        console.error('Error leaving game:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
  }

  public async startGame(): Promise<void> {
    if (this.joinConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.joinConnection.invoke('StartGame');
      } catch (error) {
        console.error('Error starting game:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
  }

  public async kickPlayer(username: string): Promise<void> {
    if (this.joinConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.joinConnection.invoke('Kick', username);
      } catch (error) {
        console.error('Error kicking player:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
  }

  public async getWaitingRoomPlayers(): Promise<any[]> {
    if (this.joinConnection.state === signalR.HubConnectionState.Connected) {
      try {
        return await this.joinConnection.invoke('GetWaitingRoomPlayers');
      } catch (error) {
        console.error('Error getting players:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
      return [];
    }
  }

  public async check(): Promise<void> {
    if (this.gameConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.gameConnection.invoke('Check');
      } catch (error) {
        console.error('Error checking:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
  }

  public async call(): Promise<void> {
    if (this.gameConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.gameConnection.invoke('Check');
      } catch (error) {
        console.error('Error calling:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
  }

  public async bet(amount: number): Promise<void> {
    if (this.gameConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.gameConnection.invoke('Bet', amount);
      } catch (error) {
        console.error('Error betting:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
  }

  public async fold(): Promise<void> {
    if (this.gameConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.gameConnection.invoke('Fold');
      } catch (error) {
        console.error('Error folding:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
  }

  public async leave(): Promise<void> {
    if (this.gameConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.gameConnection.invoke('Leave');
      } catch (error) {
        console.error('Error leaving game:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
    if (typeof window !== 'undefined') {
      window.location.href = '/join';
    }
  }

  public async showCards(): Promise<void> {
    if (this.gameConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.gameConnection.invoke('ShowCards');
      } catch (error) {
        console.error('Error showing cards:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
  }

  public async muckCards(): Promise<void> {
    if (this.gameConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.gameConnection.invoke('MuckCards');
      } catch (error) {
        console.error('Error mucking cards:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
  }

  public async gameKick(username: string): Promise<void> {
    if (this.gameConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.gameConnection.invoke('Kick', username);
      } catch (error) {
        console.error('Error kicking player in game:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
  }

  public async waitingRoomKick(username: string): Promise<void> {
    if (this.joinConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.joinConnection.invoke('Kick', username);
      } catch (error) {
        console.error('Error kicking player in waiting room:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
  }

  public async admit(username: string): Promise<void> {
    if (this.joinConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.joinConnection.invoke('Admit', username);
      } catch (error) {
        console.error('Error admitting player:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
    }
  }

  public async getGame(): Promise<any> {
    if (this.gameConnection.state === signalR.HubConnectionState.Connected) {
      try {
        return await this.gameConnection.invoke('Refresh');
      } catch (error) {
        console.error('Error getting players:', error);
        throw error;
      }
    } else {
      console.error('Connection is not in the connected state');
      return [];
    }
  }
  
}
