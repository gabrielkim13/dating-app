import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { userInfo } from 'os';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Group } from '../_models/group';
import Message from '../_models/messages';
import { User } from '../_models/user';
import { BusyService } from './busy.service';
import { getPaginatedResult, getPaginationParams } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;

  private hubConnection: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);

  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient, private busyService: BusyService) { }

  createHubConnection(sender: User, recipientName: string) {
    this.busyService.busy();

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + `messages?user=${recipientName}`, {
        accessTokenFactory: () => sender.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .catch(error => console.error(error))
      .finally(() => this.busyService.idle());

    this.hubConnection.on("ReceiveMessageThread", (messageThread: Message[]) => {
      this.messageThreadSource.next(messageThread);
    });

    this.hubConnection.on("NewMessage", (message: Message) => {
      this.messageThread$.pipe(take(1)).subscribe(messageThread => {
        this.messageThreadSource.next([...messageThread, message]);
      });
    });

    this.hubConnection.on("UpdatedGroup", (group: Group) => {
      if (group.connections.some(connection => connection.username === recipientName)) {
        this.messageThread$.pipe(take(1)).subscribe(messageThread => {
          this.messageThreadSource.next([...messageThread.map(message => ({
            ...message,
            dateRead: !message.dateRead ? new Date(Date.now()) : message.dateRead
          }))])
        })
      }
    });
  }

  stopHubConnection() {
    if (this.hubConnection) {
      this.messageThreadSource.next([]);
      this.hubConnection.stop();
    }
  }

  getMessages(pageNumber: number, pageSize: number, container?: ("Inbox" | "Outbox" | "Unread")) {
    let params = getPaginationParams(pageNumber, pageSize);

    params = params.append("Container", container);

    return getPaginatedResult<Message[]>(this.baseUrl + "messages", params, this.http);
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + `messages/thread/${username}`);
  }

  async sendMessage(username: string, content: string) {
    return this.hubConnection.invoke("SendMessage", {
      recipientUsername: username, content
    }).catch(error => console.error(error));
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + `messages/${id}`);
  }
}
