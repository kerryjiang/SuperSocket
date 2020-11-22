import { Component } from '@angular/core';
import { ConnectionService } from '../services/connectionService';

@Component({
  selector: 'app-home',
  templateUrl: './room.component.html',
  styleUrls: [ './room.component.css' ]
})
export class RoomComponent {
  constructor(private connectionService: ConnectionService) {

  }
}
