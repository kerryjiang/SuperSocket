import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ConnectionService } from '../services/connectionService';

@Component({
  selector: 'app-home',
  templateUrl: './room.component.html',
  styleUrls: [ './room.component.css' ]
})
export class RoomComponent {   
  messageToSend: string = "";
  messages: string[] = [];

  constructor(private connectionService: ConnectionService, private router: Router) {

  }

  ngOnInit() {
    if (!this.connectionService.isConnected) {
      this.router.navigate([ "" ]);
      return;
    }
    this.connectionService.msgSubject.subscribe(msg => {
      this.messages = [...this.messages, msg];
    });
  }

  onSubmit() {
    if (this.messageToSend != null && this.messageToSend.length > 0) {
      this.connectionService.send(this.messageToSend);
      this.messageToSend = "";
    }
  }
}
