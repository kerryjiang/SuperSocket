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
        this.chatWebsocket = webSocket({
            url: 'ws://localhost:4040',
            serializer: msg => msg,
            deserializer: msg => msg
        });
        this.loginName = name;
        this.isConnected = true;
        this.chatWebsocket.asObservable().subscribe(
            msg => console.log('message received: ' + msg.data),
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