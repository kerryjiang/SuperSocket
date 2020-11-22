import { webSocket, WebSocketSubject } from 'rxjs/webSocket';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})

export class ConnectionService {

    isConnected: boolean = false;

    loginName: string;

    chatWebsocket: WebSocketSubject<any>;

    async connect(name: string) {        
        this.chatWebsocket = webSocket('ws://localhost:4040');
        this.loginName = name;
        this.isConnected = true;
        this.chatWebsocket.asObservable().subscribe(
            msg => console.log('message received: ' + msg),
            err => console.log(err), 
            () => console.log('closed')
        );
        this.chatWebsocket.next('CON ' + name);
    }

    send(message: string) {
        if (!this.isConnected)
            return;
        
        this.chatWebsocket.next(message);
    }

    disconnect() {
        this.chatWebsocket.complete();
    }
}