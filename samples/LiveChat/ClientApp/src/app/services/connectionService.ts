import { webSocket, WebSocketSubject } from 'rxjs/webSocket';
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})

export class ConnectionService {

    isConnected: boolean = false;

    loginName: string;

    chatWebsocket: WebSocketSubject<any>;

    msgSubject = new Subject<string>();

    async connect(name: string) {        
        this.chatWebsocket = webSocket({
            //url: 'ws://localhost:4040',
            url: 'wss://localhost:4041',
            deserializer: msg => msg,
            serializer: msg => msg
        });
        this.loginName = name;
        this.isConnected = true;
        this.chatWebsocket.asObservable().subscribe(
            msg => {
                this.msgSubject.next(msg.data);
            },
            err => console.log(err), 
            () => console.log('closed')
        );
        this.chatWebsocket.next('CON ' + name);
    }

    send(message: string) {
        if (!this.isConnected)
            return;
        
        this.chatWebsocket.next("MSG " + message);
    }

    disconnect() {
        this.chatWebsocket.complete();
    }
}