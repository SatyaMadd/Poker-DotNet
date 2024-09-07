import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { JoinComponent } from './join/join.component';
import { LobbyComponent } from './lobby/lobby.component';
import { WaitingRoomComponent } from './waiting-room/waiting-room.component';
import { GameComponent } from './game/game.component';


export const routes: Routes = [
    { path: 'home', component: HomeComponent },
    { path: 'join', component: JoinComponent },
    { path: 'lobby', component: LobbyComponent },
    { path: 'waitingRoom', component: WaitingRoomComponent },
    { path: 'game', component: GameComponent },
    { path: '', redirectTo: '/home', pathMatch: 'full' }
];
