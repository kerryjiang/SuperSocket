import { Component } from '@angular/core';
import { ConnectionService } from '../services/connectionService';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;
  isConnected = false;

  constructor(private connectionService: ConnectionService)
  {

  }

  showDisconnect() : boolean {
    return this.connectionService.isConnected;
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  async disconnect() {
    this.connectionService.disconnect();
  }
}
